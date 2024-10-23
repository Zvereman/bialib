//var selectedUserUid = null;
//$(document).ready(function () {
//    const input = $('#customSelectInput');
//    const dropdown = $('#customSelectDropdown');
//    const options = dropdown.find('.custom-select-option');
//    const noResults = $('#customSelectNoResults');
//    const txtOnBorder = $('#textOnBorder');

//    if (input.val()) {
//        txtOnBorder.css('display', 'block');
//        input.attr('placeholder', '');
//    }

//    input.on('input', function () {
//        const filter = input.val().toLowerCase();
//        let visibleCount = 0;
//        dropdown.css('display', 'block');

//        options.each(function () {
//            const option = $(this);
//            if (option.text().toLowerCase().includes(filter)) {
//                option.css('display', '');
//                visibleCount++;
//            } else {
//                option.css('display', 'none');
//            }
//        });

//        noResults.css('display', visibleCount ? 'none' : 'block');
//    });

//    options.on('click', function () {
//        input.val($(this).text());
//        selectedUserUid = $(this).data('value');
//        dropdown.css('display', 'none');
//        txtOnBorder.css('color', '#7D7D7D')
//    });

//    $(document).on('click', function (event) {
//        if (!input.is(event.target) && !dropdown.is(event.target) && dropdown.has(event.target).length === 0) {
//            dropdown.css('display', 'none');
//            if (input.val().length === 0) {
//                input.attr('placeholder', txtOnBorder.text());
//                txtOnBorder.css('display', 'none');
//                txtOnBorder.css('color', '#7D7D7D');
//            }
//            else {
//                txtOnBorder.css('color', '#7D7D7D');
//            }
//        }
//    });

//    input.on('focus', function () {
//        dropdown.css('display', 'block');
//        input.attr('placeholder', '');
//        txtOnBorder.css('display', 'block');
//        txtOnBorder.css('color', '#2A74DA');
//        input.css('border-color', '#2A74DA');
//    });

//    input.on('blur', function () {
//        input.attr('placeholder', txtOnBorder.text());
//        if (input.val()) {
//            //txtOnBorder.css('color', '#7D7D7D');
//            input.css('border-color', '#CACACA')
//        } else {
//            //txtOnBorder.css('display', 'none');
//            input.css('border-color', '#CACACA')
//        }
//    });
//});

var selectedUserUid = null;

$(document).ready(function () {

    $('.custom-select-container').each(function () {
        const container = $(this);
        const input = container.find('.custom-select-input');
        const dropdown = container.find('.custom-select-dropdown');
        const options = dropdown.find('.custom-select-option');
        const noResults = container.find('.custom-select-no-results');
        const txtOnBorder = container.find('.text-on-border');

        if (input.val()) {
            txtOnBorder.css('display', 'block');
            input.attr('placeholder', '');
        }

        input.on('input', function () {
            const filter = input.val().toLowerCase();
            let visibleCount = 0;
            dropdown.css('display', 'block');

            options.each(function () {
                const option = $(this);
                if (option.text().toLowerCase().includes(filter)) {
                    option.css('display', '');
                    visibleCount++;
                } else {
                    option.css('display', 'none');
                }
            });

            noResults.css('display', visibleCount ? 'none' : 'block');
        });

        options.on('click', function () {
            input.val($(this).text());
            selectedUserUid = $(this).data('value');
            dropdown.css('display', 'none');
            txtOnBorder.css('color', '#7D7D7D');
        });

        $(document).on('click', function (event) {
            if (!input.is(event.target) && !dropdown.is(event.target) && dropdown.has(event.target).length === 0) {
                dropdown.css('display', 'none');
                if (input.val().length === 0) {
                    input.attr('placeholder', txtOnBorder.text());
                    txtOnBorder.css('display', 'none');
                    txtOnBorder.css('color', '#7D7D7D');
                }
                else {
                    txtOnBorder.css('color', '#7D7D7D');
                }
            }
        });

        input.on('focus', function () {
            dropdown.css('display', 'block');
            input.attr('placeholder', '');
            txtOnBorder.css('display', 'block');
            txtOnBorder.css('color', '#2A74DA');
            input.css('border-color', '#2A74DA');
        });

        input.on('blur', function () {
            input.attr('placeholder', txtOnBorder.text());
            if (input.val()) {
                input.css('border-color', '#CACACA');
            } else {
                input.css('border-color', '#CACACA');
            }
        });
    });
});
