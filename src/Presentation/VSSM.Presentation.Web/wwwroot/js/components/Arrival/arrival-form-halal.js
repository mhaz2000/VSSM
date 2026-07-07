/**
 * arrival-form-halal.js  —  منطق تب‌بندی و فیلدهای حلال
 * (دیت‌پیکر در فایل جداگانه arrival-datepicker-halal.js است)
 * (ریفکتور شده: بدون تغییر در بیزینس لاجیک، فقط تمیزکاری و افزودن گارد خطا)
 */

$(function () {

    "use strict";

    var _currentTab = $("#ArrivalTypeHidden").val() || "vaccine";
    var _halalItemsLoaded = false;
    var _calcLock = false;

    // ═══════════════════════════════════════════════════════
    //  کمکی: اجرای امن یک تابع
    // ═══════════════════════════════════════════════════════

    function _safe(fn, label) {
        try {
            fn();
        } catch (err) {
            if (window.console && console.error) {
                console.error("[arrival-form-halal] خطا در " + (label || "اجرا") + ":", err);
            }
        }
    }

    _safe(function () { _applyTab(_currentTab); }, "applyTab اولیه");

    $(document).on("click", ".arrival-tab[data-tab]", function () {
        var tab = $(this).data("tab");
        $("#ArrivalTypeHidden").val(tab);
        _safe(function () { _applyTab(tab); }, "applyTab کلیک");
    });

    window.applyArrivalTab = function (tab) { _safe(function () { _applyTab(tab); }, "applyArrivalTab"); };

    // ═══════════════════════════════════════════════════════
    //  تابع اصلی تغییر تب
    // ═══════════════════════════════════════════════════════
    function _applyTab(tab) {
        _currentTab = tab;
        var isHalal = (tab === "halal");

        // ★★★ پاک‌سازی کامل قبل از هر تغییر ★★★
        if (typeof window.clearVaccineFields === "function") {
            window.clearVaccineFields();
        }
        if (typeof window.clearHalalFields === "function") {
            window.clearHalalFields();
        }

        // ── تغییر کلاس تب ──
        $(".arrival-tab").removeClass("active");
        $(".arrival-tab[data-tab='" + tab + "']").addClass("active");

        if (isHalal) {
            _applyHalalTab();
        } else {
            _applyVaccineTab();
        }

        // ── به‌روزرسانی دکمه‌ها ──
        if (window.updateAddButtonState) window.updateAddButtonState();
        if (window.updateFooterButtonsState) window.updateFooterButtonsState();
    }

    // ═══════════════════════════════════════════════════════
    //  تب حلال
    // ═══════════════════════════════════════════════════════
    function _applyHalalTab() {
        _setValidationIgnoreHidden();

        // ★★★ پاک‌سازی مستقیم فیلدهای حلال برای اطمینان ★★★
        $("#HalalItemSelect").val("").trigger("change");
        $("#ItemIdHidden").val("");
        $("#HalalDoseSelect")
            .empty()
            .append('<option value="">ابتدا کالا را انتخاب کنید</option>')
            .prop("disabled", true);
        $("#HalalDoseValue").val("");
        $("#DosePerVialIdHidden").val("");
        $("#QuantityInput").val("");
        $("#NumVialsInput").val("");
        $("#QuantityHidden").val("");
        $("#HalalProdDate").val("").removeClass("has-value");
        $("#HalalExpDate").val("").removeClass("has-value");
        $("#ItemClassDisplay").val("حلال");

        // ── نمایش/مخفی کردن ──
        $("#batchNumGroup").show();
        $("#batchNumLabel").text("سری ساخت");
        $("#batchNumOpt").text("(اختیاری)").show();

        $("#ItemDisplay").hide().prop("disabled", true);
        $("#HalalItemSelect").show().prop("disabled", false);
        $("#ItemIdHidden").prop("disabled", true);
        $("#itemAutoLabel").text("(انتخاب)");

        $("#doseDisplayGroup").hide();
        $("#halalDoseGroup").show();
        $("#DosePerVialIdHidden").prop("disabled", true);

        $("#halalQtyFields").show();
        $("#QuantityHidden").prop("disabled", true);

        $("#vaccineDateSection").hide();
        $("#halalDateSection").show();

        $("#ItemClassIdHidden, #ItemIdHidden, #DosePerVialIdHidden").val("");

        if (!_halalItemsLoaded) _loadHalalItems();

        // دیت‌پیکر در فایل جداگانه مقداردهی می‌شود
    }

    // ═══════════════════════════════════════════════════════
    //  تب واکسن
    // ═══════════════════════════════════════════════════════
    function _applyVaccineTab() {
        _setValidationIgnoreHidden();

        // ★★★ پاک‌سازی مستقیم فیلدهای واکسن برای اطمینان ★★★
        $("#ItemDisplay").val("");
        $("#DoseDisplay").val("");
        $("#ItemIdHidden").val("");
        $("#DosePerVialIdHidden").val("");
        $("#QuantityHidden").val("");
        $("#BatchNum").val("");
        $("#ProdDay, #ProdMonth, #ProdYear").val("");
        $("#ExpDay, #ExpMonth, #ExpYear").val("");
        $("#ProductionDate").val("");
        $("#ExpiryDate").val("");
        $("#ProductionDate, #ExpiryDate").removeClass("has-value");
        $("#ItemClassDisplay").val("واکسن");

        // ── نمایش/مخفی کردن ──
        $("#batchNumGroup").show();
        $("#batchNumLabel").text("سری ساخت");
        $("#batchNumOpt").text("(اجباری)").show();

        $("#ItemDisplay").show().prop("disabled", true);
        $("#HalalItemSelect").hide().prop("disabled", true).val("");
        $("#ItemIdHidden").prop("disabled", false);
        $("#itemAutoLabel").text("(خودکار)");

        $("#doseDisplayGroup").show();
        $("#doseAutoLabel").text("(خودکار)");
        $("#halalDoseGroup").hide();
        $("#HalalDoseSelect").prop("disabled", true).val("");
        $("#DosePerVialIdHidden").prop("disabled", false);

        $("#QuantityHidden").prop("disabled", false);
        $("#halalQtyFields").hide();

        $("#vaccineDateSection").show();
        $("#halalDateSection").hide();

        // مقداردهی مجدد فیلدهای واکسن اگر در حالت ویرایش باشیم
        // (نکته: این تابع در فایل arrival-form-vaccine.js تعریف شده و در اسکوپ محلی همان
        //  فایل قرار دارد، پس در عمل همیشه undefined است — این دقیقاً همان رفتار نسخه
        //  اصلی است و برای عدم تغییر بیزینس‌لاجیک، دست‌نخورده نگه داشته شده)
        if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
            if (typeof _setAutoFields === "function") {
                _setAutoFields(arrivalExistItemName, arrivalExistItemId,
                    arrivalExistDoseName, arrivalExistDoseId);
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    //  اعتبارسنجی
    //  (هر دو حالت قبلاً منطق یکسانی داشتند: نادیده‌گرفتن فیلدهای مخفی)
    // ═══════════════════════════════════════════════════════
    function _setValidationIgnoreHidden() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

    // ═══════════════════════════════════════════════════════
    //  بارگذاری آیتم‌های حلال
    // ═══════════════════════════════════════════════════════
    function _loadHalalItems() {
        $.getJSON(arrivalGetHalalItemsUrl, function (data) {
            _halalItemsLoaded = true;
            var $sel = $("#HalalItemSelect")
                .empty()
                .append('<option value="">انتخاب کنید...</option>');

            $.each(data, function (_, item) {
                $sel.append($("<option>").val(item.id).text(item.text));
            });

            if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
                $("#HalalItemSelect").val(arrivalExistItemId);
                _loadHalalDoses(arrivalExistItemId, function () {
                    if (typeof arrivalExistDoseId !== "undefined" && arrivalExistDoseId) {
                        $("#HalalDoseSelect").val(arrivalExistDoseId);
                    }
                    if (window.updateAddButtonState) window.updateAddButtonState();
                });
            } else {
                if (window.updateAddButtonState) window.updateAddButtonState();
            }
        }).fail(function () {
            showToast("خطا در بارگذاری کالاها", "error");
        });
    }

    // ═══════════════════════════════════════════════════════
    //  بارگذاری دوزهای حلال
    // ═══════════════════════════════════════════════════════
    function _loadHalalDoses(itemId, callback) {
        $.getJSON(arrivalGetHalalDosesUrl, { itemId: itemId }, function (data) {
            var $sel = $("#HalalDoseSelect")
                .empty()
                .prop("disabled", false);

            if (!data || data.length === 0) {
                $sel.append('<option value="">دوزی تعریف نشده</option>');
                if (callback) callback();
                if (window.updateAddButtonState) window.updateAddButtonState();
                return;
            }

            $sel.append('<option value="">انتخاب دوز...</option>');
            $.each(data, function (_, d) {
                $sel.append($("<option>").val(d.value).text(d.text));
            });

            if (data.length === 1) {
                $sel.val(data[0].value);
                $("#HalalDoseValue").val(data[0].text);
            }

            if (callback) callback();
            if (window.updateAddButtonState) window.updateAddButtonState();
        }).fail(function () {
            showToast("خطا در بارگذاری دوزها", "error");
        });
    }

    // ═══════════════════════════════════════════════════════
    //  رویدادهای انتخاب آیتم و دوز
    // ═══════════════════════════════════════════════════════
    $("#HalalItemSelect").on("change", function () {
        var itemId = $(this).val();
        $("#HalalDoseSelect")
            .empty()
            .append('<option value="">ابتدا کالا را انتخاب کنید</option>')
            .prop("disabled", true);
        $("#NumVialsInput, #QuantityInput").val("");

        if (!itemId) {
            if (window.updateAddButtonState) window.updateAddButtonState();
            return;
        }
        _loadHalalDoses(itemId, null);
    });

    $("#HalalDoseSelect").on("change", function () {
        var dose = _getDoseValue();
        $("#HalalDoseValue").val(dose || "");
        if (_calcLock) return;

        var qty = _parsePos($("#QuantityInput").val());
        var doseVal = _getDoseValue();
        if (qty && doseVal) {
            _calcLock = true;
            $("#NumVialsInput").val(_fmt(qty / doseVal));
            _calcLock = false;
        }
        if (window.updateAddButtonState) window.updateAddButtonState();
    });

    // ═══════════════════════════════════════════════════════
    //  محاسبات تعداد و ویال
    // ═══════════════════════════════════════════════════════
    function _parsePos(val) {
        var n = parseFloat(val);
        return (!isNaN(n) && isFinite(n) && n > 0) ? n : null;
    }

    function _fmt(n) {
        return parseFloat(n.toFixed(2)).toString();
    }

    function _getDoseValue() {
        return _parsePos($("#HalalDoseSelect option:selected").text());
    }

    $("#QuantityInput").on("input", function () {
        if (_calcLock) return;
        var qty = _parsePos($(this).val());
        var dose = _getDoseValue();
        if (qty && dose) {
            _calcLock = true;
            $("#NumVialsInput").val(_fmt(qty / dose));
            _calcLock = false;
        }
        if (window.updateAddButtonState) window.updateAddButtonState();
    });

    $("#NumVialsInput").on("input", function () {
        if (_calcLock) return;
        var vials = _parsePos($(this).val());
        var dose = _getDoseValue();
        if (vials && dose) {
            _calcLock = true;
            $("#QuantityInput").val(Math.round(vials * dose));
            _calcLock = false;
        }
        if (window.updateAddButtonState) window.updateAddButtonState();
    });

});