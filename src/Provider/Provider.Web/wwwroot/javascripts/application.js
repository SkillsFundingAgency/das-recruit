// Training Provider Autocomplete

var providerSearchInputs = document.querySelectorAll(".app-provider-autocomplete");

if (providerSearchInputs.length > 0) {
  for (var a = 0; a < providerSearchInputs.length; a++) {
    var input = providerSearchInputs[a]
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

var vacancyApiUrl, 
    getVacancySuggestions = function (query, updateResults) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
        var jsonResponse = JSON.parse(xhr.responseText);
        updateResults(jsonResponse);
        }
    }
    xhr.open("GET", vacancyApiUrl + '?term=' + query, true);
    xhr.send();
};

var vacancySearchInputs = document.querySelectorAll(".app-vacancy-autocomplete");

if (vacancySearchInputs.length > 0) {

  for (var v = 0; v < vacancySearchInputs.length; v++) {

    var searchInput = vacancySearchInputs[v]
    vacancyApiUrl = searchInput.dataset.apiurl
    var searchInputContainer = document.createElement('div');

    searchInputContainer.className = "das-autocomplete-wrap"
    searchInput.parentNode.replaceChild(searchInputContainer, searchInput);

    accessibleAutocomplete({
      element: searchInputContainer,
      id: searchInput.id,
      name: searchInput.name,
      defaultValue: searchInput.value,
      displayMenu: 'overlay',
      showNoOptionsFound: false,
      minLength: 2,
      source: getVacancySuggestions,
      placeholder: searchInput.placeholder,
      confirmOnBlur: false,
      autoselect: true
    });
  }

  var autocompleteInputs = document.querySelectorAll(".autocomplete__input");
  if (autocompleteInputs.length > 0) {
    for (var i = 0; i < autocompleteInputs.length; i++) {
      var autocompleteInput = autocompleteInputs[i];
      autocompleteInput.setAttribute("autocomplete", "new-password");
    }
  }
}




// Legacy JavaScript from DAS
var sfa;
sfa = sfa || {};
//Floating menus
sfa.navigation = {
    elems: {
        userNav: $("nav#user-nav > ul"),
        levyNav: $("ul#global-nav-links")
    },
    init: function() {
        this.setupMenus(this.elems.userNav);
        this.setupEvents(this.elems.userNav)
    },
    setupMenus: function(n) {
        n.find("ul").addClass("js-hidden").attr("aria-hidden", "true")
    },
    setupEvents: function(n) {
        var t = this;
        n.find("li.has-sub-menu > a").on("click", function(n) {
            var i = $(this);
            t.toggleMenu(i, i.next("ul"));
            n.stopPropagation();
            n.preventDefault()
        });
        n.find("li.has-sub-menu > ul > li > a").on("focusout", function() {
            var n = $(this);
            $(this).parent().is(":last-child") && t.toggleMenu(n, n.next("ul"))
        })
    },
    toggleMenu: function(n, t) {
        var i = n.parent();
        i.hasClass("open") ? (i.removeClass("open"),
        t.addClass("js-hidden").attr("aria-hidden", "true")) : (this.closeAllOpenMenus(),
        i.addClass("open"),
        t.removeClass("js-hidden").attr("aria-hidden", "false"))
    },
    closeAllOpenMenus: function() {
        this.elems.userNav.find("li.has-sub-menu.open").removeClass("open").find("ul").addClass("js-hidden").attr("aria-hidden", "true");
        this.elems.levyNav.find("li.open").removeClass("open").addClass("js-hidden").attr("aria-hidden", "true")
    },
    linkSettings: function() {
        var n = $("a#link-settings")
          , t = this;
        this.toggleUserMenu();
        n.attr("aria-hidden", "false");
        n.on("click touchstart", function(n) {
            var i = $(this).attr("href");
            $(this).toggleClass("open");
            t.toggleUserMenu();
            n.preventDefault()
        })
    },
    toggleUserMenu: function() {
        var n = this.elems.userNav.parent();
        n.hasClass("close") ? n.removeClass("close").attr("aria-hidden", "false") : n.addClass("close").attr("aria-hidden", "true")
    }
};

//Legacy floating header script
$(window).scroll(function () {
    if ($(window).scrollTop() >= 110) {
        $('.floating-menu').addClass('fixed-header');
        $('.js-float').addClass('width-adjust');
    }
    else {
        $('.floating-menu').removeClass('fixed-header');
        $('.js-float').removeClass('width-adjust');
    }
});

/* -----------------------
    Cookie methods
    --------------
    Usage:
    Setting a cookie:
    CookieBanner.init('hobnob', 'tasty', { days: 30 });
    Reading a cookie:
    CookieBanner.init('hobnob');
    Deleting a cookie:
    CookieBanner.init('hobnob', null);
-------------------------- */
var CookieBanner = {
    init: function (name, value, options) {
        if (typeof value !== 'undefined') {
        if (value === false || value === null) {
            return CookieBanner.setCookie(name, '', { days: -1 })
        } else {
            return CookieBanner.setCookie(name, value, options)
        }
        } else {
        return CookieBanner.getCookie(name)
        }
    },
    setCookie: function (name, value, options) {
        if (typeof options === 'undefined') {
        options = {};
        }
        var cookieString = name + '=' + value + '; path=/';
        if (options.days) {
        var date = new Date();
        date.setTime(date.getTime() + (options.days * 24 * 60 * 60 * 1000));
        cookieString = cookieString + '; expires=' + date.toGMTString();
        }
        if (document.location.protocol === 'https:') {
        cookieString = cookieString + '; Secure';
        }
        document.cookie = cookieString;
    },
    getCookie: function (name) {
        var nameEQ = name + '=';
        var cookies = document.cookie.split(';');
        for (var i = 0, len = cookies.length; i < len; i++) {
        var cookie = cookies[i];
        while (cookie.charAt(0) === ' ') {
            cookie = cookie.substring(1, cookie.length);
        }
        if (cookie.indexOf(nameEQ) === 0) {
            return decodeURIComponent(cookie.substring(nameEQ.length))
        }
        }
        return null
    },
    addCookieMessage: function () {
        var message = document.querySelector('.js-cookie-banner');
        var hasCookieMessage = (message && CookieBanner.init('seen_cookie_message') === null);

        if (hasCookieMessage) {
        message.style.display = 'block';
        CookieBanner.init('seen_cookie_message', 'yes', { days: 28 });
        }
    }
};

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
        content_style: ".mce-content-body {font-size:19px;font-family:\"GDS Transport\",arial,sans-serif}",
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
});