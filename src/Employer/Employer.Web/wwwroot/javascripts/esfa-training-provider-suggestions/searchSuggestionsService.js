var searchSuggestions = function () {
    var inputId;
    
    var init = function ($element, providers) {
        inputId = $element.attr("id");

        var $container = $("<div></div>").attr("id", inputId + "-container");
        $container.insertAfter($element);
        $element.remove();

        accessibleAutocomplete({
            element: $container[0],
            id: inputId,
            name: $element.attr("name"),
            source: suggest,
            defaultValue: $element.val(),
            confirmOnBlur: false,
            autoselect: true,
            displayMenu: "inline",
            minLength: 3
        });

        function suggest(query, populateResults) {
            var filteredResults = providers.filter(
                result => result.toUpperCase().indexOf(query.trim().toUpperCase()) !== -1
            );
            populateResults(filteredResults);
        }
    }

    return {
        init: init
    };
}();