/**
 * destroy-uid.js  —  منطق فرم امحا UID
 */

var _searchResults = [];
var _searchedKeys = new Set();

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

function _resetSubmitBtn($btn) {
    $btn.html('<i class="fa fa-check"></i> ثبت امحا');
    $btn.prop('disabled', false);
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
        var pct = Math.round(len / 60 * 100);
        $('#uidBar').css({
            width: pct + '%',
            background: len >= 45 ? '#16a34a' : '#3b82f6'
        });
        $('#uidCounter').text(len + ' / 60');
        // جستجوی خودکار بعد از ۴۵ رقم
        if (len >= 45) doSearch();
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
    $('#uidCounter').text('0 / 60');
}

/* ══════════════════════════════════════════════
   جستجو
   ══════════════════════════════════════════════ */
function doSearch() {
    var mode = $('#btnBatch').hasClass('active') ? 'batch' : 'uid';
    var url, data, currentUid = null;

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
        // ===== حالت UID =====
        currentUid = $('#uidInput').val().trim();
        if (!currentUid) { showToast('UID اجباری است', 'error'); return; }
        if (currentUid.length < 20) { showToast('UID وارد شده معتبر نمی‌باشد', 'error'); return; }
        // در حالت UID، هر بار نتایج قبلی را پاک می‌کنیم (فقط یک نتیجه مجاز است)
        _searchResults = [];
        _searchedKeys.clear();
        resetResults(); // جدول را خالی می‌کند
        url = searchByUidUrl;
        data = { uid: currentUid };
    }

    _setFormLocked(true, 'search');

    if (_searchResults.length > 0) {
        // فقط در حالت batch ممکن است چندین رکورد داشته باشیم، برای loading یک ردیف اضافه می‌کنیم
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
                // خالی کردن اینپوت‌ها در صورت خطا
                $('#batchInput').val('');
                $('#uidInput').val('');
                $('#uidBar').css({ width: '0%', background: '#3b82f6' });
                $('#uidCounter').text('0 / 60');
                showNotFound(res.message);
                // اگر در حالت UID بود، نتایج قبلاً پاک شده‌اند، پس جدول خالی است
                if (mode === 'batch' && _searchResults.length > 0) renderResults();
                else resetResults();
                return;
            }

            // ثبت کلید جستجوشده (فقط برای batch)
            if (mode === 'batch') {
                var keyVal = 'batch:' + $('#batchInput').val().trim();
                _searchedKeys.add(keyVal);
            }

            // اضافه کردن نتایج جدید
            if (mode === 'batch') {
                // در حالت batch، ممکن است چندین رکورد بیاید، آنها را به لیست اضافه می‌کنیم
                $.each(res.data, function (_, item) {
                    var exists = _searchResults.some(function (r) {
                        return r.pkOrderInfo === item.pkOrderInfo;
                    });
                    if (!exists) {
                        item.searchedUid = null; // در batch mode UID ندارد
                        _searchResults.push(item);
                    }
                });
            } else {
                // حالت UID: فقط یک رکورد انتظار می‌رود
                if (res.data && res.data.length > 0) {
                    var item = res.data[0];
                    item.searchedUid = currentUid; // برای نمایش در صورت نیاز
                    _searchResults = [item]; // جایگزین می‌کنیم نه اضافه
                } else {
                    // اگر داده‌ای نبود، خطای not found نمایش داده می‌شود
                    showNotFound('اطلاعاتی یافت نشد');
                    resetResults();
                    return;
                }
            }

            // پاک کردن input بعد از جستجوی موفق
            if (mode === 'batch') {
                $('#batchInput').val('');
            } else {
                $('#uidInput').val('');
                $('#uidBar').css({ width: '0%', background: '#3b82f6' });
                $('#uidCounter').text('0 / 60');
            }

            renderResults();
        },
        error: function () {
            _setFormLocked(false, 'search');
            // خالی کردن اینپوت‌ها در صورت خطای سرور
            $('#batchInput').val('');
            $('#uidInput').val('');
            $('#uidBar').css({ width: '0%', background: '#3b82f6' });
            $('#uidCounter').text('0 / 60');
            showToast('خطا در ارتباط با سرور', 'error');
            if (mode === 'batch' && _searchResults.length > 0) renderResults();
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
            ' value="' + (todayShamsi || '') + '" placeholder="' + (todayShamsi || '1404/01/01') + '" autocomplete="off" />' +
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
        var $inp = $(this);
        DatePicker.attach($inp);
        if ($inp.val()) $inp.addClass('has-value');
        $inp.on('change', function () {
            $(this).toggleClass('has-value', !!$(this).val());
        });
    });

    if (_searchResults.length > 0) $('#submitBtn').show();
}

function removeRow(idx) {
    // کلید جستجو رو هم پاک کن تا بتونه دوباره سرچ کنه (فقط برای batch)
    var removed = _searchResults[idx];
    if (removed) {
        var key = removed.searchedUid
            ? 'uid:' + removed.searchedUid
            : 'batch:' + removed.batchNumber;
        _searchedKeys.delete(key);
    }

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
    $('#submitProgressWrap').empty();
}

/* ══════════════════════════════════════════════
   ثبت امحا — SSE + ProgressBar
   ══════════════════════════════════════════════ */
function submitDestroy() {
    var payload = [];
    var valid = true;
    var totalUids = 0;

    $.each(_searchResults, function (i) {
        var reason = $('.du-reason-sel[data-idx="' + i + '"]').val();
        var date = $('.du-destroy-date[data-idx="' + i + '"]').val().trim();
        if (!date) {
            date = todayShamsi || '';
        }
        if (!reason || !date) { valid = false; return false; }
        payload.push({
            pkOrderInfo: _searchResults[i].pkOrderInfo,
            searchedUid: _searchResults[i].searchedUid || null,
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

    var pb = ProgressBar.create('#submitProgressWrap', {
        label: 'در حال ثبت امحا...',
        showCount: true,
        showEta: true,
        autoDestroy: 0
    });
    pb.start(totalUids);

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
                buffer = lines.pop();

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
        setTimeout(function () {
            $('#submitProgressWrap').empty();
        }, 2000);
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
            $('#uidCounter').text('0 / 60');
            setTimeout(function () {
                $('#submitProgressWrap').empty();
            }, 3000);
        } else {
            setTimeout(function () {
                $('#submitProgressWrap').empty();
            }, 2000);
        }
        _resetSubmitBtn($btn);
    }
}