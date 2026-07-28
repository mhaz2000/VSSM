$(function () {
    // =========================================================
    // Searchable Select
    // =========================================================
    $('.ss-wrapper').each(function () {
        const wrapper = $(this);
        const control = wrapper.find('.ss-control');
        const display = wrapper.find('.ss-display');
        const dropdown = wrapper.find('.ss-dropdown');
        const search = wrapper.find('.ss-search');
        const options = wrapper.find('.ss-option');
        const hidden = wrapper.find('input[type="hidden"]');
        const textSpan = wrapper.find('.ss-text');

        // Open/close
        display.on('click', function (e) {
            e.stopPropagation();
            control.toggleClass('open');
        });

        // Select option
        options.on('click', function () {
            const val = $(this).data('value');
            const text = $(this).text().trim();
            hidden.val(val);
            textSpan.text(text);
            control.removeClass('open');
            hidden.trigger('change');
        });

        // Search filter
        search.on('input', function () {
            const q = $(this).val().toLowerCase();
            options.each(function () {
                const txt = $(this).text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        // Close on outside click
        $(document).on('click', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });
    });

    // =========================================================
    // Searchable Multi Select
    // =========================================================
    $('.ms-wrapper').each(function () {
        const wrapper = $(this);
        const control = wrapper.find('.ms-control');
        const display = wrapper.find('.ms-display');
        const dropdown = wrapper.find('.ms-dropdown');
        const search = wrapper.find('.ms-search');
        const options = wrapper.find('.ms-option:not(.ms-select-all)');
        const selectAll = wrapper.find('.ms-select-all');
        const hidden = wrapper.find('input[type="hidden"]');
        const tags = wrapper.find('.ms-tags');

        // Open/close
        display.on('click', function (e) {
            e.stopPropagation();
            control.toggleClass('open');
        });

        // Helper: update hidden and tags
        function updateMultiSelect() {
            const selected = [];
            options.each(function () {
                const opt = $(this);
                const cb = opt.find('input[type="checkbox"]');
                if (cb.prop('checked')) {
                    selected.push(opt.data('value'));
                }
            });
            hidden.val(selected.join(','));

            // Update tags
            tags.empty();
            if (selected.length === 0) {
                tags.html('<span class="ms-placeholder">انتخاب کنید</span>');
            } else {
                selected.forEach(function (val) {
                    const opt = options.filter('[data-value="' + val + '"]');
                    const text = opt.find('span').text().trim();
                    const tag = $('<span class="ms-tag" data-value="' + val + '">' +
                        '<span class="ms-tag-text">' + text + '</span>' +
                        '<i class="fa fa-times ms-remove-tag"></i>' +
                        '</span>');
                    tags.append(tag);
                });
            }
            hidden.trigger('change');
        }

        // Toggle option
        options.on('click', function (e) {
            e.stopPropagation();
            const cb = $(this).find('input[type="checkbox"]');
            cb.prop('checked', !cb.prop('checked'));
            $(this).toggleClass('selected', cb.prop('checked'));
            // Update select all state
            const allChecked = options.find('input[type="checkbox"]:checked').length === options.length;
            selectAll.find('input[type="checkbox"]').prop('checked', allChecked);
            updateMultiSelect();
        });

        // Select all
        selectAll.on('click', function (e) {
            e.stopPropagation();
            const cb = $(this).find('input[type="checkbox"]');
            const checked = !cb.prop('checked');
            cb.prop('checked', checked);
            options.each(function () {
                $(this).find('input[type="checkbox"]').prop('checked', checked);
                $(this).toggleClass('selected', checked);
            });
            updateMultiSelect();
        });

        // Remove tag
        tags.on('click', '.ms-remove-tag', function (e) {
            e.stopPropagation();
            const tag = $(this).closest('.ms-tag');
            const val = tag.data('value');
            const opt = options.filter('[data-value="' + val + '"]');
            opt.find('input[type="checkbox"]').prop('checked', false);
            opt.removeClass('selected');
            // Update select all
            const allChecked = options.find('input[type="checkbox"]:checked').length === options.length;
            selectAll.find('input[type="checkbox"]').prop('checked', allChecked);
            updateMultiSelect();
        });

        // Search filter
        search.on('input', function () {
            const q = $(this).val().toLowerCase();
            options.each(function () {
                const txt = $(this).find('span').text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        // Close on outside click
        $(document).on('click', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });

        // Init
        updateMultiSelect();
    });
});