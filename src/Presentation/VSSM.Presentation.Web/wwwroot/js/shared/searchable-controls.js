$(() => {
    "use strict";

    const getPlaceholder = ($wrap) => $wrap.attr('data-placeholder') || 'انتخاب کنید';

    $('.ss-wrapper').each(function () {
        const $w = $(this);
        const $hidden = $w.find('input[type="hidden"]').first();
        const $text = $w.find('.ss-text');

        $w.on('click.ssToggle', '.ss-display', function (e) {
            e.stopPropagation();
            if ($w.is('.ss-disabled, .disabled')) return;
            $('.ss-control, .ms-control').not($w.find('.ss-control')).removeClass('open');
            $w.find('.ss-control').toggleClass('open');
        });

        $w.on('click.ssOption', '.ss-option', function (e) {
            e.stopPropagation();
            if ($(this).hasClass('disabled')) return;
            $hidden.val($(this).data('value')).trigger('change');
            $text.text($(this).text().trim());
            $w.find('.ss-control').removeClass('open');
        });

        $w.on('input.ssSearch', '.ss-search', function () {
            const q = ($(this).val() || '').toLowerCase();
            $w.find('.ss-option').each(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(q) > -1);
            });
        });
    });

    $('.ms-wrapper').each(function () {
        const $w = $(this);
        const $hidden = $w.find('input[type="hidden"]').first();
        const $tags = $w.find('.ms-tags');

        const readSelected = () =>
            ($hidden.val() || '').split(',').map(v => v.trim()).filter(v => v && v !== '__all__');

        const writeSelected = (values) =>
            $hidden.val(values.join(',')).trigger('change');

        const renderTags = (selected) => {
            const total = $w.find('.ms-option:not(.ms-select-all)').length;
            const max = parseInt($w.attr('data-max-labels'), 10) || 3;
            const allFmt = $w.attr('data-all-format') || 'همه ({0} مورد)';
            const ovfFmt = $w.attr('data-overflow-format') || '{0} مورد انتخاب شده';

            $tags.empty();

            if (selected.length === 0) {
                $tags.html('<span class="ms-placeholder">' + getPlaceholder($w) + '</span>');
                return;
            }

            if (total > 0 && selected.length === total) {
                const txt = allFmt.replace('{0}', selected.length);
                $tags.html('<span class="ms-tag ms-tag-summary"><i class="fa fa-check-double"></i><span class="ms-tag-text">' + txt + '</span></span>');
                return;
            }

            if (selected.length > max) {
                const txt = ovfFmt.replace('{0}', selected.length);
                $tags.html('<span class="ms-tag ms-tag-summary"><i class="fa fa-list-ul"></i><span class="ms-tag-text">' + txt + '</span></span>');
                return;
            }

            selected.forEach(val => {
                let text = val;
                $w.find('.ms-option:not(.ms-select-all)').each(function () {
                    if (String($(this).data('value')) === String(val)) {
                        text = $(this).find('span').first().text().trim();
                    }
                });
                $tags.append(
                    '<span class="ms-tag" data-value="' + val + '">' +
                    '<span class="ms-tag-text">' + text + '</span>' +
                    '<i class="fa fa-times ms-remove-tag"></i>' +
                    '</span>'
                );
            });
        };

        const refresh = () => {
            const selected = readSelected();
            const $opts = $w.find('.ms-option:not(.ms-select-all)');

            $opts.each(function () {
                const val = String($(this).data('value'));
                const on = selected.indexOf(val) !== -1;
                $(this).find('input[type="checkbox"]').prop('checked', on);
                $(this).toggleClass('selected', on);
            });

            const allOn = $opts.length > 0 && $opts.filter('.selected').length === $opts.length;
            $w.find('.ms-select-all input[type="checkbox"]').prop('checked', allOn);

            renderTags(selected);
        };

        $hidden.on('change.msRefresh', refresh);

        $w.on('click.msToggle', '.ms-display', function (e) {
            e.stopPropagation();
            if ($w.is('.ms-disabled, .disabled')) return;
            $('.ms-control, .ss-control').not($w.find('.ms-control')).removeClass('open');
            $w.find('.ms-control').toggleClass('open');
        });

        $w.on('click.msOption', '.ms-option:not(.ms-select-all)', function (e) {
            e.stopPropagation();
            const val = String($(this).data('value'));
            const selected = readSelected();
            const i = selected.indexOf(val);
            if (i === -1) selected.push(val);
            else selected.splice(i, 1);
            writeSelected(selected);
        });

        $w.on('click.msSelectAll', '.ms-select-all', function (e) {
            e.stopPropagation();
            const selected = readSelected();
            const total = $w.find('.ms-option:not(.ms-select-all)').length;

            if (selected.length === total && total > 0) {
                writeSelected([]);
            } else {
                writeSelected(
                    $w.find('.ms-option:not(.ms-select-all)').map(function () {
                        return String($(this).data('value'));
                    }).get()
                );
            }
        });

        $w.on('click.msRemoveTag', '.ms-remove-tag', function (e) {
            e.stopPropagation();
            const val = String($(this).closest('.ms-tag').data('value'));
            writeSelected(readSelected().filter(v => v !== val));
        });

        $w.on('click.msRemoveSummary', '.ms-tag-summary', function (e) {
            e.stopPropagation();
            writeSelected([]);
        });

        $w.on('input.msSearch', '.ms-search', function () {
            const q = ($(this).val() || '').toLowerCase();
            $w.find('.ms-option:not(.ms-select-all)').each(function () {
                const txt = $(this).find('span').first().text().toLowerCase();
                $(this).toggle(txt.indexOf(q) > -1);
                $(this).next('hr').toggle(txt.indexOf(q) > -1);
            });
        });

        refresh();
    });

    $(document).off('click.searchableClose').on('click.searchableClose', function (e) {
        if (!$(e.target).closest('.ms-wrapper, .ss-wrapper').length) {
            $('.ms-control, .ss-control').removeClass('open');
        }
    });

    window.SearchableControls = {
        reset(id) {
            const $hidden = $('#' + id);
            if (!$hidden.length) return;

            const $w = $hidden.closest('.ms-wrapper, .ss-wrapper');
            $hidden.val('');

            if ($w.hasClass('ms-wrapper')) {
                $w.find('.ms-option').removeClass('selected');
                $w.find('input[type="checkbox"]').prop('checked', false);
                $w.find('.ms-search').val('');
                $w.find('.ms-option, .ms-option-separator').show();
                $w.find('.ms-tags').html('<span class="ms-placeholder">' + getPlaceholder($w) + '</span>');
            } else {
                $w.find('.ss-option').removeClass('selected');
                $w.find('.ss-search').val('');
                $w.find('.ss-option').show();
                $w.find('.ss-text').text(getPlaceholder($w));
            }

            $hidden.trigger('change');
        }
    };
});