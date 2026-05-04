SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dgmesnie.InformePormenorizadoModernizacion', 'U') IS NULL
BEGIN
    THROW 50000, 'La tabla dgmesnie.InformePormenorizadoModernizacion no existe. Ejecuta primero el script de creación.', 1;
END
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM dgmesnie.InformePormenorizadoModernizacion;

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        1, 1, N'CE
OC', N'Compensación Capacitiva Occidente', N'Presupuestal', 2015, N'En Operación', 155.93, N'P15-OC3 Compensación Zona Guanajuato
SE Guanajuato MVAr (OC)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Santa Fe II MVAr (OC)
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
P15-OC6 Compensación Zona Querétaro
SE La Fragua MVAr (CE)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE La Griega MVAr (CE)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Dolores Hidalgo MVAr (CE)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Querétaro Oriente MVAr (CE)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Buenavista MVAr (CE)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
P15-OC7 Compensación Zona Apatzingán
SE Cerro Hueco MVAr (traslado) (OC)
1 Capacitor - 8.1 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'S.E. Guanajuato, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 23.JUN.2024
S.E. Santa Fe II, 1 Capacitor -30 MVAr-115 kV
Se energizó el 30.JUN.2024
S.E. La Fragua, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 10.SEP.2024
S.E. La Griega, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 30.MAR.2025
S.E. Dolores Hidalgo, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 12.ENE.2025
S.E. Querétaro Oriente, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 11.NOV.2024
S.E. Buenavista, 1 Capacitor -22.5 MVAr-115 kV
Se energizó el 20.AGO.2024
S.E. Cerro Hueco, 1 Capacitor - 8.1 MVAr-115 kV
Se energizó el 01.AGO.2024', N'Portafolio Activo', N'P15-OC3
P15-OC4
P15-OC6
P15-OC7', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 173.10, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        2, 2, N'BC
NO', N'Compensación Capacitiva Baja California - Baja California Sur – Noroeste', N'Presupuestal', 2015, N'Por concursar', 194.88, N'SE Guamuchil Dos MVAr (NO)
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Parque Industrial San Luis MVAr (BC)
1 Capacitor - 21.0 MVAr - 115 kV 
1 Alimentador 115 kV Capacitor 
SE Packard MVAr (BC)
1 Capacitor - 21.0 MVAr - 161 kV
1 Alimentador 115 kV Capacitor 
SE San Simón MVAr (BC)
1 Capacitor - 7.5 MVAr - 115 kV 
1 Alimentador 115 kV Capacitor
SE Guerrero MVAr (BC)
1 Capacitor - 16.0 MVAr - 69 kV 
1 Alimentador 69 kV Capacitor 
SE México MVAr (BC)
1 Capacitor - 16.0 MVAr - 69 kV
1 Alimentador 69 kV Capacitor 
SE Santiago MVAr (BC)
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Bledales MVAr (BC)
1 Capacitor - 12.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación y asignación de recursos.
Derivado del alcance en las Metas Físicas, se requirió el prolongar los periodos en las revisiones, validación y liberación de documentos principalmente las ICM''s y paquete de concurso.', N'Se integra el Pliego de Requisitos, con la participación de las GRT´s  involucradas cuidando las mejores condiciones para que se adjudique el proyecto.
Se actualizó la ICM, plurianualidad, gestión de suficiencia presupuestal y completar paquete de concurso para solicitar su publicación.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas y será realizado por DIPI/CPTT.
10.OCT.25 Solicitud de Publicación
15.OCT.25 Publicación
18.DIC.25 Fallo', N'Portafolio Activo', N'P15-NO2
P15-BC3
P15-BC4
P15-BC5', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 124.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        3, 3, N'OC', N'Guadalajara Industrial', N'Presupuestal', 2016, N'Por concursar', 1073.28, N'Entronque LT Guadalajara Uno - 63650 - Bugambilias para formar la
LT Guadalajara Industrial - Bugambilias
(Tramo 1)
69 kV - 1C - 4.5 km-C - 1113 ACSR (Torre Multicircuito)
Tramo 2
69 kV - 1C - 4.5 km-C 795 ACSR PT
Tramo 3 Recalibración
69 kV - 1C - 1.79 km-C 795 ACSR PA
LT Guadalajara Industrial - Las Pintas (Nueva)
69 kV - 1C - 2.9 km-C Cable Subterráneo
(se utiliza alimentador existente de la LT 63750)
LT Guadalajara Industrial Entronque Miravalle - Álamos e Higuerillas - Álamos
69 kV - 2C - 9.0 km-C Cable Subterráneo
LT Miravalle - 63350 - Álamos
LT Higueras - 63160 - Álamos
Para formar las NUEVAS LT''s:
Guadalajara Industrial - Higuerrillas
Guadalajara Industrial - Miravalle
Dejando deshabilitado el tramo de la
LT Higuerillas - 63160 - Álamos 
desde el entronque
hasta la SE Álamos y, alimentando desde la SE Álamos
a las cargas industriales de la LT 63350.
Nueva Línea:
SE Santa Cruz entronque LT Acatlán 63860 San Agustín
69 kV - 2C - 0.4 km-C 795 ACSR
Recalibración de Líneas
LT Guadalajara I - 63150 - Higuerillas
69 kV - 1C - 11.0 km-C 611 MCM ACSC (Alta Temperatura para 108 MVA)
LT Guadalajara I - 63170 - Miravalle
69 kV - 1C - 14.0 km-C 611 MCM ACSC (Alta Temperatura para 108 MVA)
SE Guadalajara Industrial Banco 2
4 AT - 1F - 75 MVA - 300 MVA -230/69 kV
4 Alimentadores 69 kV
SE Santa Cruz (Ampliación)
2 Alimentadores 69 kV
Adecuaciones en Subestaciones Eléctricas
SE Guadalajara Industruial
Interruptor de amarre 69 kV
SE Higuerillas
Sustitución de barras, puentes y TC de 69 kV
SE Miravalle
Sustitución de barras, puentes y TC de 69 kV
SE Bugambilias
Sustitución de barras, puentes y TC de 69 kV
SE San Agustín
Sustitución de barras y puentes de 69 kV
SE Santa Cruz
Sustitución de barras y puentes de 69 kV
SE Guadalajara I
Reemplazo 1 interruptor por incremente de corto circuito
SE El Sol
Reemplazo 4 interruptores por incremente de corto circuito', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación y asignación de recursos.
Derivado del alcance en las Metas Físicas, se requirió el prolongar los periodos en las revisiones, validación y liberación de documentos principalmente la ICM y paquete de concurso.', N'Se integra el Pliego de Requisitos, con la participación de las GRT´s  involucradas cuidando las mejores condiciones para que se adjudique el proyecto.
En proceso de Validación de ICM actualizada, gestión de suficiencia presupuestal y completar paquete de concurso para solicitar su publicación.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas y será realizado por DIPI/CPTT.
24.OCT.25 Solicitud de Publicación
29.OCT.25 Publicación
22.ENE.26 Fallo', N'Portafolio Activo', N'P16-OC1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 300.00, 0.00, 48.09, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        4, 4, N'NO', N'El Habal Banco 2', N'Presupuestal', 2017, N'En Operación', 35.50, N'SE El Habal Banco 2 (traslado)
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 30.DIC.2019.', N'Portafolio Activo', N'M16-NO2', N'Concluido y en operación', NULL, NULL, NULL, 100.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        5, 5, N'NT', N'Ascensión II Banco 2', N'Presupuestal', 2017, N'En Operación', 74.85, N'SE Ascensión II, Banco 2 (traslado)
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV
SE La Salada MVAR
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 13.FEB.2020.', N'Portafolio Activo', N'P17-NT1', N'Concluido y en operación', NULL, NULL, NULL, 100.00, 7.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        6, 6, N'NO', N'El Carrizo MVAr', N'Presupuestal', 2017, N'En Operación', 8.41, N'SE El Carrizo MVAR
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 17.DIC.2021.', N'Portafolio Activo', N'M16-NO1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        7, 7, N'GAC', N'Red Eléctrica Inteligente Dirección de Transmisión 2018-2021', N'Presupuestal', 2017, N'Ejecución/Construcción', 7080.32, N'REI - Red Eléctrica Inteligente
Obra Civil
Electromecánica
Instalación de equipo de comunicaciones y de control
Tendido de cable de fibra óptica
En instalaciones de Subtransmisión.', NULL, NULL, NULL, NULL, N'Obra Civil y Electromecánca: Retrasos por procesos de contratación, licitaciones desiertas, recortes presupuestales y convenios tardíos con Distribución y Construcción.

Falta de Personal: insuficiencia de Ingenieros y técnicos para atender simultáneamente el REI y otros proyectos prioritarios (PRODESEN, STATCOM, Interconexiones).

Restricciones de libranzas (CENACE): limitan la integración de bahías y sistemas SCADA/EMS.

Riesgos sociales y de seguridad: confictos en comunidades (Chiapas, Tabasco, Oaxaca, Ensenada) y presencia de crimen organizado.

Factores externos: Guerra Rusia-Ucrania (afectando cadenas de suministro), desastres naturales (huracanes)', N'Negociaciones con comunidades, ejidos y apoyo de autoridades (Gobierno Estatal, SEDENA, Guardia Nacional)

Reprogramación y actualización de calendarios con convenios formalizados con Distribución.

Redistribución de personal entre zonas y apoyo intergerencial. 

Acuerdos con SUTERM para flexibilizar jornadas y horas extra.

Contratación parcial de servicios externos y uso de personal comisionado.', N'El proyecto fue reevaluado y la Gerencia de Automatización y Comunicaciones solicitó la reprogramación de la Fecha de Entrada en Operación Factible para el 31.DIC.2025.

Avance fisico Familias REI:

Modernización de la infraestructura SCADA en Subtransmisión de 604 Subestaciones
Modernización de los Sistemas de Comunicaciones 
- Comunicaciones Unificadas 1,867
- Red de Datos Operativa 2,255
- Sistemas de Radiocomunicación 186
- Puesta en operación de 178 enlaces con un total de 10,794 km de Fibra Óptica', N'Portafolio Activo', N'M17-REI', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        8, 8, N'OC', N'Potrerillos Banco 4', N'Presupuestal', 2017, N'En Operación', 630.60, N'LT Potrerillos Entronque León I – Ayala
115 kV - 2C - 37.12 km-C - 795 ACSR TA
LT Potrerillos – San Roque
115 kV - 2C - 9.28 km-C - 795 ACSR TA /1
SE Potrerillos Banco 4
4T - 1F - 125 MVA - 500 MVA - 400/115 kV
SE Potrerillos (ampliación) 
3 Alimentadores 115 kV
SE San Roque (ampliación) 
1 Alimentador 115 kV
SE León Tres Banco 3 (traslado)
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV

1/Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 26.JUN.2022 el Banco 4 de SE Potrerillos.
Se energizó el 19.DIC.2024 las LT''s asociadas al proyecto.', N'Portafolio Activo', N'P16-OC2', N'Concluido y en operación', NULL, NULL, NULL, 600.00, 0.00, 46.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        9, 9, N'CE', N'Querétaro Banco 1 (sustitución)', N'Presupuestal', 2017, N'Ejecución/Construcción', 161.86, N'SE Querétaro Banco 1 (Sustitución) (Nuevo)
3 AT – 1F - 75 MVA - 225 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'Este es un proyecto clasificado como continuado es decir, se segregaron las obras y suministros lo que ocasionó que el proyecto se demorara para converger la llegada de los suministros y la construcción.', N'Se tomó la decisión de realizar el proyecto en fases con la finalidad de poder concluirlo conforme a la Fecha Factible de Término.', N'En proceso Etapa 3 Ejecución / Construcción.
Esta última fase del proyecto consiste en la adquisición de tres unidades monofásicas para reponerlas de las subestaciones que prestaron los AT''s para la conformación del banco de la SE Querétaro.
El banco que se energizó el 21.NOV,2022, conformado por tres AT´s de reserva ya no será desenergizado por cuestiones técnicas, operativas, de confiabilidad y de continuidad; los tres nuevos AT''s serán devueltos a las subestaciones que prestaron las unidades monofásicas.', N'Portafolio Activo', N'P15-OC1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        10, 10, N'CE', N'Donato Guerra MVAr (traslado)', N'Presupuestal', 2017, N'En Operación', 58.43, N'SE Donato Guerra MVAr 
(traslado desde Temascal II reactores se encontraban fuera de servicio)
1 R Barra - 3F - 35 MVAr - 400 kV
1 Alimentador 400 kV Reactor 
1 R Barra - 3F - 35 MVAr - 400 kV
1 Alimentador 400 kV Reactor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 25.ABR.2024 el Reactor del bus 1.
Se energizó el 10.MAY.2024 el Reactor del bus 2.', N'Portafolio Activo', N'P15-CE1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 70.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        11, 11, N'SE', N'Frontera Comalapa MVAr', N'Presupuestal', 2017, N'En Operación', 11.82, N'SE Frontera Comalapa MVAR
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 24.MAR.2023.', N'Portafolio Activo', N'P17-OR9', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 7.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        12, 12, N'SE
OR', N'Suministro de energía en Oaxaca y Huatulco
Etapa 1
(LT Jalapa de Díaz – Oaxaca Potencia 2º. circuito)', N'Presupuestal', 2017, N'Por concursar', 737.33, N'LT Jalapa de Díaz – Oaxaca Potencia
230 kV - 2C - 152 km-C - 1113 ACSR TA /1
SE Jalapa de Díaz (Ampliación)
2 Alimentadores 230 kV 
SE Oaxaca Potencia (Ampliación)
1 Alimentador 230 kV
SE Ciénega MVAr (Ampliación)
3 R Barra - 1F - 7 MVAr - 21 MVAr - 230 kV

1/ Tendido del primer circuito', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación y asignación de recursos.
Derivado del alcance en las Metas Físicas, se requirió el prolongar los periodos en las revisiones, validación y liberación de documentos principalmente las ICM''s y paquete de concurso.', N'Se integra el Pliego de Requisitos, con la participación de las GRT´s  involucradas cuidando las mejores condiciones para que se adjudique el proyecto.
Se gestionó la validación de ICM, en proceso la gestión de plurianualidad,  suficiencia presupuestal y completar paquete de concurso para solicitar su publicación.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas y será realizado por DIPI/CPTT.
17.OCT.25 Solicitud de Publicación
23.OCT.25 Publicación
23.ENE.26 Fallo', N'Portafolio Activo', N'P17-OR4', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 21.00, 152.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        13, 13, N'NO', N'Compensación Reactiva Inductiva en Seri', N'Presupuestal', 2017, N'En Operación', 300.78, N'SE Seri MVAr
3 R Barra - 1F - 16.66 MVAr - 49.8 MVAr - 400 kV
1 Alimentador 400 kV Reactor 
3 R Barra - 1F - 16.66 MVAr - 49.8 MVAr - 400 kV
1 Alimentador 400 kV Reactor 
1 R Barra - 1F - 16.66 MVAr - 400 kV (reserva compartida)', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 02.ABR.2025 el Banco de Reactores 2.
Se energizó el 04.ABR.2025 el Banco de Reactores 1.', N'Portafolio Activo', N'P16-NO2', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 116.62, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        14, 14, N'NT', N'Zona La Laguna', N'Presupuestal', 2017, N'En Operación', 625.18, N'LT Torreón Sur – Takata (recalibración)
115 kV - 1C - 5.5 km-C - 611 ACCC TA-PA-PM
LT Takata – Torreón Oriente (recalibración)
115 kV - 1C - 5.2 km-C - 611 ACCC TA-PA-PM
LT Torreón Sur – Maniobras Mieleras (recalibración)
115 kV - 1C - 5 km-C - 611 ACCC TA-PA-PM
LT Maniobras Mieleras – Diagonal (recalibración)
115 kV - 1C - 8.9 km-C - 611 ACCC TA-PA-PM
LT Torreón Sur – Torreón Oriente (recalibración)
115 kV - 1C - 14.1 km-C - 611 ACCC TA-PA-PM
LT Torreón Oriente – California
115 kV - 1C - 5.6 km-C
Tramo 1: 5.25 km-C aéreo 1113 ACSR-PA
Tramo 2: 0.31 km-C CS XLP-CU-2500 mm2
(se considera una incertidumbre de 6.6 km-C)
SE Torreón Sur Banco 3
3 AT - 1F - 125 MVA - 375 MVA - 400/115 kV
SE Torreón Oriente
1 Alimentador 115 kV
SE California
1 Alimentador 115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 04.MAR.2025.', N'Portafolio Activo', N'P16-NT1', N'Concluido y en operación', NULL, NULL, NULL, 375.00, 0.00, 48.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        15, 15, N'PE', N'Nachicocom (Antes Chichi Suárez Banco 1)', N'Presupuestal', 2017, N'Ejecución/Construcción', 1075.75, N'LT Nachicocom Entronque Norte - Kanasín Potencia
230 kV - 2C - 17.2 km-C
SE Nachicocom Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
Recalibración de la barra 1 y 2 de 115 kV', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P16-PE2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 300.00, 0.00, 17.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        16, 16, N'GOM', N'Implementación de Sistemas de Medición para el Mercado Eléctrico Mayorista de CFE Transmisión', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 2764.00, N'Proyecto MEM 
Obra Civil
Obra Electromecánica
Instalación de Equipo Eléctrico
Equipo de Control y Medición
Instalaciones de Subtransmisión y transmisión.', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M17-MEM', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        17, 17, N'BC', N'El Arrajal Banco 1 y red asociada', N'Fibra E', 2017, N'En Proceso de Decisión', 2521.36, N'LT El Arrajal – Cucapáh
230 kV - 2C - 144 km-C – ACSR 1113 – TA (aislada en 400kV)​ /2
LT El Arrajal – San Felipe
115 kV - 2C - 56 km-C – ACSR 795 - TA​ /2
LT El Arrajal Entronque San Matías - San Felipe​
115 kV - 2C - 59.2 km-C – ACSR 795 - TA​
LT El Arrajal Entronque Minera San Felipe – Entq. San Felipe​
115 kV - 1C - 0.4 km-C – ACSR 795 – TA​
SE El Arrajal Banco 1
4 AT – 1F – 75 MVA - 300 MVA – 230/115kV /3
SE El Arrajal​
1 Alimentador 230 kV​
(para la interconexión de la LT hacia SE Cucapáh)
4 Alimentadores 115 kV​
(1 para la interconexión de la LT hacia SE San Felipe
2 para el entronque con la LT San Matías - San Felipe
1 para la interconexión de la LT hacia SE Minera San Felipe)
SE Cucapáh
1 Alimentador 230 kV​ /4
(para la interconexión de la LT hacia la nueva SE El Arrajal)
SE San Felipe
2 Alimentador 115 kV​ /4, 5
(1 para la interconexión de la LT hacia la nueva SE El Arrajal
1 para la normalización del arreglo existente)
SE Trinidad
1 Alimentador 115 kV​ /6
(para la normalización de la LT hacia SE Cañón).

2/ Tendido del primer circuito. ​
3/ Considera una fase de reserva de 75 MVA.​
4/ Ampliación de la SE.​
5/ Normalización de arreglo existente BP/BT con interruptor de transferencia.​
6/ Normalización de Alimentador a LT Cañón.​', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Pendiente la autorización por parte del Consejo de Administración de CFE.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-BC11', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 0.00, 285.56, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        18, 18, N'OR
SE
CE', N'Incremento de Capacidad de Transmisión entre las Regiones Puebla–Temascal, Temascal–Coatzacoalcos, Temascal–Grijalva y Grijalva-Tabasco', N'Fibra E', 2017, N'En Proceso de Decisión', 3788.28, N'SE Juile MVAr (sustitución)
Capacitor Serie 205.7 MVAr - 400 kV - LT Juile - A3140 - Malpaso Dos /1,2
Capacitor Serie 274.4 MVAr - 400 kV - LT Juile - A3T90 - Manuel Moreno Torres /1,2
Capacitor Serie 268.3 MVAr - 400 kV - LT Juile - A3040 - Manuel Moreno Torres /1,2
Capacidad Total: 748.4 MVAr
 
SE Temascal Dos MVAr (sustitución)
Capacitor Serie 411.7 MVAr - 400 kV - LT Temascal Dos - A3260 - Chinameca Potencia /2,3
Capacitor Serie 489.8 MVAr - 400 kV - LT Temascal Dos - A3360 - Minatitlán Dos /2,3
Capacidad Total: 901.6 MVAr
 
SE Puebla Dos MVAr (sustitución)
Capacitor Serie 266.1 MVAr - 400 kV - LT Puebla Dos - A3910 - Ojo de Agua Potencia /2,3
Capacitor Serie 266.1 MVAr - 400 kV - LT Puebla Dos - A3920 - Ojo de Agua Potencia /2,3
Capacidad Total: 532.2 MVAr
 
SE Ixtepec Potencia (sustitución)
3 Transformadores de Corriente 400 kV LT Ixtepec Potencia - A3V30 - Juile /4
3 Transformadores de Corriente 400 kV LT Ixtepec Potencia - A3V40 - Juile /4
2 Aimentadores 400 kV Línea de Transmisión /5
 
SE Juile (sustitución)
3 Transformadores de Corriente 400 kV LT Juile - A3V30 - Ixtepec Potencia /4
3 Transformadores de Corriente 400 kV LT Juile - A3V40 - Ixtepec Potencia /4
Recalibración Buses 400 kV - 2 Cond/f - ACCC 1020
2 Aimentadores 400 kV Línea de Transmisión /5
 
SE Puebla Dos (sustitución)
3 Transformadores de Corriente 400 kV LT Puebla Dos - A3930 - San Lorenzo Potencia /4
3 Transformadores de Corriente 400 kV LT Puebla Dos - A3T20 - San Lorenzo Potencia /4
2 Aimentadores 400 kV Línea de Transmisión /5
 
SE San Lorenzo Potencia (sustitución)
3 Transformadores de Corriente 400 kV LT San Lorenzo Potencia - A3930 - Puebla Dos /4
3 Transformadores de Corriente 400 kV LT San Lorenzo Potencia - A3T20 - Puebla Dos /4
2 Aimentadores 400 kV Línea de Transmisión /5
 
1/ Compensación al 25% valor reactancia LT
2/ Corriente nominal 2,000 A, equivalente 1,386 MVA
3/ Compensación al 47% valor reactancia LT
4/ Alcanzar capacidad equivalente 1500 MVA
5/ Reemplazo de Equipo Asociado serie', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio y Plan de Ejecución autorizados.
En revisión y autorización del Plan Financiero.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M16-OR1', N'En Concurso y Por Concursar', N'2026.1', N'3.2', N'Sí', 0.00, 2182.20, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        19, 19, N'VM', N'Kilómetro 110 – Tulancingo.', N'En proceso de definición', 2017, N'Instruido y CON priorización', 53.00, N'LT Kilómetro 110 – Tulancingo
85 kV – 1C – 14.2 km-C – 795 ACSR – TA/PA
SE Kilómetro 110
1 Alimentador 85 kV
SE Tulancingo
1 Alimentador 85 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P16-CE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 14.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        20, 20, N'SE', N'Tabasco Potencia MVAr.', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 49.00, N'SE Tabasco Potencia MVAr (Traslado)
1 R Barra - 3F - 63.5 MVAr - 400 kV
1 Alimentador 400 kV Reactor
(Traslado de Reactor desde la SE Temascal II).', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P17-OR3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 63.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        21, 21, N'BC', N'Maneadero entronque Ciprés-Cañón.', N'Fibra E', 2017, N'Con acuerdo de Cancelación', 148.23, N'LT Maneadero Entronque Ciprés - Cañón
115 kV - 2C - 3.5 km-C - 795 ACSR - TA/PA
SE Maneadero
2 Alimentadores 115 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Proyecto con petición de CANCELACIÓN del portafolio de proyectos.
Si entra el Arrajal, este se cancelará.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P15-BC1', N'Con acuerdo de cancelación', NULL, NULL, NULL, 0.00, 0.00, 7.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        22, 22, N'CE', N'Amozoc y Acatzingo MVAr.', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 24.00, N'SE Amozoc MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Acatzingo MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P17-OR6', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        23, 23, N'OR', N'Alvarado II y San Andrés II MVAr', N'Fibra E', 2017, N'En Proceso de Decisión', 69.96, N'S E Alvarado II MVAr (Ampliación)
1 Capacitor - 7.5 MVAr - 115 kV /1
1 Alimentador 115 kV Capacitor
SE San Andrés II MVAr (Ampliación)
1 Capacitor - 7.5 MVAr - 115 kV /1
1 Alimentador 115 kV Capacitor

/1 Incluye reactor de amortiguamiento', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P16-OR2', N'En Concurso y Por Concursar', N'2026.1', N'2.2', N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        24, 24, N'NO', N'Compensación Reactiva Inductiva en Esperanza.', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 14.65, N'SE Esperanza I
Reactor de Terciario
1 R Terciario - 3F - 21 MVAr - 13.8 kV
1 Alimentador 13.8 kV Reactor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P15-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 21.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        25, 25, N'OR', N'Esfuerzo MVAr.', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 12.00, N'SE Esfuerzo MVAr
1 Capacitor -  15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P17-OR7', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        26, 26, N'CE', N'Izúcar de Matamoros MVAr.', N'Fibra E', 2017, N'En Proceso de Decisión', 35.21, N'SE Izúcar de Matamoros MVAr
1 Capacitor  - 12.5 MVAr - 115 kV /1
1 Alimentador 115 kV Capacitor
 
1/ Con Reactor de Amortiguamiento', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P16-OR1', N'En Concurso y Por Concursar', N'2026.1', N'2.1', N'Sí', 0.00, 12.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        27, 27, N'BC', N'Interconexión Sistema Interconectado Nacional  - Baja California Sur.', N'Pendiente de definir', 2017, N'Instruido y SIN priorización', 31883.45, N'LT Coromuel Entronque Punta Prieta II – Palmira
115 kV - 2C - 2 km-C - 795 ACSR TA
LT Villa Constitución - Olas Altas
230 kV - 2C - 197 km-C - 1113 ACSR TA
LT El Infiernito - Mezquital (LT CD)
± 400 kV - BIPOLO - 250 km-C - 1113 ACSR TA
LT El Infiernito - Bahía de Kino (LT Submarina)
±400 kV - BIPOLO - 110 km-C - 1113 ACSR Cable Submarino
LT Mezquital - Villa Constitución (LTCD)
±400 kV - BIPOLO - 392 km-C - 1113 ACSR TA
LT Bahía de Kino - Esperanza (LTCD)
±400 kV - BIPOLO - 50 km-C - 1113 ACSR TA
LT Esperanza – Seri
400 kV - 2C - 53 km-C - 1113 ACSR TA
LT Olas Altas - CD Los Cabos
230 kV - 2C - 125 km-C - 1113 ACSR TA
SE Coromuel Banco 1
4 AT - 1F - 33.33 MVA - 133.33 MVA - 230/115 kV
SE Villa Constitución Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Olas Altas
2 Alimentadores 230 kV
SE CD Los Cabos SF6
2 Alimentadores 230 kV
S.E. Seri
2 Alimentadores 400 kV
SE Olas Altas
2 Alimentadores 230 kV
SE Olas Altas MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Villa Constitución MVAr
1 Capacitor - 12.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
Estación Convertidora VSC Villa Constitución
1 EC - 840 MVA - ±400/230 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2017 y sin priorización.
Transmisión había solicitado la cancelación y baja del portafolio de proyectos sin embargo en mesas de trabajo con SENER, se acordó que este proyecto se mantendrá dentro del portafolio de proyectos instruidos por SENER.', N'B4: Sin priorizar', N'P16-BS1', N'En análisis por priorizar', NULL, NULL, N'Sí', 1273.32, 27.50, 1179.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        28, 28, N'BC', N'Mezquital MVAr (traslado)', N'Presupuestal', 2018, N'En Operación', 5.18, N'SE Mezquital MVAr (Traslado)
1 R Barra - 3F - 12.5 MVAr - 115 kV
1 Alimentador 115 kV Reactor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 09.FEB.2021.', N'Portafolio Activo', N'P18-MU3', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 12.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        29, 29, N'BC', N'Santa Rosalía Banco 2', N'Presupuestal Compartido', 2018, N'En Operación', 1.30, N'SE Santa Rosalía Banco 2
1 T - 3F - 20 MVA - 115/13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 09.JUN.2021.', N'Portafolio Activo', N'P18-MU1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        30, 30, N'BC', N'Recreo MVAr', N'Presupuestal', 2018, N'En Operación', 18.87, N'SE Recreo MVAR
1 Capacitor - 12.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 22.DIC.2021.', N'Portafolio Activo', N'P18-BS6', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 12.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        31, 31, N'NE', N'Jiménez, Las Norias y San Fernando MVAr', N'Presupuestal', 2018, N'En Operación', 79.41, N'SE Jiménez MVAr
1 Capacitor - 5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Las Norias MVAr
1 Capacitor - 5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE San Fernando MVAr
1 Capacitor - 5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 25.JUN.2022.', N'Portafolio Activo', N'P18-NE8', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        32, NULL, N'OR', N'Proyecto de Inversión de CEVs para CFE Transmisión 2018-2021
Etapa 2
(1 CEV de ciclo completo por STATCOM)', N'Presupuestal', 2018, N'Actividades y Estudios Previos', 1192.22, N'Proyecto de Inversión de CEVs para CFE Transmisión 2018-2021
Modernización de ciclo completo para 1 CEV (OR)

SE Temascal Tres (GRTOR)
Modernización ciclo completo CEV a STATCOM', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación.Largos periodos en las revisiones, validación y liberación de documentos principalmente las ICM''s 

 Coordinación Interinstitucional: La coordinación entre CFE Transmisión y residencia de construcción ha sido un desafío constante en la firma de convenios y la ejecución del proyecto', N'Se conforma especificación en la cual se autoriza que la modernización del CEV sea llevado a cabo por un STATCOM

