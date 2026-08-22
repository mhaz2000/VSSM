(function () {
    'use strict';

    const SELECTORS = {
        requiredCheckbox: '.dispatch-item.required input[type="checkbox"]',
        activeCheckbox: '.dispatch-item:not(.disabled) input[type="checkbox"]'
    };

    const CLASSES = {
        active: 'active'
    };

    const TOAST_MESSAGE = 'این فیلد اجباری است و نمی‌توان آن را غیرفعال کرد.';
    const TOAST_TYPE = 'warning';

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

    function closest(element, selector) {
        if (element) {
            return element.closest(selector);
        }
        return null;
    }

    function showToast(message, type) {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
        }
    }

    function handleRequiredCheckboxClick(event) {
        event.preventDefault();
        showToast(TOAST_MESSAGE, TOAST_TYPE);
        return false;
    }

    function initRequiredCheckboxes() {
        const checkboxes = querySelectorAll(SELECTORS.requiredCheckbox);
        const length = checkboxes.length;

        for (let index = 0; index < length; index++) {
            checkboxes[index].addEventListener('click', handleRequiredCheckboxClick);
        }
    }

    function handleActiveCheckboxChange(checkbox, parent) {
        return function () {
            if (checkbox.checked) {
                addClass(parent, CLASSES.active);
            } else {
                removeClass(parent, CLASSES.active);
            }
        };
    }

    function initActiveCheckboxes() {
        const checkboxes = querySelectorAll(SELECTORS.activeCheckbox);
        const length = checkboxes.length;

        for (let index = 0; index < length; index++) {
            const checkbox = checkboxes[index];
            const parent = closest(checkbox, '.' + CLASSES.active);

            if (checkbox.checked) {
                addClass(parent, CLASSES.active);
            }

            checkbox.addEventListener('change', handleActiveCheckboxChange(checkbox, parent));
        }
    }

    function initializeSettings() {
        initRequiredCheckboxes();
        initActiveCheckboxes();
    }

    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeSettings);
        } else {
            initializeSettings();
        }
    }

    window.DispatchReportConfig = {
        init: init,
        showToast: showToast
    };

    init();
})();