/**
 * dispatch-form.js  —  لاجیک اختصاصی فرم ارسال
 */

var addedItems = [];

$(function () {

    // ════════════════════════════════════════
    //  ۱. مقداردهی اولیه
    // ════════════════════════════════════════

    if (typeof dispatchServerMsg !== 'undefined' && dispatchServerMsg) {
        showToast(dispatchServerMsg, dispatchServerType || 'info');
    }

    // ── DatePicker فیلد تاریخ ارسال ──
    // از همان DatePicker مشترک پروژه (datepicker.js) استفاده می‌شود
    // دقیقاً همان الگوی destroy-uid — pDatepicker اینجا استفاده نمی‌شود
    var $dateInput = $('#Date');

    // placeholder = تاریخ امروز شمسی (از تابع داخلی datepicker.js استفاده نمی‌شود،
    // پس خودمان محاسبه می‌کنیم)
    (function setTodayPlaceholder() {
        var n = new Date();
        var gy = n.getFullYear(), gm = n.getMonth() + 1, gd = n.getDate();
        // تبدیل ساده میلادی به شمسی (همان الگوریتم datepicker.js)
        var g_d_no, j_d_no, j_np, i, j_y, j_m, j_d;
        var g_days = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        var j_days = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
        gy -= 1600; gm -= 1; gd -= 1;
        g_d_no = 365 * gy + Math.floor((gy + 3) / 4) - Math.floor((gy + 99) / 100) + Math.floor((gy + 399) / 400);
        for (i = 0; i < gm; i++) g_d_no += g_days[i];
        if (gm > 1 && ((gy + 1600) % 4 === 0 && ((gy + 1600) % 100 !== 0 || (gy + 1600) % 400 === 0))) g_d_no++;
        g_d_no += gd;
        j_d_no = g_d_no - 79;
        j_np = Math.floor(j_d_no / 12053); j_d_no %= 12053;
        j_y = 979 + 33 * j_np + 4 * Math.floor(j_d_no / 1461);
        j_d_no %= 1461;
        if (j_d_no >= 366) { j_y += Math.floor((j_d_no - 1) / 365); j_d_no = (j_d_no - 1) % 365; }
        for (i = 0; i < 11 && j_d_no >= j_days[i]; i++) j_d_no -= j_days[i];
        j_m = i + 1; j_d = j_d_no + 1;
        var pad = function (x) { return x < 10 ? '0' + x : '' + x; };
        $dateInput.attr('placeholder', j_y + '/' + pad(j_m) + '/' + pad(j_d));
    })();

    DatePicker.attach($dateInput);
    if ($dateInput.val()) $dateInput.addClass('has-value');
    $dateInput.on('change', function () {
        $(this).toggleClass('has-value', !!$(this).val());
    });

    initBatchTable({
        tbodyId: 'itemDetailsTbody',
        showMoreWrap: 'showMoreWrap',
        pageSize: 5,
        emptyText: 'کالایی انتخاب نشده',
        colCount: 8,
    });

    // ════════════════════════════════════════
    //  ۲. helpers برای SearchableSelect
    //     این توابع آیتم‌های ss-options رو
    //     آپدیت می‌کنند بدون re-render کامل
    // ════════════════════════════════════════

    /**
     * پر کردن یک SearchableSelect با آیتم‌های جدید
     * @param {string} wrapperId  - id فیلد hidden (مثلاً 'itemSelect')
     * @param {Array}  items      - آرایه‌ای از { itemId, fullName }
     * @param {string} emptyText  - متن حالت خالی
     */
    function populateSS(wrapperId, items, emptyText) {
        var $hidden = $('#' + wrapperId);
        var $wrapper = $hidden.closest('.ss-wrapper');
        var $options = $wrapper.find('.ss-options');
        var $display = $wrapper.find('.ss-text');

        // ریست مقدار hidden
        $hidden.val('');
        $display.text(emptyText || 'انتخاب کنید...');

        // ساخت آیتم‌های جدید
        var html = '';
        $.each(items, function (_, item) {
            html += '<div class="ss-option" data-value="' + item.itemId + '">' +
                item.fullName +
                '</div><hr style="margin:5px"/>';
        });

        if (!html) {
            html = '<div class="ss-option-empty" style="padding:8px;color:#9ca3af;text-align:center;">' +
                (emptyText || 'موردی یافت نشد') + '</div>';
        }

        $options.html(html);

        // re-bind کلیک روی آیتم‌های جدید
        bindSSOptionClick($wrapper);
    }

    /**
     * bind کردن کلیک روی آیتم‌های ss-option
     */
    function bindSSOptionClick($wrapper) {
        $wrapper.find('.ss-option').off('click').on('click', function () {
            var val = $(this).data('value');
            var text = $(this).text().trim();
            $wrapper.find('input[type="hidden"]').val(val);
            $wrapper.find('.ss-text').text(text);
            $wrapper.find('.ss-dropdown').hide();
            // trigger تغییر برای cascade
            $wrapper.find('input[type="hidden"]').trigger('change');
        });
    }

    // ════════════════════════════════════════
    //  ۳. cascade: نوع کالا ← کالا ← سری ساخت
    // ════════════════════════════════════════

    $('#itemClassSelect').on('change', function () {
        var classId = $(this).val();
        loadBatchRows([]);
        populateSS('itemSelect', [], 'در حال بارگذاری...');

        if (!classId) {
            populateSS('itemSelect', [], 'ابتدا نوع کالا را انتخاب کنید');
            return;
        }

        $.ajax({
            url: dispatchGetItemsByClass,
            data: { classId: classId },
            success: function (items) {
                populateSS('itemSelect', items, 'انتخاب کنید...');
            }
        });
    });

    // وقتی کالا از SearchableSelect انتخاب شد
    $('#itemSelect').on('change', function () {
        var itemId = $(this).val();
        loadBatchRows([]);
        if (!itemId) return;

        $.ajax({
            url: dispatchGetBatchesByItem,
            data: { itemId: itemId },
            success: function (rows) {
                loadBatchRows(rows);
            }
        });
    });

    // ════════════════════════════════════════
    //  ۴. افزودن آیتم به لیست
    // ════════════════════════════════════════

    window.addSelectedItem = function () {
        var $checked = $('.batch-chk:checked');
        if ($checked.length === 0) {
            showToast('لطفاً یک سری ساخت را انتخاب کنید.', 'error');
            return;
        }

        var doseCount = parseInt($('#doseCountInput').val()) || 0;
        if (doseCount <= 0) {
            showToast('برای کلیه کالاهای ارسالی تعداد را مشخص نمایید', 'error');
            return;
        }

        var row = JSON.parse($checked.first().attr('data-row').replace(/&#39;/g, "'"));
        var vvmStageId = $('#vvmStagesSelect').val();
        var vvmStageName = $('#vvmStagesSelect option:selected').text();

        var item = {
            idx: addedItems.length,
            itemDetailStoreId: row.itemDetailStoreId,
            itemId: row.itemId,
            itemName: row.itemName,
            batchNumber: row.batchNumber,
            expiryDateShamsi: row.expiryDateShamsi,
            expiryDate: row.expiryDate,
            storeId: row.storeId,
            storeName: row.storeName,
            manufacturerName: row.manufacturerName,
            doseCount: doseCount,
            numVial: row.dosePerVialNum > 0 ? Math.floor(doseCount / row.dosePerVialNum) : 0,
            vvmStageId: vvmStageId,
            vvmStageName: vvmStageName,
            note: '',
            uids: [],
            stockInRam: row.currentStock - doseCount,
            requiresDiluent: row.requiresDiluent,
            diluentItemDetailStoreId: row.diluentItemDetailStoreId,
            diluentItemId: row.diluentItemId,
            diluentItemName: row.diluentItemName,
        };

        addedItems.push(item);
        renderAddedItems();

        if (row.requiresDiluent && row.diluentItemDetailStoreId) {
            addedItems.push({
                idx: addedItems.length,
                itemDetailStoreId: row.diluentItemDetailStoreId,
                itemId: row.diluentItemId,
                itemName: (row.diluentItemName || '') + ' (حلال - خودکار)',
                batchNumber: row.diluentBatchNumber || '',
                expiryDateShamsi: row.diluentExpiryDateShamsi || '-',
                expiryDate: row.diluentExpiryDate || null,
                storeId: row.storeId,
                storeName: row.storeName,
                manufacturerName: row.diluentManufacturerName || '-',
                doseCount: doseCount,
                numVial: 0,
                vvmStageId: '',
                vvmStageName: '-',
                note: 'حلال خودکار',
                uids: [],
                stockInRam: 0,
                requiresDiluent: false,
            });
            renderAddedItems();
        }

        $checked.prop('checked', false);
        $('#doseCountInput').val('');
    };

    // ════════════════════════════════════════
    //  ۵. حذف آیتم
    // ════════════════════════════════════════

    window.removeItem = function (idx) {
        var item = addedItems[idx];
        var $stockCell = $('.stock-cell-' + item.itemDetailStoreId);
        if ($stockCell.length) {
            $stockCell.text((parseInt($stockCell.text()) || 0) + item.doseCount);
        }
        addedItems.splice(idx, 1);
        $.each(addedItems, function (i, it) { it.idx = i; });
        renderAddedItems();
    };

    // ════════════════════════════════════════
    //  ۶. رندر جدول کالاهای افزوده‌شده
    // ════════════════════════════════════════

    function renderAddedItems() {
        var tbody = $('#addedItemsTbody');

        if (addedItems.length === 0) {
            tbody.html(
                '<tr id="noAddedRow"><td colspan="10" ' +
                'style="padding:24px;color:#9ca3af;text-align:center;">کالایی اضافه نشده</td></tr>'
            );
            rebuildHiddenInputs();
            return;
        }

        var html = '';
        $.each(addedItems, function (i, item) {
            html +=
                '<tr>' +
                '<td>' + (item.batchNumber || '-') + '</td>' +
                '<td>' + (item.itemName || '-') + '</td>' +
                '<td>' + (item.expiryDateShamsi || '-') + '</td>' +
                '<td>' + item.doseCount + '</td>' +
                '<td>' + item.numVial + '</td>' +
                '<td>-</td>' +
                '<td>' + (item.vvmStageName || '-') + '</td>' +
                '<td>' + (item.manufacturerName || '-') + '</td>' +
                '<td>' + (item.note || '-') + '</td>' +
                '<td style="white-space:nowrap;">' +
                '<button type="button" class="btn-action btn-action-uid" title="افزودن UID" ' +
                'onclick="openUidModal(' + i + ')">' +
                '<i class="fa fa-qrcode"></i>' +
                '</button> ' +
                '<button type="button" class="btn-action btn-action-del" title="حذف" ' +
                'onclick="removeItem(' + i + ')">' +
                '<i class="fa fa-trash"></i>' +
                '</button>' +
                '</td>' +
                '</tr>';
        });

        tbody.html(html);
        rebuildHiddenInputs();
    }

    // ════════════════════════════════════════
    //  ۷. ساخت hidden inputs برای submit
    // ════════════════════════════════════════

    function rebuildHiddenInputs() {
        var $container = $('#hiddenInputsContainer').empty();

        $.each(addedItems, function (i, item) {
            function add(name, val) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].' + name + '" value="' + (val || '') + '">'
                );
            }
            add('ItemDetailStoreId', item.itemDetailStoreId);
            add('ItemId', item.itemId);
            add('StoreId', item.storeId);
            add('DoseCount', item.doseCount);
            add('VvmStageId', item.vvmStageId);
            add('BatchNumber', item.batchNumber);
            add('ExpiryDate', item.expiryDate);
            add('Note', item.note);

            $.each(item.uids, function (j, uid) {
                $container.append(
                    '<input type="hidden" name="Items[' + i + '].Uids[' + j + ']" value="' + uid + '">'
                );
            });
        });
    }

    // ════════════════════════════════════════
    //  ۸. دکمه‌های footer
    // ════════════════════════════════════════

    $('.js-footer-btn').on('click', function () {
        var submitType = $(this).data('submit');

        var hasZero = false;
        $.each(addedItems, function (_, it) {
            if (!it.doseCount || it.doseCount <= 0) { hasZero = true; return false; }
        });
        if (hasZero) {
            showToast('برای کلیه کالاهای ارسالی تعداد را مشخص نمایید', 'error');
            return;
        }
        if (addedItems.length === 0) {
            showToast('حداقل یک کالا را اضافه کنید.', 'error');
            return;
        }

        $('#toastContainer').empty();
        $('#dispatchForm').find('input[name="submitType"]').remove();
        $('<input>').attr({ type: 'hidden', name: 'submitType', value: submitType })
            .appendTo('#dispatchForm');
        $('#dispatchForm').submit();
    });

    window.confirmDelete = function () {
        if (confirm('آیا از حذف این ارسال اطمینان دارید؟')) {
            $('#dispatchForm').find('input[name="submitType"]').remove();
            $('<input>').attr({ type: 'hidden', name: 'submitType', value: 'delete' })
                .appendTo('#dispatchForm');
            $('#dispatchForm').submit();
        }
    };

    // ════════════════════════════════════════
    //  ۹. bind اولیه SearchableSelect های موجود
    // ════════════════════════════════════════

    $('.ss-wrapper').each(function () {
        bindSSOptionClick($(this));

        // باز/بسته کردن dropdown
        $(this).find('.ss-display').on('click', function () {
            var $dd = $(this).siblings('.ss-dropdown');
            $('.ss-dropdown').not($dd).hide();
            $dd.toggle();
            if ($dd.is(':visible')) {
                $dd.find('.ss-search').focus();
            }
        });

        // فیلتر جستجو
        $(this).find('.ss-search').on('input', function () {
            var q = $(this).val().toLowerCase();
            $(this).closest('.ss-dropdown').find('.ss-option').each(function () {
                var match = $(this).text().toLowerCase().indexOf(q) > -1;
                $(this).toggle(match);
                $(this).next('hr').toggle(match);
            });
        });
    });

    // بستن dropdown با کلیک بیرون
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.ss-wrapper').length) {
            $('.ss-dropdown').hide();
        }
    });

});