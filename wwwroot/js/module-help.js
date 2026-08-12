function mostrarAyudaModulo(moduleInfo) {
    // Verificar si moduleInfo es una cadena JSON
    if (typeof moduleInfo === 'string') {
        try {
            moduleInfo = JSON.parse(moduleInfo);
        } catch (e) {
            console.error('Error al parsear moduleInfo:', e);
            return;
        }
    }

    Swal.fire({
        title: moduleInfo.title,
        html: `
            <div class="text-start">
                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="bi bi-info-circle"></i> Descripción
                    </h6>
                    <p>${moduleInfo.description}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="bi bi-list-task"></i> Funcionalidad
                    </h6>
                    <p>${moduleInfo.functionality}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="bi bi-arrow-repeat"></i> Etapa del Ciclo
                    </h6>
                    <p>${moduleInfo.stage}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="bi bi-person-gear"></i> Roles
                    </h6>
                    <ul class="list-unstyled">
                        ${moduleInfo.roles.map(role =>
            `<li><i class="${Icono.clase(role.icon)}"></i> ${role.text}</li>`
        ).join('')}
                    </ul>
                </div>

                <div>
                    <h6 class="border-bottom pb-2">
                        <i class="bi bi-check-all"></i> Orden en el Ciclo
                    </h6>
                    <p class="mb-0">
                        <span class="badge bg-warning">Paso ${moduleInfo.order.step}</span>
                        <span class="ms-2">${moduleInfo.order.description}</span>
                    </p>
                </div>
            </div>
        `,
        width: '600px',
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#28a745',
        customClass: {
            container: 'text-start',
            popup: 'swal-help-popup',
            htmlContainer: 'swal-help-content'
        }
    });
}

// Agregar el botón de ayuda automáticamente
document.addEventListener('DOMContentLoaded', function () {
    const header = document.querySelector('.app-header-content');
    if (header) {
        const helpButton = document.createElement('button');
        helpButton.className = 'btn btn-outline-light ms-2';
        helpButton.innerHTML = '<i class="bi bi-question-circle"></i> Ayuda';
        helpButton.onclick = function () {
            const moduleInfo = document.querySelector('[data-module-info]');
            if (moduleInfo) {
                mostrarAyudaModulo(moduleInfo.dataset.moduleInfo);
            }
        };
        header.appendChild(helpButton);
    }
});