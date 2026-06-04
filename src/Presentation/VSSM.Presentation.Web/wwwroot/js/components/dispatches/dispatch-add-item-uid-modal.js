$(document).on('click', '.add-item-uid', function () {

    let itemId = $(this).data('item-id');

    $('#addItemUidModal')
        .data('item-id', itemId);

    $('#uidInput').val('');
    $('#uidError').hide().empty();
    $('#addItemUidModal').modal('show');

});

$('#addItemUidModal').on('shown.bs.modal', function () {
    $('#uidInput').trigger('focus');
});

$('#uidInput').on('keydown', function (e) {

    if (e.key === 'Enter') {
        e.preventDefault();
        $('#saveUidBtn').click();
    }

});

$('#uidInput').on('input', function () {

    $('#uidError').hide().empty();

    let uid = $(this).val().trim();

    uid = uid.replace(/\D/g, '');

    // Limit to 20 digits
    if (uid.length > 20) {
        uid = uid.slice(0, 20);
    }

    $(this).val(uid);

    if (uid.length === 20) {
        $('#saveUidBtn').click();
    }

});

let isSaving = false;

function showModalError(message) {
    $('#uidError')
        .text(message)
        .show();

    // Scroll to error message
    $('.modal-body').animate({
        scrollTop: $('#uidError').offset().top - $('.modal-body').offset().top
    }, 300);

    $('#uidInput').focus();

}

function clearModalError() {
    $('#uidError').hide().empty();
}

$('#saveUidBtn').on('click', function () {

    if (isSaving)
        return;

    let uid = $('#uidInput').val().trim();

    if (!/^\d{20}$/.test(uid)) {
        showModalError('UID باید دقیقاً 20 رقم و فقط شامل اعداد باشد');
        $('#uidInput').focus();
        return;
    }

    let dispatchId = $('#Id').val();

    isSaving = true;
    clearModalError();

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
                clearModalError();

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

                // Close modal on success
                $('#addItemUidModal').modal('hide');
            }
            else {
                $('#uidInput').val('');
                showModalError(res.message || 'خطا در ثبت UID');
                $('#uidInput').focus();
            }
        })
        .fail(function (xhr, status, error) {
            let errorMessage = 'خطا در ارتباط با سرور';

            // Try to get error message from response
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr.responseText) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message) errorMessage = response.message;
                } catch (e) { }
            }

            showModalError(errorMessage);
        })
        .always(function () {
            isSaving = false;
            $('#uidLoading').hide();
            $('#addItemUidModal')
                .find('input, button')
                .prop('disabled', false);

            // Only focus if modal is still open
            if ($('#addItemUidModal').hasClass('show')) {
                $('#uidInput').trigger('focus');
            }

            $('#uidInput').focus();

        });
});

// Clear error when modal is hidden
$('#addItemUidModal').on('hidden.bs.modal', function () {
    clearModalError();
    $('#uidInput').val('');
    isSaving = false;
});