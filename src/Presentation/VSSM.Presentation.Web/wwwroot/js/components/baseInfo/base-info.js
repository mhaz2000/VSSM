
$(function () {

    $(document).on("click", ".open-modal", function (e) {

        debugger
        e.preventDefault();

        const url = $(this).data("url");

        $("#baseInfoModalContent").load(url, function () {

            $('#baseInfoModal').modal('show');

        });
    });

});