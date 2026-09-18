$(() => {
    "use strict";

    const form = $('#entryExitReportForm');
    if (!form.length) return;

    const clearButton = form.find('.js-clear-filters');

    const initDatePickers = () => {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') return;

        form.find('.datepicker-input').each(function () {
            const $input = $(this);
            if ($input.hasClass('datepicker-initialized')) return;

            DatePicker.attach($input, 'jalali');
            $input.addClass('datepicker-initialized');
        });
    };

    const clearFilters = () => {
        form[0].reset();

        form.find('input[type="hidden"]').not('[name="__RequestVerificationToken"]').val('');
        form.find('input[type="text"]').val('');

        form.find('.ms-wrapper').each(function () {
            const $wrapper = $(this);
            $wrapper.find('.ms-tags').html('<span class="ms-placeholder">انتخاب کنید</span>');
            $wrapper.find('.ms-option').removeClass('selected');
            $wrapper.find('.ms-option input[type="checkbox"]').prop('checked', false);
            $wrapper.find('input[type="hidden"]').val('').trigger('change');
        });

        form.find('.ss-wrapper').each(function () {
            const $wrapper = $(this);
            $wrapper.find('.ss-text').text($wrapper.find('.ss-option').first().text() || 'انتخاب کنید');
            $wrapper.find('input[type="hidden"]').val('').trigger('change');
        });

        showToast('فیلترها پاک شدند', 'success');
    };

    initDatePickers();

    clearButton.on('click', (e) => {
        e.preventDefault();
        clearFilters();
    });

    form.on('submit', (e) => {
        const $btn = $(e.originalEvent?.submitter);
        const reportType = $btn.attr('value') || $btn.data('report-type');

        if (!reportType) {
            e.preventDefault();
            showToast('لطفاً نوع گزارش را انتخاب کنید', 'warning');
            return false;
        }

        return true;
    });
});