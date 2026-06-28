/**
 * arrival-form-halal.js
 * وابستگی‌ها: jQuery، toast.js، arrival-form.js
 * این فایل را در: wwwroot/js/components/arrivals/arrival-form-halal.js قرار دهید
 */
$(function () {

    // ── وضعیت اولیه از hidden field که سرور مقدارش را ست کرده ──
    var currentTab = $("#ArrivalTypeHidden").val() || "vaccine";
    _applyTab(currentTab);

    // ── کلیک روی تب‌های اصلی پروژه (arrival-tab / data-tab) ──
    $(document).on("click", ".arrival-tab[data-tab]", function () {
        var tab = $(this).data("tab");
        $("#ArrivalTypeHidden").val(tab);
        _applyTab(tab);
    });

    window.applyArrivalTab = function (tab) { _applyTab(tab); };

    function _setValidationIgnore(ignoreAll) {
        var validator = $("#arrivalForm").data("validator");
        if (validator) {
            validator.settings.ignore = ignoreAll ? "*" : ":hidden";
        }
    }

    // ════════════════════════════════════════════════
    //  _applyTab — نمایش/مخفی بخش‌ها
    // ════════════════════════════════════════════════
    function _applyTab(tab) {
        currentTab = tab;
        var isHalal = (tab === "halal");

        // ── active روی تب درست ──
        $(".arrival-tab").removeClass("active");
        $(".arrival-tab[data-tab='" + tab + "']").addClass("active");

        if (isHalal) {
            // validation رو غیرفعال کن تا فیلدهای واکسن بلاک نکنن
            _setValidationIgnore(true);

            // نوع کالا
            $("#ItemClassDisplay").val("حلال");

            // سری ساخت: مخفی
            $("#batchNumGroup").hide();
            $("#BatchNum").val("").removeAttr("required");

            // کالا: text مخفی، select نمایش
            $("#ItemDisplay").hide().prop("disabled", true);
            $("#HalalItemSelect").show().prop("disabled", false);
            $("#itemAutoLabel").text("(انتخاب)");

            // دوز: text مخفی
            $("#DoseDisplay").hide();
            $("#doseAutoLabel").text("(وارد کنید)");

            // فیلدهای تعداد حلال: نمایش
            $("#halalQtyFields").show();

            // تاریخ‌ها
            $("#vaccineDateSection").hide();
            $("#halalDateSection").show();

            // reset hidden fields
            $("#ItemIdHidden, #DosePerVialIdHidden, #ItemClassIdHidden").val("");

            // بارگذاری آیتم‌های حلال (فقط اولین بار)
            if (!_halalItemsLoaded) _loadHalalItems();

        } else {
            // validation رو برگردون
            _setValidationIgnore(false);

            // نوع کالا
            $("#ItemClassDisplay").val("واکسن");

            // سری ساخت: نمایش
            $("#batchNumGroup").show();

            // کالا: text نمایش، select مخفی
            $("#ItemDisplay").show().prop("disabled", true);
            $("#HalalItemSelect").hide().prop("disabled", true).val("");
            $("#itemAutoLabel").text("(خودکار)");

            // دوز: text نمایش، select مخفی
            $("#DoseDisplay").show().prop("disabled", true);
            $("#doseAutoLabel").text("(خودکار)");

            // فیلدهای تعداد حلال: مخفی
            $("#halalQtyFields").hide();
            $("#DosePerVialInput, #QuantityInput, #NumVialsInput").val("");

            // تاریخ‌ها
            $("#vaccineDateSection").show();
            $("#halalDateSection").hide();
            $("#HalalProdDate, #HalalExpDate").val("");
        }
    }

    // ════════════════════════════════════════════════
    //  Halal item cascade
    // ════════════════════════════════════════════════
    var _halalItemsLoaded = false;

    function _loadHalalItems() {
        $.getJSON(arrivalGetHalalItemsUrl, function (data) {
            _halalItemsLoaded = true;
            var $sel = $("#HalalItemSelect").empty()
                .append('<option value="">انتخاب کنید...</option>');
            $.each(data, function (_, item) {
                $sel.append($("<option>").val(item.id).text(item.text));
            });
        }).fail(function () {
            showToast("خطا در بارگذاری کالاها", "error");
        });
    }

    $("#HalalItemSelect").on("change", function () {
        $("#ItemIdHidden").val($(this).val());
        $("#DosePerVialInput, #QuantityInput, #NumVialsInput").val("");
    });

    // ════════════════════════════════════════════════
    //  Bidirectional calculation (Qty ↔ NumVials)
    // ════════════════════════════════════════════════
    var _calcLock = false;

    function parsePos(val) {
        var n = parseFloat(val);
        return (!isNaN(n) && isFinite(n) && n > 0) ? n : null;
    }

    function fmt(n) { return parseFloat(n.toFixed(2)).toString(); }

    $("#QuantityInput").on("input", function () {
        if (_calcLock) return;
        var qty = parsePos($(this).val()), dose = parsePos($("#DosePerVialInput").val());
        if (qty && dose) { _calcLock = true; $("#NumVialsInput").val(fmt(qty / dose)); _calcLock = false; }
    });

    $("#NumVialsInput").on("input", function () {
        if (_calcLock) return;
        var vials = parsePos($(this).val()), dose = parsePos($("#DosePerVialInput").val());
        if (vials && dose) { _calcLock = true; $("#QuantityInput").val(Math.round(vials * dose)); _calcLock = false; }
    });

    $("#DosePerVialInput").on("input", function () {
        if (_calcLock) return;
        var qty = parsePos($("#QuantityInput").val()), dose = parsePos($(this).val());
        if (qty && dose) { _calcLock = true; $("#NumVialsInput").val(fmt(qty / dose)); _calcLock = false; }
    });

});