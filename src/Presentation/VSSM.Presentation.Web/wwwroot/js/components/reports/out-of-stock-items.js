$(function () {

    const searchBtn = $('#searchBtn');
    function toggleSearchButton() {
        const hasItemClass = $('#itemClassId').val();
        searchBtn.prop('disabled', !hasItemClass);
    }

    // Run once on page load
    toggleSearchButton();

    $('#itemClassId').on('change', function () {

        toggleSearchButton();

    });

});
