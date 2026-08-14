$(function () {
    "use strict";

    var form = $('#availableQuantityForm');
    if (!form.length) return;

    var searchButtons = form.find('.btn-confirm');
    var itemClassSelect = $('#itemClassId');
    var itemMultiSelect = $('#itemId');       // hidden input
    var storeMultiSelect = $('#storeId');
    var batchNumberInput = $('#batchNumber');
    var fromDateInput = $('#fromDate');
    var toDateInput = $('#toDate');
    var fromDateEnInput = $('#fromDateEn');
    var toDateEnInput = $('#toDateEn');
    var allSubRecipientStocks = $('#allSubRecipientStocks');

    var itemsRequest = null;
    var lastRequestedClassId = null;

    // =========================================================
    // توابع مشابه dispatch-form.js
    // =========================================================

    /**
     * پر کردن گزینه‌های MultiSelect
     */
    function populateMultiSelect(selectId, items, placeholder) {
        placeholder = placeholder || 'انتخاب کنید';
        var $hidden = $('#' + selectId);
        if (!$hidden.length) return;

        var $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;

        var $optionsContainer = $wrapper.find('.ms-options');
        var $tags = $wrapper.find('.ms-tags');

        if (!$optionsContainer.length || !$tags.length) return;

        // Reset
        $hidden.val('');
        $tags.html('<span class="ms-placeholder">' + escapeHtml(placeholder) + '</span>');
        $optionsContainer.empty();

        if (!items || items.length === 0) {
            $hidden.trigger('change');
            return;
        }

        // "انتخاب همه"
        var selectAll = $('<div class="ms-option ms-select-all" data-value="__all__">' +
            '<input type="checkbox" class="ms-option-checkbox" />' +
            '<span>انتخاب همه</span>' +
            '</div>');
        $optionsContainer.append(selectAll);

        // گزینه‌ها
        $.each(items, function (_, item) {
            var option = $('<div class="ms-option" data-value="' + escapeHtml(item.value) + '">' +
                '<input type="checkbox" class="ms-option-checkbox" />' +
                '<span>' + escapeHtml(item.text) + '</span>' +
                '</div>');
            $optionsContainer.append(option);
        });

        $hidden.trigger('change');
    }

    /**
     * خالی کردن گزینه‌های MultiSelect
     */
    function clearMultiSelect(selectId) {
        populateMultiSelect(selectId, [], 'انتخاب کالا');
        setMultiSelectDisabled(selectId, true);
    }

    /**
     * فعال/غیرفعال کردن MultiSelect
     */
    function setMultiSelectDisabled(selectId, disabled) {
        var $hidden = $('#' + selectId);
        if (!$hidden.length) return;
        var $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;
        var $dropdown = $wrapper.find('.ms-dropdown');

        $hidden.prop('disabled', disabled);
        $wrapper.toggleClass('ms-disabled', disabled);
        if (disabled) {
            $wrapper.removeClass('open');
            $dropdown.hide();
        } else {
            $dropdown.show();
        }
    }

    /**
     * حالت بارگذاری MultiSelect
     */
    function setMultiSelectLoading(selectId, loading) {
        var $hidden = $('#' + selectId);
        if (!$hidden.length) return;
        var $wrapper = $hidden.closest('.ms-wrapper');
        if (!$wrapper.length) return;
        $wrapper.toggleClass('ms-loading', loading);
    }

    // =========================================================
    // مقداردهی اولیه
    // =========================================================

    setMultiSelectDisabled('itemId', true);
    updateSearchButtons();
    initializeDatePickers();

    // =========================================================
    // رویداد تغییر نوع کالا
    // =========================================================

    itemClassSelect.on('change', function () {
        var classId = normalizeValue($(this).val());
        updateSearchButtons();
        abortItemsRequest();

        if (!classId) {
            lastRequestedClassId = null;
            clearMultiSelect('itemId');
            setMultiSelectDisabled('itemId', true);
            setMultiSelectLoading('itemId', false);
            updateSearchButtons();
            return;
        }

        if (lastRequestedClassId === classId) return;
        lastRequestedClassId = classId;
        loadItemsByClass(classId);
    });

    // =========================================================
    // بارگذاری کالاها بر اساس نوع کالا
    // =========================================================

    function loadItemsByClass(classId) {
        var url = '/CurrentStockReport/GetItemsByClass?classItemId=' + encodeURIComponent(classId);

        setMultiSelectDisabled('itemId', true);
        setMultiSelectLoading('itemId', true);
        clearMultiSelect('itemId');

        itemsRequest = $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (response) {
                var currentClassId = normalizeValue(itemClassSelect.val());
                if (currentClassId !== classId) return;

                if (!response || response.success !== true) {
                    clearMultiSelect('itemId');
                    showWarningToast(response?.message || 'دریافت کالاها با خطا مواجه شد.');
                    return;
                }

                var result = Array.isArray(response.result) ? response.result : [];
                if (result.length === 0) {
                    clearMultiSelect('itemId');
                    showWarningToast('برای نوع کالای انتخاب‌شده، کالایی یافت نشد.');
                    return;
                }

                var items = result.map(function (item) {
                    return { value: item.itemId, text: item.fullName };
                });

                populateMultiSelect('itemId', items, 'انتخاب کالا');
                setMultiSelectDisabled('itemId', false);
            },
            error: function (xhr, status) {
                if (status === 'abort') return;
                clearMultiSelect('itemId');
                showErrorToast('خطا در دریافت کالاها.');
            },
            complete: function () {
                itemsRequest = null;
                setMultiSelectLoading('itemId', false);
                updateSearchButtons();
            }
        });
    }

    function abortItemsRequest() {
        if (itemsRequest) {
            itemsRequest.abort();
            itemsRequest = null;
        }
    }

    // =========================================================
    // رویدادهای تغییر سایر فیلترها
    // =========================================================

    itemMultiSelect.on('change', updateSearchButtons);
    storeMultiSelect.on('change', updateSearchButtons);

    form.on('change', 'select, input[type="checkbox"]', updateSearchButtons);
    form.on('input', '#batchNumber, #fromDate, #toDate', updateSearchButtons);

    fromDateEnInput.on('change input', updateSearchButtons);
    toDateEnInput.on('change input', updateSearchButtons);

    allSubRecipientStocks.on('change', updateSearchButtons);

    // =========================================================
    // ارسال فرم
    // =========================================================

    form.on('submit', function (event) {
        if (!hasAnyFilter()) {
            event.preventDefault();
            updateSearchButtons();
            return false;
        }
        return true;
    });

    // =========================================================
    // توابع کمکی (بدون تغییر)
    // =========================================================

    function initializeDatePickers() {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') return;
        if (fromDateInput.length && !fromDateInput.hasClass('datepicker-initialized')) {
            DatePicker.attach(fromDateInput, 'jalali');
            fromDateInput.addClass('datepicker-initialized');
        }
        if (toDateInput.length && !toDateInput.hasClass('datepicker-initialized')) {
            DatePicker.attach(toDateInput, 'jalali');
            toDateInput.addClass('datepicker-initialized');
        }
    }

    function updateSearchButtons() {
        if (!searchButtons.length) return;
        searchButtons.prop('disabled', !hasAnyFilter());
    }

    function hasAnyFilter() {
        var itemClassValue = getValue('#itemClassId');
        var itemValue = getMultiSelectValue('itemId');
        var storeValue = getMultiSelectValue('storeId');
        var recipientValue = getValue('#recipientId');
        var levelValue = getValue('#level');
        var batchNumber = $.trim(batchNumberInput.val() || '');
        var sourceValue = getValue('#sourceId');
        var manufacturerValue = getValue('#manufacturerId');
        var vvmTypeValue = getValue('#vvmtypeId');
        var vvmStageValue = getValue('#vvmstageId');
        var commercialNameValue = getValue('#commercialNameId');
        var fromDate = $.trim(fromDateEnInput.val() || fromDateInput.val() || '');
        var toDate = $.trim(toDateEnInput.val() || toDateInput.val() || '');
        var allSubStores = allSubRecipientStocks.is(':checked');

        return hasValue(itemClassValue) ||
            hasValue(itemValue) ||
            hasValue(storeValue) ||
            hasValue(recipientValue) ||
            hasValue(levelValue) ||
            hasValue(batchNumber) ||
            hasValue(sourceValue) ||
            hasValue(manufacturerValue) ||
            hasValue(vvmTypeValue) ||
            hasValue(vvmStageValue) ||
            hasValue(commercialNameValue) ||
            hasValue(fromDate) ||
            hasValue(toDate) ||
            allSubStores;
    }

    function getValue(selector) {
        var element = $(selector);
        return element.length ? element.val() : null;
    }

    function getMultiSelectValue(selectId) {
        var element = $('#' + selectId);
        if (!element.length) return null;
        var value = element.val();

        if (Array.isArray(value)) {
            var filtered = value.filter(function (item) {
                return item != null && item !== '' && item !== '__all__';
            });
            return filtered.length ? filtered : null;
        }

        if (value != null && value !== '') {
            var values = String(value)
                .split(',')
                .map(function (item) { return item.trim(); })
                .filter(function (item) { return item !== '' && item !== '__all__'; });
            return values.length ? values : null;
        }
        return null;
    }

    function normalizeValue(value) {
        if (value == null) return null;
        var normalized = String(value).trim();
        return normalized === '' ? null : normalized;
    }

    function hasValue(value) {
        if (value == null) return false;
        if (Array.isArray(value)) return value.length > 0;
        return String(value).trim() !== '';
    }

    function showWarningToast(message) {
        if (typeof showToast === 'function') showToast(message, 'warning');
    }

    function showErrorToast(message) {
        if (typeof showToast === 'function') showToast(message, 'error');
    }

    function escapeHtml(value) {
        if (value == null) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
});