Se llevan a cabo reuniones constantes con la Residencia de Construcción Sureste en la cual se formulan la estrategía a seguir para ejecutar el proyecto.', N'ETAPA 2,
- Se encuentra en análisis la propuesta de contrato para proceder con la formalizacion y firma del convenio.
- Se están realizando las especificaciones técnicas relacionadas con el STATCOM de la SE Temascal III.
- Se recibe por parte de la GCROR los factores de participación donde se establecen rangos y horario de operación estimado para el STATCOM de la SE Temascal III.
- Se tiene reunión con la Residencia Regional de Construcción SurEste (RRCSE) donde conjuntamente se revisa la especificación de características particulares del STATCOM de la SE Temascal III.
- Se recibe cotización por parte de la RRCSE el 21/02/2025.
- Se solicitó plurianualidad con oficio GAC-CT-159/2025.
- Se informa por parte de la Unidad de Finazas que no es procedente debido al saldo insuficiente en la cartera.
- Se solicita por oficio GAC-CT-181-2025 del 4/04/2025  la autorización de uso de incertidumbre mismo que no fue autorizado.
- Se realizan reuniones periodicas con la RRCSE para revisar las especificaciones técnicas.', N'Portafolio Activo', N'M18-SIN1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 600.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        33, 33, N'NT', N'Chihuahua Norte Banco 5', N'Presupuestal', 2018, N'En Operación', 324.58, N'SE Chihuahua Norte Banco 5
4 AT - 1F - 100 MVA - 400 MVA -  230/115 kV
SE Ávalos Banco 3 (Traslado)
3 AT – 1F – 33.3 MVA - 100 MVA – 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 06.OCT.2024, el Banco 5 de SE Chihuahua Norte.
Se energizó el 08.DIC.2024, el Banco AT-97 de SE Ávalos.', N'Portafolio Activo', N'P15-NT1', N'Concluido y en operación', NULL, NULL, NULL, 500.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        34, 34, N'BC', N'Panamericana Potencia Banco 3', N'Presupuestal', 2018, N'Ejecución/Construcción', 198.15, N'SE Panamericana Potencia Banco 3
4 AT - 1F - 75 MVA - 300 MVA - 230/69 kV', NULL, NULL, NULL, NULL, N'Este es un proyecto clasificado como continuado es decir, se segregaron las obras y suministros lo que ocasionó que el proyecto se demorara para converger la llegada de los suministros y la construcción.', N'Se tomó la decisión de realizar el proyecto en fases con la finalidad de poder concluirlo conforme a la Fecha Factible de Término.', N'En proceso Etapa 3 Ejecución / Construcción.
En proceso la fase 2 del proyecto que corresponde a la adquisición de las tres unidades de transformación.', N'Portafolio Activo', N'P17-BC14', N'Ejecución/Construcción', NULL, NULL, N'Sí', 300.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        35, 35, N'CE', N'Línea de Transmisión Atlacomulco Potencia - Almoloya', N'Presupuestal', 2018, N'En Operación', 198.30, N'LT Atlacomulco Potencia - Almoloya
400 kV - 2C - 28.0 km-C - 1113 ACSR TA (Tendido del segundo circuito)
SE Atlacomulco Potencia
1 Alimentador 400 kV
SE Almoloya
1 Alimentador 400 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 21.SEP.2024.', N'Portafolio Activo', N'M15-CE2', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 28.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        36, 36, N'OR', N'Modernización de Líneas de Transmisión Chinameca Potencia A3260 Temascal II y Minatitlán II A3360 Temascal II', N'Presupuestal', 2018, N'Ejecución/Construcción', 148.30, N'LT Temascal II - A3260 - Chinameca Potencia (sustitución)
12 estructuras /1
LT Temascal II - A3360 - Minatitlán II (sustitución)
14 estructuras /2


1/ Sustitución de 114.4 km de cable conductor
2/ Sustitución de 113.6 km de cable conductor', NULL, NULL, NULL, NULL, N'Este es un proyecto clasificado como continuado es decir, se segregaron las obras y suministros lo que ocasionó que el proyecto se demorara para converger la llegada de los suministros y la construcción.', N'Se tomó la decisión de realizar el proyecto en fases con la finalidad de poder concluirlo conforme a la Fecha Factible de Término.', N'En proceso Etapa 3 Ejecución / Construcción.
En proceso la fase 4 que corresponde al suministro, instalación y montaje de 16 estructuras para la LT CHM A3260 TMD y LT TMD A3360 MID.', N'Portafolio Activo', N'M18-OR1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 228.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        37, 37, N'OC', N'Irapuato II Banco 3 (traslado)', N'Presupuestal', 2018, N'Ejecución/Construcción', 363.65, N'LT Irapuato II - 73590 - Irapuato I (recalibración)
115 kV - 1C - 8.6 km-C - 1 Cond/f - 795 ACSR - PT /1
LT Irapuato II - 73600 - Irapuato I (recalibración)
115 kV - 1C - 8.6 km-C - 1 Cond/f - 795 ACSR - PT /1
LT Irapuato I - Irapuato II
115 kV – 1C – 6.0 km-C – 795 ACSR - PT /2,3
SE Irapuato II Banco 3 (traslado desde SE Potrerillos)
4 AT – 1F - 33.33 MVA - 133.33 MVA - 230/115 kV
SE Irapuato II
Recalibración Barras 115 kV /4
1 Juego Cuchilla desconectadora 115 kV /6
SE Irapuato Poniente
Recalibración Barras 115 kV /5

1/ Alcanzar límite operativo mínimo 179 MVA
2/ Construcción de nueva Línea de Transmisión
3/ Complementar LT Irapuato II - 73500 - Irapuato I y Irapuato II - 73600 - Irapuato I
4/ Alcanzar límite operativo mínimo 1,506 A (300 MVA)
5/ Alcanzar límite operativo mínimo 1,100 A (218 MVA)
6/ Icc mínimo 31.5 kA', NULL, NULL, NULL, NULL, N'Este es un proyecto clasificado como continuado es decir, se segregaron las obras y suministros lo que ocasionó que el proyecto se demorara para converger la llegada de los suministros y la construcción.
Una de las causas más relevantes del atraso del proyecto han sido los conflictos sociales para la línea que ha impedido continuar con el proceso constructivo.', N'Se tomó la decisión de realizar el proyecto en fases con la finalidad de poder concluirlo conforme a la Fecha Factible de Término.
Se realizaron propuestas para el cambio de trayectoria dada la poca disponibilidad para negociar con los inconformes.', N'En proceso Etapa 3 Ejecución / Construcción.
Se realizan las gestiones necesarias para dar continuidad a la construcción del proyecto teniendo el compromiso de concluir en DIC.2025.', N'Portafolio Activo', N'P16-OC3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 133.33, 0.00, 20.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        38, 38, N'CE', N'Compensación capacitiva en la zona Querétaro', N'Presupuestal', 2018, N'En Operación', 88.49, N'SE Conín MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Parque Innovación MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Cimatario MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Antea MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Querétaro MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Querétaro Potencia MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'SE Conín, 1 Capacitor - 115 kV – 22.5 MVAR
Se energizó el 19.ABR.2024
SE Parque Innovación, 1 Capacitor - 115 kV – 15 MVAR
Se energizó el 27.DIC.2023
SE Cimatario, 1 Capacitor - 115 kV – 30 MVAR
Se energizó el 12.ABR.2024
SE Antea, 1 Capacitor - 115 kV – 22.5 MVAR
Se energizó el 04.FEB.2024
SE Querétaro, 1 Capacitor - 115 kV – 15 MVAR
Se energizó el 03.DIC.2023
SE Querétaro Potencia, 1 Capacitor - 115 kV – 30 MVAR
Se energizó el 02.DIC.2023', N'Portafolio Activo', N'P18-OC9', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 135.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        39, 39, N'OC', N'San Luis Potosí Banco 3 (traslado)', N'Presupuestal', 2018, N'Ejecución/Construcción', 116.52, N'SE San Luis Potosí Banco 3 (traslado)
4 AT - 1F - 33.33 MVA - 133.33 MVA - 230/115 kV
Reubicación de tres acometidas de líneas de 115 kV', NULL, NULL, NULL, NULL, N'Este es un proyecto clasificado como continuado es decir, se segregaron las obras y suministros lo que ocasionó que el proyecto se demorara para converger la llegada de los suministros y la construcción.', N'Se tomó la decisión de realizar el proyecto en fases con la finalidad de poder concluirlo conforme a la Fecha Factible de Término.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P18-OC1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 133.33, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        40, 40, N'NO', N'Quila MVAr (Traslado)', N'Presupuestal', 2018, N'En Operación', 22.68, N'SE Quila MVAR (traslado)
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 20.DIC.2022.', N'Portafolio Activo', N'P18-NO1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        41, 41, N'CE', N'Línea de transmisión Conín - Marqués Oriente y San Idelfonso - Tepeyac', N'Presupuestal', 2018, N'Por concursar', 328.67, N'LT Conín –Marqués Tendido 2do. Circuito
115 kV – 1C – 5.1 km-C – 795 ACSR
*LT San Ildefonso – Tepeyac
115 kV – 1C – 9.5 km-C – 795 ACSR
LT San Idelfonso - 73270 - El Sauz
115 kV – 1C – 17.7 km-C
SE Conin
1 Alimentador 115 kV
SE Tepeyac
1 Alimentador 115 kV
SE Idelfonzo y El Sauz
Adecuaiones de EEP serie

* Obra realizada por un tercero', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación y asignación de recursos.
Derivado del alcance en las Metas Físicas, se requirió el prolongar los periodos en las revisiones, validación y liberación de documentos principalmente ICM y paquete de concurso.', N'Se integra el Pliego de Requisitos, con la participación de la GRT cuidando las mejores condiciones para que se adjudique el proyecto.
En proceso de actualización la ICM, gestión de plurianualidad,  suficiencia presupuestal y completar paquete de concurso para solicitar su publicación.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas y será realizado por DIPI/CPTT.
31.OCT.25 Solicitud de Publicación
05.NOV.25 Publicación
22.ENE.26 Fallo

Este proyecto está asociado al siguiente proyecto de Distribución:
SE Tepeyac
Ampliación T1 - 30 MVA - 115/34.5 kV
SE Aeroespacial
Ampliación T3 - 20 MVA - 115/34.5 kV
24.OCT.25 Solicitud de Publicación
29.OCT.25 Publicación', N'Portafolio Activo', N'P16-OC4', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 0.00, 32.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        42, 42, N'NE', N'Traslado de Reactores en el Noreste', N'Presupuestal', 2018, N'En Operación', 102.29, N'SE Frontera MVAr (traslado)
1 R Barra - 3F - 50 MVAr - 400 kV
1 Alimentador 400 kV Reactor 
De la SE Guemez
SE Río Escondido MVAr (traslado)
1 R Barra - 3F - 75 MVAr - 400 kV
1 Alimentador 400 kV Reactor
De la SE Villa de García', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 28.ENE.2024 el Reactor 1 de la SE Frontera.
Se energizó el 25.FEB.2024 el Reactor 1 de la SE Río Escondido.', N'Portafolio Activo', N'P18-NE4', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 125.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        43, 43, N'OC', N'Enlace Tepic II - Cerro Blanco', N'Presupuestal', 2018, N'En Operación', 18.30, N'LT Tepic II – Cerro Blanco 
Reemplazo TC''s, LT Tepic II – A3620 – Cerro Blanco (ambos extremos).
Reemplazo TO''s LT Tepic II – A3630 – Cerro Blanco (ambos extremos).', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 24.ABR.2023.', N'Portafolio Activo', N'P18-OC2', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        44, 44, N'PE', N'Elevación de Buses de 115 kV en la SE Nizuc', N'Presupuestal', 2018, N'Ejecución/Construcción', 52.68, N'SE Nizuc
Recalibración de Bus, obra civil y electromecánica', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación y asignación de recursos.
Derivado del alcance en las Metas Físicas, se requirió el prolongar los periodos en las revisiones, validación y liberación de documentos principalmente ICM y paquete de concurso.', N'Se integra el Pliego de Requisitos, con la participación de la GRT cuidando las mejores condiciones para que se adjudique el proyecto.
Se cuenta con actualización y validación de ICM, plurianualidad, gestión de suficiencia presupuestal y paquete de concurso para solicitar su publicación.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
14.JUL.25 Solicitud de Publicación
16.JUL.25 Publicación
17.SEP.25 Fallo', N'Portafolio Activo', N'P17-M03', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        45, 45, N'NT', N'352 SLT Transformación y transmisión Querétaro, Isla del Carmen, Nuevo Casas Grandes y La Huasteca
(Nuevo Casas Grandes Banco 3)', N'PIDIREGAS 2021', 2018, N'En Operación', 180.85, N'SE Nuevo Casas Grandes
3 AT - 1F - 33.3 MVA - 100 MVA - 230/115 kV
SE Nuevo Casas Grandes MVAr
1 Capacitor - 115 kV - 30 MVAR
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 21.OCT.2023.', N'Portafolio Activo', N'P17-NT2', N'Concluido y en operación', NULL, NULL, NULL, 100.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        46, NULL, N'CE', N'352 SLT Transformación y transmisión Querétaro, Isla del Carmen, Nuevo Casas Grandes y La Huasteca
(Querétaro Potencia Banco 4)
Etapa 2 => Reemplazo de equipo de 115 kV y recalibración de bus de 115 kvV', N'PIDIREGAS 2021', 2018, N'Por concursar', 50.00, N'Reemplazo de equipo en Subestaciones
SE Querétaro Uno
1 Interruptor de Potencia 115 kV (40 kA)
(73180)
2 Cuchillas desconectadoras 115 kV (40 kA)
(73038, 73188)
SE Querétaro Potencia
3 Interruptores de Potencia 115 kV (40 kA)
(73560, 73720, 72030)
9 Cuchillas desconectadoras 115 kV (40 kA)
(73561, 73566, 73568, 73569, 73581, 73721, 73726, 73728 y 73729)
Recalibración Bus 115 kV
SE Querétario Maniobras
2 Interruptores de Potencia 115 kV (40 kA)
(73840 y 73580)', NULL, NULL, NULL, NULL, N'Como parte del proceso de revisión y tratamiento a los proyectos en la Etapa 2 se realizarán los alcances de las obras en 115 kV.', N'La GRTCE solicitó a la RRCCE el cuadernillo para conocer los importes de construcción, actividades previas y supervisión para las obras pendientes de realizar en 115 kV.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
24.JUL.26 Solicitud de Publicación
30.JUL.26 Publicación
09.OCT.26 Fallo', N'Portafolio Activo', N'P17-OC10', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        47, 47, N'PE', N'352 SLT Transformación y transmisión Querétaro, Isla del Carmen, Nuevo Casas Grandes y La Huasteca
(Puerto Real Bancos 1 y 2)', N'PIDIREGAS 2021', 2018, N'Ejecución/Construcción', 1081.57, N'LT Escárcega Potencia – Punto de Inflexión Sabancuy
230 kV – 2C – 69.30 km-C – 1113 ACSR/AS TA /1
LT Punto de Inflexión Sabancuy -  Puerto Real (Tramo Aéreo)
230 kV – 2C – 29.92 km-C – 1113 ACSR/AS TA
LT Sabancuy - Carmen (tramo marino)
230 kV – 2C – 11.0 km-C – ACCC/TW 1020 KCM TA
LD Puerto Real – Palmar
34.5 kV – 2C – 38 km-C - AAC 477 KCM
SE Puerto Real Bancos 1 y 2
7 AT – 1F – 50 MVA - 350 MVA - 230/115 kV
6 Alimentadores
SE Puerto Real Banco 3 (Traslado)
1 T – 3F – 6.25 MVA – 115/34.5 kV
SE Palmar 
2 Alimentadores 34.5 kV
SE Escárcega Potencia
2 Alimentadores 230 kV

1/ Tendido segundo circuito', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P17-PE2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 350.00, 0.00, 110.22, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        48, 48, N'OR', N'352 SLT Transformación y transmisión Querétaro, Isla del Carmen, Nuevo Casas Grandes y La Huasteca
(Las Mesas Banco 1)', N'PIDIREGAS 2021', 2018, N'En concurso', 886.49, N'LT Las Mesas – Huejutla II
115 kV –1C – 55 km-C - 795 ACSR TA
LT Las Mesas Entronque Axtla – Tamazunchale
115 kV – 2C – 13.2 km-C - 477 ACSR TA
SE Las Mesas Banco 1
4 AT – 1F – 75 MVA - 300 MVA - 400/115 kV
3 Alimentadores 115 kV
SE Axtla 
1 Alimentador 115 kV
SE Huejutla II 
1 Alimentador 115 kV
SE Huasteca
1 Alimentador 115 kV', NULL, NULL, NULL, NULL, N'El proyecto fue reevaluado en 2024. 
Se requirieron largos periodos de revisiones, validación y liberación de documentos principalmente para actualización de ICM.
Se presentaron atrasos en las actividades y estudios previos por parte de CPTT por lo que el proyecto fue reprogramado para publicarse en 2024.
Debido al cambio de variables y actualización de precios, se requiere la solicitud de incrementar el techo de inversión financiada del proyecto para su viabilidad.', N'Se realizan las gestiones de actualización de plurianualidad y solicitud de incrementar el techo de inversión financiada para el proyecto, lo cual se podrá ver reflejado en agosto de 2025 y publicado en el PEF 2026. 
Se conforma el paquete de concurso para solicitar su publicaciónen el 3er trimestre de 2025.', N'El proyecto está en concurso con el procedimiento No. CFE-0004-CACOT-0002-2025.
02.OCT.25 Publicación
06.ENE.26 Fallo', N'Portafolio Activo', N'P17-NE2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 300.00, 0.00, 68.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        49, 49, N'PE', N'354 SLT Corriente Alterna Submarina Playacar - Chankanaab II', N'PIDIREGAS 2021', 2018, N'En Proceso de Decisión', 3783.15, N'LT Playa del Carmen – Chankanaab II
115 kV - 1C - 33.44 km-C
Cable subterráneo 11.50 km-C
Cable submarino 18.90 km-C
(ampacidad equivalente a 795 ACSR/AS) CS-CS 1/
Cable submarino p/enlazar las SEs Playacar y Chankanaab II)
LT Chabkanaab II - Chankanaab
115 kV -1C/3F - 2.75 km-C - 750 MCM - Al XLPE (Cable Subterráneo)
Chankanaab II entronque Chankanaab - Cozumel / 2
34.5 kV - 2C - 0.1 km-C
Red de media tensión en la isla Cozumel
13.8 kV - 1C - 18.6 km-C / 2
(adicionar el 10% de incertidumbre a los km-C)
SE Chankanaab II Bancos 3 y 4 (SF6)
2 T - 3F - 30 MVA - 115/13.8 kV
SE Chankanaab Banco 6 (SF6)
1 T - 3F - 30 MVA - 115/13.8 kV
SE Chankanaab II Bancos 1 y 2 (Traslado)
2 T - 3F - 20 MVA (40 MVA) - 34.5/13.8 kV
SE Chankanaab Banco 3 y 5 (Traslado)
2 T - 3F - 15.25 MVA (30.5 MVA) - 34.5/13.8 kV
SE Chankanaab II MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Chankanaab MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Chankanaab II MVAr
1 Capacitor - 3.6 MVAr - 13.8 kV
1 Alimentador 115 kV Capacitor
SE Chankanaab MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV
1 Alimentador 115 kV Capacitor
SE Playa del Carmen
1 Alimentador 115 kV
SE Chankanaab II
2 Alimentadores 115 kV (en SF6)
SE Chankanaab
1 Alimentador 115 kV (en SF6)
SE Chankanaab II
12 Alimentadores 13.8 kV
SE Chankanaab
6 Alimentadores 13.8 kV', NULL, NULL, NULL, NULL, N'Previo a la etapa de contratación se tuvieron atrasos en la definición de la modalidad de contratación sin embargo el proyecto está en concurso sin problemas para que sea adjudicado.', N'Se conformó el pliego de requisitos con la participación de las áreas involucradas para que se adjudique el proyecto y no se declare desierto.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
17.OCT.25 Solicitud de Publicación
23.OCT.25 Publicación
07.FEB.26 Fallo', N'Portafolio Activo', N'P15-PE1', N'En Concurso y Por Concursar', N'2026.2', N'6.1', N'Sí', 0.00, 22.50, 36.19, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        50, 50, N'OR', N'355 SLT Suministro de energía en la Zona de Operación de Transmisión Veracruz (antes S. E. Olmeca Banco 1)', N'PIDIREGAS 2021', 2018, N'Por concursar', 3077.76, N'LT Manlio Fabio Altamirano – Temascal II
400 kV – 2C – 196.0 km-C – 1113 ACSR TA
LT Manlio Fabio Altamirano Entronque Infonavit – 73470 – Dos Bocas
115 kV – 2C – 38.6 km-C – 795 ACSR TA
(Se formarán las siguientes dos Líneas de Transmsiión:
LT Manlio Fabio Altamirano - Infonavit y LT Manlio Fabio Altamirano - Dos Bocas)
LT Manlio Fabio Altamirano Entronque Veracruz I – 73370 – Dos Bocas
115 kV – 2C – 38.6 km-C – 795 ACSR TA
Se formará la siguiente Línea de Transmsiión:
LT Manlio Fabio Altamirano - Veracruz I
el tramo Manlio Fabio Altamirano - Dos Bocas
se utiliza para entronque de la LT siguiente:
LT Manlio Fabio Altamirano - Dos Bocas (del punto anterior) entronque
LT Paso del Toro - 73V10 - Boca del Río
115 kV – 2C – 3 km-C – 795 ACSR TA
Se formará la siguiente Línea de Transmsiión:
LT Manlio Fabio Altamirano - Paso del Toro y
LT Dos Bocas - Boca del Río
SE Manlio Fabio Altamirano
4 T - 1F - 125 MVA - 500 MVA - 400/115 kV
2 Alimentadores en 400 kV 
(Interconexión de las LT''s hacia Temascal II)
Nueva Subestación Eléctrica tipo convencional en el predio de la
SE Manlio Fabio Altamirano
4 alimentadores para la interconexión de las LTs de 115 kV.
SE Temascal III (Ampliación)
2 Alimentadores en 400 kV (interconexión de las LT''s hacia Manlio Fabio)
SE Franboyanes
Modernización de 3 interruptores y equipos asociados en 115 kV
Adecuaciones de equipos de PCyM en las subestaciones:
SE Infornativ
SE Veracruz I
SE Boca del Río
SE Paso del Toro
Obras requeridas por el cambio de interconexión de la LT hacia SE Manlio Fabio Altamirano.', NULL, NULL, NULL, NULL, N'El proyecto a sido elevado a Concurso en cuatro ocaciones desde el año 2023, resultando desierto en todas las ocaciones en tres ocaciones por no tener Ofertas y en la ultima ocación no hubo ofertar solventes técnicamente.', N'Se han actualizado las condiciones comerciales de la ICM y conformado el expediente para solicitar un nuevo procedimiento de concurso.', N'El proyecto está en concurso con el procedimiento No. CFE-0004-CACOA-0003-2025.
04.SEP.25 Fallo
22.OCT.25 Firma del Contrato y presentación de Garantía de Cumplimiento.', N'Portafolio Activo', N'P18-OR1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 500.00, 0.00, 276.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        51, 51, N'CE', N'Valle de Mezquital Banco 1 (traslado)', N'En proceso de definición', 2018, N'Instruido y CON priorización', 233.00, N'LT Valle del Mezquital Entronque CH Zimapán – Dañu (93050)
230 kV – 2C – 0.2 km-C – TA
LT Valle del Mezquital Entronque Zimapán – Tap Zimapán (73260)
115 kV – 2C – 0.2 km-C – TA
LT Valle del Mezquital –Tap Zimapán
115 kV – 1C – 3 km-C – TA
SE Valle del Mezquital Banco 1 (traslado)
4 AT - 1F - 33.33 MVA - 133.33 MVA - 230/115 kV
SE Huichapan MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Humedades MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización conciliada entre Transmisión, la Coordinación de Vinculación de la DP y el CENACE.
Pendiente definición de metodología de evaluación y esquema de financiamiento.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-OC5', N'En análisis por priorizar', NULL, NULL, N'Sí', 133.33, 30.00, 3.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        52, 52, N'NT', N'Francisco Villa Banco 3', N'PIDIREGAS PPEF 2026', 2018, N'En Proceso de Decisión', 391.04, N'SE Francisco Villa Banco 3
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV
Recalibración del Bus de 230 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'P17-NT5', N'En Concurso y Por Concursar', N'2026.4', N'12.1', N'Sí', 100.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        53, 53, N'BC', N'Chapultepec entronque Cerro Prieto II - San Luis Rey', N'Pendiente de definir', 2018, N'Instruido y SIN priorización', 79.00, N'LT Chapultepec Entronque Cerro Prieto II - San Luis Rey
230 kV - 2C - 4 km-C - 1113 - ACSR-TA
SE Chapultepec
2 Alimentadores 230 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P17-BC16', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 4.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        54, 54, N'BC', N'Loreto MVAr', N'Fibra E', 2018, N'En Proceso de Decisión', 19.26, N'SE Loreto MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-BS1', N'En Concurso y Por Concursar', N'2026.1', N'2.1', N'Sí', 0.00, 7.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        55, 55, N'BC', N'Rubí entronque Cárdenas – Guerrero', N'Fibra E', 2018, N'En Proceso de Decisión', 241.25, N'LT Rubí Entronque Cárdenas - Guerrero
115 kV – 2C - 8.8 km-C – 1 Cond/f – 795 ACSR – TA/PA /1
 SE Rubí
2 Alimentadores de 115 kV LT Rubí Entronque Cárdenas - Guerrero /1
 
1/ Operación inicial 69 kV, aislado 115 Kv', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-BC2', N'En Concurso y Por Concursar', N'2026.4', N'11.2', N'Sí', 0.00, 0.00, 8.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        56, 56, N'BC', N'Frontera entronque Industrial - Universidad', N'Fibra E', 2018, N'En Proceso de Decisión', 236.53, N'LT Frontera Entronque Industrial – Universidad
115 kV - 2C - 6 km-C - 795 ACSR - PA /2
SE Frontera
2 Alimentadores 115 kV LT Frontera Entronque Industrial – Universidad /3 

/2 Cable Subterráneo con capacidad equivalente a 795 ACSR
3/ Operación inicial 69 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-BC3', N'En Concurso y Por Concursar', N'2026.2', N'5.1', N'Sí', 0.00, 0.00, 6.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        57, 57, N'OC', N'Compensación capacitiva en la zona Zacatecas', N'Pendiente de definir', 2018, N'Instruido y SIN priorización', 32.00, N'SE Fresnillo Sur MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Jerez MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE San Jerónimo MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P18-OC3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 37.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        58, 58, N'VM', N'Línea de Transmisión Deportiva-Toluca', N'En proceso de definición', 2018, N'Instruido y CON priorización', 122.00, N'LT Deportiva – Toluca (Recalibración)
230 kV – 1C – 16 km-C – 1113 CSR TA
SE Deportiva
1 Alimentador 230 kV
Remplazo de Equipo Primario
SE Toluca
1 Alimentador 230 kV
Remplazo de Equipo Primario
SE San Bernabé
1 Alimentador 230 kV
Remplazo de Equipo Primario', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P17-CE2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 16.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        59, 59, N'OC', N'Loreto y Villa Hidalgo MVAr', N'Fibra E', 2018, N'En Proceso de Decisión', 116.32, N'LT Ojo Caliente - Estancia de Ánimas
115 kV - 1C - 3.0 km-C 795 ACSR TA
SE Ojo Caliente
1 Alimentador 115 kV 
SE Loreto MVAr (traslado desde SE Villa Hidalgo)
1 Capacitor - 10 MVAr - 115 kV
SE Villa Hidalgo MVAr (sustitución)
1 Capacitor - 22.5 MVAr - 115 kV /1
1 Alimentador 115 kV Capacitor

1/ Sustitución del capacitor existente de 10 MVAr', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-OC9', N'En Concurso y Por Concursar', N'2026.4', N'12.2', N'Sí', 0.00, 32.50, 3.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        60, 60, N'OC', N'Compensación capacitiva en la zona Guadalajara', N'Fibra E', 2018, N'Por concursar', 132.36, N'Castillo MVAr
1 Capacitor - 69 kV - 18 MVAr
1 Alimentador 69 kV Capacitor
Chapala MVAr  (traslado desde SE Mojonera)
1 Capacitor - 69 kV - 8.1 MVAr
1 Alimentador 69 kV Capacitor
Miravalle MVAr
1 Capacitor - 69 kV - 24 MVAr /2
1 Alimentador 69 kV Capacitor
Mojonera MVAr
1 Capacitor - 69 kV - 24 MVAr
1 Alimentador 69 kV Capacitor
El Sol MVAr
1 Capacitor - 69 kV - 24 MVAr /3
1 Alimentador 69 kV Capacitor
San Agustín MVAr
1 Capacitor - 69 kV - 24 MVAr
1 Alimentador 69 kV Capacitor
 
1/Solicitar CENACE alternativa por falta espacio en Subestación Eléctrica.
2/ La subestación aún no se construye por lo que no es factible)
3/ No hay espacio en la SE Pinar, por lo que la Obra no es factible)', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P18-OC8', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 122.10, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        61, 61, N'NE', N'Nueva Rosita Banco 2', N'PIDIREGAS PPEF 2026', 2018, N'En Proceso de Decisión', 390.20, N'SE Nueva Rosita Banco 2
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'P17-NE1', N'En Concurso y Por Concursar', N'2026.4', N'11.2', N'Sí', 100.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        62, 62, N'OC', N'Expansión de las zonas Uruapan y Apatzingán', N'Fibra E', 2018, N'En Proceso de Decisión', 1653.23, N'LT Paracuaro Entronque Mazamitla - A3130 - Pitirera
400kV - 2C - 0.2 km-C - 2 Cond/f - 1113 ACSR - TA
LT Parácuaro Entronque Uruapan Potencia - 73280 - Apatzingan I
230 kV - 2C - 1.6 km-C - 1 Cond/f - 1113 ACSR - TA /1
LT Paracuaro Entronque Pradera - 73700 - Condembaro
115 kV - 2C - 18 km-C - 1 Cond/F - 477 ACSR - TA
LT Pradera - Valle Verde
115 kV - 1C - 9.0 km-C - 1 Cond/f - 477 ACSR - TA
LT Pátzcuaro Sur - Cerro Hueco
115 kV - 1C - 37.0 km-C - 1 Cond/f - 477 ACSR - TA /2
SE Parácuaro Banco 1
4 AT - 1F - 125 MVA - 500 MVA - 400/115 kV
SE Cerro Hueco
1 T - 3F - 30 MVA - 115/69 kV
SE Parácuaro
1 Alimentador 400 kV LT Parácuaro - Mazamitla
1 Alimentador 400 kV LT Parácuaro - Pitirera
1 Alimentador 115 kV LT Parácuaro - Condembaro
1 Alimentador 115 kV LT Parácuaro - Pradera
1 Alimentador 115 kV LT Parácuaro - Apatzingán I
1 Alimentador 115 kV LT Parácuaro - Uruapan Potencia
1 Alimentador 115 kV (Amarre)
SE Pradera
1 Alimentador (Ampliación) 115 kV LT Pradera - Valle Verde
SE Valle Verde
1 Alimentador (Ampliación) 115 kV LT Valle Verde - Pradera
SE Pátzcuaro Sur
1 Alimentador (Ampliación) 115 kV LT Pátzcuaro Sur - Cerro Hueco
SE Cerro Hueco
1 Alimentador (Ampliación) 115 kV LT Cerro Hueco - Pátzcuaro Sur
SE Uruapan III
3 Transformadores de Corriente 115 kV LT Uruapan III - 73720 - Uruapan Potencia /3
3 Transformadores de Corriente 115 kV LT Uruapan III - 73390 - La Esperanza /3,5
SE Uruapan Potencia
3 Transformadores de Corriente 115 kV LT Uruapan Potencia - 73720 - Uruapan III /3
3 Transformadores de Corriente 115 kV LT Uruapan Potencia - 73710 - Mirador /3
3 Transformadores de Corriente 115 kV LT Uruapan Potencia - 73280 Apatzingan I /4
2 Cuchillas desconectadoras 115 kV /6
SE Mirador
3 Transformadores de Corriente 115 kV LT Mirador - 73710 - Uruapan Potencia /3
SE Apatzingan I
3 Transformadores de Corriente 115 kV LT Apatzingan I - 73280 - Uruapan Potencia /4
SE La Esperanza
3 Transformadores de Corriente 115 kV LT La Esperanza - 73390 - Uruapan III /3,5
3 Transformadores de Corriente 115 kV LT La Esperanza - 73440 - Mirador /3
SE Pradera
3 Transformadores de Corriente 115 kV LT Pradera - 73700 - Condembaro /3
 
