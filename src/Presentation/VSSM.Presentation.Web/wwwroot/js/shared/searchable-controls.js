$(() => {
    "use strict";

    $('.ss-wrapper').each(function () {
        const wrapper = $(this);
        const control = wrapper.find('.ss-control');
        const display = wrapper.find('.ss-display');
        const search = wrapper.find('.ss-search');
        const hidden = wrapper.find('input[type="hidden"]');
        const textSpan = wrapper.find('.ss-text');

        display.off('click.ssToggle').on('click.ssToggle', function (e) {
            e.stopPropagation();
            if (wrapper.hasClass('ss-disabled') || wrapper.hasClass('disabled')) return;
            control.toggleClass('open');
        });

        wrapper.off('click.ssOption').on('click.ssOption', '.ss-option', function () {
            const val = $(this).data('value');
            const text = $(this).text().trim();
            hidden.val(val);
            textSpan.text(text);
            control.removeClass('open');
            hidden.trigger('change');
        });

        search.off('input.ssSearch').on('input.ssSearch', function () {
            const q = $(this).val().toLowerCase();
            wrapper.find('.ss-option').each(function () {
                const txt = $(this).text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        $(document).off('click.ssClose').on('click.ssClose', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });
    });

    $('.ms-wrapper').each(function () {
        const wrapper = $(this);
        const control = wrapper.find('.ms-control');
        const display = wrapper.find('.ms-display');
        const search = wrapper.find('.ms-search');
        const hidden = wrapper.find('input[type="hidden"]');
        const tags = wrapper.find('.ms-tags');

        const refreshFromHidden = () => {
            const value = hidden.val() || '';
            const selected = value
                ? value.split(',').filter(v => v !== '' && v !== '__all__')
                : [];

            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                const opt = $(this);
                const val = String(opt.data('value'));
                const checked = selected.indexOf(val) !== -1;
                opt.find('input[type="checkbox"]').prop('checked', checked);
                opt.toggleClass('selected', checked);
            });

            const allOptions = wrapper.find('.ms-option:not(.ms-select-all)');
            const checkedCount = allOptions.filter('.selected').length;
            const allChecked = allOptions.length > 0 && checkedCount === allOptions.length;
            wrapper.find('.ms-select-all input[type="checkbox"]').prop('checked', allChecked);

            tags.empty();
            if (selected.length === 0) {
                tags.html('<span class="ms-placeholder">انتخاب کنید</span>');
            } else {
                selected.forEach(val => {
                    const opt = wrapper.find('.ms-option:not(.ms-select-all)[data-value="' + val + '"]');
                    const text = opt.find('span').text().trim() || val;
                    const tag = $(
                        '<span class="ms-tag" data-value="' + val + '">' +
                        '<span class="ms-tag-text">' + text + '</span>' +
                        '<i class="fa fa-times ms-remove-tag"></i>' +
                        '</span>'
                    );
                    tags.append(tag);
                });
            }
        };

        const buildSelectedArray = () => {
            const selected = [];
            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                if ($(this).find('input[type="checkbox"]').prop('checked')) {
                    selected.push($(this).data('value'));
                }
            });
            return selected;
        };

        display.off('click.msToggle').on('click.msToggle', function (e) {
            e.stopPropagation();
            if (wrapper.hasClass('ms-disabled') || wrapper.hasClass('disabled')) return;
            control.toggleClass('open');
        });

        $(document).off('click.msClose').on('click.msClose', function (e) {
            if (!wrapper.is(e.target) && wrapper.has(e.target).length === 0) {
                control.removeClass('open');
            }
        });

        hidden.off('change.msRefresh').on('change.msRefresh', refreshFromHidden);

        wrapper.off('click.msOption').on('click.msOption', '.ms-option:not(.ms-select-all)', function (e) {
            e.stopPropagation();
            const opt = $(this);
            const cb = opt.find('input[type="checkbox"]');
            cb.prop('checked', !cb.prop('checked'));
            opt.toggleClass('selected', cb.prop('checked'));

            hidden.val(buildSelectedArray().join(',')).trigger('change');
        });

        wrapper.off('click.msSelectAll').on('click.msSelectAll', '.ms-select-all', function (e) {
            e.stopPropagation();
            const cb = $(this).find('input[type="checkbox"]');
            const checked = !cb.prop('checked');
            cb.prop('checked', checked);

            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                const opt = $(this);
                opt.find('input[type="checkbox"]').prop('checked', checked);
                opt.toggleClass('selected', checked);
            });

            const selected = checked ? buildSelectedArray() : [];
            hidden.val(selected.join(',')).trigger('change');
        });

        wrapper.off('click.msRemoveTag').on('click.msRemoveTag', '.ms-remove-tag', function (e) {
            e.stopPropagation();
            const tag = $(this).closest('.ms-tag');
            const val = tag.data('value');
            const opt = wrapper.find('.ms-option:not(.ms-select-all)[data-value="' + val + '"]');
            if (opt.length) {
                opt.find('input[type="checkbox"]').prop('checked', false);
                opt.removeClass('selected');
                hidden.val(buildSelectedArray().join(',')).trigger('change');
            }
        });

        search.off('input.msSearch').on('input.msSearch', function () {
            const q = $(this).val().toLowerCase();
            wrapper.find('.ms-option:not(.ms-select-all)').each(function () {
                const txt = $(this).find('span').text().trim().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
            });
        });

        refreshFromHidden();
    });
});