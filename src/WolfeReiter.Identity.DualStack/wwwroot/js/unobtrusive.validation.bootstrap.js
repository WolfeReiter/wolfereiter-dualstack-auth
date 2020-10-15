import "../lib/jquery/jquery.min.js";
import "../lib/jquery-validation/jquery.validate.min.js"
import "../lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"

"use strict"

var settings = {
    validClass: "is-valid",
    errorClass: "is-invalid"

};
$.validator.setDefaults(settings);
$.validator.unobtrusive.options = settings;