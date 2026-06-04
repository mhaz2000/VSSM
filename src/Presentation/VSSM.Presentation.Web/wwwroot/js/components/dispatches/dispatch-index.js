$(document).ready(function () {
    // Read toast data from the hidden div
    var toastData = $('#toastData');
    var message = toastData.data('message');
    var type = toastData.data('type');

    if (message && message.trim() !== '' && message !== 'null') {
        showToast(message, type || 'success');
    }
});