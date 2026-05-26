/* ==========================================================
DG MESNIE - Carga inicial desde workbook
Proyectos fuente: 60
Bitácora fuente: 38
Nota: reemplazar SharePointUrl cuando se tenga liga real al documento.
========================================================== */

-- 1) Empresas
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Acciona')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Acciona', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Akuwa Solar S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Akuwa Solar S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Aldesa')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Aldesa', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Almex')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Almex', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Alten Energías Renovables México Once, S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Alten Energías Renovables México Once, S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Banco Invex')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Banco Invex', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Bester')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Bester', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Campeche Energy S.A.P.I. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Campeche Energy S.A.P.I. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Compañía Minera Autlán')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Compañía Minera Autlán', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Delfín Solar S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Delfín Solar S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'EPM Eólica 24 S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'EPM Eólica 24 S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Elawan Energy')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Elawan Energy', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Enel Green Power')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Enel Green Power', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Energía Solar Herrera, S.A de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Energía Solar Herrera, S.A de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Energía Solar Herrera, S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Energía Solar Herrera, S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Energía Verde Meyer')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Energía Verde Meyer', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Energía Villa de Arriaga')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Energía Villa de Arriaga', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Energías Renovables de Tamaulipas, S. A. P. I. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Energías Renovables de Tamaulipas, S. A. P. I. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Eólica del Rocío S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Eólica del Rocío S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Flextronics')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Flextronics', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Gauss Energy')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Gauss Energy', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Global Solar América 2, S.A.P.I. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Global Solar América 2, S.A.P.I. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Green Park Energy, S. A. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Green Park Energy, S. A. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Grupo Carso')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Grupo Carso', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Grupo Neoen')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Grupo Neoen', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Hidroeléctrica Mizú')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Hidroeléctrica Mizú', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'INCO Renovables')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'INCO Renovables', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Jinkosolar Investment PTE. LTD')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Jinkosolar Investment PTE. LTD', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Malta Industries')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Malta Industries', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Martil Solar, S.A. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Martil Solar, S.A. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'OPDEnergy')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'OPDEnergy', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'PV Tamesí Solar SG, S. de R. L. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'PV Tamesí Solar SG, S. de R. L. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Papelera San Luis')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Papelera San Luis', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Proman')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Proman', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'SIMSA', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Saturno Solar S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Saturno Solar S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Smartener')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Smartener', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Solarcentury')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Solarcentury', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Solarmex I, S.A.P.I. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Solarmex I, S.A.P.I. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Sunstone Power 2, S. de R. L. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Sunstone Power 2, S. de R. L. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Sunstone Power, S. de R. L. de C. V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Sunstone Power, S. de R. L. de C. V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Ternium')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Ternium', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'VIVE ENERGIA')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'VIVE ENERGIA', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Vientos de Panabá S.A. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Vientos de Panabá S.A. de C.V.', N'MigracionExcel');
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Empresa WHERE Nombre = N'Zapoteca de Energía, S.A.P.I. de C.V.')
    INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (N'Zapoteca de Energía, S.A.P.I. de C.V.', N'MigracionExcel');
GO

-- 2) Documentos
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Nota Técnica Proyectos Firmes 07/05/2026', NULL, N'Nota Técnica Proyectos Firmes 07/05/2026', '2026-05-07', N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Solo 34Py')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Solo 34Py', NULL, N'Solo 34Py', NULL, N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', NULL, N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', '2026-05-20', N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'34Py + PPT')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'34Py + PPT', NULL, N'34Py + PPT', NULL, N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', NULL, N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', '2026-05-20', N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Solo PPT')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Solo PPT', NULL, N'Solo PPT', NULL, N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'20260421_ Minuta DZILAM.docx')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'20260421_ Minuta DZILAM.docx', NULL, N'20260421_ Minuta DZILAM.docx', '2026-04-21', N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Solo PPT (DGIRA)')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Solo PPT (DGIRA)', NULL, N'Solo PPT (DGIRA)', NULL, N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Otro';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Documento WHERE Titulo = N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx')
BEGIN
    INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, Descripcion)
    SELECT TipoDocumentoId, N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx', NULL, N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx', '2026-05-20', N'Importado desde Excel; pegar aquí la liga de SharePoint cuando exista.'
    FROM dgmesnie.CatTipoDocumento WHERE Nombre = N'Ficha técnica';
END
GO

-- 3) Reuniones
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Reunion WHERE Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026' AND FechaReunion = '2026-05-07')
BEGIN
    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
    SELECT N'Nota Técnica Proyectos Firmes 07/05/2026', '2026-05-07', d.DocumentoId, N'MigracionExcel'
    FROM dgmesnie.Documento d WHERE d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Reunion WHERE Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx' AND FechaReunion = '2026-05-20')
BEGIN
    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
    SELECT N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', '2026-05-20', d.DocumentoId, N'MigracionExcel'
    FROM dgmesnie.Documento d WHERE d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Reunion WHERE Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026' AND FechaReunion = '2026-05-20')
BEGIN
    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
    SELECT N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', '2026-05-20', d.DocumentoId, N'MigracionExcel'
    FROM dgmesnie.Documento d WHERE d.Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Reunion WHERE Titulo = N'20260421_ Minuta DZILAM.docx' AND FechaReunion = '2026-04-21')
BEGIN
    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
    SELECT N'20260421_ Minuta DZILAM.docx', '2026-04-21', d.DocumentoId, N'MigracionExcel'
    FROM dgmesnie.Documento d WHERE d.Titulo = N'20260421_ Minuta DZILAM.docx';
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Reunion WHERE Titulo = N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx' AND FechaReunion = '2026-05-20')
BEGIN
    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
    SELECT N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx', '2026-05-20', d.DocumentoId, N'MigracionExcel'
    FROM dgmesnie.Documento d WHERE d.Titulo = N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx';
END
GO

-- 4) Proyectos
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Ahumada II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Ahumada II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Bester'), N'No se identifica el proyecto', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ahumada', -106.511683996528, 30.617488589095, N'No se identifica el permiso', N'0', NULL, NULL, NULL, N'0', N'0', N'No se identifica el permiso', 0, NULL, N'[2026-05-07] Pertenecía a cluster original Ahumada (I-IV + 1). Solo Ahumada II quedó pendiente; los demás se construyeron. Infraestructura de conexión a SE ya preparada, pero plantas paradas la mayor parte del tiempo. Bester explora hibridar con otros FV conectados a la misma SE.

— Anterior —
La empresa evaluará el tiempo que les tomaría actualizar todos los trámites y permisos. Posible participación en próximas convocatorias.', N'Candidato a migrar', N'[2026-05-07] Bester evalúa hibridación con otros proyectos FV.

— Anterior —
Podría ser migración sin estudios. Confirmar que tenga todo vigente para que sea migración.
', N'Se requiere ubicación para ', N'La propia empresa declaró que no tiene interés en continuar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Ak Kin Green Power Park')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Ak Kin Green Power Park', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Solarcentury'), N'Ak Kin Green Power Park, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 100, 1, N'Coahuila de Zaragoza', N'Saltillo', -100.976747673551, 25.4381171646894, N'E/1972/GEN/2017', N'1044-17', '2019-05-01', '2020-05-01', '2020-06-24', N'Permiso terminado.', N'Sin solicitud de modificación pendiente de atención.', N'Permiso terminado el 05dic22 por renuncia de la Permisionaria', 0, NULL, N'La empresa ya no se encuentra activa, el proyecto fue abandonado desde 2018. No hay interés de la empresa por continuar con el desarrollo.', NULL, NULL, N'Sin conflictos identificados. ', N'El permiso terminó por renuncia, el proyecto fue abandonado y la empresa ya no está activa; no hay seguimiento que impulsar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Alaia II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Energía Solar Alaia II, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1505/GEN/2015', N'2536-24
2537-24
2851-24
2852-24', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración en trámite', N'Ingresó la solicitud de migración el 15sept2025

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.', 1, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] 4 proyectos Alaia (II-V, 120 MW) se desarrollan conjuntos: misma inversión, mismo punto de interconexión con LT 9 kV. Requieren revalidar estudios por incorporación de baterías y por modificación de emplazamientos (acercándolos de 45 km a 9 km a la SE). Implica modificar MIA y MISE.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', N'Pendiente', N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Revalidar estudios CENACE, modificar MIA y MISE, renovar licencias de construcción.

— Anterior —
Preguntar a Miguel si es una modificación del permiso o nuevo

Evaluar con Rafa si es que la EvIS puede dar el alcance

Revisar con CENACE qué procedería', N'Sin conflictos identificados. ', N'La empresa plantea mover el proyecto a un nuevo predio, consolidar los proyectos en un solo permiso y entrar por esquema mixto; esto requiere definir ruta regulatoria, revisar traslapes, iniciar nueva MIA/MISSE y coordinar interconexión con CENACE.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia III')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Alaia III', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Energía Solar Alaia III, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1506/GEN/2015', N'264-24 (Desistida)
2537-24 (requerimiemto)
2852-24 (en evaluación)
', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración en trámite', N'Ingresó la solicitud de migración el 17sept2025

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.', 1, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] Parte del cluster Alaia (II-V). Mismas condiciones que Alaia II: revalidar estudios, modificar MIA/MISE por reubicación de planta y baterías.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y 
preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', N'Pendiente', N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Revalidar estudios CENACE y modificar MIA/MISE.

— Anterior —
Citar a la empresa y ver si les interesa migrar, no pueden migrar si tienen pendientes. Ver si ya tiene una garantía.', N'Sin conflictos identificados. ', N'Aunque existía solicitud de migración, la ruta predominante ya no es una migración simple: el proyecto se reestructura hacia un nuevo proyecto consolidado, con posible esquema mixto, baterías y nuevos trámites ambientales/sociales.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia IV')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Alaia IV', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Energía Solar Alaia IV, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1507/GEN/2015', N'114-24 (Desistida)
2538-24 (requerimiento)
2854-24 (en evaluación)', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración en trámite', N'Ingresó la solicitud de migración el 17sept2025

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.', 1, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] Parte del cluster Alaia (II-V). Mismas condiciones que Alaia II.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', N'Pendiente', N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Revalidar estudios CENACE y modificar MIA/MISE.', N'Sin conflictos identificados. ', N'Cuenta con antecedentes de trámites/autorizaciones, pero la empresa confirmó que “todo se mueve”; por tanto, debe revisarse si procede como nuevo proyecto, con nueva MIA/MISSE, posible desistimiento de trámites previos y validación de interconexión.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia V')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Alaia V', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Energía Solar Alaia V, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1508/GEN/2015', N'115-24 (Desistida)
2539-24 ( requerimiento)
2855-24 (en evaluación)
', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración en trámite', N'Ingresó la solicitud de migración el 17sept2025

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.

', 1, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] Parte del cluster Alaia (II-V). Mismas condiciones que Alaia II.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', N'Pendiente', N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Revalidar estudios CENACE y modificar MIA/MISE.', N'Sin conflictos identificados. ', N'La ruta queda sujeta a la definición del proyecto consolidado de la empresa: nuevo predio, baterías, posible participación en mixtos, revisión de traslapes y eventual desistimiento/terminación de permisos o solicitudes anteriores.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Altair')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Altair', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Altair Importación y Exportación, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1503/GEN/2015', N'2533-24 (requerimiento de información)
2850-24', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración en trámite', N'Ingresó la solicitud de migración el 15sept2025

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.

Se indicó que no se avance con la migración hasta que se inicien las nuevas concovatorias de permisos.', 1, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] Comparte las mismas condiciones que EVA. SIMSA reportó que estos proyectos requieren migración completa de estudios y trámites.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', N'Pendiente', N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Migrar estudios CENACE; actualizar MIA/MISE/licencias.', N'Sin conflictos identificados. ', N'Forma parte del paquete SIMSA y debe tratarse junto con Alaia; aunque había migración en trámite, ahora predomina la definición de ruta crítica para consolidación, esquema mixto, permisos, MIA/MISSE e interconexión.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Amistad IV')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Amistad IV', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Enel Green Power'), N'Parque Amistad IV, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 150, 1, N'Coahuila de Zaragoza', N'Acuña', -100.96233400676, 29.3213925403945, N'E/2009/GEN/2018', N'2412-19 (Desistida)
2413-19 (Desistida)', '2017-06-30', '2028-12-30', '2028-12-31', N'Central totalmente construida en espera de la interconexión.', N'Sin solicitud de modificación pendiente de atención.', N'0', 1, NULL, N'[2026-05-07] 150 MW eólico. Parte del portafolio Enel; en evaluación de viabilidad comercial.

— Anterior —
Central 100% Construida, en proceso de Interconexión. Pertenece a emproblemadas. 
CENACE y CFE: Actualmente están en proceso de firma de contrato de interconexión. En paralelo se desarrolla de Esquema de Acción Remedial (estimado en ago-26, depende de proveedor) y, para la interconexión definitiva, la construcción de línea de transmisión de cerca de 25 km y otras adecuaciones en las SE Río Escondido y Piedras Negras Potencia. ', N'Seguimiento Ernesto', N'[2026-05-07] Continuar estudios y decisión de Enel sobre el portafolio.

