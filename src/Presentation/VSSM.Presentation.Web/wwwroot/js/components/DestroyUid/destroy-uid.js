/**
 * destroy-uid.js  —  منطق فرم امحا UID
 * ═══════════════════════════════════════════
 * پیش‌نیاز: jQuery، toast.js، datepicker.js، skeleton.js
 * متغیرهای سرور: searchByBatchUrl، searchByUidUrl،
 *                submitDestroyUrl، todayShamsi، reasonList
 * ═══════════════════════════════════════════
 */

var _searchResults = [];          // کل نتایج انباشته‌شده
var _searchedKeys = new Set();   // کلیدهای قبلاً جستجو‌شده (batch یا uid)

/* ══════════════════════════════════════════════
   قفل / آزاد کردن کامل فرم
   ══════════════════════════════════════════════ */
function _setFormLocked(on, ctx) {
    ctx = ctx || 'search';

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
}

/* ══════════════════════════════════════════════
   init
   ══════════════════════════════════════════════ */
$(function () {

    $('#batchInput').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); doSearch(); }
    });

    $('#uidInput').on('input', function () {
        $(this).val($(this).val().replace(/\D/g, ''));
        var len = $(this).val().length;
        var pct = Math.round(len / 20 * 100);
        $('#uidBar').css({
            width: pct + '%',
            background: len === 20 ? '#16a34a' : '#3b82f6'
        });
        $('#uidCounter').text(len + ' / 20');
        if (len === 20) doSearch();
    });

    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); doSearch(); }
    });

});

/* ══════════════════════════════════════════════
   سوئیچ حالت
   ══════════════════════════════════════════════ */
function switchMode(mode) {
    if (mode === 'batch') {
        $('#secBatch').show();
        $('#secUid').hide();
    } else {
        $('#secBatch').hide();
        $('#secUid').show();
    }
    $('#btnBatch').toggleClass('active', mode === 'batch');
    $('#btnUid').toggleClass('active', mode === 'uid');
    resetResults();
    _searchedKeys.clear();
    $('#batchInput, #uidInput').val('');
    $('#uidBar').css({ width: '0%', background: '#3b82f6' });
    $('#uidCounter').text('0 / 20');
}

/* ══════════════════════════════════════════════
   جستجو
   ══════════════════════════════════════════════ */
function doSearch() {
    var mode = $('#btnBatch').hasClass('active') ? 'batch' : 'uid';
    var url, data;

    if (mode === 'batch') {
        var batch = $('#batchInput').val().trim();
        if (!batch) { showToast('سری ساخت اجباری است', 'error'); return; }
        if (_searchedKeys.has('batch:' + batch)) {
            showToast('این سری ساخت قبلاً جستجو شده است', 'warning');
            return;
        }
        url = searchByBatchUrl;
        data = { batchNumber: batch };
    } else {
        var uid = $('#uidInput').val().trim();
        if (!uid) { showToast('UID اجباری است', 'error'); return; }
        if (uid.length < 20) { showToast('UID وارد شده معتبر نمی‌باشد', 'error'); return; }
        if (_searchedKeys.has('uid:' + uid)) {
            showToast('این UID قبلاً جستجو شده است', 'warning');
            return;
        }
        url = searchByUidUrl;
        data = { uid: uid };
    }

    _setFormLocked(true, 'search');

    // اگه نتیجه قبلی داریم، skeleton رو append کن نه جایگزین
    if (_searchResults.length > 0) {
        $('#resultsTbody').append(
            '<tr id="du-loading-row"><td colspan="10" style="text-align:center;padding:16px;color:#9ca3af;font-size:13px;">' +
            '<i class="fa fa-circle-o-notch fa-spin" style="margin-left:6px;"></i>در حال جستجو...</td></tr>'
        );
    } else {
        Skeleton.show('#resultsTbody', 4, 10);
    }

    $.ajax({
        url: url, type: 'POST', data: data,
        success: function (res) {
            _setFormLocked(false, 'search');
            if (!res.success) {
                showNotFound(res.message);
                // skeleton رو پاک کن — اگه نتیجه قبلی داریم برگردان، وگرنه empty
                if (_searchResults.length > 0) renderResults();
                else resetResults();
                return;
            }

            // ثبت کلید جستجوشده
            var mode = $('#btnBatch').hasClass('active') ? 'batch' : 'uid';
            var keyVal = mode === 'batch'
                ? 'batch:' + $('#batchInput').val().trim()
                : 'uid:' + $('#uidInput').val().trim();
            _searchedKeys.add(keyVal);

            // اضافه کردن نتایج جدید به لیست انباشته
            $.each(res.data, function (_, item) {
                // جلوگیری از تکراری بودن بر اساس pkOrderInfo
                var exists = _searchResults.some(function (r) {
                    return r.pkOrderInfo === item.pkOrderInfo;
                });
                if (!exists) _searchResults.push(item);
            });

            // پاک کردن input بعد از جستجوی موفق
            if (mode === 'batch') $('#batchInput').val('');
            else {
                $('#uidInput').val('');
                $('#uidBar').css({ width: '0%', background: '#3b82f6' });
                $('#uidCounter').text('0 / 20');
            }

            renderResults();
        },
        error: function () {
            _setFormLocked(false, 'search');
            showToast('خطا در ارتباط با سرور', 'error');
            // skeleton رو پاک کن — اگه نتیجه قبلی داریم برگردان، وگرنه empty
            if (_searchResults.length > 0) renderResults();
            else resetResults();
        }
    });
}

