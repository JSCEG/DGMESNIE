SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRANSACTION;

UPDATE dgmesnie.PAMProyectoUbicacion
   SET Fuente = N'CFE y DOF - domicilio oficial; DGMESNIE - convergencia de ocho trazas; Atlas SEN/OpenStreetMap - coordenada',
       UsuarioRegistro = N'Codex - investigacion individual autorizada - 2026-08-02'
 WHERE ProyectoId = 130
   AND Activa = 1
   AND MetodoUbicacion = N'investigacion_individual_conciliada_v2'
   AND Observaciones LIKE N'%Lote=PAM-UBICACION-PROFUNDA-20260802-001.%';

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51505, N'Normalizacion: no se encontro exactamente una fila de CFE20-PCC.', 1;
END;

COMMIT TRANSACTION;
