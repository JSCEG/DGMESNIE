SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'ClasificacionAnalisis') IS NULL
    ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD ClasificacionAnalisis NVARCHAR(80) NULL;

IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'ViabilidadTecnica') IS NULL
    ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD ViabilidadTecnica NVARCHAR(40) NULL;

IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'AnalisisTecnico') IS NULL
    ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD AnalisisTecnico NVARCHAR(4000) NULL;

IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'FechaFirmaProyecto') IS NULL
    ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD FechaFirmaProyecto DATETIME2(0) NULL;

IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'GrupoDuplicado') IS NULL
    ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD GrupoDuplicado NVARCHAR(200) NULL;

COMMIT TRANSACTION;

SELECT
    COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'ClasificacionAnalisis') AS ClasificacionAnalisis,
    COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'AnalisisTecnico') AS AnalisisTecnico;
