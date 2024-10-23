class AutoCompleteCtrl {

    constructor(ctrlId, listVals, listSelectedVals) {
        if (!ctrlId || typeof (ctrlId) !== 'string')
            throw new Error("Ожидалось значение (string)ctrlId");
        this.ctrlId = ctrlId;
        //
        if (!listVals || typeof (listVals) !== 'object' ||
            listVals.constructor.name !== 'Array')
            throw new Error("Ожидалось значение (Array)listVals");
        if (!listSelectedVals || typeof (listSelectedVals) !== 'object' ||
            listSelectedVals.constructor.name !== 'Array')
            throw new Error("Ожидалось значение (Array)listSelectedVals");
        //
        this.list = listVals;
        this.listSelected = listSelectedVals;
        //
        this.$ctrl;
        this.$ctrlParent;
        this.$ctrlContainer;
        this.$inputArea;
        this.$suggestionsBox;
        this.$tagCont;
        this.$lblText;
        this.$hiddenInput;
        //
        this.itemPropName = "chapter";
        this.sortSuggestions = "asc";
        //this.matchingExp = 2;
        this.matchingExp = 1;
        this.highlight = true;
        this.multipleSelections = true;
        //               
        this.sIndex = -1;
        //
        this.len = this.list.length;
        this.lastsSuggestionIndex;
        this.suggestionElements;
        this.isNavigating = false;
        this.sBoxHeight = 0;
        this.selectedRegistry = [];
        //
        this.suggestionsBoxClicked = false;
        //
        this.nativeList = [];
        this.listForHighlighting = [];
        //
        // 
        this.matchingFuncs = [
            function (q, item) {
                return item.indexOf(q);
            },

            function (q, item) {
                return (item.indexOf(q) === 0) ? 0 : -1;
            },

            function characterSearch(q, item) {
                var index = 0,
                    indexArr = [];
                for (var i = 0, len = q.length; i < len; i++) {
                    index = item.indexOf(q.charAt(i), index);
                    if (index === -1) {
                        return -1;
                    } else {
                        indexArr.push(index);
                        index++;
                    }
                }
                return indexArr;
            }
        ];

        this.uidFieldId = ctrlId === "autoCompleteAuthors" ? "authorUIDs" : "categoryIDs";
    }

    init() {
        var _self = this;
        //
        this.$ctrl = $("#" + this.ctrlId);
        if (this.$ctrl.length === 0)
            throw new Error("Ожидался контрол id='" + this.ctrlId + "'");
        //
        this.$ctrlParent = this.$ctrl.parent();
        this.$hiddenInput = this.$ctrlParent.parent().find('input[type="hidden"]');
        this.$ctrlContainer = this.$ctrlParent.parent();
        //
        this.$inputArea = this.$ctrlParent.find(".autocomplete-input");
        this.$suggestionsBox = this.$ctrlContainer.find(".autocomplete-suggestions");
        this.$nothingDataContainer = this.$ctrlContainer.find(".nothing-data-container");
        _self.$suggestionsBox.css('display', 'none');
        this.$tagCont = this.$ctrlParent.find(".tags");
        this.$lblText = this.$ctrlParent.find(".lbl-text");
        //
        this.matchingFunc = this.matchingFuncs[this.matchingExp];
        //
        this.$inputArea.on("input", function () {
            _self.inputAreaOnInput($(this).val());
            //
            _self.$inputArea.parent().parent().find('.invalid').hide();
        });
        this.$inputArea.on("blur", function () {
            if (!_self.suggestionsBoxClicked) {
                _self.$suggestionsBox.hide();
            }
            _self.$nothingDataContainer.hide();
        });
        this.$inputArea.on("keydown", function (e) {
            _self.inputAreaOnKeydown(e);
        });
        //
        this.$suggestionsBox.on("click", ".suggestion", function () {
            _self.selectSuggestion(this);
        });
        this.$suggestionsBox.on("mousedown", function (e) {
            _self.suggestionsBoxClicked = true;
            e.preventDefault();
            setTimeout(function () {
                _self.suggestionsBoxClicked = false;
            }, 0);
        });
        this.$nothingDataContainer.on("mousedown", function (e) {
            //_self.suggestionsBoxClicked = true;
            e.preventDefault();
            //setTimeout(function () {
            //    _self.suggestionsBoxClicked = false;
            //}, 0);
        });
        //
        this.$tagCont.on("click", ".ac-remove-btn", function () {
            var index = $(this).parent().data("acSuggestionIndex");
            _self.selectedRegistry[index] = false;
            $(this).parent().remove();
            _self.updateHiddenField();
        });
        //this.$tagCont.on('DOMNodeInserted DOMSubtreeModified', function () {
        //    _self.updatePlaceholder();
        //});
        //
        this.$ctrl.on("focus", function () {
            _self.handleFocus();
        });
        this.$ctrl.on("blur", function () {
            _self.handleBlur();
        });
        //
        //if (this.sortSuggestions) {
        //    this.list = this.list.sort(this.sortSuggestions === "asc" ?
        //        function (a, b) {
        //            return _self.sortSuggestionsInAsc(a, b);
        //        } :
        //        function (a, b) {
        //            return _self.sortSuggestionsInDesc(a, b);
        //        });
        //}
        this.sort();
        //
        //var item = undefined, tag = undefined;
        //for (var i = 0; i < this.listSelected.length; i++) {
        //    item = this.list.find(item => item.chapter == this.listSelected[i]);
        //    if (item) {
        //        //this.selectedRegistry[this.list.indexOf(item)] = true;
        //        tag = $("<span class='ac-tag'>" + this.getSuggestionText(item) + '<button type="button" class="ac-remove-btn btn-close" aria-label="Close"></button></span>');
        //        tag.data("acSuggestionIndex", this.list.indexOf(item));
        //        this.selectedRegistry[tag.data("acSuggestionIndex")] = tag;
        //    }
        //}    
        //
        this.normaliseList();
        //this.updatePlaceholder();
        //
        //if (this.listSelected.length > 0) {
        //    var item = undefined, tag = undefined;
        //    this.listSelected.forEach(function (itemVal) {
        //        tag = $("<span class='ac-tag'>" + itemVal.chapter + '<button type="button" class="ac-remove-btn btn-close" aria-label="Close"></button></span>');
        //        if (_self.uidFieldId == 'authorUIDs') {
        //            var value = $("#authorUIDs").val();
        //            if (_self.listSelected.length > 1 && value != '') {
        //                value = value + ';' + itemVal.id;
        //                $("#authorUIDs").val(value);
        //            }
        //            else {
        //                $("#authorUIDs").val(itemVal.id);
        //            }
        //        }
        //        else {
        //            var value = $("#categoryIDs").val();
        //            if (_self.listSelected.length > 1 && value != '') {
        //                value = value + ';' + itemVal.id;
        //                $("#categoryIDs").val(value);
        //            }
        //            else {
        //                $("#categoryIDs").val(itemVal.id);
        //            }
        //        }
        //        _self.$tagCont.append(tag);
        //        //
        //        item = _self.list.find(item => item.chapter == itemVal.chapter);
        //        if (item) {
        //            tag.data("acSuggestionIndex", _self.list.indexOf(item));
        //            _self.selectedRegistry[tag.data("acSuggestionIndex")] = tag;
        //        }
        //    });
        //    //
        //    this.$inputArea.attr('placeholder', '');
        //    this.$ctrlParent.css('border-color', '#CACACA');
        //    this.$lblText.css({ 'display': 'block', 'color': '#CACACA' });
        //}
        this.fillList();
    }

    sort() {
        var _self = this;
        //
        if (this.sortSuggestions) {
            this.list = this.list.sort(this.sortSuggestions === "asc" ?
                function (a, b) {
                    return _self.sortSuggestionsInAsc(a, b);
                } :
                function (a, b) {
                    return _self.sortSuggestionsInDesc(a, b);
                });
        }
    }

    fillList() {
        var _self = this;
        //
        if (this.listSelected.length > 0) {
            var item = undefined, tag = undefined;
            _self.$tagCont.html("");
            _self.$hiddenInput.val('');
            this.listSelected.forEach(function (itemVal) {
                tag = $("<span class='ac-tag'>" + itemVal.chapter + '<button type="button" class="ac-remove-btn btn-close" aria-label="Close"></button></span>');
                var value = _self.$hiddenInput.val();
                if (_self.listSelected.length > 1 && value != '') {
                    value = value + ';' + itemVal.id;
                    _self.$hiddenInput.val(value);
                }
                else {
                    _self.$hiddenInput.val(itemVal.id);
                }
                _self.$tagCont.append(tag);
                //
                item = _self.list.find(item => item.chapter == itemVal.chapter);
                if (item) {
                    tag.data("acSuggestionIndex", _self.list.indexOf(item));
                    _self.selectedRegistry[tag.data("acSuggestionIndex")] = tag;
                }
            });
            //
            this.$inputArea.attr('placeholder', '');
            this.$ctrlParent.css('border-color', '#CACACA');
            this.$lblText.css({ 'display': 'block', 'color': '#CACACA' });
        }
    }

    getHighlightedValue(item, index, q) {
        if (typeof index === "number") {
            return item.slice(0, index) + '<b>' + item.slice(index, index + q.length) + '</b>' + item.slice(index + q.length)
        } else {
            var charArr = item.split("");
            for (var i = 0; i < index.length; i++) {
                charArr[index[i]] = '<b>' + charArr[index[i]] + '</b>';
            }
            return charArr.join("");
        }
    }

    normaliseList() {
        this.nativeList = [];
        this.listForHighlighting = [];
        //
        for (var i = 0; i < this.len; i++) {
            this.nativeList.push(this.itemPropName ? this.list[i][this.itemPropName] : this.list[i]);
            if (this.highlight) {
                this.listForHighlighting.push(this.itemPropName ? this.list[i][this.itemPropName] : this.list[i]);
            }
        }
    }

    processSuggestion(suggestion, highlightedValue) {
        return highlightedValue;
    }

    hideSuggestionsBox() {
        this.$suggestionsBox.html("").hide();
        this.$nothingDataContainer.hide();
    }

    inputAreaOnInput(val) {
        if (this.isNavigating) {
            return;
        }

        //var q = $(this).val(),
        var q = val, suggestions = [];

        if (q.length === 0) {
            this.hideSuggestionsBox();
            return;
        }

        this.$suggestionsBox.html("");

        //var c = '',
        //    index, highlightedValue, item, suggestionStr;
        var index, highlightedValue, item, suggestionStr;
        for (var i = 0; i < this.list.length; i++) {
            index = this.matchingFunc(q.toLowerCase(), this.nativeList[i].toLowerCase(), i);
            if (index !== -1) {
                if (this.highlight) {
                    item = this.itemPropName ? this.list[i][this.itemPropName] : this.list[i];
                    highlightedValue = this.getHighlightedValue(item, index, q);
                }
                suggestionStr = this.processSuggestion(this.list[i], highlightedValue);
                if (this.multipleSelections) {
                    suggestionStr = '<input class="form-check-input" type="checkbox"' +
                        (this.selectedRegistry[i] ? ' checked' : '') + '> ' + suggestionStr;
                }
                suggestions.push('<li class="suggestion" data-index="' + i + '">' + suggestionStr + '</li>');
            }
        }

        if (suggestions.length === 0) {
            //    if (this.ctrlId == "autoCompleteAuthors") {
            //        this.$suggestionsBox.html("<div class='nothing-data'>Ничего не найдено! <button class='btn btn-bia-author-categories' data-bs-toggle='modal' data-bs-target='#continueBook' onclick='" +
            //            this.ctrlId + ".addCustomItem()'>Добавить</button></div>");
            //    }
            //    else {
            //        this.$suggestionsBox.html("<div class='nothing-data'>Ничего не найдено! <button class='btn btn-bia-author-categories' data-bs-toggle='modal' data-bs-target='#addCategory' onclick='" +
            //            this.ctrlId + ".addCustomItem()'>Добавить</button></div>");
            //    }
            this.$nothingDataContainer.show();
        } else {
            this.$nothingDataContainer.hide();
            this.$suggestionsBox.html('<ul>' + suggestions.join('') + '</ul>');
            this.suggestionElements = this.$suggestionsBox.find(".suggestion");
            this.lastSuggestionIndex = this.suggestionElements.length - 1;
            this.sIndex = -1;
        }

        this.suggestionsBoxClicked = false;
        this.$suggestionsBox.show();
        //suggestionHighlighted = -1;
        this.sBoxHeight = this.$suggestionsBox.height();
        this.$suggestionsBox.scrollTop(0);
    }

    getSuggestionText(suggestion) {
        return this.itemPropName ? suggestion[this.itemPropName] : suggestion;
    }

    selectSuggestion(suggestion) {
        var i = Number($(suggestion).attr("data-index"));

        if (this.multipleSelections) {
            if (!this.selectedRegistry[i]) {
                $(suggestion).find("input[type='checkbox']").prop("checked", true);

                var tag = $("<span class='ac-tag'>" + this.getSuggestionText(this.list[i]) + '<button type="button" class="ac-remove-btn btn-close" aria-label="Close"></button></span>');
                tag.data("acSuggestionIndex", i);
                tag.data("id", this.list[i].id);
                this.$tagCont.append(tag);
                this.$ctrl.val('')
                this.listSelected.push(this.list[i]);

                this.selectedRegistry[i] = tag;
            } else {
                $(suggestion).find("input[type='checkbox']").prop("checked", false);
                this.selectedRegistry[i].remove();
                this.selectedRegistry[i] = false;
            }
        } else {
            this.$inputArea.val(this.getSuggestionText(this.list[i]));
            this.hideSuggestionsBox();
        }

        this.updateHiddenField();
    }

    updateHiddenField() {
        let selectedValues = [];
        this.listSelected = [];
        for (let i = 0; i < this.selectedRegistry.length; i++) {
            if (this.selectedRegistry[i]) {
                selectedValues.push(this.list[i].id);
                this.listSelected.push(this.list[i]);
            }
        }
        $("#" + this.uidFieldId).val(selectedValues.join(";"));
    }

    synchroniseSuggestionsBox() {
        var sOffsetTop = this.suggestionElements[this.sIndex].offsetTop,
            sHeight = this.suggestionElements[this.sIndex].clientHeight;

        if ((sOffsetTop + sHeight - this.$suggestionsBox.scrollTop()) > this.sBoxHeight) {
            this.$suggestionsBox.scrollTop(sOffsetTop + sHeight - this.sBoxHeight);
        } else if (this.$suggestionsBox.scrollTop() > sOffsetTop) {
            this.$suggestionsBox.scrollTop(sOffsetTop);
        }
    }

    inputAreaOnKeydown(e) {
        this.isNavigating = false;
        if (e.keyCode === 40) {
            e.preventDefault();
            this.isNavigating = true;
            if (this.sIndex === -1) {
                $(this.suggestionElements[++this.sIndex]).addClass("hl");
                this.$suggestionsBox.scrollTop(0);
            } else if (this.sIndex === this.lastSuggestionIndex) {
                $(this.suggestionElements[this.sIndex]).removeClass("hl");
                this.sIndex = -1;
            } else {
                $(this.suggestionElements[this.sIndex]).removeClass("hl");
                $(this.suggestionElements[++this.sIndex]).addClass("hl");
            }
            if (this.sIndex !== -1) this.synchroniseSuggestionsBox();
        } else if (e.keyCode === 38) {
            e.preventDefault();
            this.isNavigating = true;
            if (this.sIndex === -1) {
                this.sIndex = this.lastSuggestionIndex;
                $(this.suggestionElements[this.sIndex]).addClass("hl");
            } else if (this.sIndex === 0) {
                $(this.suggestionElements[this.sIndex]).removeClass("hl");
                this.sIndex = -1;
                this.$suggestionsBox.scrollTop(0);
            } else {
                $(this.suggestionElements[this.sIndex]).removeClass("hl");
                $(this.suggestionElements[--this.sIndex]).addClass("hl");
            }
            if (this.sIndex !== -1) this.synchroniseSuggestionsBox();
        } else if (e.keyCode === 13) {
            this.isNavigating = true;
            if (this.sIndex !== -1) {
                this.selectSuggestion(this.suggestionElements[this.sIndex]);
            }
        }
    }

    addCustomItem(modalId) {
        //const value = this.$inputArea.val().trim();
        //if (value !== '') {
        //    var tag = $("<span class='ac-tag'>" + value + '<button type="button" class="ac-remove-btn btn-close" aria-label="Close"></button></span>');
        //    this.$tagCont.append(tag);
        //    this.$inputArea.val('');
        //}
        if (this.ctrlId == "autoCompleteAuthors") {
            var lastNameAuth = $('#lastNameAuth');
            var firstNameAuth = $('#firstNameAuth');
            var surnameNameAuth = $('#surnameNameAuth');
            var lastNameAuthLabel = $('#lastNameAuthLabel');
            var firstNameAuthLabel = $('#firstNameAuthLabel');
            var surnameNameAuthLabel = $('#surnameNameAuthLabel');
            lastNameAuth.val(this.$inputArea.val().trim());

            if (lastNameAuth.val()) {
                lastNameAuth.attr('placeholder', '');
                lastNameAuthLabel.css('display', 'block');
            }
            if (firstNameAuth.val()) {
                firstNameAuth.attr('placeholder', '');
                firstNameAuthLabel.css('display', 'block');
            }
            if (surnameNameAuth.val()) {
                surnameNameAuth.attr('placeholder', '');
                surnameNameAuthLabel.css('display', 'block');
            }
        }
        else {
            var txtBoxCategoryName = $('#categoryName');
            txtBoxCategoryName.val(this.$inputArea.val().trim());
            var lblCategoryName = $('#lblCategoryName');

            if (txtBoxCategoryName.val()) {
                txtBoxCategoryName.attr('placeholder', '');
                lblCategoryName.css('display', 'block');
            }
        }
        $(modalId).modal('show');
    }

    addAuthor(modalId) {
        var _self = this;
        //
        var lastNameAuth = $('#lastNameAuth').val().replace(/\s+/g, ' ').trim();
        var firstNameAuth = $('#firstNameAuth').val().replace(/\s+/g, ' ').trim();
        var surnameNameAuth = $('#surnameNameAuth').val().replace(/\s+/g, ' ').trim();
        //
        //
        var isLastNameValid = lastNameAuth.length > 0;
        var isFirstNameValid = firstNameAuth.length > 0;
        var isSurnameNameValid = surnameNameAuth.length > 0;
        //
        if (isLastNameValid && isFirstNameValid) {
            $('#btnAddAuthor').addClass('disabled');
            $.ajax({
                url: 'AddAuthor',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ FirstName: firstNameAuth, LastName: lastNameAuth, SurnameName: surnameNameAuth }),
                success: function (data) {
                    if (data.status === 'Success') {
                        $('#toastSuccessAddAuthor').toast('show');
                        //setTimeout(function () {
                        //    window.location.reload();
                        //}, 2000);
                        $(modalId).modal('hide');
                        //
                        var objAuthor = JSON.parse(data.details);
                        _self.list.push({ chapter: objAuthor.name, id: objAuthor.value });
                        _self.listSelected.push({ chapter: objAuthor.name, id: objAuthor.value });
                        _self.len = _self.list.length;
                        //
                        _self.sort();
                        _self.normaliseList();
                        _self.fillList();
                        setTimeout(function () {
                            $('#lastNameAuth').val('');
                            $('#firstNameAuth').val('');
                            $('#surnameNameAuth').val('');
                            $('#btnAddAuthor').removeClass('disabled');
                        }, 500);

                    } else {
                        if (data.details === 'Уже существует') {
                            $('#toastFailAddAithor').toast('show');
                        }
                        else {
                            $('#toastFail').toast('show');
                        }
                        $('#btnAddAuthor').removeClass('disabled');
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    console.error('Ошибка при выполнении запроса: ' + textStatus);
                }
            });
        }
        else {
            if (!isLastNameValid) {
                $('#lastNameAuth').removeClass('is-valid').addClass('is-invalid');
            } else {
                $('#lastNameAuth').removeClass('is-invalid').addClass('is-valid');
            }

            if (!isFirstNameValid) {
                $('#firstNameAuth').removeClass('is-valid').addClass('is-invalid');
            } else {
                $('#firstNameAuth').removeClass('is-invalid').addClass('is-valid');
            }

            //if (!isSurnameNameValid) {
            //    $('#surnameNameAuth').removeClass('is-valid').addClass('is-invalid');
            //} else {
            //    $('#surnameNameAuth').removeClass('is-invalid').addClass('is-valid');
            //}
            //
            if (!isLastNameValid || !isFirstNameValid) {
                $('#toastFail').toast('show');
            }
        }
    }

    addCategory(modalId) {
        var _self = this;
        var categoryName = $('#categoryName').val().replace(/\s+/g, ' ').trim();
        //
        var isCategoryName = categoryName.length > 0;
        //
        if (isCategoryName) {
            $('#btnAddCategory').addClass('disabled');
            $.ajax({
                url: 'AddCategory',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(categoryName),
                success: function (data) {
                    if (data.status === 'Success') {
                        $('#toastSuccessCategory').toast('show');
                        $('#categoryName').val();
                        $('#btnAddCategory').removeClass('disabled');
                        $(modalId).modal('hide');
                        //
                        var objCategory = JSON.parse(data.details);
                        _self.list.push({ chapter: objCategory.name, id: objCategory.value });
                        _self.listSelected.push({ chapter: objCategory.name, id: objCategory.value });
                        _self.len = _self.list.length;
                        //
                        _self.sort();
                        _self.normaliseList();
                        _self.fillList();
                    }
                    else {
                        if (data.details === 'Уже существует') {
                            $('#toastFailAddCategory').toast('show');
                        }
                        else {
                            $('#toastFail').toast('show');
                        }
                        $('#btnAddCategory').removeClass('disabled');
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    console.error('Ошибка при выполнении запроса: ' + textStatus);
                }
            });
        }
        if (!isCategoryName) {
            $('#categoryName').removeClass('is-valid').addClass('is-invalid');
        } else {
            $('#categoryName').removeClass('is-invalid').addClass('is-valid');
        }
        if (!isCategoryName) {
            $('#toastFail').toast('show');
        }
    }

    handleFocus() {
        //this.$ctrlParent.css("border-color", "#2A74DA");
        //his.$lblTetxt.css("color", "#2A74DA");
        //
        //if ($('.was-validated').length) {
        //    $('.needs-validation-add').removeClass('was-validated')
        //}
        //txtBoxBookName.attr('placeholder', '');
        //lblBookName.css('display', 'block');
        //lblBookName.css('color', '#2A74DA');
        //
        this.$inputArea.attr('placeholder', '');
        this.$ctrlParent.css('border-color', '#2A74DA');
        this.$lblText.css({ 'display': 'block', 'color': '#2A74DA' });
    }

    handleBlur() {
        //this.$ctrlParent.css("border-color", "#BDBDBD");
        //this.$lblText.css("color", "#BDBDBD");
        //
        if (this.$tagCont.children().length > 0) {
            this.$lblText.css('color', '#7D7D7D');
            this.$ctrlParent.css('border-color', '#CACACA')
            this.$inputArea.val('');
        }
        else {
            this.$inputArea.attr('placeholder', this.$lblText.text());
            this.$inputArea.val('');
            this.$lblText.css('display', 'none');
            this.$ctrlParent.css('border-color', '#CACACA')
        }
    }

    //updatePlaceholder() {
    //    if (this.$tagCont.html().trim() !== '') {
    //        this.$ctrl.attr('placeholder', '');
    //    } else {
    //        this.$ctrl.attr('placeholder', '');
    //    }
    //}

    sortSuggestionsInAsc(a, b) {
        if (a[this.itemPropName] < b[this.itemPropName])
            return -1;
        else if (a[this.itemPropName] > b[this.itemPropName])
            return 1;
        else return 0;
    }

    sortSuggestionsInDesc(a, b) {
        var resInAsc = this.sortSuggestionsInAsc(a, b);
        return resInAsc === 0 ? resInAsc : resInAsc * -1;
    }

}


