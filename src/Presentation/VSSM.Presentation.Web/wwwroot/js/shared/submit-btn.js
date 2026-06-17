
const SubmitBtn = (() => {

    function startLoading(btn) {
        btn.dataset.loading = 'true';
        btn.querySelector('.js-btn-content').hidden = true;
        btn.querySelector('.js-btn-spinner').hidden = false;

        document.getElementById('pageOverlay').hidden = false;

        document.querySelectorAll('.js-submit-btn').forEach(b => b.disabled = true);
    }

    function stopLoading() {
        document.querySelectorAll('.js-submit-btn').forEach(btn => {
            btn.dataset.loading = 'false';
            btn.querySelector('.js-btn-content').hidden = false;
            btn.querySelector('.js-btn-spinner').hidden = true;
            btn.disabled = false;
        });
        document.getElementById('pageOverlay').hidden = true;
    }

    return { startLoading, stopLoading };
})();