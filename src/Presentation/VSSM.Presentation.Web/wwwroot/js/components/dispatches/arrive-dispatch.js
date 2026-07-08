/**
 * arrive-dispatch.js  —  منطق صفحه دریافت محموله
 * اعتبارسنجی تطابق نوع انبار با کالا
 */
(function () {

    // For every store dropdown on the page, validate the chosen store's
    // type against the item's allowed store flags (above-zero / below-zero / ordinary).
    document.querySelectorAll('.store-selector').forEach(function (select) {
        select.addEventListener('change', function () {

            var row = select.closest('tr');

            // Flags come from data-* attributes rendered per row in Arrive.cshtml.
            var itemFlags = {
                aboveZero: row.dataset.useAboveZero === "True",
                belowZero: row.dataset.useBelowZero === "True",
                ordinary: row.dataset.useOrdinary === "True"
            };

            var selectedOption = select.options[select.selectedIndex];
            var storeTypeId = parseInt(selectedOption.dataset.typeId);
            var warningDiv = row.querySelector('.store-warning');

            // No store selected (or no type id) -> clear any warning and bail out.
            if (!storeTypeId) {
                warningDiv.style.display = 'none';
                row.classList.remove('row-danger');
                return;
            }

            var valid = true;
            var warningMessage = "⚠️ نوع انبار انتخاب شده با این کالا سازگار نیست!";

            // Store type ids: 1 = above-zero, 2 = below-zero, 3 = ordinary.
            if (![1, 2, 3].includes(storeTypeId)) {
                valid = false;
                warningMessage = "⚠️ نوع انبار مشخص نشده است!";
            } else {
                if (storeTypeId === 1 && !itemFlags.aboveZero) valid = false;
                if (storeTypeId === 2 && !itemFlags.belowZero) valid = false;
                if (storeTypeId === 3 && !itemFlags.ordinary) valid = false;
            }

            if (!valid) {
                warningDiv.textContent = warningMessage;
                warningDiv.style.display = 'block';
                row.classList.add('row-danger');
            } else {
                warningDiv.style.display = 'none';
                row.classList.remove('row-danger');
            }
        });
    });

})();