— Anterior —
Esperar instrucción del DG de la Unidad. Actualizar estatus solo con información oficial que él comparta.', N'Se requiere ubicación', N'Está en seguimiento Ernesto/DG; la central está construida y en firma/interconexión, pero el seguimiento operativo queda reservado a mesas especiales.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Fotovoltaica Flex Guadalajara')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Fotovoltaica Flex Guadalajara', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Flextronics'), N'Parque de Tecnología Electrónica, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 0.96, 1, N'Jalisco', N'Zapopan', -103.418937603997, 20.6712204108769, N'E/2036/GEN/2018', N'0', '2017-12-01', '2018-06-30', '2018-07-31', N'La central ya se encuentra en operación desde el 25 de julio de 2018.', N'Se propuso para la sesión del CT del 07/05', N'La fecha de entrada en operación comercial drivado del incremento de capacidad es el 31 de diciembre de 2027.', 1, NULL, N'[2026-05-07] FV de 0.96 MW en Guadalajara con cogeneración legada migrada a LSE este año. No pudieron completar interconexión; buscan cerrarla. EvIS solicitada en 2017; necesita regularizar MISE y trámites ambientales.

— Anterior —
Central 100% Construida, en proceso de Interconexión. Pertenece a emproblemadas. CNE no permitía la convivencia con otro permiso CIL – Cogeneración. Sin embargo, el permiso ya migró y están retomando el proceso.
CFE: En proceso de firma de la Adenda al Contrato de Interconexión.
SENER: Regularizar EvIS/MISSE, no tienen archivo-oficio._x000B_SEMARNAT: Se ingresó trámite en 2017, sin respuesta. UEVISPI pidió número de trámite para el rastreo de la información.', N'Seguimiento Ernesto', N'[2026-05-07] Cerrar interconexión, regularizar MISE y trámites ambientales.

— Anterior —
Esperar instrucción del DG de la Unidad. Actualizar estatus solo con información oficial que él comparta.', N'Se requiere ubicación', N'Está en seguimiento Ernesto/DG; ya retomó proceso tras migración, pero mantiene pendientes de adenda, EvIS/MISSE y MIA fuera de la gestión ordinaria.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Kabil I')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Kabil I', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Aldesa'), N'Discovery Management, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 30, 1, N'Yucatán', N'Buctzotz', -88.7941804146185, 21.2023071172319, N'E/2057/GEN/2018', N'261-18', '2018-10-01', '2020-07-01', '2020-08-15', N'Permiso revocado.', N'Sin solicitud de modificación pendiente de atención.

Solicitud de permiso presentada el 22abr24, solicitud desechada el 28nov24. Presenta juicio de amparo 293/2024 y su Acumulado 295/2024 en contra del desechamiento, en proceso de atención la ejecutoria en contra del desechamiento.', N'Permiso revocado por no cumplir con el programa de obras autorizado en el permiso. Se encuentra un juicio de amparo en contra de la revocación del permiso E/2057/GEN/2018.

En proceso de atender la sentencia del juicio amparo 293/2024 y su Acumulado 295/2024', 1, NULL, N'[2026-05-07] Junto con Kabil II son idénticos y mismo lugar. Obtuvieron nuevo permiso pero no han renovado CI. Tierra y INAH vigentes; estudios ambientales se siguen ampliando; licencia de construcción vencida. CENACE: ''sin estudios vigentes''; CI en proceso de rescisión por CFE GRTP.

— Anterior —
Los proyectos cuentan con tierra, estudios ambientales, liberación de INAH y capacidad de reestructuración financiera. 
CNE: Kabil I enfrenta revocación del permiso por incumplimiento al Programa de Obras, lo cual afecta a Kabil II. Los años contemplados en la sanción terminan en 2027. Actualmente se encuentran en juicio de amparo contra la revocación 295/2024. 
CENACE: El foco rojo está en estudios y contratos de interconexión, afectados por la falta o incertidumbre del permiso de generación. 
SEMARNAT: Se reportan MIA, MISSE/EvIS y CPLI concluida para ambos proyectos; debe validarse vigencia.
 INAH / licencias: Cuentan con liberación de INAH; las licencias municipales habrían vencido, pero la empresa las considera regularizables. 
Judicial: Persisten juicios de amparo vinculados a la revocación; podrían afectar permisos nuevos o continuidad.
La empresa argumento su interés y disposición en solucionar los juicios y regularizar los trámites y estudios.', N'Rojo', N'[2026-05-07] Reestructuración financiera completa y nuevos estudios.

— Anterior —
rojo', N' El municipio de Buctzotz cuenta con comunidades mayas hablantes (como La Gran Lucha y Santo Domingo) que tienen presencia cultural. Para este rpoyecto se llevó a cabo una Consulta Previa. Los proyectos han generado conflictos sociales por consultas “en tiempo récord” a la comunidad maya de La Gran Lucha, donde jornaleros denunciaron despidos tras manifestarse por falta de transparencia y beneficios insuficientes.  
Aunque no se ubica dentro de una área natural protegida, el proyecto está cerca de ecosistemas frágiles del noreste de Yucatán. 
No se encontraron amparos ni denuncias relevantes vinculados a la empresa. 
La presencia criminal reciente indica que la violencia vinculada al crimen organizado, especialmente del Cártel del Milenio/Familia Michoacana, está en aumento en el municipio. 
No se encontraron riesgos políticos relevantes. ', N'El permiso aparece revocado y sujeto a juicio de amparo; no puede avanzar como trámite ordinario hasta resolver la situación jurídica y regulatoria.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Kabil II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Kabil II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Aldesa'), N'Discovery Management, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 30, 1, N'Yucatán', N'Buctzotz', -88.7941804146185, 21.2023071172319, N'E/2058/GEN/2018', N'261-18', '2018-10-01', '2020-07-01', '2020-08-15', N'Permiso vigente', N'Sin solicitud de modificación pendiente de atención.

Solicitud de permiso presentada el 22abr24, solicitud desechada el 28nov24. Presenta juicio de amparo 293/2024 y su Acumulado 295/2024 en contra del desechamiento, en proceso de atención la ejecutoria en contra del desechamiento.', N'Permiso revocado por no cumplir con el programa de obras autorizado en el permiso.

Mediante la resolución CNE/RES/666/2025 de 17dic25 la CNE resolvió dejar insubsistente la resolución por la que se revocó el permiso. Por lo que a partir del 17dic25 el permiso E/2058/GEN/2018 sigue vigente.

En proceso de atender la sentencia del juicio amparo 293/2024 y su Acumulado 295/2024', 1, NULL, N'[2026-05-07] Idéntico a Kabil I y mismo lugar. Permiso vigente según CNE, pero CI también en proceso de rescisión.

— Anterior —
Los proyectos cuentan con tierra, estudios ambientales, liberación de INAH y capacidad de reestructuración financiera. 
CNE: Kabil I enfrenta revocación del permiso por incumplimiento al Programa de Obras, lo cual afecta a Kabil II. Los años contemplados en la sanción terminan en 2027. Actualmente se encuentran en juicio de amparo contra la revocación 295/2024. 
CENACE: El foco rojo está en estudios y contratos de interconexión, afectados por la falta o incertidumbre del permiso de generación. 
SEMARNAT: Se reportan MIA, MISSE/EvIS y CPLI concluida para ambos proyectos; debe validarse vigencia.
 INAH / licencias: Cuentan con liberación de INAH; las licencias municipales habrían vencido, pero la empresa las considera regularizables. 
Judicial: Persisten juicios de amparo vinculados a la revocación; podrían afectar permisos nuevos o continuidad.
La empresa argumento su interés y disposición en solucionar los juicios y regularizar los trámites y estudios.', N'Rojo', N'[2026-05-07] Reestructuración financiera completa y nuevos estudios.

— Anterior —
rojo', N' El municipio de Buctzotz cuenta con comunidades mayas hablantes (como La Gran Lucha y Santo Domingo) que tienen presencia cultural. Los proyectos han generado conflictos sociales por consultas “en tiempo récord” a la comunidad maya de La Gran Lucha, donde jornaleros denunciaron despidos tras manifestarse por falta de transparencia y beneficios insuficientes. 
 Aunque no se ubica dentro de una área natural protegida, el proyecto está cerca de ecosistemas frágiles del noreste de Yucatán.
 No se encontraron amparos ni denuncias relevantes vinculados a la empresa.
 La presencia criminal reciente indica que la violencia vinculada al crimen organizado, especialmente del Cártel del Milenio/Familia Michoacana, está en aumento en el municipio. 
No se encontraron riesgos políticos relevantes. ', N'Tiene antecedentes de revocación/amparo y la interconexión depende de certeza jurídica del permiso; antes de avanzar deben resolverse amparos y validarse vigencias.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.Ca
tPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central La Victoria')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central La Victoria', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Grupo Carso'), N'CE G. Sanborns 2, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 0.9, 1, N'Querétaro', N'Querétaro', -100.397250098705, 20.5896580896207, N'E/1947/GEN/2017', N'0', '2017-05-02', '2019-02-28', '2019-06-15', N'La central ya se encuentra en operación desde el 05 de junio de 2019.', N'Sin solicitud de modificación pendiente de atención.', N'Está clasificada como Abasto Aislado si se requiere la interconexión para la venta al mercado, deberá migrar.', 1, NULL, N'MISSE: No ingresada
CNE: Está clasificada como Abasto Aislado si se requiere la interconexión para la venta al mercado, deberá migrar.
Central 100% construida, con contrato vencido en diciembre 2025. Están interesados en la migración de la Central.', N'Ventanilla Única de Autoconsumos', N'Citar para ver si les interesa y entren en la Convocatoria. Conocer si ya cuentan con el Anexo IV.', N'Se requiere ubicación', N'Es una central de abasto aislado ya construida; la ruta predominante es revisar Anexo IV, contrato vencido y conveniencia de regularizarse por autoconsumo o migrar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Autoconsumo'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Media'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Los Pinos')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Los Pinos', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Aldesa'), N'No se identifica el proyecto', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Zacatecas', N'Pinos', -101.574719253257, 22.2975086818728, N'No se identifica el permiso', N'0', NULL, NULL, NULL, N'0', N'0', N'No se identifica el permiso', 0, NULL, N'[2026-05-07] Declarado INVIABLE por la propia empresa Aldesa, debido a los refuerzos requeridos.

— Anterior —
La ampliación de la Línea de Transmisión elevó los costos de desarrollo del proyecto haciéndolo inviable. La empresa no tiene interés por continuar.', NULL, N'[2026-05-07] —', N'Se requiere ubicación para ', N'La ampliación de la línea de transmisión volvió inviable el proyecto y la empresa no tiene interés en continuar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Pachamama II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Pachamama II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Grupo Neoen'), N'ENR NL, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 330, 1, N'Puebla', N'Tepeyahualco', -97.4902591981423, 19.4902070350883, N'E/1984/GEN/2017', N'743-17', '2020-01-12', '2028-12-15', '2028-12-15', N'La central se encuentra en construcción.', N'Sin solicitud de modificación pendiente de atención.', N'0', 1, NULL, N'[2026-05-07] Neoen NO compareció a la reunión del 07/05/2026. Según CENACE: adenda al CI con FEOC al 15-dic-2028, se reporta en fase de construcción aunque no se conoce el grado real de avance.

— Anterior —
El proyecto cuenta con trámites ambientales y sociales aprobados. Ingresaron modificación a la MIA y ETJ debido a ampliación de permiso de construcción, ampliado hasta diciembre de 2028.
Firmaron contrato para las obras de interconexión línea de transmisión y subestación elevadora de la planta con CFE. Están en proceso de licitación la ejecución de la planta solar y el SAE.

El permiso y el contrato de interconexión está regularizado. Sin embargo, a la empresa le interesa migrar el permiso para ampliar la vigencia, así como contar con DEF para reconocimiento de Potencia.

CNE: Analizan la migración del permiso a LSE para ampliar su vigencia
 
CFE: Trabajan en la regularización del Contrato de Interconexión

SEMARNAT: Modificación de MIA y ETJ con la nueva Entrada en Operación

CENACE: Plantean ingresar solicitud de estudio en DEF por el 100% de SAE', N'Seguimiento Ernesto', N'[2026-05-07] Pendiente reunión de seguimiento con Neoen.

