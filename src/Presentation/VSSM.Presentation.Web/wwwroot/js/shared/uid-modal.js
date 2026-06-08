/**
 * uid-modal.js  —  مودال افزودن UID (shared)
 * ═══════════════════════════════════════════
 * قابل استفاده در: Dispatch و هر فرمی با UID scanning
 *
 * پیش‌نیاز:
 *   - jQuery
 *   - uid-modal.css
 *   - toast.js (showToast)
 *   - آرایه `addedItems` در scope فراخواننده
 *   - متغیر `setUidUrl` در scope فراخواننده (آدرس اکشن SetUid)
 *
 * توابع عمومی:
 *   openUidModal(idx)
 *   closeUidModal()
 *   submitUid()
 *   removeUid(itemIdx, uidIdx)
 * ═══════════════════════════════════════════
 */

var _uidCurrentLineIndex = null;
var _uidCurrentStock = 0;

/**
 * باز کردن مودال uid برای یک ردیف
 * @param {number} idx - ایندکس آیتم در آرایه addedItems
 */
function openUidModal(idx) {
    _uidCurrentLineIndex = idx;
    var item = addedItems[idx];
    _uidCurrentStock = item.stockInRam !== undefined ? item.stockInRam : 0;

    $('#uidModalStock').text(_uidCurrentStock);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);

    $('#uidModalOverlay').addClass('open');
    $('#uidInput').val('').focus();
}

/**
 * بستن مودال
 */
function closeUidModal() {
    $('#uidModalOverlay').removeClass('open');
    _uidCurrentLineIndex = null;
}

/**
 * ثبت یک uid جدید
 */
function submitUid() {
    var uid = $('#uidInput').val().trim();
    if (!uid || _uidCurrentLineIndex === null) return;

    var item = addedItems[_uidCurrentLineIndex];

    if (_uidCurrentStock <= 0) {
        showToast('موجودی این سری ساخت به اتمام رسیده است', 'error');
        return;
    }

    $.ajax({
        url: setUidUrl,   // مقدار از صفحه تزریق می‌شود
        type: 'POST',
        data: {
            dispatchLineId: item.itemDetailStoreId,
            id: 0,
            uid: uid
        },
        success: function (res) {
            if (!res.success) {
                showToast(res.message, 'error');
                return;
            }

            // کسر از موجودی RAM
            _uidCurrentStock -= res.count;
            item.stockInRam = _uidCurrentStock;
            item.uids.push(uid);

            $('#uidModalStock').text(_uidCurrentStock);
            $('#uidModalCount').text(item.uids.length);
            renderUidList(item);

            // مودال بسته نمی‌شود — کاربر خودش می‌بندد
            $('#uidInput').val('').focus();
        },
        error: function () {
            showToast('خطا در ارتباط با سرور', 'error');
        }
    });
}

/**
 * حذف یک uid از لیست
 * @param {number} itemIdx - ایندکس آیتم
 * @param {number} uidIdx  - ایندکس uid داخل آیتم
 */
function removeUid(itemIdx, uidIdx) {
    var item = addedItems[itemIdx];
    item.uids.splice(uidIdx, 1);

    // برگشت موجودی به RAM
    _uidCurrentStock++;
    item.stockInRam = _uidCurrentStock;

    $('#uidModalStock').text(_uidCurrentStock);
    $('#uidModalCount').text(item.uids.length);
    renderUidList(item);
}

/**
 * رندر لیست uid های یک آیتم داخل مودال
 * @param {object} item
 */
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

// Enter در input uid
$(function () {
    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            submitUid();
        }
    });
});