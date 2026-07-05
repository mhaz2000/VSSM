// ============================================================
// request-details.js - مدیریت صفحه جزئیات درخواست
// شامل: تایید/رد با مودال سفارشی، نمایش/مخفی کردن ردیف‌های اضافی
// ============================================================

(function () {
    'use strict';

    // ── دکمه تایید ──
    var btnConfirm = document.getElementById('btnConfirm');
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
    var btnReject = document.getElementById('btnReject');
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

    // ── دکمه «مشاهده همه» با Event Delegation ──
    function initToggleButton() {
        if (document.body.dataset.toggleDelegated === 'true') return;
        document.body.dataset.toggleDelegated = 'true';

        document.querySelectorAll('.hidden-line').forEach(function (line) {
            line.classList.add('rq-extra-line');
        });

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.btn-show-all-lines');
            if (!btn) return;
            e.preventDefault();

            var extraLines = document.querySelectorAll('.rq-extra-line');
            var wrap = document.querySelector('.lines-table-wrap');
            var icon = btn.querySelector('i');
            var text = btn.querySelector('.btn-text');

            var isExpanded = btn.dataset.expanded === 'true';
            var total = extraLines.length + 5;

            if (isExpanded) {
                extraLines.forEach(function (line) {
                    line.classList.add('hidden-line');
                    line.style.display = 'none';
                });
                btn.dataset.expanded = 'false';
                if (icon) icon.className = 'fa fa-chevron-down';
                if (text) text.textContent = 'مشاهده همه اقلام (' + total + ' قلم)';
                if (wrap) wrap.scrollTop = 0;
            } else {
                extraLines.forEach(function (line) {
                    line.classList.remove('hidden-line');
                    line.style.removeProperty('display');
                });
                btn.dataset.expanded = 'true';
                if (icon) icon.className = 'fa fa-chevron-up';
                if (text) text.textContent = 'مشاهده کمتر';
                setTimeout(function () {
                    if (wrap) wrap.scrollTop = wrap.scrollHeight;
                }, 100);
            }
        });
    }

    // ── اجرا پس از بارگذاری کامل DOM ──
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initToggleButton);
    } else {
        initToggleButton();
    }

})();