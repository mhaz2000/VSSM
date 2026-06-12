// inputFiles.js

$(document).ready(function () {

    // File input change
    $("#xmlFile").on("change", function () {
        handleFileSelect(this);
    });

    // Monitor select changes
    monitorSelectChanges();

    // Initial validation state
    checkFormValidity();
});


$('#ItemId').on('change', function () {

    let itemId = $(this).val();

    if (!itemId) {
        setSearchableSelectDisabled('DosePerVialId', true);
        setSearchableSelectOptions('DosePerVialId', []);
        return;
    }

    setSearchableSelectDisabled('itemId', false);

    debugger


    $.get('/GetItemDoesPerVials/' + itemId, function (res) {
        if (res.success) {
            debugger


            const items = res.result.map(i => ({
                value: i.dosePerVialId,
                text: i.numDosePerVial
            }));
            setSearchableSelectOptions('DosePerVialId', items);
        }
    });
});

function checkFormValidity() {
    const isFileSelected = $("#xmlFile")[0].files.length > 0;
    const isItemSelected = !!$("#ItemId").val();
    const isDosePerVialSelected = !!$("#DosePerVialId").val();

    const isValid = isFileSelected && isItemSelected && isDosePerVialSelected;

    $("#uploadBtn")
        .prop("disabled", !isValid)
        .attr("title", isValid ? "" : "لطفاً فایل، کالا و دوز بر ویال را انتخاب کنید");

    return isValid;
}


function handleFileSelect(input) {
    const fileName = input.files.length > 0
        ? "فایل انتخاب شده: " + input.files[0].name
        : "هیچ فایلی انتخاب نشده است";

    $("#fileName").text(fileName);
    checkFormValidity();
}


function monitorSelectChanges() {
    $("#ItemId, #DosePerVialId").on("input change", function () {
        checkFormValidity();
    });

    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === "attributes" && mutation.attributeName === "value") {
                checkFormValidity();
            }
        });
    });

    const itemSelect = document.getElementById("ItemId");
    const doseSelect = document.getElementById("DosePerVialId");

    if (itemSelect) observer.observe(itemSelect, { attributes: true });
    if (doseSelect) observer.observe(doseSelect, { attributes: true });
}