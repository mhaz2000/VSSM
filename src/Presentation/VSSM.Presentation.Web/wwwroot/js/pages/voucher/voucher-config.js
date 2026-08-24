(() => {
    const $form = $("form");
    const $submitBtn = $(".btn-confirm");
    const $requiredCheckboxes = $(".voucher-item.required input[type='checkbox']");
    const $activeCheckboxes = $(".voucher-item:not(.required) input[type='checkbox']");
    let isSubmitting = false;

    const validateForm = () => {
        let isValid = true;
        $requiredCheckboxes.each((_, checkbox) => {
            const $checkbox = $(checkbox);
            if (!$checkbox.prop("checked")) {
                isValid = false;
                return false;
            }
        });
        $submitBtn.prop("disabled", !isValid);
        return isValid;
    };

    const toggleActiveClass = (checkbox) => {
        const $item = $(checkbox).closest(".voucher-item");
        if ($(checkbox).prop("checked")) {
            $item.addClass("active");
        } else {
            $item.removeClass("active");
        }
    };

    $requiredCheckboxes.on("click", (e) => {
        e.preventDefault();
        showToast("این فیلد اجباری است و نمی‌توان آن را غیرفعال کرد.", "warning");
        return false;
    });

    $activeCheckboxes.on("change", function () {
        toggleActiveClass(this);
        validateForm();
    });

    $form.on("submit", function (e) {
        if (isSubmitting) {
            e.preventDefault();
            return false;
        }

        if (!validateForm()) {
            e.preventDefault();
            showToast("فیلدهای اجباری نمی‌توانند غیرفعال شوند.", "warning");
            return false;
        }

        isSubmitting = true;

        setTimeout(() => {
            $submitBtn.prop("disabled", true);
            $submitBtn.html('<i class="fa fa-spinner fa-spin"></i> در حال ذخیره...');
        }, 100);
    });

    $activeCheckboxes.each(function () {
        if ($(this).prop("checked")) {
            $(this).closest(".voucher-item").addClass("active");
        }
    });

    validateForm();
})();