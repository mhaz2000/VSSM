$(() => {
    "use strict";

    const form = $('#availableQuantityForm');
    if (!form.length) return;

    const searchButtons = form.find('.btn-confirm');
    const itemClassSelect = $('#itemClassId');
    const itemMultiSelect = $('#itemId');
    const storeMultiSelect = $('#storeId');
    const batchNumberInput = $('#batchNumber');
    const fromDateInput = $('#fromDate');
    const toDateInput = $('#toDate');
    const fromDateEnInput = $('#fromDateEn');
    const toDateEnInput = $('#toDateEn');
    const allSubRecipientStocks = $('#allSubRecipientStocks');

    let itemsRequest = null;
    let lastRequestedClassId = null;

    const escapeHtml = (value) => {
        if (value == null) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    };

    const showWarningToast = (message) => {
        if (typeof showToast === 'function') showToast(message, 'warning');
    };

    const showErrorToast = (message) => {
        if (typeof showToast === 'function') showToast(message, 'error');
    };

    const normalizeValue = (value) => {
        if (value == null) return null;
        const normalized = String(value).trim();
        return normalized === '' ? null : normalized;
    };

    const hasValue = (value) => {
        if (value == null) return false;
        if (Array.isArray(value)) return value.length > 0;
        return String(value).trim() !== '';
    };

    const getValue = (selector) => {
        const element = $(selector);
        return element.length ? element.val() : null;
    };

    const getMultiSelectValue = (selectId) => {
        const element = $('#' + selectId);
        if (!element.length) return null;

        const value = element.val();

        if (Array.isArray(value)) {
            const filtered = value.filter(item => item != null && item !== '' && item !== '__all__');
            return filtered.length ? filtered : null;
        }

        if (value != null && value !== '') {
            const values = String(value)
                .split(',')
                .map(item => item.trim())
                .filter(item => item !== '' && item !== '__all__');
            return values.length ? values : null;
        }

        return null;
    };

    const populateMultiSelect = (selectId, items, placeholder = 'انتخاب کنید') => {
        const $hidden = $('#' + selectId);
        if (!$hidden.length) return;

        const $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;

        const $optionsContainer = $wrapper.find('.ms-options');
        const $tags = $wrapper.find('.ms-tags');

        if (!$optionsContainer.length || !$tags.length) return;

        $hidden.val('');
        $tags.html(`<span class="ms-placeholder">${escapeHtml(placeholder)}</span>`);
        $optionsContainer.empty();

        if (!items || items.length === 0) {
            $hidden.trigger('change');
            return;
        }

        const selectAll = $(`
            <div class="ms-option ms-select-all" data-value="__all__">
                <input type="checkbox" class="ms-option-checkbox" />
                <span>انتخاب همه</span>
            </div>
        `);
        $optionsContainer.append(selectAll);

        items.forEach(item => {
            const option = $(`
                <div class="ms-option" data-value="${escapeHtml(item.value)}">
                    <input type="checkbox" class="ms-option-checkbox" />
                    <span>${escapeHtml(item.text)}</span>
                </div>
            `);
            $optionsContainer.append(option);
        });

        $hidden.trigger('change');
    };

    const setMultiSelectDisabled = (selectId, disabled) => {
        const $hidden = $('#' + selectId);
        if (!$hidden.length) return;

        const $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;

        const $control = $wrapper.find('.ms-control');
        const $dropdown = $wrapper.find('.ms-dropdown');

        $hidden.prop('disabled', disabled);
        $wrapper.toggleClass('ms-disabled', disabled);
        $dropdown.css('display', '');

        if (disabled) {
            $control.removeClass('open');
            $wrapper.removeClass('open');
        }
    };

    const setMultiSelectLoading = (selectId, loading) => {
        const $hidden = $('#' + selectId);
        if (!$hidden.length) return;

        const $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;

        $wrapper.toggleClass('ms-loading', loading);
    };

    const clearMultiSelect = (selectId) => {
        populateMultiSelect(selectId, [], 'انتخاب کالا');
        setMultiSelectDisabled(selectId, true);
    };

    const hasAnyFilter = () => {
        const itemClassValue = getValue('#itemClassId');
        const itemValue = getMultiSelectValue('itemId');
        const storeValue = getMultiSelectValue('storeId');
        const recipientValue = getValue('#recipientId');
        const levelValue = getValue('#level');
        const batchNumber = $.trim(batchNumberInput.val() || '');
        const sourceValue = getValue('#sourceId');
        const manufacturerValue = getValue('#manufacturerId');
        const vvmTypeValue = getValue('#vvmtypeId');
        const vvmStageValue = getValue('#vvmstageId');
        const commercialNameValue = getValue('#commercialNameId');
        const fromDate = $.trim(fromDateEnInput.val() || fromDateInput.val() || '');
        const toDate = $.trim(toDateEnInput.val() || toDateInput.val() || '');
        const allSubStores = allSubRecipientStocks.is(':checked');

        return hasValue(itemClassValue)
            || hasValue(itemValue)
            || hasValue(storeValue)
            || hasValue(recipientValue)
            || hasValue(levelValue)
            || hasValue(batchNumber)
            || hasValue(sourceValue)
            || hasValue(manufacturerValue)
            || hasValue(vvmTypeValue)
            || hasValue(vvmStageValue)
            || hasValue(commercialNameValue)
            || hasValue(fromDate)
            || hasValue(toDate)
            || allSubStores;
    };

    const updateSearchButtons = () => {
        if (!searchButtons.length) return;
        searchButtons.prop('disabled', !hasAnyFilter());
    };

    const initializeDatePickers = () => {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') return;

        if (fromDateInput.length && !fromDateInput.hasClass('datepicker-initialized')) {
            DatePicker.attach(fromDateInput, 'jalali');
            fromDateInput.addClass('datepicker-initialized');
        }
        if (toDateInput.length && !toDateInput.hasClass('datepicker-initialized')) {
            DatePicker.attach(toDateInput, 'jalali');
            toDateInput.addClass('datepicker-initialized');
        }
    };

    const abortItemsRequest = () => {
        if (itemsRequest) {
            itemsRequest.abort();
            itemsRequest = null;
        }
    };

    const loadItemsByClass = (classId) => {
        const url = `/CurrentStockReport/GetItemsByClass?id=${encodeURIComponent(classId)}`;

        setMultiSelectDisabled('itemId', true);
        setMultiSelectLoading('itemId', true);
        clearMultiSelect('itemId');

        itemsRequest = $.ajax({
            url,
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: (response) => {
                const currentClassId = normalizeValue(itemClassSelect.val());
                if (currentClassId !== classId) return;

                if (!response || response.success !== true) {
                    clearMultiSelect('itemId');
                    showWarningToast(response?.message || 'دریافت کالاها با خطا مواجه شد.');
                    return;
                }

                const result = Array.isArray(response.result) ? response.result : [];
                if (result.length === 0) {
                    clearMultiSelect('itemId');
                    showWarningToast('برای نوع کالای انتخاب‌شده، کالایی یافت نشد.');
                    return;
                }

                const items = result.map(item => ({ value: item.itemId, text: item.fullName }));

                populateMultiSelect('itemId', items, 'انتخاب کالا');
                setMultiSelectDisabled('itemId', false);
            },
            error: (xhr, status) => {
                if (status === 'abort') return;
                clearMultiSelect('itemId');
                showErrorToast('خطا در دریافت کالاها.');
            },
            complete: () => {
                itemsRequest = null;
                setMultiSelectLoading('itemId', false);
                updateSearchButtons();
            }
        });
    };

    setMultiSelectDisabled('itemId', true);
    updateSearchButtons();
    initializeDatePickers();

    itemClassSelect.on('change', (e) => {
        const classId = normalizeValue($(e.currentTarget).val());
        updateSearchButtons();

        if (!classId) {
            abortItemsRequest();
            lastRequestedClassId = null;
            clearMultiSelect('itemId');
            setMultiSelectDisabled('itemId', true);
            setMultiSelectLoading('itemId', false);
            updateSearchButtons();
            return;
        }

        if (lastRequestedClassId === classId) return;

        abortItemsRequest();
        lastRequestedClassId = classId;
        loadItemsByClass(classId);
    });

    itemMultiSelect.on('change', updateSearchButtons);
    storeMultiSelect.on('change', updateSearchButtons);

    form.on('change', 'select, input[type="checkbox"]', updateSearchButtons);
    form.on('input', '#batchNumber, #fromDate, #toDate', updateSearchButtons);

    fromDateEnInput.on('change input', updateSearchButtons);
    toDateEnInput.on('change input', updateSearchButtons);

    allSubRecipientStocks.on('change', updateSearchButtons);

    form.on('submit', (event) => {
        if (!hasAnyFilter()) {
            event.preventDefault();
            updateSearchButtons();
            return false;
        }
        return true;
    });
});