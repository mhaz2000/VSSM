const manageUserRecipient = (() => {
    let currentPage = 1;
    const pageSize = 10;
    let searchTerm = '';
    let totalCount = 0;
    let totalPages = 1;

    const selectors = {
        searchInput: '#searchInput',
        searchButton: '#searchBtn',
        usersBody: '#usersTbody',
        pagination: '#paginationContainer',
        modal: '#changeRecipientModal',
        modalUserName: '#modalUserName',
        modalUserUsername: '#modalUserUsername',
        modalCurrentRecipient: '#modalCurrentRecipient',
        recipientSelect: '#newRecipientSelect',
        saveButton: '#saveRecipientBtn',
        closeButton: '#closeChangeRecipientModal',
        cancelButton: '#cancelChangeRecipientModal'
    };

    const init = () => {
        bindEvents();
        updateSearchButtonState();
        renderPagination(0);
    };

    const bindEvents = () => {
        $(selectors.searchInput).on('input', updateSearchButtonState);

        $(selectors.searchInput).on('keydown', handleSearchKeyDown);

        $(selectors.searchButton).on('click', handleSearch);

        $(document).on('click', '.btn-change-recipient', handleChangeRecipient);

        $(document).on('input', '.ss-search', handleRecipientSearch);

        $(document).on('click', '.ss-option', handleRecipientSelect);

        $(selectors.saveButton).on('click', saveRecipient);

        $(selectors.closeButton).on('click', closeModal);

        $(selectors.cancelButton).on('click', closeModal);

        $(selectors.modal).on('click', handleModalOverlayClick);
    };

    const updateSearchButtonState = () => {
        const value = $(selectors.searchInput).val().trim();

        $(selectors.searchButton).prop('disabled', value.length === 0);
    };

    const handleSearchKeyDown = (event) => {
        if (event.key !== 'Enter') {
            return;
        }

        event.preventDefault();

        handleSearch();
    };

    const handleSearch = () => {
        const value = $(selectors.searchInput).val().trim();

        if (!value) {
            return;
        }

        searchTerm = value;
        currentPage = 1;

        searchUsers();
    };

    const searchUsers = () => {
        const url = '/UserManagement/SearchUsers';

        setTableLoading();

        $.ajax({
            url,
            type: 'GET',
            data: {
                searchTerm,
                pageNumber: currentPage,
                pageSize
            },
            success: handleSearchSuccess,
            error: handleSearchError
        });
    };

    const handleSearchSuccess = (response) => {
        if (!response.success) {
            showToast(
                response.message || 'خطا در جستجوی کاربران',
                'error'
            );

            renderUsers([]);
            renderPagination(0);

            return;
        }

        const users = response.data || [];

        totalCount = response.totalCount || 0;
        totalPages = Math.ceil(totalCount / pageSize) || 1;

        renderUsers(users);
        renderPagination(totalCount);
    };

    const handleSearchError = (xhr) => {
        console.error('Search users error:', xhr);

        showToast(
            xhr.responseJSON?.message || 'خطا در ارتباط با سرور',
            'error'
        );

        renderUsers([]);
        renderPagination(0);
    };

    const setTableLoading = () => {
        $(selectors.usersBody).html(`
            <tr>
                <td colspan="9" class="table-state">
                    <i class="fa fa-spinner fa-spin"></i>
                    در حال بارگذاری...
                </td>
            </tr>
        `);
    };

    const renderUsers = (users) => {
        const $tbody = $(selectors.usersBody);

        if (!users || users.length === 0) {
            $tbody.html(`
                <tr>
                    <td colspan="9" class="table-state table-empty">
                        هیچ کاربری یافت نشد
                    </td>
                </tr>
            `);

            return;
        }

        const startIndex = (currentPage - 1) * pageSize;

        const rows = users.map((user, index) => {
            const rowNumber = startIndex + index + 1;

            const fullName = getFullName(
                user.firstName,
                user.lastName
            );

            const recipientFullName =
                user.recipientFullName || 'بدون محل خدمت';

            const recipientCode =
                user.recipientCode || '-';

            const province =
                user.recipientProvince || '-';

            const city =
                user.recipientCity || '-';

            return `
                <tr>
                    <td>${rowNumber}</td>

                    <td>${escapeHtml(user.userName)}</td>

                    <td>${escapeHtml(user.firstName || '-')}</td>

                    <td>${escapeHtml(user.lastName || '-')}</td>

                    <td>${escapeHtml(recipientFullName)}</td>

                    <td>${escapeHtml(recipientCode)}</td>

                    <td>${escapeHtml(province)}</td>

                    <td>${escapeHtml(city)}</td>

                    <td>
                        <button
                            type="button"
                            class="btn-change-recipient"
                            data-user-id="${user.userId}"
                            data-user-name="${escapeHtml(user.userName || '')}"
                            data-first-name="${escapeHtml(user.firstName || '')}"
                            data-last-name="${escapeHtml(user.lastName || '')}"
                            data-current-recipient="${escapeHtml(recipientFullName)}">

                            <i class="fa fa-exchange"></i>
                            تغییر
                        </button>
                    </td>
                </tr>
            `;
        });

        $tbody.html(rows.join(''));
    };

    const getFullName = (firstName, lastName) => {
        return `${firstName || ''} ${lastName || ''}`.trim() || '-';
    };

    const renderPagination = (total) => {
        const $container = $(selectors.pagination);

        totalPages = Math.ceil(total / pageSize) || 1;

        if (totalPages <= 1) {
            $container.empty();
            return;
        }

        const pages = buildPageNumbers();

        const html = `
            <div class="pager-wrap justify-content-center">
                <ul class="cpageination pager-modern">

                    ${buildPreviousButton()}

                    ${pages.map(buildPageItem).join('')}

                    ${buildNextButton()}

                </ul>
            </div>
        `;

        $container.html(html);

        $container
            .find('.page-link[data-page]')
            .on('click', handlePaginationClick);
    };

    const buildPreviousButton = () => {
        const disabled = currentPage <= 1 ? 'disabled' : '';

        return `
            <li class="page-item ${disabled}">
                <a
                    class="page-link"
                    href="#"
                    data-page="${currentPage - 1}">
                    <i class="fa fa-chevron-right"></i>
                </a>
            </li>
        `;
    };

    const buildNextButton = () => {
        const disabled = currentPage >= totalPages ? 'disabled' : '';

        return `
            <li class="page-item ${disabled}">
                <a
                    class="page-link"
                    href="#"
                    data-page="${currentPage + 1}">
                    <i class="fa fa-chevron-left"></i>
                </a>
            </li>
        `;
    };

    const buildPageNumbers = () => {
        const pages = [];

        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, startPage + 4);

        if (endPage - startPage < 4) {
            startPage = Math.max(1, endPage - 4);
        }

        if (startPage > 1) {
            pages.push(1);

            if (startPage > 2) {
                pages.push('...');
            }
        }

        for (let page = startPage; page <= endPage; page++) {
            pages.push(page);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                pages.push('...');
            }

            pages.push(totalPages);
        }

        return pages;
    };

    const buildPageItem = (page) => {
        if (page === '...') {
            return `
                <li class="page-item ellipsis">
                    <span class="page-link">...</span>
                </li>
            `;
        }

        const active = page === currentPage ? 'active' : '';

        return `
            <li class="page-item ${active}">
                <a
                    class="page-link"
                    href="#"
                    data-page="${page}">
                    ${page}
                </a>
            </li>
        `;
    };

    const handlePaginationClick = (event) => {
        event.preventDefault();

        const page = Number($(event.currentTarget).data('page'));

        if (
            !page ||
            page === currentPage ||
            page < 1 ||
            page > totalPages
        ) {
            return;
        }

        currentPage = page;

        searchUsers();
    };

    const handleChangeRecipient = (event) => {
        event.preventDefault();

        const $button = $(event.currentTarget);

        const userId = $button.data('user-id');
        const userName = $button.data('user-name') || '';
        const firstName = $button.data('first-name') || '';
        const lastName = $button.data('last-name') || '';
        const currentRecipient =
            $button.data('current-recipient') || 'بدون محل خدمت';

        if (!userId) {
            showToast(
                'شناسه کاربر نامعتبر است',
                'error'
            );

            return;
        }

        const fullName = getFullName(firstName, lastName);

        $(selectors.modalUserName).text(fullName);
        $(selectors.modalUserUsername).text(userName);
        $(selectors.modalCurrentRecipient).text(currentRecipient);

        resetRecipientSelect();

        $(selectors.saveButton)
            .data('user-id', userId)
            .prop('disabled', true);

        openModal();
    };

    const resetRecipientSelect = () => {
        const $select = $(selectors.recipientSelect);

        $select.val('');

        const $wrapper = $select.closest('.ss-wrapper');

        if (!$wrapper.length) {
            return;
        }

        $wrapper
            .find('.ss-text')
            .text('جستجوی محل خدمت...');

        $wrapper
            .find('.ss-options')
            .empty();
    };

    const openModal = () => {
        $(selectors.modal).addClass('open');
        $('body').addClass('modal-open');
    };

    const closeModal = () => {
        $(selectors.modal).removeClass('open');
        $('body').removeClass('modal-open');

        resetRecipientSelect();

        $(selectors.saveButton)
            .removeData('user-id')
            .prop('disabled', true);
    };

    const handleModalOverlayClick = (event) => {
        if (event.target === event.currentTarget) {
            closeModal();
        }
    };

    const handleRecipientSearch = (event) => {
        const $input = $(event.currentTarget);

        const value = $input.val().trim();

        const $wrapper = $input.closest('.ss-wrapper');
        const $options = $wrapper.find('.ss-options');

        if (value.length < 2) {
            $options.html(`
                <div class="ss-option-empty">
                    حداقل ۲ کاراکتر وارد کنید
                </div>
            `);

            return;
        }

        $options.html(`
            <div class="ss-option-empty">
                <i class="fa fa-spinner fa-spin"></i>
                در حال جستجو...
            </div>
        `);

        $.ajax({
            url: '/UserManagement/SearchRecipients',
            type: 'GET',
            data: {
                searchTerm: value
            },
            success: (response) => {
                if (!response.success) {
                    $options.html(`
                        <div class="ss-option-empty">
                            ${escapeHtml(
                        response.message ||
                        'خطا در جستجوی محل خدمت'
                    )}
                        </div>
                    `);

                    return;
                }

                renderRecipientOptions(
                    $wrapper,
                    response.data || []
                );
            },
            error: () => {
                $options.html(`
                    <div class="ss-option-empty">
                        خطا در ارتباط با سرور
                    </div>
                `);
            }
        });
    };

    const renderRecipientOptions = ($wrapper, recipients) => {
        const $options = $wrapper.find('.ss-options');

        if (!recipients.length) {
            $options.html(`
                <div class="ss-option-empty">
                    محل خدمتی یافت نشد
                </div>
            `);

            return;
        }

        const html = recipients.map((recipient) => {
            const details = [
                recipient.code,
                recipient.province,
                recipient.city
            ]
                .filter(Boolean)
                .join(' - ');

            const text = details
                ? `${recipient.fullName} (${details})`
                : recipient.fullName;

            return `
                <div
                    class="ss-option"
                    data-value="${recipient.recipientId}">
                    ${escapeHtml(text)}
                </div>
            `;
        });

        $options.html(html.join(''));
    };

    const handleRecipientSelect = (event) => {
        const $option = $(event.currentTarget);

        const $wrapper = $option.closest('.ss-wrapper');

        const value = $option.data('value');
        const text = $option.text().trim();

        $wrapper
            .find('input[type="hidden"]')
            .val(value)
            .trigger('change');

        $wrapper
            .find('.ss-text')
            .text(text);

        $wrapper
            .find('.ss-dropdown')
            .hide();

        $(selectors.saveButton).prop(
            'disabled',
            !value
        );
    };

    const saveRecipient = () => {
        const $button = $(selectors.saveButton);

        const userId = $button.data('user-id');
        const newRecipientId =
            $(selectors.recipientSelect).val();

        if (!userId) {
            showToast(
                'کاربر انتخاب نشده است',
                'error'
            );

            return;
        }

        if (!newRecipientId) {
            showToast(
                'لطفاً محل خدمت جدید را انتخاب کنید',
                'error'
            );

            return;
        }

        setSaveButtonLoading(true);

        $.ajax({
            url: '/UserManagement/ChangeRecipient',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                userId: Number(userId),
                newRecipientId: Number(newRecipientId)
            }),
            success: handleSaveSuccess,
            error: handleSaveError,
            complete: () => {
                setSaveButtonLoading(false);
            }
        });
    };

    const handleSaveSuccess = (response) => {
        if (!response.success) {
            showToast(
                response.message ||
                'خطا در تغییر محل خدمت',
                'error'
            );

            return;
        }

        showToast(
            response.message ||
            'محل خدمت با موفقیت تغییر یافت',
            'success'
        );

        closeModal();

        searchUsers();
    };

    const handleSaveError = (xhr) => {
        console.error('Change recipient error:', xhr);

        showToast(
            xhr.responseJSON?.message ||
            'خطا در ارتباط با سرور',
            'error'
        );
    };

    const setSaveButtonLoading = (isLoading) => {
        const $button = $(selectors.saveButton);

        if (isLoading) {
            $button
                .prop('disabled', true)
                .html(`
                    <i class="fa fa-spinner fa-spin"></i>
                    در حال ذخیره...
                `);

            return;
        }

        $button.html(`
            <i class="fa fa-save"></i>
            ذخیره
        `);
    };

    const escapeHtml = (value) => {
        return $('<div>')
            .text(value ?? '')
            .html();
    };

    return {
        init,
        searchUsers
    };
})();

$(function () {
    manageUserRecipient.init();
});