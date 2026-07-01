// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    'use strict';

    function isFormValid(form) {
        if (window.jQuery) {
            var validator = jQuery(form).data('validator');
            if (validator) {
                return jQuery(form).valid();
            }
        }

        return typeof form.checkValidity !== 'function' || form.checkValidity();
    }

    function setButtonLoading(button) {
        if (!button || button.disabled) {
            return;
        }

        button.dataset.originalHtml = button.innerHTML;
        button.disabled = true;
        button.setAttribute('aria-busy', 'true');
        button.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' +
            (button.dataset.loadingText || 'Aguarde...');
    }

    document.addEventListener('submit', function (event) {
        var form = event.target;

        if (!(form instanceof HTMLFormElement) || form.dataset.noLoading === 'true') {
            return;
        }

        if (!isFormValid(form)) {
            return;
        }

        var submitter = event.submitter
            || form.querySelector('button[type="submit"]:focus')
            || form.querySelector('button[type="submit"]');

        setButtonLoading(submitter);
    }, true);

    function applyCepMask(input) {
        var format = function () {
            var digits = input.value.replace(/\D/g, '').slice(0, 8);
            input.value = digits.length > 5
                ? digits.slice(0, 5) + '-' + digits.slice(5)
                : digits;
        };

        input.addEventListener('input', format);
        format();
    }

    document.querySelectorAll('[data-mask="cep"]').forEach(applyCepMask);
})();
