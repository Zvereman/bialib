$(document).ready(function () {
    $('#requestBookModal').on('shown.bs.modal', function () {
        $('#requestBtn').focus();
    });

    $('#requestBookBtn').click(function (e) {
        //$('#toastSucces').toast('show');
        //$('#toastFail').toast('show');
        //
        var biaId = $(this).data('content');
        var url = window.location.origin + '/Visitor/RentBook';
        var btnClose = $('#btnClose');
        var btn = this;

        SendAction(biaId, url, btn, btnClose);
    });
});

function SendAction(model, url, btn, btnClose) {
    if (btn !== null) {
        $(btn).prop('disabled', true);
    }
    if (btnClose !== null) {
        $(btnClose).prop('disabled', true);
    }

    $.ajax({
        type: "POST",
        data: JSON.stringify(model),
        url: url,
        contentType: "application/json"
    }).done(function (res) {
        if (res.status === 'Success') {
            $('#toastSucces').toast('show');
        } else {
                $('#toastFail').toast('show');
        }
        setTimeout(function () {
            location.reload();
        }, 1000);

    }).fail(function (res) {
        $('#toastFail').toast('show');
    }).always(function () {
        if (btn !== null) {
            $(btn).prop('disabled', false);
        }
        if (btnClose !== null) {
            $(btnClose).prop('disabled', false);
        }
    });
}