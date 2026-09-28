$(function () {
    "use strict";

    const form = $('#physicalInventoryForm');
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
        searchBtn.prop('disabled', !hasItemClass);
    };


    setDisabled('itemId', true);
    setOptions('itemId', []);
    toggleSearchButton();

    itemClassSelect.on('change', function () {
        const classId = $(this).val();

        // reset item
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

    /* ─────────────────────────────────────────
       پاک کردن فیلترها
    ───────────────────────────────────────── */
    form.on('click', '.js-clear-filters', function (e) {
        e.preventDefault();

        if (window.SearchableControls) {
            form.find('.ss-wrapper').each(function () {
                const id = $(this).find('input[type="hidden"]').first().attr('id');
                if (id) window.SearchableControls.reset(id);
            });
        }

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