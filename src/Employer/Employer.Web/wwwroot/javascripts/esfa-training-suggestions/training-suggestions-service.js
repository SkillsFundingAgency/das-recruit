var trainingSuggestionsService = function () {

    var init = function(selectElement) {
        
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