1/ Operada en 115 kV, aislada en 230 kV
2/ Tendido del primer circuito
3/ Alcanzar límite operativo 131 MVA
4/ Alcanzar límite operativo 180 MVA
5/ Futura trayectoria Uruapan III - Uruapan Oriente - La Esperanza
6/ Bahías 77011 y 77018 Icc mínima de 20 kA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P18-OC4', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 530.00, 0.00, 72.38, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        63, 63, N'NO', N'Sustitución de equipos de protección limitados por capacidad de corto circuito en la GRT Noroeste', N'Fibra E', 2018, N'En Proceso de Decisión', 19.07, N'SE Dynatech
1 Interruptor de Potencia 115 kV /2,3
SE Hermosillo Nueve
2 Interruptor de Potencia 115 kV /2,4
SE Hermosillo Ocho
2 Interruptor de Potencia 115 kV /2,5
SE Hermosillo Uno
1 Interruptor de Potencia 115 kV /2,6
SE Los Mochis Uno
3 Interruptor de Potencia 115 kV /2,7

2/ Considera materiales (cables, equipo de control, conectores y puentes) y obra civil.
3/ Bahía 73130 Capacidad 40 kA
4/ Bahías 72010 y 73070 Capacidad 40 kA
5/ Bahías 73780 y 77010 Capacidad 40 kA
6/ Bahía 77010 Capacidad 40 kA
7/ Bahías 72030, 73190 y 73750 Capacidad 40 kA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M18-NO1', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        64, 64, N'NT', N'Modernización de la subestación Cuadro de Maniobras Cerro del Mercado', N'Pendiente de definir', 2018, N'Instruido y SIN priorización', 19.00, N'SE Maniobras Cerro del Mercado
Bahía MCM 73890
1 Interruptores de Potencia 115 kV
2 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV
Bahía MCM 73490
1 Interruptores de Potencia 115 kV
2 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV
Bahía MCM 73900
1 Interruptores de Potencia 115 kV
2 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV

Estructura Mayor Bahías 115 kV
Caseta Integral Bahías de Línea
3 Esquemas de Protección sección tipo LT-7-87-87-AN-IN 115 kV
Esquemas de Procción, Control, Medición y Comunicaciones
Obras Electromecánica
Obra Civil', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M18-NT1
M01-GCRN', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        65, 65, N'CE', N'Suministro de energía eléctrica Zona Tlaxcala
(antes LT La Malinche - Altzayanca Maniobras)', N'En proceso de definición', 2018, N'Instruido y CON priorización', 500.00, N'LT Zocac – La Malinche
230 kV - 2C - 14.5 km-C - 1113 ACSR/AS /3
LT Apizaco II - La Malinche
115 kV - 2C - 10 km-C - 795 ACSR/AS /3
SE Zocac
1 Alimentador 230 kV
SE Apizaco II
1 Alimentador 115 kV
SE La Malinche 
1 Alimentador 230 kV
1 Alimentador 115 kV

/3 Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización conciliada entre Transmisión, la Coordinación de Vinculación de la DP y el CENACE.
Pendiente definición de metodología de evaluación y esquema de financiamiento.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P18-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 24.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        66, 66, N'OC', N'Línea de transmisión Silao Potencia - Las Colinas', N'Fibra E', 2018, N'Con acuerdo de Cancelación', 508.23, N'LT Silao Potencia – Las Colinas
115 kV – 1C - 15.6 km-C - 1 C/F - 795 CSR TA/PA
SE Silao Potencia (ampliación) 
1 Alimentador 115 kV LT Silao Potencia – Las Colinas
SE Las Colinas (ampliación) 
1 Alimentador 115 kV LT Las Colinas - Silao Potencia', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Proyecto con petición de CANCELACIÓN del portafolio de proyectos.
En el PAMRNT del 2025, este proyecto aparece como CANCELADO, por lo que Transmisión gestionará la baja y retiro del portafolio de proyectos instruidos por SENER.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P17-OC7', N'Con acuerdo de cancelación', NULL, NULL, NULL, 0.00, 0.00, 15.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        67, 67, N'OC', N'León IV entronque Aguascalientes Potencia - León III', N'Fibra E', 2018, N'En Proceso de Decisión', 1342.20, N'LT Pedro Moreno Entronque Aguascalientes Potencia - 93330 - León IV
230 kV – 2C – 2.0 km-C - 1 Cond/f - 1113 ACSR TA
LT Pedro Moreno Entronque Aguascalientes Potencia - 93960 - León IV
230 kV – 2C – 2.0 km-C - 1 Cond/f - 1113 ACSR TA
LT Pedro Moreno Entronque La Virgen - 73980 - Lagos Galera
115 kV – 2C – 2.0 km-C - 1 Cond/f - 795 ACSR TA
LT Pedro Moreno - Tecuán
115 kV – 1C – 38.60 km-C - 1 Cond/f - 795 ACSR TA
Recalibración LT León III - 73L40 - León Norte
115 kV – 1C – 1.15 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1
Recalibración LT León Malecón - 73730 - Cerro Gordo
115 kV – 1C – 4.63 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1,2
Recalibración LT Gran Jardín - 73940 - Lagos Galera
115 kV – 1C – 0.07 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1,2
Recalibración LT León III - 73L80 - Cerro Gordo
115 kV – 1C – 1.73 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1,2
Recalibración LT León IV - 73780 - León Oriente
115 kV – 1C – 0.75 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1,2
Recalibración LT León IV - 73830 - León Oriente
115 kV – 1C – 0.75 km-C - 1 Cond/f - Cable de Potencia Subterráneo /1,2
SE Pedro Moreno Banco 1 (SE Nueva)
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Pedro Moreno (SE Nueva)
1 Alimentador 230 kV LT Pedro Moreno - 93OC0 -  Aguascalientes Potencia
1 Alimentador 230 kV LT Pedro Moreno - 93OC0 -  León IV
1 Alimentador 230 kV LT Pedro Moreno - 93OC0 -  Aguascalientes Potencia
1 Alimentador 230 kV LT Pedro Moreno - 93OC0 -  León IV
1 Alimentador 230 kV (Amarre)
1 Alimentador 115 kV LT Pedro Moreno - 73OC0 -  Virgen
1 Alimentador 115 kV LT Pedro Moreno - 73OC0 -  Lagos Galera
1 Alimentador 115 kV LT Pedro Moreno - 73OC0 -  Tecuán
1 Alimentador 115 kV (Amarre)
SE Tecuán 
1 Alimentador 115 kV LT Tecuán - 73OC0 -  Pedro Moreno
Recalibración Bus 115 kV /3
SE Lagos Galera
Recalibración Bus 115 kV /1
3 Transformadores de Corriente 115 kV LT Lagos Galera - 73940 - Gran Jardín /1,4
SE Lagos Moreno
Recalibración Bus 115 kV /1
3 Transformadores de Corriente 115 kV LT Lagos Moreno - 73970 - Virgen /1,4
SE Gran Jardín (sustitución)
3 Transformadores de Corriente 115 kV LT Gran Jardín - 73940 - Lagos Galera /1
SE La Virgen (sustitución)
3 Transformadores de Corriente 115 kV LT La Virgen - 73970 - Lagos Moreno /1
3 Transformadores de Corriente 115 kV LT La Virgen - 73980 - Lagos Galera /1
SE León Alfaro (sustitución)
3 Transformadores de Corriente 115 kV LT León Alfaro - 73820 - León IV /1
SE León Oriente (sustitución)
3 Transformadores de Corriente 115 kV LT León Oriente - 73780 - León IV /1
3 Transformadores de Corriente 115 kV LT León Oriente - 73830 - León IV /1
SE León III (sustitución)
9 Cuchillas desconectadoras 115 kV /5
3 Transformadores de Corriente 115 kV LT León III - 73L40 - León Norte /1 
SE Tecuán (sustitución)
3 Transformadores de Corriente 115 kV LT Tecuán - 73390 - Aguascalientes Potencia /3
 
1/ Alcanzar límite operativo de 179 MVA
2/ Tramo Subterráneo
3/ Alcanzar límite operativo de 133 MVA
4/ Sustitución
5/ 73L11, 73L18, 73L19, 73L21, 73L28, 73L29, 73L41, 73L48, y 73L49 Icc mínima de 31.5 kA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio y Plan de Ejecución autorizados.
En revisión y autorización del Plan Financiero.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P18-OC5', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 0.00, 59.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        68, 68, N'BC', N'Camino Real MVAr', N'Fibra E', 2018, N'En Proceso de Decisión', 17.73, N'SE Camino Real MVAr
1 Capacitor - 22.5 MVAr - 115 kV 
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P16-BS2', N'En Concurso y Por Concursar', N'2026.1', N'2.1', N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        69, 69, N'SE', N'Línea de Transmisión Corriente Alterna en Tapachula Chiapas', N'Fibra E', 2018, N'En Proceso de Decisión', 1432.73, N'LT Angostura – Tapachula Potencia
400 kV - 2C - 193.5 km-C - 2Cond/f - 1113 ACSR - TA /1
 SE Tapachula Potencia MVAr 
3 R Línea - 1F - 25 MVAr - 75 MVAr - 400 kV
(Incluye reactor de neutro)
 SE Angostura
1 Alimentador 400 kV LT Angostura - Tapachula Potencia
 SE Tapachula Potencia
1 Alimentador 400 kV LT Tapachula Potencia - Angostura 
Conversión SE 400 kV 
Arreglo Barra en anillo a arreglo Interruptor y medio
Interconexión Bahías 
LT Tapachula Potencia - A3T30 - Angostura con LT Tapachula Potencia - A3T00 Los Brillantes
LT Tapachula Potencia - A3XX0 - Angostura con THP Banco 1 500 MVA - 400/115 kV
 
/1 Tendido del segundo Circuito', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P15-OR1', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 0.00, 75.00, 212.85, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        70, 70, N'NO', N'Construcción de una bahía en la SE Culiacán I', N'Pendiente de definir', 2018, N'Instruido y SIN priorización', 20.00, N'SE Culiacán Uno
1 Interruptor de Potencia 115 kV
3 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
Equipo de Protección, Control y Medición
3 Apartarrayos 115 kV

Estructura Mayor Bahías 115 kV
Eliminar el TAP de la LT 73420 TSR-CUU-HGA', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M18-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        71, 71, N'NE', N'Red de transmisión Reynosa – Monterrey', N'Pendiente de definir', 2018, N'Instruido y SIN priorización', 5922.50, N'LT Aeropuerto - Reynosa Maniobras
400 kV - 2C -  29 km-C - 1113 ACSR TA /3
LT Jacalitos – Regiomontano
400 kV - 2C - 180 km-C - 1113 ACSR TA /3
LT Reynosa Maniobras - Jacalitos
400 kV - 2C - 66 km-C - 1113 ACSR TA
LT Ternium - Regiomontano
400 kV - 2C - 30 km-C - 1113 ACSR TA /3
SE Jacalitos MVAr
4 R Barra - 1F - 33.33 MVAr - 133.33 MVAr - 400 kV
4 R Línea - 1F - 16.6 MVAr - 66.7 MVAr - 400 kV
(Incluye reactor de neutro)
SE Jacalitos
2 Alimentadores en 400 kV
SE Reynosa Maniobras
2 Alimentadores en 400 kV
Regiomontano
2 Alimentadores en 400 kV
SE Ternium
1 Alimentador en 400 kV
SE Aeropuerto
1 Alimentador de 400 kV

/3 Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2018 y sin priorización.
Transmisión había solicitado la cancelación y baja del portafolio de proyectos sin embargo en mesas de trabajo con SENER, se acordó que este proyecto se mantendrá dentro del portafolio de proyectos instruidos por SENER.', N'B4: Sin priorizar', N'I16-NE3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 200.00, 305.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        72, 72, N'OC', N'Campos Banco 1 (SF6)', N'Presupuestal Compartido', 2018, N'En Operación', 17.33, N'LT Campos – Terminal de Gas Manzanillo
115 kV - 1C - 0.1 km-C
SE Campos Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 19.OCT.2023.', N'Portafolio Activo', N'D18-OC6', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        73, 73, N'NT', N'Sauzal Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 39.24, N'LT El Sauzal Entronque Zaragoza - Medanos
115 kV - 2C – 2.4 km-C
SE El Sauzal Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE El Sauzal MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 26.NOV.2024.', N'Portafolio Activo', N'D18-NT2', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 2.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        74, 74, N'PE', N'Hunxectamán Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 33.21, N'LT Hunxectamán Entronque Mérida - Lerma
115 kV - 2C – 1.0 km-C
SE Hunxectamán Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Hunxectamán MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 27.ABR.2025.', N'Portafolio Activo', N'D18-PE4', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 1.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        75, 75, N'OC', N'Bajío Banco 1 (antes Primavera)', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 165.68, N'LT Bajío Etronque Tesistán - Niños Héroes
230 kV - 2C – 1.84 km-C
SE Bajío Banco 1
1 T - 3F - 60 MVA - 230/23 kV
SE Bajío MVAR
1 Capacitor - 3.6 MVAr - 23 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
En proceso de ejecución / construcción.
Contrato adjudicado de la obra de LAT Bajío entq. Tesistán – Niños Héroes, con fecha de inicio de construcción el 26 de febrero de 2024. Para la SE Bajío Bco. 1 lleva dos procedimientos de concurso declarados desiertos (agosto y septiembre 2024).
Reinicio construcción en marzo de 2025.
S.E. Bajio Bco. 1, se firma el contrato 06 de marzo de 2025, inicio de trabajos 10 de marzo de 2025, terminó de trabajos 05 octubre 2025.', N'Portafolio Activo', N'D18-OC5', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 3.68, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        76, 76, N'NT', N'Cuatro Siglos Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 33.51, N'LT Cuatro Siglos Entronque Fuentes - Tecnológico
115 kV - 2C – 2.0 km-C
SE Cuatro Siglos Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Cuatro Siglos MVAR
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 11.DIC.2024.', N'Portafolio Activo', N'D18-NT11', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 2.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        77, 77, N'SE', N'Berriozábal Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 36.35, N'LT Berriozábal Entronque Ocozocoautla - El Sabino
115 kV - 2C – 0.4 km-C
SE Berriozábal Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV
SE Berriozábal MVAr
1 Capacitor - 1.2 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 29.JUL.2025.', N'Portafolio Activo', N'D18-OR6', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        78, 78, N'NO', N'Compuertas Banco 1', N'Presupuestal Compartido', 2018, N'Actividades y Estudios Previos', 42.37, N'LT Compuertas Entronque Centenario - Mochis III
115 kV - 2C – 2.00 km-C
SE Compuertas Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Compuertas MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'En espera de transferencia de recursos de la Gerencia Regional de Transmisión.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
Distribución enviará actualización de ICM para validación de GPIC diciembre 2025.
Cambio de Monto por Alcances (Línea y Subestación).
63 MDP ==> Incremento.
Cambio del Plazo 190 días de ejecución.', N'Portafolio Activo', N'D18-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 1.04, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        79, 79, N'CE', N'Pedregal Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 111.79, N'LT Pedregal - Antea
115 kV - 2C – 9.0 km-C
SE Pedregal  Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV
SE Pedregal  MVAr
1 Capacitor - 1.2 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de la SE Pedregal Bco. 1, con fecha de inicio el 14 de febrero de 2024.
Para la LAT se publicó el 1er. Procedimiento de contratación el 25 de abril de 2024, el cual se declaró desierto el 28 de mayo de 2024.
LAT Pedregal, esta en concurso,se tiene estimado iniciar los trabajos en marzo de 2025.', N'Portafolio Activo', N'D18-OC8', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 9.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        80, 80, N'BC', N'La Encantada Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 24.18, N'LT Encantada Entronque Metrópoli - Tijuana
115 kV - 2C – 0.3 km-C
SE La Encantada Banco 1
1 T - 3F - 30 MVA - 115-69/13.8 kV
SE La Encantada MVAr
1 Capacitor - 13.8 kV - 1.8 MVAr', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
En proceso de ejecución / construcción.
Para la LAT se publicó el 1er. Procedimiento de contratación el 20 de diciembre de 2023, el cual se declaró desierto el 22 de enero de 2024, se publicó segunda convocatoria el 08 de marzo de 2024 e inició el contrato el 22 de abril de 2024.
S.E. Encantada Bco. 1 se publicó el 1er procedimiento el 22 de octubre. 2024, sin formalizar contrato. La 2 convocatoria se realizó el 17 de enero de 2025, el cual se declaró desierto.
S.E. Encantada Bco. 1, Tercera Convocatoria, se tiene programado el inico de los trabajso en abril de 2025.', N'Portafolio Activo', N'D18-BC4', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        81, 81, N'NT', N'Campo Setenta y Tres Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 114.20, N'LT Menonita - Campo Setenta y Tres
115 kV - 2C – 36.0 km-C
SE Campo Sesenta y Tres Banco 1
1 T - 3F - 30 MVA - 115/34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Obra Subestación concluida (2025).
Línea Campo Setenta y Tres Banco 1
Importe estimado: 117 MDP.
Plazo de ejecución: 360 días naturales.
Solicitud de Validación a GPIC: diciembre 2025.', N'Portafolio Activo', N'D18-NT1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 36.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        82, 82, N'NT', N'Viñedos Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 33.01, N'LT Viñedos Entronque Allende - Matamoros
115 kV - 2C - 1.0 km-C
SE Viñedos Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Viñedos MVAR
1 Capacitor  13.8 kV 1.8 MVAr', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 21.DIC.2024.', N'Portafolio Activo', N'D18-NT9', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 1.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        83, 83, N'OC', N'San Cristóbal Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 36.42, N'LT San Cristóbal Entronque Jesús del Monte - Reyma
115 kV - 2C – 2.6 km-C
SE San Cristóbal Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV
SE San Cristóbal MVAr
1 Capacitor - 1.2 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de la SE San Cristóbal y LAT, con fecha de inicio de construcción el 14 de febrero de 2024.
Se requiere el tendido de 21 km de fibra óptica pero las estructuras de CFE Transmisión se encuentran dañadas y no es posible instalar la FO, por lo que se realizara un contrato para el tendido de la FO y corrección de estructuras (12 pzas).', N'Portafolio Activo', N'D18-OC7', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 2.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        84, 84, N'NO', N'El Llano Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 23.96, N'LT El Llano Entronque Santa Ana - Oasis
115 kV - 2C – 0.3 km-C
SE El Llano Banco 1
1 T - 3F - 20 MVA - 115/34.5 kV
SE El Llano MVAr
1 Capacitor - 1.2 MVAr - 34.5 kV', NULL, NULL, NULL, NULL, N'Ninguna.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
LAT bajo la responsabilidad de CFE Transmisión. Para la SE se publicó el 1er. Procedimiento de contratación el 08 de marzo de 2024, el cual se declaró desierto el 29 de mayo de 2024. Se publicó segunda convocatoria el 05 de septiembre de 2024, el 15 de octubre de 2024 se declaro desierto 2do procedimiento de contratación.
La tercera convocatoriase publicó el 29 de noviembre del 2024, se adjudicó el 17 de enero del 2025.', N'Portafolio Activo', N'D18-NO1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        85, 85, N'SE', N'Traconis Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 126.95, N'LT Traconis Entronque Kilómetro Veinte – Macuspana Dos
115 kV - 2C – 32.0 km-C
SE Traconis Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Traconis MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
El 26 de septiembre, Distribución envió la ICM a la GPIC para su validación. Por recomendación de la SCyS se considera en el alcance del proyecto la construcción de obra civil y electromecánica con el suministro de postes troncocónicos por parte del concursante adjudicado.
Caso de Negocio REEVALUADO y en proceso de autorización por parte de la Comisión de Inversiones.', N'Portafolio Activo', N'D18-OR10', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 32.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        86, 86, N'PE', N'Oxtankah Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 111.22, N'LT Oxtankah - Chetumal Norte
115 kV - 2C – 3.95 km-C
SE Oxtankah Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV
SE Oxtankah MVAr
1 Capacitor - 1.2 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Proyecto bajo la responsabilidad de la Residencia Regional de Construcción de Proyectos de Transmisión y Transformación Peninsular. 1er. Procedimiento de contratación se publicó el 23 de enero de 2024, el cual se declaro desierto el 22 de enero de 2024. Se publicó segunda convocatoria en marzo 2024 y se declaro desierto el 10 de abril de 2024. Se realizará tercera convocatoria en septiembre de 2024.', N'No aplica.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Construcción y Transmisión.
Se han realizados 03 procedimientos de concurso y los 03 se han declarado DESIERTOS.
La obra de la Subestación ya cuenta con terracerias y barda perimetral
GPIC validó la ICM el 11 abril 2025.
CPTT está integrando expediente para enviar solicitu de contratación a la GCO.
GCO publicará el procedimiento de concurso: por definir.', N'Portafolio Activo', N'D18-PE3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 3.95, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        87, 87, N'SE', N'Luis Gil Pérez Banco 1', N'Presupuestal Compartido', 2018, N'En Operación', 35.23, N'LT Luis Gil Pérez Entronque Cactus Switcheo - Tamulte
115 kV - 2C – 2.0 km-C
SE Luis Gil Pérez Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Luis Gil Pérez MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 09.SEP.2025.', N'Portafolio Activo', N'D18-OR12', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 2.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        88, 88, N'BC', N'Buena Vista Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 24.07, N'LT Buena Vista Entronque El Triunfo - Santiago
115 kV - 2C – 0.2 km-C
SE Buena Vista Banco 1
1 T - 3F - 20 MVA - 115/34.5 kV
SE Buena Vista MVAr
1 Capacitor - 1.2 MVAr - 34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Obra terminada constructivamente. En proceso pruebas de puesta a punto y puesta en servicio.', N'Portafolio Activo', N'D18-BS1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        89, 89, N'BC', N'Victoria Potencia Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 149.38, N'LT Victoria Potencia - Chapultepec
115 kV - 1C – 11.0 km-C
SE Victoria Potencia Banco 1
1 T - 3F - 40 MVA - 230/13.8 kV
SE Victoria Potencia MVAr
1 Capacitor - 2.4 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Subestación Victoria Potencia Banco 1
Obra en construcción (2025)
Se requiere obras adicionales que se llevarán de manera local y otro proyecto: 
Línea Victoria Potencia Banco 1
Solicitud de Validación a GPIC: octubre 2025
Monto estimado 40 MDP.
Plazo de ejecución: 180 días naturales.', N'Portafolio Activo', N'D18-BC3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 11.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        90, 90, N'OC', N'Valle de Aguascalientes Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 84.51, N'LT Valle de Aguascalientes Entronque Cañada - Margaritas
115 kV - 2C – 12.0 km-C
SE Valle de Aguascalientes  Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Valle de Aguascalientes MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Subestación Valle de Aguascalientes Banco 1
Obra en construcción 
Línea Valle de Aguascalientes Banco 1
Solicitud de Validación ICM a GPIC: 21 marzo 2025
Se atendió Nota de Revisión 1: 15 mayo 2025
Validación de GPIC: 19 de mayo 2025
Se entregó el 22 de julio a la GCO la solicitud de contratación; el 1 de agosto se atendieron las observaciones
realizadas a la solicitud por parte de la GCO. 
Se realizó el fallo del procedimiento el 24 de sepiembre (Desierto)
Se publicó nuevamente el 29 de septiembre.', N'Portafolio Activo', N'D18-OC9', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 6.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        91, 91, N'OR', N'Laguna de Miralta Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 66.95, N'LT Laguna de Miralta Entronque Tampico - Chairel
115 kV - 2C – 1.2 km-C
SE Laguna de Miralta Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Laguna de Miralta MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Obra terminada constructivamente. En proceso pruebas de puesta a punto y puesta en servicio.', N'Portafolio Activo', N'D18-NE3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 1.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        92, 92, N'NT', N'El Capulín Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 82.17, N'LT El Capulín Entronque Casas Grandes - Janos
115 kV - 2C – 18.6 km-C
SE El Capulín Banco 1
1 T - 3F - 30 MVA - 115/34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Subestación El Capulín Banco 1 en Construcción
Línea El Capulín Banco 1 en Construcción 
Inversión estimada: 72 MDP.
Plazo de ejecución: 180 días naturales.
Solicitud de Validación ICM a GPIC: diciembre 2025
Se reprograma fecha de entrega de ICM a la GPIC, debido a que de la documentación entregada por dos propiestarios para la adquisicvión de derechos inmobiliarios, presentaron gravamen en sus propiedades.', N'Portafolio Activo', N'D18-NT3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 18.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        93, 93, N'NT', N'Lebarón Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 110.34, N'LT Galeana - Lebarón
115 kV - 2C – 33.5 km-C
SE Lebarón Banco 1
1 T - 3F - 30 MVA -115/34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Subestación Lebarón Banco 1 Obra concluida
Línea Lebarón Banco 1
Solicitud de Validación ICM a GPIC: 29 abril 2025.
Envío de Nota Revisión GPIC: 16 mayo 2025.
Atención de nota 1 de GPIC: 30 de mayo 2025.
GPIC emitió  nota 2 el 13 de junio.
Se envió la atención de la nota el 27 de junio.
Se realizó reunión entre GPIC, GPyC y DNTE el 4 de julio para revisar observaciones.​
Distribución envió ICM con la atención de las observaciones el 09 de julio.​
GPIC validó la ICM el 28 de julio; en gestión de autorización de Plurianualidad, sin embargo no se podrá concursar debvido a que 4 propietarios se retractaron de la anuencia de paso para la construcción de la LAT.', N'Portafolio Activo', N'D18-NT7', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 33.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        94, 94, N'NT', N'Buenavista Banco 1', N'Presupuestal Compartido', 2018, N'Ejecución/Construcción', 84.51, N'LT Ascensión - Buenavista
115 kV - 2C – 25.0 km-C
SE Buenavista Banco 1
1 T - 3F - 20 MVA - 115/34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Subestación Buenavista Banco 1
Obra en construcción.

Se requiere Validación y Procedimiento de Contratación de:
Línea Buenavista Banco 1
Inversión estimada: 66 MDP.
Plazo de ejecución:  270 días naturales.
Solicitud de Validación ICM a GPIC: diciembre 2025.', N'Portafolio Activo', N'D18-NT6', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 25.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        95, 95, N'SE
CE', N'Suministro de energía en la Zona Huatulco y Costa Chica', N'Presupuestal', 2019, N'En Operación', 573.75, N'Pochutla MVAr
STATCOM - +50/-50 MVAr - 115 kV
1 Alimentador 115 kV STATCOM
Agua Zarca MVAr
STATCOM - +30/-30 MVAr - 115 kV
1 Alimentador 115 kV STATCOM', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el STATCOM de SE Pochutla el 08.JUL.2024.
Se energizó el STATCOM de SE Agua Zarca el 13.JUL.2024.', N'Portafolio Activo', N'P19-OR3', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 160.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        96, 96, N'NE', N'Reducción en el nivel de cortocircuito de la red eléctrica de la Zona Metropolitana de Monterrey', N'Presupuestal', 2019, N'Ejecución/Construcción', 2159.13, N'Domingo Nuevo Entronque Monterrey Potencia – Hylsa Maniobras
400 kV - 2C - 0.4 km-C
Domingo Nuevo Entronque Felix U. Gómez - Nogalar
115 kV - 2C - 5.2 km-C
Domingo Nuevo Entronque Fundidora - Nogalar
115 kV - 2C - 5.2 km-C
San Jerónimo Potencia – Valle
115 kV - 1C - 2 km-C
Domingo Nuevo Banco 5
4 T - 1F - 125 MVA - 500 MVA - 400/115 kV', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P19-NE2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 500.00, 0.00, 12.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        97, 97, N'BC', N'Tijuana I Banco 4', N'Presupuestal', 2019, N'En Operación', 241.00, N'Tijuana I Banco 4
4 AT - 1F - 75 MVA - 300 MVA - 230/115-69 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 20.DIC.2024.', N'Portafolio Activo', N'P19-BC1', N'Concluido y en operación', NULL, NULL, NULL, 300.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        98, 98, N'NO', N'Culiacán Poniente Entronque Choacahui- La Higuera (A3N40)', N'Presupuestal', 2019, N'En Operación', 82.34, N'Culiacán Poniente Entronque Choacahui - La Higuera (A3N40)
400 kV - 2C - 0.4 km-C - 2C/F - 1113 ACSR – TA – 2A', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 29.JUL.2024.', N'Portafolio Activo', N'P15-NO1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        99, 99, N'CE
OC', N'Compensación de Potencia Reactiva Dinámica en el Bajío', N'Presupuestal', 2019, N'Ejecución/Construcción', 982.56, N'SE Potrerillos CEV
1 CEV - +300/-90 MVAr - 230 kV
1 Alimentador 230 kV CEV
SE Potrerillos MVAr
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Querétaro Potencia CEV
1 CEV - +300/-90 MVAr - 230 kV
1 Alimentador 230 kV CEV
SE Santa Fe MVAr
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE El Marqués MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P19-OC4', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 900.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        100, 100, N'NE', N'San Jerónimo Potencia Banco 2', N'Presupuestal', 2019, N'En Operación', 280.93, N'San Jerónimo Potencia Banco 2
3 T - 1F - 125 MVA - 375 MVA - 400/115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 11.AGO.2024.', N'Portafolio Activo', N'P18-NE3', N'Concluido y en operación', NULL, NULL, NULL, 375.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        101, 101, N'NT', N'Terranova Banco 2', N'Presupuestal', 2019, N'En Operación', 149.32, N'Terranova Banco 2
3 AT - 1F - 100 MVA - 300 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 02.DIC.2024.', N'Portafolio Activo', N'P19-NT1', N'Concluido y en operación', NULL, NULL, NULL, 300.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        102, 102, N'NE', N'Derramadero entronque Ramos Arizpe Potencia - Salero', N'Presupuestal', 2019, N'Ejecución/Construcción', 572.44, N'Derramadero entronque Ramos Arizpe - Salero (A3G10)
400 kV - 2C - 6.4 km-C
Derramadero MVAr (reactor de línea) (traslado)
1 R Barra - 3F - 75 MVAr - 400 kV', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P18-NE2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 75.00, 6.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        103, 103, N'NE', N'Ampliación de la red eléctrica de 115 kV del corredor Tecnológico-Lajas', N'Presupuestal', 2019, N'En Operación', 707.76, N'Regiomontano – Ladrillera
115 kV - 2C - 69.6 km-C (Tendido primer Circuito) /1
Ladrillera
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
Lajas
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
Linares
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'LT Regiomontano (RGM) 73U00 Ladrillera (LDA)
Se energizó el 16.OCT.2025

