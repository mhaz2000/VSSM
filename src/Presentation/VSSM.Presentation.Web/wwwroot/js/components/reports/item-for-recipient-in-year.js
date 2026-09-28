$(function () {
    "use strict";

    const form = $('#itemForRecipientInYearForm');
    if (!form.length) return;

    const searchBtn = form.find('#searchBtn');
    const itemSelect = $('#itemId');
    const yearSelect = $('#year');

    const toggleSearchButton = () => {
        const hasItem = !!itemSelect.val();
        const hasYear = !!yearSelect.val();
        searchBtn.prop('disabled', !(hasItem && hasYear));
    };

    toggleSearchButton();

    itemSelect.on('change', toggleSearchButton);
    yearSelect.on('change', toggleSearchButton);

    form.on('click', '.js-clear-filters', function (e) {
        e.preventDefault();

        if (window.SearchableControls) {
            form.find('.ss-wrapper').each(function () {
                const id = $(this).find('input[type="hidden"]').first().attr('id');
                if (id) window.SearchableControls.reset(id);
            });
        }

        toggleSearchButton();

        if (typeof showToast === 'function') {
            showToast('فیلترها پاک شدند', 'success');
        }
    });


    form.on('submit', function () {
        const $btn = form.find('.btn-primary-action');
        const original = $btn.html();

        $btn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i> در حال تهیه گزارش...');

        setTimeout(() => $btn.prop('disabled', false).html(original), 2500);
    });
});