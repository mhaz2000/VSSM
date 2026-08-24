(() => {
    const $form = $("form");
    const $submitBtn = $(".btn-confirm");
    const $inputs = $(".f-control[type='number']");
    let isSubmitting = false;

    const validateForm = () => {
        let isValid = true;

        $inputs.each((_, input) => {
            const $input = $(input);
            const val = parseInt($input.val(), 10);

            if (isNaN(val) || val < 0 || val > 100) {
                isValid = false;
                $input.addClass("is-invalid");
            } else {
                $input.removeClass("is-invalid");
            }
        });

        $submitBtn.prop("disabled", !isValid);
        return isValid;
    };

    const showFieldError = (input, message) => {
        const $input = $(input);
        const $error = $input.closest(".f-group").find(".field-error");
        if ($error.length) {
            $error.text(message);
        }
        $input.addClass("is-invalid");
    };

    const clearFieldErrors = () => {
        $(".field-error").text("");
        $inputs.removeClass("is-invalid");
    };

    $inputs.on("input", function () {
        const val = parseInt($(this).val(), 10);
        const $error = $(this).closest(".f-group").find(".field-error");

        if (!isNaN(val) && val > 100) {
            showFieldError(this, "مقدار نمی‌تواند بیشتر از ۱۰۰ باشد");
        } else if (!isNaN(val) && val < 0) {
            showFieldError(this, "مقدار نمی‌تواند کمتر از ۰ باشد");
        } else {
            $error.text("");
            $(this).removeClass("is-invalid");
        }

        validateForm();
    });

    $form.on("submit", function (e) {
        if (isSubmitting) {
            e.preventDefault();
            return false;
        }

        clearFieldErrors();

        if (!validateForm()) {
            e.preventDefault();
            showToast("لطفاً مقادیر معتبر (۰ تا ۱۰۰) وارد کنید.", "warning");
            return false;
        }

        isSubmitting = true;
        $submitBtn.prop("disabled", true);
        $submitBtn.html('<i class="fa fa-spinner fa-spin"></i> در حال ذخیره...');
    });

    validateForm();
})();