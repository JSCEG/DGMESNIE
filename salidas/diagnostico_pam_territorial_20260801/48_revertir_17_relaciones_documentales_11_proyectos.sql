SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE dgmesnie.PAMProyectoUbicacion
   SET Activa = 0,
       UsuarioValidacion = N'Reversión recuperable preparada por Codex · 2026-08-02',
       FechaValidacionUtc = SYSUTCDATETIME()
 WHERE Activa = 1
   AND MetodoUbicacion = N'investigacion_documental_inventario_v1'
   AND Observaciones LIKE N'%Lote=PAM-UBICACION-DOC-20260802-01.%';

SELECT @@ROWCOUNT AS RelacionesDesactivadas;

COMMIT TRANSACTION;
