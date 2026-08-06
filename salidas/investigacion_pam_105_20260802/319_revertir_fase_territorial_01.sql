SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-01';

BEGIN TRY
    BEGIN TRANSACTION;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WHERE Observaciones LIKE N'%' + @Lote + N'%' AND ProyectoId IN (141,160,192,306)) <> 4
        THROW 52101, N'Reversor: no se encontraron exactamente las cuatro ubicaciones del lote.', 1;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoRelacionVersion WHERE Observaciones LIKE N'%' + @Lote + N'%') <> 3
        THROW 52102, N'Reversor: no se encontraron exactamente las tres relaciones del lote.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE UbicacionId = 4 AND ProyectoId = 64 AND Activa = 1 AND Validada = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 52103, N'Reversor: la ubicación promovida de Cerro del Mercado ya no está en el estado aplicado.', 1;

    DELETE FROM dgmesnie.PAMProyectoRelacionVersion
    WHERE Observaciones LIKE N'%' + @Lote + N'%';
    IF @@ROWCOUNT <> 3 THROW 52104, N'Reversor: no se eliminaron exactamente tres relaciones.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%'
      AND ProyectoId IN (141,160,192,306);
    IF @@ROWCOUNT <> 4 THROW 52105, N'Reversor: no se eliminaron exactamente cuatro ubicaciones.', 1;

    UPDATE dgmesnie.PAMProyectoUbicacion
       SET PrecisionUbicacion = N'geocodificada',
           MetodoUbicacion = N'cruce_catalogo_red_v1',
           Fuente = N'https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson',
           RadioSugeridoKm = CONVERT(decimal(8,2), 20),
           Validada = 0,
           FechaValidacionUtc = NULL,
           UsuarioValidacion = NULL,
           Observaciones = N'Asociación sugerida no validada. ClaveElementoRed=se:b0943b6b0edb7166d63b; Puntaje=100; Confianza=alta; FuenteDocumento=INF Pormenorizado Tablas Rev 251125 SENER.xlsx; Lote=PAM-UBICACION-20260802-01.'
     WHERE UbicacionId = 4 AND ProyectoId = 64 AND Activa = 1
       AND Observaciones LIKE N'%' + @Lote + N'%';
    IF @@ROWCOUNT <> 1 THROW 52106, N'Reversor: no se restauró exactamente la ubicación legado.', 1;

    COMMIT TRANSACTION;
    SELECT N'Fase territorial 01 revertida' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
