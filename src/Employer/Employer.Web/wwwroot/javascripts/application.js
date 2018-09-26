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

sfa.hookupExampleVacancyToggle = function (showExampleText, hideExampleText, showExampleVacancyCookieName) {
    var $exampleLink = $("#example_link"),
        $exampleVacancy = $("#example_vacancy"),
        $exampleVacancyContainer = $("#example-vacancy-js-container");

    $exampleVacancyContainer.show();

    var showExampleVacancyCookie = Cookies.get(showExampleVacancyCookieName);

    if (typeof (showExampleVacancyCookie) === "undefined") {
        $exampleVacancy.hide();
    }
    else {
        $exampleLink[0].innerText = hideExampleText;
    }

    $exampleLink.click(function () {
        if ($exampleVacancy.is(":visible")) {
            Cookies.remove(showExampleVacancyCookieName);
            $exampleLink[0].innerText = showExampleText;
        }
        else {
            Cookies.set(showExampleVacancyCookieName, "1");
            $exampleLink[0].innerText = hideExampleText;
        }
        $exampleVacancy.slideToggle(0);
        return false;
    });
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
characterCount = function(n) {
  var i = $(n)
    , r = i.attr("data-val-length-max")
    , e = i.val()
    , h = (e.match(/\n/g) || []).length
    , o = e.length
    , u = Math.abs(r - o)
    , f = i.closest(".form-group").find(".maxchar-count")
    , t = i.closest(".form-group").find(".maxchar-text")
    , s = i.closest(".form-group").find(".aria-limit");
  if (r)
      f.text(u);
  else {
      t.hide();
      return
  }
  o > r ? (f.parent().addClass("has-error"),
  t.text(" characters over the limit"),
  s.text("Character limit has been reached, you must type fewer than " + r + " characters"),
  u == 1 ? t.text(" character over the limit") : t.text(" characters over the limit")) : (f.parent().removeClass("has-error"),
  t.text(" characters remaining"),
  s.text(""),
  u == 1 ? t.text(" character remaining") : t.text(" characters remaining"))
}

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
        $(".form-group.error").length > 0 ? button.text(label).removeClass("disabled") : $(".block-label.error").length > 0 && button.text(label).removeClass("disabled");
        button.attr("disabled")
    }, 50)
});

/* Disable Are you sure for links */
$('a').on("click", function() {
    $('form').areYouSure( {'silent':true} );
});

/* Validation accessibility fix */
$(window).load(function() {
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

$(function () {
    //Legacy menu script
    sfa.navigation.init();
    // Dirty forms handling
    $('form').areYouSure();
    //handle anchor clicks to account for floating menu
    handleAnchorClicks();
});