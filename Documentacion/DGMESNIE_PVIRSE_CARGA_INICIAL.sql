SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dgmesnie.PvirseCentralesElectricas', 'U') IS NULL
BEGIN
    THROW 50000, 'La tabla dgmesnie.PvirseCentralesElectricas no existe. Ejecuta primero el script base.', 1;
END
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM dgmesnie.PvirseCentralesElectricas;

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central La Victoria', NULL, 2026, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 0.90, N'dic', N'OCC', N'38-Querétaro', N'Querétaro', N'Querétaro', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Cogeneración Industrial Papelera San Luis', NULL, 2026, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 2.00, N'dic', N'OCC', N'33-San Luis Potosí', N'San Luis Potosí', N'San Luis Potosí', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Fotovoltaico Flex', NULL, 2026, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 2.10, N'may', N'NTE', N'10-Juárez', N'Chihuahua', N'Juárez', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Ahumada II', NULL, 2026, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'may', N'NTE', N'12-Moctezuma', N'Chihuahua', N'Ahumada', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Genomma Lab', NULL, 2026, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 2.00, N'jun', N'CEL', N'43-Toluca', N'Estado de México', N'Toluca', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Parque Eólico San Carlos', NULL, 2026, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 198.00, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Santa María', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 30.00, N'jun', N'NOR', N'08-Culiacán', N'Sinaloa', N'Rosario', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Cogeneración Abasto Aislado - Malta Texco Texcoco', NULL, 2026, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 1.09, N'jun', N'CEL', N'40-Centro', N'Estado de México', N'Texcoco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U1', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'may', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Hidroeléctrica Las Juntas', NULL, 2026, N'Adición', N'AUT', N'HID', N'HID', N'Renovable', 2.91, N'jun', N'OCC', N'29-Tepic', N'Jalisco', N'Cabo Corrientes', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Sears Puebla Zaragoza', NULL, 2026, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 1.00, N'may', N'ORI', N'47-Puebla', N'Puebla', N'Puebla', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM  Mazatepec U2', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 6.00, N'may', N'ORI', N'47-Puebla', N'Puebla', N'Tlatlauquitepec', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'CCC Jorge Luque I', NULL, 2026, N'Adición', N'GEN', N'CC', N'CC', N'No renovable', 112.87, N'jun', N'CEL', N'40-Centro', N'Estado de México', N'Tultitlán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U6', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'jul', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Chicxulub I', NULL, 2026, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 70.00, N'may', N'PEN', N'67-Mérida', N'Yucatán', N'Ixil', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Santo Domingo Solar', NULL, 2026, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 5.50, N'ago', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'Comondú', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'BAT Santo Domingo Solar', NULL, 2026, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 1.90, N'ago', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'Comondú', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Angostura U1', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 19.91, N'dic', N'ORI', N'58-Grijalva', N'Chiapas', N'Venustiano Carranza', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Angostura U3', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 19.91, N'may', N'ORI', N'58-Grijalva', N'Chiapas', N'Venustiano Carranza', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Angostura U5', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 19.91, N'jul', N'ORI', N'58-Grijalva', N'Chiapas', N'Venustiano Carranza', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Incremento Generación Almex', NULL, 2026, N'Adición', N'GEN', N'TG/COG', N'BIO', N'Renovable', 28.00, N'dic', N'OCC', N'30-Guadalajara', N'Jalisco', N'Guadalajara', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC San Luis Río Colorado', NULL, 2026, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 769.70, N'may', N'BC', N'81-San Luis Río Colorado', N'Sonora', N'San Luis Río Colorado', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Central Amata', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'may', N'NOR', N'08-Culiacán', N'Sinaloa', N'Cosalá', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U3', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'may', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U4', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'may', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U5', NULL, 2026, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'nov', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'PS Aguascalientes Sur I', NULL, 2026, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 2.97, N'may', N'OCC', N'31-Aguascalientes', N'Aguascalientes', N'Aguascalientes', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Amistad IV', NULL, 2026, N'Adición', N'GEN SLP', N'EO', N'EO', N'Renovable', 150.00, N'dic', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Acuña', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM Malpaso U2', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 11.91, N'jun', N'ORI', N'58-Grijalva', N'Chiapas', N'Tecpatán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Central Hidroeléctrica Picachos', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 6.40, N'sep', N'NOR', N'09-Mazatlán', N'Sinaloa', N'Concordia', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM  Mazatepec U3', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 6.00, N'feb', N'ORI', N'47-Puebla', N'Puebla', N'Tlatlauquitepec', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Sky EPS Supply SM', NULL, 2027, N'Adición', N'GEN', N'CI', N'CI', N'No renovable', 7.19, N'mar', N'ORI', N'47-Puebla', N'Puebla', N'San Martín Texmelucan', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Los Pinos', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'ago', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Pinos', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Palma Loca', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 399.75, N'feb', N'OCC', N'28-Primero de Mayo', N'Zacatecas', N'Mazapil', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Rancho del Norte', NULL, 2027, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 250.00, N'jul', N'NES', N'20-Reynosa', N'Nuevo León', N'General Bravo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Energia para Bebidas Carbonatadas', NULL, 2027, N'Adición', N'GEN', N'CI/COG', N'BIO', N'Renovable', 5.00, N'ene', N'OCC', N'38-Querétaro', N'Querétaro', N'San Juan del Río', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Fotovoltaica Flex Guadalajara', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 0.96, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Zapopan', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Rehabilitación y modernización C.H. Ing. Carlos Ramírez Ulloa "El Caracol" U1', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.48, N'mar', N'ORI', N'50-Acapulco', N'Guerrero', N'General Heliodoro Castillo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Rehabilitación y modernización C.H. Ing. Carlos Ramírez Ulloa "El Caracol" U2', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.48, N'sep', N'ORI', N'50-Acapulco', N'Guerrero', N'General Heliodoro Castillo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Tizimin II', NULL, 2027, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 75.60, N'jun', N'PEN', N'69-Valladolid', N'Yucatán', N'Tizimín', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'RM CH Portezuelos I y II', NULL, 2027, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.12, N'sep', N'ORI', N'47-Puebla', N'Puebla', N'Ocoyucan', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'RMSC Comercio', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Altair', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Alaia V', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Alaia IV', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Alaia III', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Alaia II', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'feb', N'NTE', N'10-Juárez', N'Chihuahua', N'Ascensión', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'FV El Toro', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 90.00, N'dic', N'OCC', N'34-Salamanca', N'Guanajuato', N'Manuel Doblado', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT El Toro', NULL, 2027, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 29.70, N'dic', N'OCC', N'34-Salamanca', N'Guanajuato', N'Manuel Doblado', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'FV Tecozautla', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 100.00, N'dic', N'OCC', N'38-Querétaro', N'Hidalgo', N'Tecozautla', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Tecozautla', NULL, 2027, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 31.40, N'dic', N'OCC', N'38-Querétaro', N'Hidalgo', N'Tecozautla', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Energía Solar Herrera', NULL, 2027, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 200.00, N'dic', N'ORI', N'47-Puebla', N'Puebla', N'Tecali de Herrera', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Energía Solar Herrera', NULL, 2027, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 60.30, N'dic', N'ORI', N'47-Puebla', N'Puebla', N'Tecali de Herrera', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Don Humberto', NULL, 2027, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 19.20, N'dic', N'NOR', N'08-Culiacán', N'Sinaloa', N'Culiacán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Don Humberto', NULL, 2027, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 5.80, N'dic', N'NOR', N'08-Culiacán', N'Sinaloa', N'Culiacán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Selka Power Plant 1', NULL, 2027, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 131.10, N'dic', N'OCC', N'32-León', N'Guanajuato', N'Romita', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Selka Power Plant 1', NULL, 2027, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.20, N'dic', N'OCC', N'32-León', N'Guanajuato', N'Romita', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Cogeneración Energía Eléctrica - TRE / El Mante', NULL, 2027, N'Adición', N'GEN', N'CI/BIO', N'BIO', N'Renovable', 44.00, N'jun', N'NES', N'25-Huasteca', N'Tamaulipas', N'El Mante', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CCC Lerdo', NULL, 2028, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 350.00, N'jun', N'NTE', N'17-Laguna', N'Durango', N'Lerdo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Campeche Energy', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 80.00, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Carmen', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'BAT Campeche Energy', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 23.69, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Carmen', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Planta Eléctrica Ingenio La Joya', NULL, 2028, N'Adición', N'GEN', N'CI/BIO', N'BIO', N'Renovable', 27.00, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Viento de Bella Unión', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 120.00, N'dic', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'Arteaga', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Viento de Bella Unión', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 36.00, N'dic', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'Arteaga', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Ciénega de Mata (Central Jalisco)', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 200.00, N'abr', N'OCC', N'31-Aguascalientes', N'Jalisco', N'Lagos de Moreno y Ojuelos de Jalisco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Parque Solar Villanueva MP', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 150.00, N'abr', N'NTE', N'17-Laguna', N'Coahuila de Zaragoza', N'Viesca', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Parque Eólico Parras', NULL, 2028, N'Adición', N'AUT', N'EO', N'EO', N'Renovable', 50.00, N'jul', N'NTE', N'17-Laguna', N'Coahuila de Zaragoza', N'Parras', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Chicoasén II', NULL, 2028, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 236.50, N'jul', N'ORI', N'58-Grijalva', N'Chiapas', N'Chicoasén', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Ak Kin Green Power Park', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 100.00, N'ene', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'Saltillo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PE Delaro', NULL, 2028, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 117.00, N'dic', N'NES', N'20-Reynosa', N'Tamaulipas', N'Reynosa', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PE Delaro', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 35.10, N'dic', N'NES', N'20-Reynosa', N'Tamaulipas', N'Reynosa', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Proyecto Geotermoeléctrico Celaya', NULL, 2028, N'Adición', N'GEN', N'GEO', N'GEO', N'Renovable', 56.00, N'abr', N'OCC', N'38-Querétaro', N'Guanajuato', N'Apaseo El Grande', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Chicxulub II', NULL, 2028, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 86.00, N'ene', N'PEN', N'67-Mérida', N'Yucatán', N'Ixil', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Yucatán Solar', NULL, 2028, N'Adición', N'GEN SLP', N'FV', N'FV', N'Renovable', 70.00, N'ene', N'PEN', N'69-Valladolid', N'Yucatán', N'Valladolid', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Pachamama II', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 330.00, N'dic', N'ORI', N'47-Puebla', N'Puebla', N'Tepeyahualco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Kabil I', NULL, 2028, N'Adición', N'GEN SLP', N'EO', N'EO', N'Renovable', 30.00, N'ene', N'PEN', N'69-Valladolid', N'Yucatán', N'Buctzotz', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Kabil II', NULL, 2028, N'Adición', N'GEN SLP', N'EO', N'EO', N'Renovable', 30.00, N'ene', N'PEN', N'69-Valladolid', N'Yucatán', N'Buctzotz', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'C.F.V. Puerto Peñasco Secuencia III', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 300.00, N'ene', N'BC', N'82-Puerto Peñasco', N'Sonora', N'Puerto Peñasco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'BAT C.F.V. Puerto Peñasco Secuencia III', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'ene', N'BC', N'82-Puerto Peñasco', N'Sonora', N'Puerto Peñasco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'C.F.V. Puerto Peñasco Secuencia IV', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 280.00, N'jun', N'BC', N'82-Puerto Peñasco', N'Sonora', N'Puerto Peñasco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'BAT C.F.V. Puerto Peñasco Secuencia IV', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 84.00, N'jun', N'BC', N'82-Puerto Peñasco', N'Sonora', N'Puerto Peñasco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC Francisco Pérez Ríos', NULL, 2028, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 1013.80, N'dic', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'Tula de Allende', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC Altamira', NULL, 2028, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 583.10, N'dic', N'NES', N'25-Huasteca', N'Tamaulipas', N'Altamira', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC Mazatlán', NULL, 2028, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 581.50, N'jun', N'NOR', N'09-Mazatlán', N'Sinaloa', N'Mazatlán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC Salamanca II', NULL, 2028, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 495.50, N'jun', N'OCC', N'34-Salamanca', N'Guanajuato', N'Salamanca', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'CFV. Ing. Concepción Mendizábal Mendoza (Fase I)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 376.00, N'jun', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'BAT CFV. Ing. Concepción Mendizábal Mendoza (Fase I)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 112.80, N'jun', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Eolica de Guanajuato', NULL, 2028, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 63.00, N'oct', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Eolica de Guanajuato', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 18.90, N'oct', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'CFV Los Girasoles', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 110.00, N'nov', N'PEN', N'67-Mérida', N'Quintana Roo', N'José María Morelos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT CFV Los Girasoles', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 33.00, N'nov', N'PEN', N'67-Mérida', N'Quintana Roo', N'José María Morelos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'San Simón', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 80.00, N'abr', N'BC', N'79-Ensenada', N'Baja California', N'San Quintín', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT San Simón', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 27.20, N'abr', N'BC', N'79-Ensenada', N'Baja California', N'San Quintín', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Solar San Jose del Cabo', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 30.00, N'mar', N'BCS', N'95-San José del Cabo', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Solar San Jose del Cabo', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 9.00, N'mar', N'BCS', N'95-San José del Cabo', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Solar Energía Tres Hermanos', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 75.00, N'dic', N'ORI', N'47-Puebla', N'Tlaxcala', N'Huamantla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Solar Energía Tres Hermanos', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 22.50, N'dic', N'ORI', N'47-Puebla', N'Tlaxcala', N'Huamantla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Tampico Solar', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 105.60, N'jun', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'Doctor Mora', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Tampico Solar', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 31.68, N'jun', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'Doctor Mora', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Central Fotovoltaica CGS', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 89.00, N'nov', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Ojocaliente', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Central Fotovoltaica CGS', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 29.70, N'nov', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Ojocaliente', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Vientos del Caribe Hybrid', NULL, 2028, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 200.00, N'dic', N'PEN', N'76-Chetumal', N'Quintana Roo', N'Othón P. Blanco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Vientos del Caribe Hybrid', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 81.70, N'dic', N'PEN', N'76-Chetumal', N'Quintana Roo', N'Othón P. Blanco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Parque Eolico El 24', NULL, 2028, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 130.00, N'dic', N'NES', N'19-Nuevo Laredo', N'Tamaulipas', N'Mier', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Parque Eolico El 24', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 54.20, N'dic', N'NES', N'19-Nuevo Laredo', N'Tamaulipas', N'Mier', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'La Esperanza Solar', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 300.00, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Escárcega', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT La Esperanza Solar', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 156.70, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Escárcega', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'La Alegría Solar', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 600.00, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Campeche', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT La Alegría Solar', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 313.50, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Campeche', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'PP. EE. Panabá 1', NULL, 2028, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 250.00, N'dic', N'PEN', N'68-Dzitnup', N'Yucatán', N'Panabá', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT PP. EE. Panabá 1', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 102.20, N'dic', N'PEN', N'68-Dzitnup', N'Yucatán', N'Panabá', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'PV Tamesi Solar', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 110.00, N'abr', N'NES', N'25-Huasteca', N'Tamaulipas', N'Altamira', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT PV Tamesi Solar', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 36.20, N'abr', N'NES', N'25-Huasteca', N'Tamaulipas', N'Altamira', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Parque FV Energias Renovables de Tamaulipas', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 78.00, N'nov', N'NES', N'25-Huasteca', N'Tamaulipas', N'Altamira', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Parque FV Energias Renovables de Tamaulipas', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 25.70, N'nov', N'NES', N'25-Huasteca', N'Tamaulipas', N'Altamira', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Planta Fotovoltaica Alten Hidalgo 100 MW', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 100.00, N'jun', N'OCC', N'38-Querétaro', N'Hidalgo', N'Nopala de Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Planta Fotovoltaica Alten Hidalgo 100 MW', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 31.40, N'jun', N'OCC', N'38-Querétaro', N'Hidalgo', N'Nopala de Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Global Solar 2 Hidalgo 100 MW', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 92.00, N'may', N'OCC', N'38-Querétaro', N'Hidalgo', N'Nopala de Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Global Solar 2 Hidalgo 100 MW', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 28.90, N'may', N'OCC', N'38-Querétaro', N'Hidalgo', N'Nopala de Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Proyecto Solar Piedras Negras', NULL, 2028, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 120.00, N'jun', N'ORI', N'46-Veracruz', N'Veracruz de Ignacio de la Llave', N'Tlalixcoyan', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Proyecto Solar Piedras Negras', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 36.20, N'jun', N'ORI', N'46-Veracruz', N'Veracruz de Ignacio de la Llave', N'Tlalixcoyan', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Zapoteca de Energía', NULL, 2028, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 200.00, N'dic', N'ORI', N'61-Juchitán', N'Oaxaca', N'Juchitán de Zaragoza', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Zapoteca de Energía', NULL, 2028, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 69.90, N'dic', N'ORI', N'61-Juchitán', N'Oaxaca', N'Juchitán de Zaragoza', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'LA SAUCEDA SOLAR', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 90.00, N'ago', N'OCC', N'33-San Luis Potosí', N'Guanajuato', N'San Diego de la Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT LA SAUCEDA SOLAR', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 27.00, N'ago', N'OCC', N'33-San Luis Potosí', N'Guanajuato', N'San Diego de la Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Sol de Sonora 70', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 70.00, N'abr', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Sol de Sonora 70', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 22.37, N'abr', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PV Moquel Solar', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 75.00, N'jul', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PV Moquel Solar', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 22.89, N'jul', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Kumeyaay Energy Center', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 38.00, N'mar', N'BC', N'80-Mexicali', N'Baja California', N'Tecate', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Kumeyaay Energy Center', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 45.00, N'mar', N'BC', N'80-Mexicali', N'Baja California', N'Tecate', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'CFV Quasara', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 150.00, N'jul', N'BC', N'80-Mexicali', N'Baja California', N'Mexicali', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT CFV Quasara', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 65.00, N'jul', N'BC', N'80-Mexicali', N'Baja California', N'Mexicali', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Solitario', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 200.00, N'abr', N'BC', N'81-San Luis Río Colorado', N'Baja California', N'San Luis Río Colorado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Solitario', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 71.00, N'abr', N'BC', N'81-San Luis Río Colorado', N'Baja California', N'San Luis Río Colorado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Santo', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'jul', N'BC', N'81-San Luis Río Colorado', N'Baja California', N'San Luis Río Colorado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Santo', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'jul', N'BC', N'81-San Luis Río Colorado', N'Baja California', N'San Luis Río Colorado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Ica - Punta Colonet', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 120.00, N'oct', N'BC', N'79-Ensenada', N'Baja California', N'Ensenada', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Ica - Punta Colonet', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 36.00, N'oct', N'BC', N'79-Ensenada', N'Baja California', N'Ensenada', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'La Pasion', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 66.00, N'jun', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'La Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT La Pasion', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 16.50, N'jun', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'La Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'El Palmar', N'1', 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 105.60, N'dic', N'BCS', N'96-El Palmar', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT El Palmar', N'1', 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 31.70, N'dic', N'BCS', N'96-El Palmar', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Solar Los Cabos', N'1', 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'may', N'BCS', N'97-Central Los Cabos', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Solar Los Cabos', N'1', 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'may', N'BCS', N'97-Central Los Cabos', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Álvaro Obregón', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 90.00, N'dic', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'La Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Álvaro Obregón', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 27.00, N'dic', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'La Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Cimarron Solar S de RL de CV', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 300.00, N'sep', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Cimarron Solar S de RL de CV', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'sep', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Tamaulipas', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 70.00, N'ene', N'NES', N'25-Huasteca', N'Tamaulipas', N'González', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Tamaulipas', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 21.00, N'ene', N'NES', N'25-Huasteca', N'Tamaulipas', N'González', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Ica Saltillo 2', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 200.00, N'ago', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'Ramos Arizpe', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Ica Saltillo 2', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 60.00, N'ago', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'Ramos Arizpe', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Proyecto registrado sin nombre', NULL, 2028, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 300.00, N'may', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Villa Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Proyecto registrado sin nombre', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'may', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Villa Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'CFV Ranchos La Crisis y La Noria, Linares N.L.', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 455.00, N'mar', N'NES', N'22-Monterrey', N'Nuevo León', N'Linares', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT CFV Ranchos La Crisis y La Noria, Linares N.L.', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 136.50, N'mar', N'NES', N'22-Monterrey', N'Nuevo León', N'Linares', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Amatitan', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 40.40, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Amatitán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Amatitan', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 12.10, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Amatitán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'La Pareja', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 17.90, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Talpa de Allende', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT La Pareja', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 5.40, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Talpa de Allende', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Las Jicamas', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 20.70, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Tlajomulco de Zúñiga', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Las Jicamas', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 6.20, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Tlajomulco de Zúñiga', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Lagos Norte', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 130.00, N'ene', N'OCC', N'32-León', N'Jalisco', N'Lagos de Moreno', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Lagos Norte', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 39.00, N'ene', N'OCC', N'32-León', N'Jalisco', N'Lagos de Moreno', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Acatlan Generacion', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 118.50, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Acatlán de Juárez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Acatlan Generacion', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 35.50, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'Acatlán de Juárez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Fresnillo (PV Atocha)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'oct', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Fresnillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Fresnillo (PV Atocha)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'oct', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Fresnillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'San Pedro Solar', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 99.00, N'mar', N'OCC', N'38-Querétaro', N'Querétaro', N'Huimilpan', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT San Pedro Solar', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 29.70, N'mar', N'OCC', N'38-Querétaro', N'Querétaro', N'Huimilpan', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Proyecto Solar Arco y Donhi', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 84.00, N'dic', N'OCC', N'38-Querétaro', N'Hidalgo', N'Huichapan', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Proyecto Solar Arco y Donhi', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 25.20, N'dic', N'OCC', N'38-Querétaro', N'Hidalgo', N'Huichapan', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Promoción Renovable del Bajío', NULL, 2028, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 88.50, N'dic', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Promoción Renovable del Bajío', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 26.60, N'dic', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Fotovoltaico San Juan Solar', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 120.00, N'dic', N'OCC', N'38-Querétaro', N'Querétaro', N'San Juan del Río', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Fotovoltaico San Juan Solar', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 36.00, N'dic', N'OCC', N'38-Querétaro', N'Querétaro', N'San Juan del Río', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Papantla', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 150.00, N'nov', N'ORI', N'45-Poza Rica', N'Veracruz de Ignacio de la Llave', N'Papantla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Papantla', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 45.00, N'nov', N'ORI', N'45-Poza Rica', N'Veracruz de Ignacio de la Llave', N'Papantla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'El Guayabo', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 299.10, N'jun', N'ORI', N'46-Veracruz', N'Veracruz de Ignacio de la Llave', N'Alvarado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT El Guayabo', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 89.70, N'jun', N'ORI', N'46-Veracruz', N'Veracruz de Ignacio de la Llave', N'Alvarado', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Planta Solar Cerro Colorado', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 455.70, N'dic', N'ORI', N'56-Juile', N'Oaxaca', N'Sayula de Alemán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Planta Solar Cerro Colorado', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 136.70, N'dic', N'ORI', N'56-Juile', N'Oaxaca', N'Sayula de Alemán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Global Solar 3 Campeche 100 MW', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 125.00, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Carmen', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Global Solar 3 Campeche 100 MW', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.50, N'jun', N'PEN', N'64-Escárcega', N'Campeche', N'Carmen', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque FV Energias Renovables de Mexico Tres (Hecelchakan)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 72.00, N'oct', N'PEN', N'66-Lerma', N'Campeche', N'Hecelchakán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque FV Energias Renovables de Mexico Tres (Hecelchakan)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 21.60, N'oct', N'PEN', N'66-Lerma', N'Campeche', N'Hecelchakán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Villas Cancún', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 150.00, N'nov', N'PEN', N'75-Cancún', N'Quintana Roo', N'Benito Juárez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Villas Cancún', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 45.00, N'nov', N'PEN', N'75-Cancún', N'Quintana Roo', N'Benito Juárez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Dalia 3 (PV Castamay)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'oct', N'PEN', N'66-Lerma', N'Campeche', N'Campeche', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Dalia 3 (PV Castamay)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.50, N'oct', N'PEN', N'66-Lerma', N'Campeche', N'Campeche', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque FV Energias Renovables Saas (Peto)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 72.00, N'ago', N'PEN', N'67-Mérida', N'Yucatán', N'Peto', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque FV Energias Renovables Saas (Peto)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 21.60, N'ago', N'PEN', N'67-Mérida', N'Yucatán', N'Peto', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Planta Solar Haab Kiin', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 123.20, N'jun', N'PEN', N'67-Mérida', N'Yucatán', N'Umán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Planta Solar Haab Kiin', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 37.00, N'jun', N'PEN', N'67-Mérida', N'Yucatán', N'Umán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Tikinmul', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 193.60, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Campeche', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Tikinmul', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 58.10, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Campeche', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PFV El Faisán', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 150.00, N'dic', N'PEN', N'64-Escárcega', N'Campeche', N'Carmen', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PFV El Faisán', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 45.00, N'dic', N'PEN', N'64-Escárcega', N'Campeche', N'Carmen', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque FV Energias Renovables Kiin (Tekax)', NULL, 2028, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 56.00, N'ago', N'PEN', N'67-Mérida', N'Yucatán', N'Tekax', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque FV Energias Renovables Kiin (Tekax)', NULL, 2028, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 16.80, N'ago', N'PEN', N'67-Mérida', N'Yucatán', N'Tekax', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Tula, U3', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 302.00, N'dic', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Tula, U4', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 302.00, N'dic', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Tula, U5', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 281.00, N'dic', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Altamira, U3', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 229.00, N'dic', N'NES', N'25-Huasteca', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Altamira, U4', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 227.00, N'dic', N'NES', N'25-Huasteca', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Mazatlán II, U1', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 146.00, N'jun', N'NOR', N'09-Mazatlán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.C. Mazatlán II, U2', NULL, 2028, N'Sustitución', N'GEN CFE', N'TC', N'TC', N'No renovable', 144.00, N'jun', N'NOR', N'09-Mazatlán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'Eolica Dzilam', NULL, 2029, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 119.00, N'jul', N'PEN', N'69-Valladolid', N'Yucatán', N'Dzilam González', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (VUPE)', N'Particulares', N'BAT Eolica Dzilam', NULL, 2029, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 48.60, N'jul', N'PEN', N'69-Valladolid', N'Yucatán', N'Dzilam González', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Cogeneración Cangrejera', NULL, 2029, N'Adición', N'GEN CFE', N'CC/COG_EF', N'CC/COG_EF', N'No renovable', 900.00, N'abr', N'ORI', N'57-Coatzacoalcos', N'Veracruz de Ignacio de la Llave', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Hidrógeno de Francia', NULL, 2029, N'Adición', N'GEN CFE', N'H2', N'H2', N'Renovable', 13.00, N'may', N'BCS', N'94-Santiago', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PE Energeo Los Molinos', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 171.00, N'dic', N'NES', N'21-Matamoros', N'Tamaulipas', N'Matamoros', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PE Energeo Los Molinos', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 67.56, N'dic', N'NES', N'21-Matamoros', N'Tamaulipas', N'Matamoros', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'CFV Las Garzas', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 270.00, N'feb', N'NTE', N'17-Laguna', N'Durango', N'Lerdo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT CFV Las Garzas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 81.00, N'feb', N'NTE', N'17-Laguna', N'Durango', N'Lerdo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'BAT Las Pilas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 50.00, N'dic', N'BCS', N'88-Las Pilas', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CFV Olas Altas', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'dic', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'BAT CFV Olas Altas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'dic', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Termosolar Olas Altas', NULL, 2029, N'Adición', N'GEN CFE', N'CSP', N'CSP', N'Renovable', 100.00, N'dic', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'CFV. Ing. Concepción Mendizábal Mendoza (Fase II)', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 180.00, N'ene', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'BAT CFV. Ing. Concepción Mendizábal Mendoza (Fase II)', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 54.00, N'ene', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'CFV. Ing. Concepción Mendizábal Mendoza (Fase III)', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 302.00, N'jun', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'BAT CFV. Ing. Concepción Mendizábal Mendoza (Fase III)', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.60, N'jun', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Nava', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'CFV Cerro Prieto II', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 215.00, N'may', N'BC', N'80-Mexicali', N'Baja California', N'Mexicali', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto Específico', N'CFE', N'BAT CFV Cerro Prieto II', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 64.50, N'may', N'BC', N'80-Mexicali', N'Baja California', N'Mexicali', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PE Montecristo', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 510.00, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Méndez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PE Montecristo', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 153.00, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Méndez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Eolico Virgen De Los Zacatecas', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 171.00, N'sep', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Vetagrande', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Eolico Virgen De Los Zacatecas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 41.38, N'sep', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Vetagrande', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Ps Perote', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 50.00, N'abr', N'ORI', N'47-Puebla', N'Veracruz de Ignacio de la Llave', N'Perote', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Ps Perote', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 15.00, N'abr', N'ORI', N'47-Puebla', N'Veracruz de Ignacio de la Llave', N'Perote', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PARQUE SOLAR SAN CAYETANO', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 90.00, N'jul', N'OCC', N'31-Aguascalientes', N'Jalisco', N'Lagos de Moreno', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PARQUE SOLAR SAN CAYETANO', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 27.00, N'jul', N'OCC', N'31-Aguascalientes', N'Jalisco', N'Lagos de Moreno', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Loreto Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 20.00, N'ene', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'Loreto', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Loreto Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 6.00, N'ene', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'Loreto', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'La Poza Solar S de RL de CV', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 300.00, N'dic', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT La Poza Solar S de RL de CV', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'dic', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Sunora S de RL de CV', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 300.00, N'ago', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Sunora S de RL de CV', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'ago', N'NOR', N'04-Hermosillo', N'Sonora', N'Hermosillo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PE Sabinas', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 117.00, N'dic', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Villa Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PE Sabinas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 35.10, N'dic', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'Villa Unión', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Energia Limpia El Mezquite', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 300.00, N'may', N'NES', N'22-Monterrey', N'Nuevo León', N'Mina', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Energia Limpia El Mezquite', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 90.00, N'may', N'NES', N'22-Monterrey', N'Nuevo León', N'Mina', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Atria Wind Farm II_Alternativa', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 259.00, N'abr', N'NES', N'22-Monterrey', N'Nuevo León', N'General Bravo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Atria Wind Farm II_Alternativa', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 81.90, N'abr', N'NES', N'22-Monterrey', N'Nuevo León', N'General Bravo', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'El Guajillo', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 468.00, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Reynosa', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT El Guajillo', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 140.40, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Reynosa', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Santa Gertrudis', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 503.00, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Méndez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Santa Gertrudis', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 150.90, N'dic', N'NES', N'22-Monterrey', N'Tamaulipas', N'Méndez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Eólico de Euronotus', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 400.50, N'sep', N'NES', N'22-Monterrey', N'Nuevo León', N'China', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Eólico de Euronotus', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 120.20, N'sep', N'NES', N'22-Monterrey', N'Nuevo León', N'China', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Rancho Grande', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 580.00, N'sep', N'NES', N'27-Güémez', N'Tamaulipas', N'San Fernando', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Rancho Grande', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 174.00, N'sep', N'NES', N'27-Güémez', N'Tamaulipas', N'San Fernando', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PE Los Huizaches', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 580.00, N'nov', N'NES', N'27-Güémez', N'Tamaulipas', N'Villagrán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PE Los Huizaches', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 174.00, N'nov', N'NES', N'27-Güémez', N'Tamaulipas', N'Villagrán', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Reysol Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 70.00, N'sep', N'NES', N'20-Reynosa', N'Tamaulipas', N'Reynosa', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Reysol Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 21.00, N'sep', N'NES', N'20-Reynosa', N'Tamaulipas', N'Reynosa', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'El Chorro - Eolico 705 MWp', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 580.00, N'dic', N'NES', N'27-Güémez', N'Tamaulipas', N'Güémez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT El Chorro - Eolico 705 MWp', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 174.00, N'dic', N'NES', N'27-Güémez', N'Tamaulipas', N'Güémez', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Sol de Zacatecas', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 60.00, N'jun', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Villa de Cos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Sol de Zacatecas', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 18.00, N'jun', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'Villa de Cos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Proyecto Solar Los Nogales', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 64.00, N'feb', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Proyecto Solar Los Nogales', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 18.30, N'feb', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'San Luis de la Paz', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Estrada', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 88.00, N'nov', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'General Enrique Estrada', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Estrada', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 26.40, N'nov', N'OCC', N'31-Aguascalientes', N'Zacatecas', N'General Enrique Estrada', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Solar Tabasco', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 100.00, N'ago', N'ORI', N'59-Tabasco', N'Tabasco', N'Centro', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Solar Tabasco', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'ago', N'ORI', N'59-Tabasco', N'Tabasco', N'Centro', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Energias Renovables Tuxtla', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 90.00, N'dic', N'ORI', N'57-Coatzacoalcos', N'Veracruz de Ignacio de la Llave', N'Santiago Tuxtla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Energias Renovables Tuxtla', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 27.00, N'dic', N'ORI', N'57-Coatzacoalcos', N'Veracruz de Ignacio de la Llave', N'Santiago Tuxtla', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Alten Siete', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 250.00, N'ene', N'ORI', N'47-Puebla', N'Puebla', N'Tepeyahualco', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Alten Siete', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 75.00, N'ene', N'ORI', N'47-Puebla', N'Puebla', N'Tepeyahualco', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Ps Tenexac', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 99.90, N'dic', N'ORI', N'47-Puebla', N'Tlaxcala', N'Terrenate', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Ps Tenexac', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.00, N'dic', N'ORI', N'47-Puebla', N'Tlaxcala', N'Terrenate', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'PFV Sandom Solar', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 54.00, N'jul', N'PEN', N'66-Lerma', N'Campeche', N'Hopelchén', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT PFV Sandom Solar', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 16.20, N'jul', N'PEN', N'66-Lerma', N'Campeche', N'Hopelchén', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Parque Eolico Champoton (Marengo)', NULL, 2029, N'Adición', N'GEN CFE', N'EO', N'EO', N'Renovable', 102.00, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Parque Eolico Champoton (Marengo)', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 30.60, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Proyecto registrado sin nombre', NULL, 2029, N'Adición', N'GEN CFE', N'H2', N'H2', N'Renovable', 5.00, N'may', N'MUL', N'99-Mulegé', N'Baja California Sur', N'Mulegé', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'El Palmar Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 30.00, N'sep', N'BCS', N'96-El Palmar', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT El Palmar Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 9.00, N'sep', N'BCS', N'96-El Palmar', N'Baja California Sur', N'Los Cabos', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'Champotón Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'FV', N'FV', N'Renovable', 70.00, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE-Mixto', N'CFE', N'BAT Champotón Energy Center', NULL, 2029, N'Adición', N'GEN CFE', N'BAT', N'BAT', N'Baterías', 21.00, N'dic', N'PEN', N'66-Lerma', N'Campeche', N'Champotón', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CC Guadalajara', NULL, 2030, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 500.00, N'may', N'OCC', N'30-Guadalajara', N'Jalisco', N'Guadalajara', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Cogeneración Tula', NULL, 2030, N'Adición', N'GEN CFE', N'CC/COG_EF', N'CC/COG_EF', N'No renovable', 734.00, N'abr', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Rebombeo Necaxa Fase 1', NULL, 2030, N'Adición', N'GEN', N'BAT', N'BAT', N'Renovable', 900.00, N'abr', N'CEL', N'42-Tula-Pachuca', N'Puebla', N'Huauchinango', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Parque Solar La Magdalena', NULL, 2031, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 500.00, N'abr', N'ORI', N'47-Puebla', N'Tlaxcala', N'Tlaxco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'BAT Parque Solar La Magdalena', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 150.00, N'abr', N'ORI', N'47-Puebla', N'Tlaxcala', N'Tlaxco', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Eólica De Guadalupe', NULL, 2031, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 300.00, N'jul', N'NES', N'22-Monterrey', N'Tamaulipas', N'Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'BAT Eólica De Guadalupe', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 90.00, N'jul', N'NES', N'22-Monterrey', N'Tamaulipas', N'Villagrán', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Sol de Los Manzanos', NULL, 2031, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 30.00, N'ene', N'NTE', N'13-Cuauhtémoc', N'Chihuahua', N'Cuauhtémoc', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'BAT Sol de Los Manzanos', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 9.00, N'ene', N'NTE', N'13-Cuauhtémoc', N'Chihuahua', N'Cuauhtémoc', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Termosolar Villa Constitución', NULL, 2031, N'Adición', N'GEN CFE', N'CSP', N'CSP', N'Renovable', 50.00, N'ene', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Aprovechamiento Cutzamala)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 3.00, N'ene', N'CEL', N'40-Centro', N'Estado de México', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Sanalona U1)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.50, N'ene', N'NOR', N'06-Obregón', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Camilo Arriaga U1)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 0.90, N'ene', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Comedero U1)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.50, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Oviachic U1)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.90, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Sanalona U2)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.50, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Juan Sabines)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'ene', N'ORI', N'58-Grijalva', N'Chiapas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Ampliación Cecilio del Valle)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.00, N'ene', N'ORI', N'58-Grijalva', N'Chiapas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Vicente Guerrero )', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'ene', N'ORI', N'50-Acapulco', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Camilo Arriaga U2)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 0.90, N'ene', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Electroquímica)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.90, N'ene', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Presidente Benito Juárez)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 12.00, N'ene', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Comedero U2)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.50, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Oviachic U2)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.90, N'ene', N'NOR', N'06-Obregón', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Mocuzari)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 1.20, N'ene', N'NOR', N'06-Obregón', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (RM Micos)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 0.90, N'ene', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Cerro de Oro Presa CONAGUA U1)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 15.00, N'ene', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Equip. Josefa Ortíz De Domínguez)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Pico de Águila)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 3.00, N'ene', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Angostura )', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'ene', N'NOR', N'04-Hermosillo', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Las Adjuntas)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 15.00, N'ene', N'NES', N'27-Güémez', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Cerro de Oro Presa CONAGUA U2)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 15.00, N'ene', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Eustaquio Buelna)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 15.00, N'ene', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Francisco Zarco)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 10.00, N'ene', N'NTE', N'16-Durango', N'Durango', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (Rosetilla)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 3.00, N'ene', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (La amistad U3)', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 12.00, N'ene', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización (CH Santa Rosa II (Amuchiltite))', NULL, 2031, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 7.50, N'ene', N'OCC', N'30-Guadalajara', N'Jalisco', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 3.00, N'abr', N'MUL', N'99-Mulegé', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 16.00, N'abr', N'PEN', N'64-Escárcega', N'Campeche', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 11.00, N'abr', N'NTE', N'17-Laguna', N'Durango', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 41.00, N'abr', N'NES', N'21-Matamoros', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 34.00, N'abr', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 2.00, N'abr', N'ORI', N'57-Coatzacoalcos', N'Veracruz de Ignacio de la Llave', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 4.00, N'abr', N'NES', N'26-Tamazunchale', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 3.00, N'abr', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 14.00, N'abr', N'ORI', N'50-Acapulco', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1028.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1079.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2031, N'Adición', N'GEN CFE', N'GEO', N'GEO', N'Renovable', 25.00, N'abr', N'ORI', N'47-Puebla', N'Puebla', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 16.00, N'abr', N'BC', N'79-Ensenada', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2031, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 549.00, N'abr', N'BC', N'79-Ensenada', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2031, N'Adición', N'GEN CFE', N'GEO', N'GEO', N'Renovable', 13.50, N'abr', N'BC', N'80-Mexicali', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2031, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 25.00, N'abr', N'MUL', N'99-Mulegé', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 200.00, N'ene', N'BC', N'80-Mexicali', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 60.00, N'ene', N'BC', N'80-Mexicali', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 3.00, N'abr', N'CEL', N'40-Centro', N'Estado de México', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 27.00, N'abr', N'NOR', N'08-Culiacán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 45.00, N'abr', N'CEL', N'41-Donato Guerra', N'Estado de México', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 29.00, N'abr', N'PEN', N'64-Escárcega', N'Campeche', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 6.00, N'abr', N'NTE', N'11-Nuevo Casas Grandes', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 5.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 9.00, N'abr', N'ORI', N'52-Pinotepa', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 250.00, N'abr', N'CEL', N'40-Centro', N'Estado de México', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2032, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 935.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2032, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 480.00, N'abr', N'NTE', N'10-Juárez', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'T.G. Ciudad Constitución', NULL, 2032, N'Sustitución', N'GEN CFE', N'TG', N'TG', N'No renovable', 33.00, N'ene', N'BCS', N'86-Villa Constitución', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Eólica Gunna Sicarú I', NULL, 2033, N'Adición', N'GEN SLP', N'EO', N'EO', N'Renovable', 252.00, N'dic', N'ORI', N'60-Ixtepec', N'Oaxaca', N'Heroica Ciudad de Juchitán de Zaragoza y Unión Hidalgo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'PRIVADO (firme)', N'Particulares', N'Central Eólica Gunna Sicarú II', NULL, 2033, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 48.00, N'dic', N'ORI', N'60-Ixtepec', N'Oaxaca', N'Heroica Ciudad de Juchitán de Zaragoza y Unión Hidalgo', N'*', N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Helax', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 600.00, N'ene', N'ORI', N'60-Ixtepec', N'Oaxaca', N'Ixtepec', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'Helax', NULL, 2033, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 450.00, N'ene', N'ORI', N'60-Ixtepec', N'Oaxaca', N'Ixtepec', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Estratégicos', N'Particulares', N'BAT Helax', NULL, 2033, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 315.00, N'ene', N'ORI', N'60-Ixtepec', N'Oaxaca', N'Ixtepec', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 800.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 200.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 713.00, N'abr', N'ORI', N'47-Puebla', N'Puebla', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 250.00, N'abr', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 180.00, N'abr', N'NES', N'25-Huasteca', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 120.00, N'abr', N'ORI', N'48-Morelos', N'Morelos', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 140.00, N'abr', N'NES', N'27-Güémez', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2033, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 150.00, N'abr', N'CEL', N'39-Lázaro Cárdenas', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2033, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 580.00, N'abr', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2033, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 462.00, N'abr', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 100.00, N'abr', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2033, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 30.00, N'ene', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 900.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 28.00, N'abr', N'OCC', N'31-Aguascalientes', N'Aguascalientes', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 26.00, N'abr', N'PEN', N'68-Dzitnup', N'Yucatán', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 56.00, N'abr', N'NOR', N'04-Hermosillo', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1104.00, N'abr', N'OCC', N'34-Salamanca', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 57.00, N'abr', N'ORI', N'50-Acapulco', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 200.00, N'abr', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2034, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 60.00, N'abr', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2034, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 477.00, N'abr', N'NTE', N'17-Laguna', N'Durango', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2034, N'Adición', N'GEN CFE', N'CSP', N'CSP', N'Renovable', 50.00, N'ene', N'BCS', N'87-Puerto San Carlos', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1000.00, N'abr', N'OCC', N'34-Salamanca', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 79.00, N'abr', N'OCC', N'37-San Luis de la Paz', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 4.00, N'abr', N'PEN', N'68-Dzitnup', N'Yucatán', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 23.00, N'abr', N'ORI', N'50-Acapulco', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 1187.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 441.00, N'abr', N'CEL', N'42-Tula-Pachuca', N'Hidalgo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 250.00, N'abr', N'PEN', N'69-Valladolid', N'Yucatán', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 200.00, N'abr', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2035, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 60.00, N'ene', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2035, N'Adición', N'GEN CFE', N'CSP', N'CSP', N'Renovable', 100.00, N'ene', N'BCS', N'89-Olas Altas', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'C.C.I. San Carlos, U1', NULL, 2035, N'Sustitución', N'GEN CFE', N'CI', N'CI', N'No renovable', 30.00, N'ene', N'BCS', N'87-Puerto San Carlos', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'C.C.I. San Carlos, U2', NULL, 2035, N'Sustitución', N'GEN CFE', N'CI', N'CI', N'No renovable', 30.00, N'ene', N'BCS', N'87-Puerto San Carlos', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 53.00, N'abr', N'NOR', N'05-Guaymas', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 56.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 544.00, N'abr', N'ORI', N'60-Ixtepec', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 808.00, N'abr', N'NES', N'18-Río Escondido', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2036, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 518.00, N'abr', N'NES', N'26-Tamazunchale', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2036, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 539.00, N'abr', N'NES', N'21-Matamoros', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2036, N'Adición', N'GEN CFE', N'HID', N'HID', N'Renovable', 455.00, N'abr', N'ORI', N'50-Acapulco', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 400.00, N'abr', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2036, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 120.00, N'abr', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'C.C.I. Baja California Sur I, U3', NULL, 2036, N'Sustitución', N'GEN CFE', N'CI', N'CI', N'No renovable', 41.00, N'ene', N'BCS', N'92-Coromuel', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE (Sustitucion)', N'CFE', N'C.C.I. Baja California Sur I, U4', NULL, 2036, N'Sustitución', N'GEN CFE', N'CI', N'CI', N'No renovable', 40.00, N'ene', N'BCS', N'92-Coromuel', N'Baja California Sur', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 16.00, N'abr', N'NTE', N'15-Camargo', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 13.00, N'abr', N'CEL', N'41-Donato Guerra', N'Estado de México', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 9.00, N'abr', N'ORI', N'58-Grijalva', N'Chiapas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 38.00, N'abr', N'OCC', N'32-León', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 10.00, N'abr', N'NTE', N'12-Moctezuma', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 43.00, N'abr', N'NES', N'22-Monterrey', N'Nuevo León', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 30.00, N'abr', N'ORI', N'47-Puebla', N'Puebla', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 73.00, N'abr', N'OCC', N'38-Querétaro', N'Querétaro', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 173.00, N'abr', N'NOR', N'09-Mazatlán', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 3.00, N'abr', N'PEN', N'71-Playa del Carmen', N'Quintana Roo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 29.00, N'abr', N'ORI', N'51-Agua Zarca', N'Guerrero', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 955.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1099.00, N'abr', N'NES', N'22-Monterrey', N'Nuevo León', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2037, N'Adición', N'GEN CFE', N'CC', N'CC', N'No renovable', 480.00, N'abr', N'NTE', N'10-Juárez', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2037, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 526.00, N'abr', N'ORI', N'46-Veracruz', N'Veracruz de Ignacio de la Llave', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 4.00, N'abr', N'BC', N'78-Tijuana', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 270.00, N'abr', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2037, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 81.00, N'abr', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 430.00, N'abr', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 129.00, N'ene', N'BC', N'83-Punta Estrella', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 1353.00, N'abr', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 395.00, N'abr', N'OCC', N'32-León', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2038, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 523.00, N'abr', N'ORI', N'55-Temascal', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2038, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 523.00, N'abr', N'ORI', N'57-Coatzacoalcos', N'Veracruz de Ignacio de la Llave', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 270.00, N'abr', N'BC', N'79-Ensenada', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2038, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 81.00, N'ene', N'BC', N'79-Ensenada', N'Baja California', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 167.00, N'abr', N'OCC', N'30-Guadalajara', N'Jalisco', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 98.00, N'abr', N'NTE', N'17-Laguna', N'Durango', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 118.00, N'abr', N'ORI', N'61-Juchitán', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 51.00, N'abr', N'NES', N'19-Nuevo Laredo', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 192.00, N'abr', N'NOR', N'07-Los Mochis', N'Sinaloa', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 38.00, N'abr', N'OCC', N'29-Tepic', N'Nayarit', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 75.00, N'abr', N'PEN', N'71-Playa del Carmen', N'Quintana Roo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 26.00, N'abr', N'PEN', N'70-Tulum', N'Quintana Roo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 35.00, N'abr', N'PEN', N'65-Ciudad del Carmen', N'Campeche', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 1173.00, N'abr', N'PEN', N'67-Mérida', N'Yucatán', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 250.00, N'abr', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 646.00, N'abr', N'OCC', N'32-León', N'Guanajuato', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 251.00, N'ene', N'PEN', N'76-Chetumal', N'Quintana Roo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 149.00, N'ene', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 214.00, N'ene', N'NES', N'23-Saltillo', N'Coahuila de Zaragoza', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 302.00, N'ene', N'NES', N'24-Valles', N'San Luis Potosí', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 58.00, N'ene', N'NOR', N'02-Cananea', N'Sonora', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2039, N'Adición', N'GEN', N'BAT', N'BAT', N'Baterías', 142.00, N'ene', N'ORI', N'53-Huatulco', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'Optimización', NULL, 2039, N'Adición', N'GEN CFE', N'CC/HIDN', N'CC/HIDN', N'No renovable', 515.00, N'abr', N'ORI', N'60-Ixtepec', N'Oaxaca', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2040, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 5.00, N'abr', N'PEN', N'67-Mérida', N'Yucatán', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2040, N'Adición', N'GEN', N'FV', N'FV', N'Renovable', 1588.00, N'abr', N'NTE', N'14-Chihuahua', N'Chihuahua', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2040, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 706.00, N'abr', N'NES', N'27-Güémez', N'Tamaulipas', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'CFE', N'CFE', N'CI Los Cabos (CC)', NULL, 2030, N'Adición', N'GEN', N'CI', N'CI', N'No renovable', 240.00, N'jun', N'BCS', N'97-Central Los Cabos', N'Baja California Sur', NULL, NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    INSERT INTO dgmesnie.PvirseCentralesElectricas
    (
        Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad, Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa, Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
    )
    VALUES
    (
        N'Por Definir', N'Por Definir', N'Optimización', NULL, 2040, N'Adición', N'GEN', N'EO', N'EO', N'Renovable', 8.00, N'abr', N'PEN', N'72-Cozumel', N'Quintana Roo', N'--', NULL, N'PVIRCE2026-2040_SQ.xlsx', SYSUTCDATETIME(), 1
    );

    PRINT 'Registros cargados: 449';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO
