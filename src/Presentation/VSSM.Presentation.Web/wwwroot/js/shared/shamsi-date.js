/**
 * shamsi-date.js  —  ابزارهای تاریخ شمسی (shared)
 * ═══════════════════════════════════════════
 * قابل استفاده در تمام فرم‌هایی که تاریخ شمسی دارند
 *
 * پیش‌نیاز:
 *   - لود شدن jQuery
 *
 * توابع عمومی:
 *   splitDate(val, dayId, monthId, yearId)
 *   combineDate(dayId, monthId, yearId, hiddenId)
 *   initDateAutoAdvance(dayId, monthId, yearId)
 *   initDateValidation(dayId, monthId, yearId)
 * ═══════════════════════════════════════════
 */

/**
 * یک رشته تاریخ را به سه input روز/ماه/سال تبدیل می‌کند
 * فرمت‌های پشتیبانی‌شده: yyyy/mm/dd  یا  yyyy-mm-dd
 *
 * @param {string} val      - رشته تاریخ
 * @param {string} dayId    - id فیلد روز
 * @param {string} monthId  - id فیلد ماه
 * @param {string} yearId   - id فیلد سال
 */
function splitDate(val, dayId, monthId, yearId) {
    if (!val) return;
    var parts = val.replace(/-/g, "/").split("/");
    if (parts.length !== 3) return;

    if (parts[0].length === 4) {
        // فرمت yyyy/mm/dd
        $("#" + yearId).val(parts[0]);
        $("#" + monthId).val(parseInt(parts[1]).toString());
        $("#" + dayId).val(parseInt(parts[2]).toString());
    } else {
        // فرمت احتمالی دیگر
        $("#" + yearId).val(parts[0]);
        $("#" + monthId).val(parts[1]);
        $("#" + dayId).val(parts[2]);
    }
}

/**
 * سه input روز/ماه/سال را به یک hidden input به‌صورت yyyy/mm/dd ترکیب می‌کند
 *
 * @param {string} dayId    - id فیلد روز
 * @param {string} monthId  - id فیلد ماه
 * @param {string} yearId   - id فیلد سال
 * @param {string} hiddenId - id فیلد hidden برای ذخیره نتیجه
 */
function combineDate(dayId, monthId, yearId, hiddenId) {
    var d = ($("#" + dayId).val() || "").padStart(2, "0");
    var m = ($("#" + monthId).val() || "").padStart(2, "0");
    var y = $("#" + yearId).val() || "";

    if (d !== "00" && m !== "00" && y.length === 4) {
        $("#" + hiddenId).val(y + "/" + m + "/" + d);
    }
}

/**
 * تنظیم auto-advance بین فیلدهای روز → ماه → سال
 * (بعد از تکمیل هر فیلد، فوکوس به فیلد بعدی می‌رود)
 *
 * @param {string} dayId   - id فیلد روز
 * @param {string} monthId - id فیلد ماه
 * @param {string} yearId  - id فیلد سال
 */
function initDateAutoAdvance(dayId, monthId, yearId) {
    $("#" + dayId).on("input", function () {
        if ($(this).val().length >= 2) $("#" + monthId).focus().select();
    });
    $("#" + monthId).on("input", function () {
        if ($(this).val().length >= 2) $("#" + yearId).focus().select();
    });
}

/**
 * محدودسازی مقادیر فیلدهای تاریخ (روز ≤ 31، ماه ≤ 12، سال حداکثر 4 رقم)
 * و جلوگیری از ورود کاراکتر غیرعددی
 *
 * @param {string} dayId   - id فیلد روز
 * @param {string} monthId - id فیلد ماه
 * @param {string} yearId  - id فیلد سال
 */
function initDateValidation(dayId, monthId, yearId) {
    $("#" + dayId).on("input", function () {
        var v = parseInt($(this).val());
        if (v > 31) $(this).val("31");
    });

    $("#" + monthId).on("input", function () {
        var v = parseInt($(this).val());
        if (v > 12) $(this).val("12");
    });

    $("#" + yearId).on("input", function () {
        if ($(this).val().length > 4) $(this).val($(this).val().slice(0, 4));
    });

    // فقط عدد قبول می‌کند
    $("#" + dayId + ", #" + monthId + ", #" + yearId).on("keypress", function (e) {
        if (!/[0-9]/.test(String.fromCharCode(e.which))) e.preventDefault();
    });
}