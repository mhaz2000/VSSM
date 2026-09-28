$(function () {
    "use strict";

    const form = $('#notDispatchedReportForm');
    if (!form.length) return;

    const searchBtn = form.find('#searchBtn');
    const itemClassSelect = $('#itemClassId');
    const itemSelect = $('#itemId');


    const setDisabled = (id, disabled) => {
        if (typeof setSearchableSelectDisabled === 'function') {
            setSearchableSelectDisabled(id, disabled);
        } else {
            const wrap = $('#' + id).closest('.ss-wrapper, .f-group');
            wrap.toggleClass('is-disabled', disabled)
                .css('opacity', disabled ? .6 : 1)
                .css('pointer-events', disabled ? 'none' : '');
        }
    };

    const setOptions = (id, options) => {
        if (typeof setSearchableSelectOptions === 'function') {
            setSearchableSelectOptions(id, options || []);
        } else if (window.SearchableControls && window.SearchableControls.setOptions) {
            window.SearchableControls.setOptions(id, options || []);
        }
    };

    const resetSelect = (id) => {
        if (window.SearchableControls && typeof window.SearchableControls.reset === 'function') {
            window.SearchableControls.reset(id);
        } else {
            $('#' + id).val('');
        }
    };


    const toggleSearchButton = () => {
        const hasItemClass = !!itemClassSelect.val();
        const hasItem = !!itemSelect.val();
        searchBtn.prop('disabled', !(hasItemClass && hasItem));
    };

    const initDatePickers = () => {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') return;

        form.find('.datepicker-input').each(function () {
            const $input = $(this);
            const mode = $input.attr('data-calendar-mode') || 'jalali';
            DatePicker.attach($input, mode);
        });
    };

    const switchCalendarMode = (targetId) => {
        const $input = $('#' + targetId);
        if (!$input.length) return;

        const current = $input.attr('data-calendar-mode') || 'jalali';
        const next = current === 'jalali' ? 'gregorian' : 'jalali';

        $input.attr('data-calendar-mode', next).val('');
        $input.siblings('input[type="hidden"]').val('');

        if (typeof DatePicker !== 'undefined' && DatePicker.attach === 'function') {
            DatePicker.attach($input, next);
        }

        const $btn = form.find(`.btn-toggle-date-mode[data-target="${targetId}"]`);
        $btn.attr('data-mode', next);
        $btn.find('.mode-label').text(next === 'jalali' ? 'شمسی' : 'میلادی');
    };


    setDisabled('itemId', true);
    setOptions('itemId', []);
    toggleSearchButton();
    initDatePickers();


    itemClassSelect.on('change', function () {
        const classId = $(this).val();

        setDisabled('itemId', true);
        resetSelect('itemId');
        setOptions('itemId', []);
        toggleSearchButton();

        if (!classId) return;

        setDisabled('itemId', false);

        $.ajax({
            url: '/CurrentStockReport/GetItemsByClass/' + classId,
            type: 'GET',
            success: function (res) {
                if (!res || res.success !== true) return;

                const items = (res.result || []).map(i => ({
                    value: String(i.id),
                    text: i.fullName
                }));

                setOptions('itemId', items);
            },
            error: function () {
                setOptions('itemId', []);
            }
        });
    });


    itemSelect.on('change', toggleSearchButton);


    form.on('click', '.btn-toggle-date-mode', function (e) {
        e.preventDefault();
        switchCalendarMode($(this).data('target'));
    });


    form.on('click', '.js-clear-filters', function (e) {
        e.preventDefault();

        if (window.SearchableControls) {
            form.find('.ss-wrapper').each(function () {
                const id = $(this).find('input[type="hidden"]').first().attr('id');
                if (id) window.SearchableControls.reset(id);
            });
        }

        form.find('input.datepicker-input').val('');
        form.find('input[type="hidden"]').not('[name="__RequestVerificationToken"]').val('');

        form.find('.btn-toggle-date-mode').each(function () {
            const $btn = $(this);
            const targetId = $btn.data('target');
            $('#' + targetId).attr('data-calendar-mode', 'jalali');
            $btn.attr('data-mode', 'jalali');
            $btn.find('.mode-label').text('شمسی');
        });

        setOptions('itemId', []);
        setDisabled('itemId', true);
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