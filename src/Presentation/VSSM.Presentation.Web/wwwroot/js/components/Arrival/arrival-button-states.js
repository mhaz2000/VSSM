/**
 * arrival-button-states.js
 * مدیریت وضعیت دکمه‌ها (افزودن ردیف، ثبت، پیش‌نویس)
 * وابسته به: jQuery
 */

$(function () {
    "use strict";

    // ── به‌روزرسانی دکمه افزودن ──
    window.updateAddButtonState = function () {
        var isHalal = ($("#ArrivalTypeHidden").val() === "halal");
        var allFilled = true;

        if (isHalal) {
            var fields = [
                "#HalalItemSelect",
                "#HalalDoseSelect",
                "#QuantityInput",
                "#NumVialsInput",
                "#HalalProdDate",
                "#HalalExpDate"
            ];
            $.each(fields, function (idx, sel) {
                var $field = $(sel);
                if ($field.is("select")) {
                    if (!$field.val()) allFilled = false;
                } else {
                    if (!$field.val().trim()) allFilled = false;
                }
            });
        } else {
            var batch = $("#BatchNum").val().trim();
            var store = $("#NewLine_StoreId").val();
            var itemId = $("#ItemIdHidden").val();
            var doseId = $("#DosePerVialIdHidden").val();
            var prodDate = $("#ProductionDate").val();
            var expDate = $("#ExpiryDate").val();
            if (!batch || !store || !itemId || !doseId || !prodDate || !expDate) {
                allFilled = false;
            }
        }

        $("#addLineBtn").prop("disabled", !allFilled);
    };

    // ── به‌روزرسانی دکمه‌های ثبت و پیش‌نویس ──
    window.updateFooterButtonsState = function () {
        var $rows = $(".lines-table tbody tr:not(.empty-row)");
        var hasRows = ($rows.length > 0);
        $(".js-submit-btn[data-command='confirm'], .js-submit-btn[data-command='draft']")
            .prop("disabled", !hasRows);
    };

    // ── رویدادهای تغییر ──
    $(document).on("change input keyup",
        "#BatchNum, #HalalItemSelect, #HalalDoseSelect, #QuantityInput, #NumVialsInput, #HalalProdDate, #HalalExpDate, #NewLine_StoreId",
        function () {
            window.updateAddButtonState();
        }
    );

    $(document).ajaxSuccess(function (e, xhr, settings) {
        if (settings.url && settings.url.indexOf("GetBatchInfo") > -1) {
            window.updateAddButtonState();
        }
    });

    $(document).on("click", ".arrival-tab", function () {
        setTimeout(function () {
            window.updateAddButtonState();
            window.updateFooterButtonsState();
        }, 100);
    });

    // ── رصد تغییرات جدول ──
    var observer = new MutationObserver(function () {
        window.updateFooterButtonsState();
    });
    var tableBody = document.querySelector(".lines-table tbody");
    if (tableBody) {
        observer.observe(tableBody, { childList: true, subtree: true });
    }

    // ── مقداردهی اولیه ──
    window.updateAddButtonState();
    window.updateFooterButtonsState();
});