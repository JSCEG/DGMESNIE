SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-09';
DECLARE @Metodo NVARCHAR(80) = N'complejo_osm_identidad_corrob_oficial';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 18.255300);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -96.391200);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 32
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'M18-SIN1'
          AND v.NombreProyecto LIKE N'Proyecto de Inversión de CEVs%'
          AND v.GRT LIKE N'%OR%'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Ejecución/Construcción'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%SE Temascal Tres%'
          AND v.ElementosEquiposAsociados LIKE N'%CEV a STATCOM%'
    )
        THROW 54101, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte del Proyecto 32.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 32 AND Activa = 1
    )
        THROW 54102, N'Preflight: M18-SIN1 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54103, N'Preflight: el lote ya fue aplicado.', 1;

    IF @Latitud NOT BETWEEN CONVERT(decimal(9,6), 18.250000) AND CONVERT(decimal(9,6), 18.258000)
       OR @Longitud NOT BETWEEN CONVERT(decimal(10,6), -96.395000) AND CONVERT(decimal(10,6), -96.387000)
        THROW 54104, N'Preflight: el punto representativo no cae dentro del complejo Temascal 2 y 3.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.RedElectricaNodo
        WHERE VersionId = (SELECT MAX(VersionId) FROM dgmesnie.RedElectricaVersion)
          AND Nombre = N'SE TEMASCAL II'
          AND TensionKv = CONVERT(decimal(9,3), 400.000)
          AND Latitud BETWEEN CONVERT(decimal(9,6), 18.250000) AND CONVERT(decimal(9,6), 18.258000)
          AND Longitud BETWEEN CONVERT(decimal(10,6), -96.395000) AND CONVERT(decimal(10,6), -96.387000)
    )
        THROW 54105, N'Preflight: el inventario local ya no confirma el complejo Temascal de 400 kV.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        32,
        N'Complejo S.E. Temascal 2 y 3 · M18-SIN1',
        N'Point',
        N'{"type":"Point","coordinates":[-96.391200,18.255300]}',
        @Latitud,
        @Longitud,
        N'Oaxaca',
        N'exacta',
        @Metodo,
        N'CENACE · PAMRNT/PRODESEN y diagramas RNT; DOF/CFE · CEV Temascal III; OpenStreetMap way 113005835; inventario DGMESNIE',
        f.FechaCorte,
        CONVERT(decimal(8,2), 0.50),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'UBICACIÓN DEL COMPLEJO CORROBORADA: el expediente PAM M18-SIN1 identifica expresamente la S.E. Temascal Tres y la modernización de un CEV a STATCOM. ',
            N'CENACE y el DOF/CFE documentan Temascal III y el CEV; el PRODESEN registra el enlace Juile-Temascal III de 400 kV. ',
            N'OpenStreetMap way 113005835 delimita el complejo Temascal 2 y 3, operador CFE, 400/230 kV. El inventario DGMESNIE confirma el nodo contiguo Temascal II de 400 kV. ',
            N'El punto publicado representa el complejo compartido y no sustituye Temascal Tres por Temascal I ni afirma la posición interna exacta del STATCOM. ',
            N'Versión OSM 5, 2025-04-01. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 32 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54106, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 32 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND MetodoUbicacion = @Metodo
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND ISJSON(GeometriaJson) = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54107, N'Aplicación: la ubicación no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 32 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
