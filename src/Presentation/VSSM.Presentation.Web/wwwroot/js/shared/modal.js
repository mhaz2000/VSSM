
(function (global, $) {
    'use strict';

    var VSSM = global.VSSM || {};
    VSSM.Modal = VSSM.Modal || {};

    var activeModal = null;

    function buildModal(options) {
        var defaults = {
            title: 'هشدار',
            message: 'آیا مطمئن هستید؟',
            confirmText: 'تأیید',
            cancelText: 'انصراف',
            type: 'info', 
            onConfirm: null,
            onCancel: null,
            closeOnBackdrop: true
        };

        var opts = $.extend({}, defaults, options);

        var iconClass = opts.type === 'danger' ? 'fa-exclamation-triangle' : 'fa-question-circle';
        var iconColor = opts.type === 'danger' ? '#dc2626' : '#1d6fb8';

        var html = `
            <div class="vssm-modal-overlay" id="vssmModalOverlay">
                <div class="vssm-modal-box" id="vssmModalBox">
                    <div class="vssm-modal-header">
                        <i class="fa ${iconClass}" style="color:${iconColor};"></i>
                        <h5>${opts.title}</h5>
                    </div>
                    <div class="vssm-modal-body">${opts.message}</div>
                    <div class="vssm-modal-footer">
                        <button class="btn btn-cancel-modal" id="vssmModalCancel">${opts.cancelText}</button>
                        <button class="btn ${opts.type === 'danger' ? 'btn-danger-modal' : 'btn-confirm-modal'}" id="vssmModalConfirm">${opts.confirmText}</button>
                    </div>
                </div>
            </div>
        `;

        return {
            html: html,
            options: opts
        };
    }

    function showModal(options) {
        if (activeModal) {
            closeModal(true);
        }

        var built = buildModal(options);
        var $overlay = $(built.html);
        var opts = built.options;

        $('body').append($overlay);

        var $confirmBtn = $overlay.find('#vssmModalConfirm');
        var $cancelBtn = $overlay.find('#vssmModalCancel');
        var $box = $overlay.find('#vssmModalBox');

        setTimeout(function () {
            $overlay.addClass('active');
        }, 10);

        activeModal = {
            overlay: $overlay,
            box: $box,
            options: opts
        };

        $confirmBtn.on('click', function (e) {
            e.preventDefault();
            if (typeof opts.onConfirm === 'function') {
                opts.onConfirm();
            }
            closeModal();
        });

        $cancelBtn.on('click', function (e) {
            e.preventDefault();
            if (typeof opts.onCancel === 'function') {
                opts.onCancel();
            }
            closeModal();
        });

        if (opts.closeOnBackdrop) {
            $overlay.on('click', function (e) {
                if (e.target === this) {
                    if (typeof opts.onCancel === 'function') {
                        opts.onCancel();
                    }
                    closeModal();
                }
            });
        }

        $(document).on('keydown.vssmModal', function (e) {
            if (e.key === 'Escape' || e.keyCode === 27) {
                if (activeModal) {
                    if (typeof activeModal.options.onCancel === 'function') {
                        activeModal.options.onCancel();
                    }
                    closeModal();
                }
            }
        });
    }

    function closeModal(immediate) {
        if (!activeModal) return;

        var $overlay = activeModal.overlay;

        if (immediate) {
            $overlay.remove();
            activeModal = null;
            $(document).off('keydown.vssmModal');
            return;
        }

        $overlay.removeClass('active');
        setTimeout(function () {
            $overlay.remove();
            activeModal = null;
            $(document).off('keydown.vssmModal');
        }, 250);
    }

    VSSM.Modal.confirm = function (options) {
        return showModal(options);
    };

    VSSM.Modal.alert = function (options) {
        var opts = $.extend({
            confirmText: 'باشه',
            cancelText: '',
            onCancel: null,
            closeOnBackdrop: true
        }, options);

        var defaultOnConfirm = opts.onConfirm;
        opts.onConfirm = function () {
            if (typeof defaultOnConfirm === 'function') {
                defaultOnConfirm();
            }
            closeModal();
        };

        return showModal(opts);
    };

    VSSM.Modal.close = function () {
        closeModal();
    };

    global.VSSM = VSSM;

})(window, jQuery);