— Anterior —
Esperar instrucción del DG de la Unidad. Actualizar estatus solo con información oficial que él comparta.', N'Conflicto social relacionado con el parque fotovoltaico  Pachamama II, debido a las disputas entre los sindicatos Confederación Libertad y Trabajo de México y la Confederación de Trabajadores Mexicanos (CTM) por los contratos de construcción. El conflicto entre ambos sindicatos terminó en la muerte de Luis Miguel Espinosa Sosa (hermano del presidente municipal de Tepeyahualco); sin embargo, este incidente no se relaciona directamente con el proyecto, empresa o tecnología, sino con la disputa y rivalidad entre ambos grupos sindicales. ', N'Está en seguimiento Ernesto/DG; aunque tiene permiso/contrato regularizado y ruta SAE/migración, la atención quedó reservada a mesas especiales.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Chicxulub I')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Chicxulub I', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Elawan Energy'), N'Eólica del Mayab, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 70, 1, N'Yucatán', N'Ixil', -89.482152452721, 21.1522430299409, N'E/2102/GEN/2018', N'379-16', '2018-11-01', '2026-03-10', '2026-05-16', N'La central no cuenta con un avance de su programa de obras.', N'Sin solicitud de modificación pendiente de atención.', N'La central se encuentra en suspensión de actividades por juicios de amparo 232/2019, 139/2021 y 948/2023.', 1, NULL, N'[2026-05-07] Más de 20 juicios de amparo/nulidad de comunidad maya de Timul. 18 ganados; caso 174/20 sobreseído por fallecimientos en pandemia, pero Franco Lamogli interpuso revisión que sigue pendiente. Permiso vence 16-may-2026. CI vigente con FEOC vencida; garantías cubiertas a dic-2026.

— Anterior —
Interconexión agrupada. Chicxulub I: A pesar de contar con la propiedad de la tierra, todos los trámites, permisos y Contrato de Interconexión, las obras se encuentran suspendidas por 3 juicios de amparo de comunidades aledañas. Su argumento principal es que trasgreden su derecho de consulta. Chicxulub II: Se encuentra suspendida dados los juicios del proyecto I, no pueden avanzar en trámites hasta su resolución. 
Empresa: Mantiene interés en continuar Chicxulub I, pese a litigios prolongados. 
CNE / permiso: Buscaría prórroga o modificación antes del vencimiento señalado del 16 de mayo de 2026. 
CENACE: La interconexión está vinculada con Chicxulub II; resolver Chicxulub I es condición para avanzar. 
SEMARNAT: Cuenta con MIA y CPLI; ETJ en trámite. 
INAH / CONAGUA: Reporta liberación INAH y no afectación al manto acuífero. 
Judicial / social: Principal bloqueo: amparos y nulidades. La empresa señala apoyo de la comunidad de Timul.
UEVISPI está en espera del informe detallado de juicios por parte de la empresa.', N'Rojo', N'[2026-05-07] Solicitar prórroga del permiso por fuerza mayor.

— Anterior —
rojo', N'El promovente pretendía desarrollar los proyectos Chicxulub I y II en el municipio de Ixil, sin embargo, tuvo problemas agrarios y decidió abandonar los proyectos en ese municipio. Posteriormente decidió promover un nuevo proyecto denominado Chicxulub en el municipio de Timul, Yucatán. Para dicho proyecto ya se realizó Consulta Indígena. Se recomienda sustituir  Parque Eólico Chicxulub II por Chicxulub, en caso de que sea viable. ', N'La obra está suspendida por juicios de amparo y el proyecto no muestra avance; debe resolverse el bloqueo social/judicial antes de cualquier gestión.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Chicxulub II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Chicxulub II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Elawan Energy'), N'Elawan Wind México I, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 86, 1, N'Yucatán', N'Ixil', -89.482152452721, 21.1522430299409, N'E/2075/GEN/2018', N'1050-18', '2019-06-01', '2022-03-29', '2022-05-29', N'Permiso terminado.', N'Sin solicitud de modificación pendiente de atención.', N'Permiso terminado el 06jun24 por renuncia de la Permisionaria', 1, NULL, N'[2026-05-07] Elawan desistió del permiso original (planeaba cambio de ubicación). Quiere resolver primero situación de Chicxulub I. CI vigente con FEOC vencida; garantías cubiertas a dic-2026.

— Anterior —
Interconexión agrupada. Chicxulub I: A pesar de contar con la propiedad de la tierra, todos los trámites, permisos y Contrato de Interconexión, las obras se encuentran suspendidas por 3 juicios de amparo de comunidades aledañas. Su argumento principal es que trasgreden su derecho de consulta. Chicxulub II: Se encuentra suspendida dados los juicios del proyecto I, no pueden avanzar en trámites hasta su resolución. 
Empresa: Mantiene interés en continuar Chicxulub I, pese a litigios prolongados. 
CNE / permiso: Buscaría prórroga o modificación antes del vencimiento señalado del 16 de mayo de 2026. 
CENACE: La interconexión está vinculada con Chicxulub II; resolver Chicxulub I es condición para avanzar. 
SEMARNAT: Cuenta con MIA y CPLI; ETJ en trámite. 
INAH / CONAGUA: Reporta liberación INAH y no afectación al manto acuífero. 
Judicial / social: Principal bloqueo: amparos y nulidades. La empresa señala apoyo de la comunidad de Timul.
UEVISPI está en espera del informe detallado de juicios por parte de la empresa.', N'Rojo', N'[2026-05-07] Resolver Chicxulub I antes de retomar Chicxulub II. Evaluar nueva ubicación.

— Anterior —
rojo', N'El promovente pretendía desarrollar los proyectos Chicxulub I y II en el municipio de Ixil, sin embargo, tuvo problemas agrarios y decidió abandonar los proyectos en ese municipio. Posteriormente decidió promover un nuevo proyecto denominado Chicxulub en el municipio de Timul, Yucatán. Para dicho proyecto ya se realizó Consulta Indígena. Se recomienda sustituir  Parque Eólico Chicxulub II por Chicxulub, en caso de que sea viable. ', N'El proyecto depende de resolver conflictos sociales, consulta/amparos y su relación con Chicxulub I; además el permiso tenía una fecha de término el 16 de mayo 2026, por lo que no es una ruta simple.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Ciénega de Mata (Central Jalisco)')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Ciénega de Mata (Central Jalisco)', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Enel Green Power'), N'No se identifica el proyecto', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 200, 1, N'Jalisco', N'Lagos de Moreno y Ojuelos de Jalisco', -101.931451419868, 21.364720185899, N'No se identifica el permiso', N'54-16', NULL, NULL, NULL, N'0', N'0', N'No se identifica el permiso', 0, NULL, N'[2026-05-07] Parte de los 5 proyectos Enel. Sin avance constructivo según CENACE.

— Anterior —
La empresa no cuenta con posesión de los terrenos para el desarrollo de los proyectos. No tienen contratos ni trámites vigentes. No hay interés de la empresa por continuar con el desarrollo.', NULL, N'[2026-05-07] Pendiente confirmación de Enel sobre ejecución.', N'Sin conflictos identificados. ', N'No se identifica permiso y la empresa no tiene terrenos, contratos ni trámites vigentes ni interés en continuar; no hay ruta activa.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Cogeneración Abasto Aislado - Malta Texco Texcoco')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Cogeneración Abasto Aislado - Malta Texco Texcoco', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Malta Industries'), N'Malta Texo de México, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 1.09, 1, N'Estado de México', N'Texcoco', -98.8814619329942, 19.5067973873946, N'E/2286/GEN/2022', N'1720-21', '2020-09-02', '2023-04-30', '2025-06-30', N'La central ya se encuentra en operación desde el 21 de mayo de 2025.', N'Sin solicitud de modificación pendiente de atención.', N'La central se encuentra operando en modo ISLA ', 1, NULL, N'MISSE: Autorizada, sin consulta.
CNE: La central se encuentra operando en modo ISLA ', N'Pendiente', N'Insistir en contactar a la empresa', N'Sin conflictos identificados. ', N'Opera en modo isla bajo lógica de abasto aislado; antes de moverlo se debe contactar a la empresa y confirmar si seguirá en autoconsumo o buscará interconexión/excedentes.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Autoconsumo'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Media'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Cogeneración Industrial Papelera San Luis')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Cogeneración Industrial Papelera San Luis', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Papelera San Luis'), N'Industrial Papelera San Luis, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 1.981, 1, N'San Luis Potosí', N'San Luis Potosí', -100.976779603468, 22.1557169141662, N'E/1766/GEN/2016', N'502-15', '2016-02-29', '2017-12-31', '2017-12-31', N'La central ya se encuentra en operación desde el 01 de octubre de 2017.', N'Sin solicitud de modificación pendiente de atención.', N'Esta clasificada como Abasto Aislado si se requiere la interconexión para la venta al mercado, deberá migrar.', 1, NULL, N'MISSE: Autorizada, sin consulta.
CNE: Está clasificada como Abasto Aislado si se requiere la interconexión para la venta al mercado, deberá migrar.
Central 100% construida, no interconectada. Están previos a la etapa de pruebas con CENACE. Están interesados en continuar.', N'Ventanilla Única de Autoconsumos', N'Contactar a la empresa para poder conocer el estado de los trámites pendientes con CFE. Autoconsumo.

Citar a la empresa para ver si les conviene la migración', N'Sin conflictos identificados. ', N'Central de cogeneración/abasto aislado ya construida; el primer paso es validar trámites con CFE y definir si continúa como autoconsumo o si le conviene migrar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Autoconsumo'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Media'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Energia para Bebidas Carbonatadas')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Energia para Bebidas Carbonatadas', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'INCO Renovables'), N'Energía para Bebidas Carbonatadas, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 5, 1, N'Querétaro', N'San Juan del Río', -99.984641930294, 20.3952433795527, N'E/2000/GEN/2017', N'1178-16', '2017-08-30', '2018-10-02', '2018-10-02', N'La central ya se encuentra en operación desde el 02 de octubre de 2018.', N'Sin solicitud de modificación pendiente de atención.', N'Permiso revocado el 22sep23 por no haber pagado la supervisión del permiso', 1, NULL, N'MISSE: Autorizada, sin consulta.
CNE: Permiso revocado el 22sep23 por no haber pagado la supervisión del permiso', N'Pendiente', N'Permiso revocado hasta sep 2026', N'Sin conflictos identificados, aunque existe una  creciente sensibilidad relacionada con el uso industrial del agua y la expansión de infraestructura energética e industrial.', N'Aunque hay interés, el permiso está revocado por falta de pago de supervisión; antes de cualquier gestión debe aclararse si existe vía real de regularización.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Baja'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Fotovoltaico Flex')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Fotovoltaico Flex', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Flextronics'), N'Parque de Tecnología Electrónica, S. A. de C. V. ', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 2.095, 1, N'Chihuahua', N'Juárez', -106.426484166715, 31.6905651808822, N'E/2015/GEN/2018', N'704-17', '2018-01-31', '2018-08-30', '2018-09-28', N'La central ya se encuentra en operación desde el 22 de septiembre de 2018.', N'Sin solicitud de modificación pendiente de atención.', N'Esta clasificada como Abasto Aislado si se requiere la interconexión para la venta al mercado, deberá migrar.', 1, NULL, N'[2026-05-07] FV de 2 MW en Zacatecas para autoabastecimiento. Busca cierre de operación comercial para formalizar CI. En proceso de implementación del sistema de medición; solicita extensión.

— Anterior —
Central 100% Construida, en proceso de Interconexión. Pertenece a emproblemadas.

CFE: En proceso de firma de la Adenda al Contrato de Interconexión.
SENER: Regularizar EvIS/MISSE_x000B_SEMARNAT: Regularizar MIA', N'Seguimiento Ernesto', N'[2026-05-07] Implementar sistema de medición y cerrar operación comercial.

— Anterior —
Esperar instrucción del DG de la Unidad. Actualizar estatus solo con información oficial que él comparta.', N'Sin conflictos identificados. ', N'Está en seguimiento Ernesto/DG; aunque es una central construida con pendientes de adenda, EvIS/MIA e interconexión, no corresponde gestionarla por la vía ordinaria.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Genomma Lab')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Genomma Lab', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Banco Invex'), N'Genomma Lab Internacional, S. A. B. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 2, 1, N'Estado de México', N'Toluca', -99.6554903244442, 19.2828881826586, N'E/2232/GEN/2020', N'3077-19', '2019-04-01', '2020-08-01', '2020-12-30', N'La central ya se encuentra en operación desde el 26 de noviembre de 2020.', N'Sin solicitud de modificación pendiente de atención.', N'La central opera bajo la modalidad de abasto aislado sin inyección de energía eléctrica al Sistema Eléctrico Nacional', 0, NULL, N'MISSE: Autorizada, sin consulta.
CNE: La central opera bajo la modalidad de abasto aislado sin inyección de energía eléctrica al Sistema Eléctrico Nacional.
No tiene interés en continuar con el proyecto', NULL, NULL, N'Sin conflictos identificados. ', N'La propia empresa declaró que no tiene interés en continuar; no conviene darle seguimiento activo salvo que la empresa reactive formalmente el proyecto.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Hidroeléctrica Las Juntas')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, E
stadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Hidroeléctrica Las Juntas', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Energía Verde Meyer'), N'Energía Verde Meyer, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'HID'), 2.91, 1, N'Jalisco', N'Cabo Corrientes', -105.603295859373, 20.3859314652783, N'E/1502/AUT/2015', N'0', NULL, NULL, NULL, N'0', N'0', N'Juicio contra PROFEPA', 0, NULL, N'La empresa tiene juicio vigente contra la PROFEPA por inconformidad de requerimientos de la autoridad ambiental.', NULL, NULL, N'Se requiere ubicación para ', N'La propia base señala que no tiene interés en continuar.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Incremento Generación Almex')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Incremento Generación Almex', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Almex'), N'No se identifica el proyecto', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'TG/COG'), 28, 1, N'Jalisco', N'Guadalajara', -103.349386076715, 20.6758041188247, N'No se identifica el permiso', N'0', NULL, NULL, NULL, N'0', N'0', N'No se identifica el permiso', 1, NULL, N'MISSE: No ingresada
CNE: No se identifica el permiso', NULL, NULL, N'Se requiere ubicación para ', N'Hay interés, pero no se identifica permiso ni proyecto; requiere diagnóstico mínimo antes de definir si va por ventanilla, migración o regularización.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Palma Loca')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Palma Loca', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Enel Green Power'), N'Desarrollo de Fuerzas Renovables, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 399.75, 1, N'Zacatecas', N'Mazapil', -101.553121078462, 24.6380047399718, N'E/2214/GEN/2019', N'1057-16', '2025-02-12', '2027-02-13', '2027-02-14', N'Sin iniciar construcción.', N'Sin solicitud de modificación pendiente de atención.', N'El 8 de agosto del 2024 se le realizó un oficio de requerimiento relaitvo al avance de obras, sin embargo la permisionaria manifestó que todavía no se han terminado las fechas del programa de obras.
Adicionalmente la permisionaria manifesto que se encuentra en la etapa de actividades previas', 0, NULL, N'[2026-05-07] 399.8 MW. Enel plantea posibilidad de reducir capacidad a 50 MW para hacerlo más viable.

