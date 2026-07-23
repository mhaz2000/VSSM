/**
 * uid-history.js
 * جستجوی تاریخچه UID
 */

var lastSearchedUid = '';

$(function () {
    // ── رویداد ورودی ──
    $('#uidInput').on('input', function () {
        var len = $(this).val().length;
        var pct = Math.min(len / 20 * 100, 100);
        $('#uidProgress').css('width', pct + '%');
        $('#uidCounter').text(len + ' / 20');
        $('#searchBtn').prop('disabled', len < 20);
    });

    // ── جستجو با Enter ──
    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter' && $(this).val().length >= 20) {
            e.preventDefault();
            doSearch();
        }
    });

    // ── کلیک دکمه جستجو ──
    $('#searchBtn').on('click', function () {
        if ($('#uidInput').val().length >= 20) {
            doSearch();
        }
    });

    // ── دکمه چاپ ──
    $('#printBtn').on('click', function () {

        if (!lastSearchedUid) {
            showToast(
                'ابتدا یک UID را جستجو کنید',
                'info'
            );

            return;
        }

        var printUrl =
            printUrlTemplate.replace(
                '__UID__',
                encodeURIComponent(lastSearchedUid)
            );

        window.open(
            printUrl,
            '_blank',
            'width=1000,height=800'
        );
    });

    // ── دکمه بازنشانی ──
    $('#resetBtn').on('click', function () {
        resetAll();
    });

    // ── مقداردهی اولیه ──
    $('#searchBtn').prop('disabled', true);
    $('#resultFooter').hide();
    $('#uidCounter').text('0 / 20');
});

function doSearch() {
    var uid = $('#uidInput').val().trim();

    if (uid.length < 20) {
        showToast('UID باید حداقل ۲۰ کاراکتر باشد', 'error');
        return;
    }

    // ── قفل فرم ──
    $('#searchBtn').prop('disabled', true);
    $('#uidInput').prop('disabled', true);
    $('#resultFooter').hide();

    // ── نمایش لودینگ ──
    $('#historyTbody').html(
        '<tr><td colspan="3" class="uh-loading">' +
        '<i class="fa fa-spinner fa-spin"></i>' +
        '<p>در حال جستجو...</p>' +
        '</td></tr>'
    );

    // ── درخواست AJAX ──
    $.ajax({
        url: searchUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ uid: uid }),
        success: function (res) {
            // ── فعال کردن فرم ──
            $('#searchBtn').prop('disabled', false);
            $('#uidInput').prop('disabled', false);

            // ── خالی کردن ورودی ──
            $('#uidInput').val('');
            $('#uidProgress').css('width', '0%');
            $('#uidCounter').text('0 / 20');

            // ── بررسی پاسخ ──
            if (!res.success) {
                showToast(res.message || 'خطا در جستجو', 'error');
                showEmptyState(res.message || 'اطلاعاتی یافت نشد');
                $('#resultFooter').hide();
                return;
            }

            if (!res.data || res.data.length === 0) {
                showToast('هیچ رویدادی برای این UID یافت نشد', 'info');
                showEmptyState('هیچ رویدادی یافت نشد');
                $('#resultFooter').hide();
                return;
            }

            // ── نمایش فوتر و رندر جدول ──
            $('#resultFooter').show();
            lastSearchedUid = uid;
            showToast(res.data.length + ' رویداد یافت شد', 'success');
            renderTable(res.data);
        },
        error: function (xhr) {
            $('#searchBtn').prop('disabled', false);
            $('#uidInput').prop('disabled', false);
            var msg = xhr.responseJSON?.message || 'خطا در ارتباط با سرور';
            showToast(msg, 'error');
            showEmptyState(msg);
            $('#resultFooter').hide();
        }
    });
}

// ── نمایش حالت خالی ──
function showEmptyState(message) {
    $('#historyTbody').html(
        '<tr><td colspan="3" class="empty-row">' +
        '<i class="fa fa-inbox"></i>' +
        (message || 'اطلاعاتی وجود ندارد') +
        '</td></tr>'
    );
    $('#resultFooter').hide();
}

// ── رندر جدول با فیلدهای صحیح ──
function renderTable(data) {
    var html = '';
    data.forEach(function (row) {
        // ✅ ترکیب عنوان رویداد + تاریخ واقعی (با فاصله)
        var eventDisplay = (row.eventTitle || '') + ' ' + (row.eventDate || '');
        html += '<tr>' +
            '<td style="text-align:center;">' + (row.rowNumber || '-') + '</td>' +
            '<td>' + eventDisplay + '</td>' +
            '<td>' + (row.description || '-') + '</td>' +
            '</tr>';
    });
    $('#historyTbody').html(html);
}

// ── بازنشانی کامل صفحه ──
function resetAll() {
    $('#historyTbody').html(
        '<tr><td colspan="3" class="empty-table-row">' +
        '<i class="fa fa-search"></i>' +
        'برای مشاهده تاریخچه، UID مورد نظر را جستجو کنید' +
        '</td></tr>'
    );
    $('#resultFooter').hide();
    lastSearchedUid = '';
    $('#uidInput').val('');
    $('#uidProgress').css('width', '0%');
    $('#uidCounter').text('0 / 20');
    $('#searchBtn').prop('disabled', true);
    showToast('صفحه به حالت اولیه بازگشت', 'info');
}