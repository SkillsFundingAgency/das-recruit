// provides the matching addresses from postcode
(function($) {
    var searchContext = '',
        $searchField = $('#AddressLine1'),
        findAddressVal = $searchField.val(),
        key = ''
        
    $searchField.keyup(function() {
        findAddressVal = $(this).val();
    });

    $searchField
        .autocomplete({
            search: function() {
                $('#addressLoading').show();
                $('#enterAddressManually').hide();
            },
            source: function(request, response) {
                $.ajax({
                    url:
                        '//services.postcodeanywhere.co.uk/CapturePlus/Interactive/Find/v2.10/json3.ws',
                    dataType: 'jsonp',
                    data: {
                        key: key,
                        country: 'GB',
                        searchTerm: request.term,
                        lastId: searchContext
                    },
                    timeout: 5000,
                    success: function(data) {
                        response(
                            $.map(data.Items, function(suggestion) {
                                return {
                                    label: suggestion.Text,
                                    value: '',
                                    data: suggestion
                                };
                            })
                        );
                    }
                });
            },
            messages: {
                noResults: function() {
                    return "We can't find an address matching " + findAddressVal;
                },
                results: function(amount) {
                    return (
                        "We've found " +
                        amount +
                        (amount > 1 ? ' addresses' : ' address') +
                        ' that match ' +
                        findAddressVal +
                        '. Use up and down arrow keys to navigate'
                    );
                }
            },
            select: function(event, ui) {
                var item = ui.item.data;

                if (item.Next == 'Retrieve') {
                    //retrieve the address
                    retrieveAddress(item.Id);
                    searchContext = '';
                } else {
                    var field = $(this);
                    searchContext = item.Id;

                    if (searchContext === 'GBR|') {
                        window.setTimeout(function() {
                            field.autocomplete('search', item.Text);
                        });
                    } else {
                        window.setTimeout(function() {
                            field.autocomplete('search', item.Id);
                        });
                    }
                }
            },
            focus: function(event, ui) {
                $('#addressInputWrapper')
                    .find('.ui-helper-hidden-accessible')
                    .text('To select ' + ui.item.label + ', press enter');
            },
            autoFocus: true,
            minLength: 1,
            delay: 100
        })
        .focus(function() {
            searchContext = '';
        });

    function hasValue(elem) {
        return (
            $(elem).filter(function() {
                return $(this).val();
            }).length > 0
        );
    }

    function retrieveAddress(id) {
        $.ajax({
            url:
                '//services.postcodeanywhere.co.uk/CapturePlus/Interactive/Retrieve/v2.10/json3.ws',
            dataType: 'jsonp',
            data: {
                key: key,
                id: id
            },
            timeout: 5000,
            success: function(data) {
                if (data.Items.length) {

                    populateAddress(data.Items[0]);
                }
            },
            error: function() {

            }
        });
    }

    function populateAddress(address) {

        $('#AddressLine1').val(address.Line1);
        $('#AddressLine2').val(address.Line2);
        $('#AddressLine3').val(address.Line3);
        $('#AddressLine4').val(address.City);
        $('#Postcode').val(address.PostalCode);

        $('#ariaAddressEntered').text('Your address has been entered into the fields below.');
    }

})(jQuery);