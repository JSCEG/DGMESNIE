document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector(".login-form");
    if (!form) return;

    const userInput = document.getElementById('Correo');
    const passwordInput = document.getElementById('Clave');

    if (!userInput || !passwordInput) {
        return;
    }

    const securityRegex = /(--|;|'|"|\b(OR|AND)\b\s*\d+|=\s*\d+|UNION\s+SELECT|DROP\s+TABLE|INSERT\s+INTO|DELETE\s+FROM|UPDATE\s+\w+|<script|1\s*=\s*1|script\s*:|javascript\s*:)/i;
    const rfcPattern = /^([A-ZÑ&]{3,4})(\d{6})([A-Z0-9]{3})$/;
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        clearErrors();

        let hasSecurityIssue = false;
        const inputs = form.querySelectorAll("input, textarea");

        inputs.forEach((input) => {
            if (securityRegex.test(input.value)) {
                hasSecurityIssue = true;
            }
        });

        if (hasSecurityIssue) {
            window.location.href = "/Acceso/ActividadSospechosa";
            return;
        }

        let isValid = true;

        const userValue = userInput.value.trim();
        if (!userValue) {
            showError(userInput, 'El campo es requerido');
            isValid = false;
        } else if (!isValidRFCorEmail(userValue)) {
            showError(userInput, 'Ingrese un RFC o correo electrónico válido');
            isValid = false;
        }

        const passwordValue = passwordInput.value.trim();
        if (!passwordValue) {
            showError(passwordInput, 'La contraseña es requerida');
            isValid = false;
        }

        if (isValid) {
            form.submit();
        }
    });

    function isValidRFCorEmail(value) {
        return rfcPattern.test(value.toUpperCase()) || emailPattern.test(value);
    }

    function showError(input, message) {
        const formGroup = input.closest('.mb-3') || input.closest('.login-input') || input.parentElement;
        const error = document.createElement('div');
        error.className = 'error-message login-field-error';
        error.textContent = message;
        formGroup.insertAdjacentElement('beforeend' in formGroup ? 'beforeend' : 'afterend', error);
        const inputShell = input.closest('.login-input');
        if (inputShell) {
            inputShell.classList.add('is-invalid');
        }
        input.classList.add('is-invalid');
    }

    function clearErrors() {
        document.querySelectorAll('.error-message').forEach(error => error.remove());
        document.querySelectorAll('.is-invalid').forEach(input =>
            input.classList.remove('is-invalid'));
    }
});