— Anterior —
La empresa no cuenta con posesión de los terrenos para el desarrollo de los proyectos. No tienen contratos ni trámites vigentes. No hay interés de la empresa por continuar con el desarrollo.', NULL, N'[2026-05-07] Decisión sobre reducción a 50 MW.', N'Sin conflictos identificados. ', N'No cuenta con terrenos, contratos ni trámites vigentes y la empresa no tiene interés; debe archivarse del seguimiento activo.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Eólico Parras')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Parque Eólico Parras', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Compañía Minera Autlán'), N'Energía y Proyectos Eólicos, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 50, 1, N'Coahuila de Zaragoza', N'Parras', -102.17526263624, 25.4498532465548, N'E/998/AUT/2013', N'604-17 (término)', '2012-06-01', N'-', '2021-12-31', N'Permiso terminado.', N'Sin solicitud de modificación pendiente de atención.', N'Permiso terminado el 11/03/2026 por renuncia de la Permisionaria', 0, NULL, N'La empresa no tiene interés en continuar con el desarrollo del proyecto.', NULL, NULL, N'Sin conflictos identificados. ', N'El permiso terminó por renuncia de la permisionaria y la empresa no tiene interés en continuar; no hay ruta activa de seguimiento.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Eólico San Carlos')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Parque Eólico San Carlos', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Acciona'), N'Parques Eólicos de San Lázaro, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 198, 1, N'Tamaulipas', N'Villagrán', -99.4907422725443, 24.4704036667956, N'CNE/E/2002/GEN/2017', N'2489-17', N'-', N'-', '2027-09-30', N'La central ya se encuentra totalmente construida y su fecha en operación comercial al ámparo de la Ley del Sector Eléctrico será el 30 de septiembre de 2027.', N'Sin solicitud de modificación pendiente de atención.', N'Mediante la resolución RES/1905/2022 la CRE revocó el permiso E/2002/GEN/2017.

Mediante la resolución CNE/RES/236/2026 la CNE resolvió el juicio de amparo indirecto 27/2023, por lo que dejó insubsistente la resolución número RES/1905/2022 y se conmutó la revocación por una multa y solicitar la migración a la LSE.

Mediante la resolución CNE/RES/366/2026 de 15abr26 se migró el permiso a la LSE.', 1, NULL, N'[2026-05-07] Parque eólico CONSTRUIDO AL 100% pero sin operación comercial. Permiso migrado a esquema LSE. Acciona revisa adenda al CI; espera que CENACE indique qué estudios solicitar para determinar obras complementarias y revalidación de estudios (criterio DEF). Pendiente actualización de MIA y pago de garantías. CENACE: no existe solicitud formal en SIASIC con características actuales del proyecto.

— Anterior —
Central 100% Construida, en proceso de Interconexión. Pertenece a emproblemadas.

CENACE: La empresa recibirá el Estudio de Instalaciones en el que se le indicará el monto de las Obras de Refuerzo y monto de la Garantía.
SEMARNAT: Una vez definidas las Obras de Refuerzo, la empresa ingresará la modificación a la MIA. 
CFE: Una vez aceptadas las Obras de Refuerzo y realizado el pago de la Garantía, la empresa firmará Contrato de Interconexión.', N'Seguimiento Ernesto', N'[2026-05-07] Actualizar MIA, pagar garantías, presentar solicitud formal en SIASIC.

— Anterior —
Ver si es que tienen el diagnóstico y cumplirían con los 6 meses', N'Sin conflictos identificados. ', N'Está en seguimiento Ernesto/DG; aunque ya migró a LSE y está en interconexión, no debe gestionarse desde el equipo operativo salvo instrucción expresa.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Solar Villanueva MP')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Parque Solar Villanueva MP', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Enel Green Power'), N'Más Energía, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 150, 1, N'Coahuila de Zaragoza', N'Viesca', -102.808016342342, 25.345219867907, N'E/2217/GEN/2019', N'1867-17', '2025-02-18', '2027-02-18', '2027-02-19', N'Sin iniciar construcción.', N'0', N'El programa de obras indica como fecha de terminación de obras el 18 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.', 0, NULL, N'[2026-05-07] 150 MW. No reporta avance constructivo según CENACE. Solo 2 de los 5 proyectos Enel mantienen permisos.

— Anterior —
La empresa no cuenta con posesión de los terrenos para el desarrollo de los proyectos. No tienen contratos ni trámites vigentes. No hay interés de la empresa por continuar con el desarrollo.', NULL, N'[2026-05-07] Pendiente confirmación de Enel sobre ejecución.', N' Protestas de trabajadores por adeudos y falta de cumplimiento de las empresas que marcaron el precedente de desconfianza hacia los megaproyectos solares en Coahuila. ', N'La empresa no cuenta con posesión de terrenos, contratos ni trámites vigentes y además no tiene interés; debe salir del seguimiento activo.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'PS Aguascalientes Sur I')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'PS Aguascalientes Sur I', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'OPDEnergy'), N'Infraestructura Energética del Norte, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 2.97, 1, N'Aguascalientes', N'Aguascalientes', -102.290238502403, 21.887503195996, N'E/2128/GEN/2018', N'1213-16', '2018-06-08', '2020-06-02', '2020-06-03', N'La central ya se encuentra en operación desde el 28 de abril de 2020', N'0', N'0', 1, NULL, N'[2026-05-07] Construcción arrancó el año pasado. En etapa de puesta en servicio previa a pruebas operativas, pero sin respuesta al diagnóstico de equipos, lo que impide formalizar inicio de pruebas. OPDEnergy solicitó al CENACE modificación de fecha del CI y recibió señales positivas.

— Anterior —
Central 100% Construida, en proceso de Interconexión. 

CFE: Se encuentran trabajos para la instalación de un medidor fiscal requerido por CFE Transmisión. El miércoles 13 de mayo CFE realizó una visita a sitio para indicarle a la empresa las características técnicas de los componentes', N'Puede continuar con LIE', N'[2026-05-07] Realizar reunión de sitio para destrabar diagnóstico de equipos y formalizar ajuste de fecha en CI.

— Anterior —
Preguntar si ya solicitaron el registro de activos formalmente por CFE.
Seguimiento al ingreso de solicitud para la modificación de FEOC. Preguntar si gustan Migrar y si requerirán Almacenamiento para iniciar estudios de almacenamiento para SAE. En el proceso se incluyó la migración con SAE.', N'Sin conflictos identificados. ', N'Puede continuar bajo LIE; lo urgente no es migración, sino cerrar medidor fiscal, registro de activos y modificación de FEOC antes de definir SAE/migración.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Media'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Rancho del Norte')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Rancho del Norte', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Ternium'), N'Arroyo Viento del Norte, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 250, 1, N'Nuevo León', N'General Bravo', -99.1789920552509, 25.7954730877495, N'E/2107/GEN/2018', N'670-18', '2020-04-01', '2027-07-14', '2027-07-15', N'Sin iniciar construcción.', N'Sin solicitud de modificación pendiente de atención.', N'El programa de obras indica como fecha de terminación de obras el 14 de julio de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.', 1, NULL, N'[2026-05-20] Mesa eléctrica revisó Ternium - Arroyo Viento del Norte. Se advirtió que exigir nuevo permiso, nueva MISSE y baterías podría convertir el proyecto original en uno nuevo y desvirtuar la activación rápida. CENACE propuso modelo San Carlos: pago de tarifa de flexibilidad en lugar de construir batería. La vía más ágil acordada es enlazar estudios técnicos históricos al nuevo permiso, mantener configuración técnica idéntica y limitar cambios a actualización de garantías financieras. No requeriría nueva MIA si se mantiene el permiso; MISSE sí debe tramitarse desde cero por restricciones legales, con canal ágil aprovechando antecedentes.', N'Ruta simplificada: estudios históricos + nuevo permiso + tarifa de flexibilidad', N'[2026-05-20] CENACE enviará a DGEASP listado de obras de refuerzo originales para asegurar diseño sin cambios. DGEASP planteará amarrar estudios históricos al nuevo permiso y exentar batería vía tarifa de flexibilidad (modelo San Carlos). Definir internamente ruta crítica simplificada y agendar reunión con directivos de Ternium.', N'Sin conflictos identificados. ', N'Se ajusta la ruta para no desvirtuar el proyecto: mantener configuración técnica, amarrar estudios históricos al nuevo permiso, evitar batería física mediante tarifa de flexibilidad y tramitar MISSE ágil.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Mesas especiales DG'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-20', N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'RMSC Comercio')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'RMSC Comercio', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'RMCS Comercio, S. A. P. I. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Chihuahua', N'Ascensión', -107.99780584764, 31.0897564231269, N'E/1510/GEN/2015', N'116-24 (Desistida)
2540-24 
2541-24', '2023-06-05', '2027-02-24', '2027-02-24', N'Sin iniciar construcción.', N'Solictud de migración desechada', N'La solicitud de migración fue desechada el 27abr2026 por no atender el oficio de prevención.

El programa de obras indica como fecha de terminación de obras el 24 de febrero de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.', 0, NULL, N'[2026-05-20] SIMSA propone consolidar RMSC Comercio, Altair, Alaia II/III/IV/V en un Proyecto Agrupado de 180 MW en SE Paso del Norte (Cd. Juárez, Chihuahua), reubicando a 9 km de la SE. CNE indica que como nuevo proyecto, puede entrar a convocatoria Mixtos. Han hecho 10 trámites (Alaia IV y V con resolutivo) pero el cambio de ubicación obliga a presentar solicitud única. Estudio CENACE SICE-01136-2023. Reingeniería con SAEE (baterías) para flexibilidad. Participó en Mixtos pero NO pasó a 2a ronda por vertimiento en GCR Norte. CNE sugiere desistir de los permisos individuales para evitar pagos de supervisión/mantenimiento.

[2026-05-07] RMCS (30 MW) requiere actualizar permisos por FEOC vencida.

— Anterior —
Proyectos agrupados que ya solicitaron migración de LIE a LSE, contaban con todas las autorizaciones y liberaciones. Sin embargo, la empresa modificó la ubicación del proyecto para reducir la Línea de Transmisión de 45 a 9 km y se adicionaron baterías.
Dicho cambio requiere la actualización de los estudios ambientales, sociales, liberaciones del INAH; así como la revalidación de estudios por parte de CENACE. 
CNE: La ruta crítica es definir si procede migración a LSE, nuevo permiso, modificación o revalidación; se señaló que debe acreditarse avance de obras.
CENACE: Requiere revalidar estudios y ajustar la interconexión por cambio de ubicación o configuración del proyecto. 
SEMARNAT: Deben actualizarse MIA, MISSE/EvIS, ETJ y permisos asociados por cambios de ubicación o alcance. 
INAH / licencias / CONAGUA: La empresa reportó avances previos, pero varios trámites tendrían que actualizarse o reingresarse.
La empresa mantiene interés en continuar los proyectos y busca consolidarlos para lograr economías de escala, reducir la longitud de la línea de transmisión y preservar derechos de interconexión.
UEVISPI compartió a CNE y DGISCPOS el KMZ de la nueva ubicación para analizar si los trámites se tratan de una modificación o un nuevo ingreso', NULL, N'[2026-05-20] Inscribir el Proyecto Agrupado en la convocatoria de Mixtos (siguiente ventana). Evaluar desistimiento de permisos individuales con CNE y CENACE. Revisar garantías con CENACE.

