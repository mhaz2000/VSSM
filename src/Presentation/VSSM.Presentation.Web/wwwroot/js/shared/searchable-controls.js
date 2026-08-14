$(function () {

    $('.ss-wrapper').each(function () {
        var wrapper = $(this);
        var control = wrapper.find('.ss-control');
        var display = wrapper.find('.ss-display');
        var search = wrapper.find('.ss-search');
        var hidden = wrapper.find('input[type="hidden"]');
        var textSpan = wrapper.find('.ss-text');

        display.on('click', function (e) {
            e.stopPropagation();
            control.toggleClass('open');
        });

        wrapper.on('click', '.ss-option', function () {
            var val = $(this).data('value');
            var text = $(this).text().trim();
            hidden.val(val);
            textSpan.text(text);
            control.removeClass('open');
            hidden.trigger('change');
        });

        search.on('input', function () {
            var q = $(this).val().toLowerCase();
            wrapper.find('.ss-option').each(function () {
                var txt = $(this).text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        $(document).on('click', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });
    });

    $('.ms-wrapper').each(function () {
        var wrapper = $(this);
        var control = wrapper.find('.ms-control');
        var display = wrapper.find('.ms-display');
        var search = wrapper.find('.ms-search');
        var hidden = wrapper.find('input[type="hidden"]');
        var tags = wrapper.find('.ms-tags');

        display.on('click', function (e) {
            e.stopPropagation();
            control.toggleClass('open');
        });

        $(document).on('click', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });

        function refreshFromHidden() {
            var value = hidden.val() || '';
            var selected = value ? value.split(',').filter(function (v) {
                return v !== '' && v !== '__all__';
            }) : [];

            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                var opt = $(this);
                var val = String(opt.data('value'));
                var checked = selected.indexOf(val) !== -1;
                opt.find('input[type="checkbox"]').prop('checked', checked);
                opt.toggleClass('selected', checked);
            });

            var allOptions = wrapper.find('.ms-option:not(.ms-select-all)');
            var checkedCount = allOptions.filter('.selected').length;
            var allChecked = allOptions.length > 0 && checkedCount === allOptions.length;
            wrapper.find('.ms-select-all input[type="checkbox"]').prop('checked', allChecked);

            tags.empty();
            if (selected.length === 0) {
                tags.html('<span class="ms-placeholder">انتخاب کنید</span>');
            } else {
                selected.forEach(function (val) {
                    var opt = wrapper.find('.ms-option:not(.ms-select-all)[data-value="' + val + '"]');
                    var text = opt.find('span').text().trim() || val;
                    var tag = $(
                        '<span class="ms-tag" data-value="' + val + '">' +
                        '<span class="ms-tag-text">' + text + '</span>' +
                        '<i class="fa fa-times ms-remove-tag"></i>' +
                        '</span>'
                    );
                    tags.append(tag);
                });
            }
        }

        hidden.on('change', refreshFromHidden);

        wrapper.on('click', '.ms-option:not(.ms-select-all)', function (e) {
            e.stopPropagation();
            var opt = $(this);
            var cb = opt.find('input[type="checkbox"]');
            cb.prop('checked', !cb.prop('checked'));
            opt.toggleClass('selected', cb.prop('checked'));

            var selected = [];
            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                if ($(this).find('input[type="checkbox"]').prop('checked')) {
                    selected.push($(this).data('value'));
                }
            });
            hidden.val(selected.join(',')).trigger('change');
        });

        wrapper.on('click', '.ms-select-all', function (e) {
            e.stopPropagation();
            var cb = $(this).find('input[type="checkbox"]');
            var checked = !cb.prop('checked');
            cb.prop('checked', checked);

            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                var opt = $(this);
                opt.find('input[type="checkbox"]').prop('checked', checked);
                opt.toggleClass('selected', checked);
            });

            var selected = [];
            if (checked) {
                wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                    selected.push($(this).data('value'));
                });
            }
            hidden.val(selected.join(',')).trigger('change');
        });

        wrapper.on('click', '.ms-remove-tag', function (e) {
            e.stopPropagation();
            var tag = $(this).closest('.ms-tag');
            var val = tag.data('value');
            var opt = wrapper.find('.ms-option:not(.ms-select-all)[data-value="' + val + '"]');
            if (opt.length) {
                opt.find('input[type="checkbox"]').prop('checked', false);
                opt.removeClass('selected');
                var selected = [];
                wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                    if ($(this).find('input[type="checkbox"]').prop('checked')) {
                        selected.push($(this).data('value'));
                    }
                });
                hidden.val(selected.join(',')).trigger('change');
            }
        });

        search.on('input', function () {
            var q = $(this).val().toLowerCase();
            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                var txt = $(this).find('span').text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        refreshFromHidden();
    });
});