SE Lajas, 1 Bco. de Capacitores - 115 kV - 15 MVAR
Se energizó el 04.MAY.2024.

SE Linares, 1 Bco. de Capacitores - 115 kV - 15 MVAR
Se energizó el 18.MAY.2025.

SE Ladrillera, 1 Bco. de Capacitores - 115 kV – 15 MVAR
Se energizó el 02.NOV.2025.', N'Portafolio Activo', N'P19-NE1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 45.00, 69.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        104, 104, N'NO', N'El Mayo entronque Navojoa Industrial - El Carrizo', N'Presupuestal', 2019, N'En Operación', 69.68, N'El Mayo entronque Navojoa Industrial - Carrizo
115 kV - 2C - 0.6 km-C - 1C/F - 795 ACSR – TA – 2A', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 18.JUL.2025.', N'Portafolio Activo', N'P16-NO1', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        105, 105, N'NO', N'Viñedos MVAr', N'Fibra E', 2019, N'Por concursar', 17.95, N'Viñedos MVAr
1 Capacitor - 22.5 MVAr - 115 Kv
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
El proyecto está en revaluación motivado a que la ICM superó el importe del Caso de Negocio.
Se cuenta con FIP y en proceso la revaluación del Caso de Negocio, Plan Financiero y Plan de Ejecución.
Se realizó una propuesta de fechas para su concurso:
28.NOV.25 Solicitud de Publicación
03.DIC.25 Publicación
29.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P19-NO1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        106, 106, N'CE
OR
SE
OC
NO
BC', N'Modernización de enlaces de transmisión requeridos para incrementar capacidad de líneas de transmisión limitadas por equipo serie', N'Pendiente de definir', 2019, N'Instruido y SIN priorización', 208.50, N'Líneas de Transmisión Gerencia de Control Regional Central
LT Jilotepec 73680 San Sebastián 
Cond 477 MCM = 132 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual)
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en la SE Jilotepec
LT Zapata Envases 73K60 Cuautitlán
Cond 795 MCM = 266 MVA / Capacidad declarada = 133 MVA
Carga detectada (% del límite actual) = 134
Equipo que limita= Transformador de Corriente y Remate de LT a Barra de SE
Descripción del proyecto:
Reemplazo TC en ambas Subestaciones Eléctricas
Repotenciación de RE en la SE Cuautitlán
LT Zapata Envases 73K80 Trimex 
Cond 795 MCM = 266 MVA / Capacidad declarada = 177 MVA
Carga detectada (% del límite actual) = 111
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en ambas Subestaciones Eléctricas
LT Victoria Envases 73K20 Trimex 
Cond 795 MCM = 266 MVA / Capacidad declarada = 177 MVA
Carga detectada (% del límite actual) = 143
Equipo que limita= Transformador de Corriente y Cuchilla desconectadora
Descripción del proyecto:
Reemplazo TC y CU en ambas Subestaciones Eléctricas
LT Becton Dickinson 73K50 Cuautitlán 
Cond 795 MCM = 266 MVA / Capacidad declarada = 133 MVA
Carga detectada (% del límite actual) = 132
Equipo que limita= Transformador de Corriente y RE
Descripción del proyecto:
Reemplazo TC en ambas Subestaciones Eléctricas
Repotenciación de RE en SE Cuautitlán
LT Victoria 73K10 Ford Motor Company
Cond 795 MCM = 266 MVA / Capacidad declarada = 177 MVA
Carga detectada (% del límite actual) = 145
Equipo que limita= Cuchilla desconectadora
Descripción del proyecto:
Reemplazo CU en ambas Subestaciones Eléctricas
LT Ford Motor Company 73K30 Alpura
Cond 795 MCM = 266 MVA / Capacidad declarada = 177 MVA
Carga detectada (% del límite actual) = 112
Equipo que limita= Transformador de Corriente y Cuchilla desconectadora
Descripción del proyecto:
Reemplazo TC y CU en ambas Subestaciones Eléctricas
LT Cementera Cruz Azul 93390 Tula 
Cond 2x1113 MCM = 837 MVA / Capacidad declarada = 319 MVA
Carga detectada (% del límite actual) = 111
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en SE Cementera Cruz Azul
LT Cementera Cruz Azul 93450 Planta de Aguas Residuales Atotonilco
Cond 2x1113 MCM = 837 MVA / Capacidad declarada = 319 MVA
Carga detectada (% del límite actual) = 103.7
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en SE Cementera Cruz Azul
LT La Paz 93D70 Ayotla
Cond 1113 MCM = 418 MVA / Capacidad declarada = 319 MVA
Carga detectada (% del límite actual) = 111
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en SE Ayotla
LT Atenco 93490 San Bernabé
Cond 1113 MCM = 418 MVA / Capacidad declarada = 319 MVA
Carga detectada (% del límite actual) = 105
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en SE San Bernabé
LT Atlacomulco Potencia 73620 Ixtlahuaca II
Cond 795 MCM = 180 MVA / Capacidad declarada = 120 MVA
Carga detectada (% del límite actual) = 103
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en ambas Subestaciones Eléctricas
LT Atlacomulco Potencia 73630 Atlacomulco
Cond 795 MCM = 180 MVA / Capacidad declarada = 120 MVA
Carga detectada (% del límite actual) = 101
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo TC en ambas Subestaciones Eléctricas

Líneas de Transmisión Gerencia de Control Regional Oriental
LT Comalcalco 73050 Jalpa
Cond 477 MCM = 131 MVA / Capacidad declarada = 60 MVA
Carga detectada (% del límite actual) = 128
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Modificación de RTC en ambas Subestaciones Eléctricas
LT Comalcalco 73880 Comalcalco Oriente
Cond 477 MCM = 131 MVA / Capacidad declarada = 80 MVA
Carga detectada (% del límite actual) = 116
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Cárdenas 73620 Cárdenas II
Cond 477 MCM = 131 MVA / Capacidad declarada = 105 MVA
Carga detectada (% del límite actual) = 109
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en SE Cárdenas
En Proceso Repotenciación Barra SE Cárdenas
LT Tamulté 73480 Tamulté Maniobras
Cond 795 MCM = 179 MVA / Capacidad declarada = 120 MVA
Carga detectada (% del límite actual) = 127
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en SE Tamulté
LT Villahermoa Poniente 73160 La Choca
Cond 477 MCM = 131 MVA / Capacidad declarada = 60 MVA
Carga detectada (% del límite actual) = 104
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Teziutlán 73080 Jalacingo
Cond 795 MCM = 180 MVA / Capacidad declarada = 80 MVA
Carga detectada (% del límite actual) = 131
Equipo que limita= Bus
Descripción del proyecto:
Recalibración de Barra en SE Teziutlán
LT Pochutla 73480 Huatulco
Cond 477 MCM = 131 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual) = 180
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Huatulco 73750 Conejos
Cond 795 MCM = 180 MVA / Capacidad declarada = 60 MVA
Carga detectada (% del límite actual) = 154
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Conejos 73470 Juchitán II
Cond 795 MCM = 180 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual) = 256
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Ejutla 73180 San Jacinto Tlacotepec
Cond 795 MCM = 180 MVA / Capacidad declarada = 60 MVA
Carga detectada (% del límite actual) = 111
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en ambas Subestaciones Eléctricas
LT Acatlán 73520 Huajuapan
Cond 477 MCM = 131 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual) = 130
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de TC en SE Acatlán
LT Oaxaca 73340 La Ciénega
Cond 477 MCM = 131 MVA / Capacidad declarada = 20 MVA
Carga detectada (% del límite actual) = 300
Equipo que limita= Bus
Descripción del proyecto:
Recalibración de Barra SE Oaxaca
LT Oaxaca Poniente 73320 Oaxaca
Cond 477 MCM = 131 MVA / Capacidad declarada = 20 MVA
Carga detectada (% del límite actual) = 133
Equipo que limita= Bus y Transformador de Corriente
Descripción del proyecto:
Recalibración de Barra SE Oaxaca
Reemplazo de Transformadores de Corriente en ambas Subestaciones Eléctricas
LT Huixtla 73V00 Belisario Domínguez
Cond 336 MCM = 106 MVA / Capacidad declarada = 60 MVA
Carga detectada (% del límite actual) = 103
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de Transformadores de Corriente en ambas Subestaciones Eléctricas
LT San Cristóbal 73380 San Cristobal Oriente
Cond 477 MCM = 131 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual) = 117
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de Transformadores de Corriente en SE San Cristóbal
LT Malpaso 73930 Mezcalapa
Cond 477 MCM = 131 MVA / Capacidad declarada = 40 MVA
Carga detectada (% del límite actual) = 125
Equipo que limita= Transformador de Corriente
Descripción del proyecto:
Reemplazo de Transformadores de Corriente en ambas Subestaciones Eléctricas
LT La Choca 73720 Tamulté
Cond 477 MCM = 131 MVA / Capacidad declarada = 120 MVA
Carga detectada (% del límite actual) = 110
Equipo que limita=Bus y Transformador de Corriente
Descripción del proyecto:
Reemplazo de Transformadores de Corriente en ambas Subestaciones Eléctricas
Recalibración de Barra en ambas Subestaciones Eléctricas
LT Zapata 63060 Temixco
Cond 795 MCM = 133 MVA / Capacidad declarada = 44 MVA
Carga detectada (% del límite actual) = 132
Equipo que limita=Transformador de Corriente
Descripción del proyecto:
Reemplazo de Transformadores de Corriente en ambas Subestaciones Elé', NULL, N'Varias', NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M19-TC1
M19-SEN5', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        107, 107, N'NT', N'Reemplazo de equipo con baja capacidad de corto circuito (KA) (en zonas Juárez y Torreón)', N'Fibra E', 2019, N'En Proceso de Decisión', 316.43, N'SE Valle de Juárez
5 Interruptores de Potencia 115 kV / 2
31 Cuchillas desconectoras 115 kV / 3
14 Transformadores de Corriente 115 kV / 4
1 Alimentador 115 kV
SE Francke
1 Interruptor de Potencia 115 kV / 2
3 Cuchillas desconectoras​ 115 kV / 3
3 Apartarayos 115 kV
3 Transformadores de Potencial  115 kV​
Requiere reforzar estructura mayor para colocación de cuchillas​
SE Torreón Sur
3 Interruptores de Potencia 115 kV / 2
13 Cuchillas desconectoras 115 kV / 3
17 Transformadores de Corriente  115 kV ​/ 4
SE Laguna
3 Interruptores de Potencia 115 kV / 2
15 Cuchillas desconectoras kV / 3
9 Transformadores de Corriente  115 kV​ / 4
SE Sacramento
2 Interruptores de Potencia 115 kV / 2
6 Transformadores de Corriente 115 kV​ / 4
SE Laguna II
1 Interruptores de Potencia 115 kV / 2
10 Cuchillas desconectoras 115 kV / 3
3 Apartarayos 115 kV​
12 Transformadores de Corriente 115 kV​ / 4

2/ El proyecto contempla la modernización de interruptores​
3/ El proyecto contempla la modernización de cuchillas desconectadoras​
4/ El proyecto contempla la modernización de TC', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M19-NT2', N'En Concurso y Por Concursar', N'2026.1', N'3.2', N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        108, 108, N'OR', N'Sustitución de Transformadores de Potencia en la subestación Poza Rica 1', N'Pendiente de definir', 2019, N'Instruido y SIN priorización', 34.00, N'SE Poza Rica
Sustitución de 2  bancos de transformación
1 AT - 3F - 53.3 MVA - 115/69 kV - AT-6
1 AT - 3F - 53.3 MVA - 115/69 kV - AT-7', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M19-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 106.60, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        109, 109, N'CE', N'Puebla Dos Mil entronque Puebla II 73890 Guadalupe Analco', N'Pendiente de definir', 2019, N'Instruido y SIN priorización', 27.00, N'Puebla Dos Mil Entronque Puebla II – Guadalupe Analco (73890)
115 kV - 2C - 0.2 km-C', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P19-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        110, 110, N'CE', N'San José Iturbide Banco 4', N'Fibra E', 2019, N'En Proceso de Decisión', 2701.07, N'LT Las Delicias – San José Iturbide
230 kV - 2C - 60.0 km-C - TA - 1113 ACSR - 1 Conf/f /2
LT San José Iturbide – La Fragua
115 kV - 1C - 16.0 km-C - TA - 1113 ACSR - 1 Conf/f
SE Las Delicias 
2 Alimentadores 230 kV LT Las Delcias - San José Iturbide
San José Iturbide Banco 4
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
San José Iturbide
2 Alimentadores 230 kV LT San José Iturbide - Las Delicias
1 Alimentador 115 kV LT San José Iturbide - La Fragua
1 Alimentador 115 kV (Amarre)
1 Alimentador 230 kV (Amarre)
Recalibración Bus 115 kV /3
1 Bus Nuevo 230 kV
SE La Fragua
1 Alimentador 115 kV LT La Fragua - San José Iturbide
SE San Luis de la Paz II
Modernización 5 Interruptores 115 kV /4
SE La Soledad
Recalibración del Bus y Puentes 115 kV /5

2/ Tendido de los dos circuitos
3/ Capacidad mínima 450 MVA
4/ Sustitución por capacidad de Icc
5/ Incrementar límite LT La Soledad 73450 San Luis de la Paz mínimo 119 MVA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P19-OC2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 0.00, 83.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        111, 111, N'BC', N'Modernización de Arreglo de Barras en 230 kV de la subestación eléctrica Tecnológico', N'Pendiente de definir', 2019, N'Instruido y SIN priorización', 16.00, N'SE Tecnológico
Cambio de arreglo a BP - BA 230 kV
9 Cuchillas desconectadoras 230 kV
6 Transformadores de Corriente 230 kV
6 Transformadores de Potencial 230 kV
Tableros de Protección, Control y Medición
Obra Civil
Obra Electromecánica', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M19-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        112, 112, N'NT', N'Modernización de tres cuadros de Maniobras para incorporar interruptores', N'Fibra E', 2019, N'En Proceso de Decisión', 112.24, N'SE Cuadro de Maniobras Bismark
1 Alimentador 115 kV LT Cuadro de Maniobras Bismark 73830 Palomas /1,4,5
SE Buenaventura
1 Alimentador 115 kV LT Buenaventura 73660 Benito Juárez /2,4,5
1 Alimentador 115 kV LT San Buenaventura - 73660 - Casas Grandes /3,4,5

1/ Incluye Equipo Asociado 
1 Pieza Interruptor de Potencia 115 kV
2 Juegos Cuchillas desconectadoras 115 kV
3 Piezas Transformadores de Corriente 115 kV
3 Piezas Transformadores de Potencial 115 kV
3 Piezas Apartarrayos 115 kV
2/ Incluye Equipo Asociado 
1 Pieza Interruptor de Potencia 115 kV
2 Juegos Cuchillas desconectadoras 115 kV
3 Piezas Transformadores de Corriente 115 kV
3 Piezas Transformadores de Potencial 115 kV
3 Piezas Apartarrayos 115 kV
3/ Incluye Equipo Asociado 
1 Pieza Interruptor de Potencia 115 kV
2 Juegos Cuchillas desconectadoras 115 kV
3 Piezas Transformadores de Corriente 115 kV
3 Piezas Transformadores de Potencial 115 kV
3 Piezas Apartarrayos 115 kV
4/ Considerar estructura mayor
5/ Caseta Integras Bahías de Línea', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M19-NT1', N'En Concurso y Por Concursar', N'2026.1', N'3.2', N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        113, 113, N'OR', N'Modernización de red eléctrica asociada a Humeros', N'Pendiente de definir', 2019, N'Instruido y SIN priorización', 59.00, N'Humeros Dos 73170 Teziutlán
115 kV - 1C - 16 km-C /1
SE Teziutlán
1 Recalibración Barra 115 kV
3 Transformadores de Corriente 115 kV
SE Libres
1 Recalibración Barra 115 kV
3 Transformadores de Corriente 115 kV
SE Humeros II
3 Transformadores de Corriente 115 kV
SE Humeros III
Sustitución 2 Interruptores de Potencia 115 kV

1/ Recalibración', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M19-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 16.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        114, 114, N'NO', N'Solución a las restricciones de capacidad de transmisión de cables subterráneos del Noroeste', N'Fibra E', 2019, N'En Proceso de Decisión', 2317.56, N'Zona Hermosillo: 48.9 km-C
LT Dynatech – Rolando García Urrea
115 kV - 1C - 7.0 km-C
LT Hermosillo Cinco – Dynatech
115 kV - 1C - 1.7 km-C
LT Hermosillo Cuatro – Hermosillo Seis
115 kV - 1C - 2.3 km-C
LT Hermosillo Cuatro – Portales
115 kV - 1C - 0.4 km-C
LT Hermosillo Loma – Hermosillo Dos
115 kV - 1C - 5.7 km-C
LT Hermosillo Loma – Hermosillo Misión
115 kV - 1C - 4.7 km-C
LT Hermosillo Loma – Pueblitos
115 kV - 1C - 0.1 km-C
LT Hermosillo Nueve – Hermosillo Cuatro
115 kV - 1C - 7.0 km-C
LT Hermosillo Seis – Hermosillo Misión
115 kV - 1C - 3.6 km-C
LT Hermosillo Uno – Hermosillo Nueve
115 kV - 1C -  0.1 km-C
LT Hermosillo Uno – Rolando García Urrea
115 kV - 1C - 9.8 km-C
LT Portales – Hermosillo Dos
115 kV - 1C - 6.5 km-C

Zona Obregón: 21.9 km-C
Bácum – Ciudad Obregón Dos
115 kV - 1C - 4.2 km-C
Bácum – Providencia
115 kV - 1C - 0.3 km-C
Ciudad Obregón Cuatro – Obregón Uno
115 kV - 1C - 11.0 km-C
Ciudad Obregón Dos – Providencia
115 kV - 1C - 4.1 km-C
Ciudad Obregón Tres – Tetabiate
115 kV - 1C - 2.3 km-C

Zona Los Mochis: 7.3 km-C
Louisiana – Centenario
115 kV - 1C - 3.6 km-C
Louisiana – Mochis Centro
115 kV - 1C - 3.7 km-C

Zona Culiacán: 13.1 km-C
Culiacán Cuatro – Costa Rica
115 kV 1C - 1.3 km-C
Culiacán Tres – Culiacán Centro
115 kV - 1C - 2.2 km-C
Culiacán Tres – Isla Musala
115 kV - 1C - 1.3 km-C
Isla Musala – Culiacán Oriente
115 kV - 1C - 3.0 km-C
Jaime Sevilla Poyastro – Culiacán Uno
115 kV - 1C - 5.3 kmC

Zona Mazatlán: 27.1 km-C
Mazatlán Aeropuerto – Subestación Villa Unión
115 kV - 1C - 1.2 km-C
Mazatlán Dos - Mazatlán Aeropuerto
115 kV 1C - 10.5 km-C
Mazatlán Dos – Subestación Villa Unión
115 kV - 1C - 9.3 km-C
Mazatlán Norte – Del Mar
115 kV - 1C - 3.6 km-C
Mazatlán Uno - Mazatlán Centro
115 kV - 1C - 2.5 km-C', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P19-NO2', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 0.00, 0.00, 130.02, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        115, 115, N'CE', N'353 LT Incremento de Capacidad de Transmisión de las Delicias Querétaro', N'PIDIREGAS 2021', 2019, N'Ejecución/Construcción', 1926.81, N'Las Delicias Entronque Querétaro - 93670 - Querétaro Potencia
230 kV - 2C - 189.2 km-C 
SE Las Delicias
2 Alimentadores 230 kV
SE Querétaro
3 Juegos Transformadores de Corriente 230 kV (sustitución)
SE Conín
2 Juegos Transformadores de Corriente 230 kV (sustitución)
SE Querétaro Potencia
1 Juego Interruptor de Potencia 230 kV (sustitución)
2 Juegos Cuchillas desconectadoras 230 kV (sustitución)', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P19-OC3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 189.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        116, 116, N'CE
OR', N'358 SLT Incremento en capacidad de transm Noreste Centro del País, I19-CE1
ETAPA 1', N'PIDIREGAS 2022', 2019, N'En concurso', 2477.43, N'I19-CE1, Etapa 1

Jilotepec Potencia Banco 1 y 2
7 AT - 1F - 125 MVA - 875 MVA - 400/230 kV
Jilotepec Potencia MVAr (reactores de barra)
3 R Barra - 1F - 16.66 MVAr - 50 MVAr - 400 kV
1 Alimentador 400 kV Reactor
3 R Barra - 1F - 16.66 MVAr - 50 MVAr - 400 kV
1 Alimentador 400 kV Reactor
Jilotepec Potencia MVAr (reactores de línea)
1 R Línea - 1F - 25 MVAr - 75 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
1 R Línea - 1F - 25 MVAr - 75 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
(Incluye fase de reserva compartida)
Jilotepec Potencia Ampliación
7 Alimentadores 400 kV
SE Las Mesas Ampliación
2 Alimentadores 400 kV', NULL, NULL, NULL, NULL, N'El proyecto a sido elevado a Concurso en tres ocaciones desde el año 2023, el primer procedimiento de concurso se declaró desierto en la etapa de evaluación Técnica, en el segundo procedimiento se logró adjudicar, sin embargo, no se formalizó el contrato porque el consorcio no presentó la garantía de cumplimiento de contrato. Durante el tercer concurso se presentó un recurso de amparo por parte del concursante ganador con relación al segundo procedimiento, por lo que no fue posible continuar con el concurso y se procedió a la cancelación de dicho concurso.', N'Se han actualizado las condiciones comerciales de la ICM y conformado el expediente para solicitar un nuevo procedimiento de concurso en cuanto no existan impedimentos legales para continuar con el proyecto.', N'El proyecto está en concurso con el procedimiento No. CFE-0004-CACOT-0001-2025.
11.SEP.25 Publicación
21.NOV.25 Fallo.', N'Portafolio Activo', N'I19-CE1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 875.00, 275.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        117, 117, N'PE', N'356 SLT Aumento de capacidad de transm de zonas Cancún y Riviera Maya
(Fase 1 - Kantenáh)', N'PIDIREGAS 2022', 2019, N'Ejecución/Construcción', 3623.17, N'Kantenáh Entronque Dzitnup - Riviera Maya (A3Q70)
400 kV - 2C- 74.8 km-C
Kantenáh Entornque Playa del Carmen - Aventura Palace (73790)
115 kV - 2C - 8.8 km-C
Kantenáh Entronque Playa del Carmen - Aktunchen (73R60)
115 kV - 2C - 8.8 km-C
Aktunchen Entronque Aventura Palace - Punto Inflexión Aktunchen
115 kV - 1C - 0.11 km-C
Aktunchen - Aventura Palace
115 kV - 1C - 1.54 km-C (Cable subtarráneo)
Aktunchen - Akumal II
115 kV - 2C - 18.04 km-C
Recalibración de Punto de Inflexión Kantenáh - Playa del Carmen
115 kV - 2C - 32.01 km-C (recalibración del conductor) / 5
Calica Entronque Kantenaá - Playa del Carmen
115 kV - 2C - 0.11 km-C
Kantenáh Banco 1
4 T - 125 MVA - 500 MVA - 400/115 kV
Kantenáh STATCOM
1 STATCOM +200/- 200 MVAr - 115 kV
1 Alimentador 115 kV STATCOM
Kantenáh MVAr (reactor de línea) (traslado)
3 R Línea - 1F - 11.11 MVAr - 33.3 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
3 R Línea - 1F - 11.11 MVAr - 33.3 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
Balam MVAr (sustitución)
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
Cancún MVAr (sustitución)
1 Capacitor - 30 MVAr -  115 kV
1 Alimentador 115 kV Capacitor
Bonampak MVAr (traslado)
1 Capacitor - 15 MVAr -  115 kV
Alimentador 115 kV Capacitor
Riviera Maya MVAR
1 Capacitor - 30 MVAr -  115 kV
1 Alimentador 115 kV Capacitor
Zac Nicté MVAr
1 Capacitor - 30 MVAr -  115 kV
1 Alimentador 115 kV Capacitor
Kantenáh - Alimentador SF6 - 2 de 400 kV
Kantenáh - Alimentador SF6 - 4 de 115 kV
Aventura Palace - Alimentador ampliación - 2 de 115 kV
Aktun-Chen - Alimentador ampliación SF6 - 2 de 115 kV
Akumal II - Alimentador ampliación - 2 de 115 kV
Puerto Morelos - Cambio de TC - 1 de 115 kV', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P18-PE2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 500.00, 601.60, 144.21, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        118, 118, N'SE', N'Zapata Oriente Banco 1', N'Presupuestal Compartido', 2019, N'En Operación', 37.95, N'LT Zapata Oriente Entronque Los Ríos Potencia - El  Zopo
115 kV - 2C – 2.0 km-C
SE Zapata Oriente Banco 1
1 T - 3F - 20 MVA - 115/34.5 kV
SE Zapata Oriente MVAr
1 Capacitor - 1.2 MVAr - 34.5 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 13.OCT.2025.', N'Portafolio Activo', N'D19-OR3', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 2.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        119, 119, N'OC', N'Jauja Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 101.10, N'LT Jauja Entronque Tabacalera - Tepic Indutrial
115 kV - 2C – 9.0 km-C
SE Jauja Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Jauja MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Obra terminada constructivamente. En proceso pruebas de puesta a punto y puesta en servicio.', N'Portafolio Activo', N'D19-OC1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 9.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        120, 120, N'OC', N'Centro Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 34.68, N'LT Centro Entronque Vallarta I - Nogalito
115 kV - 2C – 0.95 km-C
SE Centro Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Centro MVAr
1 Capacitor -1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de la SE Centro, con fecha de inicio de construcción el 24 de julio de 2024. Para la LAT se publicó el 1er. Procedimiento de contratación el 08 de marzo de 2024, el cual se declaró desierto el 30 de abril de 2024, se publicó segunda convocatoria el 08 de mayo de 2024 e inició contrato el 14 de junio de 2024.', N'Portafolio Activo', N'D19-OC2', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 1.90, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        121, 121, N'OC', N'Acatic Banco 1', N'Presupuestal Compartido', 2019, N'En Operación', 31.34, N'LT Acatic Entronque Tepatitlán - Zapotlanejo Distribución
115 kV - 2C – 0.4 km-C
SE Acatic Banco 1
1 T - 3F - 20 MVA - 115/23 kV
SE Acatic MVAr
1 Capacitor - 1.2 MVAr - 23 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 18.OCT.2025.', N'Portafolio Activo', N'D19-OC4', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 0.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        122, 122, N'OC', N'Tolimán Banco 1', N'Presupuestal Compartido', 2019, N'En Operación', 134.75, N'LT Juan Rulfo - Tolimán
115 kV - 1C – 13.6 km-C
SE Tolimán Banco 1
1 T - 3F - 20 MVA - 115/23 kV
SE Tolimán MVAr
1 Capacitor - 1.2 MVAr - 23.8 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Proyecto COMPARTIDO entre CFET y CFED.
Se energizó el 16.JUL.2025.', N'Portafolio Activo', N'D19-OC5', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 0.00, 13.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        123, 123, N'OC', N'Tapalpa Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 96.95, N'LT Tapalpa Entronque Sayula - Centro Logístico
115 kV - 2C – 11.0 km-C
SE Tapalpa Banco 1
1 T - 3F - 20 MVA - 115/23 kV
SE Tapalpa MVAr
1 Capacitor - 1.2 MVAr - 23 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de SE Tapalpa Banco 1, con fecha de inicio de construcción el 26 de febrero de 2024 y LAT con ICM autorizada, se publicó el 30 de septiembre del 2024, se adjudicó el 30 de octubre del 2024.', N'Portafolio Activo', N'D19-OC6', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 22.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        124, 124, N'OC', N'Morelos Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 51.70, N'LT Morelos Entronque León IV - León Alfaro
115 kV - 2C – 6.0 km-C
SE Morelos Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Morelos MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de SE Morelos Banco 1(Etapa 1 Obra Civil), inicio de construcción el 11 de julio de 2024. 
LAT esta en concurso, y se estima iniciar trabajos en abril de 2025', N'Portafolio Activo', N'D19-OC12', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 6.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        125, 125, N'NO', N'Santa Fe Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 41.61, N'LT Santa Fe Entronque Culiacán Poniente - Culiacán I
115 kV - 2C – 2.093 km-C
SE Santa Fe Banco 1
1 T - 3F - 30 MVA - 115/13.8 kV
SE Santa Fe MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'En espera de transferencia de recursos de la Gerencia Regional de Transmisión', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
Subestación Santa Fe Banco 1
 La GCO publicó el 20 de agosto, en proceso de evaluación economica, el fallo se realizó el 29 de septiembre.(Desierto)
Se publicó la segunda convocatoria el 2 de octubre
Línea Santa Fe Banco 1 en proceso de firma de contrato', N'Portafolio Activo', N'D19-NO3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 3.90, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        126, 126, N'NO', N'Tamazula Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 63.76, N'LT Tamazula - San Rafael
115 kV - 1C – 12.51 km-C
SE Tamazula Banco 1
1 T - 3F - 20 MVA - 115/13.8 kV
SE Tamazula MVAr
1 Capacitor - 1.8 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de  Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de la LAT Tamazula, con fecha de inicio el 16 de julio de 2024. Para la SE se publicó la convocatoria el 02 de septiembre de 2024 e inició contrato de obra el 24 de octubre de 2024.', N'Portafolio Activo', N'D19-NO4', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 12.32, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        127, 127, N'NO', N'Terramara Banco 1', N'Presupuestal Compartido', 2019, N'Ejecución/Construcción', 33.87, N'LT Terramara Entronque Viñedos - Oasis
115 kV - 2C – 0.6 km-C
SE Terramara Banco 1
1 T - 3F - 20 MVA - 115/34.5 kV
SE Terramara MVAr
1 Capacitor - 1.2 MVAr - 34.5 kV', NULL, NULL, NULL, NULL, N'Derivado a que es un proyecto compartido entre CFED y CFET, se presentaron indefiniciones para la asignación de los recursos por temas de las fronteras de responsabilidad.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
En proceso de ejecución / construcción.
Adjudicado contrato de obra de LAT Terramara, con fecha de inicio de construcción el 31 de enero de 2024 y SE Terramara se publicó la convocatoria el 30 de agosto de 2024 el 15 de octubre se declaro desierto el procedimiento.
La segunda convocatoria de SE Terramara se realizó el 27 de noviembre  y se adjudicó el 16 de enero de 2025.', N'Portafolio Activo', N'D19-NO5', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 1.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        128, 128, N'OR
VM', N'San Bartolo Banco 1', N'Presupuestal Compartido', 2019, N'En concurso', 91.14, N'LT San Bartolo Entronque Cruz de Ataque - Ixhuatlán
115 kV - 2C – 13.0 km-C
SE San Bartolo Banco 1
1 T - 3F - 9.375 MVA - 115/23 kV', NULL, NULL, NULL, NULL, N'En espera de transferencia de recursos de la Gerencia Regional de Transmisión.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
Subestación San Bartolo Bco.1
Se publicó la teercer convocatoria el 24 de septiembre

