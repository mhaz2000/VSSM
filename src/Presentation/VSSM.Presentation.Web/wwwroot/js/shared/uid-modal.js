/**
 * uid-modal.js — مودال افزودن UID (shared)
 * در حالت New: validate سمت سرور، ذخیره فقط در RAM
 * در حالت Edit: اگر dispatchLineId > 0 باشد به سرور POST می‌کند
 */

var _uidCurrentLineIndex = null;

function openUidModal(idx) {
    _uidCurrentLineIndex = idx;
    var item = addedItems[idx];

    $('#uidModalStock').text(item.stockInRam !== undefined ? item.stockInRam : 0);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);

    $('#uidModalOverlay').addClass('open');
    $('#uidInput').val('').focus();
}

function closeUidModal() {
    $('#uidModalOverlay').removeClass('open');
    _uidCurrentLineIndex = null;
}

function submitUid() {
    if (_uidCurrentLineIndex === null) return;

    var uid = $('#uidInput').val().trim();

    // اعتبارسنجی — باید دقیقاً ۲۰ رقم باشد
    if (!/^\d{20}$/.test(uid)) {
        showToast('UID باید دقیقاً ۲۰ رقم و فقط شامل اعداد باشد', 'error');
        $('#uidInput').val('').focus();
        return;
    }

    var item = addedItems[_uidCurrentLineIndex];

    // چک موجودی RAM
    if (item.stockInRam <= 0) {
        showToast('موجودی این سری ساخت به اتمام رسیده است', 'error');
        $('#uidInput').val('').focus();
        return;
    }

    // چک تکراری بودن
    if (item.uids.indexOf(uid) > -1) {
        showToast('این UID قبلاً اضافه شده است', 'error');
        $('#uidInput').val('').focus();
        return;
    }

    var dispatchLineId = item.dispatchLineId || 0;
    var dispatchId = typeof window.dispatchId !== 'undefined' ? window.dispatchId : 0;

    if (dispatchLineId > 0 && dispatchId > 0) {
        // ══ حالت Edit — ثبت در DB ══
        $.ajax({
            url: setUidUrl,
            type: 'POST',
            data: { dispatchLineId: dispatchLineId, id: dispatchId, uid: uid },
            success: function (res) {
                if (!res.success) {
                    showToast(res.message || 'خطا در ثبت UID', 'error');
                    $('#uidInput').val('').focus();
                    return;
                }
                _addUidToRam(item, uid);
            },
            error: function () {
                showToast('خطا در ارتباط با سرور', 'error');
                $('#uidInput').val('').focus();
            }
        });
    } else {
        // ══ حالت New — validate سمت سرور، ذخیره فقط در RAM ══
        $.ajax({
            url: validateUidUrl,
            type: 'POST',
            data: { itemDetailStoreId: item.itemDetailStoreId, uid: uid },
            success: function (res) {
                if (!res.isSuccess) {
                    showToast(res.message, 'error');
                    $('#uidInput').val('').focus();
                    return;
                }
                _addUidToRam(item, uid);
            },
            error: function () {
                showToast('خطا در ارتباط با سرور', 'error');
                $('#uidInput').val('').focus();
            }
        });
    }
}

function _addUidToRam(item, uid) {
    item.uids.push(uid);
    item.stockInRam -= 1;

    $('#uidModalStock').text(item.stockInRam);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);

    // مودال بسته نمی‌شود — کاربر خودش می‌بندد (بند ۱۰e)
    $('#uidInput').val('').focus();
}

function removeUid(itemIdx, uidIdx) {
    var item = addedItems[itemIdx];
    item.uids.splice(uidIdx, 1);
    item.stockInRam += 1;

    $('#uidModalStock').text(item.stockInRam);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);
}

function renderUidList(item) {
    var html = '';
    $.each(item.uids, function (j, uid) {
        html +=
            '<div class="uid-chip">' +
            '<span>' + uid + '</span>' +
            '<button type="button" class="uid-chip-del" ' +
            'onclick="removeUid(' + item.idx + ',' + j + ')">✕</button>' +
            '</div>';
    });
    $('#uidList').html(
        html || '<div style="color:#9ca3af;font-size:12px;text-align:center;">UID‌ای اضافه نشده</div>'
    );
}

$(function () {
    // auto-submit بعد از ۲۰ کاراکتر — برای اسکنر
    $('#uidInput').on('input', function () {
        var val = $(this).val().replace(/\D/g, '');
        if (val.length > 20) val = val.slice(0, 20);
        $(this).val(val);
        if (val.length === 20) submitUid();
    });

    // Enter برای تایپ دستی
    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); submitUid(); }
    });

    // بستن با کلیک روی overlay
    $('#uidModalOverlay').on('click', function (e) {
        if ($(e.target).is('#uidModalOverlay')) closeUidModal();
    });
});