function getParameterByName(name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
}

var bookId = getParameterByName('bookId');
var listAuthors = [];
var listSelectedAuthors = [];
var listCategory = [];
var listSelectedCategory = [];

function GetBookAuthors(bookId) {
    $.ajax({
        url: 'GetBookAuthorsById',
        method: 'POST',
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(bookId),
        success: function (response) {
            if (response.status === 'Success') {
                var authors = response.data;
                var selectAuthors = response.otherDate;
                //console.log(authors);
                authors.forEach(function (author) {
                    listAuthors.push({ chapter: author.fullName, id: author.uid });
                });
                selectAuthors.forEach(function (author) {
                    listSelectedAuthors.push({ chapter: author.fullName, id: author.uid });
                });
                //console.log(listSelectedAuthors);
            } else {
                console.error('Error:', response.Message);
            }
        },
        error: function (error) {
            console.error('Error:', error);
        }
    });
}

GetBookAuthors(bookId);
//
function GetBookCategory(bookId) {
    $.ajax({
        url: 'GetBookCategoryById',
        method: 'POST',
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(bookId),
        success: function (response) {
            if (response.status === 'Success') {
                var categories = response.data;
                var selectedCategory = response.otherDate;
                //console.log(categories);
                categories.forEach(function (category) {
                    listCategory.push({ chapter: category.categoryName, id: category.categoryId });
                });
                selectedCategory.forEach(function (category) {
                    listSelectedCategory.push({ chapter: category.categoryName, id: category.categoryId });
                });
                //console.log(listSelectedCategory);
            } else {
                console.error('Error:', response.Message);
            }
        },
        error: function (error) {
            console.error('Error:', error);
        }
    });
}

GetBookCategory(bookId);

// имя переменной должно совпадать с id на разметке
var autoCompleteAuthors = new AutoCompleteCtrl("autoCompleteAuthors", listAuthors, listSelectedAuthors);
var autoCompleteCategory = new AutoCompleteCtrl("autoCompleteCategory", listCategory, listSelectedCategory);

$(document).ready(function () {
    autoCompleteAuthors.init();
    autoCompleteCategory.init();
});