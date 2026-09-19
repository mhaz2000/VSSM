$(() => {
    "use strict";

    const form = $('#entryExitReportForm');
    if (!form.length) return;

    const reportTypeInput = form.find('#reportTypeInput');
    const tabs = form.find('.report-tab');
    const specificInbound = form.find('[data-specific="inbound"]');
    const specificOutbound = form.find('[data-specific="outbound"]');
    const itemClassSelect = $('#itemClassId');

    const escapeHtml = (value) => {
        if (value == null) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    };

    const setReportType = (type) => {
        if (!type) return;
        reportTypeInput.val(type);

        tabs.removeClass('is-active').attr('aria-selected', 'false')
            .filter(`[data-report="${type}"]`).addClass('is-active').attr('aria-selected', 'true');

        const category = tabs.filter(`[data-report="${type}"]`).data('category');

        if (category === 'inbound') {
            specificInbound.removeAttr('hidden');
            specificOutbound.attr('hidden', true);
        } else {
            specificInbound.attr('hidden', true);
            specificOutbound.removeAttr('hidden');
        }

        updateActiveFilters();
    };

    const initDatePickers = () => {
        if (typeof DatePicker === 'undefined' || typeof DatePicker.attach !== 'function') return;

        form.find('.datepicker-input').each(function () {
            const $input = $(this);
            const mode = $input.attr('data-calendar-mode') || 'jalali';
            DatePicker.attach($input, mode);
        });
    };

    const switchCalendarMode = (targetId) => {
        const $input = $('#' + targetId);
        if (!$input.length) return;

        const current = $input.attr('data-calendar-mode') || 'jalali';
        const next = current === 'jalali' ? 'gregorian' : 'jalali';

        $input.attr('data-calendar-mode', next).val('');
        $input.siblings('input[type="hidden"]').val('');

        if (typeof DatePicker !== 'undefined' && DatePicker.attach === 'function') {
            DatePicker.attach($input, next);
        }

        const $btn = form.find(`.btn-toggle-date-mode[data-target="${targetId}"]`);
        $btn.attr('data-mode', next);
        $btn.find('.mode-label').text(next === 'jalali' ? 'شمسی' : 'میلادی');
        updateActiveFilters();
    };

    const loadItemsByClass = (classId) => {
        if (window.SearchableControls) {
            window.SearchableControls.setOptions('itemId', []);
            window.SearchableControls.reset('itemId');
        }

        if (!classId) return;

        $.ajax({
            url: '/EntryExitReport/GetItemsByClass',
            type: 'GET',
            data: { classId: classId },
            success: function (res) {
                if (!res || res.success !== true) return;

                const mapped = (res.result || []).map(x => ({
                    value: String(x.itemId),
                    text: x.fullName
                }));

                if (window.SearchableControls) {
                    window.SearchableControls.setOptions('itemId', mapped);
                }
            },
            error: function () {
                if (window.SearchableControls) {
                    window.SearchableControls.setOptions('itemId', []);
                }
            }
        });
    };

    const collectChips = () => {
        const chips = [];

        form.find('.ms-wrapper').each(function () {
            const $wrap = $(this);
            if (!$wrap.is(':visible')) return;

            const $hidden = $wrap.find('input[type="hidden"]').first();
            const val = ($hidden.val() || '').trim();
            if (!val) return;

            const label = $wrap.closest('.f-group').find('.f-label').first().text().trim();
            const $summary = $wrap.find('.ms-tag-summary .ms-tag-text').first();
            let text;

            if ($summary.length) {
                text = $summary.text().trim();
            } else {
                const parts = [];
                $wrap.find('.ms-tag:not(.ms-tag-summary) .ms-tag-text').each(function () {
                    const t = $(this).text().trim();
                    if (t) parts.push(t);
                });
                text = parts.length ? parts.join('، ') : val;
            }

            chips.push({
                key: $hidden.attr('id') || $hidden.attr('name'),
                name: $hidden.attr('name') || $hidden.attr('id'),
                label, text, kind: 'multi'
            });
        });

        form.find('.ss-wrapper').each(function () {
            const $wrap = $(this);
            if (!$wrap.is(':visible')) return;

            const $hidden = $wrap.find('input[type="hidden"]').first();
            const val = ($hidden.val() || '').trim();
            if (!val) return;

            const label = $wrap.closest('.f-group').find('.f-label').first().text().trim();
            const text = $wrap.find('.ss-text').first().text().trim() || val;

            chips.push({
                key: $hidden.attr('id') || $hidden.attr('name'),
                name: $hidden.attr('name') || $hidden.attr('id'),
                label, text, kind: 'single'
            });
        });

        form.find('input.f-control, select.f-control').each(function () {
            const $el = $(this);
            if (!$el.is(':visible')) return;
            if ($el.hasClass('datepicker-input')) return;

            const val = ($el.val() || '').trim();
            if (!val) return;

            const label = $el.closest('.f-group').find('.f-label').first().text().trim();
            const text = $el.is('select') ? $el.find('option:selected').text().trim() : val;

            chips.push({
                key: $el.attr('id') || $el.attr('name'),
                name: $el.attr('name') || $el.attr('id'),
                label, text, kind: 'text'
            });
        });

        form.find('input.datepicker-input').each(function () {
            const $el = $(this);
            if (!$el.is(':visible')) return;

            const val = ($el.val() || '').trim();
            if (!val) return;

            const label = $el.closest('.f-group').find('.f-label').first().text().trim();
            chips.push({
                key: $el.attr('id'),
                name: $el.siblings('input[type="hidden"]').attr('name') || $el.attr('id'),
                label, text: val, kind: 'date'
            });
        });

        return chips;
    };

    const updateActiveFilters = () => {
        const box = $('#activeFilters');
        if (!box.length) return;

        const chips = collectChips();

        if (!chips.length) {
            box.attr('hidden', true).empty();
            return;
        }

        box.removeAttr('hidden').html(
            '<div class="active-filters-title">فیلترهای فعال</div>' +
            '<div class="active-filters-list">' +
            chips.map(c =>
                `<span class="active-chip" data-name="${escapeHtml(c.name)}" data-kind="${c.kind}">
                    <span class="chip-label">${escapeHtml(c.label)}:</span>
                    <span class="chip-value">${escapeHtml(c.text)}</span>
                    <i class="fa fa-times chip-remove" role="button" aria-label="حذف"></i>
                </span>`
            ).join('') +
            '<button type="button" class="active-chip active-chip-clear js-clear-filters">پاک کردن همه</button>' +
            '</div>'
        );
    };

    const removeFilterByName = (name, kind) => {
        if (!name) return;

        const $input = form.find(`[name="${name}"], #${name}`).first();
        if (!$input.length) return;

        const id = $input.attr('id');

        if (kind === 'date') {
            $input.val('');
            $input.siblings('input[type="hidden"]').val('');
        } else if ((kind === 'multi' || kind === 'single') && id && window.SearchableControls) {
            window.SearchableControls.reset(id);
        } else {
            $input.val('');
            if ($input.closest('.ms-wrapper, .ss-wrapper').length && id && window.SearchableControls) {
                window.SearchableControls.reset(id);
            }
        }

        if (name === 'itemClassId') {
            if (window.SearchableControls) {
                window.SearchableControls.setOptions('itemId', []);
                window.SearchableControls.reset('itemId');
            }
        }

        updateActiveFilters();
    };

    const clearAll = () => {
        if (window.SearchableControls) {
            form.find('.ms-wrapper, .ss-wrapper').each(function () {
                const id = $(this).find('input[type="hidden"]').first().attr('id');
                if (id) window.SearchableControls.reset(id);
            });

            window.SearchableControls.setOptions('itemId', []);
        }

        form.find('input.datepicker-input').val('');
        form.find('input.f-control').not('.datepicker-input').val('');
        form.find('select.f-control').val('');
        form.find('input[type="hidden"]').not('[name="__RequestVerificationToken"]').val('');

        form.find('.btn-toggle-date-mode').each(function () {
            const $btn = $(this);
            const targetId = $btn.data('target');
            $('#' + targetId).attr('data-calendar-mode', 'jalali');
            $btn.attr('data-mode', 'jalali');
            $btn.find('.mode-label').text('شمسی');
        });

        updateActiveFilters();
        if (typeof showToast === 'function') showToast('فیلترها پاک شدند', 'success');
    };

    itemClassSelect.on('change', function () {
        loadItemsByClass($(this).val());
        scheduleUpdate();
    });

    tabs.on('click', function () { setReportType($(this).data('report')); });

    form.on('click', '.btn-toggle-date-mode', function (e) {
        e.preventDefault();
        switchCalendarMode($(this).data('target'));
    });

    form.on('click', '.js-clear-filters', function (e) {
        e.preventDefault();
        clearAll();
    });

    form.on('click', '.chip-remove', function () {
        const $chip = $(this).closest('.active-chip');
        removeFilterByName($chip.data('name'), $chip.data('kind'));
    });

    form.on('click', '.section-toggle', function () {
        const $section = $(this).closest('.filter-section');
        const $body = $section.find('.filter-section-body');
        const open = $section.attr('data-open') !== 'false';

        $section.attr('data-open', !open);
        $body.slideToggle(180);
        $(this).find('i').toggleClass('fa-chevron-up fa-chevron-down');
        $(this).attr('aria-expanded', !open);
    });

    let scheduled = false;
    const scheduleUpdate = () => {
        if (scheduled) return;
        scheduled = true;
        requestAnimationFrame(() => { scheduled = false; updateActiveFilters(); });
    };

    form.on('change', 'input, select', scheduleUpdate);
    form.on('input', 'input.f-control', scheduleUpdate);
    form.on('change', 'input[type="checkbox"]', scheduleUpdate);

    form.on('submit', function () {
        const $btn = form.find('.btn-primary-action');
        const original = $btn.html();
        $btn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i> در حال تهیه گزارش...');
        setTimeout(() => $btn.prop('disabled', false).html(original), 2500);
    });

    initDatePickers();
    setReportType(reportTypeInput.val() || 'arrivalByItem');
});