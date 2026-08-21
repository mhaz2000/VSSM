(function () {
    'use strict';

    const SELECTORS = {
        requiredCheckbox: '.settings-item.required input[type="checkbox"]',
        activeCheckbox: '.settings-item:not(.disabled) input[type="checkbox"]',
        form: 'form',
        textInputs: 'input[type="text"], input[type="email"], input[type="tel"]'
    };

    const CLASSES = {
        active: 'active'
    };

    function querySelectorAll(selector, context) {
        context = context || document;
        return context.querySelectorAll(selector);
    }

    function querySelector(selector, context) {
        context = context || document;
        return context.querySelector(selector);
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

    function closest(element, selector) {
        if (element) {
            return element.closest(selector);
        }
        return null;
    }

    function showToast(message, type) {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
            return;
        }

        console.warn('Toast function not available. Message:', message);
    }

    function initializeSettings() {
        initRequiredCheckboxes();
        initActiveCheckboxes();
        initFormSubmit();
    }

    function initRequiredCheckboxes() {
        const checkboxes = querySelectorAll(SELECTORS.requiredCheckbox);

        checkboxes.forEach(function (checkbox) {
            checkbox.addEventListener('click', function (event) {
                event.preventDefault();
                showToast('این فیلد اجباری است و نمی‌توان آن را غیرفعال کرد.', 'warning');
                return false;
            });
        });
    }

    function initActiveCheckboxes() {
        const checkboxes = querySelectorAll(SELECTORS.activeCheckbox);

        checkboxes.forEach(function (checkbox) {
            const parent = closest(checkbox, '.' + CLASSES.active);

            if (checkbox.checked) {
                addClass(parent, CLASSES.active);
            }

            checkbox.addEventListener('change', function () {
                if (checkbox.checked) {
                    addClass(parent, CLASSES.active);
                } else {
                    removeClass(parent, CLASSES.active);
                }
            });
        });
    }

    function initFormSubmit() {
        const form = querySelector(SELECTORS.form);

        if (!form) {
            return;
        }

        form.addEventListener('submit', function () {
            const inputs = querySelectorAll(SELECTORS.textInputs, form);

            inputs.forEach(function (input) {
                if (input.value) {
                    input.value = input.value.trim();
                }
            });
        });
    }

    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeSettings);
        } else {
            initializeSettings();
        }
    }

    window.ArrivalReportConfig = {
        init: init,
        showToast: showToast
    };

    init();
})();