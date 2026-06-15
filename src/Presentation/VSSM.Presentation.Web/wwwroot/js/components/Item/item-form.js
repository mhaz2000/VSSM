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

    // اگه آپشن‌های حلال از قبل توی View لود شدن (حالت ویرایش)، دیگه از API لود نکن
    var isEditMode = $diluentDropdown.find("option").length > 1;

    function loadDiluentItems(preselectId) {
        if (isEditMode) {
            $diluentDropdown.prop("disabled", false);
            if (preselectId && preselectId !== "0") {
                $diluentDropdown.val(preselectId);
            }
            return;
        }

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
                if (preselectId && preselectId !== "0") {
                    $diluentDropdown.val(preselectId);
                }
                $diluentDropdown.prop("disabled", false);
                isEditMode = false;
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
        isEditMode = false;
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
            var selectedDiluentId = $diluentDropdown.data("selected");
            loadDiluentItems(selectedDiluentId);
        } else {
            $diluentDropdown.prop("disabled", true).empty()
                .append('<option value="">-- انتخاب کنید --</option>');
            isEditMode = false;
        }
    });

    // مقداردهی اولیه
    (function init() {
        var currentClass = parseInt($itemClassSelect.val());
        if (currentClass === VACCINE_CLASS_ID) {
            $requiresDiluent.prop("disabled", false);
            if ($requiresDiluent.prop("checked")) {
                $diluentDropdown.prop("disabled", false);
            }
        } else {
            $requiresDiluent.prop("disabled", true);
        }
    })();

})();