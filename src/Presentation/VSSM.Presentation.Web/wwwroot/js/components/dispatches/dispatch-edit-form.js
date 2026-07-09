/**
 * dispatch-edit-form.js  —  لاجیک اختصاصی فرم ویرایش ارسال
 * ═══════════════════════════════════════════
 * وابستگی‌ها (باید قبل از این فایل لود شده باشند):
 *   - jQuery
 *   - toast.js          (showToast)
 *   - uid-modal.js      (openUidModal, closeUidModal, submitUid, removeUid)
 *   - batch-table.js    (initBatchTable, loadBatchRows, showMoreRows)
 *
 * متغیرهای تزریقی از View:
 *   dispatchServerMsg
 *   dispatchServerType
 *   dispatchGetItemsByClass
 *   dispatchGetBatchesByItem
 *   setUidUrl
 *   dispatchEditGetLinesUrl   ← URL اکشن GetDispatchLines برای pre-populate
 *   dispatchId                ← Id ارسال در حال ویرایش
 *   dispatchIsConfirmed       ← bool: آیا تایید شده؟ (در صورت true عملیات قفل)
 * ═══════════════════════════════════════════
 */

// Holds every item row currently shown in the "added items" table.
// Kept in outer scope so uid-modal.js can read/update it too.
var addedItems = [];