/* ══════════════════════════════════════════════
   رندر جدول نتایج
   ══════════════════════════════════════════════ */
function renderResults() {
    var reasonOptions = '<option value="">انتخاب کنید</option>';
    $.each(reasonList, function (_, r) {
        reasonOptions += '<option value="' + r.id + '">' + r.title + '</option>';
    });

    var html = '';
    $.each(_searchResults, function (i, r) {
        html +=
            '<tr id="result-row-' + i + '">' +
            '<td>' + (i + 1) + '</td>' +
            '<td>' + (r.batchNumber || '-') + '</td>' +
            '<td>' + (r.itemName || '-') + '</td>' +
            '<td>' + (r.dosePerVial || '-') + '</td>' +
            '<td>' + (r.productionDate || '-') + '</td>' +
            '<td>' + (r.expiryDate || '-') + '</td>' +
            '<td><strong>' + (r.uidCount || 0) + '</strong></td>' +
            '<td>' +
            '<select class="f-control du-reason-sel" data-idx="' + i + '">' +
            reasonOptions + '</select>' +
            '</td>' +
            '<td>' +
            '<div class="datepicker-wrap">' +
            '<input type="text" class="f-control datepicker-input du-destroy-date" data-idx="' + i + '"' +
            ' value="' + (todayShamsi || '') + '" placeholder="1404/01/01" autocomplete="off" />' +
            '<i class="fa fa-calendar datepicker-ico"></i>' +
            '</div>' +
            '</td>' +
            '<td>' +
            '<button type="button" class="du-remove-row-btn" onclick="removeRow(' + i + ')" title="حذف سطر">' +
            '<i class="fa fa-times"></i>' +
            '</button>' +
            '</td>' +
            '</tr>';
    });

    $('#resultsTbody').html(html);

    $('#resultsTbody .datepicker-input').each(function () {
        DatePicker.attach($(this));
    });

    if (_searchResults.length > 0) $('#submitBtn').show();
}

function removeRow(idx) {
    _searchResults.splice(idx, 1);
    if (_searchResults.length === 0) {
        resetResults();
    } else {
        renderResults();
    }
}

/* ══════════════════════════════════════════════
   خالی / یافت نشد
   ══════════════════════════════════════════════ */
function showNotFound(msg) {
    showToast(msg || 'اطلاعاتی یافت نشد', 'error');
    // نتایج قبلی حفظ می‌شن
    if (_searchResults.length === 0) {
        $('#submitBtn').hide();
        $('#resultsTbody').html(
            '<tr><td colspan="10" class="du-notfound-cell">' +
            '<i class="fa fa-search du-notfound-icon"></i>' +
            (msg || 'اطلاعاتی یافت نشد') +
            '</td></tr>'
        );
    }
}