[2026-05-07] Renovar permisos por FEOC vencida y completar trámites.', N'Sin conflictos identificados. ', N'La solicitud de migración fue desechada; solo tendría sentido reabrirlo si hay reingreso formal.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Sears Puebla Zaragoza ')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Sears Puebla Zaragoza ', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Grupo Carso'), N'CE G. Sanborns 2, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'CI/COG'), 0.999, 1, N'Puebla', N'Puebla', -98.2048886246498, 19.0416518255674, N'E/2045/GEN/2018', N'0', '2018-03-01', '2018-10-15', '2019-05-30', N'La central ya se encuentra en operación desde el 30 de mayo de 2019.', N'Sin solicitud de modificación pendiente de atención.', N'0', 1, NULL, N'MISSE: No ingresada
CNE: En Operación, sin solicitudes activas.
Central 100% construida, con contrato vencido en diciembre 2025. Están interesados en la migración de la Central.', N'Ventanilla Única de Autoconsumos', N'Citar para ver si les interesa y entren en la Convocatoria. Conocer si ya cuentan con el Anexo IV.', N'Se requiere ubicación', N'Central construida con contrato vencido e interés en regularizarse; corresponde revisar Anexo IV y definir entrada por autoconsumo/convocatoria antes de otra ruta.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Autoconsumo'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Media'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Tizimin II')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Tizimin II', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Smartener'), N'Fuerza y Energía Limpia de Tizimín II, S. A. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 75.6, 1, N'Yucatán', N'Tizimín', -88.1493253717069, 21.1458807064421, N'E/2108/GEN/2018', N'0', '2023-07-01', '2027-06-28', '2027-06-29', N'Sin iniciar construcción.', N'Sin solicitud de modificación pendiente de atención.', N'El programa de obras indica como fecha de terminación de obras el 28 de junio de 2027, por lo que la comisión realizará un oficio de requerimiento que muestre el avance de su programa de obras.', 1, NULL, N'[2026-05-07] Firmó contratos de infraestructura e interconexión (SE Tizimín 115 kV), pasó primer filtro CENACE, MIA/CUSF/MISE homologadas, CI vigente y garantías cubiertas hasta jun-2027. Obra civil en ejecución; CPTT-CFE supervisa. Transformador llegaría oct-2026. En pláticas con SC para venta de energía a largo plazo. Buenas relaciones con CNE y CENACE.

— Anterior —
Proyecto en Construcción (25% avance), pueden adelantar el inicio de operaciones. Están en la última fase del cierre financiero. Cuentan con Contrato de Obra Eléctrica, Aerogeneradores y transformador principal comprados, ya están llegando desde China, estiman contar con todo el equipo en octubre 2026. En charlas con CFE Cal para negociar el PPA.

SECRETARÍA DE ECONOMÍA: Durante el proceso de importación fueron notificados sobre deficiencias en el Certificado de Calidad de los Componentes de Jaula de Pernos, indispensables para la instalación de Aerogeneradores. El jueves 14 de mayo la Secretaría de Economía se comunicó con la empresa para dar asesoría sobre la solicitud de importación.', N'Migración hasta el Contrato de Interconexión + Almacenamiento', N'[2026-05-07] Energizar LT a finales de 2026 y pruebas operativas entre ene-feb 2027. Resolver tema de importación de aerogeneradores.

