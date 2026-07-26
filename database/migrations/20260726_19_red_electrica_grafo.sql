SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'dgmesnie') IS NULL
    BEGIN
        EXEC(N'CREATE SCHEMA dgmesnie');
    END;

    IF OBJECT_ID(N'dgmesnie.RedElectricaVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.RedElectricaVersion
        (
            VersionId               BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_RedElectricaVersion PRIMARY KEY,
            VersionClave            NVARCHAR(80) NOT NULL,
            VersionReglas           NVARCHAR(40) NOT NULL,
            FuenteSubestaciones     NVARCHAR(1000) NOT NULL,
            FuenteLineas            NVARCHAR(1000) NOT NULL,
            HashSubestaciones       CHAR(64) NOT NULL,
            HashLineas              CHAR(64) NOT NULL,
            GeneradoUtc             DATETIME2(3) NOT NULL,
            DuracionConstruccionMs  BIGINT NOT NULL,
            ResumenJson             NVARCHAR(MAX) NOT NULL,
            Estado                  NVARCHAR(20) NOT NULL
                CONSTRAINT DF_RedElectricaVersion_Estado
                DEFAULT (N'preliminar'),
            Activa                  BIT NOT NULL
                CONSTRAINT DF_RedElectricaVersion_Activa DEFAULT (0),
            FechaRegistroUtc        DATETIME2(0) NOT NULL
                CONSTRAINT DF_RedElectricaVersion_Fecha
                DEFAULT (SYSUTCDATETIME()),
            UsuarioRegistro         NVARCHAR(300) NULL,
            CONSTRAINT UQ_RedElectricaVersion_Clave
                UNIQUE (VersionClave),
            CONSTRAINT CK_RedElectricaVersion_Resumen
                CHECK (ISJSON(ResumenJson) = 1),
            CONSTRAINT CK_RedElectricaVersion_Estado
                CHECK (Estado IN
                    (N'preliminar', N'publicada', N'descartada'))
        );

        CREATE UNIQUE INDEX UX_RedElectricaVersion_Activa
            ON dgmesnie.RedElectricaVersion (Activa)
            WHERE Activa = 1;
    END;

    IF OBJECT_ID(N'dgmesnie.RedElectricaNodo', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.RedElectricaNodo
        (
            VersionId                BIGINT NOT NULL,
            NodoClave                NVARCHAR(64) NOT NULL,
            ClaveElementoCatalogo    NVARCHAR(64) NULL,
            TipoNodo                 NVARCHAR(30) NOT NULL,
            Nombre                   NVARCHAR(500) NOT NULL,
            NombreNormalizado        NVARCHAR(500) NOT NULL,
            Latitud                  DECIMAL(10,7) NOT NULL,
            Longitud                 DECIMAL(11,7) NOT NULL,
            TensionKv                DECIMAL(9,3) NULL,
            Fase                     NVARCHAR(100) NULL,
            Fuente                   NVARCHAR(1000) NOT NULL,
            EsVirtual                BIT NOT NULL,
            MotivoVirtual            NVARCHAR(1000) NULL,
            Grado                    INT NOT NULL,
            ComponenteClave          NVARCHAR(40) NULL,
            CONSTRAINT PK_RedElectricaNodo
                PRIMARY KEY (VersionId, NodoClave),
            CONSTRAINT FK_RedElectricaNodo_Version
                FOREIGN KEY (VersionId)
                REFERENCES dgmesnie.RedElectricaVersion(VersionId),
            CONSTRAINT CK_RedElectricaNodo_Tipo
                CHECK (TipoNodo IN
                    (N'subestacion', N'nodo_virtual')),
            CONSTRAINT CK_RedElectricaNodo_Coordenadas
                CHECK
                (
                    Latitud BETWEEN 10 AND 40
                    AND Longitud BETWEEN -125 AND -80
                ),
            CONSTRAINT CK_RedElectricaNodo_Grado
                CHECK (Grado >= 0)
        );

        CREATE INDEX IX_RedElectricaNodo_Nombre
            ON dgmesnie.RedElectricaNodo
                (VersionId, NombreNormalizado);

        CREATE INDEX IX_RedElectricaNodo_Catalogo
            ON dgmesnie.RedElectricaNodo
                (VersionId, ClaveElementoCatalogo)
            WHERE ClaveElementoCatalogo IS NOT NULL;

        CREATE INDEX IX_RedElectricaNodo_Componente
            ON dgmesnie.RedElectricaNodo
                (VersionId, ComponenteClave, Grado);
    END;

    IF OBJECT_ID(N'dgmesnie.RedElectricaArista', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.RedElectricaArista
        (
            VersionId                BIGINT NOT NULL,
            AristaClave              NVARCHAR(64) NOT NULL,
            ClaveElementoCatalogo    NVARCHAR(64) NOT NULL,
            Nombre                   NVARCHAR(800) NOT NULL,
            NombreNormalizado        NVARCHAR(800) NOT NULL,
            NodoOrigenClave          NVARCHAR(64) NOT NULL,
            NodoDestinoClave         NVARCHAR(64) NOT NULL,
            ExtremoNominalA          NVARCHAR(500) NULL,
            ExtremoNominalB          NVARCHAR(500) NULL,
            ConfianzaOrigen          INT NOT NULL,
            ConfianzaDestino         INT NOT NULL,
            ResolucionOrigen         NVARCHAR(30) NOT NULL,
            ResolucionDestino        NVARCHAR(30) NOT NULL,
            EstadoConexion           NVARCHAR(30) NOT NULL,
            TensionKv                DECIMAL(9,3) NULL,
            Circuitos                INT NULL,
            LongitudCatalogoKm       DECIMAL(12,3) NULL,
            LongitudGeometriaKm      DECIMAL(12,3) NOT NULL,
            IndiceSegmento           INT NOT NULL,
            GeometriaJson            NVARCHAR(MAX) NOT NULL,
            Fuente                   NVARCHAR(1000) NOT NULL,
            CONSTRAINT PK_RedElectricaArista
                PRIMARY KEY (VersionId, AristaClave),
            CONSTRAINT FK_RedElectricaArista_Version
                FOREIGN KEY (VersionId)
                REFERENCES dgmesnie.RedElectricaVersion(VersionId),
            CONSTRAINT FK_RedElectricaArista_Origen
                FOREIGN KEY (VersionId, NodoOrigenClave)
                REFERENCES dgmesnie.RedElectricaNodo(VersionId, NodoClave),
            CONSTRAINT FK_RedElectricaArista_Destino
                FOREIGN KEY (VersionId, NodoDestinoClave)
                REFERENCES dgmesnie.RedElectricaNodo(VersionId, NodoClave),
            CONSTRAINT CK_RedElectricaArista_Confianza
                CHECK
                (
                    ConfianzaOrigen BETWEEN 0 AND 100
                    AND ConfianzaDestino BETWEEN 0 AND 100
                ),
            CONSTRAINT CK_RedElectricaArista_Estado
                CHECK (EstadoConexion IN
                    (N'conectada', N'parcial', N'sin_resolver')),
            CONSTRAINT CK_RedElectricaArista_Geometria
                CHECK (ISJSON(GeometriaJson) = 1),
            CONSTRAINT CK_RedElectricaArista_Longitud
                CHECK
                (
                    LongitudGeometriaKm >= 0
                    AND
                    (LongitudCatalogoKm IS NULL
                     OR LongitudCatalogoKm >= 0)
                )
        );

        CREATE INDEX IX_RedElectricaArista_Origen
            ON dgmesnie.RedElectricaArista
                (VersionId, NodoOrigenClave, EstadoConexion);

        CREATE INDEX IX_RedElectricaArista_Destino
            ON dgmesnie.RedElectricaArista
                (VersionId, NodoDestinoClave, EstadoConexion);

        CREATE INDEX IX_RedElectricaArista_Catalogo
            ON dgmesnie.RedElectricaArista
                (VersionId, ClaveElementoCatalogo);
    END;

    IF OBJECT_ID(N'dgmesnie.RedElectricaRevision', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.RedElectricaRevision
        (
            RevisionId          BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_RedElectricaRevision PRIMARY KEY,
            VersionId           BIGINT NOT NULL,
            RevisionClave       NVARCHAR(64) NOT NULL,
            AristaClave         NVARCHAR(64) NOT NULL,
            NombreLinea         NVARCHAR(800) NOT NULL,
            LadoExtremo         CHAR(1) NOT NULL,
            ExtremoNominal      NVARCHAR(500) NULL,
            Latitud             DECIMAL(10,7) NOT NULL,
            Longitud            DECIMAL(11,7) NOT NULL,
            Motivo              NVARCHAR(1000) NOT NULL,
            CandidatosJson      NVARCHAR(MAX) NOT NULL,
            Resuelta            BIT NOT NULL
                CONSTRAINT DF_RedElectricaRevision_Resuelta DEFAULT (0),
            NodoValidadoClave   NVARCHAR(64) NULL,
            FechaResolucionUtc  DATETIME2(0) NULL,
            UsuarioResolucion   NVARCHAR(300) NULL,
            Observaciones       NVARCHAR(1000) NULL,
            CONSTRAINT UQ_RedElectricaRevision_Clave
                UNIQUE (VersionId, RevisionClave),
            CONSTRAINT FK_RedElectricaRevision_Version
                FOREIGN KEY (VersionId)
                REFERENCES dgmesnie.RedElectricaVersion(VersionId),
            CONSTRAINT FK_RedElectricaRevision_Arista
                FOREIGN KEY (VersionId, AristaClave)
                REFERENCES dgmesnie.RedElectricaArista
                    (VersionId, AristaClave),
            CONSTRAINT FK_RedElectricaRevision_Nodo
                FOREIGN KEY (VersionId, NodoValidadoClave)
                REFERENCES dgmesnie.RedElectricaNodo
                    (VersionId, NodoClave),
            CONSTRAINT CK_RedElectricaRevision_Lado
                CHECK (LadoExtremo IN ('A', 'B')),
            CONSTRAINT CK_RedElectricaRevision_Candidatos
                CHECK (ISJSON(CandidatosJson) = 1)
        );

        CREATE INDEX IX_RedElectricaRevision_Pendiente
            ON dgmesnie.RedElectricaRevision
                (VersionId, Resuelta, AristaClave);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    v.VersionId,
    v.VersionClave,
    v.VersionReglas,
    v.Estado,
    v.Activa,
    COUNT(DISTINCT n.NodoClave) AS Nodos,
    COUNT(DISTINCT a.AristaClave) AS Aristas
FROM dgmesnie.RedElectricaVersion AS v
LEFT JOIN dgmesnie.RedElectricaNodo AS n
    ON n.VersionId = v.VersionId
LEFT JOIN dgmesnie.RedElectricaArista AS a
    ON a.VersionId = v.VersionId
GROUP BY
    v.VersionId,
    v.VersionClave,
    v.VersionReglas,
    v.Estado,
    v.Activa
ORDER BY v.VersionId DESC;
