var trainingSuggestionsService = function () {

    var init = function(selectElementSelector) {
        var selectElement = document.querySelector(selectElementSelector);

        accessibleAutocomplete.enhanceSelectElement({
            selectElement: selectElement,
            displayMenu: "inline",
            defaultValue: "",
            confirmOnBlur: true
        });
    };

    return {
        init: init
    };
}();