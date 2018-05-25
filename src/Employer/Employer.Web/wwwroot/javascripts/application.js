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

sfa.hookupExampleVacancyToggle = function (showExampleText, hideExampleText) {
    var $exampleLink = $("#example_link"),
        $exampleVacancy = $("#example_vacancy"),
        showExampleVacancyCookieName = "showExampleVacancy",
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
        } else {
            Cookies.set(showExampleVacancyCookieName, "1");
            $exampleLink[0].innerText = hideExampleText;
        }
        $exampleVacancy.slideToggle(0);
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

$(function() {
    //Legacy menu script
    sfa.navigation.init();
});

   