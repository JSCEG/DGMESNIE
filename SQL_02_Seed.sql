/* ==========================================================
DG MESNIE - Seed de catálogos básicos
========================================================== */
INSERT INTO dgmesnie.CatClasificacion (Nombre, Descripcion) VALUES
(N'Ernesto / Mesas especiales DG', N'Seguimiento reservado a Ernesto/DG o instrucción especial.'),
(N'Mesas especiales DG', N'Seguimiento reservado a DG; equivalente operativo usado en Excel.'),
(N'Ruta crítica', N'Proyecto con bloqueo habilitante, riesgo regulatorio, ambiental, social o de interconexión.'),
(N'Autoconsumo', N'Proyecto de abasto aislado, cogeneración, autoconsumo o regularización relacionada.'),
(N'Migraciones', N'Ruta de migración LIE/LSE/MEM o ajuste de modalidad.'),
(N'Ventanilla', N'Entrada inicial para confirmar ruta, expediente o factibilidad.'),
(N'Inviables', N'Sin interés, permiso terminado, no factible o sin ruta activa.');
GO

INSERT INTO dgmesnie.CatPrioridad (Nombre, Orden) VALUES
(N'Alta', 1), (N'Media', 2), (N'Baja', 3), (N'Sin prioridad', 4);
GO

INSERT INTO dgmesnie.CatSemaforo (Nombre, ColorHex, Descripcion) VALUES
(N'Rojo', '#C00000', N'Riesgo crítico, vencido o bloqueo relevante.'),
(N'Amarillo', '#FFC000', N'Pendiente relevante o vencimiento próximo.'),
(N'Verde', '#00B050', N'En tiempo o sin bloqueo actual.'),
(N'Gris', '#808080', N'Sin información suficiente.'),
(N'Punto', '#808080', N'Indicador importado desde Excel cuando solo existe símbolo ●.');
GO

INSERT INTO dgmesnie.CatTipoDocumento (Nombre) VALUES
(N'Minuta'), (N'Ficha técnica'), (N'PPT'), (N'Oficio'), (N'Anexo'), (N'Otro');
GO

INSERT INTO dgmesnie.CatTecnologia (Nombre)
SELECT DISTINCT v.Nombre
FROM (VALUES (N'FV'),(N'EO'),(N'Hidro'),(N'COG'),(N'CI/COG'),(N'GEN'),(N'GEN/COG'),(N'GEN SLP')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dgmesnie.CatTecnologia t WHERE t.Nombre = v.Nombre);
GO

INSERT INTO dgmesnie.CatEstatusTramite (Nombre) VALUES
(N'Autorizada'),(N'AUTORIZADA'),(N'En evaluación'),(N'No ingresada'),(N'No presentado'),
(N'Por confirmar'),(N'Vigencia por confirmar'),(N'Por iniciar / vigencia por confirmar'),
(N'Pendiente / según ruta de modificación'),(N'El promovente No ha presentado a SEMARNAT el trámite');
GO
