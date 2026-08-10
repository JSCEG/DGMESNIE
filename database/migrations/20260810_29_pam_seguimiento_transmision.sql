SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dgmesnie.PAMLoteActualizacion', N'TipoActualizacion') IS NULL
    BEGIN
        EXEC sys.sp_executesql N'
        ALTER TABLE dgmesnie.PAMLoteActualizacion
            ADD TipoActualizacion NVARCHAR(40) NOT NULL
                CONSTRAINT DF_PAMLote_TipoActualizacion
                DEFAULT N''InformePormenorizado'' WITH VALUES;';
    END;

    IF OBJECT_ID(N'dgmesnie.CK_PAMLote_TipoActualizacion', N'C') IS NULL
    BEGIN
        EXEC sys.sp_executesql N'
        ALTER TABLE dgmesnie.PAMLoteActualizacion WITH CHECK
            ADD CONSTRAINT CK_PAMLote_TipoActualizacion
            CHECK (TipoActualizacion IN (N''InformePormenorizado'', N''SeguimientoTransmision''));';
    END;

    IF OBJECT_ID(N'dgmesnie.PAMSeguimientoUnidad', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMSeguimientoUnidad
        (
            SeguimientoUnidadId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMSeguimientoUnidad PRIMARY KEY,
            SeguimientoUnidadUid      UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PAMSeguimientoUnidad_Uid DEFAULT NEWSEQUENTIALID(),
            CodigoUnico               NVARCHAR(200) NOT NULL,
            IdSepi                    NVARCHAR(200) NULL,
            NombreUnidad              NVARCHAR(1000) NOT NULL,
            TieneFases                BIT NOT NULL
                CONSTRAINT DF_PAMSeguimientoUnidad_TieneFases DEFAULT (0),
            PrimerLoteId              BIGINT NOT NULL,
            UltimoLoteId              BIGINT NOT NULL,
            FechaAltaUtc              DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMSeguimientoUnidad_FechaAlta DEFAULT SYSUTCDATETIME(),
            FechaActualizacionUtc     DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMSeguimientoUnidad_FechaActualizacion DEFAULT SYSUTCDATETIME(),
            UsuarioActualizacion      NVARCHAR(150) NOT NULL,
            VersionFila               ROWVERSION NOT NULL,
            CONSTRAINT UQ_PAMSeguimientoUnidad_Uid UNIQUE (SeguimientoUnidadUid),
            CONSTRAINT UQ_PAMSeguimientoUnidad_Codigo UNIQUE (CodigoUnico),
            CONSTRAINT FK_PAMSeguimientoUnidad_PrimerLote FOREIGN KEY (PrimerLoteId)
                REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT FK_PAMSeguimientoUnidad_UltimoLote FOREIGN KEY (UltimoLoteId)
                REFERENCES dgmesnie.PAMLoteActualizacion(LoteId)
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PAMSeguimientoCorteDetalle', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMSeguimientoCorteDetalle
        (
            SeguimientoDetalleId              BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMSeguimientoCorteDetalle PRIMARY KEY,
            SeguimientoUnidadId               BIGINT NOT NULL,
            LoteId                            BIGINT NOT NULL,
            FuenteId                          BIGINT NOT NULL,
            PerfilVersion                     NVARCHAR(50) NOT NULL,
            Hoja                              NVARCHAR(150) NOT NULL,
            NumeroFila                        INT NOT NULL,
            CodigoPem                         NVARCHAR(500) NULL,
            IdSepi                            NVARCHAR(200) NULL,
            IdProcedimiento                   NVARCHAR(300) NULL,
            ProyectoEnFases                   NVARCHAR(100) NULL,
            FaseInicial                       NVARCHAR(300) NULL,
            FasesSubsecuentes                 NVARCHAR(1000) NULL,
            ActivoAdministracionActual        BIT NULL,
            AdministracionActual              BIT NULL,
            NombreProyecto                    NVARCHAR(1000) NOT NULL,
            Categoria                         NVARCHAR(300) NULL,
            Mva                               DECIMAL(18,3) NULL,
            Mvar                              DECIMAL(18,3) NULL,
            KmC                               DECIMAL(18,3) NULL,
            OtraMetaFisica                    NVARCHAR(1000) NULL,
            ImporteMdp                        DECIMAL(18,3) NULL,
            FinanciamientoCfe                 NVARCHAR(500) NULL,
            FinanciamientoPormenorizado       NVARCHAR(500) NULL,
            AnioInstruccion                   INT NULL,
            FechaInicioConcursoProgramada     DATE NULL,
            FechaInicioConcursoReal           DATE NULL,
            FechaAdjudicacionProgramada       DATE NULL,
            FechaAdjudicacionReal             DATE NULL,
            FechaFirmaContratoProgramada      DATE NULL,
            FechaFirmaContratoReal            DATE NULL,
            FechaInicioConstruccionProgramada DATE NULL,
            FechaInicioConstruccionReal       DATE NULL,
            FechaTerminoConstruccion          DATE NULL,
            FeoIndicada                       NVARCHAR(200) NULL,
            FeoFactible                       NVARCHAR(200) NULL,
            PlazoEjecucionDias                INT NULL,
            AvanceProgramado                  DECIMAL(9,6) NULL,
            AvanceReal                        DECIMAL(9,6) NULL,
            ComentariosPpt                    NVARCHAR(MAX) NULL,
            NotaPpt                           NVARCHAR(MAX) NULL,
            NombrePormenorizado               NVARCHAR(1000) NULL,
            ElementosEquipos                  NVARCHAR(MAX) NULL,
            ActualizacionEstatus              NVARCHAR(MAX) NULL,
            UltimaActualizacionFecha          DATE NULL,
            DetalleUltimaActualizacion        NVARCHAR(MAX) NULL,
            OrigenUltimaActualizacion         NVARCHAR(1000) NULL,
            ComentariosInternos               NVARCHAR(MAX) NULL,
            Visible                           BIT NULL,
            DatosOrigenJson                   NVARCHAR(MAX) NOT NULL,
            HashFila                          CHAR(64) NOT NULL,
            FechaRegistroUtc                  DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMSeguimientoDetalle_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro                   NVARCHAR(150) NOT NULL,
            CONSTRAINT FK_PAMSeguimientoDetalle_Unidad FOREIGN KEY (SeguimientoUnidadId)
                REFERENCES dgmesnie.PAMSeguimientoUnidad(SeguimientoUnidadId),
            CONSTRAINT FK_PAMSeguimientoDetalle_Lote FOREIGN KEY (LoteId)
                REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT FK_PAMSeguimientoDetalle_Fuente FOREIGN KEY (FuenteId)
                REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT UQ_PAMSeguimientoDetalle_Fila UNIQUE (LoteId, FuenteId, Hoja, NumeroFila),
            CONSTRAINT CK_PAMSeguimientoDetalle_Json CHECK (ISJSON(DatosOrigenJson) = 1),
            CONSTRAINT CK_PAMSeguimientoDetalle_Hash CHECK (LEN(HashFila) = 64),
            CONSTRAINT CK_PAMSeguimientoDetalle_Avance CHECK
                ((AvanceProgramado IS NULL OR AvanceProgramado BETWEEN 0 AND 1)
                 AND (AvanceReal IS NULL OR AvanceReal BETWEEN 0 AND 1))
        );

        CREATE INDEX IX_PAMSeguimientoDetalle_UnidadLote
            ON dgmesnie.PAMSeguimientoCorteDetalle(SeguimientoUnidadId, LoteId DESC)
            INCLUDE (Categoria, ImporteMdp, AvanceProgramado, AvanceReal);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMSeguimientoUnidadProyecto', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMSeguimientoUnidadProyecto
        (
            SeguimientoUnidadProyectoId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMSeguimientoUnidadProyecto PRIMARY KEY,
            SeguimientoUnidadId         BIGINT NOT NULL,
            ProyectoId                  BIGINT NOT NULL,
            LoteId                      BIGINT NOT NULL,
            FuenteId                    BIGINT NOT NULL,
            CodigoPem                   NVARCHAR(200) NOT NULL,
            TipoCoincidencia            NVARCHAR(30) NOT NULL,
            FechaCorte                  DATE NOT NULL,
            FechaRegistroUtc            DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMSeguimientoUnidadProyecto_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro             NVARCHAR(150) NOT NULL,
            CONSTRAINT FK_PAMSeguimientoUnidadProyecto_Unidad FOREIGN KEY (SeguimientoUnidadId)
                REFERENCES dgmesnie.PAMSeguimientoUnidad(SeguimientoUnidadId),
            CONSTRAINT FK_PAMSeguimientoUnidadProyecto_Proyecto FOREIGN KEY (ProyectoId)
                REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMSeguimientoUnidadProyecto_Lote FOREIGN KEY (LoteId)
                REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT FK_PAMSeguimientoUnidadProyecto_Fuente FOREIGN KEY (FuenteId)
                REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT UQ_PAMSeguimientoUnidadProyecto_Corte
                UNIQUE (SeguimientoUnidadId, ProyectoId, LoteId, CodigoPem),
            CONSTRAINT CK_PAMSeguimientoUnidadProyecto_Tipo
                CHECK (TipoCoincidencia IN (N'Exacta', N'Alterna'))
        );

        CREATE INDEX IX_PAMSeguimientoUnidadProyecto_ProyectoCorte
            ON dgmesnie.PAMSeguimientoUnidadProyecto(ProyectoId, FechaCorte DESC, LoteId DESC)
            INCLUDE (SeguimientoUnidadId, CodigoPem, TipoCoincidencia);
    END;

    EXEC sys.sp_executesql N'
    CREATE OR ALTER VIEW dgmesnie.vw_PAMSeguimientoActual
    AS
    WITH cortes AS
    (
        SELECT d.*,
               l.FechaCorte,
               DENSE_RANK() OVER
               (
                   PARTITION BY d.SeguimientoUnidadId
                   ORDER BY l.FechaCorte DESC, d.LoteId DESC
               ) AS OrdenCorte
        FROM dgmesnie.PAMSeguimientoCorteDetalle d
        INNER JOIN dgmesnie.PAMLoteActualizacion l ON l.LoteId = d.LoteId
    )
    SELECT c.SeguimientoDetalleId, c.SeguimientoUnidadId,
           u.SeguimientoUnidadUid, u.CodigoUnico, u.TieneFases,
           c.LoteId, c.FuenteId, c.PerfilVersion, c.Hoja, c.NumeroFila,
           c.FechaCorte, c.CodigoPem, c.IdSepi, c.IdProcedimiento,
           c.ProyectoEnFases, c.FaseInicial, c.FasesSubsecuentes,
           c.NombreProyecto, c.Categoria, c.Mva, c.Mvar, c.KmC,
           c.OtraMetaFisica, c.ImporteMdp, c.FinanciamientoCfe,
           c.FinanciamientoPormenorizado, c.AnioInstruccion,
           c.FechaInicioConcursoProgramada, c.FechaInicioConcursoReal,
           c.FechaAdjudicacionProgramada, c.FechaAdjudicacionReal,
           c.FechaFirmaContratoProgramada, c.FechaFirmaContratoReal,
           c.FechaInicioConstruccionProgramada, c.FechaInicioConstruccionReal,
           c.FechaTerminoConstruccion, c.FeoIndicada, c.FeoFactible,
           c.PlazoEjecucionDias,
           CASE
               WHEN c.PlazoEjecucionDias IS NULL THEN NULL
               WHEN COALESCE(c.FechaInicioConcursoReal, c.FechaInicioConcursoProgramada) IS NULL THEN NULL
               ELSE DATEADD(DAY, c.PlazoEjecucionDias,
                    COALESCE(c.FechaInicioConcursoReal, c.FechaInicioConcursoProgramada))
           END AS FechaEstimadaTerminoCalculada,
           c.AvanceProgramado, c.AvanceReal,
           CASE WHEN c.AvanceProgramado IS NULL OR c.AvanceReal IS NULL THEN NULL
                ELSE c.AvanceReal - c.AvanceProgramado END AS DiferenciaAvance,
           c.ComentariosPpt, c.NotaPpt, c.NombrePormenorizado,
           c.ElementosEquipos, c.ActualizacionEstatus,
           c.UltimaActualizacionFecha, c.DetalleUltimaActualizacion,
           c.OrigenUltimaActualizacion, c.ComentariosInternos, c.Visible,
           c.HashFila, c.FechaRegistroUtc, c.UsuarioRegistro
    FROM cortes c
    INNER JOIN dgmesnie.PAMSeguimientoUnidad u
        ON u.SeguimientoUnidadId = c.SeguimientoUnidadId
    WHERE c.OrdenCorte = 1;';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER VIEW dgmesnie.vw_PAMSeguimientoProyectoActual
    AS
    SELECT actual.*, relacion.ProyectoId, relacion.CodigoPem AS ClaveProyectoRelacionada,
           relacion.TipoCoincidencia
    FROM dgmesnie.vw_PAMSeguimientoActual actual
    INNER JOIN dgmesnie.PAMSeguimientoUnidadProyecto relacion
        ON relacion.SeguimientoUnidadId = actual.SeguimientoUnidadId
       AND relacion.LoteId = actual.LoteId;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    COL_LENGTH(N'dgmesnie.PAMLoteActualizacion', N'TipoActualizacion') AS TipoActualizacionConfigurado,
    OBJECT_ID(N'dgmesnie.PAMSeguimientoUnidad') AS Unidades,
    OBJECT_ID(N'dgmesnie.PAMSeguimientoCorteDetalle') AS Detalles,
    OBJECT_ID(N'dgmesnie.PAMSeguimientoUnidadProyecto') AS Relaciones,
    OBJECT_ID(N'dgmesnie.vw_PAMSeguimientoActual') AS VistaActual;
