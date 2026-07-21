/**
 * uid-history.js
 * جستجوی تاریخچه UID
 */

$(function () {
    $('#uidInput').on('input', function () {
        var len = $(this).val().length;
        var pct = Math.min(len / 20 * 100, 100);
        $('#uidProgress').css('width', pct + '%');
        $('#uidCounter').text(len + ' / 20');
        $('#searchBtn').prop('disabled', len < 20);
    });

    $('#uidInput').on('keydown', function (e) {
        if (e.key === 'Enter' && $(this).val().length >= 20) {
            e.preventDefault();
            doSearch();
        }
    });

    $('#searchBtn').on('click', function () {
        if ($('#uidInput').val().length >= 20) {
            doSearch();
        }
    });

    $('#searchBtn').prop('disabled', true);
});

function doSearch() {
    var uid = $('#uidInput').val().trim();

    if (uid.length < 20) {
        showToast('UID باید حداقل ۲۰ کاراکتر باشد', 'error');
        return;
    }

    $('#searchBtn').prop('disabled', true);
    $('#uidInput').prop('disabled', true);

    $('#historyTbody').html(
        '<tr><td colspan="3" class="uh-loading">' +
        '<i class="fa fa-spinner fa-spin"></i>' +
        '<p>در حال جستجو...</p>' +
        '</td></tr>'
    );

    $.ajax({
        url: searchUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ uid: uid }),
        success: function (res) {
            $('#searchBtn').prop('disabled', false);
            $('#uidInput').prop('disabled', false);

            if (!res.success) {
                showToast(res.message || 'خطا در جستجو', 'error');
                showEmptyState(res.message || 'اطلاعاتی یافت نشد');
                return;
            }

            if (!res.data || res.data.length === 0) {
                showToast('هیچ رویدادی برای این UID یافت نشد', 'info');
                showEmptyState('هیچ رویدادی یافت نشد');
                return;
            }

            showToast(res.data.length + ' رویداد یافت شد', 'success');
            renderTable(res.data);
        },
        error: function (xhr) {
            $('#searchBtn').prop('disabled', false);
            $('#uidInput').prop('disabled', false);
            var msg = xhr.responseJSON?.message || 'خطا در ارتباط با سرور';
            showToast(msg, 'error');
            showEmptyState(msg);
        }
    });
}

function showEmptyState(message) {
    $('#historyTbody').html(
        '<tr><td colspan="3" class="empty-row">' +
        '<i class="fa fa-inbox"></i>' +
        (message || 'اطلاعاتی وجود ندارد') +
        '</td></tr>'
    );
}

function renderTable(data) {
    var html = '';
    data.forEach(function (row, index) {
        html += '<tr>' +
            '<td style="text-align:center;">' + (index + 1) + '</td>' +
            '<td>' + (row.تاریخ || '-') + '</td>' +
            '<td>' + (row.توضیحات || '-') + '</td>' +
            '</tr>';
    });
    $('#historyTbody').html(html);
}