— Anterior —
Comunicarse con la empresa para comentar sobre la migración y siguientes pasos', N'Las comunidades mayas cercanas (Yohactún, vecino a Dzonot Carretero, entre otras) denunciaron que la consulta indígena ocurrió “en tiempo récord” y sin participación o transparencia suficientes, generando percepciones de injusticia en términos de distribución de beneficios y reconocimiento de derechos culturales. Se denuncia que la participación comunitaria fue limitada: solo participaron en comités, sin acceso a decisiones sobre beneficios ni procesos ambientales. 
El parque eólico se ubica a aproximadamente 2 km al sur de la Reserva de la Biósfera Ría Lagartos. No se identificaron amparos ni denuncias específicas contra la empresa. ', N'Tiene avance físico, equipos y cierre financiero en curso; la propuesta de atención indica migración hasta contrato de interconexión con almacenamiento.', (SELECT TOP 1 ClasificacionId F
ROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'34Py + PPT', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Yucatán Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Yucatán Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Jinkosolar Investment PTE. LTD'), N'Energía Solar Cuncunul, S. de R. L. de C. V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 70, 1, N'Yucatán', N'Valladolid', -88.2008170765377, 20.6899685773954, N'E/1843/GEN/2016', N'0', '2018-02-01', '2018-06-21', '2018-09-28', N'Permiso terminado.', N'Sin solicitud de modificación pendiente de atención.', N'Permiso terminado el 13sep18 por renuncia de la Permisionaria', 0, NULL, N'La empresa no tiene interés en continuar con el desarrollo del proyecto.', NULL, NULL, N'Se requiere ubicación', N'El permiso terminó por renuncia de la permisionaria y la empresa no tiene interés en continuar; no hay ruta de gestión.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Inviables'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Sin prioridad'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo 34Py', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Campeche Energy')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Campeche Energy', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Campeche Energy S.A.P.I. de C.V.'), N'Campeche Energy S.A.P.I. de C.V.', NULL, 80, NULL, N'Campeche', N'Champotón', NULL, NULL, N'CNE/E/39/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV La Alegría Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV La Alegría Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Sunstone Power 2, S. de R. L. de C. V.'), N'Sunstone Power 2, S. de R. L. de C. V.', NULL, 600, NULL, N'Campeche', N'Campeche', NULL, NULL, N'CNE/E/29/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV La Esperanza Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV La Esperanza Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Sunstone Power, S. de R. L. de C. V.'), N'Sunstone Power, S. de R. L. de C. V.', NULL, 300, NULL, N'Campeche', N'Escárcega', NULL, NULL, N'CNE/E/28/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Tecozautla')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Tecozautla', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Green Park Energy, S. A. de C. V.'), N'Green Park Energy, S. A. de C. V.', NULL, 100, NULL, N'Hidalgo', N'Tecozautla', NULL, NULL, N'CNE/E/20/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV El Toro')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV El Toro', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Green Park Energy, S. A. de C. V.'), N'Green Park Energy, S. A. de C. V.', NULL, 90, NULL, N'Guanajuato', N'Manuel Doblado', NULL, NULL, N'CNE/E/21/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'PFV Energías Renovables de Tamaulipas')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'PFV Energías Renovables de Tamaulipas', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Energías Renovables de Tamaulipas, S. A. P. I. de C. V.'), N'Energías Renovables de Tamaulipas, S. A. P. I. de C. V.', NULL, 78, NULL, N'Tamaulipas', N'Altamira', NULL, NULL, N'CNE/E/40/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Eólico el 24')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Parque Eólico el 24', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'EPM Eólica 24 S.A. de C.V.'), N'EPM Eólica 24 S.A. de C.V.', NULL, 130, NULL, N'Tamaulipas', N'Mier', NULL, NULL, N'CNE/E/19/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Alten Hidalgo 100')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Alten Hidalgo 100', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Alten Energías Renovables México Once, S.A. de C.V.'), N'Alten Energías Renovables México Once, S.A. de C.V.', NULL, 100, NULL, N'Hidalgo', N'Nopala de Villagrán', NULL, NULL, N'CNE/E/22/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Global Solar 2')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Global Solar 2', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Global Solar América 2, S.A.P.I. de C.V.'), N'Global Solar América 2, S.A.P.I. de C.V.', NULL, 92, NULL, N'Hidalgo', N'Nopala de Villagrán', NULL, NULL, N'CNE/E/23/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Proyecto Solar Piedras Negras')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Proyecto Solar Piedras Negras', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Martil Solar, S.A. de C. V.'), N'Martil Solar, S.A. de C. V.', NULL, 120, NULL, N'Veracruz', N'Tlalixcoyan', NULL, NULL, N'CNE/E/26/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV Energía Solar Herrera')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV Energía Solar Herrera', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Energía Solar Herrera, S.A de C. V.'), N'Energía Solar Herrera, S.A de C. V.', NULL, 200, NULL, N'Puebla', N'Tecali de Herrera', NULL, NULL, N'CNE/E/27/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'EO Vientos del Caribe Hybrid')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'EO Vientos del Caribe Hybrid', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Eólica del Rocío S.A. de C.V.'), N'Eólica del Rocío S.A. de C.V.', NULL, 200, NULL, N'Quintana Roo', N'Othón P. Blanco', NULL, NULL, N'CNE/E/30/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'EO Zapoteca de Energía')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'EO Zapoteca de Energía', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Zapoteca de Energía, S.A.P.I. de C.V.'), N'Zapoteca de Energía, S.A.P.I. de C.V.', NULL, 200, NULL, N'Oaxaca', N'Cuauhtémoc', NULL, NULL, N'CNE/E/25/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'EO PP. EE. Panabá 1')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'EO PP. EE. Panabá 1', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Vientos de Panabá S.A. de C.V.'), N'Vientos de Panabá S.A. de C.V.', NULL, 250, NULL, N'Yucatán', N'Panabá', NULL, NULL, N'CNE/E/31/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Eólica Dzilam')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Eólica Dzilam', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'VIVE ENERGIA'), N'Eólica Dzilam, S.A.P.I. de C.V.', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'EO'), 119, NULL, N'Yucatán', N'Dzilám', NULL, NULL, N'CNE/E/32/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'[2026-04-21] Reunión SENER - VIVE ENERGIA DZILAM. Proyecto eólico en Yucatán de capital mexicano asociado con empresa china. MISE en proceso de consulta con cierre al día siguiente; INAH en etapa prospectiva trabajando en campo. MIA ya ingresada, pero recibió prevención en febrero y debe contestarse en mayo. Foco rojo: la línea de transmisión del proyecto resultó incompatible con el programa de ordenamiento territorial y ecológico estatal; plan B sería reubicar o mover el tramo, sujeto a análisis de conveniencia. Obras de refuerzo incluyen línea de transmisión de 28 km también en etapa prospectiva. En RAN solo se han ingresado 15-18 contratos de 66 por problemas operativos tras cambio de domicilio de oficinas.', N'Seguimiento interinstitucional / resolver MIA y línea de transmisión', N'[2026-04-21] Dar seguimiento puntual e interinstitucional. Solicitar y revisar información ingresada para la MIA. Revisar con Antonino el problema de incompatibilidad de la línea de transmisión. Solicitar listado de contratos atorados en RAN. Convocar reunión con la Secretaria. Dar seguimiento al Estudio Técnico Justificativo, monto de inversión social e informe de financiamiento que debe presentarse a CNE en junio.', NULL, N'La MIA tiene prevención pendiente y la línea de transmisión presenta incompatibilidad territorial/ecológica; además hay contratos RAN detenidos y trámites sociales/INAH en proceso.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'PPT + Minuta', '2026-04-21', N'20260421_ Minuta DZILAM.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'FV PV Tamesí Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'FV PV Tamesí Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'PV Tamesí Solar SG, S. de R. L. de C. V.'), N'PV Tamesí Solar SG, S. de R. L. de C. V.', NULL, 110, NULL, N'Tamaulipas', N'Altamira', NULL, NULL, N'CNE/E/18/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Fotovoltaica CGS')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Central Fotovoltaica CGS', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Solarmex I, S.A.P.I. de C.V.'), N'Solarmex I, S.A.P.I. de C.V.', NULL, 89, NULL, N'Zacatecas', N'Ojo Caliente', NULL, NULL, N'CNE/E/24/GEN/2025', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Planta Fotovoltaica Saturno Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Planta Fotovoltaica Saturno Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Saturno Solar S.A. de C.V.'), N'Saturno Solar S.A. de C.V.', NULL, 163, NULL, N'Hidalgo', N'Singuilucan, Epazoyucan y Zempoala', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT (DGIRA)', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Planta Fotovoltaica Akuwa Solar S.A. de C.V')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Planta Fotovoltaica Akuwa Solar S.A. de C.V', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Akuwa Solar S.A. de C.V.'), N'Akuwa Solar S.A. de C.V.', NULL, 125, NULL, N'Hidalgo', N'Singuilucan y Epazoyucan', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT (DGIRA)', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Planta Fotovoltaica Delfín Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Planta Fotovoltaica Delfín Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Delfín Solar S.A. de C.V.'), N'Delfín Solar S.A. de C.V.', NULL, 358, NULL, N'Hidalgo', N'Singuilucan y Zempoala', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Solo PPT (DGIRA)', NULL, NULL, N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'PFV Tecali - La Magdalena')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'PFV Tecali - La Magdalena', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Energía Solar Herrera, S.A. de C.V.'), N'Energía Solar Herrera, S.A. de C.V.', NULL, 200, NULL, N'Puebla', N'Tecali de Herrera', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'[2026-05-07] Enel Green Power presentó 5 proyectos (~1,400 MW) y reconoció que necesita evaluar si tienen sentido comercial tras años de demora. Han renovado garantías anualmente sin avanzar trámites.', NULL, N'[2026-05-07] Enel confirmará decisión sobre ejecución de los 5 proyectos.', NULL, NULL, NULL, NULL, NULL, N'Solo PPT (DGIRA)', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Sol de los Manzanos')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Sol de los Manzanos', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'SIMSA'), N'Vehículos del Grupo SIMSA', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, NULL, N'Chihuahua', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'[2026-05-07] Parte del cluster SIMSA. Tiene INAH y MIA pero la licencia de construcción ya venció.', NULL, N'[2026-05-07] Renovar licencia de construcción y actualizar trámites.', NULL, NULL, NULL, NULL, NULL, N'Solo Minuta 07/05/2026', '2026-05-07', N'Nota Técnica Proyectos Firmes 07/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Energía Villa de Arriaga')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, Tra
miteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Energía Villa de Arriaga', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Energía Villa de Arriaga'), NULL, NULL, NULL, NULL, N'San Luis Potosí', N'Villa de Arriaga', NULL, NULL, N'E-1339-GEN-2015', N'SICE-00186-2018', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'[2026-05-20] Empresa interesada en participar en convocatoria de proyectos Mixtos. Requiere actualizar estudios (incremento de capacidad + almacenamiento) y modificar fecha de entrada en operación. Se ubica en mismo predio que la Central Eléctrica. Permiso de generación E-1339-GEN-2015, estudio CENACE SICE-00186-2018.', NULL, N'[2026-05-20] Decidir en próximos días si participa en convocatoria Mixtos (requiere desistimiento del permiso actual) o continúa por cuenta propia.', NULL, NULL, NULL, NULL, NULL, N'Solo Minuta 20/05/2026', '2026-05-20', N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Aura Solar')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Aura Solar', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Gauss Energy'), N'Gauss Energy', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'FV'), 30, 1, N'Baja California Sur', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Inicio obra civil programado para julio 2026; pruebas reducidas en agosto 2026.', N'Migración de permisos a LSE / estudios de interconexión y DEF', N'Pequeña Producción; venta exclusivamente a CFE. DEF solo por 20 MW; CFE ICL como representante legal deberá solicitar registro de activos.', 1, N'Sí - SAEE 20 MW / 3 horas', N'[2026-05-20] Aura Solar (Gauss Energy), FV 30 MW en Baja California Sur, con SAEE ajustado de 30 MW/2h a 20 MW/3h. CFE indicó esquema de Pequeña Producción, por lo que debe migrar permisos a LSE y mantener venta exclusivamente a CFE. CENACE resaltó que durante migración no debe dejar de operar; DEF solo por 20 MW y el representante legal (CFE ICL) deberá solicitar registro de activos. CNE analizará si la integración de almacenamiento requiere MIA/MISSE; DGISCPOS indicó que debe presentarse solicitud ante SENER aunque se trate de zona impactada.', N'Migración LSE + SAEE', N'[2026-05-20] DGEASP compartirá formalmente con DGISCPOS la consulta de confirmación de criterio para coordinar análisis del expediente técnico y anexos. CENACE espera escrito del particular explicando arquitectura y operación del SAEE; compartirlo con DGISCPOS. Dar seguimiento a migración LSE, DEF 20 MW y registro de activos.', NULL, N'Migración LSE con almacenamiento e interconexión en BCS; requiere definir MIA/MISSE, DEF, continuidad operativa y registro de activos.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo Minuta 20/05/2026', '2026-05-20', N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Hidroeléctrica Mizú')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Hidroeléctrica Mizú', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Hidroeléctrica Mizú'), N'(solicitante no identificado)', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'Hidro'), 6.5, 1, N'Veracruz', N'Ixtaczoquitlán / Ciudad Mendoza', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Central existente en actividades; 1.2 MW rehabilitados. Expansión planteada a 6.0 MW en CH Ixtaczoquitlán + 0.5 MW Ciudad Mendoza.', N'Sin permiso / requiere estudios CENACE y permiso de generación', N'Opera físicamente sin permiso según minuta; ficha indica interconexión viable en media tensión CFE Subestación Orizaba y necesidad de estudios ante CENACE.', NULL, NULL, N'[Ficha técnica] Central Hidroeléctrica Mizu ubicada sobre el río Manzinga, afluente del Río Blanco, dentro del Parque Nacional Cañón del Río Blanco en Ixtaczoquitlán, Veracruz. Central existente: un turbogenerador fue rehabilitado/modernizado a 1.2 MW. Se interconecta en media tensión de CFE en la Subestación Orizaba, zona Orizaba. A ~15 km se ubica la Central Hidroeléctrica Ciudad Mendoza con 0.5 MW; capacidad actual total 1.7 MW. El solicitante plantea expansión por etapas en CH Ixtaczoquitlán: nueva unidad de 3 MW y posterior sustitución de la unidad de 1.2 MW por otra de 3 MW, para 6.0 MW en Ixtaczoquitlán y 6.5 MW total incluyendo Ciudad Mendoza. Generación anual estimada: 33.21 GWh. No se observa problemática de interconexión en el punto que definan CENACE/CFE.', N'Regularización + estudios CENACE + permiso de generación', N'[Ficha técnica] Definir si atenderá cargas locales y enviará excedentes al SIN. Definir fechas de entrada en operación comercial por etapa. Realizar solicitud de estudios ante CENACE. Iniciar tramitología ambiental, social y de permiso de generación. Considerar que CH Ixtaczoquitlán pretende incorporarse al SIN con 6 MW en media tensión; Planta Ciudad Mendoza 0.5 MW tendría figura de Generador Exento y contrato con CFE Suministro Básico.', NULL, N'Proyecto hidroeléctrico existente en zona industrial con expansión prevista a 6.5 MW total; interconexión no presenta problemática aparente, pero requiere definir modelo de carga/excedentes, FEOC por etapas, estudios CENACE, ambientales/sociales y permiso de generación.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Ruta crítica'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Minuta + Ficha técnica', '2026-05-20', N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx', N'MigracionExcel');
END
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Proman (Planta de Fertilizantes)')
BEGIN
    INSERT INTO dgmesnie.Proyecto (Nombre, EmpresaId, Promovente, TecnologiaId, CapacidadMW, Renovable, EntidadFederativa, Municipio, Latitud, Longitud, NumeroPermiso, FolioEvIS_MISSE, FechaInicioObras, FechaTerminacionObras, FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE, InteresadaEnContinuar, RequiereAlmacenamiento, ResumenCaso, PropuestaAtencion, SiguientesPasos, RiesgosObservaciones, RazonesBreves, ClasificacionId, PrioridadId, SemaforoId, FuenteActual, FechaUltimaActualizacion, FuenteUltimaActualizacion, CreadoPor)
    VALUES (N'Proman (Planta de Fertilizantes)', (SELECT TOP 1 EmpresaId FROM dgmesnie.Empresa WHERE Nombre = N'Proman'), N'Proman', (SELECT TOP 1 TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = N'COG'), N'18 / 24', 0, N'Sinaloa', N'Topolobampo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Entrada en operación objetivo 2027; centro de carga aún no puede tratarse como existente.', N'Migrar permiso 18 MW a LSE y modificar a 24 MW / alternativa reiniciar proceso unificado', N'Debe mantener garantía anterior y generar nueva garantía para estudio completo de cogeneración.', NULL, NULL, N'[2026-05-20] Proman (Planta de Fertilizantes) es proyecto de cogeneración en Topolobampo: 18 MW LIE con incremento previsto a 24 MW por diseño/cambio tecnológico. CENACE precisó que el centro de carga debe terminar construcción, pasar puesta en servicio con carga mínima y ser declarado en operación comercial antes de tratarse como carga existente; alternativa: dar de baja trámite actual e iniciar proceso unificado de centro de carga, generación y cogeneración bajo autoconsumo LSE. CNE aclaró secuencia: migrar permiso 18 MW a LSE y luego modificar a 24 MW.', N'Autoconsumo LSE / cogeneración', N'[2026-05-20] Reunirse con Proman para presentar dos rutas: continuar tras operación comercial como carga existente o reiniciar proceso unificado bajo LSE. Informar que obras de refuerzo probablemente se conservan, sujeto a nuevos estudios y prelación. Mantener garantía actual y generar nueva garantía para estudio completo de cogeneración; CENACE ajustará/devolverá la primera garantía cuando el centro de carga entre en operación comercial.', NULL, N'Cogeneración para autoconsumo industrial; requiere definir ruta regulatoria, conservar obras de refuerzo si procede y administrar garantías.', (SELECT TOP 1 ClasificacionId FROM dgmesnie.CatClasificacion WHERE Nombre = N'Autoconsumo'), (SELECT TOP 1 PrioridadId FROM dgmesnie.CatPrioridad WHERE Nombre = N'Alta'), (SELECT TOP 1 SemaforoId FROM dgmesnie.CatSemaforo WHERE Nombre = N'Punto'), N'Solo Minuta 20/05/2026', '2026-05-20', N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026', N'MigracionExcel');
END
GO

-- 5) Trámites ambientales/sociales por proyecto
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'El promovente No ha presentado a SEMARNAT', N'En reunión del 20 de mayo: proyectos serían integrados en uno e inscritos en Segunda Convocatoria de Mixtos.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Alaia II';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT', NULL, N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Alaia IV';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Amistad IV';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Amistad IV';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'No requiere autorización de MIA por ser menor a 3 MW', N'Central 100% Construida. Pendiente definición de LT.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Fotovoltaica Flex Guadalajara';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', NULL, N'Central 100% Construida. Pendiente definición de LT.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Fotovoltaica Flex Guadalajara';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central La Victoria';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central La Victoria';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP SGPA/DGIRA/DG.01579', N'Autorizada', N'SEMARNAT notificó desde octubre 2025 la autorización de la modificación.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Pachamama II';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA/ETJ LT DFP/SGPARN/2195/2020', N'Autorizada', N'SEMARNAT notificó desde octubre 2025 la autorización de la modificación.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Pachamama II';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central opera en modo isla. Busca la interconexión.
La empresa espera los estudios de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Cogeneración Abasto Aislado - Malta Texco Texcoco';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La Central opera en modo isla. Busca la interconexión.
La empresa espera los estudios de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Cogeneración Abasto Aislado - Malta Texco Texcoco';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Cogeneración Industrial Papelera San Luis';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Cogeneración Industrial Papelera San Luis';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'No requiere MIA por ser menor a 3MW', N'Central 100% Construida. Pendiente definición de LT.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Fotovoltaico Flex';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', NULL, N'Central 100% Construida. Pendiente definición de LT.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Fotovoltaico Flex';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 28TM2019E0057', N'Autorizada', N'La Central opera en modo isla. La empresa espera los estudios de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Parque Eólico San Carlos';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'28TM2019E0099 MIA LT', N'El promovente No ha presentado a SEMARNAT', N'La Central opera en modo isla. La empresa espera los estudios de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Parque Eólico San Carlos';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central está 100% Construída en proceso de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'PS Aguascalientes Sur I';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA/ETJ LT', N'No aplica', N'La Central está 100% Construída en proceso de interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'PS Aguascalientes Sur I';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'09/MC-0084/0220 DTU PP', N'Autorizada', N'[2026-05-20] No se requeriría nueva MIA si el permiso se mantiene. MISSE sí tendría que tramitarse desde cero por restricciones legales del nuevo permiso, aunque mediante canal ágil aprovechando antecedentes.

— Anterior —
Se metió el trámite como DTU. SEMARNAT notifica para pago al Fondo Forestal.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Rancho del Norte';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'09/MC-033/0120 MIA LT / ETJ LT', N'Autorizada', N'[2026-05-20] No se requeriría nueva MIA si el permiso se mantiene. MISSE sí tendría que tramitarse desde cero por restricciones legales del nuevo permiso, aunque mediante canal ágil aprovechando antecedentes.

