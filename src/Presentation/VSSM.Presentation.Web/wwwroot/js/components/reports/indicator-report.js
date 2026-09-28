$(function () {
    "use strict";

    const form = $('#indicatorReportForm');
    if (!form.length) return;

    const searchBtn = form.find('#searchBtn');
    const itemClassSelect = $('#itemClassId');
    const itemSelect = $('#itemId');
    const batchSelect = $('#batchNumber');

    /* ─────────────────────────────────────────
       Helpers — با تکیه بر توابع سراسری موجود
       در searchable-controls.js
    ───────────────────────────────────────── */
    const setDisabled = (id, disabled) => {
        if (typeof setSearchableSelectDisabled === 'function') {
            setSearchableSelectDisabled(id, disabled);
        } else {
            // Fallback: disabled بصری
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
        const hasBatch = !!batchSelect.val();

        // دکمه تنها زمانی فعال می‌شود که هر سه مقدار انتخاب شده باشد
        searchBtn.prop('disabled', !(hasItemClass && hasItem && hasBatch));
    };

    /* ─────────────────────────────────────────
       حالت اولیه
    ───────────────────────────────────────── */
    setDisabled('itemId', true);
    setDisabled('batchNumber', true);
    setOptions('itemId', []);
    setOptions('batchNumber', []);
    toggleSearchButton();

    /* ─────────────────────────────────────────
       تغییر نوع کالا
    ───────────────────────────────────────── */
    itemClassSelect.on('change', function () {
        const classId = $(this).val();

        // reset زنجیره
        setDisabled('itemId', true);
        setDisabled('batchNumber', true);
        resetSelect('itemId');
        resetSelect('batchNumber');
        setOptions('itemId', []);
        setOptions('batchNumber', []);
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
       تغییر کالا
    ───────────────────────────────────────── */
    itemSelect.on('change', function () {
        const itemId = $(this).val();

        setDisabled('batchNumber', true);
        resetSelect('batchNumber');
        setOptions('batchNumber', []);
        toggleSearchButton();

        if (!itemId) return;

        setDisabled('batchNumber', false);

        $.ajax({
            url: '/GetItemBatchNumbers/' + itemId,
            type: 'GET',
            success: function (res) {
                if (!res || res.success !== true) return;

                const batches = (res.result || []).map(b => ({
                    value: b.batchNumber,
                    text: b.batchNumber + ' (مقدار موجود: ' + b.currentStock + ')'
                }));

                setOptions('batchNumber', batches);
            },
            error: function () {
                setOptions('batchNumber', []);
            }
        });
    });

    /* ─────────────────────────────────────────
       تغییر سری ساخت → فقط toggle دکمه
    ───────────────────────────────────────── */
    batchSelect.on('change', toggleSearchButton);

    /* ─────────────────────────────────────────
       پاک کردن فیلترها
    ───────────────────────────────────────── */
    form.on('click', '.js-clear-filters', function (e) {
        e.preventDefault();

        resetSelect('itemClassId');
        resetSelect('itemId');
        resetSelect('batchNumber');

        setOptions('itemId', []);
        setOptions('batchNumber', []);

        setDisabled('itemId', true);
        setDisabled('batchNumber', true);

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