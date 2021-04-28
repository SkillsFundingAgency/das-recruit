var navLinksContainer = document.getElementsByClassName('das-navigation__list');
var navLinksListItems = document.getElementsByClassName('das-navigation__list-item');
var availableSpace, currentVisibleLinks, numOfVisibleItems, requiredSpace, currentHiddenLinks;
var totalSpace = 0;
var breakWidths = [];

var addMenuButton = function () {
  var priorityLi = $('<li />').addClass('das-navigation__priority-list-item govuk-visually-hidden').attr('id', 'priority-list-menu');
  var priorityUl = $('<ul />').addClass('das-navigation__priority-list govuk-visually-hidden');
  var priorityBut = $('<a />')
    .addClass('das-navigation__priority-button')
    .attr('href', '#')
    .text('More')
    .on('click', function(e) {
      $(menuLinksContainer).toggleClass('govuk-visually-hidden');
      $(this).toggleClass('open');
      e.preventDefault();
    });
  priorityLi.append(priorityBut, priorityUl).appendTo($(navLinksContainer).eq(0));
  return priorityUl;
};

var checkSpaceForPriorityLinks = function () {
  availableSpace = navLinksContainer[0].offsetWidth - 80;
  currentVisibleLinks = document.querySelectorAll('.das-navigation__list > .das-navigation__list-item');
  currentHiddenLinks = document.querySelectorAll('.das-navigation__priority-list > .das-navigation__list-item');
  numOfVisibleItems = currentVisibleLinks.length;
  requiredSpace = breakWidths[numOfVisibleItems - 1];

  if (requiredSpace > availableSpace) {
    numOfVisibleItems -= 1;
    var lastVisibleLink = currentVisibleLinks[numOfVisibleItems];
    menuLinksContainer[0].insertBefore(lastVisibleLink, menuLinksContainer[0].childNodes[0]);
    $('#priority-list-menu').removeClass('govuk-visually-hidden');
    checkSpaceForPriorityLinks();
  } else if (availableSpace > breakWidths[numOfVisibleItems]) {
    if (currentHiddenLinks.length > 0) {
      var firstLink = currentHiddenLinks[0];
      var priorityListItem = document.getElementsByClassName('das-navigation__priority-list-item');
      navLinksContainer[0].insertBefore(firstLink, priorityListItem[0])
      if (currentHiddenLinks.length === 1) {
        $('#priority-list-menu').addClass('govuk-visually-hidden');
      }
    }
    numOfVisibleItems += 1;
  }
};

if (navLinksContainer.length > 0) {
  var menuLinksContainer  = addMenuButton();
  for (var i = 0; i < navLinksListItems.length; i++) {
    var width = navLinksListItems[i].offsetWidth;
    totalSpace += width;
    breakWidths.push(totalSpace);
  }
  checkSpaceForPriorityLinks();
}

$(window).resize(function() {
  if (navLinksContainer.length > 0)
    checkSpaceForPriorityLinks();
});

var dasJs = dasJs || {};

dasJs.userNavigation = {
  elems: {
    settingsMenu: $('#das-user-navigation > ul')
  },
  init: function () {
    this.setupMenus(this.elems.settingsMenu);
    this.setupEvents(this.elems.settingsMenu);
  },
  setupMenus: function (menu) {
    menu.find('ul').attr("aria-expanded", "false");
  },
  setupEvents: function (menu) {
    var that = this;
    var subMenuLi = menu.find('li.das-user-navigation__list-item--has-sub-menu');
    subMenuLi.find('> a').on('click', function (e) {
      var $that = $(this);
      that.toggleMenu($that, $that.next('ul'));
      e.stopPropagation();
      e.preventDefault();
    });
  },
  toggleMenu: function (link, subMenu) {
    var $li = link.parent();
    if ($li.hasClass("das-user-navigation__sub-menu--open")) {
      $li.removeClass("das-user-navigation__sub-menu--open");
      subMenu.addClass("js-hidden").attr("aria-expanded", "false");
    } else {
      this.closeAllOpenMenus();
      $li.addClass("das-user-navigation__sub-menu--open");
      subMenu.removeClass("js-hidden").attr("aria-expanded", "true");
    }
  },
  closeAllOpenMenus: function () {
    $('li.das-user-navigation__list-item--has-sub-menu').each(function () {
      var listItem = $(this);
      var subMenu = $(this).children('ul');
      var openClass = 'das-user-navigation__sub-menu--open';
      if (listItem.hasClass(openClass)) {
        listItem.removeClass(openClass);
        subMenu.addClass("js-hidden").attr("aria-expanded", "false");
      }
    });
  }
}

$(document).click(function() {
  dasJs.userNavigation.closeAllOpenMenus();
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
    if ($('#das-user-navigation')) {
        dasJs.userNavigation.init();
    }
    // Dirty forms handling
    $('form').areYouSure();
    //handle anchor clicks to account for floating menu
    handleAnchorClicks();
    window.GOVUKFrontend.initAll()


    // Data Layer Pushes

    var pageTitle = document.querySelector('h1.govuk-heading-xl').innerText;
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