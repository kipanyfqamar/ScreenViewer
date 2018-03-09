
(function($) {
    // Define the togglebutton plugin.
    $.fn.togglebutton = function(opts) {
        // Apply the users options if exists.
        var settings = $.extend({}, $.fn.togglebutton.defaults, opts);

        // For each select element.
        this.each(function() {
            var self = $(this);
            var multiple = this.multiple;

            // Retrieve all options.
            var options = self.children('option');
            // Create an array of buttons with the value of select options.
            var buttons = options.map(function (index, opt) {

                if (opt.text != "-Select One-") {
                    var button = $("<button type='button' class='k-button' style='width:180px;height:30px;padding:0px 0px;'></button>")
                    //var button = $("<button type='button' class='k-button'></button>")
                    .prop('value', opt.value)
                    .text(opt.text);

                    // Add an `active` class if the option has been selected.
                    if (opt.selected)
                        button.addClass("k-state-selected");

                    // Return the button.
                    return button[0];
                }
            });

            // For each button, implement the click button removing and adding
            // `active` class to simulate the toggle effect. And also change the
            // select selected option.
            buttons.each(function(index, btn) {
                $(btn).click(function() {
                    // Retrieve all buttons siblings of the clicked one with an
                    // `active` class !
                    var activeBtn = $(btn).siblings(".k-state-selected");
                    var total = [];

                    // Check if the clicked button has the class `active`.
                    // Add or remove it according to the check.
                    if ($(btn).hasClass("k-state-selected"))  {
                        $(btn).removeClass("k-state-selected");
                    }
                    else {
                        $(btn).addClass("k-state-selected");
                        total.push(btn.value);
                    }
                   
                    // If the select allow multiple values, remove all active
                    // class to the other buttons (to keep only the last clicked
                    // button).
                    if (!multiple) {
                        activeBtn.removeClass("k-state-selected");
                    }
                    else {
                        // Push all active buttons value in an array.
                        activeBtn.each(function (index, btn) {
                            total.push(btn.value);
                        });
                    }

                    // Change selected options of the select.
                    self.val(total).change();
                });
            });

            // Group all the buttons in a `div` element.
            var width = (215 * opts) - (opts * 5);
            var btnGroup = $("<div class='btn-group' style='border:1px solid #D7D7D7;padding:10px;width: " + width + "px'>");
            var i = 1;
            var j = 1;

            $.each(buttons, function (index, element) {

                btnGroup.append("<div style='float:right;'>");
                btnGroup.append(element)
                btnGroup.append("</div>");

                if (i % opts === 0 && buttons.length != j) {
                    i = 0;
                    btnGroup.append("<hr style='height:1px; visibility:hidden; margin-bottom: -10px;' />");
                }

                i++;
                j++;
            });

            //var btnGroup = $("<div class='btn-group'>").append(buttons);

            // Include the buttons group after the select element.
            self.after(btnGroup);
            // Hide the display element.
            //self.hide();
            self.css({ 'width': '0px', 'height': '0px', 'overflow': 'hidden', 'display': 'inline-block', 'position': 'absolute', 'padding': '0px' });
        });
    };

    // Set the defaults options of the plugin.
    $.fn.togglebutton.defaults = {
    };

}(jQuery));
