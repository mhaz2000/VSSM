
$(function () {

    $(document).on("click", ".open-modal", function (e) {

        e.preventDefault();

        const url = $(this).data("url");

        $("#baseInfoModalContent").load(url, function () {

            $('#baseInfoModal').modal('show');

        });
    });

});