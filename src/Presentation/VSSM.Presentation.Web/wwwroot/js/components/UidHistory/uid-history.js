/**
 * uid-history.js
 * جستجوی تاریخچه UID
 */

var lastSearchedUid = '';
var searchTimeout = null;

$(function () {
    // ── رویداد ورودی با تاخیر ──
    $('#uidInput').on('input', function () {
        var len = $(this).val().length;
        var pct = Math.min(len / 20 * 100, 100);
        $('#uidProgress').css('width', pct + '%');
        $('#uidCounter').text(len + ' / 20');
        $('#searchBtn').prop('disabled', len < 20);

        // حذف تایمر قبلی و تنظیم تایمر جدید برای جستجوی خودکار (اختیاری)
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        if (len >= 20) {
            searchTimeout = setTimeout(function () {
                doSearch();
            }, 500);
        }
    });

    // ── جستجو با Enter ──
    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter' && $(this).val().length >= 20) {
            e.preventDefault();
            if (searchTimeout) {
                clearTimeout(searchTimeout);
            }
            doSearch();
        }
    });

    // ── کلیک دکمه جستجو ──
    $('#searchBtn').on('click', function () {
        if ($('#uidInput').val().length >= 20) {
            if (searchTimeout) {
                clearTimeout(searchTimeout);
            }
            doSearch();
        }
    });

    // ── دکمه چاپ ──
    // ── دکمه چاپ (با استفاده از Iframe مخفی) ──
    $('#printBtn').on('click', function () {
        if (!lastSearchedUid) {
            showToast('ابتدا یک UID را جستجو کنید', 'info');
            return;
        }

        var printUrl = printUrlTemplate.replace(
            '__UID__',
            encodeURIComponent(lastSearchedUid)
        );

        // ── غیرفعال کردن دکمه و نمایش وضعیت ──
        var $btn = $(this);
        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> در حال آماده‌سازی...');

        // ── ایجاد یا بازیابی iframe ──
        var iframe = document.getElementById('printIframe');
        if (!iframe) {
            iframe = document.createElement('iframe');
            iframe.id = 'printIframe';
            iframe.style.position = 'absolute';
            iframe.style.width = '0';
            iframe.style.height = '0';
            iframe.style.border = '0';
            iframe.style.display = 'none';
            document.body.appendChild(iframe);
        }

        // ── رویداد بارگذاری iframe ──
        iframe.onload = function () {
            try {
                // تمرکز روی iframe و فراخوانی چاپ
                iframe.contentWindow.focus();
                iframe.contentWindow.print();

                // بازنشانی دکمه پس از کلیک کاربر (با تاخیر)
                setTimeout(function () {
                    $btn.prop('disabled', false).html('<i class="fa fa-print"></i> چاپ');
                }, 1500);

            } catch (e) {
                showToast('خطا در باز کردن چاپگر', 'error');
                $btn.prop('disabled', false).html('<i class="fa fa-print"></i> چاپ');
            }
            // پاک کردن onload برای جلوگیری از اجرای مجدد
            iframe.onload = null;
        };

        iframe.onerror = function () {
            showToast('خطا در بارگذاری صفحه چاپ', 'error');
            $btn.prop('disabled', false).html('<i class="fa fa-print"></i> چاپ');
            iframe.onerror = null;
        };

        // ── بارگذاری URL در iframe ──
        iframe.src = printUrl;
    });

    // ── دکمه بازنشانی ──
    $('#resetBtn').on('click', function () {
        resetAll();
    });

    // ── کلیک روی UID در هدر برای کپی ──
    $('#headerUid').on('click', function () {
        var uid = $(this).text().trim();
        if (uid) {
            copyToClipboard(uid);
            showToast('UID کپی شد', 'success');
        }
    });

    // ── مقداردهی اولیه ──
    $('#searchBtn').prop('disabled', true);
    $('#resultFooter').hide();
    $('#uidCounter').text('0 / 20');
    $('#headerUid').text('').css('cursor', 'default');
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
    $('#headerUid').text('');

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
        timeout: 30000, // 30 ثانیه تایم‌اوت
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

            // ── نمایش UID در هدر ──
            $('#headerUid').text(uid).css('cursor', 'pointer');

            // ── نمایش فوتر و رندر جدول ──
            $('#resultFooter').show();
            lastSearchedUid = uid;
            showToast(res.data.length + ' رویداد یافت شد', 'success');
            renderTable(res.data);

            // ── اسکرول به نتایج ──
            $('html, body').animate({
                scrollTop: $('#resultCard').offset().top - 100
            }, 300);
        },
        error: function (xhr) {
            $('#searchBtn').prop('disabled', false);
            $('#uidInput').prop('disabled', false);

            var msg = 'خطا در ارتباط با سرور';
            if (xhr.status === 0) {
                msg = 'ارتباط با سرور برقرار نشد';
            } else if (xhr.responseJSON?.message) {
                msg = xhr.responseJSON.message;
            }

            showToast(msg, 'error');
            showEmptyState(msg);
            $('#resultFooter').hide();
            $('#headerUid').text('');
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
    $('#headerUid').text('');
}

// ── رندر جدول با فیلدهای صحیح ──
function renderTable(data) {
    var html = '';
    data.forEach(function (row) {
        // ترکیب عنوان رویداد + تاریخ واقعی (با فاصله)
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
    $('#headerUid').text('').css('cursor', 'default');
    showToast('صفحه به حالت اولیه بازگشت', 'info');
}

// ── تابع کمکی برای کپی کردن متن ──
function copyToClipboard(text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).catch(function () {
            fallbackCopy(text);
        });
    } else {
        fallbackCopy(text);
    }
}

function fallbackCopy(text) {
    var textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-9999px';
    textArea.style.top = '0';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    try {
        document.execCommand('copy');
    } catch (err) {
        console.error('کپی کردن با خطا مواجه شد:', err);
    }
    document.body.removeChild(textArea);
}

// ── دکمه به‌روزرسانی ──
$('#refreshBtn').on('click', function () {
    if (lastSearchedUid) {
        doSearchWithUid(lastSearchedUid);
    } else {
        showToast('هیچ UID برای به‌روزرسانی وجود ندارد', 'info');
    }
});

// ── جستجو با UID مشخص (برای به‌روزرسانی) ──
function doSearchWithUid(uid) {
    $('#uidInput').val(uid);
    doSearch();
}