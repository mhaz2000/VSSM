/**
 * pagination-table.js - کامپوننت صفحه‌بندی و جستجو (نسخه ساده با انتخاب سایز)
 * جستجو در بالا، صفحه‌بندی در پایین وسط + انتخاب ۵ یا ۱۰
 */

(function (global) {
    'use strict';

    function PaginationTable(options) {
        this.options = $.extend({
            tableId: null,
            tbodyId: null,
            dataUrl: null,
            pageSize: 5,
            pageSizeOptions: [5, 10],
            columns: [],
            searchFields: [],
            rowRenderer: null,
            emptyMessage: 'موردی یافت نشد',
            onDataLoaded: null,
            onRowClick: null,
            containerSelector: null,
        }, options);

        if (!this.options.tableId || !this.options.tbodyId) {
            console.error('PaginationTable: tableId و tbodyId الزامی هستند');
            return;
        }

        this.currentPage = 1;
        this.allData = [];
        this.filteredData = [];
        this.totalPages = 0;
        this.isLoading = false;
        this.searchTerm = '';

        this.$wrapper = null;
        this.$searchInput = null;
        this.$prevBtn = null;
        this.$nextBtn = null;
        this.$pageInfo = null;

        this.init();
    }

    // ─── مقداردهی اولیه ───
    PaginationTable.prototype.init = function () {
        var $table = $('#' + this.options.tableId);
        if ($table.length === 0) {
            console.error('PaginationTable: جدول با شناسه "' + this.options.tableId + '" یافت نشد');
            return;
        }

        var $container = this.options.containerSelector ?
            $(this.options.containerSelector) :
            $table.closest('.lines-table-wrap');

        if ($container.length === 0) {
            $container = $table.parent();
        }

        this.$wrapper = $container;

        if ($container.find('.pagination-controls').length === 0) {
            var controlsHtml = this.buildControlsHtml();
            $container.prepend(controlsHtml);

            var $bottombar = $container.find('.pagination-bottombar');
            if ($bottombar.length) {
                $bottombar.detach();
                $container.append($bottombar);
            }
        }

        this.$searchInput = $container.find('.pagination-search-input');
        this.$prevBtn = $container.find('.btn-pagination-prev');
        this.$nextBtn = $container.find('.btn-pagination-next');
        this.$pageInfo = $container.find('.pagination-info');

        this.attachEvents();

        if (this.options.dataUrl) {
            this.loadData();
        }
    };

    PaginationTable.prototype.buildControlsHtml = function () {
        var self = this;
        var optionsHtml = '';

        $.each(this.options.pageSizeOptions, function (_, size) {

            var selected = size === self.options.pageSize ? ' selected' : '';

            optionsHtml +=
                '<option value="' + size + '"' + selected + '>' +
                size
                '</option>';
        });


        return `
    <div class="pagination-controls">


        <!-- Header -->
        <div class="pagination-topbar">


            <div class="pagination-count">
                نمایش 
                <strong id="paginationDisplayCount">0</strong>
                از
                <strong id="paginationTotalCount">0</strong>
                مورد
            </div>



            <div class="pagination-search-wrap">

                <input type="text"
                       class="pagination-search-input"
                       placeholder="جستجو در اطلاعات..." />

            </div>


        </div>



        <!-- Footer -->
        <div class="pagination-bottombar">



            <!-- Navigation -->
            <div class="pagination-buttons">


                <button type="button"
                        class="btn-pagination btn-pagination-prev"
                        disabled
                        title="صفحه قبل">

                    <i class="fa fa-chevron-right"></i>

                </button>



                <span class="pagination-info">

                    <span class="page-current">۱</span>

                    از

                    <span class="page-total">۱</span>

                </span>



                <button type="button"
                        class="btn-pagination btn-pagination-next"
                        disabled
                        title="صفحه بعد">

                    <i class="fa fa-chevron-left"></i>

                </button>



            </div>




            <!-- Page Size -->
            <div class="pagination-page-size">


                <label for="pageSizeSelect">
                       ردیف:
                </label>



                <select id="pageSizeSelect"
                        class="pagination-size-select">

                    ${optionsHtml}

                </select>


            </div>



        </div>


    </div>
    `;
    };    // ─── اتصال رویدادها ───
    PaginationTable.prototype.attachEvents = function () {
        var self = this;

        // جستجو
        this.$searchInput.on('input', function () {
            self.searchTerm = $(this).val().trim();
            self.applyFilter();
        });

        // قبلی
        this.$prevBtn.on('click', function () {
            if (self.currentPage > 1) {
                self.currentPage--;
                self.render();
            }
        });

        // بعدی
        this.$nextBtn.on('click', function () {
            if (self.currentPage < self.totalPages) {
                self.currentPage++;
                self.render();
            }
        });

        // تغییر سایز صفحه
        this.$wrapper.find('#pageSizeSelect').on('change', function () {
            var newSize = parseInt($(this).val(), 10);
            if (!isNaN(newSize) && newSize > 0) {
                self.options.pageSize = newSize;
                self.currentPage = 1;
                self.render();
            }
        });

        // کلیک روی ردیف
        var $tbody = $('#' + this.options.tbodyId);
        if (typeof this.options.onRowClick === 'function') {
            $tbody.on('click', 'tr', function (e) {
                var $row = $(this);
                var index = $row.data('index');
                if (index !== undefined) {
                    var rowData = self.filteredData[index];
                    self.options.onRowClick(rowData, $row, e);
                }
            });
        }
    };

    // ─── بارگذاری داده ───
    PaginationTable.prototype.loadData = function () {
        var self = this;
        if (!this.options.dataUrl) return;

        this.isLoading = true;
        this.showLoadingState();

        $.ajax({
            url: this.options.dataUrl,
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                self.allData = data || [];
                self.filteredData = self.allData.slice();
                self.applyFilter();
                self.isLoading = false;
                if (typeof self.options.onDataLoaded === 'function') {
                    self.options.onDataLoaded(data);
                }
            },
            error: function (xhr) {
                self.isLoading = false;
                var message = 'خطا در بارگذاری داده';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                }
                if (typeof showToast === 'function') {
                    showToast(message, 'error');
                } else {
                    alert(message);
                }
                var $tbody = $('#' + self.options.tbodyId);
                $tbody.html('<tr><td colspan="' + self.options.columns.length + '" style="padding:24px;color:#dc2626;text-align:center;">' + message + '</td></tr>');
            }
        });
    };

    // ─── اعمال فیلتر ───
    PaginationTable.prototype.applyFilter = function () {
        var term = this.searchTerm.toLowerCase();

        if (term === '') {
            this.filteredData = this.allData.slice();
        } else {
            var searchFields = this.options.searchFields;
            this.filteredData = this.allData.filter(function (item) {
                for (var i = 0; i < searchFields.length; i++) {
                    var field = searchFields[i];
                    var val = item[field];
                    if (val && val.toString().toLowerCase().indexOf(term) !== -1) {
                        return true;
                    }
                }
                return false;
            });
        }

        this.currentPage = 1;
        this.render();
    };

    // ─── رندر ───
    PaginationTable.prototype.render = function () {
        var totalItems = this.filteredData.length;
        this.totalPages = Math.max(1, Math.ceil(totalItems / this.options.pageSize));
        if (this.currentPage > this.totalPages) {
            this.currentPage = this.totalPages;
        }

        var start = (this.currentPage - 1) * this.options.pageSize;
        var end = Math.min(start + this.options.pageSize, totalItems);
        var pageData = this.filteredData.slice(start, end);

        var $tbody = $('#' + this.options.tbodyId);
        if (this.isLoading) {
            $tbody.html('<tr><td colspan="' + this.options.columns.length + '" style="padding:24px;color:#9ca3af;text-align:center;">در حال بارگذاری...</td></tr>');
            this.updateCounters(0, 0);
            this.updatePaginationInfo();
            return;
        }

        if (pageData.length === 0) {
            $tbody.html('<tr><td colspan="' + this.options.columns.length + '" style="padding:24px;color:#9ca3af;text-align:center;">' + this.options.emptyMessage + '</td></tr>');
        } else {
            var html = '';
            for (var i = 0; i < pageData.length; i++) {
                var rowData = pageData[i];
                var rowIndex = start + i;
                var rowHtml = '';

                if (typeof this.options.rowRenderer === 'function') {
                    rowHtml = this.options.rowRenderer(rowData, rowIndex);
                } else {
                    rowHtml = '<tr data-index="' + rowIndex + '">';
                    for (var col of this.options.columns) {
                        var value = rowData[col.field] !== undefined ? rowData[col.field] : '-';
                        rowHtml += '<td>' + value + '</td>';
                    }
                    rowHtml += '</tr>';
                }
                html += rowHtml;
            }
            $tbody.html(html);
        }

        var displayCount = Math.max(0, end - start);
        this.updateCounters(displayCount, totalItems);
        this.updatePaginationInfo();

        this.$prevBtn.prop('disabled', this.currentPage <= 1);
        this.$nextBtn.prop('disabled', this.currentPage >= this.totalPages);
    };

    // ─── به‌روزرسانی شمارنده‌ها ───
    PaginationTable.prototype.updateCounters = function (display, total) {
        var $displayCount = this.$wrapper.find('#paginationDisplayCount');
        var $totalCount = this.$wrapper.find('#paginationTotalCount');

        if ($displayCount.length) {
            $displayCount.text(display);
        }
        if ($totalCount.length) {
            $totalCount.text(total);
        }
    };

    // ─── به‌روزرسانی اطلاعات صفحه ───
    PaginationTable.prototype.updatePaginationInfo = function () {
        if (this.$pageInfo.length) {
            this.$pageInfo.html(
                'صفحه <span class="page-current">' + this.currentPage + '</span> از <span class="page-total">' + this.totalPages + '</span>'
            );
        }

        // به‌روزرسانی select سایز صفحه
        var $select = this.$wrapper.find('#pageSizeSelect');
        if ($select.length) {
            $select.val(this.options.pageSize);
        }
    };

    // ─── وضعیت بارگذاری ───
    PaginationTable.prototype.showLoadingState = function () {
        var $tbody = $('#' + this.options.tbodyId);
        $tbody.html('<tr><td colspan="' + this.options.columns.length + '" style="padding:24px;color:#9ca3af;text-align:center;">در حال دریافت اطلاعات...</td></tr>');
        this.updateCounters(0, 0);
        this.updatePaginationInfo();
    };

    // ─── متدهای عمومی ───
    PaginationTable.prototype.setData = function (data) {
        this.allData = data || [];
        this.filteredData = this.allData.slice();
        this.applyFilter();
    };

    PaginationTable.prototype.search = function (term) {
        this.searchTerm = term.trim();
        this.applyFilter();
    };

    PaginationTable.prototype.goToPage = function (pageNumber) {
        if (pageNumber >= 1 && pageNumber <= this.totalPages) {
            this.currentPage = pageNumber;
            this.render();
        }
    };

    PaginationTable.prototype.refresh = function () {
        if (this.options.dataUrl) {
            this.loadData();
        } else {
            this.applyFilter();
        }
    };

    PaginationTable.prototype.getFilteredData = function () {
        return this.filteredData;
    };

    // ─── expose ───
    global.PaginationTable = PaginationTable;

})(window);