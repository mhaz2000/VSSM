(function () {
    'use strict';

    window.VSSM = window.VSSM || {};
    window.VSSM.Loading = {
        show: function (containerId) {
            const overlay = document.getElementById(containerId || 'loadingSpinner');
            if (overlay) overlay.style.display = 'flex';
        },
        hide: function (containerId) {
            const overlay = document.getElementById(containerId || 'loadingSpinner');
            if (overlay) overlay.style.display = 'none';
        }
    };
})();