$(function () {

    $(document).on('click', '[data-toggle="tree"]', function (e) {
        e.stopPropagation();

        var $toggle = $(this);
        var $parentLi = $toggle.closest('li');
        var $childrenContainer = $parentLi.children('ul.tree-children');

        if ($childrenContainer.length) {
            $childrenContainer.toggleClass('open');
            $toggle.toggleClass('expanded');
        }
    });

    $(document).on('click', '.tree-label', function (e) {
        var $checkbox = $(this).siblings('.tree-check');
        if ($checkbox.length) {
            $checkbox.prop('checked', !$checkbox.prop('checked'));
        }
    });

    $(document).on('change', '.tree-check', function () {
        var $checkbox = $(this);
        var $parentLi = $checkbox.closest('li');
        var $childCheckboxes = $parentLi.find('ul.tree-children .tree-check');

        if ($childCheckboxes.length) {
            $childCheckboxes.prop('checked', $checkbox.prop('checked'));
        }
    });

    var msg = $('#toastData').data('message');
    var type = $('#toastData').data('type');
    if (msg && msg !== '') {
        showToast(msg, type || 'success');
    }

});