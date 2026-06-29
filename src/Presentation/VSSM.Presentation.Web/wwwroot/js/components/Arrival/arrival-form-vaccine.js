/**
 * arrival-form-vaccine.js  —  منطق اختصاصی فرم ورودی واکسن
 * ═══════════════════════════════════════════════════════════
 * وابستگی‌ها (باید قبل از این فایل لود شده باشند):
 *   - jQuery
 *   - toast.js        (showToast)
 *   - shamsi-date.js  (splitDate, combineDate, initDateAutoAdvance, initDateValidation)
 *   - vvm-cascade.js  (initVvmCascade)
 *   - submit-btn.js   (SubmitBtn)
 *
 * نکته: این فایل فقط منطق واکسن و submit مشترک را دارد.
 *       منطق تب‌بندی و حلال در arrival-form-halal.js است.
 * ═══════════════════════════════════════════════════════════
 */

$(function () {

    // ════════════════════════════════════════
    //  Submit handler — مشترک بین واکسن و حلال
    // ════════════════════════════════════════

    var _submitting = false;

    $("#arrivalForm").on("submit", function () {
        if (_submitting) return false;  // جلوگیری از double-submit
        _submitting = true;

        var isHalal = ($("#ArrivalTypeHidden").val() === "halal");

        if (!isHalal) {
            // واکسن: hidden date fields را پیش از submit پر کن
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

        // reset پس از submit (مهم: برای هر دو حالت)
        setTimeout(function () { _submitting = false; }, 1500);

        var $active = $(document.activeElement);
        if ($active.hasClass("js-submit-btn")) {
            SubmitBtn.startLoading($active[0]);
        }

        return true;
    });

    // ════════════════════════════════════════
    //  Toast سرور
    // ════════════════════════════════════════

    if (typeof arrivalServerMsg !== "undefined" && arrivalServerMsg) {
        showToast(arrivalServerMsg, arrivalServerType || "info");
    }

    // ════════════════════════════════════════
    //  VVM cascade
    // ════════════════════════════════════════

    initVvmCascade("VvmTypeId", "VvmStageId");

    // ════════════════════════════════════════
    //  تاریخ ورود (واکسن + حلال مشترک)
    // ════════════════════════════════════════

    initDateAutoAdvance("DateDay", "DateMonth", "DateYear");
    initDateValidation("DateDay", "DateMonth", "DateYear");

    $("#DateDay, #DateMonth, #DateYear").on("input", function () {
        combineDate("DateDay", "DateMonth", "DateYear", "Date");
    });

    splitDate($("#Date").val(),           "DateDay",  "DateMonth",  "DateYear");
    splitDate($("#ProductionDate").val(), "ProdDay",  "ProdMonth",  "ProdYear");
    splitDate($("#ExpiryDate").val(),     "ExpDay",   "ExpMonth",   "ExpYear");

    // ════════════════════════════════════════
    //  مقداردهی اولیه فیلدهای auto (ویرایش)
    // ════════════════════════════════════════

    if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
        _setAutoFields(arrivalExistItemName, arrivalExistItemId,
                       arrivalExistDoseName, arrivalExistDoseId);
    } else {
        $("#ItemClassDisplay").val("واکسن");
    }

    // ════════════════════════════════════════
    //  helpers فیلدهای auto
    // ════════════════════════════════════════

    function _setAutoFields(itemName, itemId, doseName, dosePerVialId) {
        $("#ItemClassDisplay").val("واکسن");
        $("#ItemDisplay").val(itemName || "");
        $("#DoseDisplay").val(doseName || "");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val(itemId || "");
        $("#DosePerVialIdHidden").val(dosePerVialId || "");
    }

    function _clearAutoFields() {
        $("#ItemClassDisplay").val("");
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
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
    }

    // ════════════════════════════════════════
    //  helper نمایش تاریخ (شمسی/میلادی)
    // ════════════════════════════════════════

    function _applyDateDisplay(target, hasShamsi, hasMiladi) {
        var $toggle     = $(".btn-date-toggle[data-target='" + target + "']");
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
    }

    // ════════════════════════════════════════
    //  GetBatchInfo — جستجوی سری ساخت
    // ════════════════════════════════════════

    var _batchTimer = null;

    $("#BatchNum").on("input", function () {
        clearTimeout(_batchTimer);
        var $input = $(this);

        _batchTimer = setTimeout(function () {
            if (_submitting) return;

            var batchNum  = $input.val().trim();
            if (!batchNum) {
                _clearBatchFields();
                _clearAutoFields();
                return;
            }

            var arrivalId = $("#ArrivalId").val() || 0;

            $.ajax({
                url:  arrivalGetBatchInfoUrl,
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

    // ════════════════════════════════════════
    //  Toggle شمسی / میلادی
    // ════════════════════════════════════════

    $(document).on("click", ".btn-date-toggle", function () {
        var target      = $(this).data("target");
        var $shamsiWrap = $("#" + target + "-shamsi-wrap");
        var $miladiWrap = $("#" + target + "-miladi-wrap");
        var showingS    = $shamsiWrap.is(":visible");

        $shamsiWrap.toggle(!showingS);
        $miladiWrap.toggle(showingS);
        $(this).attr("title", showingS ? "نمایش شمسی" : "نمایش میلادی");
    });

});
