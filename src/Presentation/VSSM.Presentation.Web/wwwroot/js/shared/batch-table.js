/**
 * batch-table.js  —  جدول سری ساخت با pagination (shared)
 * ═══════════════════════════════════════════
 * قابل استفاده در: Dispatch و هر فرمی با انتخاب سری ساخت
 *
 * پیش‌نیاز:
 *   - jQuery
 *   - lines-table.css
 *
 * توابع عمومی:
 *   initBatchTable(config)
 *   showMoreRows()
 *
 * config = {
 *   tbodyId      : 'itemDetailsTbody',   // id tbody جدول
 *   showMoreWrap : 'showMoreWrap',        // id wrapper دکمه نمایش بیشتر
 *   pageSize     : 5,                     // تعداد رکورد در هر صفحه (پیش‌فرض ۵)
 *   emptyText    : 'کالایی انتخاب نشده', // متن سطر خالی
 *   colCount     : 8,                     // تعداد ستون‌ها (برای colspan)
 * }
 * ═══════════════════════════════════════════
 */

var _batchAllRows = [];
var _batchVisible = 5;
var _batchConfig = {};

/**
 * مقداردهی اولیه ماژول
 * @param {object} config
 */
function initBatchTable(config) {
    _batchConfig = $.extend({
        tbodyId: 'itemDetailsTbody',
        showMoreWrap: 'showMoreWrap',
        pageSize: 5,
        emptyText: 'کالایی انتخاب نشده',
        colCount: 8,
    }, config);
    _batchVisible = _batchConfig.pageSize;
}

/**
 * لود و رندر داده‌های جدید
 * @param {Array} rows - آرایه سری ساخت‌ها از سرور
 */
function loadBatchRows(rows) {
    _batchAllRows = rows;
    _batchVisible = _batchConfig.pageSize || 5;
    renderBatchTable();
}

/**
 * رندر جدول بر اساس تعداد visible
 */
function renderBatchTable() {
    var cfg = _batchConfig;
    var tbody = $('#' + cfg.tbodyId);
    var toShow = _batchAllRows.slice(0, _batchVisible);
    var hasMore = _batchAllRows.length > _batchVisible;

    if (_batchAllRows.length === 0) {
        tbody.html(
            '<tr><td colspan="' + cfg.colCount + '" ' +
            'style="padding:24px;color:#9ca3af;text-align:center;">' +
            cfg.emptyText + '</td></tr>'
        );
        $('#' + cfg.showMoreWrap).hide();
        return;
    }

    var html = '';
    $.each(toShow, function (_, r) {
        html +=
            '<tr>' +
            '<td><input type="checkbox" class="batch-chk" value="' + r.itemDetailStoreId + '" ' +
            'data-row=\'' + JSON.stringify(r).replace(/'/g, "&#39;") + '\'></td>' +
            '<td>' + (r.itemName || '-') + '</td>' +
            '<td>' + (r.expiryDateShamsi || '-') + '</td>' +
            '<td>' + (r.manufacturerName || '-') + '</td>' +
            '<td class="stock-cell-' + r.itemDetailStoreId + '">' + (r.currentStock || 0) + '</td>' +
            '<td>' + (r.dosePerVial || '-') + '</td>' +
            '<td>' + (r.storeName || '-') + '</td>' +
            '<td>' + (r.batchNumber || '-') + '</td>' +
            '</tr>';
    });

    tbody.html(html);
    $('#' + cfg.showMoreWrap).toggle(hasMore);
}

/**
 * نمایش ۵ رکورد بیشتر
 */
function showMoreRows() {
    _batchVisible += (_batchConfig.pageSize || 5);
    renderBatchTable();
}