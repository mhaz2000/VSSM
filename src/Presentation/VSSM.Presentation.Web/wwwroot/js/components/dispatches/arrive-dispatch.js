/**
 * arrive-dispatch.js  —  منطق صفحه دریافت محموله
 * اعتبارسنجی تطابق نوع انبار با کالا
 */
(function () {

    document.querySelectorAll('.store-selector').forEach(function (select) {
        select.addEventListener('change', function () {

            var row = select.closest('tr');

            var itemFlags = {
                aboveZero: row.dataset.useAboveZero === "True",
                belowZero: row.dataset.useBelowZero === "True",
                ordinary: row.dataset.useOrdinary === "True"
            };

            var selectedOption = select.options[select.selectedIndex];
            var storeTypeId = parseInt(selectedOption.dataset.typeId);
            var warningDiv = row.querySelector('.store-warning');

            if (!storeTypeId) {
                warningDiv.style.display = 'none';
                row.classList.remove('row-danger');
                return;
            }

            var valid = true;
            var warningMessage = "⚠️ نوع انبار انتخاب شده با این کالا سازگار نیست!";

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