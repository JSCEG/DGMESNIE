SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @ModuloBaseId INT;
    DECLARE @SeccionId INT;
    DECLARE @ModuloConvocatoriasId INT;
    DECLARE @Orden INT;

    SELECT TOP (1)
        @ModuloBaseId = ModuloId,
        @SeccionId = SeccionId
    FROM dgmesnie.Modulo
    WHERE Controller = N'ProyectosPrivados'
      AND Action = N'Index'
    ORDER BY Activo DESC, ModuloId;

    IF @ModuloBaseId IS NULL OR @SeccionId IS NULL
        THROW 51030, N'No existe el módulo base de Proyectos Privados para heredar sección y permisos.', 1;

    SELECT @ModuloConvocatoriasId = ModuloId
    FROM dgmesnie.Modulo
    WHERE Controller = N'ProyectosPrivados'
      AND Action = N'SegundaConvocatoria';

    IF @ModuloConvocatoriasId IS NULL
    BEGIN
        SELECT @Orden = ISNULL(MAX(Orden), 0) + 1
        FROM dgmesnie.Modulo
        WHERE SeccionId = @SeccionId;

        INSERT INTO dgmesnie.Modulo
            (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno)
        VALUES
            (@SeccionId, N'Convocatorias', N'ProyectosPrivados', N'SegundaConvocatoria', @Orden, 1, 0);

        SET @ModuloConvocatoriasId = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dgmesnie.Modulo
        SET Title = N'Convocatorias',
            SeccionId = @SeccionId,
            Activo = 1,
            EsExterno = 0
        WHERE ModuloId = @ModuloConvocatoriasId;
    END;

    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
    SELECT
        base.RolId,
        @ModuloConvocatoriasId,
        base.MercadoId,
        1
    FROM dgmesnie.RolModulo AS base
    WHERE base.ModuloId = @ModuloBaseId
      AND base.Activa = 1
      AND NOT EXISTS
      (
          SELECT 1
          FROM dgmesnie.RolModulo AS existente
          WHERE existente.RolId = base.RolId
            AND existente.ModuloId = @ModuloConvocatoriasId
            AND
            (
                existente.MercadoId = base.MercadoId
                OR (existente.MercadoId IS NULL AND base.MercadoId IS NULL)
            )
      );

    COMMIT TRANSACTION;

    SELECT
        s.Titulo AS Seccion,
        m.ModuloId,
        m.Title,
        m.Controller,
        m.Action,
        m.Orden,
        m.Activo
    FROM dgmesnie.Modulo AS m
    INNER JOIN dgmesnie.Seccion AS s
        ON s.SeccionId = m.SeccionId
    WHERE m.ModuloId = @ModuloConvocatoriasId;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
