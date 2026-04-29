$(function () {

    setSearchableMultiSelectDisabled('itemId', true);

    $('#fromDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#fromDateEn',
        altFormat: "YYYY-MM-DD"
    });

    $('#toDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#toDateEn',
        altFormat: "YYYY-MM-DD"
    });

    const searchBtn = $('.search-button');

    function toggleSearchButton() {
        const hasItem = $('#itemId').val();
        searchBtn.prop('disabled', !hasItem);
    }


    toggleSearchButton();


    $('#itemId').on('change', function () {

        let itemId = $(this).val();

        toggleSearchButton();
    });


    $('#itemClassId').on('change', function () {

        let classId = $(this).val();

        toggleSearchButton();

        if (!classId) {
            setSearchableMultiSelectDisabled('itemId', true);
            setSearchableMultiSelectOptions('itemId', []);
            return;
        }

        setSearchableMultiSelectDisabled('itemId', false);

        $.get('/GetItemsByClass/' + classId, function (res) {
            if (res.success) {

                const items = res.result.map(i => ({
                    value: i.id,
                    text: i.fullName
                }));

                setSearchableMultiSelectOptions('itemId', items);
            }
        });
    });

});

function setSearchableMultiSelectDisabled(selectId, disabled = true) {
    const hidden = document.getElementById(selectId);
    if (!hidden) return;

    const wrapper = hidden.closest('.ms-wrapper');
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

function setSearchableMultiSelectOptions(selectId, items, placeholder = 'انتخاب کنید') {

    const hidden = document.getElementById(selectId);
    if (!hidden) return;

    const wrapper = hidden.closest('.ms-wrapper');
    const optionsContainer = wrapper.querySelector('.ms-options');
    const tags = wrapper.querySelector('.ms-tags');

    // reset hidden value
    hidden.value = '';

    // reset tags
    tags.innerHTML = `<span class="ms-placeholder">${placeholder}</span>`;

    // clear options
    optionsContainer.innerHTML = '';

    // -------------------------
    // SELECT ALL ROW
    // -------------------------
    const selectAll = document.createElement('div');
    selectAll.className = 'ms-option ms-select-all';
    selectAll.dataset.value = '__all__';

    selectAll.innerHTML = `
        <input type="checkbox" />
        انتخاب همه
    `;

    optionsContainer.appendChild(selectAll);

    const hrTop = document.createElement('hr');
    hrTop.style.margin = '5px';
    optionsContainer.appendChild(hrTop);

    // -------------------------
    // OPTIONS
    // -------------------------
    items.forEach((i, index) => {

        const div = document.createElement('div');
        div.className = 'ms-option';
        div.dataset.value = i.value;

        div.innerHTML = `
            <input type="checkbox" />
            ${i.text}
        `;

        optionsContainer.appendChild(div);

        if (index !== items.length - 1) {
            const hr = document.createElement('hr');
            hr.style.margin = '5px';
            optionsContainer.appendChild(hr);
        }
    });

    // trigger change for consistency
    $(hidden).trigger('change');
}
