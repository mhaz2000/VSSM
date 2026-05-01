$(function () {

    // Disable both selects on page load
    setSearchableSelectDisabled('itemId', true);
    setSearchableSelectDisabled('batchNumber', true);

    const searchBtn = $('#searchBtn');

    function toggleSearchButton() {
        const hasItemClass = $('#itemClassId').val();
        const hasItem = $('#itemId').val();

        searchBtn.prop('disabled', !hasItemClass);
    }

    toggleSearchButton();


    $('#itemClassId').on('change', function () {

        let classId = $(this).val();

        toggleSearchButton();

        setSearchableSelectDisabled('itemId', true);
        setSearchableSelectOptions('itemId', []);

        setSearchableSelectDisabled('batchNumber', true);
        setSearchableSelectOptions('batchNumber', []);

        if (!classId) {
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


    $('#itemId').on('change', function () {

        let itemId = $(this).val();

        setSearchableSelectDisabled('batchNumber', true);
        setSearchableSelectOptions('batchNumber', []);

        if (!itemId) {
            return;
        }

        setSearchableSelectDisabled('batchNumber', false);

        $.get('/GetItemBatchNumbers/' + itemId, function (res) {
            if (res.success) {
                debugger
                const batchNumbers = res.result.map(b => ({
                    value: b.batchNumber,
                    text: b.batchNumber + " (مقدار موجود: " + b.currentStock + ")"
                }));

                setSearchableSelectOptions('batchNumber', batchNumbers);
            }
        });
    });

});
