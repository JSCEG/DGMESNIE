// ===== SISTEMA DE LOGIN UNIFICADO SNIER =====
// JavaScript para manejar el formulario de login unificado

document.addEventListener('DOMContentLoaded', function() {
    // URLs para las acciones del formulario (se establecerán desde la vista)
    window.urlAccesoInvitado = window.urlAccesoInvitado || '/Acceso/AccesoComoInvitado';
    window.urlLogin = window.urlLogin || '/Acceso/Login';
    window.tieneErrores = window.tieneErrores || false;
    
    // Si hay errores, mostrar campos de usuario registrado
    if (window.tieneErrores) {
        const tipoAccesoSelect = document.getElementById('tipoAcceso');
        if (tipoAccesoSelect) {
            tipoAccesoSelect.value = 'registrado';
            handleAccessTypeChange();
        }
    }
});

// Función para manejar el cambio de tipo de acceso
function handleAccessTypeChange() {
    const tipoAcceso = document.getElementById('tipoAcceso').value;
    const camposRegistrado = document.getElementById('camposRegistrado');
    const infoPublica = document.getElementById('infoPublica');
    const btnAcceso = document.getElementById('btnAcceso');
    const textoBoton = document.getElementById('textoBoton');
    const enlacesAdicionales = document.getElementById('enlacesAdicionales');
    const correoInput = document.getElementById('Correo');
    const claveInput = document.getElementById('claveInput');
    const botonSocial = document.getElementById('botonSocial');

    // Limpiar campos
    if (correoInput) correoInput.value = '';
    if (claveInput) claveInput.value = '';

    if (tipoAcceso === 'publico') {
        // Mostrar información de consulta pública
        camposRegistrado.style.display = 'none';
        infoPublica.style.display = 'block';
        enlacesAdicionales.style.display = 'none';
        btnAcceso.disabled = false;
        textoBoton.innerHTML = '<i class="bi bi-globe me-2"></i>Acceder como Consulta Pública';
        botonSocial.style.display = 'block';
        
        // Cambiar la acción del formulario para acceso público
        document.getElementById('loginForm').action = window.urlAccesoInvitado;
        
        // Remover required de los campos de usuario registrado
        if (correoInput) correoInput.removeAttribute('required');
        if (claveInput) claveInput.removeAttribute('required');
        
    } else if (tipoAcceso === 'registrado') {
        // Mostrar campos para usuario registrado
        camposRegistrado.style.display = 'block';
        infoPublica.style.display = 'none';
        enlacesAdicionales.style.display = 'block';
        btnAcceso.disabled = false;
        textoBoton.innerHTML = '<i class="bi bi-person-check me-2"></i>Iniciar Sesión';
        botonSocial.style.display = 'block';
        
        // Restaurar la acción del formulario para login normal
        document.getElementById('loginForm').action = window.urlLogin;
        
        // Agregar required a los campos de usuario registrado
        if (correoInput) correoInput.setAttribute('required', 'required');
        if (claveInput) claveInput.setAttribute('required', 'required');
        
        // Enfocar el campo de correo
        setTimeout(() => {
            if (correoInput) correoInput.focus();
        }, 100);
        
    } else {
        // No hay selección
        camposRegistrado.style.display = 'block';
        infoPublica.style.display = 'none';
        enlacesAdicionales.style.display = 'block';
        btnAcceso.disabled = false;
        textoBoton.textContent = '<i class="bi bi-person-check me-2"></i>Iniciar Sesión';
        botonSocial.style.display = 'block';

        // Restaurar la acción del formulario para login normal
        document.getElementById('loginForm').action = window.urlLogin;

        // Agregar required a los campos de usuario registrado
        if (correoInput) correoInput.setAttribute('required', 'required');
        if (claveInput) claveInput.setAttribute('required', 'required');
        
        // Enfocar el campo de correo
        setTimeout(() => {
            if (correoInput) correoInput.focus();
        }, 100);
    }
}

// Función para alternar visibilidad de contraseña
function togglePassword() {
    const passwordInput = document.getElementById('claveInput');
    const icon = document.getElementById('iconoClave');
    
    if (passwordInput && icon) {
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.className = 'bi bi-eye-slash-fill';
        } else {
            passwordInput.type = 'password';
            icon.className = 'bi bi-eye-fill';
        }
    }
}

// Manejar envío del formulario (compatible con validacion-formulario.js)
document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            const tipoAcceso = document.getElementById('tipoAcceso').value;
            
            if (!tipoAcceso) {
                e.preventDefault();
                showSecurityAlert('Por favor selecciona un tipo de acceso');
                return false;
            }
            
            // Para acceso público, permitir envío directo
            if (tipoAcceso === 'publico') {
                // Mostrar indicador de carga
                showLoadingState();
                return true; // Permitir envío
            }
            
            // Para usuarios registrados, validar campos
            if (tipoAcceso === 'registrado') {
                const correo = document.getElementById('Correo').value.trim();
                const clave = document.getElementById('claveInput').value.trim();
                
                if (!correo || !clave) {
                    e.preventDefault();
                    showSecurityAlert('Por favor completa todos los campos requeridos');
                    return false;
                }
                
                // Validación básica de email o RFC
                if (!isValidEmailOrRFC(correo)) {
                    e.preventDefault();
                    showSecurityAlert('Por favor ingresa un correo electrónico válido o RFC');
                    return false;
                }
            }
            
            // Mostrar indicador de carga
            showLoadingState();
            
            // Permitir que validacion-formulario.js haga sus validaciones adicionales
            // No prevenir el evento aquí para que el otro script pueda procesarlo
        });
    }
});

// Función para mostrar estado de carga
function showLoadingState() {
    const btnAcceso = document.getElementById('btnAcceso');
    if (btnAcceso) {
        const textoOriginal = btnAcceso.innerHTML;
        btnAcceso.disabled = true;
        btnAcceso.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Procesando...';
        
        // Restaurar después de un tiempo si no se envía
        setTimeout(() => {
            if (btnAcceso.disabled) {
                btnAcceso.disabled = false;
                btnAcceso.innerHTML = textoOriginal;
            }
        }, 10000);
    }
}

// Función para mostrar alertas de seguridad
function showSecurityAlert(message) {
    // Usar el sistema de alertas del archivo de seguridad si está disponible
    if (typeof showSecurityWarning === 'function') {
        showSecurityWarning(message);
    } else {
        alert(message);
    }
}

// Función para validar email o RFC
function isValidEmailOrRFC(value) {
    // Validación de email
    const emailRegex = new RegExp('^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$');
    // Validación básica de RFC (puede ser más específica según necesidades)
    const rfcRegex = new RegExp('^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$', 'i');
    
    return emailRegex.test(value) || rfcRegex.test(value);
}