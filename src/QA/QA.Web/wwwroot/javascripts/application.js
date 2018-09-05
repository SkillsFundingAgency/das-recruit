/* -------------------------------------------------------------
  Character count behaviour for textareas
------------------------------------------------------------- */
characterCount = function (n) {
    var i = $(n)
        , r = i.attr("data-val-length-max")
        , e = i.val()
        , h = (e.match(/\n/g) || []).length
        , o = e.length + h
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

$("textarea").on("keyup", function () {
    characterCount(this);
});


$("textarea").each(function () {
    characterCount(this);
});