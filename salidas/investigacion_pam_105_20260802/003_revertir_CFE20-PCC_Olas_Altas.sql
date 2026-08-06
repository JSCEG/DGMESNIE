SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE dgmesnie.PAMProyectoUbicacion
   SET Activa = 0,
       UsuarioValidacion = N'Reversion recuperable preparada por Codex - 2026-08-02',
       FechaValidacionUtc = SYSUTCDATETIME()
 WHERE ProyectoId = 130
   AND Activa = 1
   AND MetodoUbicacion = N'investigacion_individual_conciliada_v2'
   AND Observaciones LIKE N'%Lote=PAM-UBICACION-PROFUNDA-20260802-001.%';

SELECT @@ROWCOUNT AS RelacionesDesactivadas;

COMMIT TRANSACTION;
