SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE dgmesnie.PAMProyectoUbicacion
SET Activa = 0,
    UsuarioModificacion = N'Codex · reversión recuperable autorizada',
    FechaModificacion = SYSUTCDATETIME(),
    Observaciones = LEFT(CONCAT(COALESCE(Observaciones,N''), N' Reversión: lote desactivado.'), 1000)
WHERE Activa = 1
  AND Observaciones LIKE N'%Lote=PAM-UBICACION-CAPAS-20260802-01.%';

IF @@ROWCOUNT <> 22
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51120, N'Reversión cancelada: no se localizaron exactamente 22 relaciones activas del lote.', 1;
END;

COMMIT TRANSACTION;
