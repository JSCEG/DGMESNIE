SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH LotesConPaqueteCompleto AS
(
    SELECT
        lote.LoteId,
        paquete.PaqueteAplicacionId,
        paquete.TotalDetalles,
        ultimoEvento.TipoEvento,
        ultimoEvento.FechaEventoUtc,
        resultados.TotalResultados
    FROM dgmesnie.PAMLoteActualizacion lote
    CROSS APPLY
    (
        SELECT TOP (1)
            p.PaqueteAplicacionId,
            p.TotalDetalles
        FROM dgmesnie.PAMAplicacionPaquete p
        WHERE p.LoteId = lote.LoteId
        ORDER BY p.PaqueteAplicacionId DESC
    ) paquete
    CROSS APPLY
    (
        SELECT TOP (1)
            e.TipoEvento,
            e.FechaEventoUtc
        FROM dgmesnie.PAMAplicacionPaqueteEvento e
        WHERE e.PaqueteAplicacionId = paquete.PaqueteAplicacionId
        ORDER BY e.PaqueteEventoId DESC
    ) ultimoEvento
    CROSS APPLY
    (
        SELECT COUNT(*) AS TotalResultados
        FROM dgmesnie.PAMAplicacionResultado r
        WHERE r.PaqueteAplicacionId = paquete.PaqueteAplicacionId
    ) resultados
)
UPDATE lote
SET Estado = N'Aplicado',
    FechaActualizacionUtc = COALESCE(completo.FechaEventoUtc, SYSUTCDATETIME())
FROM dgmesnie.PAMLoteActualizacion lote
INNER JOIN LotesConPaqueteCompleto completo ON completo.LoteId = lote.LoteId
WHERE completo.TipoEvento = N'Aplicado'
  AND completo.TotalResultados = completo.TotalDetalles
  AND lote.Estado <> N'Aplicado';

COMMIT TRANSACTION;
