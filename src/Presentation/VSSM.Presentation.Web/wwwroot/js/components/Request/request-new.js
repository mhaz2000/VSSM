(function () {
    'use strict';

    const $form = $('#requestForm');
    const $itemClass = $('#ItemClassId');
    const $item = $('#ItemId');

    function setHasItem(val) {
        $form.attr('data-has-item', val ? 'true' : 'false');
    }

    /* ── تغییر نوع کالا → بارگذاری کالاها ── */
    $itemClass.on('change', function () {
        const classId = $(this).val();

        setHasItem(false);
        $item.prop('disabled', true).empty();

        if (!classId) {
            $item.append('<option value="">ابتدا نوع کالا را انتخاب کنید</option>');
            return;
        }

        $.getJSON('/Request/GetItemsByClass', { classId })
            .done(function (data) {
                $item.append('<option value="">انتخاب کنید...</option>');
                data.forEach(x => {
                    $item.append(`<option value="${x.id}">${x.text}</option>`);
                });
                $item.prop('disabled', false);
            });
    });

    /* ── تغییر کالا → بارگذاری جزئیات ── */
    $item.on('change', function () {
        setHasItem(!!$(this).val());

        const itemId = $(this).val();
        if (!itemId) return;

        $.getJSON('/Request/GetItemDetailsByItem', { itemId })
            .done(function (data) {
                $('#anncount').val(data.anncount);
                $('#currentcount').val(data.currentcount);
                $('#arivcount').val(data.arivcount);
                $('#usedcount').val(data.usedcount);
                $('#offercount').val(data.offercount);
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