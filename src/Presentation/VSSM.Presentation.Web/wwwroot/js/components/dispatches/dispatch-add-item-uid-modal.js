$(document).on('click', '.add-item-uid', function () {

    let itemId = $(this).data('item-id');

    $('#addItemUidModal')
        .data('item-id', itemId);

    $('#uidInput').val('');
    $('#addItemUidModal').modal('show');

});

$('#addItemUidModal').on('shown.bs.modal', function () {

    $('#uidInput').trigger('focus');

});

$('#uidInput').on('keydown', function (e) {

    if (e.key === 'Enter') {
        e.preventDefault();
        $('#saveUidBtn').click();

        $(this).val('');

    }

});

$('#uidInput').on('input', function () {

    let uid = $(this).val().trim();

    uid = uid.replace(/\D/g, '');
    $(this).val(uid);

    if (uid.length === 20) {
        $('#saveUidBtn').click();
    }

});

let isSaving = false;

$('#saveUidBtn').on('click', function () {

    if (isSaving)
        return;

    let uid = $('#uidInput').val().trim();

    if (!/^\d{20}$/.test(uid)) {
        alert('UID باید دقیقاً 20 رقم و فقط شامل اعداد باشد');
        $('#uidInput').focus();
        return;
    }

    let dispatchId = $('#Id').val();

    isSaving = true;

    $('#addItemUidModal')
        .find('input, button')
        .prop('disabled', true);

    $('#uidLoading').show();

    let itemId = $('#addItemUidModal').data('item-id');

    $.post('/SetUid',
        {
            dispatchLineId: itemId,
            id: dispatchId,
            uid: uid
        })
        .done(function (res) {
            if (res.success) {
                $('#uidInput').val('');

                let $targetRow = $(`tr[data-id="${res.rowId}"]`);


                if ($targetRow.length) {
                    let $doseCell = $targetRow.find('.dose-cell');
                    let $doseSpan = $doseCell.find('.dose-text');

                    $doseSpan.text(res.count);

                    $doseCell.attr('data-dose', res.count);

                    let $doseHiddenInput = $targetRow.find('input[name$=".DoseCount"]');
                    if ($doseHiddenInput.length) {
                        $doseHiddenInput.val(res.count);
                    }

                }

                showToast('عملیات با موفقیت انجام شد.', 'success');
            }
            else {
                $('#uidInput').val('');
                alert(res.message);
            }
        })
        .fail(function () {
            alert('خطا در ارتباط با سرور');
        })
        .always(function () {
            isSaving = false;
            $('#uidLoading').hide();
            $('#addItemUidModal')
                .find('input, button')
                .prop('disabled', false);
            $('#uidInput').trigger('focus');
        });
});