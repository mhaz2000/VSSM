/**
 * uid-modal.js — مودال افزودن UID (نسخه جدید با Session موقت)
 * وابستگی‌ها: scanSessionId (در View تعریف شده)
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

    var barcode = $('#uidInput').val().trim();
    if (!barcode) {
        showToast('لطفاً بارکد را وارد کنید', 'error');
        $('#uidInput').val('').focus();
        return;
    }

    var item = addedItems[_uidCurrentLineIndex];
    var dispatchLineId = item.dispatchLineId || 0;      // برای آیتم‌های جدید ۰ است
    var itemDetailStoreId = item.itemDetailStoreId;     // همیشه موجود است

    // ═══════════════════════════════════════════════════════
    //  ارسال درخواست به سرور برای افزودن UID به سشن
    // ═══════════════════════════════════════════════════════
    $.ajax({
        url: '/Dispatches/AddUidToSession',
        type: 'POST',
        data: {
            sessionId: scanSessionId,
            dispatchLineId: dispatchLineId,
            itemDetailStoreId: itemDetailStoreId,
            barcode: barcode
        },
        success: function (res) {
            if (!res.success) {
                showToast(res.message || 'خطا در اعتبارسنجی', 'error');
                $('#uidInput').val('').focus();
                return;
            }
            // ✅ موفق: UID را به لیست کلاینت اضافه کن
            _addUidToRam(item, barcode);
        },
        error: function (xhr) {
            var message = 'خطا در ارتباط با سرور';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            }
            showToast(message, 'error');
            $('#uidInput').val('').focus();
        }
    });
}

function _addUidToRam(item, uid) {
    item.uids.push(uid);
    item.stockInRam -= 1;

    $('#uidModalStock').text(item.stockInRam);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);

    // مودال بسته نمی‌شود — کاربر خودش می‌بندد
    $('#uidInput').val('').focus();
}

function removeUid(itemIdx, uidIdx) {
    var item = addedItems[itemIdx];
    var uid = item.uids[uidIdx];

    // ── ارسال درخواست حذف به سرور ──
    $.ajax({
        url: '/Dispatches/RemoveUidFromSession',
        type: 'POST',
        data: {
            sessionId: scanSessionId,
            uid: uid
        },
        success: function (res) {
            if (res.success) {
                // ✅ حذف از لیست کلاینت
                item.uids.splice(uidIdx, 1);
                item.stockInRam += 1;

                $('#uidModalStock').text(item.stockInRam);
                $('#uidModalCount').text(item.uids.length);
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

    let uidTimer;

    $('#uidInput').on('input', function () {

        let val = $(this).val().replace(/\s/g, '');

        $(this).val(val);

        clearTimeout(uidTimer);

        uidTimer = setTimeout(function () {

            if (/^\d{20}$/.test(val)) {
                submitUid();
                return;
            }

            if (val.length > 30) {
                submitUid();
                return;
            }

        }, 200);

    });



    $('#uidInput').on('keydown', function (e) {

        if (e.key === 'Enter') {

            e.preventDefault();

            let val = $(this).val().trim();


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

});