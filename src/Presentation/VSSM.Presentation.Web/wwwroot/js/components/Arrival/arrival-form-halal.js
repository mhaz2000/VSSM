/**
 * arrival-form-halal.js  —  منطق تب‌بندی و فیلدهای حلال
 */

$(function () {

    var _currentTab = $("#ArrivalTypeHidden").val() || "vaccine";
    _applyTab(_currentTab);

    $(document).on("click", ".arrival-tab[data-tab]", function () {
        var tab = $(this).data("tab");
        $("#ArrivalTypeHidden").val(tab);
        _applyTab(tab);
    });

    window.applyArrivalTab = function (tab) { _applyTab(tab); };

    function _applyTab(tab) {
        _currentTab = tab;
        var isHalal = (tab === "halal");

        // ── پاک‌سازی کامل فیلدهای هر دو تب ──
        if (typeof window.clearVaccineFields === "function") {
            window.clearVaccineFields();
        }
        if (typeof window.clearHalalFields === "function") {
            window.clearHalalFields();
        }

        // ── نمایش/مخفی کردن ──
        $(".arrival-tab").removeClass("active");
        $(".arrival-tab[data-tab='" + tab + "']").addClass("active");

        if (isHalal) {
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

        } else {
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

            // مقداردهی مجدد فیلدهای واکسن اگر در حالت ویرایش باشیم
            if (typeof arrivalExistItemId !== "undefined" && arrivalExistItemId) {
                // تابع _setAutoFields در vaccine.js تعریف شده
                if (typeof _setAutoFields === "function") {
                    _setAutoFields(arrivalExistItemName, arrivalExistItemId,
                        arrivalExistDoseName, arrivalExistDoseId);
                }
            }
        }

        // به‌روزرسانی دکمه‌ها
        if (window.updateAddButtonState) window.updateAddButtonState();
        if (window.updateFooterButtonsState) window.updateFooterButtonsState();
    }

    function _disableVaccineValidation() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

    function _enableVaccineValidation() {
        var v = $("#arrivalForm").data("validator");
        if (v) v.settings.ignore = ":hidden";
    }

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
        if (window.updateAddButtonState) window.updateAddButtonState();
    });

});