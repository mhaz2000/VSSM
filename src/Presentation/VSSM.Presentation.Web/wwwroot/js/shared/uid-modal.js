var _uidCurrentLineIndex = null;
var _isSubmitting = false;

function openUidModal(idx) {
    _uidCurrentLineIndex = idx;
    var item = addedItems[idx];

    // ⭐ دریافت مقدار requestedDose مستقیماً از سلول جدول
    var row = $('#addedItemsTable tbody tr').eq(idx);
    var doseFromTable = parseInt(row.find('td:eq(3)').text().trim()) || 0;
    if (doseFromTable > 0) {
        item.requestedDose = doseFromTable; // به‌روزرسانی در آرایه
    }

    var displayName = item.itemName || 'کالا';
    if (item.note && item.note.includes('حلال')) {
        displayName += ' (حلال)';
    }
    $('#uidModalTitle').text('افزودن UID برای ' + displayName);

    $('#uidModalStock').text(item.stockInRam !== undefined ? item.stockInRam : 0);
    updateUidModalCount(item);
    renderUidList(item);

    $('#uidModalOverlay').addClass('open');
    $('#uidInput').val('').focus();
    _isSubmitting = false;
}

function closeUidModal() {
    $('#uidModalOverlay').removeClass('open');
    _uidCurrentLineIndex = null;
    _isSubmitting = false;
}

function submitUid() {
    if (_uidCurrentLineIndex === null) return;
    if (_isSubmitting) {
        showToast('در حال پردازش درخواست قبلی...', 'warning');
        return;
    }

    var barcode = $('#uidInput').val().trim();
    if (!barcode) {
        showToast('لطفاً بارکد را وارد کنید', 'error');
        $('#uidInput').val('').focus();
        return;
    }

    var item = addedItems[_uidCurrentLineIndex];

    // چک تکراری
    var exists = item.uids.some(function (entry) {
        var existingUid = typeof entry === 'string' ? entry : entry.uid;
        return existingUid === barcode;
    });
    if (exists) {
        showToast('این UID قبلاً اسکن شده است', 'warning');
        $('#uidInput').val('').focus();
        return;
    }

    var dispatchLineId = item.dispatchLineId || 0;
    var itemDetailStoreId = item.itemDetailStoreId;

    var url = typeof addUidToSessionUrl !== 'undefined'
        ? addUidToSessionUrl
        : '/Dispatches/AddUidToSession';

    _isSubmitting = true;

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            sessionId: scanSessionId,
            dispatchLineId: dispatchLineId,
            itemDetailStoreId: itemDetailStoreId,
            barcode: barcode
        },
        success: function (res) {
            _isSubmitting = false;
            if (!res.success) {
                showToast(res.message || 'خطا در اعتبارسنجی', 'error');
                $('#uidInput').val('').focus();
                return;
            }

            var doseCount = res.count || 1;

            var currentTotal = 0;
            $.each(item.uids, function (_, entry) {
                var cnt = typeof entry === 'object' ? (entry.count || 1) : 1;
                currentTotal += cnt;
            });
            var newTotal = currentTotal + doseCount;
            // ⭐ استفاده از requestedDose که از جدول به‌روز شده
            var requestedDose = item.requestedDose || 0;

            if (newTotal > requestedDose) {
                showToast('تعداد کل دوزهای اسکن‌شده (' + newTotal + ') از تعداد درخواستی (' + requestedDose + ') بیشتر است', 'error');
                // حذف UID از سشن
                $.ajax({
                    url: removeUidFromSessionUrl || '/Dispatches/RemoveScanUid',
                    type: 'POST',
                    data: {
                        sessionId: scanSessionId,
                        uid: barcode
                    },
                    success: function (delRes) {
                        if (!delRes.success) {
                            showToast('خطا در حذف UID از سشن', 'error');
                        }
                        $('#uidInput').val('').focus();
                    },
                    error: function () {
                        showToast('خطا در ارتباط با سرور برای حذف UID', 'error');
                        $('#uidInput').val('').focus();
                    }
                });
                return;
            }

            _addUidToRam(item, barcode, doseCount);
        },
        error: function (xhr) {
            _isSubmitting = false;
            var message = 'خطا در ارتباط با سرور';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            } else if (xhr.status === 405) {
                message = 'متد درخواست مجاز نیست (405) – لطفاً مسیر API را بررسی کنید.';
            } else if (xhr.status === 404) {
                message = 'مسیر API یافت نشد (404) – لطفاً مسیر را بررسی کنید.';
            }
            showToast(message, 'error');
            $('#uidInput').val('').focus();
        }
    });
}

