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

    var isSubmitting = false;

    $("#arrivalForm").on("submit", function () {
        if ($("#ArrivalTypeHidden").val() === "halal") {
            isSubmitting = true;
            return true;
        }
        isSubmitting = true;
        setTimeout(function () { isSubmitting = false; }, 500);

        // ذخیره تاریخ ورود در hidden قبل از submit
        combineDate("DateDay", "DateMonth", "DateYear", "Date");

        // ذخیره تاریخ تولید در hidden (برای postback)
        if ($("#ProdYear").val()) {
            $("#ProductionDate").val(
                $("#ProdYear").val() + "/" + $("#ProdMonth").val() + "/" + $("#ProdDay").val()
            );
        }

        // ذخیره تاریخ انقضا در hidden (برای postback)
        if ($("#ExpYear").val()) {
            $("#ExpiryDate").val(
                $("#ExpYear").val() + "/" + $("#ExpMonth").val() + "/" + $("#ExpDay").val()
            );
        }

        var $active = $(document.activeElement);
        if ($active.hasClass('js-submit-btn')) {
            SubmitBtn.startLoading($active[0]);
        }
    });

    if (typeof arrivalServerMsg !== "undefined" && arrivalServerMsg) {
        showToast(arrivalServerMsg, arrivalServerType || "info");
    }


    initVvmCascade("VvmTypeId", "VvmStageId");

    initDateAutoAdvance("DateDay", "DateMonth", "DateYear");
    initDateValidation("DateDay", "DateMonth", "DateYear");

    $("#DateDay, #DateMonth, #DateYear").on("input", function () {
        combineDate("DateDay", "DateMonth", "DateYear", "Date");
    });

    splitDate($("#Date").val(), "DateDay", "DateMonth", "DateYear");
    splitDate($("#ProductionDate").val(), "ProdDay", "ProdMonth", "ProdYear");
    splitDate($("#ExpiryDate").val(), "ExpDay", "ExpMonth", "ExpYear");


    function setAutoFields(itemName, itemId, doseName, dosePerVialId) {
        $("#ItemClassDisplay").val("واکسن");
        $("#ItemDisplay").val(itemName || "");
        $("#DoseDisplay").val(doseName || "");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val(itemId || "");
        $("#DosePerVialIdHidden").val(dosePerVialId || "");
    }

    function clearAutoFields() {
        $("#ItemClassDisplay").val("");
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemClassIdHidden").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
    }

    function clearBatchFields() {
        $("#QuantityHidden").val("");
        $("#ProdDay, #ProdMonth, #ProdYear").val("");
        $("#ProductionDate").val("");
        $("#ExpDay, #ExpMonth, #ExpYear").val("");
        $("#ExpiryDate").val("");
        $("#ProdDayM, #ProdMonthM, #ProdYearM").val("");
        $("#ExpDayM, #ExpMonthM, #ExpYearM").val("");
        // reset نمایش به حالت اولیه
        $("#prod-shamsi-wrap, #exp-shamsi-wrap").show();
        $("#prod-miladi-wrap, #exp-miladi-wrap").hide();
        $(".btn-date-toggle").hide();
    }

    // ── helpers نمایش تاریخ ──

    function applyDateDisplay(target, hasShamsi, hasMiladi) {
        var $toggle = $(".btn-date-toggle[data-target='" + target + "']");
        var $shamsiWrap = $("#" + target + "-shamsi-wrap");
        var $miladiWrap = $("#" + target + "-miladi-wrap");

        if (hasShamsi && hasMiladi) {
            // هر دو موجودند — دکمه toggle نشان داده می‌شود، پیش‌فرض شمسی
            $toggle.show().attr("title", "نمایش میلادی");
            $shamsiWrap.show();
            $miladiWrap.hide();
        } else if (hasMiladi) {
            // فقط میلادی
            $toggle.hide();
            $shamsiWrap.hide();
            $miladiWrap.show();
        } else if (hasShamsi) {
            // فقط شمسی
            $toggle.hide();
            $shamsiWrap.show();
            $miladiWrap.hide();
        } else {
            // هیچکدام (مثلاً انقضا ندارد)
            $toggle.hide();
            $shamsiWrap.hide();
            $miladiWrap.hide();
        }
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

    if (typeof arrivalItemName !== "undefined") $("#ItemDisplay").val(arrivalItemName);
    if (typeof arrivalDoseName !== "undefined") $("#DoseDisplay").val(arrivalDoseName);

    // ════════════════════════════════════════
    //  ۵. GetBatchInfo — بررسی سری ساخت
    // ════════════════════════════════════════

    var batchTimer = null;

    $("#BatchNum").on("input", function () {
        clearTimeout(batchTimer);
        var $input = $(this);

        batchTimer = setTimeout(function () {
            if (isSubmitting) return;
            var batchNum = $input.val().trim();
            if (!batchNum) {
                clearBatchFields();
                clearAutoFields();
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
                        clearBatchFields();
                        clearAutoFields();
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
                    applyDateDisplay("prod", res.hasProdShamsi, res.hasProdMiladi);

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
                        applyDateDisplay("exp", res.hasExpShamsi, res.hasExpMiladi);
                    } else {
                        applyDateDisplay("exp", false, false);
                    }

                    console.log("expiryDateMiladi:", res.expiryDateMiladi);
                    console.log("split result:", res.expiryDateMiladi.replace(/-/g, "/").split("/"));

                    setAutoFields(res.itemName, res.itemId, res.doseName, res.dosePerVialId);
                },
                error: function () {
                    showToast("خطا در ارتباط با سرور، لطفاً دوباره تلاش کنید", "error");
                    clearBatchFields();
                    clearAutoFields();
                }
            });
        }, 600);
    });

    // ════════════════════════════════════════
    //  ۶. toggle شمسی / میلادی
    // ════════════════════════════════════════

    $(document).on("click", ".btn-date-toggle", function () {
        var target = $(this).data("target");
        var $shamsiWrap = $("#" + target + "-shamsi-wrap");
        var $miladiWrap = $("#" + target + "-miladi-wrap");
        var isShowing = $shamsiWrap.is(":visible");

        $shamsiWrap.toggle(!isShowing);
        $miladiWrap.toggle(isShowing);
        $(this).attr("title", isShowing ? "نمایش شمسی" : "نمایش میلادی");
    });

    // ════════════════════════════════════════
    //  ۷. دکمه‌های footer (ثبت / پیش‌نویس)
    // ════════════════════════════════════════

    $(".js-footer-btn").on("click", function () {
        var command = $(this).val();

        var $v = $("#arrivalForm").data("validator");
        if ($v) $v.settings.ignore = "*";

        $("#arrivalForm").find("input[name='command']").remove();
        $("<input>").attr({ type: "hidden", name: "command", value: command })
            .appendTo("#arrivalForm");

        SubmitBtn.startLoading(this);   // ← اضافه کن اینجا


        $("#arrivalForm").submit();
    });

});