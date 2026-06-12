/**
 * progress-bar.js  —  کامپوننت Progress Bar چندمنظوره
 * ═══════════════════════════════════════════════════════
 *
 * استفاده:
 *   const pb = ProgressBar.create('#containerId', { label: 'در حال ثبت...' });
 *   pb.start(total);          // شروع با تعداد کل
 *   pb.update(done, total);   // آپدیت پیشرفت
 *   pb.finish(true,  'موفق'); // پایان موفق
 *   pb.finish(false, 'خطا'); // پایان ناموفق
 *   pb.destroy();             // حذف از DOM
 *
 * گزینه‌ها (options):
 *   label       — متن پیش‌فرض (پیش‌فرض: 'در حال پردازش...')
 *   showCount   — نمایش عدد done/total (پیش‌فرض: true)
 *   showEta     — نمایش زمان تخمینی (پیش‌فرض: true)
 *   showPercent — نمایش درصد روی نوار (پیش‌فرض: true)
 *   autoDestroy — حذف خودکار بعد از finish (میلی‌ثانیه، 0 = غیرفعال)
 * ═══════════════════════════════════════════════════════
 */

var ProgressBar = (function () {

    /* ── ساخت المنت ── */
    function create(selector, options) {
        options = $.extend({
            label: 'در حال پردازش...',
            showCount: true,
            showEta: true,
            showPercent: true,
            autoDestroy: 0
        }, options);

        var $wrap = $(selector);
        if (!$wrap.length) { console.warn('ProgressBar: selector not found', selector); return null; }

        var id = 'pb-' + Date.now();

        var html =
            '<div class="pb-wrap" id="' + id + '">' +
            '<div class="pb-header">' +
            '<span class="pb-label">' + options.label + '</span>' +
            (options.showCount ? '<span class="pb-count">0 / 0</span>' : '') +
            (options.showEta ? '<span class="pb-eta"></span>' : '') +
            '</div>' +
            '<div class="pb-track">' +
            '<div class="pb-fill" style="width:0%">' +
            (options.showPercent ? '<span class="pb-pct">0%</span>' : '') +
            '</div>' +
            '</div>' +
            '<div class="pb-status"></div>' +
            '</div>';

        $wrap.html(html);

        var $root = $('#' + id);
        var _start = null;

        var instance = {

            /* ── شروع ── */
            start: function (total) {
                _start = Date.now();
                $root.find('.pb-count').text('0 / ' + _fmt(total));
                $root.find('.pb-fill').css('width', '0%').removeClass('pb-fill--done pb-fill--error');
                $root.find('.pb-pct').text('0%');
                $root.find('.pb-status').text('').removeClass('pb-status--done pb-status--error');
                $root.find('.pb-eta').text('');
                $root.removeClass('pb-wrap--done pb-wrap--error').addClass('pb-wrap--running');
            },

            /* ── آپدیت ── */
            update: function (done, total) {
                if (!total) return;
                var pct = Math.min(Math.round(done / total * 100), 100);

                $root.find('.pb-fill').css('width', pct + '%');
                $root.find('.pb-pct').text(pct + '%');
                $root.find('.pb-count').text(_fmt(done) + ' / ' + _fmt(total));

                if (options.showEta && _start && done > 0) {
                    var elapsed = (Date.now() - _start) / 1000;
                    var rate = done / elapsed;           // رکورد در ثانیه
                    var remain = (total - done) / rate;
                    $root.find('.pb-eta').text('زمان باقی: ' + _etaStr(remain));
                }
            },

            /* ── پایان ── */
            finish: function (success, msg) {
                instance.update(success ? 1 : 0, 1); // 100% یا 0%
                $root.find('.pb-fill')
                    .css('width', success ? '100%' : '100%')
                    .addClass(success ? 'pb-fill--done' : 'pb-fill--error');
                $root.find('.pb-pct').text(success ? '100%' : '!');
                $root.find('.pb-eta').text('');
                $root.find('.pb-status')
                    .text(msg || (success ? 'انجام شد' : 'خطا'))
                    .addClass(success ? 'pb-status--done' : 'pb-status--error');
                $root.removeClass('pb-wrap--running')
                    .addClass(success ? 'pb-wrap--done' : 'pb-wrap--error');

                if (options.autoDestroy > 0) {
                    setTimeout(function () { instance.destroy(); }, options.autoDestroy);
                }
            },

            /* ── متن label ── */
            setLabel: function (text) {
                $root.find('.pb-label').text(text);
            },

            /* ── حذف ── */
            destroy: function () {
                $root.fadeOut(300, function () { $root.remove(); });
            },

            /* ── خواندن container ── */
            $el: $root
        };

        return instance;
    }

    /* ── فرمت عدد با جداکننده هزار ── */
    function _fmt(n) {
        return Number(n).toLocaleString('fa-IR');
    }

    /* ── تبدیل ثانیه به رشته خوانا ── */
    function _etaStr(sec) {
        if (!isFinite(sec) || sec < 0) return '';
        if (sec < 60) return Math.ceil(sec) + ' ثانیه';
        if (sec < 3600) return Math.ceil(sec / 60) + ' دقیقه';
        return (sec / 3600).toFixed(1) + ' ساعت';
    }

    return { create: create };

})();