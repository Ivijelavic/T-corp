/**
* @preserve jQuery Plugin: Table Filter - version 0.2.1
*
* LICENSE: http://hail2u.mit-license.org/2009
*/

/*jslint indent: 2, browser: true, regexp: true */
/*global jQuery, $ */

(function ($) {
    "use strict";

    $.fn.addTableFilter = function (options) {
        var o = $.extend({}, $.fn.addTableFilter.defaults, options),
          tgt,
          id,
          label,
          holder,
          holderInner,
          input,
          form;

        if (this.is("table")) {
            // Generate ID
            if (!this.attr("id")) {
                this.attr({
                    id: "t-" + Math.floor(Math.random() * 99999999)
                });
            }
            tgt = this.attr("id");
            id = tgt + "-filtering";

            // Build filtering form
            label = $("<label/>").attr({
                "for": id
            }).append(o.labelText);

            holder = $("<div/>").attr({ class: "navbar" });
            holderInner = $("<div/>").attr({ class: "navbar-inner" });

            input = $("<input/>").attr({
                id: id,
                size: o.size,
                class: "search-query",
                placeholder: o.labelText,
                type: "search"
            });
            
            form = $("<form/>").addClass("formTableFilter navbar-search pull-right").append("<div/>").addClass("input-append");
            form = form.append(input);

            holderInner = holderInner.append(form);

            $(holder).append(holderInner).insertBefore(this);

            //$("<form/>").addClass("formTableFilter navbar-search pull-left").append(input).insertBefore(this);

            // Bind filtering function
            $("#" + id).delayBind("keyup", function (e) {
                var words = $(this).val().toLowerCase().split(" ");
                $("#" + tgt + " tbody tr").each(function () {
                    var s = $(this).html().toLowerCase().replace(/<.+?>/g, "").replace(/\s+/g, " "),
            state = 0;
                    $.each(words, function () {
                        if (s.indexOf(this) < 0) {
                            state = 1;
                            return false; // break $.each()
                        }
                    });

                    if (state) {
                        $(this).hide();
                    } else {
                        $(this).show();
                    }
                });
            }, 300);
        }

        return this;
    };

    $.fn.addTableFilter.defaults = {
        labelText: "Keyword(s): ",
        size: 32
    };

    $.fn.delayBind = function (type, data, func, timeout) {
        if ($.isFunction(data)) {
            timeout = func;
            func = data;
            data = undefined;
        }

        var self = this,
      wait = null,
      handler = function (e) {
          clearTimeout(wait);
          wait = setTimeout(function () {
              func.apply(self, [$.extend({}, e)]);
          }, timeout);
      };

        return this.bind(type, data, handler);
    };
} (jQuery));
