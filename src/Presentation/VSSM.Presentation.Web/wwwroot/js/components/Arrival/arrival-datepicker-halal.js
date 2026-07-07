/**
 * arrival-datepicker-halal.js  —  دیت‌پیکر برای تاریخ‌های حلال با قابلیت تغییر حالت
 */

$(function () {

    // ── وضعیت فعلی هر فیلد ──
    var halalDateModes = {
        prod: 'gregorian',
        exp: 'gregorian'
    };

    // ── تابع مقداردهی اولیه یا مجدد ──
    function initHalalDatepickers(forceMode) {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') {
            console.warn('DatePicker not available');
            return;
        }

        // تاریخ تولید
        var $prod = $('#HalalProdDate');
        if ($prod.length) {
            var mode = forceMode || halalDateModes.prod || 'gregorian';
            if ($prod.data('datepicker-attached')) {
                $prod.off('click.picker focus.picker');
                $prod.removeData('datepicker-attached');
            }
            $prod.prop('readonly', true).prop('disabled', false);
            DatePicker.attach($prod, mode);
            $prod.data('datepicker-attached', true);

      
            if ($prod.val() && !forceMode) {

            }

            updateModeLabel('prod', mode);
        }

        var $exp = $('#HalalExpDate');
        if ($exp.length) {
            var mode = forceMode || halalDateModes.exp || 'gregorian';
            if ($exp.data('datepicker-attached')) {
                $exp.off('click.picker focus.picker');
                $exp.removeData('datepicker-attached');
            }
            $exp.prop('readonly', true).prop('disabled', false);
            DatePicker.attach($exp, mode);
            $exp.data('datepicker-attached', true);
            updateModeLabel('exp', mode);
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
        var target = $(this).data('target');
        var currentMode = halalDateModes[target];
        var newMode = (currentMode === 'gregorian') ? 'jalali' : 'gregorian';
        halalDateModes[target] = newMode;

        var $input = (target === 'prod') ? $('#HalalProdDate') : $('#HalalExpDate');

        $input.val('');
        $input.removeClass('has-value');

        initHalalDatepickers(newMode);
    });

    $(document).on('click', '.arrival-tab[data-tab="halal"]', function () {
        setTimeout(function () {
            initHalalDatepickers();
        }, 400);
    });

    if ($('#ArrivalTypeHidden').val() === 'halal') {
        setTimeout(function () {
            initHalalDatepickers();
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