var addedItems = [];
var batchTable = null;

$(function () {

    if (typeof dispatchServerMsg !== 'undefined' && dispatchServerMsg) {
        showToast(dispatchServerMsg, dispatchServerType || 'info');
    }

    var $dateInput = $('#Date');
    if (typeof DatePicker !== 'undefined' && DatePicker.attach) {
        DatePicker.attach($dateInput);
        if ($dateInput.val()) $dateInput.addClass('has-value');
        $dateInput.on('change', function () {
            $(this).toggleClass('has-value', !!$(this).val());
        });
    }

    batchTable = new PaginationTable({
        tableId: 'itemDetailsTable',
        tbodyId: 'itemDetailsTbody',
        pageSize: 5,
        columns: [
            { field: 'select' },
            { field: 'itemName' },
            { field: 'expiryDateShamsi' },
            { field: 'manufacturerName' },
            { field: 'currentStock' },
            { field: 'dosePerVialNum' },
            { field: 'storeName' },
            { field: 'batchNumber' }
        ],
        searchFields: ['itemName', 'batchNumber', 'expiryDateShamsi', 'manufacturerName'],
        emptyMessage: 'کالایی انتخاب نشده',
        rowRenderer: function (rowData, index) {
            return `
                <tr>
                    <td><input type="checkbox" class="batch-chk" data-row='${JSON.stringify(rowData).replace(/'/g, "&#39;")}' /></td>
                    <td>${rowData.itemName || '-'}</td>
                    <td>${rowData.expiryDateShamsi || '-'}</td>
                    <td>${rowData.manufacturerName || '-'}</td>
                    <td>${rowData.currentStock || 0}</td>
                    <td>${rowData.dosePerVialNum || 0}</td>
                    <td>${rowData.storeName || '-'}</td>
                    <td>${rowData.batchNumber || '-'}</td>
                </tr>
            `;
        }
    });

    // ── رویداد change چک‌باکس ──
    $(document).on('change', '.batch-chk', function () {
        var anyChecked = $('.batch-chk:checked').length > 0;
        if (anyChecked) {
            $('#dispatchForm').attr('data-has-item', 'true');
        } else {
            $('#dispatchForm').removeAttr('data-has-item');
        }
    });

    // ════════════════════════════════════════
    //  ۳. SearchableSelect - راه‌اندازی با mousedown
    // ════════════════════════════════════════

    /**
     * اتصال رویداد کلیک به گزینه‌های یک wrapper خاص
     */
    function bindSSOptionClick($wrapper) {
        $wrapper.find('.ss-option').off('click.ss').on('click.ss', function (e) {
            e.preventDefault();
            e.stopPropagation();

            var val = $(this).data('value');
            var text = $(this).text().trim();
            var $hidden = $wrapper.find('input[type="hidden"]');
            var $display = $wrapper.find('.ss-text');

            $hidden.val(val);
            $display.text(text);
            $wrapper.find('.ss-dropdown').hide();
            $hidden.trigger('change');
        });
    }

    /**
     * پر کردن گزینه‌های یک SearchableSelect
     */
    function populateSS(wrapperId, items, emptyText) {
        var $hidden = $('#' + wrapperId);
        var $wrapper = $hidden.closest('.ss-wrapper');
        if (!$wrapper.length) return;

        var $options = $wrapper.find('.ss-options');
        var $display = $wrapper.find('.ss-text');

        $hidden.val('');
        $display.text(emptyText || 'انتخاب کنید...');

        var html = '';
        $.each(items, function (_, item) {
            html += '<div class="ss-option" data-value="' + item.itemId + '">' +
                item.fullName +
                '</div><hr style="margin:5px"/>';
        });

        if (!html) {
            html = '<div class="ss-option-empty" style="padding:8px;color:#9ca3af;text-align:center;">' +
                (emptyText || 'موردی یافت نشد') + '</div>';
        }

        $options.html(html);
        bindSSOptionClick($wrapper);
    }

    // ── راه‌اندازی اولیه تمام SearchableSelectهای موجود ──
    $('.ss-wrapper').each(function () {
        var $wrapper = $(this);

        // باز کردن دراپ‌داون با mousedown (یک کلیک)
        $wrapper.find('.ss-display').off('mousedown.ss').on('mousedown.ss', function (e) {
            e.preventDefault();
            e.stopPropagation();

            var $dd = $wrapper.find('.ss-dropdown');
            var isOpen = $dd.is(':visible');

            // بستن سایر دراپ‌داون‌ها
            $('.ss-dropdown').not($dd).hide();

            if (isOpen) {
                $dd.hide();
            } else {
                $dd.show();
                $dd.find('.ss-search').focus();
            }
        });

        // جستجو
        $wrapper.find('.ss-search').off('input.ss').on('input.ss', function () {
            var q = $(this).val().toLowerCase();
            $(this).closest('.ss-dropdown').find('.ss-option').each(function () {
                var match = $(this).text().toLowerCase().indexOf(q) > -1;
                $(this).toggle(match);
                $(this).next('hr').toggle(match);
            });
        });

        // اتصال گزینه‌های اولیه
        bindSSOptionClick($wrapper);
    });

    // ── بستن دراپ‌داون با کلیک خارج (mousedown) ──
    $(document).off('mousedown.ss-close').on('mousedown.ss-close', function (e) {
        if (!$(e.target).closest('.ss-wrapper').length) {
            setTimeout(function () {
                $('.ss-dropdown').hide();
            }, 0);
        }
    });

    // ════════════════════════════════════════
    //  ۴. cascade: نوع کالا ← کالا ← سری ساخت
    // ════════════════════════════════════════

    $('#itemClassSelect').on('change', function () {
        var classId = $(this).val();
        if (batchTable) batchTable.setData([]);
        populateSS('itemSelect', [], 'در حال بارگذاری...');

        if (!classId) {
            populateSS('itemSelect', [], 'ابتدا نوع کالا را انتخاب کنید');
            return;
        }

        $.ajax({
            url: dispatchGetItemsByClass,
            data: { classId: classId },
            success: function (items) {
                populateSS('itemSelect', items, 'انتخاب کنید...');
            },
            error: function () {
                showToast('خطا در بارگذاری کالاها', 'error');
            }
        });
    });

    // ── رویداد change روی hidden input کالا ──
    $('#itemSelect').on('change', function () {
        var itemId = $(this).val();
        if (batchTable) batchTable.setData([]);
        if (!itemId) return;

        $.ajax({
            url: dispatchGetBatchesByItem,
            data: { itemId: itemId },
            success: function (rows) {
                if (!rows || rows.length === 0) {
                    showToast('موجودی برای این کالا یافت نشد', 'error');
                    return;
                }
                if (batchTable) batchTable.setData(rows);
            },
            error: function () {
                showToast('خطا در بارگذاری سری‌های ساخت', 'error');
            }
        });
    });

    // ════════════════════════════════════════
    //  ۵. افزودن آیتم به لیست
    // ════════════════════════════════════════

    window.addSelectedItem = function () {
        var $checked = $('.batch-chk:checked');
        if ($checked.length === 0) {
            showToast('لطفاً حداقل یک سری ساخت را انتخاب کنید.', 'error');
            return;
        }

        var doseCount = parseInt($('#doseCountInput').val()) || 0;
        if (doseCount <= 0) {
            showToast('برای کلیه کالاهای ارسالی تعداد را مشخص نمایید', 'error');
            return;
        }

        var vvmStageId = $('#vvmStagesSelect').val();
        var vvmStageName = $('#vvmStagesSelect option:selected').text();
        var added = 0;

        $checked.each(function () {
            var row = JSON.parse($(this).attr('data-row').replace(/&#39;/g, "'"));

            // جلوگیری از تکرار همان itemDetailStoreId
            var exists = addedItems.some(function (it) {
                return it.itemDetailStoreId === row.itemDetailStoreId;
            });
            if (exists) return;

            addedItems.push({
                idx: addedItems.length,
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
                stockInRam: row.currentStock,
                requiresDiluent: row.requiresDiluent,
                diluentItemDetailStoreId: row.diluentItemDetailStoreId,
                diluentItemId: row.diluentItemId,
                diluentItemName: row.diluentItemName,
                dispatchLineId: 0
            });
            added++;

            // حلال خودکار
            if (row.requiresDiluent && row.diluentItemDetailStoreId) {
                addedItems.push({
                    idx: addedItems.length,
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
                    dispatchLineId: 0
                });
            }
        });

        if (added === 0) {
            showToast('این سری‌های ساخت قبلاً اضافه شده‌اند', 'warning');
            return;
        }

        renderAddedItems();
        $checked.prop('checked', false);
        $('#dispatchForm').removeAttr('data-has-item');
        $('#doseCountInput').val('');
    };

    // ════════════════════════════════════════
    //  ۶. حذف آیتم
    // ════════════════════════════════════════

    window.removeItem = function (idx) {
        var item = addedItems[idx];
        var $stockCell = $('.stock-cell-' + item.itemDetailStoreId);
        if ($stockCell.length) {
            var toReturn = item.uids.length > 0 ? item.uids.length : item.doseCount;
            $stockCell.text((parseInt($stockCell.text()) || 0) + toReturn);
        }
        addedItems.splice(idx, 1);
        $.each(addedItems, function (i, it) { it.idx = i; });
        renderAddedItems();
    };

    // ════════════════════════════════════════
    //  ۷. رندر جدول کالاهای افزوده‌شده
    // ════════════════════════════════════════

    function renderAddedItems() {
        var tbody = $('#addedItemsTbody');

        if (addedItems.length === 0) {
            tbody.html(
                '<tr id="noAddedRow"><td colspan="10" ' +
                'style="padding:24px;color:#9ca3af;text-align:center;">کالایی اضافه نشده</td></tr>'
            );
            $('#dispatchForm').removeAttr('data-has-record');
            rebuildHiddenInputs();
            return;
        }

        var html = '';
        $.each(addedItems, function (i, item) {
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
                '<td style="white-space:nowrap;">' +
                '<button type="button" class="btn-action btn-action-uid" data-idx="' + i + '" title="افزودن UID">' +
                '<i class="fa fa-qrcode"></i>' +
                '</button> ' +
                '<button type="button" class="btn-action btn-action-del" data-idx="' + i + '" title="حذف">' +
                '<i class="fa fa-trash"></i>' +
                '</button>' +
                '</td>' +
                '</tr>';
        });

        tbody.html(html);
        $('#dispatchForm').attr('data-has-record', 'true');
        rebuildHiddenInputs();
    }
    function rebuildHiddenInputs() {
        var $container = $('#hiddenInputsContainer').empty();

        $.each(addedItems, function (i, item) {
            function add(name, val) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].' + name + '" value="' + (val || '') + '">'
                );
            }
            add('ItemDetailStoreId', item.itemDetailStoreId);
            add('ItemId', item.itemId);
            add('StoreId', item.storeId);
            add('DoseCount', item.doseCount);
            add('VvmstageId', item.vvmStageId);
            add('BatchNumber', item.batchNumber);
            add('ExpiryDate', item.expiryDate);
            add('Note', item.note);

            $.each(item.uids, function (j, uid) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].Uids[' + j + ']" value="' + uid + '">'
                );
            });
        });
    }

    // ════════════════════════════════════════
    //  ۹. دکمه‌های footer
    // ════════════════════════════════════════

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

        // ── اعتبارسنجی تعداد UID در حالت final ──
        if (submitType === 'final') {
            var mismatchItems = [];
            $.each(addedItems, function (_, item) {
                // محاسبه تعداد کل UIDهای اسکن‌شده برای این آیتم
                var scannedCount = 0;
                $.each(item.uids, function (_, entry) {
                    var cnt = typeof entry === 'object' ? (entry.count || 1) : 1;
                    scannedCount += cnt;
                });

                if (scannedCount !== item.doseCount) {
                    mismatchItems.push(item.itemName + ' (درخواستی: ' + item.doseCount + ', اسکن‌شده: ' + scannedCount + ')');
                }
            });

            if (mismatchItems.length > 0) {
                showToast(
                    'تعداد UIDهای اسکن‌شده با تعداد درخواستی مطابقت ندارد:\n' + mismatchItems.join('\n'),
                    'error'
                );
                return; 
            }
        }

        $('#toastContainer').empty();
        $('#dispatchForm').find('input[name="submitType"]').remove();
        $('<input>').attr({ type: 'hidden', name: 'submitType', value: submitType })
            .appendTo('#dispatchForm');

        if (submitType === 'final' && typeof scanSessionId !== 'undefined') {
            $('#dispatchForm').find('input[name="scanSessionId"]').remove();
            $('<input>').attr({ type: 'hidden', name: 'scanSessionId', value: scanSessionId })
                .appendTo('#dispatchForm');
        }

        $('#dispatchForm').submit();
    });

    window.confirmDelete = function () {
        if (typeof VSSM !== 'undefined' && VSSM.Modal) {
            VSSM.Modal.confirm({
                title: 'حذف ارسال',
                message: 'آیا از حذف این ارسال مطمئن هستید؟',
                confirmText: 'حذف',
                cancelText: 'انصراف',
                type: 'danger',
                onConfirm: function () {
                    $('#dispatchForm').find('input[name="submitType"]').remove();
                    $('<input>').attr({ type: 'hidden', name: 'submitType', value: 'delete' })
                        .appendTo('#dispatchForm');
                    $('#dispatchForm').submit();
                }
            });
        } else {
            if (confirm('آیا از حذف این ارسال اطمینان دارید؟')) {
                $('#dispatchForm').find('input[name="submitType"]').remove();
                $('<input>').attr({ type: 'hidden', name: 'submitType', value: 'delete' })
                    .appendTo('#dispatchForm');
                $('#dispatchForm').submit();
            }
        }
    };

});