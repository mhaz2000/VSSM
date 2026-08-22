(function () {
    'use strict';

    const SELECTORS = {
        fullName: '#FullName',
        suggestedEndDate: '#SuggestedEndDate',
        submitBtn: '.btn-confirm',
        form: 'form'
    };

    const CLASSES = {
        inputValidationError: 'input-validation-error'
    };

    const MESSAGES = {
        fullNameRequired: 'لطفاً نام کامل را وارد کنید.',
        fullNameEmpty: 'لطفاً نام کامل را وارد کنید.'
    };

    const TOAST_TYPE = {
        error: 'error',
        success: 'success'
    };

    function getElement(selector) {
        return document.querySelector(selector);
    }

    function getElementById(id) {
        return document.getElementById(id);
    }

    function querySelectorAll(selector, context) {
        const targetContext = context || document;
        return targetContext.querySelectorAll(selector);
    }

    function addClass(element, className) {
        if (element) {
            element.classList.add(className);
        }
    }

    function removeClass(element, className) {
        if (element) {
            element.classList.remove(className);
        }
    }

    function showToast(message, type) {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
        }
    }

    function initializeDatePicker() {
        const input = getElementById('SuggestedEndDate');

        if (!input) {
            return;
        }

        if (typeof DatePicker !== 'undefined' && typeof $ === 'function') {
            DatePicker.attach($(input), 'jalali');
        }
    }

    function validateFullName() {
        const fullName = getElement(SELECTORS.fullName);
        const submitBtn = getElement(SELECTORS.submitBtn);
        const errorSpan = getElementById('fullNameError');

        if (!fullName || !submitBtn) {
            return;
        }

        function validate() {
            const value = fullName.value.trim();
            const isValid = value.length > 0;

            if (isValid) {
                removeClass(fullName, CLASSES.inputValidationError);
                if (errorSpan) {
                    errorSpan.textContent = '';
                }
                submitBtn.disabled = false;
            } else {
                addClass(fullName, CLASSES.inputValidationError);
                if (errorSpan) {
                    errorSpan.textContent = MESSAGES.fullNameRequired;
                }
                submitBtn.disabled = true;
            }

            return isValid;
        }

        fullName.addEventListener('input', validate);
        fullName.addEventListener('blur', validate);

        validate();
    }

    function handleFormSubmit(event) {
        const fullName = getElement(SELECTORS.fullName);

        if (!fullName || fullName.value.trim() === '') {
            event.preventDefault();
            showToast(MESSAGES.fullNameEmpty, TOAST_TYPE.error);
            fullName.focus();
            return false;
        }

        const inputs = querySelectorAll('input[type="text"]', event.target);
        const length = inputs.length;

        for (let index = 0; index < length; index++) {
            const input = inputs[index];
            if (input.value) {
                input.value = input.value.trim();
            }
        }

        return true;
    }

    function initializeForm() {
        const form = getElement(SELECTORS.form);

        if (!form) {
            return;
        }

        form.addEventListener('submit', handleFormSubmit);
    }

    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                initializeDatePicker();
                validateFullName();
                initializeForm();
            });
        } else {
            initializeDatePicker();
            validateFullName();
            initializeForm();
        }
    }

    window.FiscalPeriodForm = {
        init: init,
        showToast: showToast
    };

    init();
})();