/**
 * skeleton.js  —  انیمیشن skeleton جدول (کامپوننت مستقل)
 * ══════════════════════════════════════════════════════════
 * پیش‌نیاز: jQuery
 * ══════════════════════════════════════════════════════════
 */

var Skeleton = (function () {

    /**
     * نمایش ردیف‌های skeleton در tbody جدول
     * @param {string|jQuery} tbodySelector - selector یا jQuery object
     * @param {number} count - تعداد ردیف (پیش‌فرض: 4)
     * @param {number} cols - تعداد ستون‌ها (پیش‌فرض: 9)
     */
    function show(tbodySelector, count, cols) {
        count = count || 4;
        cols = cols || 9;

        var sizes = ['short', 'medium', 'long', 'full', 'medium'];
        var special = {
            // ستون آخر: input skeleton
            [cols - 1]: '<div class="skeleton-line input" style="margin:0 auto;"></div>',
            // ستون ماقبل آخر: select skeleton
            [cols - 2]: '<div class="skeleton-line select"></div>'
        };

        var html = '';
        for (var i = 0; i < count; i++) {
            html += '<tr>';
            for (var c = 0; c < cols; c++) {
                if (special[c]) {
                    html += '<td class="skeleton-cell">' + special[c] + '</td>';
                } else {
                    var sz = sizes[(i + c) % sizes.length];
                    html += '<td class="skeleton-cell"><div class="skeleton-line ' + sz + '"></div></td>';
                }
            }
            html += '</tr>';
        }

        $(tbodySelector).html(html);
    }

    return { show: show };

})();