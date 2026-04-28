-- ============================================================================
-- SCRIPT: Creación de Tablas Base para SIIL (Cimientos)
-- Descripción: Tablas fundamentales para Pronósticos y Registro de Muestras
-- Fecha: 2026-02-06
-- ============================================================================

-- 1.1. Tabla para Inteligencia (Pronósticos)
-- Esta tabla recibe la carga masiva del Excel
CREATE TABLE Pronostico_Pozos (
    IdInterno INT IDENTITY(1,1) PRIMARY KEY,
    IdPozo_Externo VARCHAR(50), -- Columna 'ID' del Excel (ej. P6.0137)
    Campo VARCHAR(100),         -- Columna 'Campo' (ej. Costero)
    Pozo VARCHAR(100),          -- Columna 'Pozo' (ej. Costero 1)
    EstadoPozo VARCHAR(50),     -- Columna 'Estado del pozo'
    FormacionGeologica VARCHAR(100), -- Columna 'Formación / Edad'
    -- OJO: Esta columna es vital para tus mapas de calor
    PosibilidadIntervalo VARCHAR(50), -- '1. Muy alto', '2. Alto', etc.
    RangoLi VARCHAR(50),        -- Columna 'Intervalo de concentración...'
    IdInsumo VARCHAR(50),       -- Para saber de qué carga vino (ej. Insumo 202502)
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaActualizacion DATETIME DEFAULT GETDATE()
);

-- 1.2. Tabla para Operación (Formulario 1)
-- Esta tabla soporta los datos del Formulario 1 y el ID único
CREATE TABLE Registro_Muestras (
    -- Esta es la PK generada por tu calculadora (ej. ARC-BRR-2026/01/23...)
    IdMuestra VARCHAR(50) PRIMARY KEY, 
    
    -- Contexto General
    IdProyecto VARCHAR(50),
    Institucion VARCHAR(100),
    ResponsableRegistro VARCHAR(100), -- Email
    FechaRegistro DATETIME,
    
    -- Ubicación y Geología
    Estado VARCHAR(50),
    Municipio VARCHAR(50),
    Latitud DECIMAL(18,6),
    Longitud DECIMAL(18,6),
    
    -- Clasificación (Source 1)
    Fuente VARCHAR(20), -- 'Arcilla' o 'Salmuera'
    Origen VARCHAR(50), -- 'Prospectiva' o 'Barrenación'
    
    -- Datos Condicionales (Arcilla - Barrenación)
    Azimut DECIMAL(10,2) NULL,
    Inclinacion DECIMAL(10,2) NULL,
    Largo DECIMAL(10,2) NULL,
    Diametro DECIMAL(10,2) NULL,
    RQD VARCHAR(20) NULL, -- Calidad de roca (ej. '50% - 75%')
    FamiliaRoca VARCHAR(50) NULL,
    
    -- Datos Condicionales (Salmuera)
    ProfundidadPozo DECIMAL(10,2) NULL,
    PH DECIMAL(4,2) NULL,
    Conductividad DECIMAL(10,2) NULL,
    Temperatura DECIMAL(5,2) NULL,
    
    -- Vinculación con Pronóstico (La clave de la integración)
    IdPozo_Pronostico_FK INT NULL,
    
    -- Auditoría
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaActualizacion DATETIME DEFAULT GETDATE(),
    
    -- Constraint de FK
    CONSTRAINT FK_RegistroMuestras_Pronostico 
        FOREIGN KEY (IdPozo_Pronostico_FK) 
        REFERENCES Pronostico_Pozos(IdInterno)
);

-- Crear índices para optimizar consultas
CREATE INDEX IX_Pronostico_IdPozo_Externo ON Pronostico_Pozos(IdPozo_Externo);
CREATE INDEX IX_Pronostico_Campo ON Pronostico_Pozos(Campo);
CREATE INDEX IX_Pronostico_IdInsumo ON Pronostico_Pozos(IdInsumo);

CREATE INDEX IX_Registro_IdProyecto ON Registro_Muestras(IdProyecto);
CREATE INDEX IX_Registro_Estado ON Registro_Muestras(Estado);
CREATE INDEX IX_Registro_Fuente ON Registro_Muestras(Fuente);
CREATE INDEX IX_Registro_Origen ON Registro_Muestras(Origen);
CREATE INDEX IX_Registro_FK_Pronostico ON Registro_Muestras(IdPozo_Pronostico_FK);

-- ============================================================================
-- Script ejecutado exitosamente
-- Próximo paso: Insumo 1.3 - Procedimientos Almacenados (SP)
-- ============================================================================