function _addUidToRam(item, uid, count) {
    var currentTotal = 0;
    $.each(item.uids, function (_, entry) {
        var cnt = typeof entry === 'object' ? (entry.count || 1) : 1;
        currentTotal += cnt;
    });
    var requestedDose = item.requestedDose || 0;
    if (currentTotal + (count || 1) > requestedDose) {
        showToast('خطای داخلی: مجموع دوزها از حد مجاز بیشتر شد!', 'error');
        return;
    }

    item.uids.push({
        uid: uid,
        count: count || 1
    });
    item.stockInRam -= (count || 1);

    $('#uidModalStock').text(item.stockInRam);
    updateUidModalCount(item);
    renderUidList(item);

    $('#uidInput').val('').focus();
}

function removeUid(itemIdx, uidIdx) {
    var item = addedItems[itemIdx];
    var entry = item.uids[uidIdx];
    var uid = entry.uid;
    var count = entry.count || 1;

    var url = typeof removeUidFromSessionUrl !== 'undefined'
        ? removeUidFromSessionUrl
        : '/Dispatches/RemoveScanUid';

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            sessionId: scanSessionId,
            uid: uid
        },
        success: function (res) {
            if (res.success) {
                item.uids.splice(uidIdx, 1);
                item.stockInRam += count;

                $('#uidModalStock').text(item.stockInRam);
                updateUidModalCount(item);
                renderUidList(item);

                showToast('UID با موفقیت حذف شد', 'success');
            } else {
                showToast('خطا در حذف UID از سشن', 'error');
            }
        },
        error: function () {
            showToast('خطا در ارتباط با سرور', 'error');
        }
    });
}

function updateUidModalCount(item) {
    var totalCount = 0;
    $.each(item.uids, function (_, entry) {
        var count = typeof entry === 'object' ? (entry.count || 1) : 1;
        totalCount += count;
    });
    $('#uidModalCount').text(totalCount);
}

function renderUidList(item) {
    var html = '';
    $.each(item.uids, function (j, entry) {
        var uid = typeof entry === 'string' ? entry : entry.uid;
        var count = typeof entry === 'object' ? (entry.count || 1) : 1;
        var display = count > 1 ? uid + ' (' + count + ' دوز)' : uid;
        html +=
            '<div class="uid-chip">' +
            '<span>' + display + '</span>' +
            '<button type="button" class="uid-chip-del" ' +
            'onclick="removeUid(' + item.idx + ',' + j + ')">✕</button>' +
            '</div>';
    });
    $('#uidList').html(
        html || '<div style="color:#9ca3af;font-size:12px;text-align:center;">UID‌ای اضافه نشده</div>'
    );
}

$(function () {
    var uidTimer;

    $('#uidInput').on('input', function () {
        var val = $(this).val().replace(/\s/g, '');
        $(this).val(val);

        clearTimeout(uidTimer);
        uidTimer = setTimeout(function () {
            if (/^\d{20}$/.test(val) || val.length > 30) {
                submitUid();
            }
        }, 200);
    });

    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            var val = $(this).val().trim();
            if (val.length >= 20) {
                submitUid();
            }
        }
    });

    $('#uidModalOverlay').on('click', function (e) {
        if ($(e.target).is('#uidModalOverlay')) {
            closeUidModal();
        }
    });

    $(document).on('click', '.btn-action-uid', function (e) {
        e.stopPropagation();
        var idx = $(this).data('idx');
        if (idx !== undefined) {
            openUidModal(parseInt(idx));
        }
    });

    $(document).on('click', '.btn-action-del', function (e) {
        e.stopPropagation();
        var idx = $(this).data('idx');
        if (idx !== undefined && typeof removeItem === 'function') {
            removeItem(parseInt(idx));
        }
    });
});