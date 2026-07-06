/**
 * arrival-form-vaccine.js  —  منطق اختصاصی فرم ورودی واکسن
 */

$(function () {

    var _submitting = false;

    $("#arrivalForm").on("submit", function () {
        if (_submitting) return false;
        _submitting = true;

        var isHalal = ($("#ArrivalTypeHidden").val() === "halal");

        if (!isHalal) {
            combineDate("DateDay", "DateMonth", "DateYear", "Date");
            if ($("#ProdYear").val()) {
                $("#ProductionDate").val(
                    $("#ProdYear").val() + "/" + $("#ProdMonth").val() + "/" + $("#ProdDay").val()
                );
            }
            if ($("#ExpYear").val()) {
                $("#ExpiryDate").val(
                    $("#ExpYear").val() + "/" + $("#ExpMonth").val() + "/" + $("#ExpDay").val()
                );
            }
        }

        setTimeout(function () { _submitting = false; }, 1500);

        var $active = $(document.activeElement);
        if ($active.hasClass("js-submit-btn")) {
            $("#commandHidden").val($active.val());
            SubmitBtn.startLoading($active[0]);
        }

        return true;
    });

    // توابع کمکی برای پاک‌سازی کامل (برای استفاده در حلال)
    window.clearVaccineFields = function () {
        // پاک کردن فیلدهای خودکار واکسن
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
        $("#QuantityHidden").val("");
        // پاک کردن تاریخ‌ها
        $("#ProdDay, #ProdMonth, #ProdYear").val("");
        $("#ProdDayM, #ProdMonthM, #ProdYearM").val("");
        $("#ExpDay, #ExpMonth, #ExpYear").val("");
        $("#ExpDayM, #ExpMonthM, #ExpYearM").val("");
        $("#ProductionDate").val("");
        $("#ExpiryDate").val("");
        // مخفی کردن دکمه‌های تغییر فرمت
        $(".btn-date-toggle").hide();
        // نمایش شمسی و مخفی میلادی
        $("#prod-shamsi-wrap, #exp-shamsi-wrap").show();
        $("#prod-miladi-wrap, #exp-miladi-wrap").hide();
        // پاک کردن سری ساخت (اختیاری)
        $("#BatchNum").val("");
    };

    window.clearHalalFields = function () {
        $("#HalalItemSelect").val("").trigger("change");
        $("#HalalDoseSelect").empty().append('<option value="">انتخاب کنید...</option>').prop("disabled", true);
        $("#QuantityInput, #NumVialsInput").val("");
        $("#HalalProdDate, #HalalExpDate").val("");
        $("#HalalDoseValue").val("");
    };

    // بقیه کدهای قبلی (VVM, تاریخ ورود, GetBatchInfo, و ...) بدون تغییر
    initVvmCascade("VvmTypeId", "VvmStageId");
    initDateAutoAdvance("DateDay", "DateMonth", "DateYear");
    initDateValidation("DateDay", "DateMonth", "DateYear");

    $("#DateDay, #DateMonth, #DateYear").on("input", function () {
        combineDate("DateDay", "DateMonth", "DateYear", "Date");
    });

    splitDate($("#Date").val(), "DateDay", "DateMonth", "DateYear");
    splitDate($("#ProductionDate").val(), "ProdDay", "ProdMonth", "ProdYear");
    splitDate($("#ExpiryDate").val(), "ExpDay", "ExpMonth", "ExpYear");

    if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
        _setAutoFields(arrivalExistItemName, arrivalExistItemId,
            arrivalExistDoseName, arrivalExistDoseId);
    } else {
        $("#ItemClassDisplay").val("واکسن");
    }

    function _setAutoFields(itemName, itemId, doseName, dosePerVialId) {
        $("#ItemClassDisplay").val("واکسن");
        $("#ItemDisplay").val(itemName || "");
        $("#DoseDisplay").val(doseName || "");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val(itemId || "");
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
        $("#ProdDay, #ProdMonth, #ProdYear").val("");
        $("#ProductionDate").val("");
        $("#ExpDay, #ExpMonth, #ExpYear").val("");
        $("#ExpiryDate").val("");
        $("#ProdDayM, #ProdMonthM, #ProdYearM").val("");
        $("#ExpDayM, #ExpMonthM, #ExpYearM").val("");
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

    var _batchTimer = null;

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
        }, 600);
    });

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