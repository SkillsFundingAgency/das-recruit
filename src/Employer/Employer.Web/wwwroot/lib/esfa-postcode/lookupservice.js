(function(n, t) {
    function i(t, i) {
        var u, f, e, o = t.nodeName.toLowerCase();
        return "area" === o ? (u = t.parentNode,
        f = u.name,
        t.href && f && "map" === u.nodeName.toLowerCase() ? (e = n("img[usemap=#" + f + "]")[0],
        !!e && r(e)) : !1) : (/input|select|textarea|button|object/.test(o) ? !t.disabled : "a" === o ? t.href || i : i) && r(t)
    }
    function r(t) {
        return n.expr.filters.visible(t) && !n(t).parents().addBack().filter(function() {
            return "hidden" === n.css(this, "visibility")
        }).length
    }
    var u = 0
      , f = /^ui-id-\d+$/;
    n.ui = n.ui || {};
    n.extend(n.ui, {
        version: "1.10.4",
        keyCode: {
            BACKSPACE: 8,
            COMMA: 188,
            DELETE: 46,
            DOWN: 40,
            END: 35,
            ENTER: 13,
            ESCAPE: 27,
            HOME: 36,
            LEFT: 37,
            NUMPAD_ADD: 107,
            NUMPAD_DECIMAL: 110,
            NUMPAD_DIVIDE: 111,
            NUMPAD_ENTER: 108,
            NUMPAD_MULTIPLY: 106,
            NUMPAD_SUBTRACT: 109,
            PAGE_DOWN: 34,
            PAGE_UP: 33,
            PERIOD: 190,
            RIGHT: 39,
            SPACE: 32,
            TAB: 9,
            UP: 38
        }
    });
    n.fn.extend({
        focus: function(t) {
            return function(i, r) {
                return "number" == typeof i ? this.each(function() {
                    var t = this;
                    setTimeout(function() {
                        n(t).focus();
                        r && r.call(t)
                    }, i)
                }) : t.apply(this, arguments)
            }
        }(n.fn.focus),
        scrollParent: function() {
            var t;
            return t = n.ui.ie && /(static|relative)/.test(this.css("position")) || /absolute/.test(this.css("position")) ? this.parents().filter(function() {
                return /(relative|absolute|fixed)/.test(n.css(this, "position")) && /(auto|scroll)/.test(n.css(this, "overflow") + n.css(this, "overflow-y") + n.css(this, "overflow-x"))
            }).eq(0) : this.parents().filter(function() {
                return /(auto|scroll)/.test(n.css(this, "overflow") + n.css(this, "overflow-y") + n.css(this, "overflow-x"))
            }).eq(0),
            /fixed/.test(this.css("position")) || !t.length ? n(document) : t
        },
        zIndex: function(i) {
            if (i !== t)
                return this.css("zIndex", i);
            if (this.length)
                for (var u, f, r = n(this[0]); r.length && r[0] !== document; ) {
                    if (u = r.css("position"),
                    ("absolute" === u || "relative" === u || "fixed" === u) && (f = parseInt(r.css("zIndex"), 10),
                    !isNaN(f) && 0 !== f))
                        return f;
                    r = r.parent()
                }
            return 0
        },
        uniqueId: function() {
            return this.each(function() {
                this.id || (this.id = "ui-id-" + ++u)
            })
        },
        removeUniqueId: function() {
            return this.each(function() {
                f.test(this.id) && n(this).removeAttr("id")
            })
        }
    });
    n.extend(n.expr[":"], {
        data: n.expr.createPseudo ? n.expr.createPseudo(function(t) {
            return function(i) {
                return !!n.data(i, t)
            }
        }) : function(t, i, r) {
            return !!n.data(t, r[3])
        }
        ,
        focusable: function(t) {
            return i(t, !isNaN(n.attr(t, "tabindex")))
        },
        tabbable: function(t) {
            var r = n.attr(t, "tabindex")
              , u = isNaN(r);
            return (u || r >= 0) && i(t, !u)
        }
    });
    n("<a>").outerWidth(1).jquery || n.each(["Width", "Height"], function(i, r) {
        function u(t, i, r, u) {
            return n.each(o, function() {
                i -= parseFloat(n.css(t, "padding" + this)) || 0;
                r && (i -= parseFloat(n.css(t, "border" + this + "Width")) || 0);
                u && (i -= parseFloat(n.css(t, "margin" + this)) || 0)
            }),
            i
        }
        var o = "Width" === r ? ["Left", "Right"] : ["Top", "Bottom"]
          , f = r.toLowerCase()
          , e = {
            innerWidth: n.fn.innerWidth,
            innerHeight: n.fn.innerHeight,
            outerWidth: n.fn.outerWidth,
            outerHeight: n.fn.outerHeight
        };
        n.fn["inner" + r] = function(i) {
            return i === t ? e["inner" + r].call(this) : this.each(function() {
                n(this).css(f, u(this, i) + "px")
            })
        }
        ;
        n.fn["outer" + r] = function(t, i) {
            return "number" != typeof t ? e["outer" + r].call(this, t) : this.each(function() {
                n(this).css(f, u(this, t, !0, i) + "px")
            })
        }
    });
    n.fn.addBack || (n.fn.addBack = function(n) {
        return this.add(null == n ? this.prevObject : this.prevObject.filter(n))
    }
    );
    n("<a>").data("a-b", "a").removeData("a-b").data("a-b") && (n.fn.removeData = function(t) {
        return function(i) {
            return arguments.length ? t.call(this, n.camelCase(i)) : t.call(this)
        }
    }(n.fn.removeData));
    n.ui.ie = !!/msie [\w.]+/.exec(navigator.userAgent.toLowerCase());
    n.support.selectstart = "onselectstart"in document.createElement("div");
    n.fn.extend({
        disableSelection: function() {
            return this.bind((n.support.selectstart ? "selectstart" : "mousedown") + ".ui-disableSelection", function(n) {
                n.preventDefault()
            })
        },
        enableSelection: function() {
            return this.unbind(".ui-disableSelection")
        }
    });
    n.extend(n.ui, {
        plugin: {
            add: function(t, i, r) {
                var u, f = n.ui[t].prototype;
                for (u in r)
                    f.plugins[u] = f.plugins[u] || [],
                    f.plugins[u].push([i, r[u]])
            },
            call: function(n, t, i) {
                var r, u = n.plugins[t];
                if (u && n.element[0].parentNode && 11 !== n.element[0].parentNode.nodeType)
                    for (r = 0; u.length > r; r++)
                        n.options[u[r][0]] && u[r][1].apply(n.element, i)
            }
        },
        hasScroll: function(t, i) {
            if ("hidden" === n(t).css("overflow"))
                return !1;
            var r = i && "left" === i ? "scrollLeft" : "scrollTop"
              , u = !1;
            return t[r] > 0 ? !0 : (t[r] = 1,
            u = t[r] > 0,
            t[r] = 0,
            u)
        }
    })
}
)(jQuery),
function(n, t) {
    var r = 0
      , i = Array.prototype.slice
      , u = n.cleanData;
    n.cleanData = function(t) {
        for (var i, r = 0; null != (i = t[r]); r++)
            try {
                n(i).triggerHandler("remove")
            } catch (f) {}
        u(t)
    }
    ;
    n.widget = function(i, r, u) {
        var h, e, f, s, c = {}, o = i.split(".")[0];
        i = i.split(".")[1];
        h = o + "-" + i;
        u || (u = r,
        r = n.Widget);
        n.expr[":"][h.toLowerCase()] = function(t) {
            return !!n.data(t, h)
        }
        ;
        n[o] = n[o] || {};
        e = n[o][i];
        f = n[o][i] = function(n, i) {
            return this._createWidget ? (arguments.length && this._createWidget(n, i),
            t) : new f(n,i)
        }
        ;
        n.extend(f, e, {
            version: u.version,
            _proto: n.extend({}, u),
            _childConstructors: []
        });
        s = new r;
        s.options = n.widget.extend({}, s.options);
        n.each(u, function(i, u) {
            return n.isFunction(u) ? (c[i] = function() {
                var n = function() {
                    return r.prototype[i].apply(this, arguments)
                }
                  , t = function(n) {
                    return r.prototype[i].apply(this, n)
                };
                return function() {
                    var i, r = this._super, f = this._superApply;
                    return this._super = n,
                    this._superApply = t,
                    i = u.apply(this, arguments),
                    this._super = r,
                    this._superApply = f,
                    i
                }
            }(),
            t) : (c[i] = u,
            t)
        });
        f.prototype = n.widget.extend(s, {
            widgetEventPrefix: e ? s.widgetEventPrefix || i : i
        }, c, {
            constructor: f,
            namespace: o,
            widgetName: i,
            widgetFullName: h
        });
        e ? (n.each(e._childConstructors, function(t, i) {
            var r = i.prototype;
            n.widget(r.namespace + "." + r.widgetName, f, i._proto)
        }),
        delete e._childConstructors) : r._childConstructors.push(f);
        n.widget.bridge(i, f)
    }
    ;
    n.widget.extend = function(r) {
        for (var u, f, o = i.call(arguments, 1), e = 0, s = o.length; s > e; e++)
            for (u in o[e])
                f = o[e][u],
                o[e].hasOwnProperty(u) && f !== t && (r[u] = n.isPlainObject(f) ? n.isPlainObject(r[u]) ? n.widget.extend({}, r[u], f) : n.widget.extend({}, f) : f);
        return r
    }
    ;
    n.widget.bridge = function(r, u) {
        var f = u.prototype.widgetFullName || r;
        n.fn[r] = function(e) {
            var h = "string" == typeof e
              , o = i.call(arguments, 1)
              , s = this;
            return e = !h && o.length ? n.widget.extend.apply(null, [e].concat(o)) : e,
            h ? this.each(function() {
                var i, u = n.data(this, f);
                return u ? n.isFunction(u[e]) && "_" !== e.charAt(0) ? (i = u[e].apply(u, o),
                i !== u && i !== t ? (s = i && i.jquery ? s.pushStack(i.get()) : i,
                !1) : t) : n.error("no such method '" + e + "' for " + r + " widget instance") : n.error("cannot call methods on " + r + " prior to initialization; attempted to call method '" + e + "'")
            }) : this.each(function() {
                var t = n.data(this, f);
                t ? t.option(e || {})._init() : n.data(this, f, new u(e,this))
            }),
            s
        }
    }
    ;
    n.Widget = function() {}
    ;
    n.Widget._childConstructors = [];
    n.Widget.prototype = {
        widgetName: "widget",
        widgetEventPrefix: "",
        defaultElement: "<div>",
        options: {
            disabled: !1,
            create: null
        },
        _createWidget: function(t, i) {
            i = n(i || this.defaultElement || this)[0];
            this.element = n(i);
            this.uuid = r++;
            this.eventNamespace = "." + this.widgetName + this.uuid;
            this.options = n.widget.extend({}, this.options, this._getCreateOptions(), t);
            this.bindings = n();
            this.hoverable = n();
            this.focusable = n();
            i !== this && (n.data(i, this.widgetFullName, this),
            this._on(!0, this.element, {
                remove: function(n) {
                    n.target === i && this.destroy()
                }
            }),
            this.document = n(i.style ? i.ownerDocument : i.document || i),
            this.window = n(this.document[0].defaultView || this.document[0].parentWindow));
            this._create();
            this._trigger("create", null, this._getCreateEventData());
            this._init()
        },
        _getCreateOptions: n.noop,
        _getCreateEventData: n.noop,
        _create: n.noop,
        _init: n.noop,
        destroy: function() {
            this._destroy();
            this.element.unbind(this.eventNamespace).removeData(this.widgetName).removeData(this.widgetFullName).removeData(n.camelCase(this.widgetFullName));
            this.widget().unbind(this.eventNamespace).removeAttr("aria-disabled").removeClass(this.widgetFullName + "-disabled ui-state-disabled");
            this.bindings.unbind(this.eventNamespace);
            this.hoverable.removeClass("ui-state-hover");
            this.focusable.removeClass("ui-state-focus")
        },
        _destroy: n.noop,
        widget: function() {
            return this.element
        },
        option: function(i, r) {
            var u, f, e, o = i;
            if (0 === arguments.length)
                return n.widget.extend({}, this.options);
            if ("string" == typeof i)
                if (o = {},
                u = i.split("."),
                i = u.shift(),
                u.length) {
                    for (f = o[i] = n.widget.extend({}, this.options[i]),
                    e = 0; u.length - 1 > e; e++)
                        f[u[e]] = f[u[e]] || {},
                        f = f[u[e]];
                    if (i = u.pop(),
                    1 === arguments.length)
                        return f[i] === t ? null : f[i];
                    f[i] = r
                } else {
                    if (1 === arguments.length)
                        return this.options[i] === t ? null : this.options[i];
                    o[i] = r
                }
            return this._setOptions(o),
            this
        },
        _setOptions: function(n) {
            var t;
            for (t in n)
                this._setOption(t, n[t]);
            return this
        },
        _setOption: function(n, t) {
            return this.options[n] = t,
            "disabled" === n && (this.widget().toggleClass(this.widgetFullName + "-disabled ui-state-disabled", !!t).attr("aria-disabled", t),
            this.hoverable.removeClass("ui-state-hover"),
            this.focusable.removeClass("ui-state-focus")),
            this
        },
        enable: function() {
            return this._setOption("disabled", !1)
        },
        disable: function() {
            return this._setOption("disabled", !0)
        },
        _on: function(i, r, u) {
            var e, f = this;
            "boolean" != typeof i && (u = r,
            r = i,
            i = !1);
            u ? (r = e = n(r),
            this.bindings = this.bindings.add(r)) : (u = r,
            r = this.element,
            e = this.widget());
            n.each(u, function(u, o) {
                function s() {
                    return i || f.options.disabled !== !0 && !n(this).hasClass("ui-state-disabled") ? ("string" == typeof o ? f[o] : o).apply(f, arguments) : t
                }
                "string" != typeof o && (s.guid = o.guid = o.guid || s.guid || n.guid++);
                var h = u.match(/^(\w+)\s*(.*)$/)
                  , c = h[1] + f.eventNamespace
                  , l = h[2];
                l ? e.delegate(l, c, s) : r.bind(c, s)
            })
        },
        _off: function(n, t) {
            t = (t || "").split(" ").join(this.eventNamespace + " ") + this.eventNamespace;
            n.unbind(t).undelegate(t)
        },
        _delay: function(n, t) {
            function r() {
                return ("string" == typeof n ? i[n] : n).apply(i, arguments)
            }
            var i = this;
            return setTimeout(r, t || 0)
        },
        _hoverable: function(t) {
            this.hoverable = this.hoverable.add(t);
            this._on(t, {
                mouseenter: function(t) {
                    n(t.currentTarget).addClass("ui-state-hover")
                },
                mouseleave: function(t) {
                    n(t.currentTarget).removeClass("ui-state-hover")
                }
            })
        },
        _focusable: function(t) {
            this.focusable = this.focusable.add(t);
            this._on(t, {
                focusin: function(t) {
                    n(t.currentTarget).addClass("ui-state-focus")
                },
                focusout: function(t) {
                    n(t.currentTarget).removeClass("ui-state-focus")
                }
            })
        },
        _trigger: function(t, i, r) {
            var u, f, e = this.options[t];
            if (r = r || {},
            i = n.Event(i),
            i.type = (t === this.widgetEventPrefix ? t : this.widgetEventPrefix + t).toLowerCase(),
            i.target = this.element[0],
            f = i.originalEvent)
                for (u in f)
                    u in i || (i[u] = f[u]);
            return this.element.trigger(i, r),
            !(n.isFunction(e) && e.apply(this.element[0], [i].concat(r)) === !1 || i.isDefaultPrevented())
        }
    };
    n.each({
        show: "fadeIn",
        hide: "fadeOut"
    }, function(t, i) {
        n.Widget.prototype["_" + t] = function(r, u, f) {
            "string" == typeof u && (u = {
                effect: u
            });
            var o, e = u ? u === !0 || "number" == typeof u ? i : u.effect || i : t;
            u = u || {};
            "number" == typeof u && (u = {
                duration: u
            });
            o = !n.isEmptyObject(u);
            u.complete = f;
            u.delay && r.delay(u.delay);
            o && n.effects && n.effects.effect[e] ? r[t](u) : e !== t && r[e] ? r[e](u.duration, u.easing, f) : r.queue(function(i) {
                n(this)[t]();
                f && f.call(r[0]);
                i()
            })
        }
    })
}(jQuery),
function(n, t) {
    function e(n, t, i) {
        return [parseFloat(n[0]) * (a.test(n[0]) ? t / 100 : 1), parseFloat(n[1]) * (a.test(n[1]) ? i / 100 : 1)]
    }
    function r(t, i) {
        return parseInt(n.css(t, i), 10) || 0
    }
    function v(t) {
        var i = t[0];
        return 9 === i.nodeType ? {
            width: t.width(),
            height: t.height(),
            offset: {
                top: 0,
                left: 0
            }
        } : n.isWindow(i) ? {
            width: t.width(),
            height: t.height(),
            offset: {
                top: t.scrollTop(),
                left: t.scrollLeft()
            }
        } : i.preventDefault ? {
            width: 0,
            height: 0,
            offset: {
                top: i.pageY,
                left: i.pageX
            }
        } : {
            width: t.outerWidth(),
            height: t.outerHeight(),
            offset: t.offset()
        }
    }
    n.ui = n.ui || {};
    var f, u = Math.max, i = Math.abs, o = Math.round, s = /left|center|right/, h = /top|center|bottom/, c = /[\+\-]\d+(\.[\d]+)?%?/, l = /^\w+/, a = /%$/, y = n.fn.position;
    n.position = {
        scrollbarWidth: function() {
            if (f !== t)
                return f;
            var u, r, i = n("<div style='display:block;position:absolute;width:50px;height:50px;overflow:hidden;'><div style='height:100px;width:auto;'><\/div><\/div>"), e = i.children()[0];
            return n("body").append(i),
            u = e.offsetWidth,
            i.css("overflow", "scroll"),
            r = e.offsetWidth,
            u === r && (r = i[0].clientWidth),
            i.remove(),
            f = u - r
        },
        getScrollInfo: function(t) {
            var i = t.isWindow || t.isDocument ? "" : t.element.css("overflow-x")
              , r = t.isWindow || t.isDocument ? "" : t.element.css("overflow-y")
              , u = "scroll" === i || "auto" === i && t.width < t.element[0].scrollWidth
              , f = "scroll" === r || "auto" === r && t.height < t.element[0].scrollHeight;
            return {
                width: f ? n.position.scrollbarWidth() : 0,
                height: u ? n.position.scrollbarWidth() : 0
            }
        },
        getWithinInfo: function(t) {
            var i = n(t || window)
              , r = n.isWindow(i[0])
              , u = !!i[0] && 9 === i[0].nodeType;
            return {
                element: i,
                isWindow: r,
                isDocument: u,
                offset: i.offset() || {
                    left: 0,
                    top: 0
                },
                scrollLeft: i.scrollLeft(),
                scrollTop: i.scrollTop(),
                width: r ? i.width() : i.outerWidth(),
                height: r ? i.height() : i.outerHeight()
            }
        }
    };
    n.fn.position = function(t) {
        if (!t || !t.of)
            return y.apply(this, arguments);
        t = n.extend({}, t);
        var b, f, a, w, p, d, g = n(t.of), tt = n.position.getWithinInfo(t.within), it = n.position.getScrollInfo(tt), k = (t.collision || "flip").split(" "), nt = {};
        return d = v(g),
        g[0].preventDefault && (t.at = "left top"),
        f = d.width,
        a = d.height,
        w = d.offset,
        p = n.extend({}, w),
        n.each(["my", "at"], function() {
            var i, r, n = (t[this] || "").split(" ");
            1 === n.length && (n = s.test(n[0]) ? n.concat(["center"]) : h.test(n[0]) ? ["center"].concat(n) : ["center", "center"]);
            n[0] = s.test(n[0]) ? n[0] : "center";
            n[1] = h.test(n[1]) ? n[1] : "center";
            i = c.exec(n[0]);
            r = c.exec(n[1]);
            nt[this] = [i ? i[0] : 0, r ? r[0] : 0];
            t[this] = [l.exec(n[0])[0], l.exec(n[1])[0]]
        }),
        1 === k.length && (k[1] = k[0]),
        "right" === t.at[0] ? p.left += f : "center" === t.at[0] && (p.left += f / 2),
        "bottom" === t.at[1] ? p.top += a : "center" === t.at[1] && (p.top += a / 2),
        b = e(nt.at, f, a),
        p.left += b[0],
        p.top += b[1],
        this.each(function() {
            var y, d, h = n(this), c = h.outerWidth(), l = h.outerHeight(), rt = r(this, "marginLeft"), ut = r(this, "marginTop"), ft = c + rt + r(this, "marginRight") + it.width, et = l + ut + r(this, "marginBottom") + it.height, s = n.extend({}, p), v = e(nt.my, h.outerWidth(), h.outerHeight());
            "right" === t.my[0] ? s.left -= c : "center" === t.my[0] && (s.left -= c / 2);
            "bottom" === t.my[1] ? s.top -= l : "center" === t.my[1] && (s.top -= l / 2);
            s.left += v[0];
            s.top += v[1];
            n.support.offsetFractions || (s.left = o(s.left),
            s.top = o(s.top));
            y = {
                marginLeft: rt,
                marginTop: ut
            };
            n.each(["left", "top"], function(i, r) {
                n.ui.position[k[i]] && n.ui.position[k[i]][r](s, {
                    targetWidth: f,
                    targetHeight: a,
                    elemWidth: c,
                    elemHeight: l,
                    collisionPosition: y,
                    collisionWidth: ft,
                    collisionHeight: et,
                    offset: [b[0] + v[0], b[1] + v[1]],
                    my: t.my,
                    at: t.at,
                    within: tt,
                    elem: h
                })
            });
            t.using && (d = function(n) {
                var r = w.left - s.left
                  , v = r + f - c
                  , e = w.top - s.top
                  , y = e + a - l
                  , o = {
                    target: {
                        element: g,
                        left: w.left,
                        top: w.top,
                        width: f,
                        height: a
                    },
                    element: {
                        element: h,
                        left: s.left,
                        top: s.top,
                        width: c,
                        height: l
                    },
                    horizontal: 0 > v ? "left" : r > 0 ? "right" : "center",
                    vertical: 0 > y ? "top" : e > 0 ? "bottom" : "middle"
                };
                c > f && f > i(r + v) && (o.horizontal = "center");
                l > a && a > i(e + y) && (o.vertical = "middle");
                o.important = u(i(r), i(v)) > u(i(e), i(y)) ? "horizontal" : "vertical";
                t.using.call(this, n, o)
            }
            );
            h.offset(n.extend(s, {
                using: d
            }))
        })
    }
    ;
    n.ui.position = {
        fit: {
            left: function(n, t) {
                var h, e = t.within, r = e.isWindow ? e.scrollLeft : e.offset.left, o = e.width, s = n.left - t.collisionPosition.marginLeft, i = r - s, f = s + t.collisionWidth - o - r;
                t.collisionWidth > o ? i > 0 && 0 >= f ? (h = n.left + i + t.collisionWidth - o - r,
                n.left += i - h) : n.left = f > 0 && 0 >= i ? r : i > f ? r + o - t.collisionWidth : r : i > 0 ? n.left += i : f > 0 ? n.left -= f : n.left = u(n.left - s, n.left)
            },
            top: function(n, t) {
                var h, o = t.within, r = o.isWindow ? o.scrollTop : o.offset.top, e = t.within.height, s = n.top - t.collisionPosition.marginTop, i = r - s, f = s + t.collisionHeight - e - r;
                t.collisionHeight > e ? i > 0 && 0 >= f ? (h = n.top + i + t.collisionHeight - e - r,
                n.top += i - h) : n.top = f > 0 && 0 >= i ? r : i > f ? r + e - t.collisionHeight : r : i > 0 ? n.top += i : f > 0 ? n.top -= f : n.top = u(n.top - s, n.top)
            }
        },
        flip: {
            left: function(n, t) {
                var o, s, r = t.within, y = r.offset.left + r.scrollLeft, c = r.width, h = r.isWindow ? r.scrollLeft : r.offset.left, l = n.left - t.collisionPosition.marginLeft, a = l - h, v = l + t.collisionWidth - c - h, u = "left" === t.my[0] ? -t.elemWidth : "right" === t.my[0] ? t.elemWidth : 0, f = "left" === t.at[0] ? t.targetWidth : "right" === t.at[0] ? -t.targetWidth : 0, e = -2 * t.offset[0];
                0 > a ? (o = n.left + u + f + e + t.collisionWidth - c - y,
                (0 > o || i(a) > o) && (n.left += u + f + e)) : v > 0 && (s = n.left - t.collisionPosition.marginLeft + u + f + e - h,
                (s > 0 || v > i(s)) && (n.left += u + f + e))
            },
            top: function(n, t) {
                var o, s, r = t.within, y = r.offset.top + r.scrollTop, a = r.height, h = r.isWindow ? r.scrollTop : r.offset.top, v = n.top - t.collisionPosition.marginTop, c = v - h, l = v + t.collisionHeight - a - h, p = "top" === t.my[1], u = p ? -t.elemHeight : "bottom" === t.my[1] ? t.elemHeight : 0, f = "top" === t.at[1] ? t.targetHeight : "bottom" === t.at[1] ? -t.targetHeight : 0, e = -2 * t.offset[1];
                0 > c ? (s = n.top + u + f + e + t.collisionHeight - a - y,
                n.top + u + f + e > c && (0 > s || i(c) > s) && (n.top += u + f + e)) : l > 0 && (o = n.top - t.collisionPosition.marginTop + u + f + e - h,
                n.top + u + f + e > l && (o > 0 || l > i(o)) && (n.top += u + f + e))
            }
        },
        flipfit: {
            left: function() {
                n.ui.position.flip.left.apply(this, arguments);
                n.ui.position.fit.left.apply(this, arguments)
            },
            top: function() {
                n.ui.position.flip.top.apply(this, arguments);
                n.ui.position.fit.top.apply(this, arguments)
            }
        }
    },
    function() {
        var t, i, r, u, f, e = document.getElementsByTagName("body")[0], o = document.createElement("div");
        t = document.createElement(e ? "div" : "body");
        r = {
            visibility: "hidden",
            width: 0,
            height: 0,
            border: 0,
            margin: 0,
            background: "none"
        };
        e && n.extend(r, {
            position: "absolute",
            left: "-1000px",
            top: "-1000px"
        });
        for (f in r)
            t.style[f] = r[f];
        t.appendChild(o);
        i = e || document.documentElement;
        i.insertBefore(t, i.firstChild);
        o.style.cssText = "position: absolute; left: 10.7432222px;";
        u = n(o).offset().left;
        n.support.offsetFractions = u > 10 && 11 > u;
        t.innerHTML = "";
        i.removeChild(t)
    }()
}(jQuery),
function(n) {
    n.widget("ui.autocomplete", {
        version: "1.10.4",
        defaultElement: "<input>",
        options: {
            appendTo: null,
            autoFocus: !1,
            delay: 300,
            minLength: 1,
            position: {
                my: "left top",
                at: "left bottom",
                collision: "none"
            },
            source: null,
            change: null,
            close: null,
            focus: null,
            open: null,
            response: null,
            search: null,
            select: null
        },
        requestIndex: 0,
        pending: 0,
        _create: function() {
            var t, i, r, u = this.element[0].nodeName.toLowerCase(), f = "textarea" === u, e = "input" === u;
            this.isMultiLine = f ? !0 : e ? !1 : this.element.prop("isContentEditable");
            this.valueMethod = this.element[f || e ? "val" : "text"];
            this.isNewMenu = !0;
            this.element.addClass("ui-autocomplete-input").attr("autocomplete", "off");
            this._on(this.element, {
                keydown: function(u) {
                    if (this.element.prop("readOnly"))
                        return t = !0,
                        r = !0,
                        i = !0,
                        undefined;
                    t = !1;
                    r = !1;
                    i = !1;
                    var f = n.ui.keyCode;
                    switch (u.keyCode) {
                    case f.PAGE_UP:
                        t = !0;
                        this._move("previousPage", u);
                        break;
                    case f.PAGE_DOWN:
                        t = !0;
                        this._move("nextPage", u);
                        break;
                    case f.UP:
                        t = !0;
                        this._keyEvent("previous", u);
                        break;
                    case f.DOWN:
                        t = !0;
                        this._keyEvent("next", u);
                        break;
                    case f.ENTER:
                    case f.NUMPAD_ENTER:
                        this.menu.active && (t = !0,
                        u.preventDefault(),
                        this.menu.select(u));
                        break;
                    case f.TAB:
                        this.menu.active && this.menu.select(u);
                        break;
                    case f.ESCAPE:
                        this.menu.element.is(":visible") && (this._value(this.term),
                        this.close(u),
                        u.preventDefault());
                        break;
                    default:
                        i = !0;
                        this._searchTimeout(u)
                    }
                },
                keypress: function(r) {
                    if (t)
                        return t = !1,
                        (!this.isMultiLine || this.menu.element.is(":visible")) && r.preventDefault(),
                        undefined;
                    if (!i) {
                        var u = n.ui.keyCode;
                        switch (r.keyCode) {
                        case u.PAGE_UP:
                            this._move("previousPage", r);
                            break;
                        case u.PAGE_DOWN:
                            this._move("nextPage", r);
                            break;
                        case u.UP:
                            this._keyEvent("previous", r);
                            break;
                        case u.DOWN:
                            this._keyEvent("next", r)
                        }
                    }
                },
                input: function(n) {
                    return r ? (r = !1,
                    n.preventDefault(),
                    undefined) : (this._searchTimeout(n),
                    undefined)
                },
                focus: function() {
                    this.selectedItem = null;
                    this.previous = this._value()
                },
                blur: function(n) {
                    return this.cancelBlur ? (delete this.cancelBlur,
                    undefined) : (clearTimeout(this.searching),
                    this.close(n),
                    this._change(n),
                    undefined)
                }
            });
            this._initSource();
            this.menu = n("<ul>").addClass("ui-autocomplete ui-front").appendTo(this._appendTo()).menu({
                role: null
            }).hide().data("ui-menu");
            this._on(this.menu.element, {
                mousedown: function(t) {
                    t.preventDefault();
                    this.cancelBlur = !0;
                    this._delay(function() {
                        delete this.cancelBlur
                    });
                    var i = this.menu.element[0];
                    n(t.target).closest(".ui-menu-item").length || this._delay(function() {
                        var t = this;
                        this.document.one("mousedown", function(r) {
                            r.target === t.element[0] || r.target === i || n.contains(i, r.target) || t.close()
                        })
                    })
                },
                menufocus: function(t, i) {
                    if (this.isNewMenu && (this.isNewMenu = !1,
                    t.originalEvent && /^mouse/.test(t.originalEvent.type)))
                        return this.menu.blur(),
                        this.document.one("mousemove", function() {
                            n(t.target).trigger(t.originalEvent)
                        }),
                        undefined;
                    var r = i.item.data("ui-autocomplete-item");
                    !1 !== this._trigger("focus", t, {
                        item: r
                    }) ? t.originalEvent && /^key/.test(t.originalEvent.type) && this._value(r.value) : this.liveRegion.text(r.value)
                },
                menuselect: function(n, t) {
                    var i = t.item.data("ui-autocomplete-item")
                      , r = this.previous;
                    this.element[0] !== this.document[0].activeElement && (this.element.focus(),
                    this.previous = r,
                    this._delay(function() {
                        this.previous = r;
                        this.selectedItem = i
                    }));
                    !1 !== this._trigger("select", n, {
                        item: i
                    }) && this._value(i.value);
                    this.term = this._value();
                    this.close(n);
                    this.selectedItem = i
                }
            });
            this.liveRegion = n("<span>", {
                role: "status",
                "aria-live": "polite"
            }).addClass("ui-helper-hidden-accessible").insertBefore(this.element);
            this._on(this.window, {
                beforeunload: function() {
                    this.element.removeAttr("autocomplete")
                }
            })
        },
        _destroy: function() {
            clearTimeout(this.searching);
            this.element.removeClass("ui-autocomplete-input").removeAttr("autocomplete");
            this.menu.element.remove();
            this.liveRegion.remove()
        },
        _setOption: function(n, t) {
            this._super(n, t);
            "source" === n && this._initSource();
            "appendTo" === n && this.menu.element.appendTo(this._appendTo());
            "disabled" === n && t && this.xhr && this.xhr.abort()
        },
        _appendTo: function() {
            var t = this.options.appendTo;
            return t && (t = t.jquery || t.nodeType ? n(t) : this.document.find(t).eq(0)),
            t || (t = this.element.closest(".ui-front")),
            t.length || (t = this.document[0].body),
            t
        },
        _initSource: function() {
            var i, r, t = this;
            n.isArray(this.options.source) ? (i = this.options.source,
            this.source = function(t, r) {
                r(n.ui.autocomplete.filter(i, t.term))
            }
            ) : "string" == typeof this.options.source ? (r = this.options.source,
            this.source = function(i, u) {
                t.xhr && t.xhr.abort();
                t.xhr = n.ajax({
                    url: r,
                    data: i,
                    dataType: "json",
                    success: function(n) {
                        u(n)
                    },
                    error: function() {
                        u([])
                    }
                })
            }
            ) : this.source = this.options.source
        },
        _searchTimeout: function(n) {
            clearTimeout(this.searching);
            this.searching = this._delay(function() {
                this.term !== this._value() && (this.selectedItem = null,
                this.search(null, n))
            }, this.options.delay)
        },
        search: function(n, t) {
            return n = null != n ? n : this._value(),
            this.term = this._value(),
            n.length < this.options.minLength ? this.close(t) : this._trigger("search", t) !== !1 ? this._search(n) : undefined
        },
        _search: function(n) {
            this.pending++;
            this.element.addClass("ui-autocomplete-loading");
            this.cancelSearch = !1;
            this.source({
                term: n
            }, this._response())
        },
        _response: function() {
            var t = ++this.requestIndex;
            return n.proxy(function(n) {
                t === this.requestIndex && this.__response(n);
                this.pending--;
                this.pending || this.element.removeClass("ui-autocomplete-loading")
            }, this)
        },
        __response: function(n) {
            n && (n = this._normalize(n));
            this._trigger("response", null, {
                content: n
            });
            !this.options.disabled && n && n.length && !this.cancelSearch ? (this._suggest(n),
            this._trigger("open")) : this._close()
        },
        close: function(n) {
            this.cancelSearch = !0;
            this._close(n)
        },
        _close: function(n) {
            this.menu.element.is(":visible") && (this.menu.element.hide(),
            this.menu.blur(),
            this.isNewMenu = !0,
            this._trigger("close", n))
        },
        _change: function(n) {
            this.previous !== this._value() && this._trigger("change", n, {
                item: this.selectedItem
            })
        },
        _normalize: function(t) {
            return t.length && t[0].label && t[0].value ? t : n.map(t, function(t) {
                return "string" == typeof t ? {
                    label: t,
                    value: t
                } : n.extend({
                    label: t.label || t.value,
                    value: t.value || t.label
                }, t)
            })
        },
        _suggest: function(t) {
            var i = this.menu.element.empty();
            this._renderMenu(i, t);
            this.isNewMenu = !0;
            this.menu.refresh();
            i.show();
            this._resizeMenu();
            i.position(n.extend({
                of: this.element
            }, this.options.position));
            this.options.autoFocus && this.menu.next()
        },
        _resizeMenu: function() {
            var n = this.menu.element;
            n.outerWidth(Math.max(n.width("").outerWidth() + 1, this.element.outerWidth()))
        },
        _renderMenu: function(t, i) {
            var r = this;
            n.each(i, function(n, i) {
                r._renderItemData(t, i)
            })
        },
        _renderItemData: function(n, t) {
            return this._renderItem(n, t).data("ui-autocomplete-item", t)
        },
        _renderItem: function(t, i) {
            return n("<li>").append(n("<a>").text(i.label)).appendTo(t)
        },
        _move: function(n, t) {
            return this.menu.element.is(":visible") ? this.menu.isFirstItem() && /^previous/.test(n) || this.menu.isLastItem() && /^next/.test(n) ? (this._value(this.term),
            this.menu.blur(),
            undefined) : (this.menu[n](t),
            undefined) : (this.search(null, t),
            undefined)
        },
        widget: function() {
            return this.menu.element
        },
        _value: function() {
            return this.valueMethod.apply(this.element, arguments)
        },
        _keyEvent: function(n, t) {
            (!this.isMultiLine || this.menu.element.is(":visible")) && (this._move(n, t),
            t.preventDefault())
        }
    });
    n.extend(n.ui.autocomplete, {
        escapeRegex: function(n) {
            return n.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&")
        },
        filter: function(t, i) {
            var r = RegExp(n.ui.autocomplete.escapeRegex(i), "i");
            return n.grep(t, function(n) {
                return r.test(n.label || n.value || n)
            })
        }
    });
    n.widget("ui.autocomplete", n.ui.autocomplete, {
        options: {
            messages: {
                noResults: "No search results.",
                results: function(n) {
                    return n + (n > 1 ? " results are" : " result is") + " available, use up and down arrow keys to navigate."
                }
            }
        },
        __response: function(n) {
            var t;
            this._superApply(arguments);
            this.options.disabled || this.cancelSearch || (t = n && n.length ? this.options.messages.results(n.length) : this.options.messages.noResults,
            this.liveRegion.text(t))
        }
    })
}(jQuery),
function(n) {
    n.widget("ui.menu", {
        version: "1.10.4",
        defaultElement: "<ul>",
        delay: 300,
        options: {
            icons: {
                submenu: "ui-icon-carat-1-e"
            },
            menus: "ul",
            position: {
                my: "left top",
                at: "right top"
            },
            role: "menu",
            blur: null,
            focus: null,
            select: null
        },
        _create: function() {
            this.activeMenu = this.element;
            this.mouseHandled = !1;
            this.element.uniqueId().addClass("ui-menu ui-widget ui-widget-content ui-corner-all").toggleClass("ui-menu-icons", !!this.element.find(".ui-icon").length).attr({
                role: this.options.role,
                tabIndex: 0
            }).bind("click" + this.eventNamespace, n.proxy(function(n) {
                this.options.disabled && n.preventDefault()
            }, this));
            this.options.disabled && this.element.addClass("ui-state-disabled").attr("aria-disabled", "true");
            this._on({
                "mousedown .ui-menu-item > a": function(n) {
                    n.preventDefault()
                },
                "click .ui-state-disabled > a": function(n) {
                    n.preventDefault()
                },
                "click .ui-menu-item:has(a)": function(t) {
                    var i = n(t.target).closest(".ui-menu-item");
                    !this.mouseHandled && i.not(".ui-state-disabled").length && (this.select(t),
                    t.isPropagationStopped() || (this.mouseHandled = !0),
                    i.has(".ui-menu").length ? this.expand(t) : !this.element.is(":focus") && n(this.document[0].activeElement).closest(".ui-menu").length && (this.element.trigger("focus", [!0]),
                    this.active && 1 === this.active.parents(".ui-menu").length && clearTimeout(this.timer)))
                },
                "mouseenter .ui-menu-item": function(t) {
                    var i = n(t.currentTarget);
                    i.siblings().children(".ui-state-active").removeClass("ui-state-active");
                    this.focus(t, i)
                },
                mouseleave: "collapseAll",
                "mouseleave .ui-menu": "collapseAll",
                focus: function(n, t) {
                    var i = this.active || this.element.children(".ui-menu-item").eq(0);
                    t || this.focus(n, i)
                },
                blur: function(t) {
                    this._delay(function() {
                        n.contains(this.element[0], this.document[0].activeElement) || this.collapseAll(t)
                    })
                },
                keydown: "_keydown"
            });
            this.refresh();
            this._on(this.document, {
                click: function(t) {
                    n(t.target).closest(".ui-menu").length || this.collapseAll(t);
                    this.mouseHandled = !1
                }
            })
        },
        _destroy: function() {
            this.element.removeAttr("aria-activedescendant").find(".ui-menu").addBack().removeClass("ui-menu ui-widget ui-widget-content ui-corner-all ui-menu-icons").removeAttr("role").removeAttr("tabIndex").removeAttr("aria-labelledby").removeAttr("aria-expanded").removeAttr("aria-hidden").removeAttr("aria-disabled").removeUniqueId().show();
            this.element.find(".ui-menu-item").removeClass("ui-menu-item").removeAttr("role").removeAttr("aria-disabled").children("a").removeUniqueId().removeClass("ui-corner-all ui-state-hover").removeAttr("tabIndex").removeAttr("role").removeAttr("aria-haspopup").children().each(function() {
                var t = n(this);
                t.data("ui-menu-submenu-carat") && t.remove()
            });
            this.element.find(".ui-menu-divider").removeClass("ui-menu-divider ui-widget-content")
        },
        _keydown: function(t) {
            function o(n) {
                return n.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&")
            }
            var i, f, r, e, u, s = !0;
            switch (t.keyCode) {
            case n.ui.keyCode.PAGE_UP:
                this.previousPage(t);
                break;
            case n.ui.keyCode.PAGE_DOWN:
                this.nextPage(t);
                break;
            case n.ui.keyCode.HOME:
                this._move("first", "first", t);
                break;
            case n.ui.keyCode.END:
                this._move("last", "last", t);
                break;
            case n.ui.keyCode.UP:
                this.previous(t);
                break;
            case n.ui.keyCode.DOWN:
                this.next(t);
                break;
            case n.ui.keyCode.LEFT:
                this.collapse(t);
                break;
            case n.ui.keyCode.RIGHT:
                this.active && !this.active.is(".ui-state-disabled") && this.expand(t);
                break;
            case n.ui.keyCode.ENTER:
            case n.ui.keyCode.SPACE:
                this._activate(t);
                break;
            case n.ui.keyCode.ESCAPE:
                this.collapse(t);
                break;
            default:
                s = !1;
                f = this.previousFilter || "";
                r = String.fromCharCode(t.keyCode);
                e = !1;
                clearTimeout(this.filterTimer);
                r === f ? e = !0 : r = f + r;
                u = RegExp("^" + o(r), "i");
                i = this.activeMenu.children(".ui-menu-item").filter(function() {
                    return u.test(n(this).children("a").text())
                });
                i = e && -1 !== i.index(this.active.next()) ? this.active.nextAll(".ui-menu-item") : i;
                i.length || (r = String.fromCharCode(t.keyCode),
                u = RegExp("^" + o(r), "i"),
                i = this.activeMenu.children(".ui-menu-item").filter(function() {
                    return u.test(n(this).children("a").text())
                }));
                i.length ? (this.focus(t, i),
                i.length > 1 ? (this.previousFilter = r,
                this.filterTimer = this._delay(function() {
                    delete this.previousFilter
                }, 1e3)) : delete this.previousFilter) : delete this.previousFilter
            }
            s && t.preventDefault()
        },
        _activate: function(n) {
            this.active.is(".ui-state-disabled") || (this.active.children("a[aria-haspopup='true']").length ? this.expand(n) : this.select(n))
        },
        refresh: function() {
            var t, r = this.options.icons.submenu, i = this.element.find(this.options.menus);
            this.element.toggleClass("ui-menu-icons", !!this.element.find(".ui-icon").length);
            i.filter(":not(.ui-menu)").addClass("ui-menu ui-widget ui-widget-content ui-corner-all").hide().attr({
                role: this.options.role,
                "aria-hidden": "true",
                "aria-expanded": "false"
            }).each(function() {
                var t = n(this)
                  , i = t.prev("a")
                  , u = n("<span>").addClass("ui-menu-icon ui-icon " + r).data("ui-menu-submenu-carat", !0);
                i.attr("aria-haspopup", "true").prepend(u);
                t.attr("aria-labelledby", i.attr("id"))
            });
            t = i.add(this.element);
            t.children(":not(.ui-menu-item):has(a)").addClass("ui-menu-item").attr("role", "presentation").children("a").uniqueId().addClass("ui-corner-all").attr({
                tabIndex: -1,
                role: this._itemRole()
            });
            t.children(":not(.ui-menu-item)").each(function() {
                var t = n(this);
                /[^\-\u2014\u2013\s]/.test(t.text()) || t.addClass("ui-widget-content ui-menu-divider")
            });
            t.children(".ui-state-disabled").attr("aria-disabled", "true");
            this.active && !n.contains(this.element[0], this.active[0]) && this.blur()
        },
        _itemRole: function() {
            return {
                menu: "menuitem",
                listbox: "option"
            }[this.options.role]
        },
        _setOption: function(n, t) {
            "icons" === n && this.element.find(".ui-menu-icon").removeClass(this.options.icons.submenu).addClass(t.submenu);
            this._super(n, t)
        },
        focus: function(n, t) {
            var i, r;
            this.blur(n, n && "focus" === n.type);
            this._scrollIntoView(t);
            this.active = t.first();
            r = this.active.children("a").addClass("ui-state-focus");
            this.options.role && this.element.attr("aria-activedescendant", r.attr("id"));
            this.active.parent().closest(".ui-menu-item").children("a:first").addClass("ui-state-active");
            n && "keydown" === n.type ? this._close() : this.timer = this._delay(function() {
                this._close()
            }, this.delay);
            i = t.children(".ui-menu");
            i.length && n && /^mouse/.test(n.type) && this._startOpening(i);
            this.activeMenu = t.parent();
            this._trigger("focus", n, {
                item: t
            })
        },
        _scrollIntoView: function(t) {
            var e, o, i, r, u, f;
            this._hasScroll() && (e = parseFloat(n.css(this.activeMenu[0], "borderTopWidth")) || 0,
            o = parseFloat(n.css(this.activeMenu[0], "paddingTop")) || 0,
            i = t.offset().top - this.activeMenu.offset().top - e - o,
            r = this.activeMenu.scrollTop(),
            u = this.activeMenu.height(),
            f = t.height(),
            0 > i ? this.activeMenu.scrollTop(r + i) : i + f > u && this.activeMenu.scrollTop(r + i - u + f))
        },
        blur: function(n, t) {
            t || clearTimeout(this.timer);
            this.active && (this.active.children("a").removeClass("ui-state-focus"),
            this.active = null,
            this._trigger("blur", n, {
                item: this.active
            }))
        },
        _startOpening: function(n) {
            clearTimeout(this.timer);
            "true" === n.attr("aria-hidden") && (this.timer = this._delay(function() {
                this._close();
                this._open(n)
            }, this.delay))
        },
        _open: function(t) {
            var i = n.extend({
                of: this.active
            }, this.options.position);
            clearTimeout(this.timer);
            this.element.find(".ui-menu").not(t.parents(".ui-menu")).hide().attr("aria-hidden", "true");
            t.show().removeAttr("aria-hidden").attr("aria-expanded", "true").position(i)
        },
        collapseAll: function(t, i) {
            clearTimeout(this.timer);
            this.timer = this._delay(function() {
                var r = i ? this.element : n(t && t.target).closest(this.element.find(".ui-menu"));
                r.length || (r = this.element);
                this._close(r);
                this.blur(t);
                this.activeMenu = r
            }, this.delay)
        },
        _close: function(n) {
            n || (n = this.active ? this.active.parent() : this.element);
            n.find(".ui-menu").hide().attr("aria-hidden", "true").attr("aria-expanded", "false").end().find("a.ui-state-active").removeClass("ui-state-active")
        },
        collapse: function(n) {
            var t = this.active && this.active.parent().closest(".ui-menu-item", this.element);
            t && t.length && (this._close(),
            this.focus(n, t))
        },
        expand: function(n) {
            var t = this.active && this.active.children(".ui-menu ").children(".ui-menu-item").first();
            t && t.length && (this._open(t.parent()),
            this._delay(function() {
                this.focus(n, t)
            }))
        },
        next: function(n) {
            this._move("next", "first", n)
        },
        previous: function(n) {
            this._move("prev", "last", n)
        },
        isFirstItem: function() {
            return this.active && !this.active.prevAll(".ui-menu-item").length
        },
        isLastItem: function() {
            return this.active && !this.active.nextAll(".ui-menu-item").length
        },
        _move: function(n, t, i) {
            var r;
            this.active && (r = "first" === n || "last" === n ? this.active["first" === n ? "prevAll" : "nextAll"](".ui-menu-item").eq(-1) : this.active[n + "All"](".ui-menu-item").eq(0));
            r && r.length && this.active || (r = this.activeMenu.children(".ui-menu-item")[t]());
            this.focus(i, r)
        },
        nextPage: function(t) {
            var i, r, u;
            return this.active ? (this.isLastItem() || (this._hasScroll() ? (r = this.active.offset().top,
            u = this.element.height(),
            this.active.nextAll(".ui-menu-item").each(function() {
                return i = n(this),
                0 > i.offset().top - r - u
            }),
            this.focus(t, i)) : this.focus(t, this.activeMenu.children(".ui-menu-item")[this.active ? "last" : "first"]())),
            undefined) : (this.next(t),
            undefined)
        },
        previousPage: function(t) {
            var i, r, u;
            return this.active ? (this.isFirstItem() || (this._hasScroll() ? (r = this.active.offset().top,
            u = this.element.height(),
            this.active.prevAll(".ui-menu-item").each(function() {
                return i = n(this),
                i.offset().top - r + u > 0
            }),
            this.focus(t, i)) : this.focus(t, this.activeMenu.children(".ui-menu-item").first())),
            undefined) : (this.next(t),
            undefined)
        },
        _hasScroll: function() {
            return this.element.outerHeight() < this.element.prop("scrollHeight")
        },
        select: function(t) {
            this.active = this.active || n(t.target).closest(".ui-menu-item");
            var i = {
                item: this.active
            };
            this.active.has(".ui-menu").length || this.collapseAll(t, !0);
            this._trigger("select", t, i)
        }
    })
}(jQuery);
$(document).ready(function() {
    $(document).on("change", ".address-item", function() {
        $("#Address_Latitude").val("");
        $("#Address_Longitude").val("")
    })
}),
function(n) {
    function u(t) {
        n("#addressLoading").show();
        n("#enterAddressManually").hide();
        n("#postcodeServiceUnavailable").hide();
        n("#address-details").addClass("disabled");
        n.ajax({
            url: "//services.postcodeanywhere.co.uk/CapturePlus/Interactive/Retrieve/v2.10/json3.ws",
            dataType: "jsonp",
            data: {
                key: key,
                id: t
            },
            timeout: 5e3,
            success: function(t) {
                t.Items.length && (n("#address-details").removeClass("disabled"),
                n("#addressLoading").hide(),
                n("#enterAddressManually").show(),
                n("#addressManualWrapper").unbind("click"),
                n("#postcode-search").val(""),
                f(t.Items[0]))
            },
            error: function() {
                n("#postcodeServiceUnavailable").show();
                n("#enterAddressManually").hide();
                n("#addressLoading").hide();
                n("#address-details").removeClass("disabled")
            }
        })
    }
    function f(t) {
        n("#Companyname").val() || n("#Companyname").val(t.Company);
        n("#AddressLine1").val(t.Line1);
        n("#AddressLine2").val(t.Line2);
        n("#AddressLine3").val(t.Line3);
        n("#City").val(t.City);
        n("#Postcode").val(t.PostalCode);
        n("#ariaAddressEntered").text("Your address has been entered into the fields below.");
        e(t);
    }
    function e(t) {
        var r = "https://api.postcodes.io/postcodes/" + t.PostalCode, i;
        n.get(r).done(function(t) {
            i = t;
            i.status == 200 && i.result !== null && (n("#Address_Latitude").val(i.result.latitude),
            n("#Address_Longitude").val(i.result.longitude))
        }).fail(function() {})
    }
    var t = ""
      , r = n("form").attr("action")
      , i = n("#postcode-search").val();
    n("#enterAddressManually").on("click", function(t) {
        t.preventDefault();
        n("#addressManualWrapper").unbind("click");
        n("#address-details").removeClass("disabled");
        n("#AddressLine1").focus()
    });
    n("#addressManualWrapper").bind("click", function() {
        n(this).unbind("click");
        n("#address-details").removeClass("disabled");
        n("#AddressLine1").focus()
    });
    n("#postcode-search").keyup(function() {
        i = n(this).val()
    });
    n("#postcode-search").autocomplete({
        source: function(i, r) {
            n.ajax({
                url: "//services.postcodeanywhere.co.uk/CapturePlus/Interactive/Find/v2.10/json3.ws",
                dataType: "jsonp",
                data: {
                    key: key,
                    country: "GB",
                    searchTerm: i.term,
                    lastId: t
                },
                timeout: 5e3,
                success: function(t) {
                    n("#postcodeServiceUnavailable").hide();
                    n("#enterAddressManually").hide();
                    n("#addressLoading").show();
                    n("#postcode-search").one("blur", function() {
                        n("#enterAddressManually").show();
                        n("#addressLoading").hide()
                    });
                    r(n.map(t.Items, function(n) {
                        return {
                            label: n.Text,
                            value: "",
                            data: n
                        }
                    }))
                },
                error: function() {
                    n("#postcodeServiceUnavailable").show();
                    n("#enterAddressManually").hide();
                    n("#address-details").removeClass("disabled")
                }
            })
        },
        messages: {
            noResults: function() {
                return "We can't find an address matching " + i
            },
            results: function(n) {
                return "We've found " + n + (n > 1 ? " addresses" : " address") + " that match " + i + ". Use up and down arrow keys to navigate"
            }
        },
        select: function(i, r) {
            var f = r.item.data, e;
            f.Next == "Retrieve" ? u(f.Id) : (e = n(this),
            t = f.Id,
            n("#addressLoading").show(),
            n("#enterAddressManually").hide(),
            n("#postcodeServiceUnavailable").hide(),
            t === "GBR|" ? window.setTimeout(function() {
                e.autocomplete("search", f.Text)
            }) : window.setTimeout(function() {
                e.autocomplete("search", f.Id)
            }))
        },
        focus: function(t, i) {
            n("#addressInputWrapper").find(".ui-helper-hidden-accessible").text("To select " + i.item.label + ", press enter")
        },
        autoFocus: !0,
        minLength: 1,
        delay: 100
    }).focus(function() {
        t = ""
    })
}(jQuery)