La ICM del proyecto LT San Bartolo entq. Ixhuatlán de Madero-Cruz de Ataque será entregada a GPIC el 17 de octubre 2025.​
Plazo de ejecución: 250 días naturales.', N'Portafolio Activo', N'D19-NE2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 0.00, 12.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        129, 129, N'BC', N'Libramiento Banco 1', N'Presupuestal Compartido', 2019, N'Actividades y Estudios Previos', 103.79, N'LT Libramiento Entronque Parque Industrial San Luis - San Luis Rey
230 kV - 2C – 0.4 km-C
SE Libramiento Banco 1
1 T - 3F - 40 MVA - 230/13.8 kV
SE Libramiento MVAr
1 Capacitor - 2.4 MVAr - 13.8 kV', NULL, NULL, NULL, NULL, N'En espera de transferencia de recursos de la Gerencia Regional de Transmisión.', N'Debido a que son proyectos compartidos, se formalizó contrato de servicios en noviembre de 2023 entre CFE Distribución y CFE Transmisión para llevar a cabo la ejecución del proyecto de la RNT.', N'Proyecto COMPARTIDO entre Transmisión y Distribución.
La administración y gerenciación del proyecto es por parte de Distribución.
Subestación Libramiento Banco 1
Distribución se encuentra en Actividades Previas para la Subestación y su componente de Línea (400 m aprox.)

Distribución enviará solicitud de Validación de ICM a GPIC en diciembre 2025.
El avance del 9% se debe a que se estan por concluir con las actividades previas y ya adquirió el predio.', N'Portafolio Activo', N'D19-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        130, 130, N'BC', N'Olas Altas Banco 2
(antes Obras de Refuerzo CCC Baja California Sur)', N'Presupuestal', 2020, N'En Operación', 135.29, N'SE  Olas Altas
3 AT - 1F - 33.3 MVA - 100 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 17.DIC.2024.', N'Portafolio Activo', N'CFE20-PCC', N'Concluido y en operación', NULL, NULL, NULL, 100.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        131, 131, N'NO', N'Pericos MVAr', N'Presupuestal', 2020, N'En Operación', 16.07, N'SE Pericos MVAr
1 Capacitor 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 20.DIC.2023', N'Portafolio Activo', N'P20-NO5', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        132, 132, N'NO', N'Juan José Ríos MVAr', N'Presupuestal', 2020, N'En Operación', 22.74, N'SE Juan José Ríos MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 26.FEB.2025.', N'Portafolio Activo', N'P20-NO3', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        133, 133, N'OR', N'Confiabilidad de Suministros de energía eléctrica en Nanchital II', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 64.04, N'LT Nanchital II Entronque Coatzacoalcos - Pajaritos II
115 kV - 2C - 7.2 km-C', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P20-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 7.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        134, 134, N'SE', N'Suministro de energía eléctrica en la zona San Cristóbal', N'Fibra E', 2020, N'En Proceso de Decisión', 939.74, N'LT Manuel Moreno Torres - San Cristóbal Oriente
115 kV - 2C - 65.0 km-C - 1 Cond/f - 795 ACSR - TA /1
SE Angostura (sustitución)
4 AT - 125 MVA - 500 MVA - 400/115 kV
SE Manuel Moreno Torres
1 Alimentador 115 kV (Ampliación) LT Manuel Moreno Torres - San Cristóbal Oriente
SE San Cristóbal Oriente
1 Alimentador 115 kV (Ampliación) LT San Cristóbal Oriente - Manuel Moreno Torres

1/ Tendido del primer circuito', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-OR3', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 500.00, 0.00, 71.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        135, 135, N'OC', N'Aumento de Capacidad de transformación al suroriente de la zona Metropolitana de Guadalajara (400/230kV)', N'Fibra E', 2020, N'En Proceso de Decisión', 459.91, N'SE Atequiza Banco 6​
3 AT - 1F - 100 MVA - 300 MVA - 400/230 kV​ /1
SE Atequiza​
 Reubicación de la LT Tlajomulco - Atequiza (A3L90)​
400 kV - 1C - 0.1 km-C - 1113 ACSR - TA /2
Recalibracion Barras 230 kV -  3 Cond/f - 1113 ACSR​
Reemplazo equipo serie ATQ 97010 /3
11 Transformadores de Corriente 230 kV /4,5

1/ Incluye paralelismo compatibles con bancos en Operación
Caseta de control distribuida PCYM para  nuevo equipamiento
2/ Liberar espació en SE para instalación de nuevo banco de transformación
/3 Capacidad de Equipo Asociado mínima de 31.5 kA
/4 Reemplazo de TCs existentes en el Bus 230 kV 
5/ RTC mínima 1600/5 A', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-OC1', N'En Concurso y Por Concursar', N'2026.1', N'2.2', N'Sí', 300.00, 0.00, 0.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        136, 136, N'OC', N'Atención del suministro en las zonas Zapotlán y Costa', N'Fibra E', 2020, N'En Proceso de Decisión', 1453.52, N'LT Tuxcacuesco Entronque  Manzanillo - Acatlán
400 kV - 2C - 0.2 km-C
LT Tuxcacuesco Entronque Autlán - El Grullo
115 kV - 2C - 70 km-C
LT Tuxcacuesco - Juan Rulfo
115 kV - 1C - 10.5 km-C
SE Tuxcacuesco Banco 1
4 T - 1F - 125 MVA - 500 MVA - 400/115 kV
2 Alimentadores 400 kV (Alimentador Nuevo)
3 Alimentadores 115 kV (Alimentador Nuevo)
SE Juan Rulfo
1 Alimentador (Ampliación)
1 Juego TC 115 kV (sustitución)
SE Sayula
1 Juego TC 115 kV (sustitución)
SE Melaque
1 Juego TC 115 kV (sustitución)
1 Recalibración de Bus 115 kV
SE La Huerta
1 Juego TC 115 kV (sustitución)
1 Recalibración de Bus 115 kV
SE Ciudad Guzmán MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Centro Logístico MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tecomates MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Ameca MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Sayula MVAr (traslado)
1 Capacitor - 18 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tuxcacuesco MVAr (traslado)
1 Reactor - 60 MVAr - 400 kV
1 Alimentador 400 kV Reactor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
En proceso de revisión, validación y aprobación de metas físicas, importes y periodo de construcción en la Ficha de Información del Proyecto entre CENACE y Transmisión.
Pendiente la elaboración del Caso de Negocio, Plan Financiero y Plan de Ejecución.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-OC2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 500.00, 130.50, 80.70, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        137, 137, N'OC', N'Aumento de Capacidad de transformación al suroriente de la zona Metropolitana de Guadalajara (230/69kV)', N'Fibra E', 2020, N'En Proceso de Decisión', 494.75, N'LT  El Salto Jalisco - Castillo /2,3
69 kV - 1C - 6.5 km-C - 1 Cond/f - 795 ACSR - PT 
LT Guadalajara II - Parque Industrial
69 kV - 1C - 4.0 km-C - 1 Cond/f - 795 ACSR - PT
Recalibración LT Castillo – 63580 – Parque Industrial
69 kV - 1C - 12.15 km-C - 1 Cond/f - 795 ACSR /4
SE Guadalajara II Banco 5
3 T - 1F - 33.33 MVA - 100 MVA - 230/69 kV /5,9
SE Guadalajara II
1 Alimentador 69 kV LT Guadalajara II - Parque Industrial
3 Transformadores de Corriente 69 kV LT Guadalajara II - 63770 - Parque Industrial /6
3 Transformadores de Corriente 69 kV LT Guadalajara II - 63690 - El Salto Jalisto /7
30 Cuchillas desconectadoras 69 kV /8
Recalibración Buses 69 kV - 3 Cond/f - 1113 ACSR
SE Parque Industrial
1 Alimentador 69 kV LT Parque Industrial - Guadalajara II
3 Transformadores de Corriente 69 kV LT Parque Industrial - 63770 - Guadalajara II /6
SE El Salto Jalisto
3 Transformadores de Corriente 69 kV LT El Salto Jalisto - 63690 - Guadalajara II /7
SE Atequiza
2 Interruptores 69 kV /8

2/ Inhabilitar LT El Salto Jalisco - 63670 - Castillo
3/ Se utilizarán los alimentadores existentes
4/ Si las estructuras no soportan el peso del conductor, debe recalibrarse con conductor 477 ACCR
Alta Temperatura
5/ Considerar el Paralelismo (Paralelos existentes que sean compatibles con los nuevos)
6/ Alcanzar una cargabilidad 157 MVA
7/ Alcanzar una cargabilidad 108 MVA
8/ Por violación de Icc.
9/ Reubicación de Equipo asociado por adición del nuevo Banco 5', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-OC3', N'En Concurso y Por Concursar', N'2026.2', N'5.1', N'Sí', 100.00, 0.00, 25.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        138, 138, N'OC', N'Aumento de capacidad de suministro para el sur de San Luis Potosí', N'En proceso de definición', 2020, N'Instruido y CON priorización', 587.09, N'Laguna San Vicente II Entronque Maniobras BMW - San Luis Potosí
 230 kV - 2C - 7.3 km-C
Laguna San Vicente II Entronque Laguna San Vicente - San Luis Potosí
 230 kV - 2C - 0.4 km-C
Laguna San Vicente II Entronque La Pila - Maniobras WTC y La Pila - Barracuda
115 kV - 4C - 9.8 km-C
Laguna San Vicente II - Logistik
115 kV - 1C - 3.7 km-C
Laguna San Vicente II - Logistik (Parques Industriales)
115 kV - 1C - 4.7 km-C
Modernización San Luis Indistrial - San Luis Potosí
115 kV - 1C - 1.2 km-C
SE Laguna San Vicente II
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Laguna San Vicente II
4 Alimentadores 230 kV 
6 Alimentadores 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización conciliada entre Transmisión, la Coordinación de Vinculación de la DP y el CENACE.
Pendiente definición de metodología de evaluación y esquema de financiamiento.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-OC4', N'En análisis por priorizar', NULL, NULL, N'Sí', 300.00, 0.00, 27.05, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        139, 139, N'NO
BC', N'Incremento en la capacidad de transformación en la zona Peñasco', N'Fibra E', 2020, N'En Proceso de Decisión', 520.48, N'LT Seis de Abril - Mar de Cortés (Conversión a 230 kV)
230 kV - 1C - 104 km-C - TA /45, 46
LT Oriente Entronque Puerto Peñasco - (Mar de Cortés) Seis de Abril
115 kV - 2C - 4.3 km-C - 795 ACSR - 1 Cond/f - PA /2
LT Mar de Cortés Entronque Maniobras Fresnillo (Playa Encanto) - Puerto Peñasco
115 kV - 2C - 0.3 km-C - 795 ACSR - 1 Cond/f - TA /2
LT Mar de Cortés Entronque Seis de Abril - Puerto Peñasco
230 kV - 2C - 0.1 km-C - 1113 ACSR - TA /2,3,4
SE Mar de Cortés Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Mar de Cortés
3 Alimentadores de LT 115 kV
1 Alimentador de LT 230 kV
SE Mar de Cortés MVAr
1 R Terciario - 3F - 21 MVAr - 13.8 kV
1 Alimentador 13.8 kV Reactor
SE Oriente MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Oriente
1 Alimentador de LT - 115 kV
SE Seis de Abril
1 Alimentador de LT - 230 kV (Ampliación)

2/ Tendido de los dos cirucitos
3/ El circuito del entronque hacia la SE Puerto PEñasco operará en 115 kV
4/ Uno de los ctos. operará inicialmente en 115 kV aislado en 230 kV, y el otro aislado y operado en 115 kV
/45 Al ser cambio de tensión no se contabiliza en las metas físicas
/46  Cambio de tensión LT 73A10', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NO1', N'En Concurso y Por Concursar', N'2026.4', N'12.2', N'Sí', 300.00, 43.50, 5.17, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        140, 140, N'NO', N'Incremento en la capacidad de transformación en la zona Hermosillo', N'Fibra E', 2020, N'En Proceso de Decisión', 467.29, N'LT Hermosillo Loma - Derivación Quiroga (Tramo 1)
115 kV - 2C -  13 km-C - 795 ACSR - 1 Cond/f /3
LT Derivación Quiroga - Punto de Inflexión Quiroga (Tramo 2)
115 kV - 2C - 1.5 km-C - 795 ACSR - 1 Cond/f /4
LT Punto de Inflexión Quiroga - Quiroga (Tramo 3)
115 kV - 2C - 1.0 km-C - 795 ACSR - 1 Cond/f /3, 12, 13
SE Hermosillo Loma Banco 2
3 AT - 1F - 75 MVA - 225 MVA - 230/115 kV
SE Hermosillo Loma
1 Alimentador 115 kV de LT Hermosillo Loma - Quiroga (Ampliación)
SE Quiroga
1 Alimentador 115 kV de LT Quiroga - Hermosillo (Ampliación)

3/Tendido del primer circuito
4/ Tendido del segundo circuito
12/ Cable subterráneo
13/ Capacidad mínima 179 MVA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NO2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 225.00, 0.00, 17.05, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        141, 141, N'NO', N'Cerro Cañedo MVAr', N'En proceso de definición', 2020, N'Instruido y CON priorización', 27.66, N'SE Cerro Cañedo
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P20-NO4', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        142, 142, N'NO', N'Incremento de la confiabilidad de la transformación en la Zona Mazatlán', N'Fibra E', 2020, N'En Proceso de Decisión', 635.89, N'SE Mazatlán Dos Banco 4 y Banco 5 (sustitución)
7 AT – 1F – 75 MVA - 525 MVA - 230/115 kV
SE El Habal Banco 1 (traslado MZD - AT5 y sustitución)
3 AT – 1F – 33.33 MVA - 100 MVA - 230/115 kV
SE Mazatlán Dos MVAr
4 R Barra - 1F - 25 MVAr - 100 MVAr - 400 kV
1 R Terciario - 3F - 30 MVAr - 13.8 kV
1 Alimentador 13.8 kV Reactor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NO6', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 625.00, 130.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        143, 143, N'NO', N'Eliminar limitaciones de capacidad en cables subterráneos de las Zonas Hermosillo, Obregón, Los Mochis, Culiacán y Mazatlán', N'Fibra E', 2020, N'Por concursar', 872.97, N'Zona Hermosillo, 13.53 km-C
Hermosillo Cuatro - Río Sonora /7,8
115 kV - 1C - 0.17 km-C
Hermosillo Cuatro  - Río Sonora /3,4,6
115 kV - 1C - 2.58 km-C
Hermosillo Uno - Río Sonora /3,4,6
115 kV - 1C - 5.68 km-C
Hermosillo Uno - Río Sonora /7,8
115 kV - 1C - 0.17 km-C
Pueblitos - Ladrilleras /1,2
115 kV - 1C - 0.10 km-C
Hermosillo Cereso - Villas del Pitic /1,5
115 kV - 1C - 4.83 km-C

Zona Obregón, 3.693 km-C
Tetabiate - Obregón Uno /1,2,3,4
115 kV - 1C - 3.20 km-C
Banderas - Ciudad Obregón Tres /1,2
115 kV - 1C - 0.493 km-C

Zona Los Mochis, 12.1 km-C
Los Mochis Tres - Centenario /1
115 kV - 1C - 8.6 km-C
Los Mochis Tres - Centenario /2,3,4,5
115 kV - 1C - 3.5 km-C

Zona Culiacán, 6.05 km-C
Culiacán Milenium - La Higuera /1,2
115 kV - 1C - 2.5 km-C
Jaime Sevilla - Culiacán Milenium /1,2,5
115 kV - 1C - 2.3 km-C
Jaime Sevilla - Culiacán Milenium /3,4
115 kV - 1C - 1.1 km-C
Culiacán Uno - Tres Ríos /1,2
115 kV - 1C - 0.15 km-C

Zona Mazatlán, 4.37 km-C
Mazatlán del Mar - Mazatlán Centro /1,2
115 kV - 1C - 4.37 km-C

1/ Construcción tramo con cable subterráneo
2/ Cable CU-XLP con capacidad de 179 MVA
3/ Utilizar línea existente en desuso Cable 795 ACSR
4/ Tendido del segundo circuito Cable 795 ACSR
5/ Cable CU-XLP con capacidad de 131 MVA
6/ Cambio del módulo subterráneo a aéreo en SE Río Sonora
7/ Construcción de línea de transmisión PT
8/ Cable 795 ACSR', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
El proyecto está en revaluación motivado a que la ICM superó el importe del Caso de Negocio.
Se cuenta con FIP y en proceso la revaluación del Caso de Negocio, Plan Financiero y Plan de Ejecución.
Se realizó una propuesta de fechas para su concurso:
10.NOV.25 Solicitud de Publicación
13.NOV.25 Publicación
16.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NO7', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 0.00, 43.71, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        144, 144, N'NT', N'Soporte de tensión para la región Mesteñas', N'Fibra E', 2020, N'Por concursar', 475.58, N'SE Oasis MVAr
1 STATCOM - 40 / +40 MVAr - 115 kV
1 Alimentador 115 kV STATCOM
SE Mesteñas MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE El Trébol MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
Se realizó una propuesta de fechas para su concurso:
08.OCT.25 Solicitud de Publicación
13.OCT.25 Publicación
08.DIC.25 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NT1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 125.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        145, 145, N'NT', N'Soporte de tensión para las zonas Nuevo Casas Grandes y Moctezuma', N'Fibra E', 2020, N'En Proceso de Decisión', 881.83, N'LT Maniobras Santa María Solar Entronque Galeana - Casas Grandes
115 kV – 4C – 2.0 km-C
SE Maniobras Santa María Banco 1 
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Maniobras Santa María 
6 Alimentadores 115 kV Líneas de Transmisión
1 Alimentador 115 kV (Amarre)
SE Maniobras María Solar MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE Gavilán MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE Villa Ahumada MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE Valle Esperanza MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE El Capulin MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE Monteverde MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor
SE Vado Santa María MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Aimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NT2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 105.00, 2.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        146, 146, N'NE', N'Soporte de tensión para la zona Nuevo Laredo', N'Fibra E', 2020, N'Por concursar', 370.84, N'SE Nuevo Laredo MVAr
1 STATCOM - -50 / +200 MVAr - 138 kV
1 Alimentador 138 kV STATCOM
SE Falcón México MVAr
1 Capacitor - 18 MVAr - 138 kV
1 Alimentador 138 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
El proyecto está en revaluación motivado a que la ICM superó el importe del Caso de Negocio.
Se cuenta con FIP y en proceso la revaluación del Caso de Negocio, Plan Financiero y Plan de Ejecución.
Se realizó una propuesta de fechas para su concurso:
10.NOV.25 Solicitud de Publicación
13.NOV.25 Publicación
14.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NE1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 268.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        147, 147, N'NE', N'Aumento de capacidad de transformación en la zona Matamoros', N'Fibra E', 2020, N'En Proceso de Decisión', 1081.53, N'LT Matamoros Potencia - Matamoros
138 kV - 2C - 14 km-C - 1 Cond/f - 1113 ACSR - TA /2,3
LT Matamoros Potencia - Lauro Villar
138 kV - 1C - 24 km-C - 1 Cond/f - 1113 ACSR - TA
SE Matamoros Potencia Banco 2
3 AT - 1F - 75 MVA -  225 MVA - 230/138 kV
SE Valle Hermoso MVAr
1 Capacitor - 18 MVAr - 138 kV
1 Alimentador 138 kV Capacitor
SE Matamoros Potencia (Ampiación)
1 Alimentador 138 kV LT Matamoros Potencia - Matamoros
1 Alimentador 138 kV LT Matamoros Potencia - Lauro Villar
SE Matamoros (Ampiación) 
1 Alimentador 138 kV LT Matamoros - Matamoros Potencia 
SE SE Lauro Villar (Ampiación) 
1 Alimentador 138 kV LT Matamoros Potencia - Lauro Villar

2/ Tendido del segundo circuito
3/ Actual LT Matamoros Potencia - 83680 - Matamoros', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-NE2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 225.00, 18.00, 41.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        148, 148, N'PE', N'Reforzamiento de la Red eléctrica para atender el crecimiento de la demanda del corredor Industrial Mérida - Umán', N'Fibra E', 2020, N'En Proceso de Decisión', 93.36, N'LT Maxcanú Entronque Lerma - 73ET0 - Hunxectamán
115 kV - 2C - 0.7 km-C - 1 Cond/F - 477 ACSR/AS-TA
SE Maxcanú
1 Capacitor  - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
1 Alimentador 115 kV LT Maxcanú - Lerma
1 Alimentador 115 kV LT Maxcanú - Hunxectamán', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-PE1', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 0.00, 15.00, 0.66, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        149, 149, N'PE', N'Reforzamiento de la Red eléctrica para atender el crecimiento de la demanda del corredor Ticul - Chetumal en 115 kV', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 132.17, N'LT Ticul I - Tekax II
115 kV - 1C - 33 km-C
LT Ticul Potencia - Ticul I
115 kV - 1C - 12 km-C /5
SE Ticul I
1 Alimentador 115 kV  (Ampliación)
SE Tekax II
1 Alimentador 115 kV  (Ampliación)

/5 Recalibración', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P20-PE2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 45.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        150, 150, N'BC', N'Solución a la problemática de bajos voltajes al sur de la Zona Ensenada', N'Fibra E', 2020, N'Por concursar', 229.68, N'SE San Quintín MVAr
1 STATCOM - -30/+30 MVAr - 115 kV
1 Alimentador 115 kV STATCOM
SE San Quintín
1 Alimentador 115 kV Normalizar LT San Quintín - 73130 - Cañón', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
Se realizó una propuesta de fechas para su concurso:
08.OCT.25 Solicitud de Publicación
13.OCT.25 Publicación
04.DIC.25 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P20-BC1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 60.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        151, 151, N'BC', N'Compensación Capacitiva en Zona Los Cabos', N'Fibra E', 2020, N'En Proceso de Decisión', 33.40, N'SE Monte Real MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Buena Vista MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-BS1', N'En Concurso y Por Concursar', N'2026.1', N'2.1', N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        152, 152, N'BC', N'Incremento en la capacidad de transformación en zona Los Cabos', N'Fibra E', 2020, N'En Proceso de Decisión', 573.22, N'LT El Palmar - Monte Real /1
115 kV – 2C – 16.83 km-C - 1 Cond/f - 795 ACSR - PT /1
SE El Palmar Banco 3
4 AT – 1F – 75 MVA - 300 MVA - 230/115 kV
SE El Palmar
1 Alimentador (Ampliación) 115 kV LT El Palmar - Monte Real
SE Monte Real
1 Alimentador (Ampliación) 115 kV LT Monte Real - El Palmar
1/Tendido del primer circuito', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P20-BS2', N'En Concurso y Por Concursar', N'2026.3', N'8.1', N'Sí', 300.00, 0.00, 16.83, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        153, 153, N'BC', N'Solución integral al suministro de energía eléctrica de la zona Constitución', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 10.67, N'SE Villa Constitución
1 STATCOM - -50/+50 MVAr - 115 kV
1 Alimentador 115 kV STATCOM', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P20-BS3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 100.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        154, 154, N'CE', N'Modernización de Equipos de Protección y Control de Bancos de capacitores serie CS1, CS2 y CS3 de Subestación Donato Guerra', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 263.87, N'SE Donato Guerra
Bancos de Capacitores Serie CS1 /1
Bancos de Capacitores Serie CS2 /1
Bancos de Capacitores Serie CS3 /1

1/ Modernización de Equipos de Protección, Control y Medición.', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M20-CE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        155, 155, N'CE', N'Modernización de Equipos de Protección y Control de Bancos de capacitores serie CS2, CS3 y CS4 de Subestación Tecali', N'Fibra E', 2020, N'En Proceso de Decisión', 277.93, N'SE Tecali MVAr (sustitución)
1 Interruptor de Potencia 400 kV (bypass) - Capacitor Serie 4 /1
1 Interruptor de Potencia 400 kV (bypass) - Capacitor Serie 2 /1
1 Tablero de Protección, Control y Medición - Capacitor Serie 2
1 Tablero de Protección, Control y Medición - Capacitor Serie 3
1 Tablero de Protección, Control y Medición - Capacitor Serie 4

1/ Adecuaciones al Control Supervisorio', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M20-OR2', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        156, 156, N'OR', N'Modernización equipo de Equipo Primario, de Protección, Control, Comunicaciones y Medición en la Subestación Minatitlán II 115 Kv', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 67.76, N'SE Minatitlán II
1 Caseta de Control
36 Cuchillas desconectadoras 115 kV
13 Equipos de PCyM
1 Equipo SCADA
Obra Electromecánica
Obra Civil', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M20-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        157, 157, N'NO', N'Eliminar derivación de la Línea de Subtransmisión de Guasave ( 73150) - San Rafael Ampliación -Bamoa', N'Pendiente de definir', 2020, N'Con acuerdo de Cancelación', 21.64, N'Conexión de Alimentador a LT Nueva San Rafael Ampliación - Bamoa
115 kV - 1C - 0.2 km-C
SE San Rafael
Completar Bahía (ampliación) 115 kV
1 Interruptor de Potencia 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV
SE Bamoa
Instalación de Equipo de PCyM
Extensión de Bus Auxiliar', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'La problemática quedará atendida con el P22-NO1.
Se solicitará cancelación.', N'B3: Con priorización sólo por CFET', N'M20-NO1', N'Con acuerdo de cancelación', NULL, NULL, NULL, 0.00, 0.00, 0.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        158, 158, N'NO', N'Eliminar derivación de los transformadores en SE San Rafael de la LT Guamchil - 73730 -San Rafael', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 11.71, N'SE San Rafael
 LT Guamúchil - 73730 - San Rafael
Eliminar TAP del Transformador T1 y T2
3 Estructuras Mayor y Menor Buses 2 Niveles Bahía 73730
1 Interruptor de Potencia 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV
Instalación de Equipo de PCyM
1 Extensión del Bus Principal y Bus Auxiliar
SE San Rafael Ampliación
1 Extensión del Bus Principal y Bus Auxiliar
Tablero PCyM', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        159, 159, N'NO', N'Eliminar derivación de la SE Salvador Alvarado de la LT Guamúchil -73730 - San Rafael', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 7.08, N'SE Salvador Alvarado
Eliminar derivación LT Guamúchil a San Rafael LT GML 73730 SRF
115 kV - 1C - 0.25 km-C  Tramo SAA - GML
Eliminar derivación LT Guamúchil a San Rafael LT GML 73730 SRF
115 kV - 1C - 0.25 km-C  Tramo SAA - SRF
4 Instalación de Estructura Mayor Buses 2 Niveles
Bahías
GML 73730 SAA
SFR 73XX0 SAA
SAA 72010
SAA 77010
3 Interruptores de Potencia 115 kV
8 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
9 Transformadores de Potencial 115 kV
6 Apartarrayos 115 kV
12 AIs 115 kV
Instalación de Equipo de PCyM', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        160, 160, N'NT', N'Cambio de arreglo de la SE Moctezuma en 230 y 115 kV', N'En proceso de definición', 2020, N'Instruido y CON priorización', 111.00, N'Obras de Modernización:
Construcción de una bahía nueva incluyendo esquemas de PCyM para separar el B1 en dos barras.
Construcción de una bahía nueva entre las dos secciones del bus de transferencia de 115 kV, para poder utilizarlo como amarre de barras.
SE Moctezuma
Cambio arreglo a 4 Buses 230 kV
Reubicación Bahía 93670 230 kV
Reubicación Bahía 93250 230 kV
3 Bahías (nuevas) 230 kV
1 Bahías (nueva) 115 kV
6 Cuchillas desconectadoras 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-NT1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        161, 161, N'NT', N'Modernización de la red de transmisión de la zona Durango', N'En proceso de definición', 2020, N'Instruido y CON priorización', 211.86, N'LT CM Centauro - 73370 - Amado Nervo
115 kV - 1C - 32.2 km-C /1
LT Amado Nervo - 73350 - C.M. La Parrilla
115 kV - 1C - 11.8 km-C /1
LT C.M. La Parrilla - 73570 - Vicente Guerrero
115 kV - 1C - 10.6 km-C /1

