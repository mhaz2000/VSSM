(function () {
    'use strict';

    const $form = $('#requestForm');
    const $itemClass = $('#ItemClassId');
    const $item = $('#ItemId');
    const $reqCount = $('#reqcount');
    const $addBtn = $('#btnAddLine');
    const $computedWrap = $('#computedFieldsWrap');
    const $computedLoading = $('#computedLoading');

    let maxAllowed = 0;
    let hasStock = false;

    function setHasItem(val) {
        $form.attr('data-has-item', val ? 'true' : 'false');
    }

    function resetItemDropdown(placeholder) {
        $item.prop('disabled', true).empty().append(`<option value="">${placeholder}</option>`);
    }

    function clearComputedFields() {
        $('#arivcount, #currentcount, #usedcount, #inventoryHealth, #timeBetweenRequestAndArrival').val('');
    }

    function updateAddButtonState() {
        const classSelected = !!$itemClass.val();
        const itemSelected = !!$item.val();
        const reqVal = parseInt($reqCount.val() || 0, 10);
        const reqFilled = $reqCount.val() !== '' && !isNaN(reqVal) && reqVal > 0;

        const canEnable = classSelected && itemSelected && reqFilled;
        $addBtn.prop('disabled', !canEnable);
    }

    function removeNoteValidation() {
        const $note = $('#note');
        $note.removeAttr('data-val-required data-val aria-required aria-describedby');
        if (typeof $.fn.rules === 'function') {
            try { $note.rules('remove', 'required'); } catch (_) { }
        }
        $note.removeClass('input-validation-error');
        $note.siblings('.field-validation-valid, .field-validation-error').remove();
    }

    function disableFormValidation() {
        if ($form.data('validator')) $form.removeData('validator');
        if ($form.data('unobtrusiveValidation')) $form.removeData('unobtrusiveValidation');
        $form.off('submit.validate keyup.validate focusout.validate');
        $('button[formnovalidate]').prop('formnovalidate', true);
    }

    // ── تنظیم dropdownها از آخرین ردیف ──
    function setSelectsFromLastLine() {
        // فقط در حالت ایجاد جدید (غیر ویرایش) اجرا شود
        if ($form.data('is-edit') === 'true') return;

        const $rows = $('tbody tr:not(.more-lines-trigger)');
        if ($rows.length === 0) return;

        const $lastRow = $rows.last();
        const classId = $lastRow.data('item-class-id');
        const itemId = $lastRow.data('item-id');
        if (!classId || !itemId) return;

        // اگر dropdown نوع کالا از قبل مقدار دارد، کاری نکن (از مقدار موجود استفاده کن)
        if ($itemClass.val()) return;

        // تنظیم نوع کالا
        $itemClass.val(classId).trigger('change');

        // پس از بارگذاری آیتم‌ها، کالا را انتخاب کن
        function waitForItemsAndSet() {
            if ($item.prop('disabled') === false && $item.find('option[value="' + itemId + '"]').length) {
                $item.val(itemId).trigger('change');
                $reqCount.val(''); // خالی کردن تعداد برای ورود جدید
                updateAddButtonState();
            } else {
                setTimeout(waitForItemsAndSet, 200);
            }
        }
        setTimeout(waitForItemsAndSet, 300);
    }

    // ── دکمه نمایش همه (تغییر سایز) ──
    function initToggleButton() {
        if (document.body.dataset.toggleDelegatedNew === 'true') return;
        document.body.dataset.toggleDelegatedNew = 'true';

        document.querySelectorAll('.hidden-line').forEach(line => line.classList.add('rq-extra-line'));

        document.addEventListener('click', function (e) {
            const btn = e.target.closest('.btn-show-all-lines');
            if (!btn) return;
            e.preventDefault();

            const extraLines = document.querySelectorAll('.rq-extra-line');
            const wrap = document.querySelector('.lines-table-wrap');
            const icon = btn.querySelector('i');
            const text = btn.querySelector('.btn-text');

            const isExpanded = btn.dataset.expanded === 'true';
            const total = extraLines.length + 5;

            if (isExpanded) {
                extraLines.forEach(line => {
                    line.classList.add('hidden-line');
                    line.style.display = 'none';
                });
                btn.dataset.expanded = 'false';
                if (icon) icon.className = 'fa fa-chevron-down';
                if (text) text.textContent = `مشاهده همه اقلام (${total} قلم)`;
                if (wrap) wrap.scrollTop = 0;
            } else {
                extraLines.forEach(line => {
                    line.classList.remove('hidden-line');
                    line.style.removeProperty('display');
                });
                btn.dataset.expanded = 'true';
                if (icon) icon.className = 'fa fa-chevron-up';
                if (text) text.textContent = 'مشاهده کمتر';
                setTimeout(() => { if (wrap) wrap.scrollTop = wrap.scrollHeight; }, 100);
            }
        });
    }

    // ── رویدادهای فرم ──
    $itemClass.on('change', function () {
        const classId = $(this).val();
        setHasItem(false);
        clearComputedFields();
        maxAllowed = 0;
        hasStock = false;
        updateAddButtonState();

        if (!classId) {
            resetItemDropdown('ابتدا نوع کالا را انتخاب کنید');
            return;
        }

        resetItemDropdown('در حال بارگذاری...');
        $form.addClass('form-loading');

        $.getJSON('/Request/GetItemsByClass', { classId })
            .done(function (data) {
                $item.empty().append('<option value="">انتخاب کنید...</option>');
                data.forEach(x => $item.append(`<option value="${x.id}">${x.text}</option>`));
                $item.prop('disabled', false);
            })
            .always(() => $form.removeClass('form-loading'));
    });

    $item.on('change', function () {
        const itemId = $(this).val();
        setHasItem(!!itemId);
        maxAllowed = 0;
        hasStock = false;

        if (!itemId) {
            clearComputedFields();
            updateAddButtonState();
            return;
        }

        clearComputedFields();
        $computedWrap.addClass('is-loading');
        $computedLoading.css('display', 'flex');
        $form.addClass('form-loading');

        $.getJSON('/Request/GetItemDetailsByItem', { itemId })
            .done(function (data) {
                maxAllowed = parseInt(data?.arivCount ?? 0, 10);
                $('#currentcount').val(data.currentCount);
                $('#arivcount').val(data.arivCount);
                $('#usedcount').val(data.usedCount);
                $('#inventoryHealth').val(0);
                $('#timeBetweenRequestAndArrival').val(0);
                $reqCount.prop('disabled', false);
            })
            .fail(function () {
                if (typeof showToast === 'function') {
                    showToast('خطا در دریافت اطلاعات کالا. لطفاً دوباره تلاش کنید.', 'error');
                }
            })
            .always(function () {
                $computedWrap.removeClass('is-loading');
                $computedLoading.hide();
                $form.removeClass('form-loading');
                updateAddButtonState();
            });
    });

    $reqCount.on('input', function () {
        const val = this.value.replace(/[^\d]/g, '');
        if (this.value !== val) this.value = val;
        updateAddButtonState();
    });

    $form.on('submit', function (e) {
        removeNoteValidation();
        disableFormValidation();

        if ($addBtn.is(':disabled')) {
            const command = $('button[type="submit"]:focus').val();
            if (command === 'addLine') {
                e.preventDefault();
                return false;
            }
        }

        const command = $('button[type="submit"]:focus').val();

        if (command === 'StatusDraft' || command === 'StatusSent' || command === 'StatusDelete' || command?.startsWith('deleteLine_')) {
            const overlay = document.createElement('div');
            overlay.className = 'page-overlay';
            document.body.appendChild(overlay);
            setTimeout(() => overlay.remove(), 8000);
            return true;
        }

        if (command === 'addLine') {
            const overlay = document.createElement('div');
            overlay.className = 'page-overlay';
            document.body.appendChild(overlay);
            setTimeout(() => overlay.remove(), 8000);
            return true;
        }

        return true;
    });

    function initialize() {
        removeNoteValidation();
        disableFormValidation();
        updateAddButtonState();
        initToggleButton();
        setSelectsFromLastLine(); // <-- فراخوانی جدید
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

})();