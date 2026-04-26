$(document).on("click", ".ms-display", function (e) {
    e.stopPropagation();

    var wrapper = $(this).closest(".ms-wrapper");
    var dropdown = wrapper.find(".ms-dropdown");

    $(".ms-dropdown").not(dropdown).hide();
    $(".ms-wrapper").not(wrapper).removeClass("open");

    dropdown.toggle();
    wrapper.toggleClass("open");
});

// Close when clicking outside
$(document).on("click", function () {
    $(".ms-dropdown").hide();
    $(".ms-wrapper").removeClass("open");
});

$(document).on("keyup", ".ms-search", function () {
    var value = $(this).val().toLowerCase();
    $(this)
        .parent()
        .find(".ms-option")
        .not(".ms-select-all")
        .each(function () {
            var text = $(this).text().toLowerCase();
            $(this).toggle(text.indexOf(value) > -1);
        });
});

$(document).on("click", ".ms-option", function (e) {
    e.stopPropagation();

    var wrapper = $(this).closest(".ms-wrapper");
    var isSelectAll = $(this).hasClass("ms-select-all");
    var checkbox = $(this).find("input[type=checkbox]")[0];

    checkbox.checked = !checkbox.checked;

    if (isSelectAll) {
        toggleSelectAll(wrapper, checkbox.checked);
    } else {
        $(this).toggleClass("selected", checkbox.checked);
        syncSelectAllState(wrapper);
    }

    updateMsTags(wrapper);
});

function toggleSelectAll(wrapper, isChecked) {

    var options = wrapper.find(".ms-option").not(".ms-select-all");

    options.each(function () {
        $(this)
            .toggleClass("selected", isChecked)
            .find("input[type=checkbox]")[0].checked = isChecked;
    });

    wrapper.find(".ms-select-all")
        .toggleClass("selected", isChecked)
        .find("input[type=checkbox]")[0].checked = isChecked;
}

function syncSelectAllState(wrapper) {

    var allOptions = wrapper.find(".ms-option").not(".ms-select-all");
    var selectedOptions = allOptions.filter(".selected");

    var allSelected = allOptions.length > 0 &&
        selectedOptions.length === allOptions.length;

    wrapper.find(".ms-select-all")
        .toggleClass("selected", allSelected)
        .find("input[type=checkbox]")[0].checked = allSelected;
}


$(document).on("click", ".ms-remove-tag", function (e) {
    e.stopPropagation();

    var tag = $(this).closest(".ms-tag");
    var value = tag.data("value");
    var wrapper = tag.closest(".ms-wrapper");

    var option = wrapper.find(".ms-option[data-value='" + value + "']");

    option.removeClass("selected")
        .find("input[type=checkbox]")[0].checked = false;

    syncSelectAllState(wrapper);
    updateMsTags(wrapper);
});

function updateMsTags(wrapper) {

    var tagContainer = wrapper.find(".ms-tags");
    var hiddenInput = wrapper.find("input[type=hidden]");
    var selectedOptions = wrapper
        .find(".ms-option.selected")
        .not(".ms-select-all");

    tagContainer.empty();

    var values = [];

    selectedOptions.each(function () {
        var val = $(this).data("value");
        var text = $(this).clone()
            .children()
            .remove()
            .end()
            .text()
            .trim();

        values.push(val);

        tagContainer.append(
            '<span class="ms-tag" data-value="' + val + '">' +
            text + ' <i class="fa fa-times ms-remove-tag"></i>' +
            '</span>'
        );
    });

    if (values.length === 0) {
        tagContainer.html('<span class="ms-placeholder">انتخاب کنید</span>');
    }

    hiddenInput.val(values.join(","));
}