— Anterior —
Se metió el trámite como DTU. SEMARNAT notifica para pago al Fondo Forestal.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Rancho del Norte';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Sears Puebla Zaragoza ';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La Central opera en modo isla. Busca la interconexión.', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Sears Puebla Zaragoza ';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'Autorizada', N'-', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Tizimin II';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT / ETJ LT', N'Autorizada', N'-', N'SEMARNAT/SENER', N'34Py + PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Tizimin II';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 04CA2024E0013', N'Autorizada', N'Sin embargo, la construcción puede iniciar donde no requiere cambio de uso del suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Campeche Energy';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP', N'El promovente No ha presentado a SEMARNAT el cambio de uso del suelo', N'Sin embargo, la construcción puede iniciar donde no requiere cambio de uso del suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Campeche Energy';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 04CA2025E0024', N'Autorizada', N'A partir del 26 de mayo puede notificarse.
El promovente ya cuenta con el Resolutivo autorizado.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Alegría Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'DTU LT 09/MC-0114/01/26', N'Autorizado', N'A partir del 26 de mayo puede notificarse.
El promovente ya cuenta con el Resolutivo autorizado.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Alegría Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'DTU PP 09/MC-0268/05/25', N'Autorizada', N'El 25 de mayo se notifica el cumplimiento de condicionante.
El 26 de mayo se notifica el cumplimiento de condicionante.
Ya se notificó el Resolutivo autorizado.
El 27 de mayo se notificaría la Resolución autorizada, si la empresa paga al Fondo Forestal el cambio de uso de suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Esperanza Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT 09/MG-0365/11/25', N'Autorizado', N'El 25 de mayo se notifica el cumplimiento de condicionante.
El 26 de mayo se notifica el cumplimiento de condicionante.
Ya se notificó el Resolutivo autorizado.
El 27 de mayo se notificaría la Resolución autorizada, si la empresa paga al Fondo Forestal el cambio de uso de suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Esperanza Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'CUSF ETJ 1 04/DS-0065/12/25', N'Autorizado', N'El 25 de mayo se notifica el cumplimiento de condicionante.
El 26 de mayo se notifica el cumplimiento de condicionante.
Ya se notificó el Resolutivo autorizado.
El 27 de mayo se notificaría la Resolución autorizada, si la empresa paga al Fondo Forestal el cambio de uso de suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Esperanza Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'CUSF ETJ 2 04/DS-0025/01/26', N'Autorizado', N'El 25 de mayo se notifica el cumplimiento de condicionante.
El 26 de mayo se notifica el cumplimiento de condicionante.
Ya se notificó el Resolutivo autorizado.
El 27 de mayo se notificaría la Resolución autorizada, si la empresa paga al Fondo Forestal el cambio de uso de suelo.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV La Esperanza Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 13HI2025E0096', N'Autorizado', N'A partir del 26 de mayo el promovente puede notificarse.
Ingresó el 13 de marzo de 2026 con insuficiencias legales respecto a la propiedad del terreno', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Tecozautla';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP 13/DS-0139/03/26', N'En evaluación', N'A partir del 26 de mayo el promovente puede notificarse.
Ingresó el 13 de marzo de 2026 con insuficiencias legales respecto a la propiedad del terreno', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Tecozautla';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 11GU2025E0126', N'Autorizado', N'A partir del 26 de mayo puede notificarse.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV El Toro';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT', N'SEMARNAT NO CUENTA CON EL PROYECTO YA QUE EL PROMOVENTE NO LO HA PRESENTADO.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'PFV Energías Renovables de Tamaulipas';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 28TM2020E0058', N'Autorizada', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Parque Eólico el 24';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Parque Eólico el 24';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 13HI2020E0057', N'Autorizada', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Alten Hidalgo 100';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Alten Hidalgo 100';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 13HI2025E0081', N'Autorizada', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Global Solar 2';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Global Solar 2';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELEC
T p.ProyectoId, N'MIA PP 30VE2025E0034', N'Autorizada', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Proyecto Solar Piedras Negras';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP', N'El promovente No ha presentado a SEMARNAT el trámite', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Proyecto Solar Piedras Negras';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 21PU2025E0122', N'Autorizada', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Energía Solar Herrera';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'2 ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', NULL, N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV Energía Solar Herrera';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 23QR2021E0072', N'Autorizada', N'Se prevé entrega el 29 de mayo', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO Vientos del Caribe Hybrid';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT En elaboración', N'El promovente No ha presentado a SEMARNAT', N'Se prevé entrega el 29 de mayo', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO Vientos del Caribe Hybrid';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP', N'El promovente No ha presentado a SEMARNAT el trámite', N'Se prevé entrega el 15 de junio
La LT se encuentra en determinación por CFE.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO Zapoteca de Energía';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA LT', N'El promovente No ha presentado a SEMARNAT la MIA', N'Se prevé entrega el 15 de junio
La LT se encuentra en determinación por CFE.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO Zapoteca de Energía';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'Se prevé entrega el 15 de junio
La LT se encuentra en determinación por CFE.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO Zapoteca de Energía';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 31YU2026E0036', N'En evaluación', N'La promovente ingresó el proyecto a SEMARNAT el 27 de abril de 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO PP. EE. Panabá 1';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP', N'El promovente No ha presentado a SEMARNAT el trámite', N'La promovente ingresó el proyecto a SEMARNAT el 27 de abril de 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO PP. EE. Panabá 1';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'La promovente ingresó el proyecto a SEMARNAT el 27 de abril de 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'EO PP. EE. Panabá 1';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 31YU2025E0092', N'En evaluación', N'[2026-04-21] MIA ingresada; recibió prevención en febrero y debe contestarse en mayo. Foco rojo: línea de transmisión incompatible con programa de ordenamiento territorial y ecológico estatal; evaluar reubicación/movimiento del tramo.

— Anterior —
A partir del 26 de mayo puede notificarse; si realizó el pago al fondo forestal.', N'SEMARNAT/SENER', N'PPT + Minuta', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Eólica Dzilam';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP 31/DS-0052/04/26', N'En evaluación', N'[2026-04-21] MIA ingresada; recibió prevención en febrero y debe contestarse en mayo. Foco rojo: línea de transmisión incompatible con programa de ordenamiento territorial y ecológico estatal; evaluar reubicación/movimiento del tramo.

— Anterior —
A partir del 26 de mayo puede notificarse; si realizó el pago al fondo forestal.', N'SEMARNAT/SENER', N'PPT + Minuta', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Eólica Dzilam';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT Pendiente', N'El promovente No ha presentado a SEMARNAT el trámite', N'[2026-04-21] MIA ingresada; recibió prevención en febrero y debe contestarse en mayo. Foco rojo: línea de transmisión incompatible con programa de ordenamiento territorial y ecológico estatal; evaluar reubicación/movimiento del tramo.

— Anterior —
A partir del 26 de mayo puede notificarse; si realizó el pago al fondo forestal.', N'SEMARNAT/SENER', N'PPT + Minuta', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Eólica Dzilam';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 28TM2023E0057', N'Autorizada', N'Solarig trabaja en un nuevo estudio y trazo por servidumbre de paso.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV PV Tamesí Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP Pendiente', N'El promovente No ha presentado a SEMARNAT', N'Solarig trabaja en un nuevo estudio y trazo por servidumbre de paso.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV PV Tamesí Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA/ETJ LT', N'El promovente No ha presentado a SEMARNAT', N'Solarig trabaja en un nuevo estudio y trazo por servidumbre de paso.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'FV PV Tamesí Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA PP 32ZA2019E0060', N'Autorizada', N'A partir del 26 de mayo el promovente puede notificarse.
En diciembre de 2025 se solicitó un entronque para retirar una conexión existente de 13.5 km; reportan 98% de avance en terrenos. Estiman ingreso en julio 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Fotovoltaica CGS';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ PP', N'Autorizado', N'A partir del 26 de mayo el promovente puede notificarse.
En diciembre de 2025 se solicitó un entronque para retirar una conexión existente de 13.5 km; reportan 98% de avance en terrenos. Estiman ingreso en julio 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Fotovoltaica CGS';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'ETJ LT', N'El promovente No ha presentado a SEMARNAT el trámite', N'A partir del 26 de mayo el promovente puede notificarse.
En diciembre de 2025 se solicitó un entronque para retirar una conexión existente de 13.5 km; reportan 98% de avance en terrenos. Estiman ingreso en julio 2026.', N'SEMARNAT/SENER', N'Solo PPT', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Central Fotovoltaica CGS';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA-R 13HI2019E0019', N'AUTORIZADA', N'SGPA/DGIRA/DG/05338 de fecha 11/07/2019. Ampliación de vigencia mediante SRA/DGIRA/DG-03771-25 del 19/06/25. Se bajó de convocatoria.', N'SEMARNAT/SENER', N'Solo PPT (DGIRA)', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Planta Fotovoltaica Saturno Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA-R 13HI2020E0068', N'AUTORIZADA', N'SGPA/DGIRA/DG/07300 de fecha 16/09/2019. El 11/03/2025 se notificó el oficio SRA/DGIRA/DG-01243-25 con ampliación de vigencia. Se bajó de convocatoria.', N'SEMARNAT/SENER', N'Solo PPT (DGIRA)', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Planta Fotovoltaica Akuwa Solar S.A. de C.V';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA-R 13HI2020E0018', N'AUTORIZADA', N'SRA/DGIRA/DG-01633-26 del 20/02/26. Se bajó de convocatoria.', N'SEMARNAT/SENER', N'Solo PPT (DGIRA)', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Planta Fotovoltaica Delfín Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA-R 21PU2025E0122', N'AUTORIZADA', N'SRA/DGIRA/DG-03739-26 del 07/05/2026.', N'SEMARNAT/SENER', N'Solo PPT (DGIRA)', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'PFV Tecali - La Magdalena';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA/MISSE por integración de SAEE', N'Por confirmar / solicitud ante SENER requerida', N'CNE analizará si MIA y MISSE aplican por almacenamiento; DGISCPOS indicó que debe presentarse solicitud ante SENER.', N'SEMARNAT/SENER', N'Solo Minuta 20/05/2026', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Aura Solar';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'MIA/MISSE / estudios ambientales-sociales', N'Por iniciar / vigencia por confirmar', N'[Ficha técnica] Debe iniciar tramitología de estudios ambientales y sociales para permiso de generación. Ubicación dentro del Parque Nacional Cañón del Río Blanco puede requerir revisión ambiental cuidadosa.

— Anterior —
La empresa notificará si cuenta con MISSE vigente; si no, ingreso inmediato de MIA y MISSE.', N'SEMARNAT/SENER', N'Minuta + Ficha técnica', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Hidroeléctrica Mizú';
INSERT INTO dgmesnie.TramiteProyecto (ProyectoId, TipoTramite, EstatusTexto, Observaciones, Autoridad, Fuente, CreadoPor)
SELECT p.ProyectoId, N'Actualización MISSE', N'Pendiente / según ruta de modificación', N'No presentaría estudio de impacto durante migración; se presentaría al ingresar modificación. Actualización MISSE prevista.', N'SEMARNAT/SENER', N'Solo Minuta 20/05/2026', N'MigracionExcel'
FROM dgmesnie.Proyecto p WHERE p.Nombre = N'Proman (Planta de Fertilizantes)';
GO

-- Bitácora: Tizimin II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Tizimin II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'EN CONSTRUCCIÓN', N'Firmó contratos infra+CI (SE Tizimín 115 kV), pasó 1er filtro CENACE, MIA/CUSF/MISE homologadas, CI vigente, garantías a jun-2027. Obra civil en ejecución; CPTT-CFE supervisa.', N'Energizar LT fines 2026 y pruebas operativas ene-feb 2027. Resolver importación de aerogeneradores.', N'Demora en liberación de aerogeneradores afecta fecha de energización.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Tizimin II';
END
-- Bitácora: Chicxulub I
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Chicxulub I')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'DETENIDO', N'Más de 20 juicios maya Timul; 18 ganados; revisión pendiente del 174/20. Permiso vence 16-may-2026. CI vigente, FEOC vencida, garantías a dic-2026.', N'Solicitar prórroga del permiso por fuerza mayor.', N'ALTO RIESGO JURÍDICO. Permiso vence 16-may-2026.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Chicxulub I';
END
-- Bitácora: Chicxulub II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Chicxulub II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'DETENIDO', N'Elawan desistió del permiso original. CI vigente con FEOC vencida; garantías a dic-2026.', N'Resolver Chicxulub I primero; evaluar nueva ubicación.', N'Sin permiso vigente; proyecto detenido.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Chicxulub II';
END
-- Bitácora: PS Aguascalientes Sur I
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'PS Aguascalientes Sur I')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'EN PRUEBAS', N'En puesta en servicio previa a pruebas. Sin respuesta al diagnóstico de equipos. CENACE positivo a modificar fecha del CI.', N'Reunión de sitio para destrabar diagnóstico y formalizar ajuste de CI.', N'Incumplir CI = suspensión total.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'PS Aguascalientes Sur I';
END
-- Bitácora: Alaia II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Cluster Alaia II-V (120 MW) con misma inversión y mismo PI. Reubicación 45→9 km y baterías obligan a revalidar estudios y modificar MIA/MISE.', N'Revalidar estudios CENACE; modificar MIA/MISE; renovar licencias.', N'Migración completa de estudios antes de obra.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Alaia II';
END
-- Bitácora: Alaia III
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia III')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Cluster Alaia. Mismas condiciones que Alaia II.', N'Revalidar estudios; modificar MIA/MISE.', N'Migración completa de estudios.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Alaia III';
END
-- Bitácora: Alaia IV
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia IV')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Cluster Alaia. Mismas condiciones que Alaia II.', N'Revalidar estudios; modificar MIA/MISE.', N'Migración completa de estudios.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Alaia IV';
END
-- Bitácora: Alaia V
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia V')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Cluster Alaia. Mismas condiciones que Alaia II.', N'Revalidar estudios; modificar MIA/MISE.', N'Migración completa de estudios.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Alaia V';
END
-- Bitácora: Altair
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Altair')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Mismas condiciones que EVA. Sin CI ni avance.', N'Migrar estudios CENACE; actualizar MIA/MISE/licencias.', N'Sin CI ni avance.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Altair';
END
-- Bitácora: RMSC Comercio
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'RMSC Comercio')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Permisos por FEOC vencida.', N'Renovar permisos y completar trámites.', N'Permisos vencidos.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'RMSC Comercio';
END
-- Bitácora: PFV Tecali - La Magdalena
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'PFV Tecali - La Magdalena')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'POR DEFINIR', N'Enel tiene 5 proyectos (~1,400 MW); evalúa si tienen sentido comercial. Renuevan garantías sin avanzar.', N'Enel confirmará decisión de ejecución del portafolio.', N'Riesgo de abandono. Sin permisos de tierra.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'PFV Tecali - La Magdalena';
END
-- Bitácora: Palma Loca
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Palma Loca')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'POR DEFINIR', N'399.8 MW. Enel plantea reducir a 50 MW para viabilizar.', N'Decisión sobre reducción a 50 MW.', N'Sin permisos de tierra; trámites por reiniciar.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Palma Loca';
END
-- Bitácora: Ciénega de Mata (Central Jalisco)
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Ciénega de Mata (Central Jalisco)')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Sin avance constructivo según CENACE.', N'Pendiente confirmación de Enel.', N'Riesgo de abandono.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Ciénega de Mata (Central Jalisco)';
END
-- Bitácora: Parque Solar Villanueva MP
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Solar Villanueva MP')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'150 MW; sin avance. Solo 2 de los 5 Enel tienen permisos.', N'Pendiente confirmación de Enel.', N'Sin avance ni permisos.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Parque Solar Villanueva MP';
END
-- Bitácora: Amistad IV
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Amistad IV')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'EN ESTUDIOS', N'150 MW eólico; en evaluación comercial.', N'Continuar estudios y decisión global de Enel.', N'Sujeto a decisión global del portafolio Enel.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Amistad IV';
END
-- Bitácora: Ahumada II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Ahumada II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'POR DEFINIR', N'Único pendiente del cluster Ahumada. Infraestructura de conexión preparada. Bester explora hibridación con otros FV.', N'Bester evalúa hibridación.', N'Permisos sin vigencia; SENER pide replanteo.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Ahumada II';
END
-- Bitácora: Central Los Pinos
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Los Pinos')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'INVIABLE', N'Declarado INVIABLE por Aldesa.', N'—', N'PROYECTO INVIABLE.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Central Los Pinos';
END
-- Bitácora: Central Kabil I
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Kabil I')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'REVOCADO', N'Idéntico a Kabil II. Sin CI vigente; CENACE ''sin estudios vigentes''; CI en proceso de rescisión por CFE GRTP. Licencia construcción vencida.', N'Reestructuración financiera y nuevos
 estudios.', N'CI en proceso de rescisión.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Central Kabil I';
