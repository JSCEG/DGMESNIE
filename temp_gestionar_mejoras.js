// Mejoras para agregar a GestionarFuentes.cshtml

// 1. En actualizarTablaDetalle(), reemplazar la línea de botones:
tbody.innerHTML += `<tr>
	<td>${f.tipo || 'N/A'}</td>
	<td>${f.rubro || 'N/A'}</td>
	<td>${f.etiqueta || 'N/A'}</td>
	<td>${f.unidades || 'N/A'}</td>
	<td>${f.periodicidad_Corte_de_Informacion || 'N/A'}</td>
	<td>${enlaceHtml}</td>
	<td>
		<div class="btn-group" role="group">
			<button class="btn btn-sm btn-info" onclick="verFuenteCompleta(${f.id})" title="Ver detalles completos">
				<i class="fas fa-eye"></i>
			</button>
			<button class="btn btn-sm btn-secondary" onclick="verHistorialFuente(${f.id})" title="Ver historial de cambios">
				<i class="fas fa-history"></i>
			</button>
			<button class="btn btn-sm btn-warning" onclick="editarFuente(${f.id})" title="Editar fuente">
				<i class="fas fa-edit"></i>
			</button>
			<button class="btn btn-sm btn-danger" onclick="eliminarFuente(${f.id})" title="Eliminar fuente">
				<i class="fas fa-trash"></i>
			</button>
		</div>
	</td>
</tr>`;

// 2. En actualizarTablaDetalleFiltrada(), reemplazar la línea de botones:
tbody.innerHTML += `<tr>
	<td>${f.tipo || 'N/A'}</td>
	<td>${f.rubro || 'N/A'}</td>
	<td>${f.etiqueta || 'N/A'}</td>
	<td>${f.unidades || 'N/A'}</td>
	<td>${f.periodicidad_Corte_de_Informacion || 'N/A'}</td>
	<td>${enlaceHtml}</td>
	<td>
		<div class="btn-group" role="group">
			<button class="btn btn-sm btn-info" onclick="verFuenteCompleta(${f.id})" title="Ver detalles completos">
				<i class="fas fa-eye"></i>
			</button>
			<button class="btn btn-sm btn-secondary" onclick="verHistorialFuente(${f.id})" title="Ver historial de cambios">
				<i class="fas fa-history"></i>
			</button>
			<button class="btn btn-sm btn-warning" onclick="editarFuente(${f.id})" title="Editar fuente">
				<i class="fas fa-edit"></i>
			</button>
			<button class="btn btn-sm btn-danger" onclick="eliminarFuente(${f.id})" title="Eliminar fuente">
				<i class="fas fa-trash"></i>
			</button>
		</div>
	</td>
</tr>`;