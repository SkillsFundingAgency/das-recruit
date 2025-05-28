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

/* -------------------------------------------------------------
  Character count behaviour for textareas
------------------------------------------------------------- */
const characterCount = function (element, count) {
    var $element = $(element);

    if (typeof count === "undefined") {
        var text = $element.val();
        count = text.length;
        count += (text.match(/\n/g) || []).length;
    }

    var maxLength = $element.attr("data-val-length-max"),
        absRemainder = Math.abs(maxLength - count),
        $maxLengthCountElement = $element.closest(".form-group").find(".maxchar-count"),
        $maxLengthTextElement = $element.closest(".form-group").find(".maxchar-text");

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
    else {
        $maxLengthCountElement.parent().removeClass("has-error");
        $maxLengthTextElement.text(absRemainder === 1 ? " character remaining" : " characters remaining");
    }
};

$("textarea").on("keyup", function () {
    characterCount(this);
});


$("textarea").each(function () {
    characterCount(this);
});

const hookupHistory = function() {
    $("#history_link").click(function() {
        if ($("#history").is(":visible")) {
            $("#history_link")[0].innerText = "Show reviewers history";
        } else {
            $("#history_link")[0].innerText = "Hide reviewers history";
        }
        $("#history").slideToggle();
    });

    $("#history").hide();
};

const initializeHtmlEditors = function() {
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
        setup: function(tinyMceEditor) {
            var element = tinyMceEditor.getElement();

            tinyMceEditor.on('keyup',
                function(e) {
                    setEditorMaxLength(element, tinyMceEditor);
                });
            tinyMceEditor.on('focus',
                function(e) {
                    tinyMceEditor.editorContainer.classList.add("editor-focus");
                });
            tinyMceEditor.on('blur',
                function(e) {
                    tinyMceEditor.editorContainer.classList.remove("editor-focus");
                });
        },
        init_instance_callback: function(tinyMceEditor) {
            var element = tinyMceEditor.getElement();
            setEditorMaxLength(element, tinyMceEditor);
        }
    });
};

const setEditorMaxLength = function(element, tinyMceEditor) {
    var innerText = tinyMceEditor.contentDocument.body.innerText;
    innerText = innerText.replace(/\n\n/g, "|");
    var innerTextLength = innerText.charAt(innerText.length - 1) === String.fromCharCode(10) ? innerText.length - 1 : innerText.length;
    characterCount(element, innerTextLength);
};

$(function () {
    // Add cookie message
    CookieBanner.addCookieMessage();
});