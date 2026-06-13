/**
 * item-form.js  —  منطق فرم ثبت/ویرایش کالا
 * پیش‌نیاز: jQuery، toast.js
 */
(function () {
    var VACCINE_CLASS_ID = 54;
    var DILUENT_CLASS_ID = 55;

    var $itemClassSelect = $("#ItemClassId");
    var $requiresDiluent = $("#requiresDiluentCheckbox");
    var $diluentDropdown = $("#diluentDropdown");

    function loadDiluentItems() {
        $diluentDropdown.prop("disabled", true).empty()
            .append('<option value="">در حال بارگذاری...</option>');

        $.getJSON("/GetItems/" + DILUENT_CLASS_ID)
            .done(function (res) {
                $diluentDropdown.empty()
                    .append('<option value="">-- انتخاب کنید --</option>');
                if (res.success && res.result) {
                    $.each(res.result, function (_, item) {
                        $diluentDropdown.append(
                            $('<option>', { value: item.id, text: item.fullName })
                        );
                    });
                }
                $diluentDropdown.prop("disabled", false);
            })
            .fail(function () {
                $diluentDropdown.empty()
                    .append('<option value="">خطا در بارگذاری</option>');
            });
    }

    function clearDiluent() {
        $diluentDropdown.prop("disabled", true).empty()
            .append('<option value="">-- انتخاب کنید --</option>');
        $requiresDiluent.prop({ checked: false, disabled: true });
    }

    $itemClassSelect.on("change", function () {
        if (parseInt($(this).val()) === VACCINE_CLASS_ID) {
            $requiresDiluent.prop("disabled", false);
        } else {
            clearDiluent();
        }
    });

    $requiresDiluent.on("change", function () {
        if (this.checked) {
            loadDiluentItems();
        } else {
            $diluentDropdown.prop("disabled", true).empty()
                .append('<option value="">-- انتخاب کنید --</option>');
        }
    });

    // مقداردهی اولیه (حالت ویرایش)
    (function init() {
        var currentClass = parseInt($itemClassSelect.val());
        if (currentClass === VACCINE_CLASS_ID) {
            $requiresDiluent.prop("disabled", false);
            if ($requiresDiluent.prop("checked")) {
                loadDiluentItems();
            }
        } else {
            $requiresDiluent.prop("disabled", true);
        }
    })();

})();