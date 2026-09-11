'use strict';

(() => {
    let lastSearchedUid = '';
    let searchTimeoutId = null;

    const MIN_UID_LENGTH = 20;
    const AUTO_SEARCH_DELAY_MS = 500;
    const REQUEST_TIMEOUT_MS = 30_000;
    const PRINT_BUTTON_RESET_DELAY_MS = 1500;
    const SCROLL_OFFSET_PX = 100;

    const $uidInput = $('#uidInput');
    const $uidProgress = $('#uidProgress');
    const $uidCounter = $('#uidCounter');
    const $searchBtn = $('#searchBtn');
    const $printBtn = $('#printBtn');
    const $resetBtn = $('#resetBtn');
    const $refreshBtn = $('#refreshBtn');
    const $headerUid = $('#headerUid');
    const $resultFooter = $('#resultFooter');
    const $resultCard = $('#resultCard');
    const $historyTbody = $('#historyTbody');

    const getAntiForgeryToken = () => {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : null;
    };

    const updateUidProgress = (length) => {
        const pct = Math.min((length / MIN_UID_LENGTH) * 100, 100);
        $uidProgress.css('width', `${pct}%`);
        $uidCounter.text(`${length} / ${MIN_UID_LENGTH}`);
    };

    const setFormBusy = (busy) => {
        $searchBtn.prop('disabled', busy);
        $uidInput.prop('disabled', busy);
    };

    const showEmptyState = (message) => {
        $historyTbody.html(
            `<tr><td colspan="3" class="empty-row">
                <i class="fa fa-inbox"></i>${message || 'اطلاعاتی وجود ندارد'}
            </td></tr>`
        );
        $resultFooter.hide();
        $headerUid.text('').css('cursor', 'default');
    };

    const renderTable = (rows) => {
        const html = rows.map((row) => {
            const eventDisplay = `${row.eventTitle ?? ''} ${row.eventDate ?? ''}`.trim();
            return `<tr>
                <td style="text-align:center;">${row.rowNumber ?? '-'}</td>
                <td>${eventDisplay || '-'}</td>
                <td>${row.description ?? '-'}</td>
            </tr>`;
        }).join('');

        $historyTbody.html(html);
    };

    const clearInput = () => {
        $uidInput.val('');
        updateUidProgress(0);
    };

    const copyToClipboard = (text) => {
        if (navigator.clipboard?.writeText) {
            navigator.clipboard.writeText(text).catch(() => fallbackCopy(text));
            return;
        }
        fallbackCopy(text);
    };

    const fallbackCopy = (text) => {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.cssText = 'position:fixed;left:-9999px;top:0;';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
        } catch (err) {
            console.error('کپی کردن با خطا مواجه شد:', err);
        }
        document.body.removeChild(textArea);
    };

    const doSearch = () => {
        const uid = $uidInput.val().trim();

        if (uid.length < MIN_UID_LENGTH) {
            showToast(`UID باید حداقل ${MIN_UID_LENGTH} کاراکتر باشد`, 'error');
            return;
        }

        const antiForgeryToken = getAntiForgeryToken();
        if (!antiForgeryToken) {
            showToast('توکن امنیتی یافت نشد. صفحه را رفرش کنید.', 'error');
            return;
        }

        setFormBusy(true);
        $resultFooter.hide();
        $headerUid.text('');

        $historyTbody.html(
            `<tr><td colspan="3" class="uh-loading">
                <i class="fa fa-spinner fa-spin"></i>
                <p>در حال جستجو...</p>
            </td></tr>`
        );

        $.ajax({
            url: searchUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ uid }),
            headers: {
                'RequestVerificationToken': antiForgeryToken
            },
            timeout: REQUEST_TIMEOUT_MS,
            success: (res) => {
                setFormBusy(false);
                clearInput();

                if (!res?.success) {
                    const msg = res?.message || 'خطا در جستجو';
                    showToast(msg, 'error');
                    showEmptyState(msg);
                    return;
                }

                if (!Array.isArray(res.data) || res.data.length === 0) {
                    showToast('هیچ رویدادی برای این UID یافت نشد', 'info');
                    showEmptyState('هیچ رویدادی یافت نشد');
                    return;
                }

                $headerUid.text(uid).css('cursor', 'pointer');
                $resultFooter.show();
                lastSearchedUid = uid;
                showToast(`${res.data.length} رویداد یافت شد`, 'success');
                renderTable(res.data);

                $('html, body').animate({
                    scrollTop: $resultCard.offset().top - SCROLL_OFFSET_PX
                }, 300);
            },
            error: (xhr) => {
                setFormBusy(false);

                let msg = 'خطا در ارتباط با سرور';
                if (xhr.status === 0) {
                    msg = 'ارتباط با سرور برقرار نشد';
                } else if (xhr.responseJSON?.message) {
                    msg = xhr.responseJSON.message;
                }

                showToast(msg, 'error');
                showEmptyState(msg);
            }
        });
    };

    const doPrint = () => {
        if (!lastSearchedUid) {
            showToast('ابتدا یک UID را جستجو کنید', 'info');
            return;
        }

        const printUrl = printUrlTemplate.replace(
            '__UID__',
            encodeURIComponent(lastSearchedUid)
        );

        const resetPrintBtn = () => {
            $printBtn.prop('disabled', false)
                .html('<i class="fa fa-print"></i> چاپ');
        };

        $printBtn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i> در حال آماده‌سازی...');

        let iframe = document.getElementById('printIframe');
        if (!iframe) {
            iframe = document.createElement('iframe');
            iframe.id = 'printIframe';
            iframe.style.cssText = 'position:absolute;width:0;height:0;border:0;display:none;';
            document.body.appendChild(iframe);
        }

        iframe.onload = () => {
            try {
                iframe.contentWindow.focus();
                iframe.contentWindow.print();
                setTimeout(resetPrintBtn, PRINT_BUTTON_RESET_DELAY_MS);
            } catch (err) {
                console.error('خطا در چاپ:', err);
                showToast('خطا در باز کردن چاپگر', 'error');
                resetPrintBtn();
            }
            iframe.onload = null;
        };

        iframe.onerror = () => {
            showToast('خطا در بارگذاری صفحه چاپ', 'error');
            resetPrintBtn();
            iframe.onerror = null;
        };

        iframe.src = printUrl;
    };

    const resetAll = () => {
        $historyTbody.html(
            `<tr><td colspan="3" class="empty-table-row">
                <i class="fa fa-search"></i>
                برای مشاهده تاریخچه، UID مورد نظر را جستجو کنید
            </td></tr>`
        );
        $resultFooter.hide();
        lastSearchedUid = '';
        clearInput();
        $searchBtn.prop('disabled', true);
        $headerUid.text('').css('cursor', 'default');
        showToast('صفحه به حالت اولیه بازگشت', 'info');
    };

    const doSearchWithUid = (uid) => {
        $uidInput.val(uid);
        doSearch();
    };

    const bindEvents = () => {
        $uidInput.on('input', function () {
            const len = this.value.length;
            updateUidProgress(len);
            $searchBtn.prop('disabled', len < MIN_UID_LENGTH);

            if (searchTimeoutId) {
                clearTimeout(searchTimeoutId);
                searchTimeoutId = null;
            }

            if (len >= MIN_UID_LENGTH) {
                searchTimeoutId = setTimeout(doSearch, AUTO_SEARCH_DELAY_MS);
            }
        });

        $uidInput.on('keydown', function (e) {
            if (e.key === 'Enter' && this.value.length >= MIN_UID_LENGTH) {
                e.preventDefault();
                if (searchTimeoutId) {
                    clearTimeout(searchTimeoutId);
                    searchTimeoutId = null;
                }
                doSearch();
            }
        });

        $searchBtn.on('click', () => {
            if ($uidInput.val().length >= MIN_UID_LENGTH) {
                if (searchTimeoutId) {
                    clearTimeout(searchTimeoutId);
                    searchTimeoutId = null;
                }
                doSearch();
            }
        });

        $printBtn.on('click', doPrint);
        $resetBtn.on('click', resetAll);

        $refreshBtn.on('click', () => {
            if (lastSearchedUid) {
                doSearchWithUid(lastSearchedUid);
            } else {
                showToast('هیچ UID برای به‌روزرسانی وجود ندارد', 'info');
            }
        });

        $headerUid.on('click', function () {
            const uid = $(this).text().trim();
            if (uid) {
                copyToClipboard(uid);
                showToast('UID کپی شد', 'success');
            }
        });
    };

    $(() => {
        bindEvents();
        $searchBtn.prop('disabled', true);
        $resultFooter.hide();
        $uidCounter.text(`0 / ${MIN_UID_LENGTH}`);
        $headerUid.text('').css('cursor', 'default');
    });
})();