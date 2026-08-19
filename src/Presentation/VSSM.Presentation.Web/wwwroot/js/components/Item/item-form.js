(function () {
    const VACCINE_CLASS_ID = 54;
    const DILUENT_CLASS_ID = 55;

    let $itemClassSelect = $("#ItemClassId");
    let $requiresDiluent = $("#requiresDiluentCheckbox");
    let $diluentDropdown = $("#diluentDropdown");
    let $submitBtn = $(".btn-confirm");
    let $form = $("form");

    let $requiredFields = $("#ItemClassId, #FullName, #DayToWarmExpire");
    let isEditMode = $diluentDropdown.find("option").length > 1;
    let isSubmitting = false;

    function validateForm() {
        var isValid = true;

        $requiredFields.each(function () {
            var $field = $(this);
            var val = $field.val();

            if ($field.is("select")) {
                if (!val || val === "") {
                    isValid = false;
                    return false;
                }
            } else {
                if (!val || val.trim() === "") {
                    isValid = false;
                    return false;
                }
            }
        });

        $submitBtn.prop("disabled", !isValid);
        return isValid;
    }

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
                validateForm();
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
        validateForm();
    }

    $itemClassSelect.on("change", function () {
        if (parseInt($(this).val()) === VACCINE_CLASS_ID) {
            $requiresDiluent.prop("disabled", false);
        } else {
            clearDiluent();
        }
        validateForm();
    });

    $requiresDiluent.on("change", function () {
        if (this.checked) {
            var selectedDiluentId = $diluentDropdown.data("selected");
            loadDiluentItems(selectedDiluentId);
        } else {
            $diluentDropdown.prop("disabled", true).empty()
                .append('<option value="">-- انتخاب کنید --</option>');
            isEditMode = false;
            validateForm();
        }
    });

    $requiredFields.on("change keyup", function () {
        validateForm();
    });

    $form.on("submit", function (e) {
        if (isSubmitting) {
            e.preventDefault();
            return false;
        }

        if (!validateForm()) {
            e.preventDefault();
            showToast("لطفاً تمام فیلدهای اجباری را پر کنید", "warning");
            return false;
        }

        isSubmitting = true;
        $submitBtn.prop("disabled", true);
        $submitBtn.html('<i class="fa fa-spinner fa-spin"></i> در حال ثبت...');
    });

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
        validateForm();
    })();

})();