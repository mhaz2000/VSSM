(function () {
    'use strict';

    function showToast(message, type) {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
            return;
        }
        console.warn('Toast function not available. Message:', message);
    }

    function initializeDatePicker() {
        var input = document.getElementById('SuggestedEndDate');

        if (!input) {
            return;
        }

        if (typeof DatePicker !== 'undefined') {
            DatePicker.attach($(input), 'jalali');
        }
    }

    function validateForm() {
        var fullName = document.getElementById('FullName');
        var submitBtn = document.querySelector('.btn-confirm');
        var errorSpan = document.getElementById('fullNameError');

        if (!fullName || !submitBtn) {
            return;
        }

        function validate() {
            var value = fullName.value.trim();
            var isValid = value.length > 0;

            if (isValid) {
                fullName.classList.remove('input-validation-error');
                if (errorSpan) {
                    errorSpan.textContent = '';
                }
                submitBtn.disabled = false;
            } else {
                fullName.classList.add('input-validation-error');
                if (errorSpan) {
                    errorSpan.textContent = 'لطفاً نام کامل را وارد کنید.';
                }
                submitBtn.disabled = true;
            }

            return isValid;
        }

        fullName.addEventListener('input', validate);
        fullName.addEventListener('blur', validate);

        validate();
    }

    function initializeForm() {
        var form = document.querySelector('form');

        if (!form) {
            return;
        }

        form.addEventListener('submit', function (e) {
            var fullName = document.getElementById('FullName');

            if (!fullName || fullName.value.trim() === '') {
                e.preventDefault();
                showToast('لطفاً نام کامل را وارد کنید.', 'error');
                fullName.focus();
                return false;
            }

            var inputs = form.querySelectorAll('input[type="text"]');
            inputs.forEach(function (input) {
                if (input.value) {
                    input.value = input.value.trim();
                }
            });
        });
    }

    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                initializeDatePicker();
                validateForm();
                initializeForm();
            });
        } else {
            initializeDatePicker();
            validateForm();
            initializeForm();
        }
    }

    window.FiscalPeriodForm = {
        init: init,
        showToast: showToast
    };

    init();
})();