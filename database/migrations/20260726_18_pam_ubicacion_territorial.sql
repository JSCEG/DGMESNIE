SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoUbicacion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoUbicacion
        (
            UbicacionId        BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMProyectoUbicacion PRIMARY KEY,
            ProyectoId         BIGINT NOT NULL,
            Etiqueta           NVARCHAR(300) NULL,
            TipoGeometria      NVARCHAR(30) NOT NULL,
            GeometriaJson      NVARCHAR(MAX) NOT NULL,
            Latitud            DECIMAL(9,6) NULL,
            Longitud           DECIMAL(10,6) NULL,
            Direccion          NVARCHAR(1000) NULL,
            Entidad            NVARCHAR(200) NULL,
            Municipio          NVARCHAR(300) NULL,
            Localidad          NVARCHAR(300) NULL,
            PrecisionUbicacion NVARCHAR(40) NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Precision DEFAULT (N'exacta'),
            MetodoUbicacion    NVARCHAR(80) NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Metodo DEFAULT (N'coordenada_oficial'),
            Fuente             NVARCHAR(500) NOT NULL,
            FechaCorte         DATE NULL,
            RadioSugeridoKm    DECIMAL(8,2) NULL,
            Orden              INT NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Orden DEFAULT (1),
            EsPrincipal        BIT NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Principal DEFAULT (0),
            Validada           BIT NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Validada DEFAULT (0),
            Activa             BIT NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Activa DEFAULT (1),
            FechaRegistroUtc   DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMProyectoUbicacion_Fecha DEFAULT (SYSUTCDATETIME()),
            UsuarioRegistro    NVARCHAR(300) NULL,
            FechaValidacionUtc DATETIME2(0) NULL,
            UsuarioValidacion  NVARCHAR(300) NULL,
            Observaciones      NVARCHAR(1000) NULL,
            CONSTRAINT FK_PAMProyectoUbicacion_Proyecto
                FOREIGN KEY (ProyectoId)
                REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT CK_PAMProyectoUbicacion_Tipo
                CHECK (TipoGeometria IN
                    (N'Point', N'MultiPoint', N'LineString', N'MultiLineString',
                     N'Polygon', N'MultiPolygon')),
            CONSTRAINT CK_PAMProyectoUbicacion_Precision
                CHECK (PrecisionUbicacion IN
                    (N'exacta', N'geocodificada', N'municipal', N'regional')),
            CONSTRAINT CK_PAMProyectoUbicacion_Geometria
                CHECK (ISJSON(GeometriaJson) = 1),
            CONSTRAINT CK_PAMProyectoUbicacion_Coordenadas
                CHECK
                (
                    (Latitud IS NULL AND Longitud IS NULL)
                    OR
                    (Latitud BETWEEN 14 AND 33.5
                     AND Longitud BETWEEN -118 AND -86)
                ),
            CONSTRAINT CK_PAMProyectoUbicacion_Radio
                CHECK (RadioSugeridoKm IS NULL OR RadioSugeridoKm BETWEEN 0 AND 250)
        );

        CREATE INDEX IX_PAMProyectoUbicacion_Proyecto
            ON dgmesnie.PAMProyectoUbicacion
                (ProyectoId, Activa, Validada, EsPrincipal, Orden);

        CREATE INDEX IX_PAMProyectoUbicacion_Coordenadas
            ON dgmesnie.PAMProyectoUbicacion
                (Latitud, Longitud)
            WHERE Activa = 1 AND Validada = 1
              AND Latitud IS NOT NULL AND Longitud IS NOT NULL;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    COUNT(*) AS TotalUbicaciones,
    SUM(CASE WHEN Validada = 1 AND Activa = 1 THEN 1 ELSE 0 END)
        AS UbicacionesValidadas
FROM dgmesnie.PAMProyectoUbicacion;
