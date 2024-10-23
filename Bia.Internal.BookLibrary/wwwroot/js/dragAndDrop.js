var files = null;

$(document).ready(function () {
    const $fileUpload = $('#file-upload');
    const $fileNameDisplay = $('#file-name');
    const $fileSizeDisplay = $('#file-size');
    const $fileDelBtn = $('#file-del');

    $fileUpload.on('dragover', function (event) {
        event.preventDefault();
        event.stopPropagation();
        $fileUpload.addClass('dragover');
    });

    $fileUpload.on('dragleave', function (event) {
        event.preventDefault();
        event.stopPropagation();
        $fileUpload.removeClass('dragover');
    });

    $fileUpload.on('drop', function (event) {
        event.preventDefault();
        event.stopPropagation();
        $fileUpload.removeClass('dragover');
        files = event.originalEvent.dataTransfer.files;
        handleFiles(files);
    });

    $fileUpload.on('click', function () {
        const $fileInput = $('<input id="fileAtt" multiple="false" type="file" style="display: none;" accept=".png, .jpeg">');
        $('body').append($fileInput);
        $fileInput.trigger('click');
        $fileInput.on('change', function (event) {
            files = event.target.files;
            handleFiles(files);
            //$fileInput.remove();
        });
    });

    $fileDelBtn.on('click', function () {
        $fileNameDisplay.text('');
        $fileSizeDisplay.text('');
        $fileDelBtn.hide();
        files = null;
    })

    function handleFiles(files) {
        if (files.length > 0) {
            const file = files[0];
            const fileType = file.type;

            if (fileType !== 'image/png' && fileType !== 'image/jpeg') {
                $('#toastFailAddFiles').toast('show');
                return;
            }

            $fileNameDisplay.text(file.name);
            $fileSizeDisplay.text((file.size / 1000).toFixed(2) + " KB");
            $fileDelBtn.show();
            
            //$fileNameDisplay.text(files[0].name);
            //$fileSizeDisplay.text(files[0].size / 1000 + " KB");
            //$fileDelBtn.show();
        } else {
            $fileNameDisplay.text('');
            $fileSizeDisplay.text('');
            $fileDelBtn.hide();
        }
    }
});

function uploadFile() {
    //
    var bookId = getParameterByName('bookId');
    const formData = new FormData();
    formData.append('bookId', bookId);
    if (files != null) {
        formData.append('file', files[0]);
    }

    $.ajax({
        url: 'AddCover',
        method: 'POST',
        contentType: false,
        processData: false,
        data: formData,
        success: function (response) {
            if (response.status.status === 'Success') {
                const coverImagePath = response.details;
                addBook(coverImagePath);
            } else {
                $('#toastFail').toast('show');
                console.error('Ошибка:', response.details);
                $('#addOrEditBtn').prop('disabled', false);
            }
        },
        error: function (error) {
            console.error('Ошибка:', error);
        }
    });
}

//function addBook(coverImagePath) {
//    //
//    var countError = 0;
//    var txtBoxBookName = $('#txtBoxBookName').val();
//    //var txtBoxBookSubtitle = $('#txtBoxBookSubtitle').val();
//    //var txtBoxDescription = $('#txtBoxDescription').val();
//    //var txtBoxBookPages = $('#txtBoxBookPages').val();
//    var txtBoxAuthor = $('#autoCompleteAuthors');
//    var txtBoxCategory = $('#autoCompleteCategory');
//    //var txtBoxYear = $('#customSelectInput').val();
//    //
//    if (txtBoxBookName.length == 0) {
//        $('#txtBoxBookName').parent().find('.invalid').show();
//        countError++;
//        //return;
//    }
//    else {
//        $('#txtBoxBookName').parent().find('.invalid').hide();
//    }
//    //
//    //if (txtBoxBookSubtitle.length == 0) {
//    //    $('#txtBoxBookSubtitle').parent().find('.invalid').show();
//    //    countError++
//    //    //return;
//    //}
//    //else {
//    //    $('#txtBoxBookSubtitle').parent().find('.invalid').hide();
//    //}
//    //
//    //if (txtBoxDescription.length == 0) {
//    //    $('#txtBoxDescription').parent().find('.invalid').show();
//    //    countError++;
//    //    //return;
//    //}
//    //else {
//    //    $('#txtBoxDescription').parent().find('.invalid').hide();
//    //}
//    ////
//    //if (txtBoxBookPages == 0 || $('#txtBoxBookPages').val()[0] == 0) {
//    //    $('#txtBoxBookPages').parent().find('.invalid').show();
//    //    countError++;
//    //    //return;
//    //}
//    //else {
//    //    $('#txtBoxBookPages').parent().find('.invalid').hide();
//    //}
//    //
//    //if (txtBoxYear.length == 0) {
//    //    $('#customSelectInput').parent().find('.invalid').show();
//    //    countError++;
//    //    //return;
//    //}
//    //else {
//    //    $('#customSelectInput').parent().find('.invalid').hide();
//    //}
//    //
//    if (txtBoxAuthor.parent().find('.tags').children().length == 0) {
//        txtBoxAuthor.parent().parent().find('.invalid').show()
//        countError++;
//    }
//    else {
//        txtBoxAuthor.parent().parent().find('.invalid').hide()
//    }
//    //
//    if (txtBoxCategory.parent().find('.tags').children().length == 0) {
//        txtBoxCategory.parent().parent().find('.invalid').show()
//        countError++;
//    }
//    else {
//        txtBoxCategory.parent().parent().find('.invalid').hide()
//    }
//    //
//    if (countError != 0)
//        return;
//    //
//    $('#addOrEditBtn').prop('disabled', true);
//    var bookId = getParameterByName('bookId');
//    var formData = new FormData();
//    //formData.append('bookId', bookId);
//    if (files != null) {
//        formData.append('file', files[0]);
//    }
//    //
//    const bookData = {
//        biaId: bookId,
//        title: $('#txtBoxBookName').val(),
//        subtitle: $('#txtBoxBookSubtitle').val(),
//        annotation: $('#txtBoxDescription').val(),
//        year: $('#txtBoxYear').val(),
//        pages: $('#txtBoxBookPages').val(),
//        language: $('#txtBoxLenguege').val(),
//        authors: $('#authorUIDs').val().split(';'),
//        categories: $('#categoryIDs').val().split(';'),
//        coverImage: formData
//    };

