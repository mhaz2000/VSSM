(function () {
    'use strict';

    function submitForm(formId) {
        var form = document.getElementById(formId);
        if (form) {
            form.submit();
        }
    }

    function bindConfirm(btnId, formId, options) {
        var btn = document.getElementById(btnId);
        if (!btn) {
            return;
        }

        btn.addEventListener('click', function () {
            if (typeof VSSM !== 'undefined' && VSSM.Modal) {
                VSSM.Modal.confirm({
                    title: options.title,
                    message: options.message,
                    confirmText: options.confirmText,
                    cancelText: 'انصراف',
                    type: options.type,
                    onConfirm: function () { submitForm(formId); }
                });
            } else if (window.confirm(options.message)) {
                submitForm(formId);
            }
        });
    }

    function initialize() {
        bindConfirm('btnConfirm', 'confirmForm', {
            title: 'تایید درخواست',
            message: 'با تایید درخواست، نیاز می‌باشد یک درخواست جدید برای رده بالاتر ثبت نمایید. آیا ادامه می‌دهید؟',
            confirmText: 'تایید',
            type: 'info'
        });

        bindConfirm('btnReject', 'rejectForm', {
            title: 'رد درخواست',
            message: 'آیا نسبت به رد درخواست مورد نظر مطمئنید؟',
            confirmText: 'رد',
            type: 'danger'
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }
})();