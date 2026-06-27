(function () {
    'use strict';

    var EYE_OPEN = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>';
    var EYE_SHUT = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>'
        + '<path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>'
        + '<path d="M14.12 14.12a3 3 0 1 1-4.24-4.24"/>'
        + '<line x1="1" y1="1" x2="23" y2="23"/>';

    window.togglePassword = function () {
        var input = document.getElementById('Password');
        var icon = document.getElementById('eye-icon');
        var btn = document.querySelector('.toggle-password');
        var visible = input.type === 'text';

        input.type = visible ? 'password' : 'text';
        icon.innerHTML = visible ? EYE_OPEN : EYE_SHUT;
        btn.setAttribute('aria-label', visible ? 'نمایش رمز عبور' : 'پنهان کردن رمز عبور');
    };
}) ();