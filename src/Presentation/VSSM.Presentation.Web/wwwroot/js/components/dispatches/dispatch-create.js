$(function () {

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

    });

    $('#itemClassSelect').change(function () {

        let classId = $(this).val();
        let itemSelect = $('#itemSelect');

        itemSelect.empty().append('<option>در حال بارگذاری...</option>');

        if (!classId) {
            itemSelect.html('<option>انتخاب کنید</option>');
            return;
        }

        $.get('/GetItems/' + classId, function (res) {
            itemSelect.empty().append('<option>انتخاب کنید</option>');
            if (res.success) {
                res.result.forEach(i => {
                    itemSelect.append(
                        `<option value="${i.id}">${i.fullName}</option>`
                    );
                });
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
                    tbody.append(`
                        <tr data-numDosePerVial=${d.numDosePerVial} data-expiryDate=${d.expiryDate} data-item-detail-store-id=${d.itemDetailStoreId} data-batch="${d.batchNumber}" data-store-id="${d.storeId}">
                            <td class="text-center">
                                <input type="radio"
                                       name="SelectedItemDetailId"
                                       value="${d.itemId}"
                                       class="item-radio" />
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

        $('#itemExtraFields').show();
        $('#itemExtraFields input, #itemExtraFields textarea, #itemExtraFields select')
            .prop('disabled', false);

        $('#addItemBtn').prop('disabled', false);
    });

    let addedItemsIndex = 0;

    $('#addItemBtn').click(function () {

        let selectedRadio = $('.item-radio:checked');
        if (!selectedRadio.length) {
            alert('لطفاً یک کالا انتخاب کنید');
            return;
        }

        debugger
        let row = selectedRadio.closest('tr');
        let itemId = selectedRadio.val();
        let batchNumber = row.data('batch');
        let storeId = row.data('store-id');
        let expiryDate = row.data('expirydate');
        let itemDetailStoreId = row.data('item-detail-store-id');
        let dosePerVial = row.data('numdosepervial');

        if ($(`#addedItemsTable tbody tr[data-id="${itemId}"]`).length) {
            alert('این کالا قبلاً اضافه شده است');
            return;
        }

        let itemName = row.find('td:eq(1)').text();
        let expiry = row.find('td:eq(2)').text();

        let dose = parseInt($('input[name="DoseCount"]').val());
        let vvmStageId = $('#vvmStagesSelect').val();
        let vvmStageText = $('#vvmStagesSelect option:selected').text();


        let note = $('textarea[name="ExtraNote"]').val();

        let tbody = $('#addedItemsTable tbody');
        tbody.find('tr.empty-row').remove();

        if (!dose || dose <= 0) {
            alert('لطفاً مقدار دوز معتبر وارد کنید');
            return;
        }

        if (!dosePerVial || dosePerVial <= 0) {
            alert('لطفاً دوز بر ویال را انتخاب کنید');
            return;
        }

        if (dose % dosePerVial !== 0) {
            alert('مقدار وارد شده باید مضربی از دوز بر ویال باشد');
            return;
        }


        tbody.append(`
            <tr data-id="${itemId}"
                <td>${itemName}</td>
                <td>${expiry}</td>
            
                <td class="dose-cell dose-col" data-dose="${dose}">
                    <span class="dose-text">${dose || '-'}</span>
                </td>

                <td>${vvmStageText || '-'}</td>
                <td>${note || '-'}</td>
            
                <td class="text-center actions-col">
                    <button type="button"
                            class="btn btn-sm btn-warning edit-item">
                        ویرایش
                    </button>
                    <button type="button"
                            class="btn btn-sm btn-danger remove-item ms-1">
                        حذف
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


        addedItemsIndex++;

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
            .addClass('btn-success save-item')
            .text('دخیره');
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
            .addClass('btn-warning edit-item')
            .text('ویرایش');
    });




});

function formatDate(d) {
    if (!d) return '-';
    return new Date(d).toLocaleDateString('fa-IR');
}

