var addedItems = [];
var batchTable = null;
var dispatchIsConfirmed = false;

$(function () {

    var csrfToken = $('input[name="__RequestVerificationToken"]').val();

    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            var method = (settings.type || 'GET').toUpperCase();
            if (method !== 'GET' && csrfToken) {
                xhr.setRequestHeader('RequestVerificationToken', csrfToken);
            }
        }
    });

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
        rowRenderer: function (rowData) {
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

    $(document).on('change', '.batch-chk', function () {
        var anyChecked = $('.batch-chk:checked').length > 0;
        if (anyChecked) {
            $('#dispatchForm').attr('data-has-item', 'true');
        } else {
            $('#dispatchForm').removeAttr('data-has-item');
        }
    });

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

    $('.ss-wrapper').each(function () {
        var $wrapper = $(this);

        $wrapper.find('.ss-display').off('mousedown.ss').on('mousedown.ss', function (e) {
            e.preventDefault();
            e.stopPropagation();

            var $dd = $wrapper.find('.ss-dropdown');
            var isOpen = $dd.is(':visible');

            $('.ss-dropdown').not($dd).hide();

            if (isOpen) {
                $dd.hide();
            } else {
                $dd.show();
                $dd.find('.ss-search').focus();
            }
        });

        $wrapper.find('.ss-search').off('input.ss').on('input.ss', function () {
            var q = $(this).val().toLowerCase();
            $(this).closest('.ss-dropdown').find('.ss-option').each(function () {
                var match = $(this).text().toLowerCase().indexOf(q) > -1;
                $(this).toggle(match);
                $(this).next('hr').toggle(match);
            });
        });

        bindSSOptionClick($wrapper);
    });

    $(document).off('mousedown.ss-close').on('mousedown.ss-close', function (e) {
        if (!$(e.target).closest('.ss-wrapper').length) {
            setTimeout(function () {
                $('.ss-dropdown').hide();
            }, 0);
        }
    });

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

    window.addSelectedItem = function () {
        if (dispatchIsConfirmed) {
            showToast('ارسال تایید شده — امکان ویرایش وجود ندارد', 'error');
            return;
        }

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

            var exists = addedItems.some(function (it) {
                return it.itemDetailStoreId === row.itemDetailStoreId;
            });
            if (exists) return;

            var rowIsDiluent = row.isDiluent === true || row.IsDiluent === true;

            addedItems.push({
                idx: addedItems.length,
                dispatchLineId: 0,
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
                dosePerVial: row.dosePerVialNum || 0,
                vvmStageId: rowIsDiluent ? '' : vvmStageId,
                vvmStageName: rowIsDiluent ? '-' : vvmStageName,
                note: '',
                uids: [],
                stockInRam: row.currentStock,
                requiresDiluent: row.requiresDiluent,
                isDiluent: rowIsDiluent,
                diluentItemDetailStoreId: row.diluentItemDetailStoreId,
                diluentItemId: row.diluentItemId,
                diluentItemName: row.diluentItemName
            });
            added++;

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
                    dosePerVial: 0,
                    vvmStageId: '',
                    vvmStageName: '-',
                    note: 'حلال خودکار',
                    uids: [],
                    stockInRam: 0,
                    requiresDiluent: false,
                    isDiluent: true
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

    window.removeItem = function (idx) {
        if (dispatchIsConfirmed) {
            showToast('ارسال تایید شده — امکان ویرایش وجود ندارد', 'error');
            return;
        }

        var item = addedItems[idx];
        var $stockCell = $('.stock-cell-' + item.itemDetailStoreId);
        if ($stockCell.length && !item.isDiluent) {
            var toReturn = getItemScannedCount(item);
            if (toReturn === 0) toReturn = item.doseCount;
            $stockCell.text((parseInt($stockCell.text()) || 0) + toReturn);
        }
        addedItems.splice(idx, 1);
        $.each(addedItems, function (i, it) { it.idx = i; });
        renderAddedItems();
    };

    function renderAddedItems() {
        var tbody = $('#addedItemsTbody');

        if (addedItems.length === 0) {
            tbody.html(
                '<tr><td colspan="11" style="padding:24px;color:#9ca3af;text-align:center;">' +
                'کالایی اضافه نشده</td></tr>'
            );
            $('#dispatchForm').removeAttr('data-has-record');
            rebuildHiddenInputs();
            return;
        }

        var html = '';
        $.each(addedItems, function (i, item) {
            var scannedCount = item.isDiluent ? '-' : getItemScannedCount(item);

            var actions;
            if (dispatchIsConfirmed) {
                actions = '<span style="color:#9ca3af;font-size:11px;">تایید شده</span>';
            } else if (item.isDiluent) {
                actions =
                    '<button type="button" class="btn-action btn-action-del" data-idx="' + i + '" title="حذف">' +
                    '<i class="fa fa-trash"></i>' +
                    '</button>';
            } else {
                actions =
                    '<button type="button" class="btn-action btn-action-uid" data-idx="' + i + '" title="افزودن UID">' +
                    '<i class="fa fa-qrcode"></i>' +
                    '</button> ' +
                    '<button type="button" class="btn-action btn-action-del" data-idx="' + i + '" title="حذف">' +
                    '<i class="fa fa-trash"></i>' +
                    '</button>';
            }

            html +=
                '<tr' + (item.isDiluent ? ' style="background:#f9fafb;"' : '') + '>' +
                '<td>' + (item.batchNumber || '-') + '</td>' +
                '<td>' + (item.itemName || '-') + '</td>' +
                '<td>' + (item.expiryDateShamsi || '-') + '</td>' +
                '<td>' + item.doseCount + '</td>' +
                '<td>' + (item.numVial || 0) + '</td>' +
                '<td>' + scannedCount + '</td>' +
                '<td>-</td>' +
                '<td>' + (item.vvmStageName || '-') + '</td>' +
                '<td>' + (item.manufacturerName || '-') + '</td>' +
                '<td>' + (item.note || '-') + '</td>' +
                '<td style="white-space:nowrap;">' + actions + '</td>' +
                '</tr>';
        });

        tbody.html(html);
        $('#dispatchForm').attr('data-has-record', 'true');
        rebuildHiddenInputs();
    }

    $(document).on('uids-updated', function () {
        renderAddedItems();
    });

    function rebuildHiddenInputs() {
        var $container = $('#hiddenInputsContainer').empty();

        $.each(addedItems, function (i, item) {
            function add(name, val) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].' + name +
                    '" value="' + (val === undefined || val === null ? '' : val) + '">'
                );
            }
            add('DispatchLineId', item.dispatchLineId);
            add('ItemDetailStoreId', item.itemDetailStoreId);
            add('ItemId', item.itemId);
            add('ItemName', item.itemName);
            add('StoreId', item.storeId);
            add('DoseCount', item.doseCount);
            add('VvmstageId', item.vvmStageId);
            add('BatchNumber', item.batchNumber);
            add('ExpiryDate', item.expiryDate);
            add('Note', item.note);
            add('IsDiluent', item.isDiluent ? 'true' : 'false');

            if (!item.isDiluent) {
                $.each(item.uids, function (j, uid) {
                    var uidVal = typeof uid === 'string' ? uid : (uid.uid || '');
                    $container.append(
                        '<input type="hidden" name="Items[' + i + '].Uids[' + j + ']" value="' + uidVal + '">'
                    );
                });
            }
        });
    }

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

        if (submitType === 'final') {
            var mismatchItems = [];
            var hasScannable = false;

            $.each(addedItems, function (_, item) {
                if (item.isDiluent) return;

                hasScannable = true;
                var scannedCount = getItemScannedCount(item);

                if (scannedCount > item.doseCount) {
                    mismatchItems.push(item.itemName +
                        ' (درخواستی: ' + item.doseCount +
                        ', اسکن‌شده: ' + scannedCount + ')');
                }
            });

            if (mismatchItems.length > 0) {
                showToast(
                    'تعداد UIDهای اسکن‌شده بیشتر از مقدار درخواستی است:\n' + mismatchItems.join('\n'),
                    'error'
                );
                return;
            }

            if (!hasScannable) {
                scanSessionId = null;
            }
        }

        $('#toastContainer').empty();
        $('#dispatchForm').find('input[name="submitType"]').remove();
        $('<input>').attr({ type: 'hidden', name: 'submitType', value: submitType })
            .appendTo('#dispatchForm');

        if (submitType === 'final' && scanSessionId) {
            $('#dispatchForm').find('input[name="scanSessionId"]').remove();
            $('<input>').attr({ type: 'hidden', name: 'scanSessionId', value: scanSessionId })
                .appendTo('#dispatchForm');
        }

        $('#dispatchForm').submit();
    });
});