/**
 * arrival-form-halal.js  —  منطق تب‌بندی و فیلدهای حلال
 * ════════════════════════════════════════════════════════
 * وابستگی‌ها: jQuery، toast.js، arrival-form-vaccine.js
 *
 * تفاوت واکسن vs حلال:
 *   واکسن:  BatchNum → fetch → کالا + دوز + تاریخ‌ها + تعداد (همه خودکار)
 *   حلال:   BatchNum دستی (اختیاری، fetch نمیشه)
 *             کالا: dropdown دستی
 *             دوز بر ویال: dropdown که با انتخاب کالا cascade میشه
 *             تاریخ تولید/انقضا: دستی
 *             تعداد/ویال: دستی
 * ════════════════════════════════════════════════════════
 */

$(function () {

    // ── وضعیت اولیه از hidden field که سرور مقدارش را set کرده ──
    var _currentTab = $("#ArrivalTypeHidden").val() || "vaccine";
    _applyTab(_currentTab);

    // ── کلیک روی تب‌ها ──
    $(document).on("click", ".arrival-tab[data-tab]", function () {
        var tab = $(this).data("tab");
        $("#ArrivalTypeHidden").val(tab);
        _applyTab(tab);
    });

    window.applyArrivalTab = function (tab) { _applyTab(tab); };

    // ════════════════════════════════════════════════
    //  _applyTab — نمایش/مخفی بخش‌ها
    // ════════════════════════════════════════════════

    function _applyTab(tab) {
        _currentTab = tab;
        var isHalal = (tab === "halal");

        $(".arrival-tab").removeClass("active");
        $(".arrival-tab[data-tab='" + tab + "']").addClass("active");

        if (isHalal) {
            _disableVaccineValidation();

            $("#ItemClassDisplay").val("حلال");

            // سری ساخت: نمایش ولی دستی (fetch نمیشه)
            $("#batchNumGroup").show();
            // label تغییر میکنه تا کاربر بدونه fetch نمیشه
            $("#batchNumLabel").text("سری ساخت");
            $("#batchNumOpt").text("(اختیاری)").show();

            // کالا: text مخفی، select نمایش
            $("#ItemDisplay").hide().prop("disabled", true);
            $("#HalalItemSelect").show().prop("disabled", false);
            $("#ItemIdHidden").prop("disabled", true);
            $("#itemAutoLabel").text("(انتخاب)");

            // دوز: display مخفی، select حلال نمایش
            $("#doseDisplayGroup").hide();
            $("#halalDoseGroup").show();
            // DosePerVialIdHidden واکسن را disable کن
            $("#DosePerVialIdHidden").prop("disabled", true);

            // فیلدهای تعداد حلال
            $("#halalQtyFields").show();
            $("#QuantityHidden").prop("disabled", true);

            // تاریخ‌ها
            $("#vaccineDateSection").hide();
            $("#halalDateSection").show();

            // hidden fields واکسن را خالی کن
            $("#ItemClassIdHidden, #ItemIdHidden, #DosePerVialIdHidden").val("");

            // بارگذاری آیتم‌های حلال (فقط اولین بار)
            if (!_halalItemsLoaded) _loadHalalItems();

        } else {
            _enableVaccineValidation();

            $("#ItemClassDisplay").val("واکسن");

            // سری ساخت: نمایش + fetch (حالت عادی واکسن)
            $("#batchNumGroup").show();
            $("#batchNumLabel").text("سری ساخت");
            $("#batchNumOpt").text("(اجباری)").show();

            // کالا: text نمایش، select مخفی
            $("#ItemDisplay").show().prop("disabled", true);
            $("#HalalItemSelect").hide().prop("disabled", true).val("");
            $("#ItemIdHidden").prop("disabled", false);
            $("#itemAutoLabel").text("(خودکار)");

            // دوز: display نمایش، select حلال مخفی
            $("#doseDisplayGroup").show();
            $("#doseAutoLabel").text("(خودکار)");
            $("#halalDoseGroup").hide();
            $("#HalalDoseSelect").prop("disabled", true).val("");
            $("#DosePerVialIdHidden").prop("disabled", false);

            // فیلدهای تعداد
            $("#QuantityHidden").prop("disabled", false);
            $("#halalQtyFields").hide();
            $("#QuantityInput, #NumVialsInput").val("");

            // تاریخ‌ها
            $("#vaccineDateSection").show();
            $("#halalDateSection").hide();
            $("#HalalProdDate, #HalalExpDate").val("");
        }
    }

    // ════════════════════════════════════════════════
    //  Validation ignore
    // ════════════════════════════════════════════════

    function _disableVaccineValidation() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

    function _enableVaccineValidation() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

    // ════════════════════════════════════════════════
    //  Halal item list (lazy load)
    // ════════════════════════════════════════════════

    var _halalItemsLoaded = false;

    function _loadHalalItems() {
        $.getJSON(arrivalGetHalalItemsUrl, function (data) {
            _halalItemsLoaded = true;
            var $sel = $("#HalalItemSelect")
                .empty()
                .append('<option value="">انتخاب کنید...</option>');
            $.each(data, function (_, item) {
                $sel.append($("<option>").val(item.id).text(item.text));
            });

            // اگه edit mode است مقدار قبلی را restore کن
            if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
                $("#HalalItemSelect").val(arrivalExistItemId);
                // بارگذاری دوزها برای آیتم موجود
                _loadHalalDoses(arrivalExistItemId, function () {
                    if (typeof arrivalExistDoseId !== "undefined" && arrivalExistDoseId) {
                        $("#HalalDoseSelect").val(arrivalExistDoseId);
                    }
                });
            }
        }).fail(function () {
            showToast("خطا در بارگذاری کالاها", "error");
        });
    }

    // ════════════════════════════════════════════════
    //  Cascade دوز بر ویال — با انتخاب کالا
    // ════════════════════════════════════════════════

    $("#HalalItemSelect").on("change", function () {
        var itemId = $(this).val();

        // reset دوز
        $("#HalalDoseSelect")
            .empty()
            .append('<option value="">ابتدا کالا را انتخاب کنید</option>')
            .prop("disabled", true);

        $("#NumVialsInput, #QuantityInput").val("");

        if (!itemId) return;

        _loadHalalDoses(itemId, null);
    });

    function _loadHalalDoses(itemId, callback) {
        $.getJSON(arrivalGetHalalDosesUrl, { itemId: itemId }, function (data) {
            var $sel = $("#HalalDoseSelect")
                .empty()
                .prop("disabled", false);

            if (!data || data.length === 0) {
                $sel.append('<option value="">دوزی تعریف نشده</option>');
                if (callback) callback();
                return;
            }

            $sel.append('<option value="">انتخاب دوز...</option>');
            $.each(data, function (_, d) {
                $sel.append($("<option>").val(d.value).text(d.text));
            });

            // اگه فقط یک دوز هست auto-select کن
            if (data.length === 1) {
                $sel.val(data[0].value);
                $("#HalalDoseValue").val(data[0].text); 

            }

            if (callback) callback();
        }).fail(function () {
            showToast("خطا در بارگذاری دوزها", "error");
        });
    }

    // ════════════════════════════════════════════════
    //  محاسبه دوطرفه: Qty ↔ NumVials (با دوز از select)
    // ════════════════════════════════════════════════

    var _calcLock = false;

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
    });

    // وقتی دوز تغییر کرد NumVials را recalculate کن
    $("#HalalDoseSelect").on("change", function () {
         var dose = _getDoseValue();
    $("#HalalDoseValue").val(dose || "");
        if (_calcLock) return;
        var qty = _parsePos($("#QuantityInput").val());
        var dose = _getDoseValue();
        if (qty && dose) {
            _calcLock = true;
            $("#NumVialsInput").val(_fmt(qty / dose));
            _calcLock = false;
        }
    });

});