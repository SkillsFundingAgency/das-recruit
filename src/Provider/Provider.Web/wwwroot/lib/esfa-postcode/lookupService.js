(function ($) {
    $searchField = $('.postcode-lookup'),
        findAddressVal = $searchField.val();
    $searchField.keyup(function (e) {
        findAddressVal = $(e.target).val();
    });
    $searchField
        .autocomplete({
            search: function () {
                $('#addressLoading').show();
                $('#enterAddressManually').hide();
            },
            source: function (request, response) {
                $.ajax({
                    url: 'getAddresses',
                    dataType: 'json',
                    data: {
                        searchTerm: request.term,
                    },
                    timeout: 5000,
                    success: function (data) {
                        response(
                            $.map(data.addresses, function (suggestion) {
                                return {
                                    label: suggestion.text,
                                    value: '',
                                    data: suggestion
                                };
                            })
                        );
                    }
                });
            },
            messages: {
                noResults: function () {
                    return "We can't find an address matching " + findAddressVal;
                },
                results: function (amount) {
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
            select: function (event, ui) {
                var item = ui.item.data;
                populateAddress(item);
            },
            focus: function (event, ui) {
                $('#addressInputWrapper')
                    .find('.ui-helper-hidden-accessible')
                    .text('To select ' + ui.item.label + ', press enter');
            },
            autoFocus: true,
            minLength: 1,
            delay: 100
        })
        .focus(function () {
            searchContext = '';
        });

    function populateAddress(address) {
        $('#AddressLine1').val(address.addressLine1);
        $('#AddressLine2').val(address.addressLine2);
        $('#AddressLine3').val(address.postTown);
        $('#AddressLine4').val(address.county);
        $('#Postcode').val(address.postcode);

        $('#ariaAddressEntered').text('Your address has been entered into the fields below.');
    }
})(jQuery);