$(function () {

    setSearchableSelectDisabled('itemId', true);

    $('#fromDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#fromDateEn',  // hidden field
        altFormat: "YYYY-MM-DD"
    });

    $('#toDate').pDatepicker({
        format: "YYYY/MM/DD",
        autoClose: true,
        initialValue: false,
        altField: '#toDateEn',
        altFormat: "YYYY-MM-DD"
    });

    const searchBtn = $('#searchBtn');

    function toggleSearchButton() {
        const hasItemClass = $('#itemClassId').val();
        const hasItem = $('#itemId').val();

        searchBtn.prop('disabled', !(hasItemClass && hasItem));
    }

    // initial call
    toggleSearchButton();

    // when category changes
    $('#itemClassId').on('change', toggleSearchButton);

    // when item (hidden field) changes
    $('#itemId').on('change', toggleSearchButton);


    $('#itemClassId').on('change', function () {

        let classId = $(this).val();

        // Toggle search button on every change
        toggleSearchButton();

        if (!classId) {
            setSearchableSelectDisabled('itemId', true);
            setSearchableSelectOptions('itemId', []);
            return;
        }

        setSearchableSelectDisabled('itemId', false);

        $.get('/CurrentStockReport/GetItemsByClass/' + classId, function (res) {
            if (res.success) {

                const items = res.result.map(i => ({
                    value: i.id,
                    text: i.fullName
                }));

                setSearchableSelectOptions('itemId', items);
            }
        });
    });

});
