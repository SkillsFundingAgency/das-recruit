// Training Provider Autocomplete

var providerSearchInputs = document.querySelectorAll(".app-provider-autocomplete");

if (providerSearchInputs.length > 0) {
  for (var i = 0; i < providerSearchInputs.length; i++) {
    var input = providerSearchInputs[i]
    var container = document.createElement('div');

    container.className = "das-autocomplete-wrap"
    input.parentNode.replaceChild(container, input);

    accessibleAutocomplete({
      element: container,
      id: input.id,
      name: input.name,
      defaultValue: input.value,
      displayMenu: 'overlay',
      showNoOptionsFound: false,
      minLength: 2,
      source: providerArray,
      placeholder: input.placeholder,
      confirmOnBlur: false,
      autoselect: true
    });
  }
}


// Select Field Autocomplete 

var selectFields = document.querySelectorAll(".app-autocomplete");
if (selectFields.length > 0) {
    for (var s = 0; s < selectFields.length; s++) {
        accessibleAutocomplete.enhanceSelectElement({
            selectElement: selectFields[s],
            minLength: 2,
            autoselect: true,
            defaultValue: '',
            displayMenu: 'overlay',
            placeholder: '',
            onConfirm: function (opt) {
                var txtInput = document.querySelector('#' + this.id);
                var searchString = opt || txtInput.value;
                var requestedOption = [].filter.call(this.selectElement.options,
                function (option) {
                    return (option.textContent || option.innerText) === searchString
                }
                )[0];
                if (requestedOption) {
                    requestedOption.selected = true;
                } else {
                    this.selectElement.selectedIndex = 0;
                }
            }
        });
    }
}

// Vacancy Autocomplete

var vacancySearchInputs = document.querySelectorAll(".app-vacancy-autocomplete");

if (vacancySearchInputs.length > 0) {

  for (var i = 0; i < vacancySearchInputs.length; i++) {

    var input = vacancySearchInputs[i]
    var apiUrl = input.dataset.apiurl
    var container = document.createElement('div');

    container.className = "das-autocomplete-wrap"
    input.parentNode.replaceChild(container, input);

    var getSuggestions = function (query, updateResults) {
      var results = [];
      var xhr = new XMLHttpRequest();
      xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
          var jsonResponse = JSON.parse(xhr.responseText);
          updateResults(jsonResponse);
        }
      }
      xhr.open("GET", apiUrl + '?term=' + query, true);
      xhr.send();
    };

    accessibleAutocomplete({
      element: container,
      id: input.id,
      name: input.name,
      defaultValue: input.value,
      displayMenu: 'overlay',
      showNoOptionsFound: false,
      minLength: 2,
      source: getSuggestions,
      placeholder: input.placeholder,
      confirmOnBlur: false,
      autoselect: true
    });
  }

  var autocompleteInputs = document.querySelectorAll(".autocomplete__input");
  if (autocompleteInputs.length > 0) {
    for (i = 0; i < autocompleteInputs.length; i++) {
      var autocompleteInput = autocompleteInputs[i];
      autocompleteInput.setAttribute("autocomplete", "new-password");
    }
  }
}



/* -----------------------
Character count behaviour
-------------------------- */
characterCount = function (element, count) {
    var $element = $(element);

    if (typeof count === "undefined") {
        var text = $element.val();
        count = text.length;
        count += (text.match(/\n/g) || []).length;
    }
    
    var maxLength = $element.attr("data-val-length-max"),
    absRemainder = Math.abs(maxLength - count),
    $maxLengthCountElement = $element.closest(".govuk-form-group").find(".maxchar-count"),
    $maxLengthTextElement = $element.closest(".govuk-form-group").find(".maxchar-text");

    if (maxLength) {
        $maxLengthCountElement.text(absRemainder);
    }
    else {
        $maxLengthCountElement.hide();
        return;
    }
        
    if (count > maxLength) {
        $maxLengthCountElement.parent().addClass("has-error");
        $maxLengthTextElement.text(absRemainder === 1 ? " character over the limit" : " characters over the limit");
    }
    else
    {
        $maxLengthCountElement.parent().removeClass("has-error");
        $maxLengthTextElement.text(absRemainder === 1 ? " character remaining" : " characters remaining");
    }
};

$(".character-count").on("keyup", function() {
  characterCount(this);
});

$(".character-count").each(function() {
  characterCount(this);
});


/* Disable Are you sure for links */
$('a').on("click", function() {
    $('form').areYouSure( {'silent':true} );
});

/* Validation accessibility fix */
$(window).on("load", function(e) {
    // If there is an error summary, set focus to the summary
    if ($('.error-summary').length) {
      $('.error-summary').focus();
    } 
});

