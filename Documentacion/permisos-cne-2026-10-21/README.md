# Endpoints de las cinco tablas CNE

Publicado y verificado el 21 de octubre de 2026 en Azure App Service `dmesnie`.
Rama: `claude/improve-law-search-11ipO` del repositorio DGMESNIE.
Contrato de referencia: `JSCEG/buscador_leyes`, commit `18bb91e`, `docs/permisos-cne-2026-10-21/CONTRATO-BACKEND.md`.

Base: https://dmesnie-bkezcpb5erfrgnbp.mexicocentral-01.azurewebsites.net/DashboardProyectos/PermisosEnergeticos/Tablas

- GET `/Lista?tabla=<clave>[&estatus=][&ef=][&tipo=]`
- GET `/Estados?tabla=<clave>`
- GET `/Puntos?tabla=<clave>`
- GET `/Actualizacion?tabla=<clave>`

Las claves transporte, comercializacion, infraestructura, suministro y terceros se resuelven mediante switch a tablas fijas. Terceros consulta exclusivamente Terceros_Acreditados_CNE. Se utiliza Dapper y DefaultConnection con parámetros y SELECT únicamente. Las rutas tienen AllowAnonymous. Las fechas date se devuelven como yyyy-MM-dd y los timestamps como UTC con Z; los NULL se conservan. Los parámetros inválidos devuelven 400 y los fallos de consulta 500 con mensaje público fijo.

## Validación

Compilación Release correcta (0 errores; una advertencia preexistente CS0219 en Views/Home/Index.cshtml).

Pruebas aisladas:

    dotnet run --project tests/cne-contrato-smoke/CneContratoSmoke.csproj -c Release

Pruebas HTTP de solo lectura, ejecutadas en localhost y producción:

    python tests/permisos-cne-tablas.test.py https://dmesnie-bkezcpb5erfrgnbp.mexicocentral-01.azurewebsites.net

| Tabla | Filas Lista | Grupos Estados | Puntos |
|---|---:|---:|---:|
| transporte | 2781 | 33 | 2 |
| comercializacion | 937 | 1 | 0 |
| infraestructura | 215 | 1 | 0 |
| suministro | 86 | 1 | 0 |
| terceros | 134 | 1 | 0 |

Las cuatro rutas devolvieron 200 para las cinco tablas. Se comprobaron propiedades JSON, fechas, filtros parametrizados, vigencia, coordenadas y códigos INEGI. Las cuatro rutas devolvieron 400 para tabla=otra, tabla ausente, tabla maliciosa y tabla duplicada. Se comprobaron además filtros inválidos y una cadena de inyección como valor literal de tipo. Las pruebas aisladas verifican la respuesta 500 sin detalles de conexión y el formato UTC y NULL. El endpoint previo PermisosEnergeticos/Actualizacion conserva HTTP 200.

## Publicación

Sólo se sustituyó NSIE.dll mediante Kudu y se reinició App Service; las dependencias de producción y del build coinciden. No se cambiaron configuración, frontend ni base de datos. Se respaldó el DLL previo en out/cne-tablas-backup/NSIE.dll (salida local ignorada por Git).

SHA-256 publicado y comprobado por descarga:

    44389B5DD44A43B4D0A18C4814701982AFF0F7842BC2A4A63727B4ECBE38D17E

Se ejecutó graphify update . y el grafo local quedó actualizado (20,053 nodos).