1/ Recalibración y cambio de estructuras', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-NT2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 54.60, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        162, 162, N'NE', N'Adición de protecciones 87B a subestaciones de la red de transmisión en el ámbito de la Gerencia Regional de Transmisión Noreste', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 68.87, N'11 SE''s programadas para el 2021
ZT Monterrey Poniente 
3 Bahías - SE Cumbres Poniente
4 Bahías - SE Solidaridad
4 Bahías - SE Modelo
3 Bahías - SE San Martín
ZT Monterrey Oriente
4 Bahías - SE Prolec
4 Bahías - SE La Fe
4 Bahías - SE Monte Kristal
ZT Monclova
6 Bahías - SE Acero
ZT Río Escondido
5 Bahías - SE Piedras Negras
ZT Frontera
6 Bahías - SE Anzalduas
7 Bahías - SE Jarachina
11 SE''s programadas para el 2022
ZT Monterrey Poniente 
4 Bahías - SE Nuevo Escobedo
4 Bahías - SE Girasoles
4 Bahías - SE El Canadá
7 Bahías - SE Estrella
ZT Monterrey Oriente
4 Bahías - SE Villa Juáres II
3 Bahías - SE Juárez Norte
4 Bahías - SE Josefa Zozaya
ZT Monclova
3 Bahías - SE Regidores
ZT Río Escondido
5 Bahías - SE Fuentes
ZT Frontera
4 Bahías - SE México
3 Bahías - SE Orizatlán
11 SE''s programadas para el 2023
ZT Monterrey Poniente 
3 Bahías - SE Ciénega de Flores II
4 Bahías - SE Topochico
5 Bahías - SE Félix U. Gómez
4 Bahías - SE Mezquital
4 Bahías - SE Finsa Guadalupe
ZT Monclova
6 Bahías - SE Xochipili
ZT Río Escondido
3 Bahías - SE Parque Industrial Acuña
ZT Frontera
5 Bahías - SE Parque Industrial
5 Bahías - SE Petrolera
ZT Monterrey Oriente
5 Bahías - SE Allende
4 Bahías - SE Linares
11 SE''s programadas para el 2024
ZT Monterrey Poniente 
4 Bahías - SE Agua Nueva
4 Bahías - SE Las Brisas
3 Bahías - SE Capellanía
3 Bahías - SE Morelos
ZT Río Escondido 
3 Bahías - SE Puente Internacional
ZT Frontera
3 Bahías - SE Parque Industrial Colonial
3 Bahías - SE Parque Industrial Prologis
3 Bahías - SE Río Bravo Poniente
ZT Monterrey Oriente
3 Bahías - SE Montemorelos
6 Bahías - SE Villa de Santiago
7 Bahías - SE Mante
12 SE''s programadas para el 2025
ZT Monterrey Poniente 
4 Bahías - SE Parque Industrial Amistad
5 Bahías - SE Parque Industrial Santa María
6 Bahías - SE Ramos Arizpe
5 Bahías - SE Zapaliname
4 Bahías - SE Ladrillera
ZT Río Escondido 
3 Bahías - SE Finsa Laredo
4 Bahías - SE Ojo Caliente
ZT Frontera
3 Bahías - SE Ricsa
3 Bahías - SE Verde Corporate
4 Bahías - SE Villa Florida
ZT Monterrey Tampico
6 Bahías - SE El Salto
ZT Monterrey Oriente
5 Bahías - SE El Olivo
Total 56 SE''s para implementar esquemas 87B', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-NE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        163, 163, N'NE', N'Reemplazo de transformadores de potencia por término de su vida útil', N'Fibra E', 2020, N'Por concursar', 1516.07, N'SE Huinalá AT 7 (sustitución)
4 AT - 1F- 125 MVA - 500 MVA - 400/230/13.8 kV
SE Huinalá AT 8 (sustitución)
4 AT - 1F - 33.33 MVA - 133.33MVA - 230/115/13.8 kV
SE Villa de García AT 2 (sustitución)
3 AT - 1F - 125 MVA - 375 MVA - 400/230/13.8 kV
SE Saltillo AT 2 (sustitución)
4 AT - 1F - 33.33 MVA - 133.33 MVA - 230/115/13.8 kV
SE Escobedo AT 3 (sustitución)
4 AT - 1F- 33.33 MVA - 133.33 MVA - 230/115/13.8 kV
SE Arrollo del Coyote AT 4 (sustitución)
4 AT - 1F- 33.33 MVA - 133.33 MVA - 230/138/13.8 kV
SE Nava AT 3 (sustitución)
 1 AT - 3F - 40 MVA - 230/138/13.8 kV
SE Río Bravo AT 6 (sustitución)
4 AT - 1F - 46.66 - 186.66 MVA - 230/138/13.8 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
Se realizó una propuesta de fechas para su concurso:
31.OCT.25 Solicitud de Publicación
05.NOV.25 Publicación
13.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'M20-NE2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 1635.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        164, 164, N'BC', N'Modernización de arreglo de barras en la SE Ciprés en 230 y 115 kV', N'Pendiente de definir', 2020, N'Instruido y SIN priorización', 31.99, N'SE Cipres
Cambio de arreglo de barras a BP - BA 115 y 230 kV
11 Cuchillas desconectadoras 115 kV
14 Aisladores Soporte tipo Columna 115 kV
6 Transformadores de Corriente 115 kV
24 Transformadores de Potencial 115 kV
4 Cuchillas desconectadoras 230 kV
12 Aisladores Soporte tipo Columna 230 kV
6 Transformadores de Potencial 115 kV
Instalación de Equipo de PCyM
Obra Civil y Electromecánica', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M20-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        165, 165, N'BC', N'Modernización de arreglo de barras y de la transformación en la SE Panamericana Potencia', N'PIDIREGAS PPEF 2026', 2020, N'En Proceso de Decisión', 355.56, N'SE Panamericana Potencia Banco 4 /1,2
3 AT - 1F - 75 MVA - 225 MVA - 230/115-69 kV (sustitución)
SE Panamericana Potencia
Cambio de arreglo Barras (Bus 115 y 230 kV con cable conductor alta temperatura 2C/F - 1113 MCM ACSS/TW
23 Cuchillas deconectadoras 115 kV
3 Transformadores de Corriente 115 kV
24 Transformadores de Potencial 115 kV
85 Aisladores Soporte 115 kV
21 Cuchillas deconectadoras 230 kV
6 Transformadores de Corriente 230 kV
9 Transformadores de Potencial 230 kV
3 Apartarrayos 230 kV
2 Tableros PCyM en 115 kV
  Sección TT-PA-PA-IN
  Sección IA-9-PA-IN
  Sección LT-7-51-51-PA-IN
  Secicón MM-IN) 
3 Tableros PCyM en 230 kV
  Sección TT-PA-PA-IN
  Sección IA-9-PA-IN
  Sección LT-7-51-51-PA-IN
  Secicón MM-IN) 
1 Obra Civil y Electromecánica (Cimentaciones, Estructuras menores, etc.)

1/ Operación Inicial 230/69 kV
2/ Se realizará la obra del proyecto colindante a los equipos de transformación de 225 MVA de capacidad asociados al proyecto P17-BC14 Panamericana Potencia Banco 3 que serán instalados (T30) derivado de implicaciones constructivas y operativas, posteriormente serán retirados los equipos de T20 y serán eniando al almacén de la ciudad de Mexicali.', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'M20-BC2', N'En Concurso y Por Concursar', N'2026.3', N'7.1', N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        166, 166, N'PE', N'357 SLT Aumento de capacidad de transm zonas Cancún y Riviera Maya II
(Fase 2 - Leona Vicario)', N'PIDIREGAS 2022', 2020, N'Ejecución/Construcción', 3747.18, N'LT Leona Vicario Entronque Dzitnup - Riviera Maya
400 kV - 2C - 90.2 km-C
LT Leona Vicario - Kohunlich
115 kV - 2C - 16.28 km-C
LT Leona Vicario - Yaxché
115 kV - 2C - 7.7 km-C
LT Leona Vicario - Kekén
115 kV - 2C - 11.66 km-C
LT Canek - Kohunlich
115 kV - 1C - 6.27 km-C
SE Leona Vicario
4 AT - 1F - 125 MVA - 500 MVA - 400/115 kV
(Incluye fase de reserva)
2 Alimentadores 400 kV
8 Alimentadores 115 kV
SE Leona Vicario MVAr
1 STATCOM - -200/+200 MVAr - 115 kV
1 Alimentador 115 kV STATCOM
4 R Barra - 1F - 16.66 MVAr - 66.6 MVAr - 400 kV
1 Alimentador 400 kV Reactor', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'P20-PE3', N'Ejecución/Construcción', NULL, NULL, N'Sí', 500.00, 466.60, 132.11, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        167, 167, N'OR', N'Obras de Refuerzo CCC Tuxpan Fase I', N'Presupuestal', 2020, N'Ejecución/Construcción', 279.33, N'Obra de refuerzo asociadas a la CCC Tuxpan Fase 1
SE Tuxpan Vapor
Sustitución de Equipo
13 interruptores, 33 TC''s, 34 cuchillas, 8 TO''s y equipo de comunicación.
SE Poza Rica II
Sustitución de Equipo
2 TO''s, 6 TC’s  y equipos de comunicación
LT Tuxpan Vapor - A3580 - Poza Rica II
Tendido de 69.0 km de FO', NULL, NULL, NULL, NULL, N'La contratista tuvo atrasos por la llegada tardía de los suministros lo que impacta en el avance de la construcción / ejecución.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.
Este proyecto tiene restricciones por CENACE para la autorización de licencias por tratarse de trabajos en equipos energizados, por lo que es realizan semanalmente reuniones para conciliar programa de licencias en función de los avances en las bahías terminadas y listas para integrar al sistema existente.
Se tiene el acuerdo de que la contratista aumentará su fuerza de trabajo y de ser necesario ampliar los horarios y días de trabajo para recuperar el avance de las obras.', N'En proceso Etapa 3 Ejecución / Construcción.
El proyecto continúa con la instalación y montaje de los equipos que van llegando a sitio y se tiene el compromiso de concluir en la nueva fecha acordada de AGO.2025 por lo que se está dando un seguimiento muy puntual por parte de Transmisión.', N'Portafolio Activo', N'CFE20-TUC', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        168, 168, N'PE', N'Obras de Refuerzo CCC Valladolid', N'Presupuestal', 2020, N'En Operación', 255.57, N'SE Escárcega Potencia
4 R Barra - 1F - 33.3 MVAr - 133.3 MVAr - 400 kV
SE Nizuc
Sustitución de equipo (43 cuchillas)
Adecuaciones en subestaciones: Escárcega Potencia y Nizuc
Recalibración de alimentaciones de SP en Caseta de Control de 400 kV
Suministro y tendido de cable de control.', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 19.DIC.2024.', N'Portafolio Activo', N'CFE20-VAC-R', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 133.20, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        169, 169, N'PE', N'Obras de Refuerzo CCC Mérida', N'Presupuestal', 2020, N'En Operación', 461.05, N'LT Caucel Potencia – Poniente (73570) (recalibración)
115 kV - 1C - 4.9 km-C 1C/F 611 ACCC PA/TA
LT Caucel Potencia – Poniente (73590) (recalibración)
115 kV - 1C - 4.9 km-C 1C/F 611 ACCC PA/TA
SE Mérida II MVAr
1 Compensación Discreta Distribuida de 1 x 30 MVAr
1 Compensación Discreta Distribuida de 1 x 15 MVAr
Sustitución de equipos (24 Int., 42 Cuchillas y 72 TCs)
SE Sur MVAr
1 Compensación Discreta Distribuida de 1 x 30 MVAR
SE Kanasín Potencia
Sustitución de equipos (5 Int., 28 Cuchillas y 24 TCs)
SE Norte
Sustitución de equipos (41 Cuchillas y 11 TCs)
SE Itzaes
Sustitución de equipos (3 TCs)
SE Nachi-Cocom
Sustitución de equipos (43 Cuchillas)
Adecuaciones en: Subestaciones Mérida II, Poniente, Kanasín Pot., Itzaes Nachicocom, Norte  y Sur.
Recalibración de barras, suministro de equipos de PCyC, sustitución de estructuras de concreto por metálica tipo celosía y de cable de control.', NULL, NULL, NULL, NULL, N'No aplica.', N'No aplica.', N'Se energizó el 05.MAR.2025.', N'Portafolio Activo', N'CFE20-MDC-R', N'Concluido y en operación', NULL, NULL, NULL, 0.00, 75.00, 9.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        170, 170, N'BC', N'Obras de Refuerzo CCC San Luís Río Colorado', N'Presupuestal', 2020, N'Ejecución/Construcción', 1064.34, N'LT SE Cerro Prieto II – SE Sánchez Taboada
230 kV - 2C - 36 km-C /4
LT Ruiz Cortines Entronque Parque Industrial – Ruiz Cortines
230 kV - 2C - 6 km-C /5, 46
LT Parque Industrial – Ruiz Cortínes
230 kV - 1C - 0 km-C / 5, 46, 47
LT Parque Industrial – Hidalgo
230 kV - 1C - 0 km-C / 5, 46, 47
LT Cerro Prieto I – Ruiz Cortines
230 kV - 1C - 2 km-C  / 47 , 48
SE Ruiz Cortines Banco 1 (traslado)
4 AT - 1F - 75 MVA - 300 MVA - 230/161 kV
3 Alimimentadores  230 kV
SE Ruiz Cortines Banco 2
3 AT - 1F - 75 MVA - 225 MVA - 230/161 kV
SE Parque Industrial Banco 3 (sustitución)
1T - 3F - 40 MVA - 230/13.8 kV
SE Paredones Potencia MVAr (traslado desde SE Parque Industrial)
1 Capacitor - 21 MVAr - 161 kV /4
1 Alimentador 161 kV Capacitor
SE Sánchez Taboada
1 Alimentador 230 kV
SE Cerro Prieto II
1 Alimentador 230 kV
SE Cerro Prieto III
1 Alimentador 230 kV
SE Parque Industrial
2 Alimentadores - 230 kV

/4Tendido del segundo circuito
5/ Cambio de tensión de operación a 230 kV
46/ Se forma las LT Ruiz Cortines - Parque Industrial en 230 kV y las LT Ruiz Cortines - Hidalgo en 161 kV
47/ Cambio de tensión de operación de 161 kV a 230 kV
48/ Se modifica punto de interconexión de Cerro Prieto Uno a Cerro Prietp Tres, formando la LT Cerro Prieto Tres - Ruiz Cortines en 230 kV', NULL, NULL, NULL, NULL, N'La contratista tuvo atrasos por la llegada tardía de los suministros lo que impacta en el avance de la construcción / ejecución.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.
Se tiene el acuerdo de que la contratista aumentará su fuerza de trabajo y de ser necesario ampliar los horarios y días de trabajo para recuperar el avance de las obras.', N'En proceso Etapa 3 Ejecución / Construcción.
El proyecto continúa con la instalación y montaje de los equipos que van llegando a sitio y se tiene el compromiso de concluir en la nueva fecha acordada de JUN.2025 por lo que se está dando un seguimiento muy puntual por parte de Transmisión.', N'Portafolio Activo', N'CFE20-ESL', N'Ejecución/Construcción', NULL, NULL, N'Sí', 525.00, 21.00, 44.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        171, 171, N'BC', N'Obras de Refuerzo CCC González Ortega', N'Presupuestal', 2020, N'Ejecución/Construcción', 4022.15, N'LT Tijuana Uno - La Herradura
230 kV - 2C - 32.6 km-C - 2C/F 1113 ACSR TA/PA
La Herradura - La Rosita (Op. Inicial en 230 kV pero aislada en 400 kV)
400 kV - 2C - 225.56 km-C - 2C/F 1113 ACSR TA
SE La Herradura
4 Alimentadores 230 kV
SE Tijuana 
2 Alimentadores 230 kV
SE La Rosita
1 Alimentador 230 kV 
Reemplazar equipo que limita la capacidad de LTs en 161 kV
Adecuaciones en las SE’s La Herradura, Tijuana Uno y La Rosita
asociadas a la interconexión
(suministro e instalación de secciones de PCyM)
SE Presidente Juárez
Sustitución de Equipo por Ncc
Interruptores
Cuchillas
Transformadores de Corriente
Adecuaciones:
SE Carranza
SE CETyS
SE Cerro Prieto IV
SE Mexicali Oriente
SE González Ortega
SE Mexicali I', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'CFE20-GCC', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 0.00, 258.16, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        172, 172, N'NO
OC
NT', N'359 SLT Solución congestión de enlaces transm GCR Noro Occid Norte, I20-SIN1
FASE 1', N'FIDEICOMISO 1320', 2021, N'Ejecución/Construcción', 4686.57, N'I20-SIN1 Fase 1

SE Mazatlán Dos
1 STATCOM - +300/-300 MVAr - 400 kV
1 Alimentador 400 kV STATCOM
SE Primero de Mayo
1 STATCOM - 300/-300 MVAr - 400 kV
1 Alimentador 400 kV STATCOM
SE Seri
1 STATCOM - +200/-200 MVAr - 230 kV
1 Alimentador 400 kV STATCOM
SE Nuevo Casas Grandes
1 STATCOM - +300/-300 MVAr - 230 kV
1 Alimentador 400 kV STATCOM', NULL, NULL, NULL, NULL, N'Ninguna.', N'Se llevan a cabo reuniones semanales y mensuales de seguimiento de avances (Obra Civil, Obra Electromecánica y Suministros) para que el proyecto concluya en la fecha de término contractual.', N'En proceso Etapa 3 Ejecución / Construcción.', N'Portafolio Activo', N'I20-SIN1', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 2200.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        173, 173, N'SE', N'Suministro de energía eléctrica en la zona Tapachula y San Cristóbal', N'Fibra E', 2021, N'En Proceso de Decisión', 557.66, N'Tapachula Aeropuerto MVAr
1 STATCOM - -50/+50 MVAr - 115 kV /1
1 Alimentador 115 kV STATCOM
 Huixtla MVAr
1 STATCOM - -50/+50 MVAr - 115 kV /1,2
1 Alimentador 115 kV STATCOM
 
1/ Incluye transformador trifásico para su acoplamiento
2/ Incluye reubicacion de TAP LT Mapastepec - Belisario Domínguez', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P21-OR1', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 0.00, 200.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        174, 174, N'NO', N'Compensación capacitiva al sur de la zona Culiacán', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 15.90, N'SE El Dorado
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        175, 175, N'NO', N'Compensación capacitiva al noroeste de la zona Mazatlán', N'En proceso de definición', 2021, N'Instruido y CON priorización', 14.35, N'SE La Cruz
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        176, 176, N'CE', N'Incremento en la capacidad de transmisión en el corredor Teotihuacán - Texcoco en 400 kV.', N'En proceso de definición', 2021, N'Instruido y CON priorización', 133.50, N'LT Texcoco - Teotihuacán
LT TEX - A3W10 - TTH /5
LT TEX - A3W20 - TTH /5
400 kV - 2C - 32.14 km-C

/5 Recalibración', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-CE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 32.14, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        177, 177, N'CE', N'Suministro de energía eléctrica en Morelos', N'PIDIREGAS PPEF 2026', 2021, N'En Proceso de Decisión', 1024.15, N'LT Ciclo Combinado Centro Entronque Cuautla Dos - 73180 - Cuautla Industrial
115 kV - 2C - 2 km-C
LT Ciclo Combinado Centro Entronque Cuautla Dos - 73150 Tepalcingo
115 kV - 2C - 2 km-C
SE Ciclo Combinado Centro Banco 1
4 T - 1F - 125 MVA - 500 MVA - 400/115 kV
SE Yautepec Potencia
3 Transformadores de Corriente 115 kV
SE Jiutepec
3 Transformadores de Corriente 115 kV
Recalibración de Bus y Bajante de puentes a 500 MCM CU (624m)
SE Ciclo Combinado Centro
4 Alimentadores 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P21-OR2', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 500.00, 0.00, 4.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        178, 178, N'OR', N'Refuerzo de Transmisión en la zona Xalapa', N'En proceso de definición', 2021, N'Instruido y CON priorización', 75.10, N'Zona Xalapa
LT Veracruz II - Tamarindo II
115 kV - 2C - 36 km-C
SE Veracruz II
1 Alimentador 115 kV LT Veracruz II - Tamarindo II
SE Tamarindo II
1 Alimentador 115 kV LT Tamarindo II - Veracruz II', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-OR3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 36.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        179, 179, N'CE', N'Incremento en capacidad de transmisión en la red de 115 kV de la zona Querétaro.', N'Fibra E', 2021, N'En Proceso de Decisión', 240.78, N'LT Querétaro Potencia - Querétaro Sur
115 kV - 1C - 5.0 km-C
LT Satélite - La Loma
115 kV - 1C - 0.8 km-C
LT Querétaro Uno - Satélite
115 kV - 1C - 0.4 km-C', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
En proceso de revisión, validación y aprobación de metas físicas, importes y periodo de construcción en la Ficha de Información del Proyecto entre CENACE y Transmisión.
Pendiente la elaboración del Caso de Negocio, Plan Financiero y Plan de Ejecución.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P21-OC1', N'En Concurso y Por Concursar', N'2026.3', N'8.1', N'Sí', 0.00, 0.00, 6.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        180, 180, N'OC', N'Incremento de transformación en la zona Los Altos', N'En proceso de definición', 2021, N'Instruido y CON priorización', 214.90, N'SE San Juan II Banco 2
3 AT - 1F - 75 MVA - 225 MVA - 230/115 kV
SE Capilla Guadalupe MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tepetitlán MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Peñuelas MVAr (traslado)
1 Capacitor - 18 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P21-OC2', N'En análisis por priorizar', NULL, NULL, N'Sí', 225.00, 55.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        181, 181, N'OC', N'Soporte de tensión para la zona Minas', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 144.90, N'LT Minas Entronque La Venta - Santa Rosa (63970)
69 kV - 2C - 0.2 km-C
LT Minas Entronque Amatlán - Tala (actual Tala - Santa Rosa 73980)
 69 kV - 2C - 0.2 km-C
SE Minas MVAr
1 Capacitor - 8.1 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
4 Alimentadores - 69 kV
SE La Vega MVAr
1 Capacitor - 5 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
SE Estancia MVAr
1 Capacitor - 5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P21-OC3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 18.10, 0.40, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        182, 182, N'CE', N'Incremento de transformación en la zona Querétaro', N'PIDIREGAS PPEF 2026', 2021, N'En Proceso de Decisión', 1795.79, N'LT Otomí Entronque Las Delicias - 93300 - Querétaro Potencia
230 kV - 2C - 12 km-C
LT Otomí - Colinas de Apasco
115 kV - 2C - 6.5 km-C 1/
LT Otomí Entronque La Loma - 73860 - Querétaro Poniente
115 kV - 2C - 26 km-C
LT La Loma - 73860 - Querétaro Potencia /2
115 kV - 1C - 3.8 km-C
LT Querétaro Maniobras - 73220 - Querétaro Industrial
115 kV - 1C - 3.2 km-C
0.6 km CS 1600 mm2 cU
1 poste y fosa de transición
12 terminales tipo exterior 115 kV
SE Otomí Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Colinas de Apaseo MVAr /1
1 Cpacitor - 30 MVAr - 115 kV
SE Otomí
2 Alimentadores Nuevos 230 kV
1 Alimentador Amarre 230 kV
3 Alimentadores Nuevos 115 kV
1 Alimentador Amarre 115 kV
SE Colinas de Apaseo
1 Alimentador Ampliación 115 kV
SE Querétaro Industrial
1 Interruptor de Potencia 115 kV
3 Cuchillas desconectadoras 115 kV
1 Equipo PCyM
SE La Loma
1 Interruptor de Potencia 115 kV
3 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
1 Equipo PCyM
SE Querétaro Poniente
1 Interruptor de Potencia 115 kV
3 Cuchillas desconectadoras 115 kV
3 Apartarrayos 115 kV
1 Equipo PCyM
1 Recalibración Bus 115 kV ACSR-AS 795 MCM 120 m
SE Querétato Maniobras /3
1 Recalibración Bus 115 kV ACSR-AS 795 MCM 120 m
1 Interruptor de Potencia 115 kV
3 Cuchillas desconectadoras 115 kV
SE Las Delicias
1 Equipo PCyM
SE Querétaro Potencia
1 Equipo PCyM

1/ Tendido del primer circuito
2/ Recalibración
3/ Para incrementar la capacidad de transmisión de la LT QRP 73580 QMR a 179 MVA', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P21-OC4', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 300.00, 30.00, 56.70, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        183, 183, N'OC', N'Incremento en la capacidad de transmisión en la red 115 kV de las zonas León e Irapuato', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 98.30, N'LT León Oriente - San Carlos
115 kV - 1C - 0.95 km-C /1
LT León Oriente - San Carlos
115 kV - 1C - 5.15 km-C /2
SE León Oriente
1 Alimentador 115 kV
SE San Carlos
1 Alimentador 115 kV
SE Maniobras Michelín MVAr
1 Capacitor - 15 MVAr - 115 kV

1/ Tendido del primer circuito
2/ Tendido del segundo circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P21-OC7', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 6.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        184, 184, N'OC', N'Aumento de capacidad de transformación y transmisión entre las zonas Tepic y Vallarta', N'En proceso de definición', 2021, N'Instruido y CON priorización', 2123.20, N'LT Cerro Blanco - Vallejo
400 kV - 1C - 90 km-C 1/
LT Vallejo Entronque Vallarta Potencia - Nuevo Vallarta
230 kV - 2C - 40 km-C
SE Vallejo Banco 1
4 AT - 1F - 125 MVA - 500 MVA - 400/230 kV
3 Alimentadores (1 ampliación y 2 nuevos)
SE Vallejo MVAr
4 R Barra - 1F - 16.66 MVAr - 66.6 MVAr - 400 kV
1 Alimentador 400 kV Reactor

/1 Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P21-OC8', N'En análisis por priorizar', NULL, NULL, N'Sí', 500.00, 66.70, 130.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        185, 185, N'NO', N'Eliminar restricciones de capacidad de transmisión cables subterráneos en 115 kV de la SE Ruiz Cortines', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 15.90, N'LT Los Mochis Ind. - Ruiz Cortínez
115 kV - 1C - 1.0 km-C 1/

1/ Sustitución de tramo con cable de potencia subterráneo por tramo aéreo', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 1.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        186, 186, N'NO', N'Eliminar restricciones de capacidad de transmisión cables subterráneos en 115 kV de la SE Mazatlán Tecnológico', N'En proceso de definición', 2021, N'Instruido y CON priorización', 100.47, N'LT Mazatlán Tecnológico Entronque Mazatlán Dos - Mazatlán Norte 
115 kV - 2C - 6.62 km-C /1,2,3

1/ Sustitución de tramo con cable de potencia subterráneo a disposición aérea
2/ Incluye adecuaciones a módulos híbridos existentes por cambio de acometida subterránea a aérea
3/ Antes: Mazatlán Dos - Mazatlán Tecnológico', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'P21-NO4', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 7.26, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        187, 187, N'NE', N'Incremento de capacidad de transmisión en la red de 115 kV de la Zona Victoria', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 235.60, N'LT Güemez - Olivo
115 kV - 1C - 13.5 km-C /1
LT Güemez - Libertad
115 kV - 1C - 20 km-C /1

1/ Recalibración', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-NE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 33.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        188, 188, N'PE', N'Incremento de capacidad de transformación con relación de transformación 230/115/69 kV en la zona Campeche', N'En proceso de definición', 2021, N'Instruido y CON priorización', 174.60, N'SE Lerma Banco 9
3 AT - 1F - 75 MVA - 225 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P21-PE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        189, 189, N'BC', N'Incremento de capacidad de transformación con relación de transformación230/115/69 kV en la zona Tijuana', N'PIDIREGAS PPEF 2026', 2021, N'En Proceso de Decisión', 323.85, N'SE Metrópoli Potencia Banco 5
4 T - 1F - 75 MVA - 300 MVA - 230/115/69 kV /1

1/ Operación inicial en 230/69 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'P21-BC1', N'En Concurso y Por Concursar', N'2026.4', N'10.1', N'Sí', 300.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        190, 190, N'BC', N'Compensación capacitiva en la zona Tecate', N'Fibra E', 2021, N'Por concursar', 75.46, N'SE Tecate Uno
1 Capacitor - 30 MVAr - 115 kV /2
1 Alimentador 115 kV Capacitor

2/ Operación inicial 69 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
Se realizó una propuesta de fechas para su concurso:
24.OCT.25 Solicitud de Publicación
29.OCT.25 Publicación
19.DIC.25 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P21-BC2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        191, 191, N'CE', N'Moderización parcial del CEV Nopala (+300/-90 MVAr): Controlador, Protecciones, Válvula de Tiristores y Sistema de Enfriamiento', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 195.90, N'SE Nopala
Control, Protección, Válvulas de Tiristores y Sistema de Enfriamiento - 400 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M21-CE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        192, 192, N'VM', N'Adecuación de Subestaciones Eléctricas Hidalgo y Cubitos', N'En proceso de definición', 2021, N'Instruido y CON priorización', 154.50, N'LT Hidalgo Entronque Apasco - Pachuca
85 kV - 2C - 1.1 km-C /12
SE Hidalgo
1 Interruptor Encapsulado en SF6 85 kV
SE Cubitos
1 Interruptor Encapsulado en SF6 85 kV

12/ Circuito o tramo con cable subterráneo', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-CE2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 1.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        193, 193, N'CE', N'Modernización de la Línea de Transmisión Tecamachalco - 73690 - Tlacotepec en 115 kV.', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 13.70, N'LT Tecamachalco - Tlacotepec
115 kV - 1C - 33.7 km-C /1

1/ Recalibración', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M21-OR4', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 33.70, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        194, 194, N'OR', N'Modernización integral de la Subestación Eléctrica Juile y participación de barras de 400 kV', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 120.50, N'SE Juile
Partición de barras de 400 kV
2 Interruptores de Potencia 400 kV
4 Cuchillas desconectadores 400 kV
Modernización de Equipos PCyM
Modernización del sistema SCADA
Ampliación caseta de control 
Instalación de equipo', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        195, 195, N'OR', N'Modernización de Cuchillas, Equipo PCyM y SCADA de la Subestación Eléctrica Tres Estrellas', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 75.50, N'SE Tres Estrellas
55 Cuchillas desconectadoras 400 kV (sustitución)
Modernización de Esquemas PCyM
Modernización SCADA
5 Casetas Control Distribuido', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M21-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        196, 196, N'NO', N'Modernización de la SE Sahuaro para adición de nueva bahía en 115 kV', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 27.70, N'SE Sahuaro
4 Tableros Integral PCyM
2 Postes Troncocónicos remate para 2 Circuitos 2/
115 kV - 2C
1 Interruptor de Potencia 115 kV
8 Cuchillas desconectadoras 115 kV
6 Transformadores de Corriente 115 kV
12 Transformadores de Potencial 115 kV
6 Apartarrayos 115 kV
12 Aisladores tipo pedestal 115 kV
Obra Civil
Obras Electromecánica
2/ Vestimiento del posto troncocónico', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        197, 197, N'NO', N'Normalizar las derivaciones en la Línea de Transmisión Bácum - 73450 - Maniobras Bluemex que suministra las SE Valle del Yaqui y SE Vícam', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 43.30, N'Obras de Modernización:
SE Valle del Yaqui y SE Vicam
6 Tableros Integral PCyM
LT Bácum - Derivación TAP Valle del Yaqui /1,2
115 kV - 2C - 5 km-C
LT Vicam Entronque Bácum - Maniobras Bluemex
115 kV - 1C - 0.5 km-C
SE Bácum
1 Interruptor de Potencia 115 kV
4 Cuchillas desconectadoras 115 kV
3 Transformadores de Corriente 115 kV
3 Transformadores de Potencial 115 kV
3 Apartarrayos 115 kV
12 Aisladores tipo pedestal 115 kV
Obra Civil
Obra Electromecánica
SE Vícam
2 Interruptores de Potencia 115 kV
6 Cuchillas desconectadoras 115 kV
6 Transformadores de Potencial 115 kV
6 Apartarrayos 115 kV
6 Aisladores tipo pedestal 115 kV
Obra Civil
Obra Electromecánica

