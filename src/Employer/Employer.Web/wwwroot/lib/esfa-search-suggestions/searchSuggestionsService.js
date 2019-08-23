'use strict';
var searchSuggestions = function () {
    var endpointUrl,
        searchInputElementIdentifier;

    var init = function (inputElementIdentifier, routeUrl) {
        endpointUrl = routeUrl;
        searchInputElementIdentifier = inputElementIdentifier;
        $(searchInputElementIdentifier).autocomplete({
            source: getSearchAutocompleteData,
            select: selectItem,
            minLength: 3
        });
    }

    var getSearchAutocompleteData = function(request, response) {
        var params = {term: request.term};
        $.getJSON(endpointUrl, params, function(data) { 
            response(data);
        });
    };

    var selectItem = function (event, ui) {
        $(searchInputElementIdentifier).val(ui.item.value);
        return false;
    };

    return {
        init : init
    };
}();
