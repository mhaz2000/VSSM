$(function () {

    const searchBtn = $('#searchBtn');

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

    function toggleSearchButton() {
        const hasItem = $('#itemId').val();

        searchBtn.prop('disabled', !hasItem);
    }

    // initial call
    toggleSearchButton();

    // when item (hidden field) changes
    $('#itemId').on('change', toggleSearchButton);
});
