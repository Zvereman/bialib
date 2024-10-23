$(document).ready(function () {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
    //
    window.addEventListener('popstate', function (event) {
        $('#btnSearch').val('');
        $('.search-control').removeClass('active');
        window.location.reload()
    });
    //
    if ($('#btnSearch').val()) {
        $('.search-control').addClass('white active');
    }

    $('#btnSearch').focus(function () {
        $('.search-control').addClass('active');
    });

    $('#btnSearch').blur(function () {
        if (!$('#btnSearch').val())
            $('.search-control').removeClass('active');
    });

    $('.close-icon').click(function () {
        $('#btnSearch').val('');
        $('.search-control').removeClass('active');
    });

    $('#btnSearch').on('keypress', function (event) {
        if (event.key === 'Enter') {
            var query = $(this).val().trim();
            var encodedCategory = '%D0%92%D1%81%D0%B5';
            var url = '/Visitor/Books?sortBy=title&sortOrder=asc&pageNumber=1&pageSize=100';
            url += '&selectedCategories=' + encodedCategory + '&searchQuery=' + encodeURIComponent(query);
            //
            history.pushState({ searchQuery: query }, 'Search Results', url);
            window.location.href = url;
            //window.location.href = url;
        }
    });

    function restoreSearchQuery() {
        var queryParam = getQueryParam('searchQuery');
        if (queryParam) {
            $('#btnSearch').val(decodeURIComponent(queryParam));
            $('.search-control').addClass('white active');
        } else {
            $('#btnSearch').val('');
            $('.search-control').removeClass('active');
        }
    }

    function getQueryParam(name) {
        var urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    }

    restoreSearchQuery();

});

function redirectToBook(bookId) {
    var url = '/Visitor/Book/' + bookId;
    $(location).attr('href', url);
}