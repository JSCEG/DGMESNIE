document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form"); // Selecciona tu formulario
    if (!form) return;

    // Referencias a campos específicos del formulario
    const userInput = document.getElementById('Correo');
    const passwordInput = document.getElementById('claveInput');

    // Expresiones regulares para validaciones
    // Detecta patrones maliciosos de SQL Injection y XSS
    const securityRegex = /(--|;|'|"|\b(OR|AND)\b\s*\d+|=\s*\d+|UNION\s+SELECT|DROP\s+TABLE|INSERT\s+INTO|DELETE\s+FROM|UPDATE\s+\w+|<script|1\s*=\s*1|script\s*:|javascript\s*:)/i;
    // Valida formato de RFC (personas morales y físicas)
    const rfcPattern = /^([A-ZÑ&]{3,4})(\d{6})([A-Z0-9]{3})$/;
    // Valida formato básico de email
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // Manejador del evento submit del formulario
    form.addEventListener("submit", function (event) {
        const tipoAcceso = document.getElementById('tipoAcceso')?.value;

        // Si es consulta pública, no se necesita validación de campos.
        // Se permite que el formulario se envíe a la URL ya establecida por login-unificado.js.
        if (tipoAcceso === 'publico') {
            // El script login-unificado.js se encarga de mostrar el estado de carga.
            return;
        }

        event.preventDefault(); // Previene el envío para usuarios registrados hasta validar.
        clearErrors(); // Limpia mensajes de error previos

        // === VALIDACIÓN DE SEGURIDAD ===
        let hasSecurityIssue = false;
        const inputs = form.querySelectorAll("input, textarea");

        // Revisa cada campo buscando patrones maliciosos
        inputs.forEach((input) => {
            if (securityRegex.test(input.value)) {
                hasSecurityIssue = true;
            }
        });

        // Si encuentra patrones maliciosos, redirige a página de error
        if (hasSecurityIssue) {
            window.location.href = "/Acceso/ActividadSospechosa";
            return;
        }

        // === VALIDACIÓN DE FORMATO ===
        let isValid = true;

        // Validación del campo usuario (RFC o email)
        const userValue = userInput.value.trim();
        if (!userValue) {
            showError(userInput, 'El campo es requerido');
            isValid = false;
        } else if (!isValidRFCorEmail(userValue)) {
            showError(userInput, 'Ingrese un RFC o correo electrónico válido');
            isValid = false;
        }

        // Validación del campo contraseña
        const passwordValue = passwordInput.value.trim();
        if (!passwordValue) {
            showError(passwordInput, 'La contraseña es requerida');
            isValid = false;
        }

        // Si todas las validaciones pasan, envía el formulario
        if (isValid) {
            form.submit();
        }
    });

    /**
     * Valida si el valor proporcionado es un RFC o email válido
     * @param {string} value - Valor a validar
     * @returns {boolean} - true si es válido, false si no
     */
    function isValidRFCorEmail(value) {
        return rfcPattern.test(value.toUpperCase()) || emailPattern.test(value);
    }

    /**
     * Muestra mensaje de error debajo del campo correspondiente
     * @param {HTMLElement} input - Campo de entrada con error
     * @param {string} message - Mensaje de error a mostrar
     */
    function showError(input, message) {
        const formGroup = input.closest('.input-group');
        const error = document.createElement('div');
        error.className = 'error-message';
        error.textContent = message;
        formGroup.insertAdjacentElement('afterend', error);
        input.classList.add('is-invalid');
    }

    /**
     * Limpia todos los mensajes de error y estados de error
     */
    function clearErrors() {
        document.querySelectorAll('.error-message').forEach(error => error.remove());
        document.querySelectorAll('.is-invalid').forEach(input =>
            input.classList.remove('is-invalid'));
    }
});

