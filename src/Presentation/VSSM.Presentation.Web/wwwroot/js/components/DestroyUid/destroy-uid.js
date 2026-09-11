'use strict';

/* global showToast, searchByBatchUrl, searchByUidUrl, submitDestroyProgressUrl, reasonList, todayShamsi, Skeleton, DatePicker, ProgressBar */

(() => {
    let _searchResults = [];
    const _searchedKeys = new Set();

    const UID_MIN_LENGTH = 20;
    const UID_MAX_LENGTH = 60;
    const UID_AUTO_SEARCH_LENGTH = 45;

    const getAntiForgeryToken = () => {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : null;
    };

    const safeJsonParse = (text) => {
        try {
            return JSON.parse(text);
        } catch {
            return null;
        }
    };

    const clearUidInput = () => {
        $('#uidInput').val('');
        $('#uidBar').css({ width: '0%', background: '#3b82f6' });
        $('#uidCounter').text(`0 / ${UID_MAX_LENGTH}`);
    };

    const clearAllInputs = () => {
        $('#batchInput').val('');
        clearUidInput();
    };

    const setFormLocked = (on, ctx = 'search') => {
        $('#btnBatch, #btnUid').prop('disabled', on);
        $('#batchInput, #uidInput').prop('disabled', on);
        $('.du-search-btn').prop('disabled', on);

        if (ctx === 'submit') {
            $('#resultsTable .du-reason-sel').prop('disabled', on);
            $('#resultsTable .datepicker-input').prop('disabled', on);
        }

        $('.a-card-body').toggleClass('du-locked', on);

        if (ctx === 'search') {
            if (on) $('#submitBtn').hide();
        } else {
            $('#submitBtn').prop('disabled', on);
        }
    };

    const resetSubmitBtn = ($btn) => {
        $btn.html('<i class="fa fa-check"></i> ثبت امحا');
        $btn.prop('disabled', false);
    };

    const resetResults = () => {
        _searchResults = [];
        _searchedKeys.clear();
        $('#submitBtn').hide();
        $('#resultsTbody').html(
            `<tr><td colspan="10" class="du-empty-cell">
                <i class="fa fa-inbox du-empty-icon"></i>
                <span>اطلاعاتی وارد نشد</span>
            </td></tr>`
        );
        $('#submitProgressWrap').empty();
    };

    const showNotFound = (msg) => {
        showToast(msg || 'اطلاعاتی یافت نشد', 'error');
        if (_searchResults.length === 0) {
            $('#submitBtn').hide();
            $('#resultsTbody').html(
                `<tr><td colspan="10" class="du-notfound-cell">
                    <i class="fa fa-search du-notfound-icon"></i>${msg || 'اطلاعاتی یافت نشد'}
                </td></tr>`
            );
        }
    };

    const switchMode = (mode) => {
        const isBatch = mode === 'batch';
        $('#secBatch').toggle(isBatch);
        $('#secUid').toggle(!isBatch);
        $('#btnBatch').toggleClass('active', isBatch);
        $('#btnUid').toggleClass('active', !isBatch);
        resetResults();
        _searchedKeys.clear();
        clearAllInputs();
    };

    const renderResults = () => {
        let reasonOptions = '<option value="">انتخاب کنید</option>';
        reasonList.forEach((r) => {
            reasonOptions += `<option value="${r.id}">${r.title}</option>`;
        });

        const html = _searchResults.map((r, i) => `
            <tr id="result-row-${i}">
                <td>${i + 1}</td>
                <td>${r.batchNumber || '-'}</td>
                <td>${r.itemName || '-'}</td>
                <td>${r.dosePerVial || '-'}</td>
                <td>${r.productionDate || '-'}</td>
                <td>${r.expiryDate || '-'}</td>
                <td><strong>${r.uidCount || 0}</strong></td>
                <td><select class="f-control du-reason-sel" data-idx="${i}">${reasonOptions}</select></td>
                <td>
                    <div class="datepicker-wrap">
                        <input type="text" class="f-control datepicker-input du-destroy-date" data-idx="${i}"
                               value="${todayShamsi || ''}" placeholder="${todayShamsi || '1404/01/01'}" autocomplete="off" />
                        <i class="fa fa-calendar datepicker-ico"></i>
                    </div>
                </td>
                <td>
                    <button type="button" class="du-remove-row-btn" data-idx="${i}" title="حذف سطر">
                        <i class="fa fa-times"></i>
                    </button>
                </td>
            </tr>
        `).join('');

        $('#resultsTbody').html(html);

        $('#resultsTbody .datepicker-input').each(function () {
            const $inp = $(this);
            DatePicker.attach($inp);
            if ($inp.val()) $inp.addClass('has-value');
            $inp.on('change', function () {
                $(this).toggleClass('has-value', !!$(this).val());
            });
        });

        if (_searchResults.length > 0) $('#submitBtn').show();
    };

    const removeRow = (idx) => {
        const removed = _searchResults[idx];
        if (removed) {
            const key = removed.searchedUid
                ? `uid:${removed.searchedUid}`
                : `batch:${removed.batchNumber}`;
            _searchedKeys.delete(key);
        }

        _searchResults.splice(idx, 1);

        if (_searchResults.length === 0) {
            resetResults();
        } else {
            renderResults();
        }
    };

    const doSearch = () => {
        const isBatch = $('#btnBatch').hasClass('active');
        const token = getAntiForgeryToken();
        if (!token) {
            showToast('توکن امنیتی یافت نشد. صفحه را رفرش کنید.', 'error');
            return;
        }

        let url;
        let data;
        let currentUid = null;

        if (isBatch) {
            const batch = $('#batchInput').val().trim();
            if (!batch) { showToast('سری ساخت اجباری است', 'error'); return; }
            if (_searchedKeys.has(`batch:${batch}`)) {
                showToast('این سری ساخت قبلاً جستجو شده است', 'warning');
                return;
            }
            url = searchByBatchUrl;
            data = { batchNumber: batch };
        } else {
            currentUid = $('#uidInput').val().trim();
            if (!currentUid) { showToast('UID اجباری است', 'error'); return; }
            if (currentUid.length < UID_MIN_LENGTH) {
                showToast('UID وارد شده معتبر نمی‌باشد', 'error');
                return;
            }
            _searchResults = [];
            _searchedKeys.clear();
            resetResults();
            url = searchByUidUrl;
            data = { uid: currentUid };
        }

        setFormLocked(true, 'search');

        if (_searchResults.length > 0) {
            $('#resultsTbody').append(
                `<tr id="du-loading-row"><td colspan="10" style="text-align:center;padding:16px;color:#9ca3af;font-size:13px;">
                    <i class="fa fa-circle-o-notch fa-spin" style="margin-left:6px;"></i>در حال جستجو...
                </td></tr>`
            );
        } else {
            Skeleton.show('#resultsTbody', 4, 10);
        }

        $.ajax({
            url,
            type: 'POST',
            data,
            headers: { 'RequestVerificationToken': token },
            success: (res) => {
                setFormLocked(false, 'search');

                if (!res.success) {
                    clearAllInputs();
                    showNotFound(res.message);
                    if (isBatch && _searchResults.length > 0) renderResults();
                    else resetResults();
                    return;
                }

                if (isBatch) {
                    _searchedKeys.add(`batch:${$('#batchInput').val().trim()}`);
                    res.data.forEach((item) => {
                        const exists = _searchResults.some((r) => r.pkOrderInfo === item.pkOrderInfo);
                        if (!exists) {
                            item.searchedUid = null;
                            _searchResults.push(item);
                        }
                    });
                } else if (Array.isArray(res.data) && res.data.length > 0) {
                    const item = res.data[0];
                    item.searchedUid = currentUid;
                    _searchResults = [item];
                } else {
                    showNotFound('اطلاعاتی یافت نشد');
                    resetResults();
                    return;
                }

                if (isBatch) $('#batchInput').val('');
                else clearUidInput();

                renderResults();
            },
            error: () => {
                setFormLocked(false, 'search');
                clearAllInputs();
                showToast('خطا در ارتباط با سرور', 'error');
                if (isBatch && _searchResults.length > 0) renderResults();
                else resetResults();
            }
        });
    };

    const handleSSEEvent = (eventName, data, pb, $btn, totalUids) => {
        if (eventName === 'start') {
            pb.start(data.total || totalUids);
            return;
        }

        if (eventName === 'progress') {
            pb.update(data.done, data.total);
            return;
        }

        if (eventName === 'done') {
            setFormLocked(false, 'submit');
            pb.finish(data.success, data.message);
            showToast(data.message, data.success ? 'success' : 'error');

            if (data.success) {
                resetResults();
                clearAllInputs();
                setTimeout(() => $('#submitProgressWrap').empty(), 3000);
            } else {
                setTimeout(() => $('#submitProgressWrap').empty(), 2000);
            }
            resetSubmitBtn($btn);
        }
    };

    const submitDestroy = () => {
        const payload = [];
        let valid = true;
        let totalUids = 0;

        for (let i = 0; i < _searchResults.length; i++) {
            const r = _searchResults[i];
            const reason = $(`.du-reason-sel[data-idx="${i}"]`).val();
            const date = $(`.du-destroy-date[data-idx="${i}"]`).val().trim() || todayShamsi || '';
            if (!reason || !date) {
                valid = false;
                break;
            }
            payload.push({
                pkOrderInfo: r.pkOrderInfo,
                searchedUid: r.searchedUid || null,
                destroyReasonId: parseInt(reason, 10),
                destroyDate: date
            });
            totalUids += (r.uidCount || 0);
        }

        if (!valid) {
            showToast('لطفاً دلیل امحا و تاریخ را برای همه ردیف‌ها وارد کنید', 'error');
            return;
        }

        const token = getAntiForgeryToken();
        if (!token) {
            showToast('توکن امنیتی یافت نشد. صفحه را رفرش کنید.', 'error');
            return;
        }

        setFormLocked(true, 'submit');
        const $btn = $('#submitBtn');
        $btn.html(
            `<span class="du-submit-spinner">
                <span class="du-spin"></span>در حال ثبت...
            </span>`
        );

        const pb = ProgressBar.create('#submitProgressWrap', {
            label: 'در حال ثبت امحا...',
            showCount: true,
            showEta: true,
            autoDestroy: 0
        });
        pb.start(totalUids);

        fetch(submitDestroyProgressUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(payload)
        }).then((response) => {
            const reader = response.body.getReader();
            const decoder = new TextDecoder();
            let buffer = '';

            const pump = () => reader.read().then((chunk) => {
                if (chunk.done) return;

                buffer += decoder.decode(chunk.value, { stream: true });
                const lines = buffer.split('\n');
                buffer = lines.pop();

                let eventName = 'message';
                lines.forEach((line) => {
                    if (line.startsWith('event:')) {
                        eventName = line.slice(6).trim();
                    } else if (line.startsWith('data:')) {
                        const parsed = safeJsonParse(line.slice(5).trim());
                        if (parsed !== null) {
                            handleSSEEvent(eventName, parsed, pb, $btn, totalUids);
                        }
                        eventName = 'message';
                    }
                });

                return pump();
            });

            return pump();
        }).catch(() => {
            setFormLocked(false, 'submit');
            pb.finish(false, 'خطا در ارتباط با سرور');
            showToast('خطا در ارتباط با سرور', 'error');
            resetSubmitBtn($btn);
            setTimeout(() => $('#submitProgressWrap').empty(), 2000);
        });
    };

    const bindEvents = () => {
        $('#batchInput').on('keydown', (e) => {
            if (e.key === 'Enter') { e.preventDefault(); doSearch(); }
        });

        $('#uidInput').on('input', function () {
            const $el = $(this);
            $el.val($el.val().replace(/\D/g, ''));
            const len = $el.val().length;
            const pct = Math.round((len / UID_MAX_LENGTH) * 100);
            $('#uidBar').css({
                width: `${pct}%`,
                background: len >= UID_AUTO_SEARCH_LENGTH ? '#16a34a' : '#3b82f6'
            });
            $('#uidCounter').text(`${len} / ${UID_MAX_LENGTH}`);
            if (len >= UID_AUTO_SEARCH_LENGTH) doSearch();
        });

        $('#uidInput').on('keydown', (e) => {
            if (e.key === 'Enter') { e.preventDefault(); doSearch(); }
        });

        $('#resultsTbody').on('click', '.du-remove-row-btn', function () {
            removeRow(parseInt($(this).data('idx'), 10));
        });
    };

    $(() => bindEvents());

    window.switchMode = switchMode;
    window.doSearch = doSearch;
    window.submitDestroy = submitDestroy;
})();