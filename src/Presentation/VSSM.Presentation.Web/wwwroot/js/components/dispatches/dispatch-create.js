let addedItemsIndex = 0;
let addedItems = [];

$(function () {

    const message = $('#ErrorMessage').val();
    if (message && message.trim() !== '') {
        showToast(message, 'error');
    }

    setSearchableSelectDisabled('itemSelect', true);

    $('#Date').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true
    });

    if ($('#Date').val() === '') {
        if (typeof persianDate !== 'undefined') {
            var today = new persianDate().format('YYYY/MM/DD');
            $('#Date').val(today);
        }
    }

    $('#dispatchForm').on('submit', function (e) {
        let hasItem = $('#addedItemsTable tbody tr').not('.empty-row').length > 0;
        if (!hasItem) {
            e.preventDefault();
            showToast('حداقل یک کالا باید به لیست اضافه شود', 'warning');
            return false;
        }

        let receiverId = $('#ReceiverId').val();
        if (!receiverId || receiverId === '') {
            e.preventDefault();
            showToast('لطفاً گیرنده را انتخاب کنید', 'warning');
            return false;
        }
    });

    // تغییر نوع کالا
    $('#itemClassSelect').on('change', function () {
        const hasValue = $(this).val();
        if (hasValue) {
            setSearchableSelectDisabled('itemSelect', false);
        } else {
            setSearchableSelectDisabled('itemSelect', true);
            setSearchableSelectOptions('itemSelect', []);
        }
    });

    $('#itemClassSelect').change(function () {
        let classId = $(this).val();
        if (!classId) {
            setSearchableSelectOptions('itemSelect', []);
            return;
        }
        $.get('/GetItems/' + classId, function (res) {
            if (res.success) {
                const items = res.result.map(i => ({
                    value: i.id,
                    text: i.fullName
                }));
                setSearchableSelectOptions('itemSelect', items);
            }
        });
    });

    // انتخاب کالا
    $('#itemSelect').change(function () {
        let itemId = $(this).val();
        let tbody = $('#itemDetailsTable tbody');
        tbody.empty();
        $('#itemExtraFields').hide();
        $('#itemExtraFields input, #itemExtraFields textarea, #itemExtraFields select')
            .prop('disabled', true);

        if (!itemId) {
            tbody.append('<tr><td colspan="6" class="text-center">کالایی انتخاب نشده</td></tr>');
            return;
        }

        $.get('/GetItemDetails/' + itemId, function (res) {
            if (res.success && res.result.length > 0) {
                res.result.forEach(d => {
                    let diluentAttr = d.diluent ? JSON.stringify(d.diluent).replace(/"/g, "&quot;") : '';
                    tbody.append(`
                        <tr data-numDosePerVial="${d.numDosePerVial}" 
                            data-expiryDate="${d.expiryDate}" 
                            data-item-detail-store-id="${d.itemDetailStoreId}" 
                            data-batch="${d.batchNumber}" 
                            data-store-id="${d.storeId}" 
                            data-diluent="${diluentAttr}">
                            <td class="text-center">
                                <input type="radio" name="SelectedItemDetailId" value="${d.itemId}" class="item-radio" />
                            </td>
                            <td>${d.itemName ?? '-'}</td>
                            <td>${formatDate(d.expiryDate)}</td>
                            <td>${d.vvmStep ?? '-'}</td>
                            <td>${d.currentStock ?? '-'}</td>
                            <td>${d.manufacture ?? '-'}</td>
                            <td>${d.storeName ?? '-'}</td>
                            <td>${d.batchNumber ?? '-'}</td>
                        </tr>
                    `);
                });
            } else {
                tbody.append('<tr><td colspan="8" class="text-center">اطلاعاتی یافت نشد</td></tr>');
            }
        });
    });

    // انتخاب رادیو
    $(document).on('change', '.item-radio', function () {
        $('.item-radio').closest('tr').removeClass('table-primary');
        $(this).closest('tr').addClass('table-primary');

        let row = $(this).closest('tr');
        let vvmStep = row.find('td:eq(3)').text().trim();

        $('#itemExtraFields').show();
        $('#itemExtraFields input, #itemExtraFields textarea').prop('disabled', false);

        if (!vvmStep || vvmStep === '-') {
            $('#vvmStagesSelect').closest('.col-md-6').hide();
            $('#vvmStagesSelect').prop('disabled', true);
        } else {
            $('#vvmStagesSelect').closest('.col-md-6').show();
            $('#vvmStagesSelect').prop('disabled', false);
            $('#vvmStagesSelect').val(row.data('vvmstageid') || '');
        }
    });

    // ════════════════════════════════════════════════════════════
    // ═══ تابع افزودن کالا (onclick در HTML) ═══
    // ════════════════════════════════════════════════════════════
    window.addSelectedItem = function () {

        let selectedRadio = $('.item-radio:checked');
        if (!selectedRadio.length) {
            showToast('لطفاً یک کالا انتخاب کنید', 'warning');
            return;
        }

        let row = selectedRadio.closest('tr');
        let itemId = selectedRadio.val();
        let batchNumber = row.data('batch');
        let storeId = row.data('store-id');
        let expiryDate = row.data('expirydate');
        let itemDetailStoreId = row.data('item-detail-store-id');
        let dosePerVial = row.data('numdosepervial');

        // ⭐ خواندن تعداد از فیلد ورودی (همان مقداری که در جدول نمایش داده می‌شود)
        let doseInput = $('#doseCountInput').val();
        let dose = parseInt(doseInput);
        if (isNaN(dose) || dose <= 0) {
            showToast('لطفاً تعداد معتبر وارد کنید', 'warning');
            return;
        }

        // بررسی موجودی (ستون ۴)
        let stockText = row.find('td:eq(4)').text().trim();
        let stock = parseInt(stockText) || 0;
        if (dose > stock) {
            showToast(`تعداد وارد شده (${dose}) بیشتر از موجودی (${stock}) است`, 'warning');
            return;
        }

        // جلوگیری از تکراری
        if ($(`#addedItemsTable tbody tr[data-id="${itemId}"]`).length) {
            showToast('این کالا قبلاً اضافه شده است', 'warning');
            return;
        }

        let itemName = row.find('td:eq(1)').text();
        let expiry = row.find('td:eq(2)').text();
        let vvmStageId = $('#vvmStagesSelect').val();
        let vvmStageText = $('#vvmStagesSelect option:selected').text();
        let note = $('textarea[name="ExtraNote"]').val();

        let tbody = $('#addedItemsTable tbody');
        tbody.find('tr.empty-row').remove();

        if (!dosePerVial || dosePerVial <= 0) {
            showToast('لطفاً دوز بر ویال را انتخاب کنید', 'warning');
            return;
        }

        const isDrafted = $('#IsDrafted').val() === 'true';

        let addUidButton = '';
        if (isDrafted) {
            addUidButton = `
                <button type="button"
                        class="item-table-action add-item-uid ms-1"
                        title="افزودن UID"
                        data-idx="${addedItemsIndex}">
                    <i class="fa fa-plus"></i>
                </button>
            `;
        }

        // ⭐ افزودن ردیف با data-requested-dose
        tbody.append(`
            <tr data-id="${itemId}" data-batch="${batchNumber}" data-requested-dose="${dose}">
                <td>${batchNumber}</td>
                <td>${itemName}</td>
                <td>${expiry}</td>
                <td class="dose-cell dose-col" data-dose="${dose}">
                    <span class="dose-text">${dose}</span>
                </td>
                <td>${vvmStageId ? vvmStageText : '-'}</td>
                <td>${note || '-'}</td>
                <td class="text-center actions-col">
                    ${addUidButton}
                    <button type="button" class="item-table-action edit-item" title="ویرایش">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button type="button" class="item-table-action remove-item ms-1" title="حذف">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
                <input type="hidden" name="Items[${addedItemsIndex}].ItemDetailStoreId" value="${itemDetailStoreId}" />
                <input type="hidden" name="Items[${addedItemsIndex}].ExpiryDate" value="${expiryDate}" />
                <input type="hidden" name="Items[${addedItemsIndex}].BatchNumber" value="${batchNumber}" />
                <input type="hidden" name="Items[${addedItemsIndex}].StoreId" value="${storeId}" />
                <input type="hidden" name="Items[${addedItemsIndex}].ItemId" value="${itemId}" />
                <input type="hidden" name="Items[${addedItemsIndex}].DoseCount" value="${dose}" />
                <input type="hidden" name="Items[${addedItemsIndex}].VvmStageId" value="${vvmStageId}" />
                <input type="hidden" name="Items[${addedItemsIndex}].Note" value="${note}" />
            </tr>
        `);

        // ⭐ ذخیره در addedItems (با requestedDose)
        addedItems.push({
            idx: addedItemsIndex,
            itemId: itemId,
            batchNumber: batchNumber,
            itemName: itemName,
            requestedDose: dose,
            stockInRam: stock,
            uids: [],
            itemDetailStoreId: itemDetailStoreId,
            dispatchLineId: 0,
            note: note,
            vvmStageId: vvmStageId
        });

        addedItemsIndex++;

        // افزودن حلال (در صورت وجود)
        let diluentData = row.data('diluent');
        if (diluentData) {
            let diluentDose = dose;
            tbody.append(`
                <tr data-id="diluent-${itemId}" data-batch="${diluentData.batchNumber}" data-requested-dose="${diluentDose}">
                    <td>${diluentData.batchNumber}</td>
                    <td>${diluentData.itemName}</td>
                    <td>${formatDate(diluentData.expiryDate)}</td>
                    <td class="dose-cell dose-col" data-dose="${diluentDose}">
                        <span class="dose-text">${diluentDose}</span>
                    </td>
                    <td>${diluentData.vvmStep || '-'}</td>
                    <td>${note || '-'}</td>
                    <td class="text-center actions-col">
                        <button type="button" class="item-table-action edit-item" title="ویرایش">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button type="button" class="item-table-action remove-item ms-1" title="حذف">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                    <input type="hidden" name="Items[${addedItemsIndex}].ItemDetailStoreId" value="${diluentData.itemDetailStoreId}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].ExpiryDate" value="${diluentData.expiryDate}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].BatchNumber" value="${diluentData.batchNumber}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].StoreId" value="${diluentData.storeId}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].ItemId" value="${diluentData.itemId}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].DoseCount" value="${diluentDose}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].VvmStageId" value="${diluentData.vvmStageId}" />
                    <input type="hidden" name="Items[${addedItemsIndex}].Note" value="${note}" />
                </tr>
            `);

            addedItems.push({
                idx: addedItemsIndex,
                itemId: diluentData.itemId,
                batchNumber: diluentData.batchNumber,
                itemName: diluentData.itemName,
                requestedDose: diluentDose,
                stockInRam: diluentData.currentStock || 0,
                uids: [],
                itemDetailStoreId: diluentData.itemDetailStoreId,
                dispatchLineId: 0,
                note: note,
                vvmStageId: diluentData.vvmStageId
            });
            addedItemsIndex++;
        }

        // reset
        $('#itemExtraFields input, #itemExtraFields textarea').val('');
        $('#itemExtraFields select').val('');
        $('.item-radio').prop('checked', false).closest('tr').removeClass('table-primary');
        $('#doseCountInput').val('');
    };

    // ── حذف ردیف ──
    $(document).on('click', '.remove-item', function () {
        let row = $(this).closest('tr');
        let itemId = row.data('id');
        row.remove();

        let batch = row.data('batch');
        let indexToRemove = -1;
        for (let i = 0; i < addedItems.length; i++) {
            if (addedItems[i].itemId == itemId && addedItems[i].batchNumber == batch) {
                indexToRemove = i;
                break;
            }
        }
        if (indexToRemove !== -1) {
            addedItems.splice(indexToRemove, 1);
        }

        if (!$('#addedItemsTable tbody tr').length) {
            $('#addedItemsTable tbody').html(`
                <tr class="empty-row">
                    <td colspan="12" class="text-center text-muted">
                        کالایی اضافه نشده
                    </td>
                </tr>
            `);
        }
        reindexItems();
    });

    // ── ویرایش دوز ──
    $(document).on('click', '.edit-item', function () {
        let row = $(this).closest('tr');
        let cell = row.find('.dose-cell');
        if (cell.find('input').length) return;

        let currentDose = cell.data('dose');
        let stock = parseInt(row.closest('table').find('tr').eq(row.index()).find('td:eq(4)').text().trim()) || 0;
        let itemId = row.data('id');
        let batch = row.data('batch');
        let totalOther = 0;
        $('#addedItemsTable tbody tr').not(row).each(function () {
            if ($(this).data('id') == itemId && $(this).data('batch') == batch) {
                let d = parseInt($(this).find('.dose-cell').data('dose')) || 0;
                totalOther += d;
            }
        });
        let maxAllowed = stock - totalOther;
        if (maxAllowed < 0) maxAllowed = 0;

        cell.html(`
            <input type="number"
                   class="form-control form-control-sm dose-input"
                   value="${currentDose}"
                   min="1"
                   max="${maxAllowed}" />
        `);
        $(this)
            .removeClass('edit-item')
            .addClass('save-item')
            .html('<i class="fa fa-check"></i>');
    });

    // ── ذخیره ویرایش دوز ──
    $(document).on('click', '.save-item', function () {
        let row = $(this).closest('tr');
        let cell = row.find('.dose-cell');
        let newDose = parseInt(cell.find('.dose-input').val());
        let dosePerVial = parseInt(row.data('numdosepervial'));

        if (!newDose || newDose <= 0) {
            showToast('دوز معتبر وارد کنید', 'warning');
            return;
        }

        if (dosePerVial && newDose % dosePerVial !== 0) {
            showToast('مقدار وارد شده باید مضربی از دوز بر ویال باشد', 'warning');
            return;
        }

        cell.data('dose', newDose).html(`<span class="dose-text">${newDose}</span>`);
        row.find('input[type="hidden"][name$=".DoseCount"]').val(newDose);

        // به‌روزرسانی data-requested-dose
        row.data('requested-dose', newDose);

        let itemId = row.data('id');
        let batch = row.data('batch');
        for (let i = 0; i < addedItems.length; i++) {
            if (addedItems[i].itemId == itemId && addedItems[i].batchNumber == batch) {
                addedItems[i].requestedDose = newDose;
                break;
            }
        }

        $(this)
            .removeClass('save-item')
            .addClass('edit-item')
            .html('<i class="fa fa-edit"></i>');
    });

});

