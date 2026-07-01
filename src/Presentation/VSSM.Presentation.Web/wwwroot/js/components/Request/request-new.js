(function () {
    'use strict';

    const $form = $('#requestForm');
    const $itemClass = $('#ItemClassId');
    const $item = $('#ItemId');

    function setHasItem(val) {
        $form.attr('data-has-item', val ? 'true' : 'false');
    }

    function resetItemDropdown(placeholder) {
        $item.prop('disabled', true).empty().append(`<option value="">${placeholder}</option>`);
    }

    function clearComputedFields() {
        $('#arivcount, #currentcount, #usedcount, #inventoryHealth, #timeBetweenRequestAndArrival').val('');
    }

    /* ── تغییر نوع کالا → بارگذاری کالاها ── */
    $itemClass.on('change', function () {
        const classId = $(this).val();

        setHasItem(false);
        clearComputedFields();

        if (!classId) {
            resetItemDropdown('ابتدا نوع کالا را انتخاب کنید');
            return;
        }

        resetItemDropdown('در حال بارگذاری...');

        $.getJSON('/Request/GetItemsByClass', { classId })
            .done(function (data) {
                $item.empty().append('<option value="">انتخاب کنید...</option>');
                data.forEach(x => {
                    $item.append(`<option value="${x.id}">${x.text}</option>`);
                });
                $item.prop('disabled', false);
            });
    });

    /* ── تغییر کالا → بارگذاری جزئیات (تعداد ورودی / مصرف‌شده / موجودی انبار) ── */
    $item.on('change', function () {
        const itemId = $(this).val();
        setHasItem(!!itemId);

        if (!itemId) {
            clearComputedFields();
            return;
        }

        $.getJSON('/Request/GetItemDetailsByItem', { itemId })
            .done(function (data) {
                arivCount = data?.arivCount ?? 0;
                $('#currentcount').val(data.currentCount);
                $('#arivcount').val(data.arivCount);
                $('#usedcount').val(data.usedCount);
                $('#inventoryHealth').val(0);
                $('#timeBetweenRequestAndArrival').val(0);
            });
    });

    let arivCount = 0;

    function validateRequestCount() {
        const reqCount = parseInt($('#reqcount').val() || 0);
        const max = parseInt(arivCount || 0);

        if (reqCount > max) {
            $('#reqcount').addClass('is-invalid');

            if (!$('#reqcount').next('.error-msg').length) {
                $('#reqcount').after(
                    `<small class="text-danger error-msg">
                    تعداد درخواستی نمی‌تواند بیشتر از موجودی (${max}) باشد
                </small>`
                );
            }

            return false;
        } else {
            $('#reqcount').removeClass('is-invalid');
            $('#reqcount').next('.error-msg').remove();
            return true;
        }
    }

    $('#reqcount').on('input', function () {
        validateRequestCount();
    });



    $form.on('submit', function (e) {

        if (!validateRequestCount()) {
            e.preventDefault();
            return false;
        }

        // اگر jQuery Unobtrusive Validation روی فرم فعاله، اول خودمون
        // اعتبارسنجی رو چک می‌کنیم؛ اگر نامعتبره اسپینر رو نشون نمی‌دیم
        // چون submit در ادامه توسط validate() کنسل خواهد شد.
        if (typeof $form.valid === 'function' && !$form.valid()) {
            return; // اجازه بده پیام‌های خطای validate نمایش داده بشن
        }

        const overlay = document.createElement('div');
        overlay.className = 'page-overlay';
        document.body.appendChild(overlay);

        // ایمنی: اگر به هر دلیلی ناوبری انجام نشد (خطای شبکه و ...)
        // بعد از چند ثانیه اسپینر رو حذف کن تا صفحه گیر نکنه.
        setTimeout(() => overlay.remove(), 8000);
    });

})();