// ===== SISTEMA DE SEGURIDAD SNIER =====
// Protección básica para aplicaciones web corporativas

(function () {
    'use strict';

    // Variables de configuración
    const config = {
        isProduction: window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1',
        maxWarnings: 3,
        warningCount: 0
    };

    // Función para mostrar notificaciones de seguridad
    function showNotification(message) {
        // Crear notificación simple sin modal complejo
        const notification = document.createElement('div');
        notification.style.cssText = `
            position: fixed; top: 20px; right: 20px; 
            background: #f44336; color: white; padding: 15px;
            border-radius: 5px; z-index: 1000; font-size: 14px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        // Auto-remover después de 3 segundos
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }

    // Protección básica de teclado
    document.addEventListener('keydown', function (e) {
        // Solo en producción
        if (!config.isProduction) return;

        const blocked = [
            e.keyCode === 123, // F12
            e.ctrlKey && e.shiftKey && e.keyCode === 73, // Ctrl+Shift+I
            e.ctrlKey && e.keyCode === 85, // Ctrl+U
            e.ctrlKey && e.shiftKey && e.keyCode === 67, // Ctrl+Shift+C
            e.ctrlKey && e.shiftKey && e.keyCode === 74  // Ctrl+Shift+J
        ];

        if (blocked.some(condition => condition)) {
            e.preventDefault();
            showNotification('Función deshabilitada por seguridad');
            return false;
        }
    });

    // Protección básica contra clic derecho
    document.addEventListener('contextmenu', function (e) {
        if (config.isProduction) {
            e.preventDefault();
            showNotification('Menú contextual deshabilitado');
            return false;
        }
    });

    // Protección contra iframe (clickjacking)
    if (window.top !== window.self) {
        window.top.location = window.self.location;
    }

    // Limpiar campos sensibles al salir
    window.addEventListener('beforeunload', function () {
        try {
            const passwordFields = document.querySelectorAll('input[type="password"]');
            passwordFields.forEach(field => field.value = '');
        } catch (e) {
            // Ignorar errores silenciosamente
        }
    });

    // Detección simple de herramientas de desarrollo
    if (config.isProduction) {
        let devtoolsDetected = false;

        setInterval(function () {
            const heightDiff = window.outerHeight - window.innerHeight;
            const widthDiff = window.outerWidth - window.innerWidth;

            if ((heightDiff > 150 || widthDiff > 150) && !devtoolsDetected) {
                devtoolsDetected = true;
                config.warningCount++;

                showNotification('Herramientas de desarrollo detectadas');

                if (config.warningCount >= config.maxWarnings) {
                    window.location.href = '/';
                }

                // Reset después de 30 segundos
                setTimeout(() => {
                    devtoolsDetected = false;
                }, 30000);
            }
        }, 2000);
    }

    // Mensaje informativo en consola
    if (config.isProduction) {
        console.log('Sistema SNIER - Acceso restringido a herramientas de desarrollo');
    }

})();