1/ Tendido del segundo circuito
2/  Considera 3 postes troncocónicos y una estructura de madera o concreto 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 5.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        198, 198, N'NO', N'Normalizar la derivación en la Línea de Transmisión Culiacán Poniente - 73J30 - La Higuera que suministra la SE Navolato', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 230.60, N'LT Navolato Entronque Villa Ángel Flores - La Higuera
115 kV - 2C - 36 km-C
SE Navolato
8 Tableros Integral PCyM
SE del Proyecto
3 Interruptores de Potencia 115 kV
8 Cuchillas desconectadoras 115 kV
9 Transformadores de Corriente 115 kV
9 Transformadores de Potencial 115 kV
6 Apartarrayos 115 kV
16 Aisladores tipo pedestal 115 kV
Obra Civil
Obra Electromecánica', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 36.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        199, 199, N'NT', N'Repotenciación de la Línea de Transmisión Cuauhtémoc - 73840 - Maniobras Treinta y Cuatro', N'En proceso de definición', 2021, N'Instruido y CON priorización', 83.80, N'LT Cuauhtémoc - Maniobras 34
115 kV - 1C - 8.33 km-C /1
SE Cuauhtémoc
Recalibración Bus y puentes 115 kV
3 Cuchillas desconectadoras 115 kV

1/ Recalibración por conductor de alta temperatura', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-NT1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 8.33, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        200, 200, N'NE', N'Cambio de arreglo de la Subestación Eléctrica Villa de García en 115 kV  y modernización de tablero PCyM', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 164.70, N'SE Villa de García
Construcción Arreglo B1 - B2 - BT 115 kV
Construcción Plataformas para nuevas bahías 115 kV
Estructura Mayor
Caseta Integral
Reubicación llegadas de Líneas de Transmisión
Obra Civil y Electromecánica', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-NE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        201, 201, N'BC', N'Modernización de arreglo de barras en la SE Santa Rosalía en 115 kV', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 20.20, N'SE Santa Rosalía
Cambio de Arreglo de Barra
Barra Simple a Barra Principal y Barra Auxiliar
1 Interruptor de Potencia 115 kV
9 Cuchillas desconectadoras 115 kV
6 Transformadores de Corriente 115 kV
9 Transformadores de Potencial 115 kV
Obra Electromecánica
Obra Civil
Puesta en Servicio', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-MU1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        202, 202, N'CE
VM', N'Sustitución de equipamiento en la GCR Central que han sido rebasado en su capacidad de corto circuito', N'Fibra E', 2021, N'En Proceso de Decisión', 998.68, N'SE Tula
34 Piezas Interruptores de Potencia 230 kV
68 Juegos Cuchillas desconectadoras 230 kV
126 Piezas Transformadores de Corriente 230 kV
4 Piezas de Trampas de Onda 230 kV
 
SE Teotihuacán
3  Piezas Interruptores de Potencia 400 kV
19 Juegos Cuchillas desconectadoras 400 kV
42 Piezas Transformadores de Corriente 400 kV
8 Piezas de Trampas de Onda 400 kV
4 Juegos Cuchillas desconectadoras 230 kV
 
SE Texcoco
15  Piezas Interruptores de Potencia 400 kV
48 Juegos Cuchillas desconectadoras 400 kV
42 Piezas Transformadores de Corriente 400 kV
14 Piezas de Trampas de Onda 400 kV
49 Juegos de Cuchillas 230 kV
2 Piezas de Trampas de Onda 230 kV
37 Piezas Transformadores de Corriente de 230 kV
 
SE Remedios
21 Piezas Interruptores de Potencia 230 kV
55 Juegos Cuchillas desconectadoras 230 kV
72 Piezas Transformadores de Corriente 230 kV
Recalibración Buses (1 y 2) 230 kV - 1020 ACCC
 
Totales
73 Piezas Interruptores de Potencia
243 Juegos de Cuchillas desconectadoras
319 Piezas Transformadores de Corriente
28 Piezas de Trampas de Onda', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio y Plan de Ejecución autorizados.
En revisión y autorización del Plan Financiero.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M21-CE3', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        203, 203, N'BC', N'Modernización de interruptores en el ámbito de la Gerencia de Control Regional Baja California', N'Pendiente de definir', 2021, N'Instruido y SIN priorización', 116.90, N'SE Centro
1 Interruptor de Potencia 161 kV
SE Lago
3 Interruptores de Potencia 115 kV
SE La Mesa /1,2
1 Interruptor de Potencia 115 kV
SE Metrópoli Potencia /1
11 Interruptores de Potencia 115 kV
SE Panamericana Potencia /1
10 Interruptores de Potencia 115 kV
SE Río /1,2
1 Interruptor de Potencia 115 kV
SE Río Nuevo
1 Interruptor de Potencia 115 kV
SE Rubí /1
8 Interruptores de Potencia 115 kV
SE Tijuana I /1
17 Interruptores de Potencia 115 kV

1/ Operación inicial 69 kV
2/ Interruptor proveniente de otra Subestación Eléctrica', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M21-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        204, 204, N'BC', N'Incremento en la compensación capacitiva zona Los Cabos', N'En proceso de definición', 2021, N'Instruido y CON priorización', 34.80, N'SE Cabo San Lucas Dos MVAr  /1,2
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Cabo Bello MVAr /1,3
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE José del Cabo MVAr /1,4
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor

1/ Modernización y Aumento de Capacidad 
2/ Actual Banco de Capacitores de 15 MVAr degradado a 10 MVAr
3/  Actual Banco de Capacitores de 15 MVAr degradado a 9 MVAr
4/ Actual Banco de Capacitores de 15 MVAr degradado a 6 MVAr', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P21-BS1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 67.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        205, 205, N'SE', N'363 SLT Suministro de energía eléctrica en la Zona Los Ríos', N'PIDIREGAS 2023', 2022, N'Por concursar', 268.50, N'LT Los Ríos Entronque Macuspana II - 93810 - Santa Lucía
230 kV - 2C - 0.2 km-C
SE Los Ríos
3 AT - 1F - 33. 33 MVA - 100 MVA - 230/115 kV
SE Lacanjá MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'El proyecto fue reevaluado en 2024.
Se requirieron largos periodos de revisiones, validación y liberación de documentos principalmente para actualización de ICM.
Se presentaron atrasos en las actividades y estudios previos por parte de CPTT por lo que el proyecto fue reprogramado para publicarse en 2024.
Debido al cambio de variables y actualización de precios, se requiere la solicitud de incrementar el techo de inversión financiada del proyecto para su viabilidad.', N'Se realizan las gestiones de actualización solicitud de incrementar el techo de inversión financiada para el proyecto, lo cual se podrá ver reflejado en agosto de 2025 y publicado en el PEF 2026. 
Se conforma el paquete de concurso para solicitar su publicaciónen el 3er trimestre de 2025.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
17.OCT.25 Solicitud de Publicación
23.OCT.25 Publicación
11.FEB.26 Fallo', N'Portafolio Activo', N'P22-OR1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 100.00, 7.50, 0.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        206, 206, N'OC', N'360 SE Atención al suministro en la Zona Vallarta', N'PIDIREGAS 2023', 2022, N'En concurso', 221.58, N'LT Vallarta I – 73610 – Vallarta Potencia
Cambio de TC''s de 115 kV en ambos extremos de la línea
LT Pitillal – 73630 – Vallarta Potencia
Cambio de TC''s de 115 kV en ambos extremos de la línea
SE Vallarta Potencia
Cambio de TC''s de 115 kV con una RTC = 800/5 A
SE San Vicente
Cambio de TC''s de 115 kV con una RTC = 1000/5 A
LT Compostela – 73870- Las Varas
Cambio de estado operativo de Normalmente Abierto a Normalmente Cerrado
SE Nuevo Vallarta
3 AT - 1F - 75 MVA - 225 MVA - 230/115 kV
SE Vallarta Uno
Recalibración del Bus de 115 kV
SE Pitillal
Recalibración del Bus de 115 kV', NULL, NULL, NULL, NULL, N'Se requirieron largos periodos de revisiones, validación y liberación de documentos principalmente para actualización de ICM.
Se presentaron atrasos en las actividades y estudios previos por parte de CPTT por lo que el proyecto fue reprogramado para publicarse en 2024.
Se tuvo un periodo de espera para la autorización de presupuesto por parte de la SHCP.', N'Se realizaron las gestiones necesarias para la actualización de ICM, autorización presupuestal e integración de paquete de concurso para solicitar su publicación en el 3er trimestre de 2025.', N'El proyecto está en concurso con el procedimiento No. CFE-0004-CACOA-0005-2025.
24.JUL.25 Publicación
21.OCT.25 Fallo', N'Portafolio Activo', N'P22-OC1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        207, 207, N'NT', N'361 SE Paso del Norte Banco 2', N'PIDIREGAS 2023', 2022, N'Por concursar', 340.23, N'SE Paso del Norte Banco 2
3 AT - 1F - 100 MVA - 300 MVA - 230/115 kV
SE Paso del Norte MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Reforma
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Cuatro Siglos
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Libertad
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Médanos
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Se requirieron largos periodos de revisiones, validación y liberación de documentos principalmente para actualización de ICM.
Se presentaron atrasos en las actividades y estudios previos por parte de CPTT por lo que el proyecto fue reprogramado para publicarse en 2024.
Se tuvo un periodo de espera para la autorización de presupuesto por parte de la SHCP.', N'Se realiza la actualización de ICM, autorización presupuestal e integración de paquete de concurso para solicitar su publicación en el 3er trimestre de 2025.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
17.OCT.25 Solicitud de Publicación
23.OCT.25 Publicación
09.ENE.26 Fallo', N'Portafolio Activo', N'P22-NT1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 300.00, 105.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        208, 208, N'NE', N'362 SE Refuerzo de la Red de la Zona Piedras Negras', N'PIDIREGAS 2023', 2022, N'En concurso', 217.30, N'LT Piedras Negras Potencia - 83130 - Acuña
Cambio de TC''s de 138 kV en ambos extremos de la línea
SE Los Novillos
3 AT - 1F - 75 MVA - 225 MVA - 230/138 kV', NULL, NULL, NULL, NULL, N'Se requirieron largos periodos de revisiones, validación y liberación de documentos principalmente para actualización de ICM.
Se presentaron atrasos en las actividades y estudios previos por parte de CPTT por lo que el proyecto fue reprogramado para publicarse en 2024.
Se tuvo un periodo de espera para la autorización de presupuesto por parte de la SHCP.', N'Se realiza la actualización de ICM, autorización presupuestal e integración de paquete de concurso para solicitar su publicación en el 3er trimestre de 2025.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
14.OCT.25 Solicitud de Publicación
21.OCT.25 Publicación
08.ENE.26 Fallo', N'Portafolio Activo', N'P22-NE1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        209, 209, N'SE', N'Compensación capacitiva en la zona Villahermosa', N'Pendiente de definir', 2022, N'Instruido y SIN priorización', 34.75, N'SE Frontera MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tabasquillo MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P22-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 15.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        210, 210, N'CE', N'Compensación capacitiva en el Suroriente de Puebla', N'Fibra E', 2022, N'En Proceso de Decisión', 45.39, N'SE Coapan
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Zinacantepec
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tehuacán (sustitución)
3 Transformadores de Corriente 115 kV LT Tehuacán - 73280 - Coapan /1
 
1/ Alcanzar un límite operativo mínimo 120 MVA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-OR3', N'En Concurso y Por Concursar', N'2026.1', N'2.1', N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        211, 211, N'OC', N'Atención al suministro en la zona Irapuato', N'Fibra E', 2022, N'En Proceso de Decisión', 1418.30, N'LT Abasolo II Entronque Abasolo - 73610 - Pueblo Nuevo
115 kV - 2C - 0.2 km-C 477 ACSR
SE Las Fresas Banco 2
3 AT - 1F - 125 MVA - 375 MVA - 400/115 kV
Recalibración de Bus 115 kV
SE Abasolo II
1 Alimentador 115 kV SE Abasolo
1 Alimentador 115 kV SE Pueblo Nuevo.
SE Silao Potencia
3 Interruptores de Potencia 230 kV
1 Cuchilla desconetadora 230 kV
SE Irapuato I
3 Cuchillas desconectadoras 115 kV
SE Irapuato II
8 Interruptores de Potencia 115 kV
23 Cuchillas desconectadoras 115 kV
4 Cuchillas desconectadoras 230 kV
Recalibración de Bus 115 kV.', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
En proceso de revisión, validación y aprobación de metas físicas, importes y periodo de construcción en la Ficha de Información del Proyecto entre CENACE y Transmisión.
Pendiente la elaboración del Caso de Negocio, Plan Financiero y Plan de Ejecución.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-OC2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 375.00, 0.00, 36.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        212, 212, N'OC', N'Compensación reactiva para la zona Santiago', N'Fibra E', 2022, N'En Proceso de Decisión', 58.92, N'SE Peñitas MVAr (sustitución)
1 Capacitor - 115 kV - 15 MVAr
(Capacitor actual de 7.5 MVAr)
1 Alimentador 115 kV Capacitor
SE Acaponeta MVAr (sustitución)
1 Capacitor - 115kV - 12 MVAr
(Capacitor actual de 5.0 MVAr)
1 Alimentador 115 kV Capacitor
SE Santiago
3 Transformadores de Corriente LT Peñitas - 73850 - Santiago /1,3
Recalibración Bus 115 kV /4
SE Tepic II (sustitución)
3 Transformadores de Corriente LT Tepic II - 73750 - Santiago /2,3
Recalibración Bus 115 kV /5
 
1/ RTC 600/5 Alcanzar un límite operativo 120 MVA
2/ RTC 800/5 Alcanzar un límite operativo 159 MVA
3/ Sustitución Transformadores de Corriente
4/ Calibre mínimo 500 MCM CU
4/ Calibre mínimo 400 MCM CU', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-OC3', N'En Concurso y Por Concursar', N'2026.1', N'2.2', N'Sí', 0.00, 27.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        213, 213, N'NO', N'Incremento en la capacidad de Transformación entre la zona Guasave y Los Mochis', N'PIDIREGAS PPEF 2026', 2022, N'En Proceso de Decisión', 995.69, N'LT Caimanero Entronque Los Mochis Dos - Guamúchil Dos
230 kV - 2C - 1.0 km-C
LT Caimanero Entronque Guasave - Hernando de Villafañe ( futuro entronque a SE Santa María)
115 kV - 2C - 55.6 km-C
LT Caimanero - Bamoa
115 kV - 1C - 5.7 km-C
SE Caimanero Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
 (aledaña a la futura SE Naranjo)
SE Caimanero
1 Alimentador Línea Nuevo 230 kV (hacia LMD) 
1 Alimentador Línea Nuevo 230 kV (hacia LMD) 
1 Alimentador Amarre de Barras 230 kV (interruptor de amarre)
1 Alimentador Línea Nuevo 115 kV (hacia HVF futura STM)
1 Alimentador Línea Nuevo 115 kV (hacia GSV)
1 Alimentador Línea Nuevo 115 kV (hacia BMO)
1 Alimentador Amarre de Barras 115 kV (interruptor de amarre) /1
SE Bamoa
1 Alimentador Línea Ampliación 115 kV (hacia Caimanero) /2
1 Alimentador Línea Ampliación 115 kV (hacia SRA) /2
1 Interruptor de Transferencia 115 kV /1
SE San Rafael
1 Alimentador Línea Ampliación 115 kV (hacia BMO) /3,4

1/ El interruptor se considera dentro de los Alimentadores en dicha Subestación Eéctrica
2/ Modernización SE Bamoa, incluye normalización del arreglo de Barras BP-BT, incluye interruptor de transferencia.
3/ Se elimina la derivación de la LT hacia Bamoa y se Normaiza el alimentador en la SE SRA.
4/ Obra considerada originalmente en el proyecto M20-NO1', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P22-NO1', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 300.00, 0.00, 68.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        214, 214, N'NO', N'Compensación capacitiva al poniente de la ciudad de Culiacán', N'Pendiente de definir', 2022, N'Instruido y SIN priorización', 16.46, N'SE Navolato
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P22-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        215, 215, N'NO', N'Compensación capacitiva en el corredor de 115 kV entre las zonas Hermosillo y Santa Ana', N'En proceso de definición', 2022, N'Instruido y CON priorización', 15.92, N'SE Oasis
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P22-NO3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 22.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        216, 216, N'NT', N'Soporte de Tensión en la zona Chihuahua', N'Fibra E', 2022, N'En Proceso de Decisión', 184.63, N'SE Chihuahua Norte
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Disión del Norte
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Ávalos
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Chuviscar
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Chihuahua Planta
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Menonitas
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Campo Setenta y Tres
1 Capacitor - 10 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Maniobras Treinta y Cuatro
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Manitoba
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-NT2', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 0.00, 205.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        217, 217, N'NT', N'Soporte de Tensión en la zona Camargo', N'Fibra E', 2022, N'En Proceso de Decisión', 118.42, N'SE Jiménez
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Abraham González
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Puerto Justo
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Santa María del Oro
1 Capacitor - 10 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Río Florido
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor 
SE Bolívar
1 Capacitor - 10 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-NT3', N'En Concurso y Por Concursar', N'2026.1', N'2.2', N'Sí', 0.00, 80.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        218, 218, N'BC', N'Compensación capacitiva en la red de 69 kV, de la zona Tijuana', N'Fibra E', 2022, N'En Proceso de Decisión', 82.21, N'SE Tijuana I MVAr
1 Capacitor - 24.3 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
SE Francisco Villa MVAr
1 Capacitor - 24.3 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
SE Lago MVAr
1 Capacitor - 24.3 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
SE Seminario MVAr
1 Capacitor - 16.2 MVAr - 69 kV
1 Alimentador 69 kV Capacitor
SE Durazno MVAr
1 Capacitor - 16.2 MVAr - 69 kV
1 Alimentador 69 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P22-BC1', N'En Concurso y Por Concursar', N'2026.1', N'3.2', N'Sí', 0.00, 105.30, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        219, 219, N'BC', N'Compensación capacitiva en la red de 115 kV, de la zona Tijuana', N'Fibra E', 2022, N'Por concursar', 57.80, N'SE Panamericana MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE La Joya MVAr
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Popotla MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
El proyecto está en revaluación motivado a que la ICM superó el importe del Caso de Negocio.
Se cuenta con FIP y en proceso la revaluación del Caso de Negocio, Plan Financiero y Plan de Ejecución.
Se realizó una propuesta de fechas para su concurso:
28.NOV.25 Solicitud de Publicación
03.DIC.25 Publicación
29.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P22-BC2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 0.00, 52.50, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        220, 220, N'NO', N'Solución a la problemática de suministro de la carga en la SE Piaxtla', N'En proceso de definición', 2022, N'Instruido y CON priorización', 164.46, N'LT Piaxtla Entronque Subestación Culiacán Potencia - El Habal 
230 kV - 2C - 2.0 km-C
SE Piaxtla Banco 3 (traslado)
4 AT - 1F - 33.33 MVA - 133.33 MVA - 230/115 kV
SE Piaxtla
2 Alimentadores (Nuevo) 230 kV
1 Alimentador (Amarre) 230 kV
1 Alimentador (Ampliación) 115 kV
1 Alimentador (Amarre) 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'M22-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 133.33, 0.00, 2.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        221, 221, N'BC', N'Modernización de la transformación en la SE Tijuana 1 (sustitución de AT)', N'PIDIREGAS PPEF 2026', 2022, N'En Proceso de Decisión', 263.79, N'SE Tijuana I Banco 1 (sustitución) /1,2,
3 AT - 1F - 75 MVA - 225 MVA - 230/115-69 kV /1
SE Tijuana I /3
1 Alimentador Ampliación 230 kV

1/ Operación inicial 230/69 kV
2/ Se realizará la obra del proyecto colindante a los equipos de transformación de 225 MVA de capacidad instalados (T70) derivado de implicaciones constructivas y operativas, posteriormente serán retirados los equipos de AT10 con obsolescencia y serán enviado al almacén de la ciudad de Mexicali. 
3/ Normalizar conexión de T40, Considera adecuaciones en la SE', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'M22-BC1', N'En Concurso y Por Concursar', N'2026.3', N'9.2', N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        222, 222, N'SE', N'Suministro de energía en Tabasco', N'PIDIREGAS PPEF 2026', 2023, N'En Proceso de Decisión', 998.44, N'LT Pichucalco Entronque Reforma - Mezcalapa (73R10)
115 kV - 2C - 60.0 km-C
LT Teapa Tabasco Entronque Kilometro Veinte - Tacotalpa (73460)
115kV - 1C - 3.0 km-C
SE Malpaso
4 AT - 1F - 75 MVA - 300 MVA - 400/115 kV
SE Simojovel MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Teapa Tabasco
2 Alimentadores Ampliación 115 kV
SE Pichucalco
3 Alimentadores Ampliación 115 kV
SE Simojovel
1 Alimentador Ampliación 115 kV
SE Malpaso
3 Transformadores de Corriente 115 kV
SE Mezcalapa
3 Transformadores de Corriente 115 kV
SE Peñitas
3 Transformadores de Corriente 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P23-OR1', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 7.50, 69.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        223, 223, N'OC', N'Compensación Reactiva en la red de 400 kV Gerencia Occidental', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 750.80, N'SE Manzanillo MVAr
4 R Barra - 1F - 25 MVAr - 100 MVAr - 400 kV
1 Alimentador 400 kV Reactor
SE Tapeixtles MVAr
4 R Barra - 1F - 25 MVAr - 100 MVAr - 400 kV
1 Alimentador 400 kV Reactor
SE Mazamitla MVAr
4 R Barra - 1F - 16.66 MVAr - 66.66 MVAr - 400 kV
1 Alimentador 400 kV Reactor', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P23-OC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 266.64, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        224, 224, N'OC', N'Incremento en la Transformación de la Zona Colima', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 224.90, N'SE Colomo Banco 3
1 AT - 3F - 100 MVA - 230/115 kV
SE Colima II Banco 3
1 AT - 3F - 100 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P23-OC2', N'En análisis por priorizar', NULL, NULL, N'Sí', 200.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        225, 225, N'NO', N'Eliminar restricción de capacidad de transmisión LT Marina - Venadillo y LT Marina - Mazatlán Norte', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 4.35, N'LT Culiacán Poniente - Santa Fe
115 kV - 2C - 0.8 km-C /1

1/ Considerar una nueva LT en forma aérea', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P23-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        226, 226, N'NO', N'Eliminar restricción de capacidad de transmisión LT Culiacán Poniente - Tres Ríos', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 54.34, N'LT Marina Entronque Venadillo - Mazatlán Norte 
115 kV - 1C - 9.1 km-C /1
LT Tres Ríos - Santa Fe
115 kV - 1C - 2.0 km-C /1

1/ Recalibrar tramo 477 ACSR a 795 ACSR', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P23-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 11.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        227, 227, N'NT
NE', N'Red de Transmisión para la integración de la generación Noroeste - Norte', N'PIDIREGAS PPEF 2026', 2023, N'En Proceso de Decisión', 24196.60, N'LT Moctezuma – 93420 - El Encino
400 kV - 1C - 0 km-C /1
LT Moctezuma - Chuviscar
230 kV - 1C - 187 km-C  /2
LT El Encino - Hércules Potencia 
400 kV - 2C - 219 km-C /3
LT Hércules Potencia - Río Escondido
400 kV - 1C - 355 km-C /3
LT El Encino II - Entronque Francisco Villa - 93170 - Camargo Dos 
400 kV - 1C - 70 km-C /2
LT Torreón Sur - Derramadero
400 kV - 1C - 238 km-C /2
SE Moctezuma Banco 7
3 AT - 1F - 125 MVA - 375 MVA - 400/230 kV
SE El Encino Banco 1 (sustitución)
4 AT - 1F - 125 MVA - 500 MVA - 400/230 kV
SE El Encino Banco 2 y Banco 3 (sustitución)
6 AT - 1F - 125 MVA - 750 MVA - 400/230 kV
SE Moctezuma
3 R Línea - 1F - 25 MVAr - 75 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
1 Alimentador 400 kV para línea de transmisión
SE El Encino
3 R Línea - 1F - 33.33 MVAr - 100 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
2 Alimentadores 400 kV para línea de transmisión
Recalibración del Bus 1 y 2 - 2 Cond/f -1020 ACCC/TW (conductor de alta temperatura)
SE Hércules Potencia
3 R Línea - 1F - 50 MVAr - 150 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
4 R Barra - 1F - 33.33 MVAr - 133.33 MVAr - 400 kV
3 Alimentadores 400 kV para línea de transmisión 
SE Río Escondido
3 R Barra - 1F - 25 MVAr - 75 MVAr - 400 kV
1 Alimentador 400 kV Reactor
1 Alimentador 400 kV  para línea de transmisión 
SE Torreón Sur
4 R Línea - 1F - 33.33 MVAr - 133.33 MVAr - 400 kV
(Incluye reactor de neutro)
1 Alimentador 400 kV Reactor
1 Alimentador 400 kV para línea de transmisión 
SE Derramadero
1 Alimentador 400 kV para línea de transmisión 
SE Encino Dos
1 Alimentador 230 kV para línea de transmisión 
SE Chuvíscar
1 Alimentador 230 kV para línea de transmisión

1/ Actual LT, cambio de nivel de tensión de 230 a 400 kV.  Operación inicial 230 kV.
2/ Tendido del primer circuito, dos circuitos de dos conductores por fase de 1113 ACSR/AS kcmil.
3/ Dos circuitos de dos conductores por fase de 1113 ACSR/AS', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'I23-NT1', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 1625.00, 666.66, 1175.90, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        228, 228, N'NT', N'Soporte de Tensión zona La Laguna red de 115 kV', N'PIDIREGAS PPEF 2026', 2023, N'En Proceso de Decisión', 315.59, N'SE San Pedro MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV
SE John Deere MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV
SE Viñedos MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV
SE Bermejillo MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV
SE Parras MVAr (incremento y sustitución)
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P23-NT1', N'En Concurso y Por Concursar', N'2026.4', N'12.1', N'Sí', 0.00, 75.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        229, 229, N'NT', N'Soporte de Tensión zona Durango red de 115 kV', N'PIDIREGAS PPEF 2026', 2023, N'En Proceso de Decisión', 249.74, N'SE Jerónimo Ortiz Martinez MVAr
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV
SE Amado Nervo MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV
SE Vicente Guerrero MVAr (sustitución)
1 Capacitor - 15 MVAr - 115 kV
SE Sombrerete MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'P23-NT2', N'En Concurso y Por Concursar', N'2026.4', N'11.2', N'Sí', 0.00, 75.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        230, 230, N'BC', N'Incremento en la confiabilidad de suministro de la SE Victoria Potencia', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 56.40, N'LT Victoria Potencia Entronque Cerro Prieto Dos - Chapultepec (93470)
230 kV - 2C - 11.0 km-C /1

1/ Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto en REVISIÓN y SIN PRIORIZACIÓN.
El proyectos ha sido priorizado solo por Transmisión quedando pendiente la conciliación de su nivel de prioridad con la Coordinación de Vinculación de la DP y el CENACE.', N'B3: Con priorización sólo por CFET', N'P23-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 11.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        231, 231, N'BC', N'Incremento en la capacidad de transformación en la zona Ensenada', N'Fibra E', 2023, N'En Proceso de Decisión', 177.22, N'SE Lomas Banco 3
3 AT - 1F - 33.33 MVA - 100 MVA - 230/115 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P23-BC2', N'En Concurso y Por Concursar', N'2026.3', N'8.2', N'Sí', 100.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        232, 232, N'BC', N'Incremento en la capacidad de transmisión entre las zonas La Paz - Los Cabos', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 1826.23, N'LT Olas Altas - Turbo gas Los Cabos 
230 kV - 2C - 11.0 km-C /1

1/ Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'P23-BS1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 11.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        233, 233, N'CE', N'Modernización parcial del CEV Pie de la Cuesta (+150/-50 MVAR); Controlador, Protecciones, Válvula de Tiristores y Sistema de Enfriamiento', N'En proceso de definición', 2023, N'Instruido y CON priorización', 204.09, N'SE Pie de la Cuesta
Modernización del Control, Protección, Válvulas de tiristores y Sistemas de Enfriamiento - 230 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M22-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        234, 234, N'OR', N'Modernización de Líneas de Transmisión Subterráneas en la Zona de Carga Veracruz', N'PIDIREGAS PPEF 2026', 2023, N'En Proceso de Decisión', 775.19, N'LT Playa del Norte SF6 - 73730 - Pages /1
115 kV - 1C - 3.83 km-C 
LT Mocambo - 73760 - Sacrificios /1
115 kV - 1C - 2.71 km-C  
LT Sacrificios - 73780 - Veracruz Uno /1
115 kV - 1C - 4.17 km-C 
SE Playa Norte SF6 /2
1 Alimentadores Complemento Módulo 115 kV
SE Veracruz Uno /2
1 Alimentador Ampliación 115 kV
SE Mocambo Uno /2,3
1 Alimentador Modernización 115 kV
SE Sacrificios SF6 /2
1 Alimentador Ampliación 115 kV
SE Pagés SF6
5 Alimentadores 115 kV
Secciones PCyM Nuevas
Sistema SCADA redundante.
2 Distribuidores Ópticos dedicados para la diferencial de línea 87L
Tableros de Servicios Propios de CA y CD.

1/ Reemplazo de cables subterráneos en la zona Veracruz
2/ Construcción de la línea subterránea CU 1600 mm2 - XLP
    1 Sección diferencia de línea 87L.
    1 Sección diferencial de Barras 87B en Subestaciones Colaterales.
    2 Distribuidores Ópticos dedicados para la diferencial de línea 87L
    Modernización de alimentadores y equipos terminales en Subestaciones Colaterales. 
3/ Suministro e instalación de un juego de terminales nuevas.', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-B: Con priorización conciliada CFET, DCPE y CENACE', N'M23-OR1', N'En Concurso y Por Concursar', N'2026.3', N'8.2', N'Sí', 0.00, 0.00, 11.78, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        235, 235, N'BC', N'Modernización de arreglo de barras en la SE Puerto Peñasco en 115 kV', N'Pendiente de definir', 2023, N'Instruido y SIN priorización', 49.23, N'SE Puerto Peñasco
Cambio de arreglo Barra a BP - BA 115 kV
1 Alimentador 115 kV (amarre)
Arreglo 
Barra Principal
LT Puerto Peñasco 73690 La Choya
LT Puerto Peñasco 73630 Seis de Abril
LT Puerto Peñasco 73060 Maniobras Fresnillo
Barra Auxiliar
LT Puerto Peñasco 73A60 Maniobras Fotovoltaica Puerto Peñasco
LT Puerto Peñasco 73070 Sonoyta
LT Puerto Peñasco 73A10 Mar de Cortes
Banco de Capacitores 75020', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto CON priorización parcial conciliada entre Transmisión y la Coordinación de Vinculación de la DP faltando consensuar con CENACE.', N'B2: Solo con priorización parcial entre CFET y DP', N'M23-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        236, 236, N'SE', N'Compensación de potencia reactiva en la zona Tuxtla', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 101.80, N'SE Tonalá MVAr
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Villaflores Dos MVAr
1 Capacitor - 7.5 MVAr - 115  kV
1 Alimentador 115 kV Capacitor
SE Arriaga MVAr
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Tonalá
1 Alimentador 115 kV
SE Villaflores Dos
1 Alimentador 115 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        237, 237, N'OC', N'Aumento de la transformación de la zona Morelia', N'En proceso de definición', 2024, N'Instruido y CON priorización', 478.20, N'LT Pedregal Entronque Morelia Potencia - 73320 - Crisoba 
115 kV - 2C - 8.0 km-C
LT Morelia Potencia - Santiaguito
115 kV - 1C - 10.0 km-C /1
SE Morelia Potencia
1 AT - 3F - 100 MVA - 230/115 kV
SE Pátzcuaro Norte MVAr (traslado)
1 Capacitor - 9 MVAr - 115 kV /2
1 Alimentador 115 kV Capacitor
SE Lagunillas
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Morelia Industrial
1 Capacitor - 30 MVAr - 115 kV
1 Alimentador 115 kV Capacitor

 1/ Circuito existente, solo se considera la puesta en operación
