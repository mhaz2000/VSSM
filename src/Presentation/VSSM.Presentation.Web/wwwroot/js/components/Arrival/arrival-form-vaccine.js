/**
 * arrival-form-vaccine.js  —  منطق اختصاصی فرم ورودی واکسن
 */

$(function () {
    "use strict";

    var _submitting = false;

    // ═══════════════════════════════════════════════════════════
    //  کمکی: اجرای امن
    // ═══════════════════════════════════════════════════════════

    function _safe(fn, label) {
        try { fn(); } catch (err) {
            if (window.console) console.error("[arrival-form-vaccine] " + (label || "خطا") + ":", err);
        }
    }

    function _clearValidationErrors() {
        $(".field-validation-error").remove();
        $(".input-validation-error").removeClass("input-validation-error");
    }

    // ═══════════════════════════════════════════════════════════
    //  تنظیم command دکمه‌ها (فقط مقداردهی، بدون دخالت در submit)
    // ═══════════════════════════════════════════════════════════

    $(document).on("click", ".js-submit-btn", function () {
        // فقط مقدار commandHidden را تنظیم کن
        // هیچ preventDefault یا submit دستی انجام نده
        $("#commandHidden").val($(this).val());
    });

    // ═══════════════════════════════════════════════════════════
    //  رویداد ارسال فرم
    // ═══════════════════════════════════════════════════════════

    $("#arrivalForm").on("submit", function () {
        // جلوگیری از ارسال مجدد
        if (_submitting) return false;
        _submitting = true;

        var isHalal = ($("#ArrivalTypeHidden").val() === "halal");

        // فقط برای واکسن تاریخ‌ها را ترکیب کن
        if (!isHalal) {
            _safe(function () {
                combineDate("DateDay", "DateMonth", "DateYear", "Date");

                if ($("#ProdYear").val()) {
                    $("#ProductionDate").val(
                        $("#ProdYear").val() + "/" +
                        $("#ProdMonth").val() + "/" +
                        $("#ProdDay").val()
                    );
                }

                if ($("#ExpYear").val()) {
                    $("#ExpiryDate").val(
                        $("#ExpYear").val() + "/" +
                        $("#ExpMonth").val() + "/" +
                        $("#ExpDay").val()
                    );
                }
            }, "ترکیب تاریخ‌ها");
        }

        // آزادسازی قفل بعد از ۱.۵ ثانیه
        setTimeout(function () {
            _submitting = false;
        }, 1500);

        // اجازه ارسال عادی فرم داده می‌شود
        return true;
    });

    // ═══════════════════════════════════════════════════════════
    //  بقیه کد (پاک‌سازی، مقداردهی اولیه، جستجوی سری ساخت)
    //  بدون تغییر باقی می‌ماند
    // ═══════════════════════════════════════════════════════════

    var VACCINE_TEXT_FIELDS = "#ItemDisplay, #DoseDisplay, #BatchNum";
    var VACCINE_HIDDEN_FIELDS = "#ItemIdHidden, #DosePerVialIdHidden, #QuantityHidden";
    var VACCINE_DATE_FIELDS_SHAMSI =
        "#ProdDay, #ProdMonth, #ProdYear, #ExpDay, #ExpMonth, #ExpYear";
    var VACCINE_DATE_FIELDS_MILADI =
        "#ProdDayM, #ProdMonthM, #ProdYearM, #ExpDayM, #ExpMonthM, #ExpYearM";

    var HALAL_HIDDEN_FIELDS = "#ItemIdHidden, #DosePerVialIdHidden, #QuantityHidden, #HalalDoseValue";

    window.clearVaccineFields = function () {
        $(VACCINE_TEXT_FIELDS).val("");
        $(VACCINE_HIDDEN_FIELDS).val("");
        $("#ItemClassDisplay").val("واکسن");

        $(VACCINE_DATE_FIELDS_SHAMSI).val("");
        $(VACCINE_DATE_FIELDS_MILADI).val("");
        $("#ProductionDate, #ExpiryDate").val("").removeClass("has-value");

        $(".btn-date-toggle").hide();

        $("#prod-shamsi-wrap, #exp-shamsi-wrap").show();
        $("#prod-miladi-wrap, #exp-miladi-wrap").hide();

        _clearValidationErrors();
    };

    window.clearHalalFields = function () {
        $("#HalalItemSelect").val("").trigger("change");
        $(HALAL_HIDDEN_FIELDS).val("");

        $("#HalalDoseSelect")
            .empty()
            .append('<option value="">ابتدا کالا را انتخاب کنید</option>')
            .prop("disabled", true);

        $("#QuantityInput, #NumVialsInput").val("");
        $("#HalalProdDate, #HalalExpDate").val("").removeClass("has-value");

        $("#ItemClassDisplay").val("حلال");

        _clearValidationErrors();
    };

    // ── مقداردهی اولیه ──
    _safe(function () { initVvmCascade("VvmTypeId", "VvmStageId"); }, "initVvmCascade");
    _safe(function () { initDateAutoAdvance("DateDay", "DateMonth", "DateYear"); }, "initDateAutoAdvance");
    _safe(function () { initDateValidation("DateDay", "DateMonth", "DateYear"); }, "initDateValidation");

    $("#DateDay, #DateMonth, #DateYear").on("input", function () {
        _safe(function () { combineDate("DateDay", "DateMonth", "DateYear", "Date"); }, "combineDate");
    });

    _safe(function () { splitDate($("#Date").val(), "DateDay", "DateMonth", "DateYear"); }, "splitDate(Date)");
    _safe(function () { splitDate($("#ProductionDate").val(), "ProdDay", "ProdMonth", "ProdYear"); }, "splitDate(Production)");
    _safe(function () { splitDate($("#ExpiryDate").val(), "ExpDay", "ExpMonth", "ExpYear"); }, "splitDate(Expiry)");

    if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
        _setAutoFields(arrivalExistItemName, arrivalExistItemId, arrivalExistDoseName, arrivalExistDoseId);
    } else {
        $("#ItemClassDisplay").val("واکسن");
    }

    function _setAutoFields(itemName, itemId, doseName, dosePerVialId) {
        console.log("itemId received:", itemId);
        $("#ItemClassDisplay").val("واکسن");
        $("#ItemDisplay").val(itemName || "");
        $("#DoseDisplay").val(doseName || "");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val(itemId || "");         
        $("#ItemIdHiddenMain").val(itemId || "");
        $("#DosePerVialIdHidden").val(dosePerVialId || "");
        if (window.updateAddButtonState) window.updateAddButtonState();
    }

    function _clearAutoFields() {
        $("#ItemClassDisplay").val("");
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
        if (window.updateAddButtonState) window.updateAddButtonState();
    }

    function _clearBatchFields() {
        $("#QuantityHidden").val("");
        $(VACCINE_DATE_FIELDS_SHAMSI).val("");
        $("#ProductionDate").val("");
        $("#ExpiryDate").val("");
        $(VACCINE_DATE_FIELDS_MILADI).val("");
        $("#prod-shamsi-wrap, #exp-shamsi-wrap").show();
        $("#prod-miladi-wrap, #exp-miladi-wrap").hide();
        $(".btn-date-toggle").hide();
        if (window.updateAddButtonState) window.updateAddButtonState();
    }

    function _applyDateDisplay(target, hasShamsi, hasMiladi) {
        var $toggle = $(".btn-date-toggle[data-target='" + target + "']");
        var $shamsiWrap = $("#" + target + "-shamsi-wrap");
        var $miladiWrap = $("#" + target + "-miladi-wrap");

        if (hasShamsi && hasMiladi) {
            $toggle.show().attr("title", "نمایش میلادی");
            $shamsiWrap.show();
            $miladiWrap.hide();
        } else if (hasMiladi) {
            $toggle.hide();
            $shamsiWrap.hide();
            $miladiWrap.show();
        } else if (hasShamsi) {
            $toggle.hide();
            $shamsiWrap.show();
            $miladiWrap.hide();
        } else {
            $toggle.hide();
            $shamsiWrap.hide();
            $miladiWrap.hide();
        }
        if (window.updateAddButtonState) window.updateAddButtonState();
    }

    // ── جستجوی سری ساخت ──
    var _batchTimer = null;
    var BATCH_DEBOUNCE_MS = 600;

    $("#BatchNum").on("input", function () {
        clearTimeout(_batchTimer);
        var $input = $(this);

        _batchTimer = setTimeout(function () {
            if (_submitting) return;
            if ($("#ArrivalTypeHidden").val() === "halal") return;

            var batchNum = $input.val().trim();
            if (!batchNum) {
                _clearBatchFields();
                _clearAutoFields();
                return;
            }

            var arrivalId = $("#ArrivalId").val() || 0;

            $.ajax({
                url: arrivalGetBatchInfoUrl,
                type: "GET",
                data: { batchNum: batchNum, arrivalId: arrivalId },

                success: function (res) {
                    if (!res.success) {
                        showToast(res.message, "error");
                        _clearBatchFields();
                        _clearAutoFields();
                        return;
                    }

                    showToast("سری ساخت تایید شد", "success");
                    $("#QuantityHidden").val(res.quantity);

                    if (res.hasProdShamsi) {
                        var p = res.productionDateShamsi.split("/");
                        $("#ProdYear").val(p[0]);
                        $("#ProdMonth").val(p[1]);
                        $("#ProdDay").val(p[2]);
                        $("#ProductionDate").val(res.productionDateShamsi);
                    }
                    if (res.hasProdMiladi) {
                        var pm = res.productionDateMiladi.split("/");
                        $("#ProdYearM").val(pm[0]);
                        $("#ProdMonthM").val(pm[1]);
                        $("#ProdDayM").val(pm[2]);
                    }
                    _applyDateDisplay("prod", res.hasProdShamsi, res.hasProdMiladi);

                    if (res.hasExpiry) {
                        if (res.hasExpShamsi) {
                            var e = res.expiryDateShamsi.replace(/-/g, "/").split("/");
                            $("#ExpYear").val(e[0]);
                            $("#ExpMonth").val(e[1]);
                            $("#ExpDay").val(e[2]);
                            $("#ExpiryDate").val(res.expiryDateShamsi);
                        }
                        if (res.hasExpMiladi) {
                            var em = res.expiryDateMiladi.replace(/-/g, "/").split("/");
                            $("#ExpYearM").val(em[0]);
                            $("#ExpMonthM").val(em[1]);
                            $("#ExpDayM").val(em[2]);
                        }
                        _applyDateDisplay("exp", res.hasExpShamsi, res.hasExpMiladi);
                    } else {
                        _applyDateDisplay("exp", false, false);
                    }

                    _setAutoFields(res.itemName, res.itemId, res.doseName, res.dosePerVialId);
                },

                error: function () {
                    showToast("خطا در ارتباط با سرور، لطفاً دوباره تلاش کنید", "error");
                    _clearBatchFields();
                    _clearAutoFields();
                }
            });
        }, BATCH_DEBOUNCE_MS);
    });

    // ── دکمه تغییر فرمت تاریخ ──
    $(document).on("click", ".btn-date-toggle", function () {
        var target = $(this).data("target");
        var $shamsiWrap = $("#" + target + "-shamsi-wrap");
        var $miladiWrap = $("#" + target + "-miladi-wrap");
        var showingS = $shamsiWrap.is(":visible");

        $shamsiWrap.toggle(!showingS);
        $miladiWrap.toggle(showingS);
        $(this).attr("title", showingS ? "نمایش شمسی" : "نمایش میلادی");
    });

});