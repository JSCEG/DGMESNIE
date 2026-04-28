-- Script de prueba para verificar las operaciones CRUD de Fuentes de Información
-- Ejecutar este script para probar la funcionalidad

-- 1. Verificar estructura de la tabla
SELECT TOP 5 * FROM dbo.FuentesdeInformacion;

-- 2. Insertar una fuente de prueba
INSERT INTO dbo.FuentesdeInformacion 
(Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion, Desagregacion, Sub_desagregacion, Unidades, Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario)
VALUES 
('Entidad de Prueba', 'Estadística', 'Energía', 'Fuente de Prueba CRUD', 'Datos de prueba para verificar CRUD', 'Nacional', 'Por estado', 'MW', 'Mensual', 'https://ejemplo.com', 'Fuente creada para pruebas');

-- 3. Verificar que se insertó correctamente
SELECT * FROM dbo.FuentesdeInformacion WHERE Etiqueta = 'Fuente de Prueba CRUD';

-- 4. Probar actualización
UPDATE dbo.FuentesdeInformacion 
SET Tipo = 'Reporte Actualizado', Comentario = 'Fuente actualizada en prueba'
WHERE Etiqueta = 'Fuente de Prueba CRUD';

-- 5. Verificar actualización
SELECT * FROM dbo.FuentesdeInformacion WHERE Etiqueta = 'Fuente de Prueba CRUD';

-- 6. Probar consultas que usa la aplicación
-- Obtener fuentes por entidad
SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion,
       Desagregacion, Sub_desagregacion, Unidades,
       Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
FROM dbo.FuentesdeInformacion
WHERE Entidad = 'Entidad de Prueba';

-- Obtener totales por entidad
SELECT Entidad, COUNT(*) AS Total 
FROM dbo.FuentesdeInformacion 
GROUP BY Entidad
ORDER BY Total DESC;

-- 7. Limpiar datos de prueba
DELETE FROM dbo.FuentesdeInformacion WHERE Etiqueta = 'Fuente de Prueba CRUD';

-- 8. Verificar que se eliminó
SELECT * FROM dbo.FuentesdeInformacion WHERE Etiqueta = 'Fuente de Prueba CRUD';

PRINT 'Script de prueba completado. Si no hay errores, las operaciones CRUD están funcionando correctamente.';