function handleAnchorClicks() {
    if (document.documentElement.scrollIntoView) {

        var $menu = $("#floating-menu-holder .account-information");

        $(".summary-link").on("click", function (e) {
            
            var hash = $(this)[0].hash;
            var $element = $(hash);
            $element.focus();

            //if the element has a label scroll to that instead
            var $label = $("label[for='" + $element.attr("id") + "']");
            if ($label.length === 1) {
                $element = $label;
            }

            $element[0].scrollIntoView(true);

            if(history.pushState) {
                history.pushState(null, null, hash);
            }
            else {
                location.hash = hash;
            }

            //handle floating menu height (may or may not be completely visible in Viewport after initial scroll)
            setTimeout(function () {
                var visibleMenuHeight = inViewport($menu);
                var scrollY = $element.offset().top - visibleMenuHeight;
                    window.scrollTo(0, scrollY);
                }, 100);

            e.preventDefault();
            return false;
        });
    }
}

function inViewport($el) {
    var elH = $el.outerHeight(),
        wH = $(window).height(),
        r = $el[0].getBoundingClientRect(), t = r.top, b = r.bottom;
    return Math.max(0, t > 0 ? Math.min(elH, wH - t) : Math.min(b, wH));
}

function initializeHtmlEditors() {
    tinymce.init({
        element_format: 'html',
        apply_source_formatting: true,
        menubar: false,
        plugins: 'lists paste',
        selector: ".html-editor",
        statusbar: false,
        toolbar: 'bullist',
        paste_as_text: true,
        content_style: ".mce-content-body {font-size:19px;font-family:nta,Arial,sans-serif;}",
        setup: function (tinyMceEditor) {
            var element = tinyMceEditor.getElement();
            
            tinyMceEditor.on('keyup',
                function (e) {
                    setEditorMaxLength(element, tinyMceEditor);
                });
            tinyMceEditor.on('focus',
                function (e) {
                    tinyMceEditor.editorContainer.classList.add("editor-focus");
                });
            tinyMceEditor.on('blur',
                function (e) {
                    tinyMceEditor.editorContainer.classList.remove("editor-focus");
                });
        },
        init_instance_callback: function (tinyMceEditor) {
            var element = tinyMceEditor.getElement();
            setEditorMaxLength(element, tinyMceEditor);
        }
    });
}

function setEditorMaxLength(element, tinyMceEditor) {
    var innerText = tinyMceEditor.contentDocument.body.innerText;
    innerText = innerText.replace(/\n\n/g, "|");
    var innerTextLength = innerText.charAt(innerText.length - 1) === String.fromCharCode(10) ? innerText.length - 1 : innerText.length;
    characterCount(element, innerTextLength);
}

$(function () {

    // Dirty forms handling
    $('form').areYouSure();
    //handle anchor clicks to account for floating menu
    handleAnchorClicks();

    // Data Layer Pushes

    var pageHeading = document.querySelector('h1.govuk-heading-xl') || document.querySelector('h1.govuk-heading-l') || document.querySelector('h1.govuk-fieldset__heading') || document.querySelector('label.govuk-label--xl');
    var pageTitle = pageHeading.innerText

    // Form validation - dataLayer pushes
    var errorSummary = document.querySelector('.govuk-error-summary');
    if (errorSummary !== null) {
      var validationErrors = errorSummary.querySelectorAll('ul.govuk-error-summary__list li a');
      var validationErrorsArr = [];
      nodeListForEach(validationErrors, function(validationError) {
        validationErrorsArr.push(validationError.innerText)
      });
      var validationMessage = validationErrorsArr.join();
      var dataLayerObj = {
        event: 'form submission error',
        page: pageTitle,
        message: validationMessage
      }
      window.dataLayer.push(dataLayerObj)
    }
    // Radio button selection - dataLayer pushes
    var radioWrapper = document.querySelector('.govuk-radios');
    if (radioWrapper !== null) {
        var radios = radioWrapper.querySelectorAll('input[type=radio]');
        var labelText;
        var dataLayerObj;
        nodeListForEach(radios, function(radio) {
            radio.addEventListener('change', function() {
                labelText = this.nextElementSibling.innerText;
                dataLayerObj = {
                    event: 'radio button selected',
                    page: pageTitle,
                    radio: labelText
                }
                window.dataLayer.push(dataLayerObj)
            })
        })
    }

});

function nodeListForEach(nodes, callback) {
  if (window.NodeList.prototype.forEach) {
      return nodes.forEach(callback)
  }
  for (var i = 0; i < nodes.length; i++) {
      callback.call(window, nodes[i], i, nodes);
  }
}