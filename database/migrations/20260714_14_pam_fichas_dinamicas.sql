SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 14: fichas ejecutivas dinámicas PAM/PAMRNT.
  - PAMProyectoFicha conserva contenido narrativo enriquecido en JSON por proyecto/version.
  - PAMProyectoFichaRecurso permite adjuntar PNG/URL para diagrama unifilar, geoespacial,
    lámina C7U u otros recursos sin cambiar la vista Razor.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoFicha', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoFicha
        (
            FichaId              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyectoFicha PRIMARY KEY,
            ProyectoId           BIGINT NOT NULL,
            ProyectoVersionId    BIGINT NULL,
            ClaveProyecto        NVARCHAR(200) NOT NULL,
            Titulo               NVARCHAR(500) NULL,
            TipoFicha            NVARCHAR(100) NOT NULL CONSTRAINT DF_PAMProyectoFicha_Tipo DEFAULT N'Ficha ejecutiva dinámica',
            FichaJson            NVARCHAR(MAX) NOT NULL,
            EsVigente            BIT NOT NULL CONSTRAINT DF_PAMProyectoFicha_EsVigente DEFAULT (1),
            FechaRegistroUtc     DATETIME2(0) NOT NULL CONSTRAINT DF_PAMProyectoFicha_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro      NVARCHAR(150) NULL,
            FechaActualizacionUtc DATETIME2(0) NULL,
            UsuarioActualizacion NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMProyectoFicha_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMProyectoFicha_Version FOREIGN KEY (ProyectoVersionId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT CK_PAMProyectoFicha_Json CHECK (ISJSON(FichaJson) = 1)
        );

        CREATE UNIQUE INDEX UX_PAMProyectoFicha_Vigente
            ON dgmesnie.PAMProyectoFicha(ProyectoId)
            WHERE EsVigente = 1;

        CREATE INDEX IX_PAMProyectoFicha_Clave
            ON dgmesnie.PAMProyectoFicha(ClaveProyecto, EsVigente);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoFichaRecurso', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoFichaRecurso
        (
            RecursoId            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyectoFichaRecurso PRIMARY KEY,
            ProyectoId           BIGINT NOT NULL,
            FichaId              BIGINT NULL,
            TipoRecurso          NVARCHAR(60) NOT NULL,
            Titulo               NVARCHAR(200) NOT NULL,
            Descripcion          NVARCHAR(1000) NULL,
            Url                  NVARCHAR(1000) NULL,
            AltText              NVARCHAR(250) NULL,
            Orden                INT NOT NULL CONSTRAINT DF_PAMProyectoFichaRecurso_Orden DEFAULT (10),
            Aplica               BIT NOT NULL CONSTRAINT DF_PAMProyectoFichaRecurso_Aplica DEFAULT (1),
            Activo               BIT NOT NULL CONSTRAINT DF_PAMProyectoFichaRecurso_Activo DEFAULT (1),
            FechaRegistroUtc     DATETIME2(0) NOT NULL CONSTRAINT DF_PAMProyectoFichaRecurso_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro      NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMProyectoFichaRecurso_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMProyectoFichaRecurso_Ficha FOREIGN KEY (FichaId) REFERENCES dgmesnie.PAMProyectoFicha(FichaId),
            CONSTRAINT CK_PAMProyectoFichaRecurso_Tipo CHECK (TipoRecurso IN (N'DiagramaUnifilar', N'Geoespacial', N'C7U', N'Otro'))
        );

        CREATE INDEX IX_PAMProyectoFichaRecurso_Proyecto
            ON dgmesnie.PAMProyectoFichaRecurso(ProyectoId, Activo, Orden);
    END;

    DECLARE @ProyectoId BIGINT, @ProyectoVersionId BIGINT, @FichaId BIGINT;

    SELECT TOP (1)
        @ProyectoId = ProyectoId,
        @ProyectoVersionId = ProyectoVersionId
    FROM dgmesnie.vw_PAMProyectoVigente
    WHERE ClaveProyecto = N'I26-PE1';

    IF @ProyectoId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoFicha WHERE ProyectoId = @ProyectoId AND EsVigente = 1)
    BEGIN
        INSERT dgmesnie.PAMProyectoFicha
        (
            ProyectoId, ProyectoVersionId, ClaveProyecto, Titulo, TipoFicha, FichaJson, UsuarioRegistro
        )
        VALUES
        (
            @ProyectoId,
            @ProyectoVersionId,
            N'I26-PE1',
            N'Suministro de energía eléctrica para los estados de Tabasco, Campeche, Yucatán y Quintana Roo',
            N'Ficha ejecutiva PAMRNT',
            N'{
  "clavePem":"I26-PE1",
  "titulo":"Suministro de energía eléctrica para los estados de Tabasco, Campeche, Yucatán y Quintana Roo",
  "tipoFicha":"Ficha ejecutiva PAMRNT",
  "resumenEjecutivo":"Incrementar la capacidad de transmisión hacia el Sureste y la Península de Yucatán para atender el crecimiento de la demanda, aliviar las compuertas críticas y reforzar directamente el suministro de Campeche y Mérida.",
  "alternativaSeleccionada":"Alternativa 1 - red de corriente alterna en 400 kV",
  "recomendacionEjecutiva":"Construcción por etapas para anticipar beneficios y reforzar directamente Campeche y Mérida.",
  "inversionMdp":31293.202,
  "fechaNecesaria":"abril de 2031",
  "fechaFactible":"abril de 2031",
  "relacionBeneficioCosto":5.08,
  "totalObras":55,
  "corredorPrincipal":"Tecpatán - Los Ríos - Campeche - Ticul - Kanasín, con refuerzos hacia Tulum, Kantenáh, Leona Vicario y Riviera Maya",
  "fuente":"PAMRNT 2026-2040, ficha I26-PE1, cuadros 8.4.6.1 a 8.4.6.10",
  "estados":["Tabasco","Campeche","Yucatán","Quintana Roo"],
  "metasFisicas":[{"concepto":"Transmisión","valor":"1,684.0","unidad":"km-C"},{"concepto":"Transformación","valor":"2,350","unidad":"MVA reportados"},{"concepto":"Compensación","valor":"3,558","unidad":"MVAr"},{"concepto":"Obras","valor":"55","unidad":"componentes"}],
  "pendientesValidacion":["Validar el total de transformación: el cuadro reporta 2,350 MVA y el detalle de componentes requiere conciliación aritmética.","Incorporar el cronograma cruzado entre proyectos de generación y las fases de transmisión.","Incorporar las capas geoespaciales de líneas, subestaciones, permisos e impacto social desde el CDN."],
  "diagnosticoOperativo":"La red hacia la Península de Yucatán es longitudinal desde la zona del Grijalva. Toda la región depende de las compuertas Grijalva–Tabasco, Escárcega–Sur y Valladolid–Cancún; en 2025 la primera rebasó durante ocho horas su límite con EAR.",
  "pronosticoDemanda":"La demanda de la GCR Peninsular mantiene una trayectoria de crecimiento que vuelve permanente la saturación del corredor a partir de 2031 si no se incorporan nuevas obras de transmisión.",
  "crecimientoDemandaPorcentaje":4.72,
  "demandaMaxima":"2,921 MWh/h · 27 de mayo de 2025 · 22:00 h",
  "beneficioNetoUsdMiles":5822,
  "notaEvaluacionEconomica":"Evaluación por modelo de producción para la Alternativa 1, en valor presente 2028. La evaluación de la Alternativa 2 estaba en proceso al cierre de edición del PAMRNT.",
  "conclusionEjecutiva":"La red de corriente alterna en 400 kV es la alternativa recomendada: refuerza directamente Campeche y Mérida, puede construirse por etapas, utiliza tecnología consolidada en el SEN y presenta la mejor relación beneficio/costo.",
  "riesgos":[{"riesgo":"Saturación del corredor Grijalva–Tabasco","impacto":"Restricción permanente del suministro hacia la Península a partir de 2031.","mitigacion":"Desarrollar por etapas la red troncal de 400 kV."},{"riesgo":"Disponibilidad de gas del Mayakán","impacto":"Retraso de las centrales Mérida IV y Riviera Maya–Valladolid.","mitigacion":"Dar seguimiento al gasoducto y sincronizar generación con transmisión."},{"riesgo":"Reserva insuficiente y crecimiento concentrado","impacto":"Mayor exposición operativa en Mérida, Cancún y Riviera Maya.","mitigacion":"Coordinar refuerzos regionales y compensación dinámica."}],
  "alternativas":[{"nombre":"Alternativa 1","tecnologia":"Red de corriente alterna en 400 kV","inversionMdp":31293.202,"alcance":"Red troncal por etapas, derivaciones regionales y compensación dinámica.","ventaja":"Beneficios anticipados y suministro directo a Campeche y Mérida.","recomendada":true},{"nombre":"Alternativa 2","tecnologia":"Enlace bipolar de corriente directa VSC","inversionMdp":33500.791,"alcance":"Transporte punto a punto con estaciones convertidoras especializadas.","ventaja":"Menor requerimiento de derecho de vía.","recomendada":false}],
  "comparativa":[{"criterio":"Costo de inversión","alternativa1":"31,293.202 MDP","alternativa2":"33,500.791 MDP","favorable":"Alternativa 1"},{"criterio":"Operabilidad","alternativa1":"Tecnología consolidada en el SEN","alternativa2":"Operación y mantenimiento especializados","favorable":"Alternativa 1"},{"criterio":"Beneficio regional","alternativa1":"Derivaciones para Campeche y Mérida","alternativa2":"Requiere obras adicionales","favorable":"Alternativa 1"},{"criterio":"Derecho de vía","alternativa1":"Mayor ocupación en subestaciones","alternativa2":"Menor ocupación lineal","favorable":"Alternativa 2"},{"criterio":"Ejecución","alternativa1":"Construible y energizable por etapas","alternativa2":"Beneficios al término total","favorable":"Alternativa 1"}],
  "proximosPasos":["Concluir la evaluación económica de la Alternativa 2.","Dar seguimiento al gasoducto Mayakán y a las centrales Mérida IV y Riviera Maya–Valladolid.","Desarrollar la Alternativa 1 por etapas para su entrada en operación en abril de 2031.","Validar las metas físicas y conectar las capas geoespaciales institucionales."]
}',
            SUSER_SNAME()
        );

        SET @FichaId = SCOPE_IDENTITY();

        INSERT dgmesnie.PAMProyectoFichaRecurso
            (ProyectoId, FichaId, TipoRecurso, Titulo, Descripcion, Url, AltText, Orden, Aplica, UsuarioRegistro)
        VALUES
            (@ProyectoId, @FichaId, N'DiagramaUnifilar', N'Diagrama unifilar', N'PNG pendiente de integrar a la base de datos.', NULL, N'Diagrama unifilar del proyecto I26-PE1', 10, 1, SUSER_SNAME()),
            (@ProyectoId, @FichaId, N'Geoespacial', N'Mapa geoespacial', N'PNG o mapa institucional pendiente de integrar a la base de datos.', NULL, N'Mapa geoespacial del proyecto I26-PE1', 20, 1, SUSER_SNAME()),
            (@ProyectoId, @FichaId, N'C7U', N'Lámina C7U', N'Reserva para lámina C7U del proyecto.', NULL, N'Lámina C7U del proyecto I26-PE1', 30, 0, SUSER_SNAME());
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    (SELECT COUNT(*) FROM dgmesnie.PAMProyectoFicha) AS Fichas,
    (SELECT COUNT(*) FROM dgmesnie.PAMProyectoFichaRecurso) AS Recursos;
