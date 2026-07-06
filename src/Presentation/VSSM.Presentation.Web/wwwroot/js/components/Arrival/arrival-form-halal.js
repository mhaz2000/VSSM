/**
 * arrival-form-halal.js  —  منطق تب‌بندی و فیلدهای حلال
 */

$(function () {

    var _currentTab = $("#ArrivalTypeHidden").val() || "vaccine";
    var _halalItemsLoaded = false;
    var _calcLock = false;

    // ── اجرای تب اولیه ──
    _applyTab(_currentTab);

    // ── رویداد کلیک روی تب‌ها ──
    $(document).on("click", ".arrival-tab[data-tab]", function () {
        var tab = $(this).data("tab");
        $("#ArrivalTypeHidden").val(tab);
        _applyTab(tab);
    });

    window.applyArrivalTab = function (tab) { _applyTab(tab); };

    // ═══════════════════════════════════════════════════════
    //  تابع اصلی تغییر تب
    // ═══════════════════════════════════════════════════════
    function _applyTab(tab) {
        _currentTab = tab;
        var isHalal = (tab === "halal");

        // ── پاک‌سازی فیلدها ──
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
        _disableVaccineValidation();

        $("#ItemClassDisplay").val("حلال");
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

        // ★★★ مقداردهی مجدد دیت‌پیکر ★★★
        setTimeout(function () {
            _initDatepickers();
        }, 200);
    }

    // ═══════════════════════════════════════════════════════
    //  تب واکسن
    // ═══════════════════════════════════════════════════════
    function _applyVaccineTab() {
        _enableVaccineValidation();

        $("#ItemClassDisplay").val("واکسن");
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

        if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
            if (typeof _setAutoFields === "function") {
                _setAutoFields(arrivalExistItemName, arrivalExistItemId,
                    arrivalExistDoseName, arrivalExistDoseId);
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    //  اعتبارسنجی
    // ═══════════════════════════════════════════════════════
    function _disableVaccineValidation() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

    function _enableVaccineValidation() {
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

    // ═══════════════════════════════════════════════════════
    //  دیت‌پیکر شمسی برای تاریخ‌های حلال + تاریخ ورودی
    // ═══════════════════════════════════════════════════════
    function _initDatepickers() {
        var $inputs = $("#HalalProdDate, #HalalExpDate, #Date");

        if ($inputs.length === 0) return;

        // حذف نمونه‌های قبلی
        $inputs.each(function () {
            if (this._flatpickr) {
                this._flatpickr.destroy();
            }
        });

        // مقداردهی جدید
        flatpickr($inputs, {
            locale: 'fa',
            dateFormat: 'Y/m/d',
            altInput: true,
            altFormat: 'Y/m/d',
            allowInput: true,
            onChange: function (selectedDates, dateStr, instance) {
                $(instance.input).css('color', '#000000');
            },
            onOpen: function (selectedDates, dateStr, instance) {
                if (instance.input.value) {
                    $(instance.input).css('color', '#000000');
                }
            }
        });

        // اگر مقدار دارند، رنگ مشکی شود
        $inputs.each(function () {
            if ($(this).val()) {
                $(this).css('color', '#000000');
            }
        });
    }

    // ── اجرای دیت‌پیکر در بارگذاری صفحه ──
    setTimeout(_initDatepickers, 100);
    $(document).ready(function () {
        _initDatepickers();
    });

});