2/ Traslado, Banco existente en la SE Lagunillas', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-OC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 100.00, 61.50, 18.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        238, 238, N'OC', N'Suministro de energía para la red eléctrica del Puerto Interior en la zona Irapuato', N'Fibra E', 2024, N'En Proceso de Decisión', 1127.12, N'LT Puerto Interior Entronque Silao Potencia - 93450 - León IV
230 kV - 2C - 20.0 km-C - 1 Cond/f - 1113 ACSR /1
Reubicación LT Silao Potencia - 73660 - Silao
115 kV - 2C - 3.4 km-C - 1 Cond/f - 795 ACSR /2,3,4
Recalibración LT Silao - 73700 - Las Colinas
115 kV - 1C - 0.42 km-C - 1 Cond/f - Cable de Potencia Subterráneo /4
Recalibración LT Santa Fe II - 73D10 - Silao Potencia
115 kV - 1C - 0.20 km-C - 1 Cond/f - Cable de Potencia Subterráneo /5
Reubicación LT 73D90
115 kV - 1C - 0.2 km-C - 1 Cond/f - Cable de Potencia Subterráneo /6
Reubicación LT 73D20
115 kV - 1C - 0.08 km-C - 1 Cond/f - Cable de Potencia Subterráneo /6
Reubicación LT 73D60
115 kV - 1C - 0.08 km-C - 1 Cond/f - Cable de Potencia Subterráneo /6
SE Puerto Interior Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Puerto Interior
1 Alimentador 230 kV LT Puerto Interior - 93OC0 - Silao Potencia
1 Alimentador 230 kV LT Puerto Interior - 93OC0 - León IV
1 Alimentador 230 kV (Amarre)
1 Bus Auxiliar 115 kV /7
6 Juegos de Cuchillas desconectadoras 115 kV /8
1 Alimentador 115 kV (Amarre)
Recalibración Bus 115 kV /9
 
1/ Incluye la construcción de 1.5 km- C Cable de Potencia Subterráneo
2/ Tendido del primer circuito
3/ Reubicar tramo trayectoria calibre 366 ACSR
4/ Alcanzar límite operativo mínimo 179 MVA
5/ Alcanzar límite operativo mínimo 133 MVA
6/ Cable de Potencia Subterráneo CU XLP 1000 mm2
7/ Construcción nuevo Bus Auxiliar
8/ Completar arreglo BP - BA
9/ Alcanzar límite operativo mínimo 225 MVA', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio y Plan de Ejecución autorizados.
En revisión y autorización del Plan Financiero.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P24-OC2', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 0.00, 26.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        239, 239, N'CE', N'Suministro de energía para la región de Querétaro', N'Fibra E', 2024, N'En Proceso de Decisión', 2701.08, N'LT El Blanco - 730C0- La Esperanza
115 kV - 1C - 2.0 km-C - TA - 795 ACSR - 1 Cond/f /2
LT Aerotech - 730C0 - Vynmsa Querétaro mediante la conexión de la
LT Aeroespacial - 73A60 - Aeroespacial con la
LT Aeroespacial - 73C00 - Vynmsa Querétaro
115 kV - 1C - 0.6 km-C /3
LT EL Blanco Maniobreas - 730C0 - Vynmsa Querétaro
115 kV - 1C - 13.6 km-C - TA - 795 ACSR - 1 Cond/f /4
LT Aeroespacial Entronque El Blanco - 730C0 - Vynmsa QUerétaro
115 kV - 2C - 2.0 km-C /3
LT Toyota Maniobras Entronque Celaya II - 93470 - Queretaro Potencia
230 kV - 2C - 13.0 km-C - TA - 1113 ACSR - 1 Cond/f 
LT Montenegro Entronque Querétaro I - 93100 - Las Delicias
230 kV - 2C - 3.0 km-C - TA - 1113 ACSR - 1 Cond/f 
LT Querétaro Potencia - 73610 - Tejeda 
y
 LT Tejeda - 73840 - Querétaro Maniobras
115 kV - 2C - 0.2 km-C /5,3
LT La Palma Entronque Marqués Oriente - 73140 - Parque innovación
115 kV - 2C - 16.6 km-C - TA - 795 ACSR - 1 Cond/f 
LT Parque Innovación - 73A30 - El Marqués
115 kV - 1C - 0.2 km-C /5,3
SE El Blanco Banco 3
3 T - 1F - 125 MVA - 375 MVA - 400/115 kV
SE Montenegro Banco 2 (Nueva SE)
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE El Blanco MVAr
1 STATCOM - +300/-300 MVAr - 115 kV /6
1 Alimentador 115 kV STATCOM
SE Montenegro MVAr
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE El Blanco
1 Alimentador 115 kV LT El Blanco - 73OC0 - Vynmsa Querétaro
SE Vynmsa Querétaro
1 Alimentador 115 kV LT  Vynmsa Querétaro - 73OC0 - El Blanco
SE Toyota Maniobras
2 Alimentadores 230 kV Entronque LT Celaya II - 93470 - Querétaro Potencia
SE Montenegro
1 Alimentador 115 kV (Amarre) 
1 Alimentador 230 kV (Amarre)
2 Alimentadores 230 kV Entronque LT Querétaro I - 93100 - Las Delicias
1 Alimentador 115 kV Capacitor 45 MVAr 
La Palma
2 Alimentadores 115 kV Entronque LT Marqués Oriente - 73140 - Parque Innovación
SE Querétaro Potencia
3 Transformadores de Corriente 115 kV LT Querétaro Potencia - 73610 - Tejeda
SE Tejeda
3 Transformadores de Corriente 115 kV LT Tejeda - 73610 - Querétaro Potencia
3 Transformadores de Corriente 115 kV LT Tejeda - 73840 - Querétaro Maniobras
SE Querétaro Maniobras
3 Transformadores de Corriente 115 kV LT Querétaro Maniobras - 73840 - Tejeda
1 Juego de Cuchillas desconectadoras 115 kV LT Querétaro Maniobras - 73840 - Tejeda /7
SE La Fragua
3 Transformadores de Corriente 115 kV LT La Fragua - 73960 - Buenavista Norte
1 Juego de Cuchillas desconectadoras 115 kV LT La Fragua - 73960 - Buenavista Norte /7
SE Buenavista Norte
3 Transformadores de Corriente 115 kV LT Buenavista Norte - 73960 - La Fragua
El Marqués
3 Transformadores de Corriente 115 kV LT El Marqués - 73A30 - Parque Innovación
Parque Innovación
3 Transformadores de Corriente 115 kV LT Parque Innovación - 73A30 El Marqués
SE Buenavista
3 Transformadores de Corriente 115 kV LT Buenavista - 73650 - Parque Jurica
Parque Jurica
3 Transformadores de Corriente 115 kV LT Parque Jurica - 73650 - Buenavista
SE Nogales
3 Transformadores de Corriente 115 kV LT Nogales - 73970 - Fragua
SE San José Iturbide
1 Juego de Cuchillas desconectadoras 115 kV LT San José Iturbide - 73160 - Los Nogales /7
SE Nogales
Recalibración Bus 115 kV /8
SE San Idelfonso
1 Juego de Cuchillas desconectadoras 115 kV LT San Idelfonso - 73200 - Maniobras Vesta Park /7

2/ Recalibración y tendido del segundo circuito
3/ Capacidad mínima 179 MVA
4/ Tendido del segundo circuito
/5 Recalibración de CS
/6 La definición del punto de conexión podría ser modificada en ejercicios subsecuentes dado el ritmo de crecimiento de demanda de la zona Querétaro y adiciones de capacidad de generación en la región noreste de México
/7 Alcanzar límite de cargabilidad de conductor de LT
/8 Alcanzar capacidad de 220 MVA', NULL, N'abr-28
a
abr-30', NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
Cuenta con el Caso de Negocio, Plan Financiero y Plan de Ejecución.
Proyecto en condiciones de iniciar su carpeta de concurso.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P24-OC3', N'En Concurso y Por Concursar', N'2026.4', N'12.1', N'Sí', 675.00, 645.00, 56.32, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        240, 240, N'NO', N'Eliminar restricciones en el suministro al norte de la ciudad de Navojoa y Villa Juárez', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 62.81, N'LT Navojoa Norte - Villa Juárez, Tramo 1
115 kV - 1C - 6.5 km-C
LT Navojoa Norte - Navojoa Oriente, Tramo A y Tramo B
115 kV - 1C - 1.6 km-C
SE Navojoa Norte 
1 Alimentador 115 kV (ampliación)', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 8.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        241, 241, N'NO', N'Eliminar restricciones en el suministro al poniente de la ciudad de Navojoa', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 118.67, N'LT Navojoa Centenario entronque Navojoa - Huatabampo
115 kV - 2C - 13.0 km-C
SE Navojoa Centenario
2 Alimentadores 115 kV (ampliación)
SE Navojoa
Modernización de Equipamiento serie TCs', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 13.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        242, 242, N'NO', N'Suministro de energía para la zona Nogales
Fase 1', N'Fibra E', 2024, N'En Proceso de Decisión', 727.43, N'FASE 1
LT Nogales Aeropuerto - Chimeneas (Cambio de tensión) (Circuito existente, solo se considera el cambio de tensión de operación a 230 kV)
230 kV - 2C - 0.0 km-C
LT Nogales Aeropuerto - Chimeneas (Tendido del segundo circuito, Operación inicial en 115 kV)
230 kV - 2C - 18.0 km-C
LT Chimeneas Entronque Punto de Inflexión Nogales Aeropuerto (Tendido del primer circuito, Tendido del segundo circuito, Operación inicial en 115 kV)
230 kV - 2C - 4.0 km-C
LT Nuevo Nogales - Chimeneas (Tendido del primer circuito, Tendido del segundo circuito)
115 kV - 2C - 8.0 km-C
LT Chimeneas Entronque Punto de inflexión nogales Norte (Tendido del primer circuito)
115 kV - 2C - 2.0 km-C
SE Chimeneas Banco 1
4 AT - 1F - 75 MVA - 300 MVA - 230/115 kV
SE Chimeneas
1 Capacitor - 22.5 MVAr - 115 kV
1 Alimentador 115 kV Capacitor
SE Nogales
1 Capacitor - 15 MVAr - 115 kV
1 Alimentador 115 kV Capacitor', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
En proceso de revisión, validación y aprobación de metas físicas, importes y periodo de construcción en la Ficha de Información del Proyecto entre CENACE y Transmisión derivado a la integración del proyecto en una sola fase.
Pendiente la elaboración del Caso de Negocio, Plan Financiero y Plan de Ejecución.
Mediante oficios números SDT-4224/2025, DO-393/2025 y DP/2025/326, se solicitó a la SENER la re instrucción del proyecto en una sola fase para el cumplimiento de los indicadores de rentabilidad económica.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'P24-NO3', N'En Concurso y Por Concursar', N'2027 - 2028', NULL, N'Sí', 300.00, 37.50, 35.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        243, 243, N'NT', N'Incremento de capacidad en la red de transmisión de la zona urbana de Juárez', N'PIDIREGAS PPEF 2026', 2024, N'En Proceso de Decisión', 299.23, N'LT apertura de Paso del Norte - 93320 - Reforma
LT Norte Cereso - 93660- Samalayuca Sur
230 kV - 2C - 12.0 km-C
LT Norte Cereso - 93630 - Terranova /1
230 kV - 1C - 13.0 km-C
SE Norte Cereso
1 Alimentador Ampliación 230 kV
SE Terranova
1 Alimentador Ampliación 230 kV
SE Paso del Norte
1 sustitución TC 230 kV
SE Reforma
3 Transformadores de Corriente 230 kV

1/ Tendido del segundo circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025.
El 29.MAY.2025, Transmisión entregó a la Dirección de Planeación el ACB bajo el esquema de financiamiento PIDIREGAS para revisión de la SENER y posterior autorización e integración al PEF 2026 por parte de la SHCP.', N'B2-A: Con priorización conciliada CFET, DCPE y CENACE', N'P24-NT1', N'En Concurso y Por Concursar', N'2026.4', N'11.2', N'Sí', 0.00, 0.00, 27.50, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        244, 244, N'NT', N'Aumento de la capacidad de subtransmisión en zona Laguna', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 242.17, N'L.T. Torreón Sur Entronque Viñedos - Revolución (73NT0)
115 kV - 2C - 22 km-C - 1 cond/f - 1113 ACSR - AT 
SE Torreón Sur
2 Alimentadores 115 kV (ampliación)', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-NT2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 22.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        245, 245, N'NE', N'Atención al suministro de energía de la zona norte del área Metropolitana de Monterrey', N'Fibra E', 2024, N'Por concursar', 1845.02, N'LT Las Glorias - Real de Palmas
115 kV -1C - 13.9 km-C - 1 cond/f - 1113 kcmil ACSR - TA 
SE Escobedo Banco 3 (sustitución)
3 T - 1F - 75 MVA - 225 MVA - 230/115 kV
SE Las Glorias Banco 2 (Ampliación)
3 T - 1F - 125 MVA - 375 MVA - 400/115 kV
SE Américas MVAr (Ampliación)
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV
SE Sabinas Hidalgo MVAr  (Ampliación)
1 Capacitor - 7.5 MVAr - 115 kV
1 Alimentador 115 kV
SE Parque Industrial Estrella MVAr (sustitución)
1 Capacitor - 45 MVAr - 115 kV
1 Alimentador 115 kV
SE Las Glorias
1 Alimentador 115 kV Interconexión LT Las Glorias - Real de Palmas
SE Real de Palmas
1 Alimentador 115 kV Interconexión LT Real de Palmas - Las Glorias
SE Escobedo (Ampliación)
2 Alimentadores 115 kV (Amarre)
SE Estrella (sustitución)
3 Transformadores de Corriente 115 kV LT Estrella - 73F60 - Escobedo
3 Transformadores de Corriente 115 kV LT Estrella - 73G40 - Escobedo
SE El Canada (sustitución)
3 Transformadores de Corriente 115 kV LT El Canada - 73880 - Escobedo
SE Nuevo Escobedo (sustitución)
3 Transformadores de Corriente LT Nuevo Escobedo - 73H70 - Escobedo
SE Escobedo (sustitución)
3 Transformadores de Corriente LT Escobedo - 73H70 - Nuevo Escobedo
SE Solidaridad (sustitución)
3 Transformadores de Corriente LT Soliradiad - 73860 - Escobedo
SE Villa de García (sustitución)
3 Transformadores de Corriente LT Villa de García - 73F00 - Fomerrey
SE Fomerrey (sustitución)
3 Transformadores de Corriente LT Fomerrey - 73F00 - Villa de García
SE Villa de García (sustitución)
3 Transformadores de Corriente LT Villa de García - 73E90 - Santa Catarina
SE Santa Catarina (sustitución)
3 Transformadores de Corriente LT Villa de García - 73E90 - Santa Catarina
SE Jerónimo Potencia (sustitución)
3 Transformadores de Corriente LT Jerónimo Potencia - 73720 - Valle
3 Transformadores de Corriente LT Jerónimo Potencia - 73330 - Dinastía
3 Transformadores de Corriente LT Jerónimo Potencia - 73D60 - Leona
3 Transformadores de Corriente LT Jerónimo Potencia - 73230 - Lechugal
3 Transformadores de Corriente LT Jerónimo Potencia - 73280 - Valle Poniente
SE Valle (sustitución)
3 Transformadores de Corriente LT Valle - 73720 - Jerónimo Potencia
SE Dinastía (sustitución)
3 Transformadores de Corriente LT Dinastía - 73330 - Jerónimo Potencia
SE San Nicolas (sustitución)
3 Transformadores de Corriente LT San Nicolas - 73E80 - Celulosa
3 Transformadores de Corriente LT San Nicolas - 73890 - Topo Chico
SE Celulosa (sustitución)
3 Transformadores de Corriente LT Celulosa - 73E80 - San Nicolas
SE Topo Chico (sustitución)
3 Transformadores de Corriente LT Topo Chico - 73890 - San Nicolas
SE Leona (sustitución)
3 Transformadores de Corriente LT Leona - 73D 60 - Jerónimo Potencia
SE Lechugal (sustitución)
3 Transformadores de Corriente LT Lechugal - 73230 - Jerónimo Potencia', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Este proyecto forma parte del nuevo portafolio de proyectos 2025 (primer grupo de 09 proyectos).
Se realizó una propuesta de fechas para su concurso:
07.NOV.25 Solicitud de Publicación
12.NOV.25 Publicación
15.ENE.26 Fallo', N'B1-B: Con priorización conciliada CFET, DCPE y CENACE', N'P24-NE1', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 600.00, 97.50, 15.29, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        246, 246, N'BC', N'Incremento en la capacidad de transmisión en la región de San Quintín', N'En proceso de definición', 2024, N'Instruido y CON priorización', 494.02, N'LT Cañon - San Quintín
115 kV - 1C - 70.0 km-C - 1 cond/f - 795 ACSR - AT /1

1/ Tendido del primer circuito', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 70.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        247, 247, N'BC', N'Incremento en la capacidad de suministro en la región de Valle de Guadalupe', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 117.87, N'SE Lomas Banco 4
1 T - 3F - 40 MVA - 115/69 kV
SE Valle de Guadalupe MVAr
1 Capacitor - 16.2 MVAr - 69 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'P24-BC2', N'En análisis por priorizar', NULL, NULL, N'Sí', 40.00, 16.20, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        248, 248, N'CE', N'Incremento de confiabilidad en la Subestación Eléctrica Zocac', N'En proceso de definición', 2024, N'Instruido y CON priorización', 54.36, N'SE Zocac
Recalibración de Barra 230 kV
2 cond/f - 1113 ACSR - Cap 2200 A
Modernización de Barra 115 kV
B1 y B2 - BT', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'M24-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        249, 249, N'BC', N'Incremento en la confiabilidad de suministro en la región de Valle de la Palmas', N'Pendiente de definir', 2024, N'Instruido y SIN priorización', 93.27, N'SE Nueva Denominada El Fortín Maniobras
3 Alimentadores 115 kV /1
1 A - LT Herradura - TAP Alpha De Acero - Valle de las Palmas
1 A - LT Valle de las Palmas - TAP Vallecitos - Valle de Guadalupe
1 A - SE Valle de las Palmas

1/ Operación inicial 69 kV', NULL, NULL, NULL, NULL, N'No aplica, dado que no han iniciado las etapas del proyecto.', N'No aplica, dado que no han iniciado las etapas del proyecto.', N'Proyecto instruido en 2024 y SIN priorización.', N'B4: Sin priorizar', N'M24-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        250, 250, N'OR', N'Modernización de la Subestación Eléctrica Laguna Verde 230 kV (SF6)', N'FIDEICOMISO 10670', 2024, N'En concurso', 1609.02, N'SE Laguna Verde
Modernización de la Subestación Eléctrica Laguna Verde 230 kV (SF6)
Subestación encapsulada en SF6 de 230 kV - 6 Alimentadores 230 kV
2 A LT Veracruz Dos (C1 y C2)
1A LT Futura
1A LAV T4
1A LAV TX
1A LT AT3
4 AT - 1F - 125 MVA - 500 MVA - 400/230/34.5 kV
1 T - 3F - 50 MVA - 230/34.5 kV
Subestación de 34.5 kV con 5 alimentadores de 31.5 kA Bus 31
Subestación de 34.5 kV con 6 alimentadores de 31.5 kA Bus 32
6 Alimentadores 230 kV
11 Alimentadores 34. 5 kV', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'El proyecto está programado para que inicie su procedimiento de concurso con las siguientes fechas estimadas:
02.OCT.25 Solicitud de Publicación
10.OCT.25 Publicación
03.DIC.25 Fallo

* NOTA:
El proyecto fue publicado el 10.OCT.2025 con el procedimiento No. CFE-0004-CACOA-0006-2025', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'M24-OR2', N'En Concurso y Por Concursar', N'2025.4', NULL, N'Sí', 550.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        251, 251, N'SE', N'Atención al suministro de energía eléctrica en las Zonas Villahermosa y Chontalpa', N'En proceso de definición el tipo de financiamiento', 2025, N'Por instruir PAMRNT 2025', 932.89, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-OR1', N'En análisis por priorizar', NULL, NULL, N'Sí', 450.00, 300.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        252, 252, N'SE', N'Incremento de capacidad de suministro hacia la Zona San Cristóbal', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 185.77, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 22.50, 65.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        253, 253, N'SE', N'Suministro de energía en la zona de carga Tehuantepec', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 305.88, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P24-OR2', N'En análisis por priorizar', NULL, NULL, N'Sí', 100.00, 0.00, 5.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        254, 254, N'OC', N'Soporte de tensión para la Zona Matehuala', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 107.90, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-OC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 96.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        255, 255, N'OC', N'Suministro de energía en la región Ciénega - Zamora', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 181.93, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-OC2', N'En análisis por priorizar', NULL, NULL, N'Sí', 100.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        256, 256, N'OC', N'Incremento de transformación en la Zona Tepic', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 520.85, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-OC3', N'En análisis por priorizar', NULL, NULL, N'Sí', 525.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        257, 257, N'NO', N'Incremento en la capacidad de transformación de la Zona Navojoa', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 275.59, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NO1', N'En análisis por priorizar', NULL, NULL, N'Sí', 758.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        258, 258, N'NO', N'Eliminar restricción en la capacidad de transmisión de la LT Culiacán Cuatro - Culiacán Sur', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 11.47, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NO2', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.97, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        259, 259, N'NT', N'Incremento de transformación en la Zona Juárez', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 796.70, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NT1', N'En análisis por priorizar', NULL, NULL, N'Sí', 533.30, 45.00, 5.16, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        260, 260, N'NT', N'Incremento de transformación en la Zona La Laguna', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 278.66, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NT2', N'En análisis por priorizar', NULL, NULL, N'Sí', 100.00, 45.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        261, 261, N'NT', N'Soporte de tensión en la Zona Cuauhtémoc', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 463.79, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NT3', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 305.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        262, 262, N'NE', N'Incremento de Capacidad de Transformación de la zona Monclova', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 201.07, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 100.00, 30.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        263, 263, N'NE', N'Atención al suministro de energía del Sureste de la Zona Metropolitana de Monterrey', N'En proceso de definición el tipo de financiamiento', 2025, N'Por instruir PAMRNT 2025', 1471.72, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-NE2', N'En análisis por priorizar', NULL, NULL, N'Sí', 750.00, 697.50, 1.75, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        264, 264, N'BC', N'Incremento en la capacidad de transformación en la SE Rubí', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 306.09, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-BC1', N'En análisis por priorizar', NULL, NULL, N'Sí', 225.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        265, 265, N'BC', N'Cambio de tensión de operación de 69 kV a 115 kV al oriente de la ciudad de Tijuana y Tecate', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 4801.55, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'P25-BC2', N'En análisis por priorizar', NULL, NULL, N'Sí', 260.00, 30.00, 12.20, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        266, 266, N'VM', N'Modernización integral de la Subestación Eléctrica San Bernabé en 400 kV', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 1131.80, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'M25-CE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        267, 267, N'PE', N'Sustitución de Autotransformador AT7 en SE Valladolid', N'Pendiente de definir', 2025, N'Por instruir PAMRNT 2025', 252.19, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'B5: Sin priorizar', N'M25-PE1', N'En análisis por priorizar', NULL, NULL, N'Sí', 300.00, 0.00, 0.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        268, 268, N'VM', N'Obras de Refuerzo CCC Francisco Pérez Ríos
(antes denominado "Repotenciación de la Línea Remedios – El Vidrio")', N'Fibra E', 2025, N'Por instruir PAMRNT 2025', 72.89, N'LT Remedios - El Vídrio 
230 kV - 1C - 23.3 km-C - 1020 ACCC - TA/PA
Incremento en la capacidad de transmisión con cable de alta temperatura

SE El Vidrio
Sustitución de 6 TC’s de 230 kV​
Recalibración de bus auxiliar de 230 kV​​', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Mediante oficios números SDT-4225/2025, DO-394/2025 y DP/2025/327, se solicitó a la SENER la INSTRUCCIÓN del proyecto para el 2025, debido a la ALTA PRIORIDAD para reforzar la RNT por el incremento en la capacidad de generación.
Se cuenta con la Ficha de Información del Proyecto.
En proceso de validación y aprobación del Caso de Negocio, Plan Financiero y Plan de Ejecución.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'CFE25-TUL', N'En Concurso y Por Concursar', N'2026.1', N'2.2', N'Sí', 0.00, 0.00, 23.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        269, 269, N'CE', N'Obras de Refuerzo CCC Salamanca II
(antes denominado "Sustitución de EEP en la SE Salamanca Cogeneración y Salamanca 230 kV por NCC")', N'Fibra E', 2025, N'Por instruir PAMRNT 2025', 582.20, N'SE Salamanca 
Reemplazo de TC’s en 115 kV para el circuito 73180 y en 230 kV circuitos 93G30 y 93G40 como sustitución de Equipo Eléctrico Primario.
SE Salamanca Cogeneración SF6 (Sustitución)
Reemplazo de TC’s para los circuitos 93G30 y 93G40 y sustitución de Equipo Eléctrico Primario.
SE Salamanca Norte
Reemplazo de TC’s en 115 kV para el circuito 73180.
SE Salamanca Sur
Cambio de Equipo Serie Circuito 73230.
SE Salamanca II
Cambio de Equipo Serie Circuito 73230.
SE Irapuato II
Reemplazo de TC’s en 115 kV para el circuito 73580.
SE Querétaro Potencia
Reemplazo de Equipo Eléctrico Primario en 115 kV.
LT Salamanca-73120-Salamanca Sur 
115 kV - 1C - TA/PC Recalibración de 5.93 km-C a 795 ACSR, incluye 4.00 km de reubicación de LT por invasión (0.5 km subterránea) y 1.93 km-C de recalibración. 
LT Villa de Reyes-93630-Maniobras WTC II 
230 kV - 1C - 24 km-C TA Recalibración a 1020 ACCC.', NULL, NULL, NULL, NULL, N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Sin comentarios.
La etapa de ejecución / construcción aún no ha iniciado.', N'Mediante oficios números SDT-4381/2025, DO-401/2025 y DP/2025/328, se solicitó a la SENER la INSTRUCCIÓN del proyecto para el 2025, debido a la ALTA PRIORIDAD para reforzar la RNT por el incremento en la capacidad de generación.
Se cuenta con la Ficha de Información del Proyecto.
En proceso de validación y aprobación del Caso de Negocio, Plan Financiero y Plan de Ejecución.', N'B1: Con priorización conciliada CFET, DCPE y CENACE', N'CFE25-SLM', N'En Concurso y Por Concursar', N'2026.1', N'3.1', N'Sí', 0.00, 0.00, 5.93, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        270, 270, N'BC', N'Red de Transmisión Asociada al Proyecto CFV Puerto Peñasco Secuencia 3 (100 MW) y 4 (300 MW)', N'FIDEICOMISO 1320', 2025, N'Por instruir PAMRNT 2025', 7474.56, N'Transmisión y Transformación', NULL, NULL, NULL, NULL, NULL, NULL, N'Se energizó el 10.DIC.2023', N'1', NULL, N'En análisis por priorizar', NULL, NULL, N'Sí', 2375.00, 1350.00, 350.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        271, 271, N'NE
OR', N'304 LT 1805 Línea de Transmisión Huasteca-Monterrey', N'PIDIREGAS Legado', 2010, N'Ejecución/Construcción', 3253.31, N'Proyecto PIDIREGAS LEGADO
Alcances: 3 LT''s con 441.8 km-C y 195 MVAr de compensación.
LT Champayán – Güemez (182.5 km)
LT Güemez – Regiomontano (230.7 km)
LT Regiomontano Ent. Huinalá – Lajas 2 (28.6 km)
SE Güemez ampliación, 133 MVAr
SE Champayán ampliación 62 MVAr', NULL, N'N/A', NULL, NULL, NULL, NULL, N'El proyecto lleva 5 convocatorias que no han prosperado.

Datos de los dos últimos procedimientos de concurso:
27.JUL.2023 Se determinó la CANCELACIÓN del Concurso Abierto Internacional No. CFE-0003-CACOA-0003-2023.
04.OCT.2024 Se declaró desierto el Concurso Abierto Internacional No. CFE-0003-CACOA-0007-2024.', NULL, N'Sin PEM', N'Ejecución/Construcción', NULL, NULL, N'Sí', 0.00, 195.00, 441.80, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        272, 272, N'PE', N'Recalibración de RNT 115 kV de la Zona Mérida', N'FIDEICOMISO', 2025, N'Por instruir PAMRNT 2025', 495.23, N'RNT Zona Mérida', NULL, N'N/A', NULL, NULL, NULL, NULL, N'CFE solicitará a la SENER que se instruya el proyecto.', NULL, N'Sin PEM', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 47.30, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        273, 273, N'NO', N'Red de Interconexión Asociada al Proyecto Equipamiento Hidroeléctrico de la Presa Santa María', N'FIDEICOMISO 10670', 2023, N'Por instruir PAMRNT 2025', 219.73, N'Presa Santa María', NULL, N'N/A', NULL, NULL, NULL, NULL, N'CFE solicitará a la SENER que se instruya el proyecto.', NULL, N'Sin PEM', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 25.00, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    INSERT INTO dgmesnie.InformePormenorizadoModernizacion
    (
        Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, Activo
    )
    VALUES
    (
        274, 274, N'SE', N'290 LT Red de Transmisión asociada a la CH Chicoasén II', N'PIDIREGAS Legado', 2012, N'Por instruir PAMRNT 2025', 137.82, N'CH Chicoasán', NULL, N'N/A', NULL, NULL, NULL, NULL, N'CFE solicitará a la SENER que se instruya el proyecto.', NULL, N'Sin PEM', N'En análisis por priorizar', NULL, NULL, N'Sí', 0.00, 0.00, 8.10, N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', 1
    );

    PRINT 'Registros cargados: 274';
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
