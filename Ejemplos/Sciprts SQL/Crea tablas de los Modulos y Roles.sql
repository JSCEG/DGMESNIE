CREATE TABLE Secciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(100),
    Articulos NVARCHAR(100),
    FundamentoLegal NVARCHAR(MAX),
    Descripcion NVARCHAR(MAX),
    Ayuda NVARCHAR(MAX),
    Objetivo NVARCHAR(MAX),
    ResponsableNormativo NVARCHAR(MAX),
    PublicoObjetivo NVARCHAR(MAX),
    Activo BIT DEFAULT 1
);

CREATE TABLE Modulos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SeccionId INT FOREIGN KEY REFERENCES Secciones(Id),
    Title NVARCHAR(100),
    FundamentoLegalModulo NVARCHAR(MAX),
    Roles NVARCHAR(MAX),
    NombresRoles NVARCHAR(MAX),
    Perfiles NVARCHAR(100),
    Etapa NVARCHAR(100),
    JustificacionOrden NVARCHAR(MAX),
    AyudaContextual NVARCHAR(MAX),
    Controller NVARCHAR(50),
    Action NVARCHAR(50),
    [Desc] NVARCHAR(MAX),
    Img NVARCHAR(100),
    Btn NVARCHAR(50),
    ElementosUI NVARCHAR(MAX),
    AyudaVista NVARCHAR(MAX),
    Orden INT,
    Activo BIT DEFAULT 1
);