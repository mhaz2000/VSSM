$(function () {
    /* =========================================================
       ELEMENTS
       ========================================================= */
    const form = $('#availableQuantityForm');
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

    /* =========================================================
       INITIAL STATE
       ========================================================= */
    setSearchableMultiSelectDisabled('itemId', true);
    updateSearchButtons();

    /* =========================================================
       PERSIAN DATE PICKER
       ========================================================= */
    $('#fromDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#fromDateEn',
        altFormat: "YYYY-MM-DD",
        onSelect: function () { updateSearchButtons(); }
    });
    $('#toDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#toDateEn',
        altFormat: "YYYY-MM-DD",
        onSelect: function () { updateSearchButtons(); }
    });

    /* =========================================================
       ITEM CLASS CHANGE
       ========================================================= */
    itemClassSelect.on('change', function () {
        const classId = $(this).val();
        updateSearchButtons();
        if (!classId) {
            setSearchableMultiSelectDisabled('itemId', true);
            setSearchableMultiSelectOptions('itemId', []);
            updateSearchButtons();
            return;
        }
        setSearchableMultiSelectDisabled('itemId', false);
        setSearchableMultiSelectLoading('itemId', true);
        $.get('/GetItemsByClass/' + classId)
            .done(function (res) {
                if (!res || !res.success) {
                    setSearchableMultiSelectOptions('itemId', []);
                    return;
                }
                const items = (res.result || []).map(function (item) {
                    return { value: item.itemId, text: item.fullName };
                });
                setSearchableMultiSelectOptions('itemId', items);
            })
            .fail(function () {
                setSearchableMultiSelectOptions('itemId', []);
            })
            .always(function () {
                setSearchableMultiSelectLoading('itemId', false);
                updateSearchButtons();
            });
    });

    /* =========================================================
       MULTI SELECT CHANGE
       ========================================================= */
    itemMultiSelect.on('change', function () { updateSearchButtons(); });
    storeMultiSelect.on('change', function () { updateSearchButtons(); });

    /* =========================================================
       STANDARD SELECTS & CHECKBOX
       ========================================================= */
    form.on('change', 'select, input[type="checkbox"]', function () { updateSearchButtons(); });

    /* =========================================================
       TEXT INPUTS
       ========================================================= */
    form.on('input', '#batchNumber, #fromDate, #toDate', function () { updateSearchButtons(); });

    /* =========================================================
       DATE HIDDEN INPUTS
       ========================================================= */
    fromDateEnInput.on('change', function () { updateSearchButtons(); });
    toDateEnInput.on('change', function () { updateSearchButtons(); });

    /* =========================================================
       ALL SUB RECIPIENT STOCKS
       ========================================================= */
    allSubRecipientStocks.on('change', function () { updateSearchButtons(); });

    /* =========================================================
       FINAL FORM SUBMIT
       ========================================================= */
    form.on('submit', function (event) {
        if (!hasAnyFilter()) {
            event.preventDefault();
            updateSearchButtons();
            return false;
        }
    });

    /* =========================================================
       UPDATE SEARCH BUTTONS
       ========================================================= */
    function updateSearchButtons() {
        searchButtons.prop('disabled', !hasAnyFilter());
    }

    /* =========================================================
       CHECK ANY FILTER
       ========================================================= */
    function hasAnyFilter() {
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

        return Boolean(
            hasValue(itemClassValue) ||
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
            allSubStores
        );
    }

    /* =========================================================
       GET NORMAL SELECT VALUE
       ========================================================= */
    function getValue(selector) {
        const element = $(selector);
        if (!element.length) return null;
        return element.val();
    }

    /* =========================================================
       GET MULTI SELECT VALUE
       ========================================================= */
    function getMultiSelectValue(selectId) {
        const element = $('#' + selectId);
        if (!element.length) return null;
        const value = element.val();
        if (Array.isArray(value)) {
            return value.filter(function (item) {
                return item !== null && item !== undefined && item !== '' && item !== '__all__';
            });
        }
        if (value !== null && value !== undefined && value !== '') {
            return value;
        }
        return null;
    }

    /* =========================================================
       CHECK VALUE
       ========================================================= */
    function hasValue(value) {
        if (value === null || value === undefined) return false;
        if (Array.isArray(value)) return value.length > 0;
        return String(value).trim() !== '';
    }
});

/* 
   DISABLE / ENABLE SEARCHABLE MULTI SELECT
    */
function setSearchableMultiSelectDisabled(selectId, disabled = true) {
    const hidden = document.getElementById(selectId);
    if (!hidden) return;
    const wrapper = hidden.closest('.ms-wrapper');
    if (!wrapper) return;
    const dropdown = wrapper.querySelector('.ms-dropdown');
    hidden.disabled = disabled;
    if (disabled) {
        wrapper.classList.add('ms-disabled');
        wrapper.classList.remove('open');
        if (dropdown) dropdown.style.display = 'none';
    } else {
        wrapper.classList.remove('ms-disabled');
    }
}

/* 
   SET MULTI SELECT OPTIONS
    */
function setSearchableMultiSelectOptions(selectId, items, placeholder = 'انتخاب کنید') {
    const hidden = document.getElementById(selectId);
    if (!hidden) return;
    const wrapper = hidden.closest('.ms-wrapper');
    if (!wrapper) return;
    const optionsContainer = wrapper.querySelector('.ms-options');
    const tags = wrapper.querySelector('.ms-tags');
    if (!optionsContainer || !tags) return;
    // Reset hidden value
    hidden.value = '';
    // Reset tags
    tags.innerHTML = `<span class="ms-placeholder">${placeholder}</span>`;
    // Clear options
    optionsContainer.innerHTML = '';
    // Normalize items
    items = Array.isArray(items) ? items : [];
    if (items.length === 0) {
        tags.innerHTML = `<span class="ms-placeholder">${placeholder}</span>`;
        $(hidden).trigger('change');
        return;
    }
    // Select All
    const selectAll = document.createElement('div');
    selectAll.className = 'ms-option ms-select-all';
    selectAll.dataset.value = '__all__';
    selectAll.innerHTML = `
        <input type="checkbox" class="ms-option-checkbox" />
        <span>انتخاب همه</span>
    `;
    optionsContainer.appendChild(selectAll);
    const hrTop = document.createElement('hr');
    hrTop.className = 'ms-option-separator';
    optionsContainer.appendChild(hrTop);
    // Options
    items.forEach(function (item, index) {
        const div = document.createElement('div');
        div.className = 'ms-option';
        div.dataset.value = item.value;
        div.innerHTML = `
            <input type="checkbox" class="ms-option-checkbox" />
            <span>${item.text}</span>
        `;
        optionsContainer.appendChild(div);
        if (index !== items.length - 1) {
            const hr = document.createElement('hr');
            hr.className = 'ms-option-separator';
            optionsContainer.appendChild(hr);
        }
    });
    $(hidden).trigger('change');
}

/* 
   MULTI SELECT LOADING STATE
    */
function setSearchableMultiSelectLoading(selectId, loading) {
    const hidden = document.getElementById(selectId);
    if (!hidden) return;
    const wrapper = hidden.closest('.ms-wrapper');
    if (!wrapper) return;
    if (loading) {
        wrapper.classList.add('ms-loading');
    } else {
        wrapper.classList.remove('ms-loading');
    }
}