$(function () {

    // Show a leftover toast message from the previous server round-trip (e.g. after redirect).
    if (typeof dispatchServerMsg !== 'undefined' && dispatchServerMsg) {
        showToast(dispatchServerMsg, dispatchServerType || 'info');
    }

    // Init the Persian date picker on the Date field, if the plugin is loaded.
    var $dateInput = $('#Date');
    if (typeof DatePicker !== 'undefined' && DatePicker.attach) {
        DatePicker.attach($dateInput);
        if ($dateInput.val()) $dateInput.addClass('has-value');
        $dateInput.on('change', function () {
            $(this).toggleClass('has-value', !!$(this).val());
        });
    }
    // Init the paginated batch-selection table (shared component).
    initBatchTable({
        tbodyId: 'itemDetailsTbody',
        showMoreWrap: 'showMoreWrap',
        pageSize: 5,
        emptyText: 'کالایی انتخاب نشده',
        colCount: 8,
    });


    // Fetch existing dispatch lines for this dispatch and render them into the table.
    if (dispatchId) {
        $.ajax({
            url: dispatchEditGetLinesUrl,
            data: { id: dispatchId },
            success: function (lines) {
                if (!lines || !lines.length) return;

                // Map each server line into the in-memory item shape used by
                // renderAddedItems() / rebuildHiddenInputs().
                $.each(lines, function (_, line) {
                    addedItems.push({
                        idx: addedItems.length,
                        dispatchLineId: line.dispatchLineId,
                        itemDetailStoreId: line.itemDetailStoreId,
                        itemId: line.itemId,
                        itemName: line.itemName,
                        batchNumber: line.batchNumber,
                        expiryDateShamsi: line.expiryDateShamsi,
                        expiryDate: line.expiryDate,
                        storeId: line.storeId,
                        storeName: line.storeName,
                        manufacturerName: line.manufacturerName,
                        doseCount: line.doseCount,
                        numVial: line.numVial || 0,
                        vvmStageId: line.vvmStageId || '',
                        vvmStageName: line.vvmStageName || '-',
                        note: line.note || '',
                        uids: line.uids || [],
                        stockInRam: line.currentStock || 0,
                        requiresDiluent: false,
                    });
                });

                renderAddedItems();
            },
            error: function () {
                showToast('خطا در بارگذاری اقلام ارسال', 'error');
            }
        });
    }


    // Item class changed -> reload the item dropdown, reset the batch table.
    $('#itemClassSelect').on('change', function () {
        var classId = $(this).val();
        $('#itemSelect').html('<option value="">در حال بارگذاری...</option>');
        loadBatchRows([]);

        if (!classId) {
            $('#itemSelect').html('<option value="">ابتدا نوع کالا را انتخاب کنید</option>');
            return;
        }

        $.ajax({
            url: dispatchGetItemsByClass,
            data: { classId: classId },
            success: function (items) {
                var opts = '<option value="">انتخاب کنید...</option>';
                $.each(items, function (_, item) {
                    opts += '<option value="' + item.itemId + '">' + item.fullName + '</option>';
                });
                $('#itemSelect').html(opts);
            }
        });
    });

    // Item changed -> reload the batch/lot table for that item.
    $('#itemSelect').on('change', function () {
        var itemId = $(this).val();
        loadBatchRows([]);
        if (!itemId) return;

        $.ajax({
            url: dispatchGetBatchesByItem,
            data: { itemId: itemId },
            success: function (rows) { loadBatchRows(rows); }
        });
    });


    // Add the currently-checked batch row (plus its auto diluent, if any) to addedItems.
    window.addSelectedItem = function () {
        // Locked once the dispatch is confirmed.
        if (dispatchIsConfirmed) {
            showToast('ارسال تایید شده — امکان ویرایش وجود ندارد', 'error');
            return;
        }

        var $checked = $('.batch-chk:checked');
        if ($checked.length === 0) {
            showToast('لطفاً یک سری ساخت را انتخاب کنید.', 'error');
            return;
        }

        var doseCount = parseInt($('#doseCountInput').val()) || 0;
        if (doseCount <= 0) {
            showToast('برای کلیه کالاهای ارسالی تعداد را مشخص نمایید', 'error');
            return;
        }

        // Row data is stashed as a JSON string in data-row (HTML-entity-encoded quotes).
        var row = JSON.parse($checked.first().attr('data-row').replace(/&#39;/g, "'"));
        var vvmStageId = $('#vvmStagesSelect').val();
        var vvmStageName = $('#vvmStagesSelect option:selected').text();

        addedItems.push({
            idx: addedItems.length,
            dispatchLineId: 0,   // جدید — هنوز در DB نیست
            itemDetailStoreId: row.itemDetailStoreId,
            itemId: row.itemId,
            itemName: row.itemName,
            batchNumber: row.batchNumber,
            expiryDateShamsi: row.expiryDateShamsi,
            expiryDate: row.expiryDate,
            storeId: row.storeId,
            storeName: row.storeName,
            manufacturerName: row.manufacturerName,
            doseCount: doseCount,
            numVial: row.dosePerVialNum > 0 ? Math.floor(doseCount / row.dosePerVialNum) : 0,
            vvmStageId: vvmStageId,
            vvmStageName: vvmStageName,
            note: '',
            uids: [],
            stockInRam: row.currentStock - doseCount,
            requiresDiluent: row.requiresDiluent,
            diluentItemDetailStoreId: row.diluentItemDetailStoreId,
            diluentItemId: row.diluentItemId,
            diluentItemName: row.diluentItemName,
        });

        renderAddedItems();

        // حلال خودکار
        // If the item requires a diluent, auto-add a second line for it with the same dose count.
        if (row.requiresDiluent && row.diluentItemDetailStoreId) {
            addedItems.push({
                idx: addedItems.length,
                dispatchLineId: 0,
                itemDetailStoreId: row.diluentItemDetailStoreId,
                itemId: row.diluentItemId,
                itemName: (row.diluentItemName || '') + ' (حلال - خودکار)',
                batchNumber: row.diluentBatchNumber || '',
                expiryDateShamsi: row.diluentExpiryDateShamsi || '-',
                expiryDate: row.diluentExpiryDate || null,
                storeId: row.storeId,
                storeName: row.storeName,
                manufacturerName: row.diluentManufacturerName || '-',
                doseCount: doseCount,
                numVial: 0,
                vvmStageId: '',
                vvmStageName: '-',
                note: 'حلال خودکار',
                uids: [],
                stockInRam: 0,
                requiresDiluent: false,
            });
            renderAddedItems();
        }

        $checked.prop('checked', false);
        $('#doseCountInput').val('');
    };


    // Remove a line from addedItems and give its dose count back to the on-screen stock display.
    window.removeItem = function (idx) {
        if (dispatchIsConfirmed) {
            showToast('ارسال تایید شده — امکان ویرایش وجود ندارد', 'error');
            return;
        }

        var item = addedItems[idx];
        var $stockCell = $('.stock-cell-' + item.itemDetailStoreId);
        if ($stockCell.length) {
            $stockCell.text((parseInt($stockCell.text()) || 0) + item.doseCount);
        }

        addedItems.splice(idx, 1);
        // Re-index remaining items so idx always matches array position.
        $.each(addedItems, function (i, it) { it.idx = i; });
        renderAddedItems();
    };


    // Re-draw the "added items" table body from the addedItems array, then rebuild hidden inputs.
    function renderAddedItems() {
        var tbody = $('#addedItemsTbody');

        if (addedItems.length === 0) {
            tbody.html(
                '<tr><td colspan="10" style="padding:24px;color:#9ca3af;text-align:center;">' +
                'کالایی اضافه نشده</td></tr>'
            );
            rebuildHiddenInputs();
            return;
        }

        var html = '';
        $.each(addedItems, function (i, item) {
            // No action buttons once confirmed — everything is read-only.
            var actions = dispatchIsConfirmed
                ? '<span style="color:#9ca3af;font-size:11px;">تایید شده</span>'
                : '<button type="button" class="btn-action btn-action-uid" title="افزودن UID" ' +
                'onclick="openUidModal(' + i + ')">' +
                '<i class="fa fa-qrcode"></i></button> ' +
                '<button type="button" class="btn-action btn-action-del" title="حذف" ' +
                'onclick="removeItem(' + i + ')">' +
                '<i class="fa fa-trash"></i></button>';

            html +=
                '<tr>' +
                '<td>' + (item.batchNumber || '-') + '</td>' +
                '<td>' + (item.itemName || '-') + '</td>' +
                '<td>' + (item.expiryDateShamsi || '-') + '</td>' +
                '<td>' + item.doseCount + '</td>' +
                '<td>' + item.numVial + '</td>' +
                '<td>-</td>' +
                '<td>' + (item.vvmStageName || '-') + '</td>' +
                '<td>' + (item.manufacturerName || '-') + '</td>' +
                '<td>' + (item.note || '-') + '</td>' +
                '<td style="white-space:nowrap;">' + actions + '</td>' +
                '</tr>';
        });

        tbody.html(html);
        rebuildHiddenInputs();
    }


    // Build the Items[i].* hidden inputs (model-binding format) posted with the form.
    function rebuildHiddenInputs() {
        var $container = $('#hiddenInputsContainer').empty();

        $.each(addedItems, function (i, item) {
            function add(name, val) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].' + name +
                    '" value="' + (val || '') + '">'
                );
            }
            add('DispatchLineId', item.dispatchLineId);
            add('ItemDetailStoreId', item.itemDetailStoreId);
            add('ItemId', item.itemId);
            add('StoreId', item.storeId);
            add('DoseCount', item.doseCount);
            add('VvmStageId', item.vvmStageId);
            add('BatchNumber', item.batchNumber);
            add('ExpiryDate', item.expiryDate);
            add('Note', item.note);

            // Uids[] needs its own indexed inputs, one per scanned UID.
            $.each(item.uids, function (j, uid) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].Uids[' + j + ']" value="' + uid + '">'
                );
            });
        });
    }


    // Final submit / draft submit buttons — validate, tag submitType, then post the form.
    $('.js-footer-btn').on('click', function () {
        var submitType = $(this).data('submit');

        if (addedItems.length === 0) {
            showToast('حداقل یک کالا را اضافه کنید.', 'error');
            return;
        }

        var hasZero = false;
        $.each(addedItems, function (_, it) {
            if (!it.doseCount || it.doseCount <= 0) { hasZero = true; return false; }
        });
        if (hasZero) {
            showToast('برای کلیه کالاهای ارسالی تعداد را مشخص نمایید', 'error');
            return;
        }

        $('#toastContainer').empty();
        // Replace any previous submitType hidden field with the one for this click.
        $('#dispatchForm').find('input[name="submitType"]').remove();
        $('<input>').attr({ type: 'hidden', name: 'submitType', value: submitType })
            .appendTo('#dispatchForm');
        $('#dispatchForm').submit();
    });

    // Delete button — confirm, then submit the form with submitType=delete.
    window.confirmDelete = function () {
        if (confirm('آیا از حذف این ارسال اطمینان دارید؟')) {
            $('#dispatchForm').find('input[name="submitType"]').remove();
            $('<input>').attr({ type: 'hidden', name: 'submitType', value: 'delete' })
                .appendTo('#dispatchForm');
            $('#dispatchForm').submit();
        }
    };

});