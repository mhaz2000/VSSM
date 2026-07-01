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
        const reqValid = reqFilled && reqVal <= maxAllowed;

        const canEnable = classSelected && itemSelected && hasStock && reqValid;
        $addBtn.prop('disabled', !canEnable);
    }

    /* ── حذف کامل اعتبارسنجی از فیلد Note ── */
    function removeNoteValidation() {
        var $note = $('#note');

        // حذف attribute های اعتبارسنجی
        $note.removeAttr('data-val-required');
        $note.removeAttr('data-val');
        $note.removeAttr('aria-required');
        $note.removeAttr('aria-describedby');

        // حذف قانون required از jQuery Validation
        if (typeof $.fn.rules === 'function') {
            try {
                $note.rules('remove', 'required');
            } catch (e) {
                // خطا را نادیده بگیرید
            }
        }

        // حذف کلاس‌های خطا
        $note.removeClass('input-validation-error');
        $note.siblings('.field-validation-valid, .field-validation-error').remove();
    }

    /* ── غیرفعال کردن کامل اعتبارسنجی فرم ── */
    function disableFormValidation() {
        // حذف validator از فرم
        if ($form.data('validator')) {
            $form.removeData('validator');
        }
        if ($form.data('unobtrusiveValidation')) {
            $form.removeData('unobtrusiveValidation');
        }

        // حذف event handler های اعتبارسنجی
        $form.off('submit.validate');
        $form.off('keyup.validate');
        $form.off('focusout.validate');

        // غیرفعال کردن اعتبارسنجی برای دکمه‌های خاص
        $('button[formnovalidate]').prop('formnovalidate', true);
    }

    /* ── تغییر نوع کالا → بارگذاری کالاها ── */
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

    /* ── تغییر کالا → بارگذاری جزئیات ── */
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

                if (maxAllowed <= 0) {
                    hasStock = false;
                    showToast('موجودی این کالا صفر است و امکان ثبت درخواست برای آن وجود ندارد.', 'error');
                    $reqCount.val('').prop('disabled', true);
                } else {
                    hasStock = true;
                    $reqCount.prop('disabled', false);
                }
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

    /* ── جلوگیری از تایپ بیشتر از سقف مجاز ── */
    function clampReqCount() {
        let val = $reqCount.val().replace(/[^\d]/g, '');
        if (val !== '' && maxAllowed > 0) {
            let num = parseInt(val, 10);
            if (num > maxAllowed) {
                num = maxAllowed;
                val = String(num);
            }
        }
        if ($reqCount.val() !== val) {
            $reqCount.val(val);
        }
    }

    $reqCount.on('input', function () {
        clampReqCount();
        updateAddButtonState();
    });

    /* ── اجرا در زمان مناسب ── */
    $(document).ready(function () {
        // حذف اعتبارسنجی Note
        removeNoteValidation();

        // غیرفعال کردن اعتبارسنجی فرم
        disableFormValidation();
    });

    /* ── رویداد submit ── */
    $form.on('submit', function (e) {
        console.log('🚀 Submit event triggered!');

        // حذف کامل اعتبارسنجی
        removeNoteValidation();
        disableFormValidation();

        // اگر دکمه افزودن غیرفعال است، اجازه ندهید (فقط برای دکمه افزودن)
        if ($addBtn.is(':disabled')) {
            // بررسی کنید کدام دکمه کلیک شده است
            var command = $('button[type="submit"]:focus').val();
            if (command === 'addLine') {
                console.log('❌ Add button is disabled');
                e.preventDefault();
                return false;
            }
        }

        // اگر دکمه پیش‌نویس یا ارسال است، اجازه ارسال دهید
        var command = $('button[type="submit"]:focus').val();
        console.log('📝 Command:', command);

        if (command === 'StatusDraft' || command === 'StatusSent') {
            console.log('✅ Draft or Send clicked, submitting...');
            // نمایش overlay
            const overlay = document.createElement('div');
            overlay.className = 'page-overlay';
            document.body.appendChild(overlay);
            setTimeout(() => overlay.remove(), 8000);
            return true;
        }

        // برای دکمه افزودن، اعتبارسنجی معمولی
        console.log('✅ Add button clicked, submitting...');
        const overlay = document.createElement('div');
        overlay.className = 'page-overlay';
        document.body.appendChild(overlay);
        setTimeout(() => overlay.remove(), 8000);

        return true;
    });

    updateAddButtonState();
})();