/**
 * arrival-datepicker-halal.js  —  دیت‌پیکر برای تاریخ‌های حلال با قابلیت تغییر حالت
 * (ریفکتور شده: بدون تغییر در بیزینس لاجیک، فقط تمیزکاری و افزودن گارد خطا)
 */

$(function () {

    "use strict";

    // ── وضعیت فعلی هر فیلد ──
    var halalDateModes = {
        prod: 'gregorian',
        exp: 'gregorian'
    };

    function _safe(fn, label) {
        try {
            fn();
        } catch (err) {
            if (window.console && console.error) {
                console.error("[arrival-datepicker-halal] خطا در " + (label || "اجرا") + ":", err);
            }
        }
    }

    function _isDatePickerAvailable() {
        return (typeof DatePicker !== 'undefined' && typeof DatePicker.attach === 'function');
    }

    function _attachOne($input, mode) {
        if (!$input.length) return;

        if ($input.data('datepicker-attached')) {
            $input.off('click.picker focus.picker');
            $input.removeData('datepicker-attached');
        }

        $input.prop('readonly', true).prop('disabled', false);
        DatePicker.attach($input, mode);
        $input.data('datepicker-attached', true);
    }

    // ── تابع مقداردهی اولیه یا مجدد ──
    function initHalalDatepickers(forceMode) {
        if (!_isDatePickerAvailable()) {
            console.warn('DatePicker not available');
            return;
        }

        // تاریخ تولید
        var $prod = $('#HalalProdDate');
        if ($prod.length) {
            var prodMode = forceMode || halalDateModes.prod || 'gregorian';
            _attachOne($prod, prodMode);
            updateModeLabel('prod', prodMode);
        }

        // تاریخ انقضا
        var $exp = $('#HalalExpDate');
        if ($exp.length) {
            var expMode = forceMode || halalDateModes.exp || 'gregorian';
            _attachOne($exp, expMode);
            updateModeLabel('exp', expMode);
        }
    }

    function updateModeLabel(target, mode) {
        var $btn = $('.btn-toggle-date-mode[data-target="' + target + '"]');
        if ($btn.length) {
            var label = (mode === 'gregorian') ? 'میلادی' : 'شمسی';
            $btn.find('.mode-label').text(label);
            $btn.attr('title', 'تغییر به ' + (mode === 'gregorian' ? 'شمسی' : 'میلادی'));
        }
    }

    $(document).on('click', '.btn-toggle-date-mode', function () {
        _safe(function () {
            var target = $(this).data('target');
            var currentMode = halalDateModes[target];
            var newMode = (currentMode === 'gregorian') ? 'jalali' : 'gregorian';
            halalDateModes[target] = newMode;

            var $input = (target === 'prod') ? $('#HalalProdDate') : $('#HalalExpDate');

            $input.val('').removeClass('has-value');

            initHalalDatepickers(newMode);
        }.bind(this), "تغییر حالت تاریخ");
    });

    $(document).on('click', '.arrival-tab[data-tab="halal"]', function () {
        setTimeout(function () {
            _safe(initHalalDatepickers, "initHalalDatepickers بعد از کلیک تب");
        }, 400);
    });

    if ($('#ArrivalTypeHidden').val() === 'halal') {
        setTimeout(function () {
            _safe(initHalalDatepickers, "initHalalDatepickers اولیه");
        }, 500);
    }

    $(document).ready(function () {
        if ($('#HalalProdDate').val()) {
            $('#HalalProdDate').addClass('has-value');
        }
        if ($('#HalalExpDate').val()) {
            $('#HalalExpDate').addClass('has-value');
        }
    });

});