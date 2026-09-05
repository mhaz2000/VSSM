(function () {
    'use strict';

    const VACCINE_CLASS_ID = 54;
    const DILUENT_CLASS_ID = 55;

    let $itemClassSelect = $("#ItemClassId");
    let $code = $("#Code");
    let $fullName = $("#FullName");
    let $dayToWarmExpire = $("#DayToWarmExpire");
    let $requiresDiluent = $("#requiresDiluentCheckbox");
    let $diluentDropdown = $("#diluentDropdown");
    let $diluentGroup = $("#diluentGroup");
    let $diluentRequiredTag = $("#diluentRequiredTag");
    let $diluentOptionalTag = $("#diluentOptionalTag");
    let $submitBtn = $("#submitBtn");
    let $form = $("#itemForm");

    let $requiredFields = $("#ItemClassId, #Code, #FullName, #DayToWarmExpire");
    let isEditMode = $diluentDropdown.find("option").length > 1;
    let isSubmitting = false;

    function validateForm() {
        var isValid = true;
        var $fields = $requiredFields;

        if ($requiresDiluent.prop("checked")) {
            $fields = $requiredFields.add($diluentDropdown);
        }

        $fields.each(function () {
            var $field = $(this);
            var val = $field.val();
            var $group = $field.closest('.f-group');
            var isValidField = true;

            if ($field.is("select")) {
                if (!val || val === "" || val === "0") {
                    isValidField = false;
                }
            } else {
                if (!val || val.trim() === "") {
                    isValidField = false;
                }
            }

            if (!isValidField) {
                isValid = false;
                $group.addClass('error');
            } else {
                $group.removeClass('error');
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
        $diluentRequiredTag.hide();
        $diluentOptionalTag.show();
        $diluentGroup.removeClass('required-field');
        isEditMode = false;
        validateForm();
    }

    $("#DayToWarmExpire").on("keypress", function (e) {
        var charCode = e.which ? e.which : e.keyCode;
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
            return false;
        }
        return true;
    });

    $("#DayToWarmExpire").on("paste", function (e) {
        var pastedText = (e.originalEvent || e).clipboardData.getData('text');
        if (!/^\d+$/.test(pastedText)) {
            e.preventDefault();
            showToast("فقط عدد وارد نمایید", "warning");
            return false;
        }
    });

    $itemClassSelect.on("change", function () {
        if (parseInt($(this).val()) === VACCINE_CLASS_ID) {
            $requiresDiluent.prop("disabled", false);
        } else {
            clearDiluent();
        }
        validateForm();
    });

    $requiresDiluent.on("change", function () {
        var $diluentDropdown = $("#diluentDropdown");

        if (this.checked) {
            $diluentRequiredTag.show();
            $diluentOptionalTag.hide();
            $diluentGroup.addClass('required-field');

            var selectedDiluentId = $diluentDropdown.data("selected");
            loadDiluentItems(selectedDiluentId);
            $diluentDropdown.prop("disabled", false);
        } else {
            $diluentRequiredTag.hide();
            $diluentOptionalTag.show();
            $diluentGroup.removeClass('required-field');

            $diluentDropdown.prop("disabled", true).empty()
                .append('<option value="">-- انتخاب کنید --</option>');
            isEditMode = false;
        }
        validateForm();
    });

    $requiredFields.on("change keyup", function () {
        validateForm();
    });

    $diluentDropdown.on("change", function () {
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


        if ($requiresDiluent.prop("checked")) {
            var diluentVal = $diluentDropdown.val();
            if (!diluentVal || diluentVal === "" || diluentVal === "0") {
                e.preventDefault();
                showToast("لطفاً حلال را انتخاب کنید", "warning");
                $diluentDropdown.focus();
                return false;
            }
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
                $diluentRequiredTag.show();
                $diluentOptionalTag.hide();
                $diluentGroup.addClass('required-field');
                $diluentDropdown.prop("disabled", false);
            }
        } else {
            $requiresDiluent.prop("disabled", true);
        }
        validateForm();
    })();

})();