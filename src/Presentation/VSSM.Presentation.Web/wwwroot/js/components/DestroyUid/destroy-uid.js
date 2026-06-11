/**
 * destroy-uid.js  —  منطق فرم امحا UID
 * ═══════════════════════════════════════════
 * پیش‌نیاز: jQuery، toast.js، datepicker.js، skeleton.js
 * متغیرهای سرور: searchByBatchUrl، searchByUidUrl،
 *                submitDestroyUrl، todayShamsi، reasonList
 * ═══════════════════════════════════════════
 */

var _searchResults = [];

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
        url = searchByBatchUrl;
        data = { batchNumber: batch };
    } else {
        var uid = $('#uidInput').val().trim();
        if (!uid) { showToast('UID اجباری است', 'error'); return; }
        if (uid.length < 20) { showToast('UID وارد شده معتبر نمی‌باشد', 'error'); return; }
        url = searchByUidUrl;
        data = { uid: uid };
    }

    _setFormLocked(true, 'search');
    Skeleton.show('#resultsTbody', 4, 9);

    $.ajax({
        url: url, type: 'POST', data: data,
        success: function (res) {
            _setFormLocked(false, 'search');
            if (!res.success) { showNotFound(res.message); return; }
            _searchResults = res.data;
            renderResults();
        },
        error: function () {
            _setFormLocked(false, 'search');
            showToast('خطا در ارتباط با سرور', 'error');
            resetResults();
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
            '<tr>' +
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
            '</tr>';
    });

    $('#resultsTbody').html(html);

    $('#resultsTbody .datepicker-input').each(function () {
        DatePicker.attach($(this));
    });

    $('#submitBtn').show();
}

/* ══════════════════════════════════════════════
   خالی / یافت نشد
   ══════════════════════════════════════════════ */
function showNotFound(msg) {
    _searchResults = [];
    $('#submitBtn').hide();
    $('#resultsTbody').html(
        '<tr><td colspan="9" class="du-notfound-cell">' +
        '<i class="fa fa-search du-notfound-icon"></i>' +
        (msg || 'اطلاعاتی یافت نشد') +
        '</td></tr>'
    );
}

function resetResults() {
    _searchResults = [];
    $('#submitBtn').hide();
    $('#resultsTbody').html(
        '<tr><td colspan="9" class="du-empty-cell">' +
        '<i class="fa fa-inbox du-empty-icon"></i>' +
        '<span>اطلاعاتی وارد نشد</span>' +
        '</td></tr>'
    );
}

/* ══════════════════════════════════════════════
   ثبت امحا
   ══════════════════════════════════════════════ */
function submitDestroy() {
    var payload = [];
    var valid = true;

    $.each(_searchResults, function (i) {
        var reason = $('.du-reason-sel[data-idx="' + i + '"]').val();
        var date = $('.du-destroy-date[data-idx="' + i + '"]').val().trim();
        if (!reason || !date) { valid = false; return false; }
        payload.push({
            pkOrderInfo: _searchResults[i].pkOrderInfo,
            destroyReasonId: parseInt(reason),
            destroyDate: date
        });
    });

    if (!valid) {
        showToast('لطفاً دلیل تعدیل و تاریخ را برای همه ردیف‌ها وارد کنید', 'error');
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

    $.ajax({
        url: submitDestroyUrl, type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            _setFormLocked(false, 'submit');
            if (!res.success) {
                showToast(res.message, 'error');
                _resetSubmitBtn($btn);
                return;
            }
            showToast(res.message || 'امحا با موفقیت ثبت شد', 'success');
            resetResults();
            $('#batchInput, #uidInput').val('');
            $('#uidBar').css({ width: '0%', background: '#3b82f6' });
            $('#uidCounter').text('0 / 20');
            _resetSubmitBtn($btn);
        },
        error: function () {
            _setFormLocked(false, 'submit');
            showToast('خطا در ارتباط با سرور', 'error');
            _resetSubmitBtn($btn);
        }
    });
}

function _resetSubmitBtn($btn) {
    $btn.html('<i class="fa fa-check"></i> ثبت امحا');
}