
    INSERT INTO Modulos (
        SeccionId, Title, FundamentoLegalModulo, Roles, NombresRoles, Perfiles, Etapa,
        JustificacionOrden, AyudaContextual, Controller, Action, [Desc], Img, Btn,
        ElementosUI, AyudaVista, Orden, Activo
    ) VALUES (
        1, N'Gestión de Usuarios y Perfiles', N'Art. 70 Reglamento', N'1,2,3,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68',
        N'Secretaría de Energía, Unidad SNIEr, Dirección TICs, Presidencia Consejo, Entidades Normativas, Universidades, Institutos, Gobiernos locales', N'Administrador General, Técnico', N'Inicialización', N'Control de accesos desde el inicio (Art. 70)',
        N'Administra usuarios y permisos basados en Lineamientos.', N'Usuarios', N'AdministrarUsuarios', N'Gestiona perfiles, roles y accesos según normativa.',
        N'usuarios_scel.png', N'Administrar', N'Tabla de usuarios, árbol de permisos, e.firma.', N'Permite registrar y editar usuarios con trazabilidad legal.', 1, 1
    );
    
    INSERT INTO Modulos (
        SeccionId, Title, FundamentoLegalModulo, Roles, NombresRoles, Perfiles, Etapa,
        JustificacionOrden, AyudaContextual, Controller, Action, [Desc], Img, Btn,
        ElementosUI, AyudaVista, Orden, Activo
    ) VALUES (
        2, N'Carga de Información', N'Art. 70, 74 Reglamento', N'1,2,3,4,5,6,7,8,9,10,14,26,27,33,34,35,42,43,44,67,68',
        N'SENER, Unidad SNIEr, TICs, PEMEX, CFE, CONUEE, Representantes Sectorizados, Entidades Federativas, Municipios, Sujetos Obligados, CELs, Enlaces', N'Usuario Institucional, Técnico', N'Captura Inicial', N'Base para alimentar el sistema con datos confiables (Arts. 70 y 74)',
        N'Permite cargar archivos validados por perfil con control de versiones.', N'SNIE', N'CargaInformacion', N'Carga archivos de información oficial desde entidades responsables.',
        N'modulos/carga_info.png', N'Subir Datos', N'Formularios de carga, tabla de registros, validación en tiempo real.', N'Guía para enlaces institucionales al subir datos.', 2, 1
    );
    