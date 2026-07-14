(function () {
    'use strict';

    const tableBody = document.querySelector("#itemsTbody");
    const fromStore = document.getElementById("FromStoreId");
    const toStore = document.getElementById("ToStoreId");
    const form = document.getElementById("transferForm");
    const hiddenContainer = document.getElementById("hiddenInputsContainer");
    const selectAllCheckbox = document.getElementById("selectAll");
    const btnTransfer = document.getElementById("btnTransfer");

    // ── به‌روزرسانی وضعیت دکمه ──
    function updateTransferButtonState() {
        const checked = tableBody.querySelectorAll(".row-checkbox:checked");
        if (checked.length > 0) {
            btnTransfer.disabled = false;
            btnTransfer.classList.remove('disabled');
        } else {
            btnTransfer.disabled = true;
            btnTransfer.classList.add('disabled');
        }
    }

    // ── بارگذاری ظرفیت انبار ──
    function loadStoreCapacity(storeId, capacityId, freeCapacityId) {
        const capacityField = document.getElementById(capacityId);
        const freeCapacityField = document.getElementById(freeCapacityId);

        if (!storeId) {
            capacityField.value = "";
            freeCapacityField.value = "";
            return;
        }

        fetch(`/StoreTransfer/GetStoreCapacity?storeId=${storeId}`)
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(data => {
                capacityField.value = data.capacity;
                freeCapacityField.value = data.freeCapacity;
            })
            .catch(() => {
                capacityField.value = "";
                freeCapacityField.value = "";
            });
    }

    // ── نمایش حالت خالی ──
    function showEmptyState(message) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="4" class="empty-table-row">
                    <i class="fa fa-inbox"></i>
                    ${message}
                </td>
            </tr>
        `;
        updateTransferButtonState();
    }

    // ── نمایش اسپینر بارگذاری ──
    function showLoadingState() {
        tableBody.innerHTML = `
            <tr>
                <td colspan="4" class="empty-table-row">
                    <i class="fa fa-spinner fa-spin" style="font-size: 24px; color: #1d6fb8;"></i>
                    <span style="display: block; margin-top: 8px; color: #6b7280;">در حال بارگذاری...</span>
                </td>
            </tr>
        `;
        updateTransferButtonState();
    }

    // ── ساخت یک ردیف جدول ──
    function createItemRow(item, index) {
        return `
            <tr>
                <td>
                    <input type="checkbox" class="row-checkbox" data-index="${index}" />
                    <input type="hidden" class="item-detail-store-id" value="${item.itemDetailStoreId}" />
                </td>
                <td>${item.itemName}</td>
                <td class="current-stock">${item.currentStock}</td>
                <td>
                    <input type="number" class="f-control quantity-input" 
                           min="1" max="${item.currentStock}" disabled />
                </td>
            </tr>
        `;
    }

    // ── ساخت جدول با پشتیبانی از اسکرول ──
    function buildTableWithScroll(items) {
        tableBody.innerHTML = '';

        if (!items || items.length === 0) {
            showEmptyState('کالایی در این انبار موجود نیست');
            return;
        }

        const visibleCount = 5;
        const hasMany = items.length > visibleCount;

        const firstItems = items.slice(0, visibleCount);
        firstItems.forEach((item, index) => {
            tableBody.insertAdjacentHTML('beforeend', createItemRow(item, index));
        });

        if (hasMany) {
            const extraItems = items.slice(visibleCount);
            extraItems.forEach((item, index) => {
                const row = createItemRow(item, visibleCount + index);
                tableBody.insertAdjacentHTML('beforeend', row);
                const lastRow = tableBody.lastElementChild;
                lastRow.classList.add('hidden-line');
                lastRow.style.display = 'none';
            });

            const toggleRow = document.createElement('tr');
            toggleRow.className = 'more-lines-trigger';
            toggleRow.innerHTML = `
                <td colspan="4">
                    <button type="button" class="btn-show-all-lines">
                        <i class="fa fa-chevron-down"></i>
                        <span class="btn-text">مشاهده همه اقلام (${items.length} قلم)</span>
                    </button>
                </td>
            `;
            tableBody.appendChild(toggleRow);
        }

        attachCheckboxEvents();
        updateTransferButtonState();

        if (window.VSSM && window.VSSM.ScrollableTable) {
            window.VSSM.ScrollableTable.refresh();
        }

        const wrap = document.querySelector('.lines-table-wrap');
        if (wrap) {
            if (hasMany) {
                wrap.classList.add('has-scroll');
            } else {
                wrap.classList.remove('has-scroll');
            }
        }
    }

    // ── رویدادهای چک‌باکس ──
    function attachCheckboxEvents() {
        const checkboxes = tableBody.querySelectorAll(".row-checkbox");
        checkboxes.forEach(cb => {
            cb.removeEventListener("change", handleCheckboxChange);
            cb.addEventListener("change", handleCheckboxChange);
        });
    }

    function handleCheckboxChange(e) {
        const cb = e.target;
        const row = cb.closest("tr");
        const stock = parseInt(row.querySelector(".current-stock").textContent);
        const qtyInput = row.querySelector(".quantity-input");

        if (cb.checked) {
            qtyInput.value = stock;
            qtyInput.disabled = false;
        } else {
            qtyInput.value = "";
            qtyInput.disabled = true;
        }

        // به‌روزرسانی وضعیت دکمه
        updateTransferButtonState();
    }

    // ── بارگذاری کالاها ──
    function loadItemsByStore(storeId) {
        if (!storeId) {
            showEmptyState('ابتدا انبار مبدا را انتخاب کنید');
            return;
        }

        showLoadingState();

        fetch(`/StoreTransfer/GetItemsByStore?storeId=${storeId}`)
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(items => {
                buildTableWithScroll(items);
            })
            .catch(() => {
                tableBody.innerHTML = `
                    <tr>
                        <td colspan="4" class="empty-table-row">
                            <i class="fa fa-exclamation-triangle" style="color:#f59e0b;"></i>
                            خطا در دریافت اطلاعات
                        </td>
                    </tr>`;
                updateTransferButtonState();
            });
    }

    // ── اعتبارسنجی و ارسال ──
    function validateAndSubmit() {
        const checkedRows = tableBody.querySelectorAll(".row-checkbox:checked");

        if (checkedRows.length === 0) {
            alert("لطفاً حداقل یک کالا را انتخاب کنید.");
            return;
        }

        if (!toStore.value || toStore.value.trim() === "") {
            alert("انتخاب انبار مقصد اجباری است.");
            toStore.focus();
            return;
        }

        if (fromStore.value === toStore.value) {
            alert("انبار مبدا و مقصد نمی‌توانند یکسان باشند.");
            return;
        }

        for (let cb of checkedRows) {
            const row = cb.closest("tr");
            const qtyInput = row.querySelector(".quantity-input");
            const stockCell = row.querySelector(".current-stock");
            const stock = parseInt(stockCell.textContent.trim());
            const qty = parseInt(qtyInput.value);

            if (!qty || qty <= 0) {
                alert("مقدار انتقال برای همه کالاهای انتخاب شده باید بزرگتر از صفر باشد.");
                qtyInput.focus();
                return;
            }

            if (qty > stock) {
                alert("مقدار انتقال نمی‌تواند بیشتر از موجودی کالا باشد.");
                qtyInput.focus();
                return;
            }
        }

        hiddenContainer.innerHTML = "";
        checkedRows.forEach((cb, idx) => {
            const row = cb.closest("tr");
            const itemDetailStoreId = row.querySelector(".item-detail-store-id").value;
            const qtyInput = row.querySelector(".quantity-input");
            const quantity = parseInt(qtyInput.value);

            const fields = [
                { name: `Items[${idx}].ItemDetailStoreId`, value: itemDetailStoreId },
                { name: `Items[${idx}].Quantity`, value: quantity },
                { name: `Items[${idx}].IsSelected`, value: "true" }
            ];

            fields.forEach(f => {
                const input = document.createElement("input");
                input.type = "hidden";
                input.name = f.name;
                input.value = f.value;
                hiddenContainer.appendChild(input);
            });
        });

        form.submit();
    }

    // ── رویدادها ──
    function handleSelectAll(e) {
        const checked = e.target.checked;
        const checkboxes = tableBody.querySelectorAll(".row-checkbox");
        checkboxes.forEach(cb => {
            cb.checked = checked;
            const row = cb.closest("tr");
            const stock = parseInt(row.querySelector(".current-stock").textContent);
            const qtyInput = row.querySelector(".quantity-input");

            if (checked) {
                qtyInput.value = stock;
                qtyInput.disabled = false;
            } else {
                qtyInput.value = "";
                qtyInput.disabled = true;
            }
        });

        updateTransferButtonState();
    }

    function onFromStoreChange(e) {
        const val = e.target.value;
        loadStoreCapacity(val, "FromStoreCapacity", "FromStoreFreeCapacity");
        loadItemsByStore(val);
    }

    function onToStoreChange(e) {
        const val = e.target.value;
        loadStoreCapacity(val, "ToStoreCapacity", "ToStoreFreeCapacity");
    }

    // ── مقداردهی اولیه ──
    function init() {
        // دکمه انتقال ابتدا غیرفعال است
        btnTransfer.disabled = true;
        btnTransfer.classList.add('disabled');

        if (fromStore) {
            fromStore.addEventListener("change", onFromStoreChange);
        }
        if (toStore) {
            toStore.addEventListener("change", onToStoreChange);
        }
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener("change", handleSelectAll);
        }
        if (btnTransfer) {
            btnTransfer.addEventListener("click", validateAndSubmit);
        }

        if (fromStore && fromStore.value) {
            loadStoreCapacity(fromStore.value, "FromStoreCapacity", "FromStoreFreeCapacity");
            loadItemsByStore(fromStore.value);
        } else {
            showEmptyState('ابتدا انبار مبدا را انتخاب کنید');
        }

        if (toStore && toStore.value) {
            loadStoreCapacity(toStore.value, "ToStoreCapacity", "ToStoreFreeCapacity");
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();