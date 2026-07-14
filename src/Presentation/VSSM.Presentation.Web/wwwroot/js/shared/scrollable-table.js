(function () {
    'use strict';

    function initToggleButtons() {
        document.querySelectorAll('.btn-show-all-lines').forEach(btn => {
            btn.removeEventListener('click', handleToggle);
            btn.addEventListener('click', handleToggle);
        });
    }

    function handleToggle(e) {
        const btn = e.currentTarget;
        const tableWrap = btn.closest('.lines-table-wrap');
        if (!tableWrap) return;

        const extraLines = tableWrap.querySelectorAll('.hidden-line');
        const icon = btn.querySelector('i');
        const text = btn.querySelector('.btn-text');
        const totalItems = text.textContent.match(/\d+/)?.[0] || '0';

        const isExpanded = btn.dataset.expanded === 'true';

        if (isExpanded) {
            extraLines.forEach(line => {
                line.classList.add('hidden-line');
                line.style.display = 'none';
            });
            btn.dataset.expanded = 'false';
            if (icon) icon.className = 'fa fa-chevron-down';
            if (text) text.textContent = `مشاهده همه اقلام (${totalItems} قلم)`;
            if (tableWrap) tableWrap.scrollTop = 0;
        } else {
            extraLines.forEach(line => {
                line.classList.remove('hidden-line');
                line.style.removeProperty('display');
            });
            btn.dataset.expanded = 'true';
            if (icon) icon.className = 'fa fa-chevron-up';
            if (text) text.textContent = 'مشاهده کمتر';
            setTimeout(() => { if (tableWrap) tableWrap.scrollTop = tableWrap.scrollHeight; }, 100);
        }
    }

    function initialize() {
        initToggleButtons();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    window.VSSM = window.VSSM || {};
    window.VSSM.ScrollableTable = {
        refresh: initToggleButtons
    };
})();