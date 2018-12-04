'use strict';
(function ($, pcaConfig) {
	var	pcaConfig = pcaConfig,
		searchContext = '',
		$searchField = $('.postcode-lookup'),
		findAddressVal = $searchField.val();

	$searchField.keyup(function(e) {
		findAddressVal = $(e.target).val();
	});

    $searchField
        .autocomplete({
            search: function() {
                $('#addressLoading').show();
                $('#enterAddressManually').hide();
            },
			source: function (request, response) {
				$.ajax({
					url: pcaConfig.findEndpoint,
                    dataType: 'jsonp',
					data: {
						key: pcaConfig.key,
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

                if (item.Next === 'Retrieve') {
                    retrieveAddress(item.Id);
                    searchContext = '';
                } else {
                    var field = $(event.target);
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

    function retrieveAddress(id) {
		$.ajax({
			url: pcaConfig.retrieveEndpoint,
            dataType: 'jsonp',
			data: {
				key: pcaConfig.key,
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
})(jQuery, window.EsfaRecruit.PcaConfig);