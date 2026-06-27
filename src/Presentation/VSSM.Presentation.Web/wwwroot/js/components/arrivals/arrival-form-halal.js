/**
 * arrival-form-halal.js
 * مدیریت سوئیچ بین تب واکسن و حلال در فرم مدیریت ورودی
 */

(function () {

    var HALAL_CLASS_ID = 55;

    var $tabVaccine = $('#tabVaccine');
    var $tabHalal = $('#tabHalal');
    var $arrivalTypeHidden = $('#ArrivalTypeHidden');

    // فیلدهای سری ساخت
    var $batchNumGroup = $('#batchNumGroup');
    var $batchNumInput = $('#BatchNum');

    // نمایش نوع کالا
    var $itemClassDisplay = $('#ItemClassDisplay');

    // کالا: واکسن=text، حلال=select
    var $itemDisplay = $('#ItemDisplay');
    var $halalItemSelect = $('#HalalItemSelect');

    // دوز بر ویال: واکسن=text، حلال=select
    var $doseDisplay = $('#DoseDisplay');
    var $halalDoseSelect = $('#HalalDoseSelect');

    // فیلدهای تعداد حلال
    var $halalQtyFields = $('#halalQtyFields');
    var $dosePerVialInput = $('#DosePerVialInput');
    var $quantityInput = $('#QuantityInput');
    var $numVialsInput = $('#NumVialsInput');
    var $quantityHidden = $('#QuantityHidden');

    // تاریخ
    setTimeout(function () {
        DatePicker.attach($('#HalalProdDate'));
        DatePicker.attach($('#HalalExpDate'));
    }, 50);

    // ========================
    // سوئیچ به واکسن
    // ========================
    function switchToVaccine() {
        $tabVaccine.addClass('active');
        $tabHalal.removeClass('active');
        $arrivalTypeHidden.val('vaccine');

        // سری ساخت: نمایش و اجباری
        $batchNumGroup.show();
        $batchNumInput.prop('required', true);

        // نوع کالا
        $itemClassDisplay.val('واکسن');

        // کالا: text فعال، select مخفی
        $itemDisplay.show();
        $halalItemSelect.hide().prop('disabled', true).val('');
        $('#itemAutoLabel').text('(خودکار)');

        // دوز: text فعال، select مخفی
        $doseDisplay.show();
        $halalDoseSelect.hide().prop('disabled', true).val('');
        $('#doseAutoLabel').text('(خودکار)');

        // فیلدهای تعداد حلال: مخفی
        $halalQtyFields.hide();
        $dosePerVialInput.val('').prop('required', false);
        $quantityInput.val('').prop('required', false);
        $numVialsInput.val('');
        $quantityHidden.val('');

        // تاریخ
        $vaccineDateSection.show();
        $halalDateSection.hide();
    }

    // ========================
    // سوئیچ به حلال
    // ========================
    function switchToHalal() {
        $tabHalal.addClass('active');
        $tabVaccine.removeClass('active');
        $arrivalTypeHidden.val('halal');

        // سری ساخت: مخفی و غیراجباری
        $batchNumGroup.hide();
        $batchNumInput.prop('required', false).val('');

        // نوع کالا
        $itemClassDisplay.val('حلال');

        // کالا: select فعال، text مخفی
        $itemDisplay.hide();
        $halalItemSelect.show().prop('disabled', false);
        $('#itemAutoLabel').text('(اجباری)');

        // دوز: select فعال، text مخفی
        $doseDisplay.hide();
        $halalDoseSelect.show().prop('disabled', false);
        $('#doseAutoLabel').text('(اجباری)');

        // فیلدهای تعداد حلال: نمایش و اجباری
        $halalQtyFields.show();
        $dosePerVialInput.prop('required', true);
        $quantityInput.prop('required', true);

        // تاریخ
        $vaccineDateSection.hide();
        $halalDateSection.show();
        DatePicker.attach($('#HalalProdDate'));
        DatePicker.attach($('#HalalExpDate'));

        // بارگذاری کالاهای حلال
        loadHalalItems();
    }

    // ========================
    // محاسبه تعداد ویال = تعداد ÷ دوز بر ویال
    // و sync به hidden quantity
    // ========================
    function calcNumVials() {
        var qty = parseInt($quantityInput.val()) || 0;
        var dose = parseInt($dosePerVialInput.val()) || 0;

        $quantityHidden.val(qty > 0 ? qty : '');

        if (qty > 0 && dose > 0) {
            $numVialsInput.val(Math.floor(qty / dose));
        } else {
            $numVialsInput.val('');
        }
    }

    // وقتی کاربر مستقیم numVials رو عوض کنه → quantity رو محاسبه کن
    function calcQuantityFromVials() {
        var vials = parseInt($numVialsInput.val()) || 0;
        var dose = parseInt($dosePerVialInput.val()) || 0;
        if (vials > 0 && dose > 0) {
            var qty = vials * dose;
            $quantityInput.val(qty);
            $quantityHidden.val(qty);
        }
    }

    // ========================
    // بارگذاری کالاهای حلال
    // ========================
    function loadHalalItems() {
        if (typeof arrivalGetHalalItemsUrl === 'undefined' || !arrivalGetHalalItemsUrl) return;

        $halalItemSelect.html('<option value="">در حال بارگذاری...</option>').prop('disabled', true);

        $.get(arrivalGetHalalItemsUrl, { classId: HALAL_CLASS_ID }, function (data) {
            $halalItemSelect.html('<option value="">انتخاب کنید...</option>').prop('disabled', false);
            if (data && data.items) {
                $.each(data.items, function (_, item) {
                    $halalItemSelect.append($('<option>').val(item.id).text(item.name));
                });
            }
        });
    }

    // ========================
    // بارگذاری دوزهای حلال بر اساس کالا
    // ========================
    function loadHalalDoses(itemId) {
        if (!itemId || typeof arrivalGetHalalDosesUrl === 'undefined' || !arrivalGetHalalDosesUrl) return;

        $halalDoseSelect.html('<option value="">در حال بارگذاری...</option>').prop('disabled', true);

        $.get(arrivalGetHalalDosesUrl, { itemId: itemId }, function (data) {
            $halalDoseSelect.html('<option value="">انتخاب کنید...</option>').prop('disabled', false);
            if (data && data.doses) {
                $.each(data.doses, function (_, dose) {
                    $halalDoseSelect.append($('<option>').val(dose.id).text(dose.name));
                });
            }
        });
    }

    // ========================
    // Events
    // ========================
    $tabVaccine.on('click', function () {
        if (!$(this).hasClass('active')) switchToVaccine();
    });

    $tabHalal.on('click', function () {
        if (!$(this).hasClass('active')) switchToHalal();
    });

    // محاسبه numVials وقتی quantity یا dosePerVial تغییر کنه
    $quantityInput.on('input', calcNumVials);
    $dosePerVialInput.on('input', function () {
        // اگه quantity پر بود، numVials رو محاسبه کن
        if ($quantityInput.val()) {
            calcNumVials();
        } else if ($numVialsInput.val()) {
            // اگه numVials پر بود، quantity رو محاسبه کن
            calcQuantityFromVials();
        }
    });

    // وقتی کاربر مستقیم numVials رو وارد کنه
    $numVialsInput.on('input', calcQuantityFromVials);

    // بارگذاری دوزها وقتی کالا انتخاب شد
    $halalItemSelect.on('change', function () {
        loadHalalDoses($(this).val());
    });

    // ========================
    // init
    // ========================
    $(function () {
        switchToVaccine();
    });

})();