// ─── توابع کمکی ───
function formatDate(d) {
    if (!d) return '-';
    return new Date(d).toLocaleDateString('fa-IR');
}

function reindexItems() {
    let rows = $('#addedItemsTable tbody tr').not('.empty-row');
    rows.each(function (index) {
        $(this).find('input[type="hidden"]').each(function () {
            let name = $(this).attr('name');
            if (!name) return;
            let newName = name.replace(/Items\[\d+\]/, `Items[${index}]`);
            $(this).attr('name', newName);
        });
        $(this).find('.add-item-uid').attr('data-idx', index);
    });
    addedItemsIndex = rows.length;

    let newAddedItems = [];
    rows.each(function (index) {
        let itemId = $(this).data('id');
        let batch = $(this).data('batch');
        // ⭐ مقدار requestedDose را از data-requested-dose ردیف بگیریم
        let requestedDose = parseInt($(this).data('requested-dose')) || 0;
        for (let i = 0; i < addedItems.length; i++) {
            if (addedItems[i].itemId == itemId && addedItems[i].batchNumber == batch) {
                let item = addedItems[i];
                item.idx = index;
                item.requestedDose = requestedDose; // بازنویسی با مقدار دقیق از ردیف
                newAddedItems.push(item);
                break;
            }
        }
    });
    addedItems = newAddedItems;
}