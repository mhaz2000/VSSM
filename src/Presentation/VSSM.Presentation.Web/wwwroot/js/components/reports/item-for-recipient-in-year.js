$(function () {

    const searchBtn = $('#searchBtn');

    function toggleSearchButton() {
        const hasYear = $('#year').val();
        const hasItem = $('#itemId').val();

        searchBtn.prop('disabled', !(hasYear && hasItem));
    }

    // initial call
    toggleSearchButton();

    // when category changes
    $('#year').on('change', toggleSearchButton);

    // when item (hidden field) changes
    $('#itemId').on('change', toggleSearchButton);
});
