/* -------------------------------------------------------------
  Character count behaviour for textareas
------------------------------------------------------------- */
characterCount = function (element, count) {
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

hookupHistory = function() {
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

initializeHtmlEditors = function() {
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

setEditorMaxLength = function(element, tinyMceEditor) {
    var innerText = tinyMceEditor.contentDocument.body.innerText;
    innerText = innerText.replace(/\n\n/g, "|");
    var innerTextLength = innerText.charAt(innerText.length - 1) === String.fromCharCode(10) ? innerText.length - 1 : innerText.length;
    characterCount(element, innerTextLength);
};