//    $.ajax({
//        url: 'AddBook',
//        method: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(bookData),
//        success: function (response) {
//            if (response.status === 'Success') {
//                if (bookId == '0')
//                    $('#toastSuccesAdd').toast('show');
//                else
//                    $('#toastSuccesEdit').toast('show');
//                setTimeout(function () {
//                    window.location.reload();
//                }, 500);
//            } else {
//                $('#toastFail').toast('show');
//                console.error('Ошибка:', response.details);
//                $('#addOrEditBtn').prop('disabled', false);
//            }
//        },
//        error: function (error) {
//            console.error('Ошибка:', error);
//        }
//    });
//}

function addBook() {
    var countError = 0;
    var txtBoxBookName = $('#txtBoxBookName').val().replace(/\s+/g, ' ').trim();
    //var txtBoxBookSubtitle = $('#txtBoxBookSubtitle').val();
    //var txtBoxDescription = $('#txtBoxDescription').val();
    var txtBoxBookPages = $('#txtBoxBookPages').val();
    var txtBoxAuthor = $('#autoCompleteAuthors');
    var txtBoxCategory = $('#autoCompleteCategory');
    //var txtBoxYear = $('#customSelectInput').val();
    //
    if (txtBoxBookName.length == 0) {
        $('#txtBoxBookName').parent().find('.invalid').show();
        countError++;
        //return;
    }
    else {
        $('#txtBoxBookName').parent().find('.invalid').hide();
    }
    //
    //if (txtBoxBookSubtitle.length == 0) {
    //    $('#txtBoxBookSubtitle').parent().find('.invalid').show();
    //    countError++
    //    //return;
    //}
    //else {
    //    $('#txtBoxBookSubtitle').parent().find('.invalid').hide();
    //}
    //
    //if (txtBoxDescription.length == 0) {
    //    $('#txtBoxDescription').parent().find('.invalid').show();
    //    countError++;
    //    //return;
    //}
    //else {
    //    $('#txtBoxDescription').parent().find('.invalid').hide();
    //}
    ////
    if ($('#txtBoxBookPages').val()[0] == 0 && txtBoxBookPages.length > 1) {
        $('#txtBoxBookPages').parent().find('.invalid').show();
        countError++;
        //return;
    }
    else {
        $('#txtBoxBookPages').parent().find('.invalid').hide();
    }
    //
    //if (txtBoxYear.length == 0) {
    //    $('#customSelectInput').parent().find('.invalid').show();
    //    countError++;
    //    //return;
    //}
    //else {
    //    $('#customSelectInput').parent().find('.invalid').hide();
    //}
    //
    if (txtBoxAuthor.parent().find('.tags').children().length == 0) {
        txtBoxAuthor.parent().parent().find('.invalid').show()
        countError++;
    }
    else {
        txtBoxAuthor.parent().parent().find('.invalid').hide()
    }
    //
    if (txtBoxCategory.parent().find('.tags').children().length == 0) {
        txtBoxCategory.parent().parent().find('.invalid').show()
        countError++;
    }
    else {
        txtBoxCategory.parent().parent().find('.invalid').hide()
    }
    //
    if (countError != 0)
        return;
    //
    $('#addOrEditBtn').prop('disabled', true);

    var formData = new FormData();
    formData.append('biaId', getParameterByName('bookId'));
    formData.append('title', $('#txtBoxBookName').val());
    formData.append('subtitle', $('#txtBoxBookSubtitle').val());
    formData.append('annotation', $('#txtBoxDescription').val());
    formData.append('year', $('#txtBoxYear').val());
    formData.append('pages', $('#txtBoxBookPages').val());
    formData.append('language', $('#txtBoxLenguege').val());

    // Добавляем файл обложки
    if (files != null) {
        formData.append('CoverImage', files[0]);
    }

    // Добавляем авторов и категории
    var authors = $('#authorUIDs').val().split(';');
    var categories = $('#categoryIDs').val().split(';');

    authors.forEach(function (author) {
        formData.append('authors', author);
    });

    categories.forEach(function (category) {
        formData.append('categories', category);
    });
    //formData.append('authors', $('#authorUIDs').val().split(';'));
    //formData.append('categories', $('#categoryIDs').val().split(';'));

    $.ajax({
        url: 'AddBook',
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.status === 'Success') {
                if (bookId == '0')
                    $('#toastSuccesAdd').toast('show');
                else
                    $('#toastSuccesEdit').toast('show');
                setTimeout(function () {
                    window.location.href = window.location.origin + '/Admin/NewOrEditBook?bookId=' + response.data;
                }, 1000);
            } else {
                $('#toastFail').toast('show');
                console.error('Ошибка:', response.details);
                $('#addOrEditBtn').prop('disabled', false);
            }
        },
        error: function (error) {
            console.error('Ошибка:', error);
        }
    });
}
