$(document).ready(function () {

    // =========================================================
    // ELEMENTS
    // =========================================================
    var $fileInput = $('#xmlFile');
    var $fileNameDisplay = $('#fileNameDisplay');
    var $removeBtn = $('#fileRemoveBtn');
    var $itemSelect = $('#ItemId');
    var $doseSelect = $('#DosePerVialId');
    var $uploadBtn = $('#uploadBtn');

    // =========================================================
    // FILE INPUT CHANGE
    // =========================================================
    $fileInput.on('change', function () {
        var files = this.files;
        if (files.length > 0) {
            var fileName = files[0].name;
            $fileNameDisplay.text(fileName).addClass('has-file');
            $removeBtn.removeClass('d-none');
        } else {
            resetFileInputDisplay();
        }
        checkFormValidity();
    });

    // =========================================================
    // REMOVE FILE BUTTON
    // =========================================================
    $removeBtn.on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        resetFileInput();
    });

    // =========================================================
    // RESET FILE INPUT (کامل)
    // =========================================================
    window.resetFileInput = function () {
        $fileInput.val('');
        resetFileInputDisplay();
        $itemSelect.val('').trigger('change');
        checkFormValidity();
    };

    // =========================================================
    // RESET DISPLAY (فقط نمایش)
    // =========================================================
    function resetFileInputDisplay() {
        $fileNameDisplay.text('هیچ فایلی انتخاب نشده است').removeClass('has-file');
        $removeBtn.addClass('d-none');
    }

    // =========================================================
    // LOAD DOSE PER VIAL BY ITEM
    // =========================================================
    function loadDosePerVial(itemId) {
        if (!itemId) {
            $doseSelect.prop('disabled', true)
                .html('<option value="">-- ابتدا کالا را انتخاب کنید --</option>');
            checkFormValidity();
            return;
        }

        $doseSelect.prop('disabled', false);

        $.ajax({
            url: '/GetItemDoesPerVials/' + itemId,
            type: 'GET',
            success: function (res) {
                if (res && res.success && res.result && res.result.length > 0) {
                    var options = '<option value="">-- انتخاب کنید --</option>';
                    $.each(res.result, function (i, item) {
                        options += '<option value="' + item.dosePerVialId + '">' + item.numDosePerVial + '</option>';
                    });
                    $doseSelect.html(options);
                } else {
                    $doseSelect.html('<option value="">-- هیچ دوزی یافت نشد --</option>');
                }
                checkFormValidity();
            },
            error: function () {
                $doseSelect.html('<option value="">-- خطا در بارگذاری دوزها --</option>');
                checkFormValidity();
            }
        });
    }

    // =========================================================
    // ITEM CHANGE EVENT
    // =========================================================
    $itemSelect.on('change', function () {
        var itemId = $(this).val();
        loadDosePerVial(itemId);
    });

    // =========================================================
    // DOSE CHANGE EVENT
    // =========================================================
    $doseSelect.on('change', function () {
        checkFormValidity();
    });

    // =========================================================
    // CHECK FORM VALIDITY
    // =========================================================
    function checkFormValidity() {
        var isFileSelected = $fileInput[0].files.length > 0;
        var isItemSelected = !!$itemSelect.val();
        var isDoseSelected = !!$doseSelect.val();

        var isValid = isFileSelected && isItemSelected && isDoseSelected;

        $uploadBtn
            .prop('disabled', !isValid)
            .attr('title', isValid ? '' : 'لطفاً فایل، کالا و دوز بر ویال را انتخاب کنید');

        if (!isValid) {
            $uploadBtn.find('.js-btn-spinner').attr('hidden', true);
        }

        return isValid;
    }

    // =========================================================
    // INITIAL LOAD
    // =========================================================
    var initialItemId = $itemSelect.val();
    if (initialItemId) {
        loadDosePerVial(initialItemId);
    } else {
        $doseSelect.prop('disabled', true)
            .html('<option value="">-- ابتدا کالا را انتخاب کنید --</option>');
    }

    // =========================================================
    // INITIAL VALIDITY CHECK
    // =========================================================
    checkFormValidity();

    // =========================================================
    // EXPOSE GLOBAL FUNCTIONS
    // =========================================================
    window.checkFormValidity = checkFormValidity;
    window.loadDosePerVial = loadDosePerVial;
    window.resetFileInput = resetFileInput;

    // =========================================================
    // SUBMIT BUTTON - SHOW LOADER INSIDE BUTTON
    // =========================================================
    $('#uploadForm').on('submit', function () {
        var $btn = $('#uploadBtn');

        if (!$btn.prop('disabled')) {
            $btn.prop('disabled', true);
            $btn.find('.js-btn-spinner').removeAttr('hidden');
        }
    });

    // =========================================================
    // =========================================================
    // CUSTOM MODAL (با استایل‌های modal.css)
    // =========================================================
    // =========================================================

    function showModal(options) {
        // حذف مودال قبلی اگر وجود دارد
        $('.vssm-modal-overlay').remove();

        var overlay = $('<div class="vssm-modal-overlay active"></div>');
        var box = $('<div class="vssm-modal-box"></div>');

        var header = $('<div class="vssm-modal-header"></div>');
        header.append('<i class="fa fa-exclamation-triangle"></i>');
        header.append('<h5>' + (options.title || 'تایید') + '</h5>');

        var body = $('<div class="vssm-modal-body"></div>');
        body.text(options.message || '');

        var footer = $('<div class="vssm-modal-footer"></div>');

        var confirmBtn = $('<button class="btn btn-confirm-modal">' + (options.confirmText || 'تایید') + '</button>');
        var cancelBtn = $('<button class="btn btn-cancel-modal">' + (options.cancelText || 'انصراف') + '</button>');

        if (options.type === 'danger') {
            confirmBtn.removeClass('btn-confirm-modal').addClass('btn-danger-modal');
        }

        confirmBtn.on('click', function () {
            overlay.remove();
            if (options.onConfirm) options.onConfirm();
        });

        cancelBtn.on('click', function () {
            overlay.remove();
            if (options.onCancel) options.onCancel();
        });

        footer.append(confirmBtn, cancelBtn);
        box.append(header, body, footer);
        overlay.append(box);
        $('body').append(overlay);

        // بستن مودال با کلیک روی overlay
        overlay.on('click', function (e) {
            if ($(e.target).is(overlay)) {
                overlay.remove();
                if (options.onCancel) options.onCancel();
            }
        });

        // بستن با دکمه ESC
        $(document).on('keydown.modal', function (e) {
            if (e.key === 'Escape') {
                overlay.remove();
                $(document).off('keydown.modal');
                if (options.onCancel) options.onCancel();
            }
        });
    }

    // =========================================================
    // CONFIRM MODAL - با استفاده از showModal
    // =========================================================
    $(document).on('click', '.confirm-read', function (e) {
        e.preventDefault();
        var $form = $(this).closest('form');

        showModal({
            title: 'تایید نهایی',
            message: 'UIDهای این فایل استعلام TTac ندارند. آیا از تایید آنها اطمینان دارید؟',
            confirmText: 'تایید',
            cancelText: 'انصراف',
            type: 'danger',
            onConfirm: function () {
                $form.submit();
            }
        });
    });

});