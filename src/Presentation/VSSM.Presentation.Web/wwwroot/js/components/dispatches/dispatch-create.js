let addedItemsIndex = 0;

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

    $('#createForm').on('submit', function (e) {

        let hasItem =
            $('#addedItemsTable tbody tr')
                .not('.empty-row')
                .length > 0;

        if (!hasItem) {
            e.preventDefault();
            alert('حداقل یک کالا باید به لیست اضافه شود');
            return false;
        }

        let receiverId = $('#ReceiverId').val();
        if (!receiverId || receiverId === '') {
            e.preventDefault();
            alert('لطفاً گیرنده را انتخاب کنید');
            return false;
        }

    });

    $('#itemClassSelect').on('change', function () {
        const hasValue = $(this).val();

        if (hasValue) {
            setSearchableSelectDisabled('itemSelect', false);
        } else {
            setSearchableSelectDisabled('itemSelect', true);

            // optional: also clear items
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
                        <tr data-numDosePerVial=${d.numDosePerVial} 
                            data-expiryDate=${d.expiryDate} 
                            data-item-detail-store-id=${d.itemDetailStoreId} 
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

        $('#addItemBtn').prop('disabled', false);
    });

    addedItemsIndex =
        $('#addedItemsTable tbody tr')
            .not('.empty-row')
            .length;

    $('#addItemBtn').click(function () {

        let selectedRadio = $('.item-radio:checked');
        if (!selectedRadio.length) {
            alert('لطفاً یک کالا انتخاب کنید');
            return;
        }

        let row = selectedRadio.closest('tr');
        let itemId = selectedRadio.val();
        let batchNumber = row.data('batch');
        let storeId = row.data('store-id');
        let expiryDate = row.data('expirydate');
        let itemDetailStoreId = row.data('item-detail-store-id');
        let dosePerVial = row.data('numdosepervial');

        // prevent duplicate main item
        if ($(`#addedItemsTable tbody tr[data-id="${itemId}"]`).length) {
            alert('این کالا قبلاً اضافه شده است');
            return;
        }

        let itemName = row.find('td:eq(1)').text();
        let expiry = row.find('td:eq(2)').text();
        //let dose = parseInt($('input[name="DoseCount"]').val());
        let dose = 0;
        let vvmStageId = $('#vvmStagesSelect').val();
        let vvmStageText = $('#vvmStagesSelect option:selected').text();
        let note = $('textarea[name="ExtraNote"]').val();

        let tbody = $('#addedItemsTable tbody');
        tbody.find('tr.empty-row').remove();

        //// validate dose
        //if (!dose || dose <= 0) {
        //    alert('لطفاً مقدار دوز معتبر وارد کنید');
        //    return;
        //}


        if (!dosePerVial || dosePerVial <= 0) {
            alert('لطفاً دوز بر ویال را انتخاب کنید');
            return;
        }
        //if (dose % dosePerVial !== 0) {
        //    alert('مقدار وارد شده باید مضربی از دوز بر ویال باشد');
        //    return;
        //}
        const isDrafted = $('#IsDrafted').val() === 'true';

        let addUidButton = '';

        if (isDrafted) {
            addUidButton = `
        <button type="button"
                class="item-table-action add-item-uid ms-1"
                title="افزودن">
            <i class="fa fa-plus"></i>
        </button>
    `;
        }

        // Add main item row
        tbody.append(`
        <tr data-id="${itemId}">
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

        // increment index for main item
        addedItemsIndex++;

        let diluentData = row.data('diluent'); // pass this from server when generating item table
        if (diluentData) {

            //let diluentDose = parseInt($('input[name="DoseCount"]').val()); // same dose or custom
            let diluentDose = 0; // same dose or custom
            tbody.append(`
            <tr data-id="diluent-${itemId}">
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

            addedItemsIndex++;
        }

        // reset form fields
        $('#itemExtraFields input, #itemExtraFields textarea').val('');
        $('#itemExtraFields select').val('');
        $('.item-radio').prop('checked', false).closest('tr').removeClass('table-primary');
        $('#addItemBtn').prop('disabled', true);

    });

    $(document).on('click', '.remove-item', function () {
        $(this).closest('tr').remove();

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

    $(document).on('click', '.edit-item', function () {

        let row = $(this).closest('tr');
        let cell = row.find('.dose-cell');

        // prevent double edit
        if (cell.find('input').length) return;

        let currentDose = cell.data('dose');

        cell.html(`
        <input type="number"
               class="form-control form-control-sm dose-input"
               value="${currentDose}" />
    `);
        $(this)
            .removeClass('btn-warning edit-item')
            .addClass('save-item')
            .html('<i class="fa fa-check"></i>');
    });


    $(document).on('click', '.save-item', function () {


        let row = $(this).closest('tr');
        let cell = row.find('.dose-cell');

        let newDose = parseInt(cell.find('.dose-input').val());
        let dosePerVial = parseInt(row.data('dose-per-vial'));

        if (!newDose || newDose <= 0) {
            alert('دوز معتبر وارد کنید');
            return;
        }

        if (dosePerVial && newDose % dosePerVial !== 0) {
            alert('مقدار وارد شده باید مضربی از دوز بر ویال باشد');
            return;
        }

        cell
            .data('dose', newDose)
            .html(`<span class="dose-text">${newDose}</span>`);

        row.find('input[type="hidden"][name$=".DoseCount"]')
            .val(newDose);

        $(this)
            .removeClass('btn-success save-item')
            .addClass('edit-item')
            .html('<i class="fa fa-edit"></i>');
    });




});

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

            // Replace index in name attribute
            let newName = name.replace(/Items\[\d+\]/, `Items[${index}]`);
            $(this).attr('name', newName);
        });
    });

    addedItemsIndex = rows.length;
}