/**
 * arrival-form.js  —  لاجیک اختصاصی فرم ورودی
 * ═══════════════════════════════════════════
 * وابستگی‌ها (باید قبل از این فایل لود شده باشند):
 *   - jQuery
 *   - toast.js        (showToast)
 *   - shamsi-date.js  (splitDate, combineDate, initDateAutoAdvance, initDateValidation)
 *   - vvm-cascade.js  (initVvmCascade)
 * ═══════════════════════════════════════════
 */

$(function () {

    // ════════════════════════════════════════
    //  ۱. متغیرهای کمکی
    // ════════════════════════════════════════

    /** جلوگیری از اجرای blur هنگام submit */
    var isSubmitting = false;

    $("#arrivalForm").on("submit", function () {
        isSubmitting = true;
        setTimeout(function () { isSubmitting = false; }, 500);
    });

    // ════════════════════════════════════════
    //  ۲. نمایش toast سرور (TempData)
    // ════════════════════════════════════════

    /* مقادیر توسط View تزریق می‌شوند — ر.ک. بخش View */
    if (typeof arrivalServerMsg !== "undefined" && arrivalServerMsg) {
        showToast(arrivalServerMsg, arrivalServerType || "info");
    }

    // ════════════════════════════════════════
    //  ۳. مقداردهی اولیه ماژول‌های shared
    // ════════════════════════════════════════

    // cascade VVM
    initVvmCascade("VvmTypeId", "VvmStageId");

    // تاریخ ورود
    initDateAutoAdvance("DateDay", "DateMonth", "DateYear");
    initDateValidation("DateDay", "DateMonth", "DateYear");

    $("#DateDay, #DateMonth, #DateYear").on("input", function () {
        combineDate("DateDay", "DateMonth", "DateYear", "Date");
    });

    // تفکیک تاریخ‌های موجود (حالت ویرایش)
    splitDate($("#Date").val(), "DateDay", "DateMonth", "DateYear");
    splitDate($("#ProductionDate").val(), "ProdDay", "ProdMonth", "ProdYear");
    splitDate($("#ExpiryDate").val(), "ExpDay", "ExpMonth", "ExpYear");

    // ════════════════════════════════════════
    //  ۴. مقداردهی فیلدهای خودکار (کالا / دوز)
    // ════════════════════════════════════════

    /**
     * پر کردن فیلدهای نمایشی و hidden مربوط به سری ساخت
     * @param {string} itemName
     * @param {string|number} itemId
     * @param {string} doseName
     * @param {string|number} dosePerVialId
     */
    function setAutoFields(itemName, itemId, doseName, dosePerVialId) {
        $("#ItemClassDisplay").val("واکسن");
        $("#ItemDisplay").val(itemName || "");
        $("#DoseDisplay").val(doseName || "");
        $("#ItemClassIdHidden").val("");                  // نوع کالا پیش‌فرض واکسن
        $("#ItemIdHidden").val(itemId || "");
        $("#DosePerVialIdHidden").val(dosePerVialId || "");
    }

    /** پاک کردن فیلدهای خودکار */
    function clearAutoFields() {
        $("#ItemClassDisplay").val("");
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
    }

    /** پاک کردن فیلدهای تاریخ و تعداد مرتبط با سری ساخت */
    function clearBatchFields() {
        $("#QuantityHidden").val("");
        $("#ProdDay, #ProdMonth, #ProdYear").val("");
        $("#ProductionDate").val("");
        $("#ExpDay, #ExpMonth, #ExpYear").val("");
        $("#ExpiryDate").val("");
    }

    // مقداردهی اولیه در حالت ویرایش
    if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
        setAutoFields(
            arrivalExistItemName,
            arrivalExistItemId,
            arrivalExistDoseName,
            arrivalExistDoseId
        );
    } else {
        $("#ItemClassDisplay").val("واکسن");
    }

    // مقادیر نمایشی کالا و دوز در حالت ویرایش
    if (typeof arrivalItemName !== "undefined") $("#ItemDisplay").val(arrivalItemName);
    if (typeof arrivalDoseName !== "undefined") $("#DoseDisplay").val(arrivalDoseName);

    // ════════════════════════════════════════
    //  ۵. GetBatchInfo — بررسی سری ساخت
    // ════════════════════════════════════════

    $("#BatchNum").on("blur", function () {
        if (isSubmitting) return;
        var batchNum = $(this).val().trim();
        if (!batchNum) return;

        var arrivalId = $("#ArrivalId").val() || 0;

        $.ajax({
            url: arrivalGetBatchInfoUrl,   // مقدار از View تزریق می‌شود
            type: "GET",
            data: { batchNum: batchNum, arrivalId: arrivalId },
            success: function (res) {
                if (!res.success) {
                    showToast(res.message, "error");
                    clearBatchFields();
                    clearAutoFields();
                    return;
                }

                showToast("سری ساخت تایید شد", "success");
                $("#QuantityHidden").val(res.quantity);

                // تاریخ تولید
                if (res.productionDateShamsi) {
                    var p = res.productionDateShamsi.split("/");
                    if (p.length === 3) {
                        $("#ProdYear").val(p[0]);
                        $("#ProdMonth").val(p[1]);
                        $("#ProdDay").val(p[2]);
                        $("#ProductionDate").val(res.productionDateShamsi);
                    }
                }

                // تاریخ انقضا
                if (res.expiryDateShamsi) {
                    var e = res.expiryDateShamsi.split("/");
                    if (e.length === 3) {
                        $("#ExpYear").val(e[0]);
                        $("#ExpMonth").val(e[1]);
                        $("#ExpDay").val(e[2]);
                        $("#ExpiryDate").val(res.expiryDateShamsi);
                    }
                }

                // فیلدهای خودکار کالا و دوز
                setAutoFields(res.itemName, res.itemId, res.doseName, res.dosePerVialId);
            },
            error: function () {
                showToast("خطا در بررسی سری ساخت", "error");
                clearBatchFields();
                clearAutoFields();
            }
        });
    });

    // ════════════════════════════════════════
    //  ۶. دکمه‌های footer (ثبت / پیش‌نویس)
    // ════════════════════════════════════════

    $(".js-footer-btn").on("click", function () {
        var command = $(this).val();

        // غیرفعال کردن validation برای این submit
        var $v = $("#arrivalForm").data("validator");
        if ($v) $v.settings.ignore = "*";

        // حذف command قبلی و افزودن جدید
        $("#arrivalForm").find("input[name='command']").remove();
        $("<input>").attr({ type: "hidden", name: "command", value: command })
            .appendTo("#arrivalForm");

        $("#arrivalForm").submit();
    });

});