END
-- Bitácora: Central Kabil II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Kabil II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN ESTUDIOS', N'Idéntico a Kabil I. Permiso vigente CNE, CI en proceso de rescisión.', N'Reestructuración financiera y nuevos estudios.', N'CI en proceso de rescisión.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Central Kabil II';
END
-- Bitácora: Parque Eólico San Carlos
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Parque Eólico San Carlos')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'CONSTRUIDO', N'Parque al 100% sin operación comercial. Permiso migrado a LSE. Pendiente adenda CI, MIA y pago de garantías. Sin solicitud formal en SIASIC.', N'Actualizar MIA, pagar garantías, presentar SIASIC.', N'Construido sin operación; revalidación pendiente.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Parque Eólico San Carlos';
END
-- Bitácora: Fotovoltaico Flex
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Fotovoltaico Flex')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'EN PROCESO', N'2 MW Zacatecas autoabastecimiento. Implementando medición; solicita extensión.', N'Implementar medición y cerrar operación comercial.', N'Necesita extensión del CI.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Fotovoltaico Flex';
END
-- Bitácora: Central Fotovoltaica Flex Guadalajara
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Fotovoltaica Flex Guadalajara')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'EN PROCESO', N'0.96 MW GDL con cogeneración legada migrada a LSE. EvIS solicitada en 2017.', N'Cerrar interconexión, regularizar MISE y trámites ambientales.', N'Trámites desactualizados desde 2017.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Central Fotovoltaica Flex Guadalajara';
END
-- Bitácora: Rancho del Norte
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Rancho del Norte')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'250 MW eólico para descarbonizar Ternium NL. Permiso, CI, INAH y aeronáuticos vigentes. FEOC no cumplible.', N'Migrar a LSE, adenda CI, MIA SEMARNAT. Construcción 1er cuatrimestre 2027.', N'FEOC no cumplible en plazos originales.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Rancho del Norte';
END
-- Bitácora: Central Pachamama II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Central Pachamama II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Neoen NO compareció. CENACE: adenda CI con FEOC 15-dic-2028, en fase de construcción sin grado real conocido.', N'Reunión de seguimiento con Neoen.', N'No comparecencia — no se verifica avance.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Central Pachamama II';
END
-- Bitácora: Sol de los Manzanos
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Sol de los Manzanos')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-07', N'SIN AVANCE', N'Cluster SIMSA. INAH y MIA pero licencia de construcción vencida.', N'Renovar licencia y actualizar trámites.', N'Licencia de construcción vencida.', N'Claude / SENER (22-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota Técnica Proyectos Firmes 07/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-07'
    WHERE p.Nombre = N'Sol de los Manzanos';
END
-- Bitácora: Alaia II
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia II')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS', N'Proyecto Agrupado Alaias: consolida 6 centrales (Alaia II-V, Altair, RMSC) en 180 MW único en SE Paso del Norte. Reubicación a 9 km de la SE. Participó en Mixtos pero no pasó a 2a ronda por vertimiento en GCR Norte.', N'Inscribir Proyecto Agrupado en próxima ventana Mixtos. Desistir permisos individuales. Revisar garantías con CENACE.', N'No pasó 2a ronda Mixtos por vertimiento GCR Norte; requiere reingeniería completa con SAEE.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Alaia II';
END
-- Bitácora: Alaia III
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia III')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS', N'Parte del Proyecto Agrupado Alaias (180 MW en SE Paso del Norte).', N'Inscribir Proyecto Agrupado en Mixtos. Desistir permiso individual.', N'Mismas que Proyecto Agrupado.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Alaia III';
END
-- Bitácora: Alaia IV
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia IV')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS (con resolutivo)', N'Parte del Proyecto Agrupado Alaias. Cuenta con resolutivo, pero por cambio de ubicación se consolida.', N'Inscribir Proyecto Agrupado en Mixtos. Desistir permiso individual.', N'Resolutivo previo queda sin uso por consolidación.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Alaia IV';
END
-- Bitácora: Alaia V
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Alaia V')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS (con resolutivo)', N'Parte del Proyecto Agrupado Alaias. Cuenta con resolutivo, pero por cambio de ubicación se consolida.', N'Inscribir Proyecto Agrupado en Mixtos. Desistir permiso individual.', N'Resolutivo previo queda sin uso por consolidación.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Alaia V';
END
-- Bitácora: Altair
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Altair')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS', N'Parte del Proyecto Agrupado Alaias (180 MW en SE Paso del Norte).', N'Inscribir Proyecto Agrupado en Mixtos. Desistir permiso individual.', N'Mismas que Proyecto Agrupado.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Altair';
END
-- Bitácora: RMSC Comercio
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'RMSC Comercio')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'CONSOLIDADO EN MIXTOS', N'Parte del Proyecto Agrupado Alaias (180 MW en SE Paso del Norte).', N'Inscribir Proyecto Agrupado en Mixtos. Desistir permiso individual.', N'Mismas que Proyecto Agrupado.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'RMSC Comercio';
END
-- Bitácora: Energía Villa de Arriaga
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Energía Villa de Arriaga')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'EN EVALUACIÓN MIXTOS', N'Proyecto en Villa de Arriaga, SLP. Permiso E-1339-GEN-2015, estudio SICE-00186-2018. Interesado en Mixtos pero requiere actualizar estudios (incremento de capacidad + almacenamiento) y modificar fecha de operación.', N'Decidir en próximos días: participar en Mixtos (con desistimiento) o continuar por cuenta propia.', N'Inversión previa puede perderse si optan por Mixtos.', N'Claude/SENER', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Nota_Reunión_20mayo2026_Alaias y Villa de Arriaga_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Energía Villa de Arriaga';
END
-- Bitácora: Aura Solar
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Aura Solar')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'MIGRACIÓN / SAEE', N'FV 30 MW BCS; Pequeña Producción con migración a LSE, venta a CFE y SAEE ajustado a 20 MW/3h. CENACE advierte continuidad operativa en demanda máxima; DEF solo por 20 MW.', N'DGEASP compartirá consulta con DGISCPOS; CENACE recibirá escrito de arquitectura/operación del SAEE; CFE ICL deberá solicitar registro de activos.', N'Riesgo de detener operación durante migración; MIA/MISSE por almacenamiento aún por definir.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Aura Solar';
END
-- Bitácora: Hidroeléctrica Mizú
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Hidroeléctrica Mizú')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'ALERTA REGULATORIA', N'Proyecto estratégico; CENACE ve interconexión viable, pero CNE alerta que opera sin permiso ni solicitudes registradas.', N'CNE revisará sanciones; DGEASP definirá vía ordinaria o expedita; convocar empresa para VUPE, MIA/MISSE, estudios y permiso CNE.', N'Operación sin permiso implica posible sanción inmediata; vigencia de MISSE/estudios previos incierta.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Hidroeléctrica Mizú';
END
-- Bitácora: Proman (Planta de Fertilizantes)
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Proman (Planta de Fertilizantes)')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'RUTA AUTOCONSUMO', N'Cogeneración Topolobampo 18 MW LIE con incremento a 24 MW; CENACE exige centro de carga en operación comercial para tratarlo como existente.', N'Reunirse con Proman y ofrecer dos rutas: continuar tras operación comercial o reiniciar proceso unificado LSE; mantener garantía actual y nueva garantía de estudio.', N'Obras de refuerzo probablemente se conservan pero dependen de nuevos estudios y prelación; centro de carga no es aún carga existente.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Proman (Planta de Fertilizantes)';
END
-- Bitácora: Rancho del Norte / Arroyo Viento del Norte
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Rancho del Norte / Arroyo Viento del Norte')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'RUTA SIMPLIFICADA', N'Se acordó vía ágil: enlazar estudios históricos al nuevo permiso, mantener configuración técnica y evitar batería física mediante tarifa de flexibilidad modelo San Carlos.', N'CENACE enviará obras de refuerzo originales; DGEASP planteará ruta simplificada y agendará reunión con directivos Ternium.', N'Si se exige nuevo permiso/MISSE/baterías sin simplificación, el proyecto se desvirtúa y se retrasa; MISSE debe tramitarse desde cero.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Minuta Mesa eléctrica - Ruta Interconexión Proyectos de Generación 20/05/2026'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Rancho del Norte / Arroyo Viento del Norte';
END
-- Bitácora: Eólica Dzilam
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Eólica Dzilam')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-04-21', N'RUTA CRÍTICA AMBIENTAL / RAN', N'Proyecto eólico en Yucatán. MISE en consulta; INAH en etapa prospectiva. MIA con prevención pendiente de respuesta en mayo; línea de transmisión incompatible con ordenamiento territorial/ecológico estatal.', N'Compartir información ingresada para MIA; revisar con Antonino incompatibilidad de LT; enviar listado de contratos atorados en RAN; convocar reunión con la Secretaria.', N'Foco rojo por incompatibilidad de LT; solo 15-18 de 66 contratos ingresados al RAN; informe de financiamiento a CNE vence en junio.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'20260421_ Minuta DZILAM.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-04-21'
    WHERE p.Nombre = N'Eólica Dzilam';
END
-- Bitácora: Hidroeléctrica Mizú
IF EXISTS (SELECT 1 FROM dgmesnie.Proyecto WHERE Nombre = N'Hidroeléctrica Mizú')
BEGIN
    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, ProcesadoPor, CreadoPor)
    SELECT p.ProyectoId, re.ReunionId, d.DocumentoId, '2026-05-20', N'FICHA TÉCNICA / REGULARIZACIÓN', N'Central existente en Ixtaczoquitlán, Veracruz, sobre río Manzinga; 1.2 MW rehabilitados, Ciudad Mendoza 0.5 MW y expansión propuesta a 6.5 MW total. Interconexión en media tensión CFE SE Orizaba; no se observa problemática de interconexión.', N'Definir cargas locales/excedentes al SIN, FEOC por etapas, solicitud de estudios CENACE, estudios ambientales/sociales y permiso de generación. Ciudad Mendoza 0.5 MW sería Generador Exento con contrato CFE Suministro Básico.', N'Opera/pretende operar sin permisos completos; ubicación dentro del Parque Nacional Cañón del Río Blanco exige atención ambiental/social. No forma parte de planeación vinculante al no estar en PVIRCE.', N'ChatGPT/SENER (24-may-2026)', N'MigracionExcel'
    FROM dgmesnie.Proyecto p
    LEFT JOIN dgmesnie.Documento d ON d.Titulo = N'Ficha_Proyecto Central Hidroeléctrica Mizu_VF1.docx'
    LEFT JOIN dgmesnie.Reunion re ON re.DocumentoId = d.DocumentoId AND re.FechaReunion = '2026-05-20'
    WHERE p.Nombre = N'Hidroeléctrica Mizú';
END
GO
