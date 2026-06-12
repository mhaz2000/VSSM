/**
 * dynamic-grid.js
 * مدیریت مرتب‌سازی داینامیک ستون‌های _DynamicGrid
 *
 * نحوه کار:
 *   هر دکمه [data-dg-sort] کلیک می‌شود →
 *   پارامترهای sort و direction در URL آپدیت می‌شوند →
 *   صفحه reload می‌شود
 */

(function () {
    'use strict';

    // ── راه‌اندازی پس از بارگذاری DOM ─────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        initSortButtons();
    });

    /**
     * پیدا کردن و bind کردن همه دکمه‌های sort
     */
    function initSortButtons() {
        const buttons = document.querySelectorAll('[data-dg-sort]');
        buttons.forEach(function (btn) {
            btn.addEventListener('click', function () {
                onSortClick(btn);
            });
        });
    }

    /**
     * @param {HTMLElement} btn  – دکمه‌ای که کلیک شده
     */
    function onSortClick(btn) {
        const field = btn.dataset.dgSort;
        const currentSort = btn.dataset.dgCurrentSort;
        const currentDir = btn.dataset.dgCurrentDir;

        // اگر همین ستون قبلاً انتخاب شده بود، جهت را برعکس کن؛ وگرنه desc
        const newDir = (field === currentSort && currentDir === 'asc') ? 'desc' : 'asc';

        const url = buildUrl({ sort: field, direction: newDir });
        window.location.href = url;
    }

    /**
     * URL فعلی را می‌گیرد و پارامترهای مشخص را جایگزین می‌کند
     * @param {Object} params  – { sort, direction }
     * @returns {string}
     */
    function buildUrl(params) {
        const sp = new URLSearchParams(window.location.search);

        // pageNumber را به ۱ برمی‌گردانیم تا با sort جدید از اول ببینیم
        sp.set('pageNumber', '1');

        Object.entries(params).forEach(function ([key, value]) {
            sp.set(key, value);
        });

        return window.location.pathname + '?' + sp.toString();
    }

})();