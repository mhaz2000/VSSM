/**
 * datepicker.js  —  تقویم شمسی (کامپوننت مستقل)
 * ═══════════════════════════════════════════════════
 * پیش‌نیاز: jQuery
 * ═══════════════════════════════════════════════════
 */

var DatePicker = (function () {

    /* ── داده‌های ثابت ── */
    var MONTHS = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
        'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];
    var DAYS = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];

    /* ── تبدیل میلادی → شمسی ── */
    function toJalali(gy, gm, gd) {
        var g_d_no, j_d_no, j_np, i, j_y, j_m, j_d;
        var g_days_in_month = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        var j_days_in_month = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
        gy -= 1600; gm -= 1; gd -= 1;
        g_d_no = 365 * gy + Math.floor((gy + 3) / 4) - Math.floor((gy + 99) / 100) + Math.floor((gy + 399) / 400);
        for (i = 0; i < gm; i++) g_d_no += g_days_in_month[i];
        if (gm > 1 && ((gy + 1600) % 4 === 0 && ((gy + 1600) % 100 !== 0 || (gy + 1600) % 400 === 0))) g_d_no++;
        g_d_no += gd;
        j_d_no = g_d_no - 79;
        j_np = Math.floor(j_d_no / 12053); j_d_no %= 12053;
        j_y = 979 + 33 * j_np + 4 * Math.floor(j_d_no / 1461);
        j_d_no %= 1461;
        if (j_d_no >= 366) { j_y += Math.floor((j_d_no - 1) / 365); j_d_no = (j_d_no - 1) % 365; }
        for (i = 0; i < 11 && j_d_no >= j_days_in_month[i]; i++) j_d_no -= j_days_in_month[i];
        j_m = i + 1; j_d = j_d_no + 1;
        return [j_y, j_m, j_d];
    }

    /* ── تبدیل شمسی → میلادی ── */
    function toGregorian(jy, jm, jd) {
        var j_days_in_month = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
        jy -= 979; jm -= 1; jd -= 1;
        var j_day_no = 365 * jy + Math.floor(jy / 4) * 8 + Math.floor(((jy % 4) + 3) / 3);
        for (var i = 0; i < jm; i++) j_day_no += j_days_in_month[i];
        j_day_no += jd;
        var g_day_no = j_day_no + 79;
        var gy = 1600 + 400 * Math.floor(g_day_no / 146097); g_day_no %= 146097;
        var leap = true;
        if (g_day_no >= 36525) { g_day_no--; gy += 100 * Math.floor(g_day_no / 36524); g_day_no %= 36524; if (g_day_no >= 365) g_day_no++; else leap = false; }
        gy += 4 * Math.floor(g_day_no / 1461); g_day_no %= 1461;
        if (g_day_no >= 366) { leap = false; g_day_no--; gy += Math.floor(g_day_no / 365); g_day_no %= 365; }
        var g_days_in_month = [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        var gm;
        for (gm = 0; g_day_no >= g_days_in_month[gm]; gm++) g_day_no -= g_days_in_month[gm];
        return [gy, gm + 1, g_day_no + 1];
    }

    /* ── روز هفته اول ماه شمسی (0=شنبه) ── */
    function firstWeekday(jy, jm) {
        var g = toGregorian(jy, jm, 1);
        var dow = new Date(g[0], g[1] - 1, g[2]).getDay();
        return (dow + 1) % 7;
    }

    /* ── امروز شمسی ── */
    function todayJalali() {
        var n = new Date();
        return toJalali(n.getFullYear(), n.getMonth() + 1, n.getDate());
    }

    function pad(n) { return n < 10 ? '0' + n : '' + n; }

    /* ── وضعیت picker فعال ── */
    var _activePicker = null;

    function closePicker() {
        if (_activePicker) {
            _activePicker.$popup.remove();
            _activePicker = null;
        }
    }

    /* ── بستن با کلیک بیرون ── */
    $(document).on('mousedown.picker', function (e) {
        if (_activePicker && !$(e.target).closest('.cal-popup, .datepicker-input').length) {
            closePicker();
        }
    });

    /* ── ساخت popup ── */
    function buildPopup($input) {
        var today = todayJalali();
        var parts = ($input.val() || '').split('/');
        var cy = parts[0] ? parseInt(parts[0]) : today[0];
        var cm = parts[1] ? parseInt(parts[1]) : today[1];

        var $popup = $('<div class="cal-popup"></div>');

        function renderDays() {
            var daysInMonth = (cm <= 6) ? 31 : (cm <= 11) ? 30 : 29;
            var firstWd = firstWeekday(cy, cm);
            var selParts = ($input.val() || '').split('/');
            var selD = selParts[0] == cy && selParts[1] == pad(cm) ? parseInt(selParts[2]) : 0;
            var todayStr = today[0] + '/' + pad(today[1]) + '/' + pad(today[2]);

            var html = '<div class="cal-header">' +
                '<button class="cal-nav" id="calNext"><i class="fa fa-chevron-right"></i></button>' +
                '<span class="cal-title" id="calTitleBtn">' + MONTHS[cm - 1] + ' ' + cy + '</span>' +
                '<button class="cal-nav" id="calPrev"><i class="fa fa-chevron-left"></i></button>' +
                '</div>';
            html += '<div class="cal-days-head">';
            DAYS.forEach(function (d) { html += '<span>' + d + '</span>'; });
            html += '</div><div class="cal-days">';
            for (var e = 0; e < firstWd; e++) html += '<button class="cal-day empty"></button>';
            for (var d = 1; d <= daysInMonth; d++) {
                var ds = cy + '/' + pad(cm) + '/' + pad(d);
                var cls = 'cal-day';
                if (d === selD) cls += ' selected';
                else if (ds === todayStr) cls += ' today';
                html += '<button class="' + cls + '" data-d="' + d + '">' + d + '</button>';
            }
            html += '</div>';
            $popup.html(html);
            bindDayEvents();
        }

        function renderMonths() {
            var html = '<div class="cal-header">' +
                '<button class="cal-nav" id="calNext"><i class="fa fa-chevron-right"></i></button>' +
                '<span class="cal-title" id="calTitleBtn">' + cy + '</span>' +
                '<button class="cal-nav" id="calPrev"><i class="fa fa-chevron-left"></i></button>' +
                '</div><div class="cal-months">';
            MONTHS.forEach(function (mn, i) {
                var cls = 'cal-month-btn' + (i + 1 === cm ? ' selected' : '');
                html += '<button class="' + cls + '" data-m="' + (i + 1) + '">' + mn + '</button>';
            });
            html += '</div>';
            $popup.html(html);
            bindMonthEvents();
        }

        function bindDayEvents() {
            $popup.find('#calPrev').on('click', function () { cm--; if (cm < 1) { cm = 12; cy--; } renderDays(); });
            $popup.find('#calNext').on('click', function () { cm++; if (cm > 12) { cm = 1; cy++; } renderDays(); });
            $popup.find('#calTitleBtn').on('click', function () { renderMonths(); });
            $popup.find('.cal-day:not(.empty)').on('click', function () {
                var d = $(this).data('d');
                $input.val(cy + '/' + pad(cm) + '/' + pad(d)).trigger('change');
                closePicker();
            });
        }

        function bindMonthEvents() {
            $popup.find('#calPrev').on('click', function () { cy--; renderMonths(); });
            $popup.find('#calNext').on('click', function () { cy++; renderMonths(); });
            $popup.find('#calTitleBtn').on('click', function () { renderDays(); });
            $popup.find('.cal-month-btn').on('click', function () {
                cm = parseInt($(this).data('m'));
                renderDays();
            });
        }

        renderDays();

        // موقعیت‌دهی
        var r = $input[0].getBoundingClientRect();
        var top = r.bottom + window.scrollY + 4;
        var left = r.left + window.scrollX;
        if (left + 272 > window.innerWidth - 8) left = window.innerWidth - 280;
        $popup.css({ top: top + 'px', left: left + 'px' });

        $('body').append($popup);
        _activePicker = { $popup: $popup, $input: $input };
    }

    /* ── API عمومی ── */
    return {
        /** وصل‌کردن datepicker به یک input */
        attach: function ($input) {
            $input.on('click focus', function (e) {
                e.stopPropagation();
                if ($input.prop('disabled')) return;
                if (_activePicker && _activePicker.$input[0] === $input[0]) return;
                closePicker();
                buildPopup($input);
            });
            $input.attr('readonly', true);
        },
        close: closePicker
    };

})();