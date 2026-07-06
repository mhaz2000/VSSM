// ============================================================
// request-details.js  —  مدیریت صفحه جزئیات درخواست
// شامل: تایید/رد با مودال، اسکرول و دکمه «مشاهده همه»
// ============================================================

(function () {
    'use strict';

    // ── دکمه تایید ──
    const btnConfirm = document.getElementById('btnConfirm');
    if (btnConfirm) {
        btnConfirm.addEventListener('click', function () {
            if (typeof VSSM !== 'undefined' && VSSM.Modal) {
                VSSM.Modal.confirm({
                    title: 'تایید درخواست',
                    message: 'با تایید درخواست، نیاز می‌باشد یک درخواست جدید برای رده بالاتر ثبت نمایید. آیا ادامه می‌دهید؟',
                    confirmText: 'تایید',
                    cancelText: 'انصراف',
                    type: 'info',
                    onConfirm: function () {
                        document.getElementById('confirmForm').submit();
                    }
                });
            } else {
                if (confirm('با تایید درخواست، نیاز می‌باشد یک درخواست جدید برای رده بالاتر ثبت نمایید. آیا ادامه می‌دهید؟')) {
                    document.getElementById('confirmForm').submit();
                }
            }
        });
    }

    // ── دکمه رد ──
    const btnReject = document.getElementById('btnReject');
    if (btnReject) {
        btnReject.addEventListener('click', function () {
            if (typeof VSSM !== 'undefined' && VSSM.Modal) {
                VSSM.Modal.confirm({
                    title: 'رد درخواست',
                    message: 'آیا نسبت به رد درخواست مورد نظر مطمئنید؟',
                    confirmText: 'رد',
                    cancelText: 'انصراف',
                    type: 'danger',
                    onConfirm: function () {
                        document.getElementById('rejectForm').submit();
                    }
                });
            } else {
                if (confirm('آیا نسبت به رد درخواست مورد نظر مطمئنید؟')) {
                    document.getElementById('rejectForm').submit();
                }
            }
        });
    }
    function initialize() {
        initToggleButton();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

})();