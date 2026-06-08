/**
 * vvm-cascade.js  —  cascade dropdown نوع/مرحله VVM (shared)
 * ═══════════════════════════════════════════
 * قابل استفاده در هر فرمی که VVM دارد
 *
 * پیش‌نیاز:
 *   - لود شدن jQuery
 *   - وجود select با id="VvmTypeId" و id="VvmStageId" در صفحه
 *
 * استفاده:
 *   initVvmCascade();                            // با id پیش‌فرض
 *   initVvmCascade("VvmTypeId", "VvmStageId");   // با id سفارشی
 * ═══════════════════════════════════════════
 */

/**
 * فعال/غیرفعال کردن select مرحله VVM بر اساس انتخاب نوع VVM
 *
 * @param {string} [typeId="VvmTypeId"]   - id select نوع VVM
 * @param {string} [stageId="VvmStageId"] - id select مرحله VVM
 */
function initVvmCascade(typeId, stageId) {
    typeId = typeId || "VvmTypeId";
    stageId = stageId || "VvmStageId";

    var $type = $("#" + typeId);
    var $stage = $("#" + stageId);

    $type.on("change", function () {
        if ($(this).val()) {
            $stage.prop("disabled", false);
        } else {
            $stage.prop("disabled", true).val("");
        }
    });
}