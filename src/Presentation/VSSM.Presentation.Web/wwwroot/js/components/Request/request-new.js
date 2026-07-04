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
        var $note = $('#note');
        $note.removeAttr('data-val-required');
        $note.removeAttr('data-val');
        $note.removeAttr('aria-required');
        $note.removeAttr('aria-describedby');

        if (typeof $.fn.rules === 'function') {
            try {
                $note.rules('remove', 'required');
            } catch (e) {
    
            }
        }

        $note.removeClass('input-validation-error');
        $note.siblings('.field-validation-valid, .field-validation-error').remove();
    }

    
    function disableFormValidation() {
        if ($form.data('validator')) {
            $form.removeData('validator');
        }
        if ($form.data('unobtrusiveValidation')) {
            $form.removeData('unobtrusiveValidation');
        }

        $form.off('submit.validate');
        $form.off('keyup.validate');
        $form.off('focusout.validate');

        $('button[formnovalidate]').prop('formnovalidate', true);
    }

    
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
                data.forEach(x => {
                    $item.append(`<option value="${x.id}">${x.text}</option>`);
                });
                $item.prop('disabled', false);
            })
            .always(function () {
                $form.removeClass('form-loading');
            });
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
                showToast('خطا در دریافت اطلاعات کالا. لطفاً دوباره تلاش کنید.', 'error');
            })
            .always(function () {
                $computedWrap.removeClass('is-loading');
                $computedLoading.hide();
                $form.removeClass('form-loading');
                updateAddButtonState();
            });
    });

    /* ── فقط اعداد مجاز هستند، بدون محدودیت عددی ── */
    $reqCount.on('input', function () {
        // فقط کاراکترهای غیرعددی را حذف میکنیم
        const val = this.value.replace(/[^\d]/g, '');
        if (this.value !== val) {
            this.value = val;
        }
        updateAddButtonState();
    });

    /* ── اجرا در زمان مناسب ── */
    $(document).ready(function () {
        removeNoteValidation();
        disableFormValidation();
    });

    /* ── رویداد submit ── */
    $form.on('submit', function (e) {
        console.log('🚀 Submit event triggered!');

        removeNoteValidation();
        disableFormValidation();

        if ($addBtn.is(':disabled')) {
            var command = $('button[type="submit"]:focus').val();
            if (command === 'addLine') {
                console.log('❌ Add button is disabled');
                e.preventDefault();
                return false;
            }
        }

        var command = $('button[type="submit"]:focus').val();
        console.log('📝 Command:', command);

        if (command === 'StatusDraft' || command === 'StatusSent') {
            console.log('✅ Draft or Send clicked, submitting...');
            const overlay = document.createElement('div');
            overlay.className = 'page-overlay';
            document.body.appendChild(overlay);
            setTimeout(() => overlay.remove(), 8000);
            return true;
        }

        console.log('✅ Add button clicked, submitting...');
        const overlay = document.createElement('div');
        overlay.className = 'page-overlay';
        document.body.appendChild(overlay);
        setTimeout(() => overlay.remove(), 8000);

        return true;
    });

    updateAddButtonState();
})();