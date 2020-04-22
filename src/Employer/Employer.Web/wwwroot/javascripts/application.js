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

/* Prevent multiple submissions */
$('button, input[type="submit"], a.button').on("click", function() {
    var button = $(this)
      , label = button.text();
      button.is(".save-button") ? button.text("Saving").addClass("disabled") : button.text("Loading").addClass("disabled");
    setTimeout(function() {
        $(".govuk-form-group.error").length > 0 ? button.text(label).removeClass("disabled") : $(".block-label.error").length > 0 && button.text(label).removeClass("disabled");
        button.attr("disabled")
    }, 50)
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
    //Legacy menu script
    sfa.navigation.init();
    $('ul#global-nav-links').collapsableNav();
    // Dirty forms handling
    $('form').areYouSure();
    //handle anchor clicks to account for floating menu
    handleAnchorClicks();
    window.GOVUKFrontend.initAll()
});

//back link
function historyBack() {
    history.back();
}
var el = document.getElementById('goBack');
if (el) {
    el.addEventListener('click', historyBack, false);
}