/**
 * toast.js  —  سیستم نوتیفیکیشن (shared)
 * ═══════════════════════════════════════════
 * قابل استفاده در تمام صفحات پروژه
 *
 * پیش‌نیاز:
 *   - وجود <div id="toastContainer"></div> در صفحه
 *   - لود شدن jQuery
 *   - لینک toast.css
 *
 * استفاده:
 *   showToast("متن پیام", "success");   // یا "error" یا "info"
 * ═══════════════════════════════════════════
 */

/**
 * نمایش یک toast notification
 * @param {string} message  - متن پیام
 * @param {"success"|"error"|"info"} type - نوع پیام
 * @param {number} [duration=5000] - مدت نمایش به میلی‌ثانیه
 */
function showToast(message, type, duration) {
    duration = duration || 5000;

    var titles = {
        success: "مورد قبول",
        error: "خطا",
        info: "اطلاعات"
    };

    var title = titles[type] || "پیغام";

    var $t = $(
        '<div class="toast-msg ' + type + '">' +
        '  <div class="toast-body">' +
        '    <div class="toast-title">' + title + '</div>' +
        '    <div class="toast-text">' + message + '</div>' +
        '  </div>' +
        '  <button class="toast-close-btn" onclick="$(this).closest(\'.toast-msg\').remove()">✕</button>' +
        '</div>'
    );

    $("#toastContainer").append($t);
    setTimeout(function () {
        $t.fadeOut(300, function () { $t.remove(); });
    }, duration);
}