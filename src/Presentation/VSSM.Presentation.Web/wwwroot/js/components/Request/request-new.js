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
                $('#currentcount').val(data.currentcount);
                $('#arivcount').val(data.arivcount);
                $('#usedcount').val(data.usedcount);
                $('#inventoryHealth').val(0);
                $('#timeBetweenRequestAndArrival').val(0);
            });
    });

    /* ── submit overlay ── */
    $form.on('submit', function () {
        const overlay = document.createElement('div');
        overlay.className = 'page-overlay';
        document.body.appendChild(overlay);
    });

})();