function resetResults() {
    _searchResults = [];
    _searchedKeys.clear();
    $('#submitBtn').hide();
    $('#resultsTbody').html(
        '<tr><td colspan="10" class="du-empty-cell">' +
        '<i class="fa fa-inbox du-empty-icon"></i>' +
        '<span>اطلاعاتی وارد نشد</span>' +
        '</td></tr>'
    );
}

/* ══════════════════════════════════════════════
   ثبت امحا — SSE + ProgressBar component
   ══════════════════════════════════════════════ */
function submitDestroy() {
    var payload = [];
    var valid = true;
    var totalUids = 0;

    $.each(_searchResults, function (i) {
        var reason = $('.du-reason-sel[data-idx="' + i + '"]').val();
        var date = $('.du-destroy-date[data-idx="' + i + '"]').val().trim();
        if (!reason || !date) { valid = false; return false; }
        payload.push({
            pkOrderInfo: _searchResults[i].pkOrderInfo,
            destroyReasonId: parseInt(reason),
            destroyDate: date
        });
        totalUids += (_searchResults[i].uidCount || 0);
    });

    if (!valid) {
        showToast('لطفاً دلیل امحا و تاریخ را برای همه ردیف‌ها وارد کنید', 'error');
        return;
    }

    _setFormLocked(true, 'submit');
    var $btn = $('#submitBtn');
    $btn.html(
        '<span class="du-submit-spinner">' +
        '<span class="du-spin"></span>' +
        'در حال ثبت...' +
        '</span>'
    );

    // ── ساخت progress bar ─────────────────────────────────────────────
    var pb = ProgressBar.create('#submitProgressWrap', {
        label: 'در حال ثبت امحا...',
        showCount: true,
        showEta: true,
        autoDestroy: 0
    });
    pb.start(totalUids);

    // ── ارسال JSON از طریق fetch + EventSource ─────────────────────────
    // چون EventSource فقط GET داره، داده رو با fetch POST می‌فرستیم
    // و سرور SSE برمی‌گردونه
    fetch(submitDestroyProgressUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    }).then(function (response) {
        var reader = response.body.getReader();
        var decoder = new TextDecoder();
        var buffer = '';

        function pump() {
            return reader.read().then(function (chunk) {
                if (chunk.done) return;

                buffer += decoder.decode(chunk.value, { stream: true });
                var lines = buffer.split('\n');
                buffer = lines.pop(); // آخری ممکنه ناقص باشه

                var eventName = 'message';
                lines.forEach(function (line) {
                    if (line.startsWith('event:')) {
                        eventName = line.slice(6).trim();
                    } else if (line.startsWith('data:')) {
                        var json = line.slice(5).trim();
                        try {
                            var d = JSON.parse(json);
                            _handleSSEEvent(eventName, d, pb, $btn, totalUids);
                        } catch (e) { }
                        eventName = 'message';
                    }
                });

                return pump();
            });
        }

        return pump();

    }).catch(function () {
        _setFormLocked(false, 'submit');
        pb.finish(false, 'خطا در ارتباط با سرور');
        showToast('خطا در ارتباط با سرور', 'error');
        _resetSubmitBtn($btn);
    });
}

function _handleSSEEvent(eventName, data, pb, $btn, totalUids) {
    if (eventName === 'start') {
        pb.start(data.total || totalUids);

    } else if (eventName === 'progress') {
        pb.update(data.done, data.total);

    } else if (eventName === 'done') {
        _setFormLocked(false, 'submit');
        pb.finish(data.success, data.message);
        showToast(data.message, data.success ? 'success' : 'error');

        if (data.success) {
            resetResults();
            $('#batchInput, #uidInput').val('');
            $('#uidBar').css({ width: '0%', background: '#3b82f6' });
            $('#uidCounter').text('0 / 20');
        }
        _resetSubmitBtn($btn);
    }
}