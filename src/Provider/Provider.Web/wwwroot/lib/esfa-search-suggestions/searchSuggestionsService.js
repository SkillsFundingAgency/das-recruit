'use strict';
var searchSuggestions = function () {
    var endpointUrl = "";

    var init = function (searchInputElementIdentifier, routeUrl)
    {
        endpointUrl = routeUrl;
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
        $("#search-input").val(ui.item.value);
        return false;
    }

    return {
        init : init
    };
}();
