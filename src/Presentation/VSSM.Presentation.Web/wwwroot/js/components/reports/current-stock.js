$(function () {

    setSearchableSelectDisabled('itemId', true);

    const searchBtn = $('#searchBtn');

    function toggleSearchButton() {
        const hasItemClass = $('#itemClassId').val();
        searchBtn.prop('disabled', !hasItemClass);
    }

    // Run once on page load
    toggleSearchButton();

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

        $.get('/GetItemsByClass/' + classId, function (res) {
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
