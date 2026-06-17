
const SubmitBtn = (() => {

    function startLoading(btn) {
        btn.dataset.loading = 'true';

        var content = btn.querySelector('.js-btn-content');
        var spinner = btn.querySelector('.js-btn-spinner');

        if (content) content.hidden = true;
        if (spinner) spinner.hidden = false;

        document.getElementById('pageOverlay').hidden = false;
        document.querySelectorAll('.js-submit-btn').forEach(function (b) { b.disabled = true; });
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