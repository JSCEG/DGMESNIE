//    document.addEventListener("DOMContentLoaded", function () {
let combinedData = [];

function iniciarSankey() {
    //Arrastre derl SVG
    let isDragging = false;
    let originalX;

    const svg = document.getElementById("sankeySvg");

    svg.addEventListener("mousedown", function (e) {
        isDragging = true;
        originalX = e.clientX;
    });

    document.addEventListener("mousemove", function (e) {
        if (isDragging) {
            const deltaX = e.clientX - originalX;
            const newTransform = `translate(${deltaX}px, 0px)`;
            svg.style.transform = newTransform;
        }
    });

    document.addEventListener("mouseup", function () {
        isDragging = false;
    });

    // FUNCIÓN CORREGIDA PARA ACTUALIZAR TARJETAS CON EL AÑO CORRECTO
    function actualizarTarjetasConAño(selectedYear) {
        // Mostrar el div de depuración
        $("#debug-totales").show();
        $("#debug-year").text(selectedYear);
        $("#debug-lista").html('<div class="text-info">Cargando datos...</div>');

        let tempCombinedData = [];
        let completedRequests = 0;
        const totalRequests = 5;

        // Variables para cálculos adicionales (como en la función original)
        let tempNombren = [];
        let tempContadorCombus = 0;
        let tempContadorPlant = 0;
        let tempContadorEl = 0;
        let tempDist = 0;
        let tempRnt = 0;

        function checkIfComplete() {
            completedRequests++;
            if (completedRequests === totalRequests) {
                procesarDatosCompletos();
            }
        }

        function procesarDatosCompletos() {
            // Agregar Nivel 1 - Provisión y Producción (calculado)
            if (tempNombren.length > 0) {
                tempNombren.forEach(function (nombre) {
                    let valor = 0;
                    if (nombre === "Combustible") {
                        valor = tempContadorCombus;
                    } else if (nombre === "Calor") {
                        valor = tempContadorPlant;
                    } else if (nombre === "Electricidad") {
                        valor = tempContadorEl;
                    }

                    tempCombinedData.push({
                        año: selectedYear,
                        nivel: "Nivel 1 - Provisión y Producción",
                        dato: nombre,
                        valor: valor,
                    });
                });
            }

            // Agregar Nivel 4 - Distribución (calculado)
            tempCombinedData.push({
                año: selectedYear,
                nivel: "Nivel 4 - Distribución",
                dato: "Distribución",
                valor: tempDist,
            });
            tempCombinedData.push({
                año: selectedYear,
                nivel: "Nivel 4 - Distribución",
                dato: "Centrales Eléctricas",
                valor: tempRnt,
            });

            // Calcular totales por nivel
            const totalPorNivel = {};

            tempCombinedData.forEach(item => {
                if (!totalPorNivel[item.nivel]) {
                    totalPorNivel[item.nivel] = 0;
                }
                totalPorNivel[item.nivel] += parseFloat(item.valor);
            });

            // Actualizar div de depuración con totales calculados
            let debugInfo = `
                <div class="mb-2">
                    <strong>Año seleccionado:</strong> ${selectedYear}
                </div>
                <div class="mb-2">
                    <strong>Total de datos procesados:</strong> ${tempCombinedData.length} registros
                </div>
                <div class="mb-3">
                    <strong>Totales calculados por nivel:</strong>
                </div>
            `;

            for (let nivel in totalPorNivel) {
                debugInfo += `
                    <div class="alert alert-info py-2 mb-2">
                        <strong>${nivel}:</strong> ${totalPorNivel[nivel].toFixed(1)} PJ
                    </div>
                `;
            }

            $("#debug-lista").html(debugInfo);

            // Verificar si hay datos
            const hayDatos = Object.keys(totalPorNivel).length > 0;

            if (!hayDatos) {
                $("#tarjetas-totales").html(`
                    <div class="alert alert-warning w-100 text-center" role="alert">
                        No hay datos disponibles para el año ${selectedYear}.
                    </div>
                `);
                return;
            }

            // Limpiar contenedor y renderizar tarjetas
            $("#tarjetas-totales").html("").removeClass();

            for (let nivel in totalPorNivel) {
                // Determinar el ícono apropiado para cada nivel
                let icono = 'bi-bar-chart-line';
                switch (nivel) {
                    case 'Nivel FEP - Fuentes de Energía Primaria':
                        icono = 'bi-lightning-charge';
                        break;
                    case 'Nivel FOCAL - Sector Energético':
                        icono = 'bi-building';
                        break;
                    case 'Nivel 1 - Provisión y Producción':
                        icono = 'bi-gear';
                        break;
                    case 'Nivel 2 - Transformaciones':
                        icono = 'bi-arrow-repeat';
                        break;
                    case 'Nivel 3 - Tipos de Energía':
                        icono = 'bi-fuel-pump';
                        break;
                    case 'Nivel 4 - Distribución':
                        icono = 'bi-diagram-3';
                        break;
                    case 'Nivel 5 - Uso Final':
                        icono = 'bi-house-gear';
                        break;
                    default:
                        icono = 'bi-bar-chart-line';
                }

                $("#tarjetas-totales").append(`
                    <div class="sankey-card">
                        <div class="card-body">
                            <div class="card-icon">
                                <i class="bi ${icono} fs-4"></i>
                            </div>
                            <h6 class="card-title">${nivel}</h6>
                            <p class="card-value">${totalPorNivel[nivel].toFixed(1)} PJ</p>
                        </div>
                    </div>
                `);
            }

            // Añadir efectos adicionales después de crear las tarjetas
            setTimeout(() => {
                $('.sankey-card').each(function (index) {
                    const $card = $(this);
                    const value = parseFloat($card.find('.card-value').text());

                    // Destacar valores altos con efecto de pulso
                    if (value > 8000) {
                        $card.find('.card-value').addClass('highlight');
                    }

                    // Agregar tooltip con información adicional
                    $card.attr('title', `${$card.find('.card-title').text()}: ${value.toFixed(1)} Petajoules`)
                        .tooltip({
                            placement: 'top',
                            trigger: 'hover'
                        });
                });
            }, 1000);
        }

        // 1. Obtener datos de FEP
        $.ajax({
            url: "/Sankey/NodosTablaFep",
            type: "GET",
            datatype: "json",
            success: function (data1) {
                var filteredData1 = data1.filter(function (item) {
                    return item.año == selectedYear;
                });

                filteredData1.forEach(function (item1) {
                    tempCombinedData.push({
                        año: item1.año,
                        nivel: "Nivel FEP - Fuentes de Energía Primaria",
                        dato: item1.feP_Nombre_sin_espacios,
                        valor: item1.valor,
                    });
                });
                checkIfComplete();
            },
            error: function () { checkIfComplete(); }
        });

        // 2. Obtener datos de Sector (incluye cálculos para Nivel 1)
        $.ajax({
            url: "/Sankey/NodosTablaSector",
            type: "GET",
            datatype: "json",
            success: function (data2) {
                var filteredData2 = data2.filter(function (item) {
                    return item.año == selectedYear;
                });

                // Calcular contadores como en la función original
                let contadorPet = 0;
                let contadorCombus = 0;
                let contadorPlant = 0;
                let contadorEl = 0;

                filteredData2.forEach(function (item2) {
                    tempCombinedData.push({
                        año: item2.año,
                        nivel: "Nivel FOCAL - Sector Energético",
                        dato: item2.sector_Nombre_SE,
                        valor: item2.valor,
                    });

                    // Calcular contadores como en la función original
                    if (item2.tipo_SE === "Sector petróleo y gas") {
                        contadorPet += item2.valor;
                        if (item2.sector_Nombre_SE === "Coquizadoras" || item2.sector_Nombre_SE === "Refinerias y despuntadoras") {
                            contadorCombus += item2.valor;
                        }
                        if (item2.sector_Nombre_SE === "Plantas de gas y fraccionadoras") {
                            contadorPlant += item2.valor;
                        }
                    } else {
                        contadorEl += item2.valor;
                    }
                });

                // Guardar para el cálculo del Nivel 1
                tempNombren = ["Combustible", "Calor", "Electricidad"];
                tempContadorCombus = contadorCombus;
                tempContadorPlant = contadorPlant;
                tempContadorEl = contadorEl;

                checkIfComplete();
            },
            error: function () { checkIfComplete(); }
        });

        // 3. Obtener datos de Transformaciones
        $.ajax({
            url: "/Sankey/NodosTablaTransformacion",
            type: "GET",
            datatype: "json",
            success: function (data3) {
                var filteredData3 = data3.filter(function (item) {
                    return item.año == selectedYear;
                });

                filteredData3.forEach(function (item3) {
                    tempCombinedData.push({
                        año: item3.año,
                        nivel: "Nivel 2 - Transformaciones",
                        dato: item3.transformacion_Nombre_SE,
                        valor: item3.valor,
                    });
                });
                checkIfComplete();
            },
            error: function () { checkIfComplete(); }
        });

        // 4. Obtener datos de Tipos (incluye cálculos para Nivel 4)
        $.ajax({
            url: "/Sankey/NodosTablaTipos",
            type: "GET",
            datatype: "json",
            success: function (data4) {
                var filteredData4 = data4.filter(function (item) {
                    return item.año == selectedYear;
                });

                filteredData4.forEach(function (item4) {
                    // Calcular el total como en la función original
                    let valorTotal = item4.cargaPico + item4.intermitente + item4.cargaBase + item4.gasSeco + item4.gasLP + item4.petrolíferos;

                    tempCombinedData.push({
                        año: item4.año,
                        nivel: "Nivel 3 - Tipos de Energía",
                        dato: "Tipos de Energía",
                        valor: valorTotal,
                    });

                    // Calcular dist y rnt para el Nivel 4
                    tempDist = item4.petrolíferos + item4.gasSeco + item4.gasLP;
                    tempRnt = item4.cargaPico + item4.cargaBase + item4.intermitente;
                });
                checkIfComplete();
            },
            error: function () { checkIfComplete(); }
        });

        // 5. Obtener datos de Uso Final
        $.ajax({
            url: "/Sankey/NodosTablaUso",
            type: "GET",
            datatype: "json",
            success: function (data5) {
                var filteredData5 = data5.filter(function (item) {
                    return item.año == selectedYear;
                });

                filteredData5.forEach(function (item5) {
                    tempCombinedData.push({
                        año: item5.año,
                        nivel: "Nivel 5 - Uso Final",
                        dato: item5.usoFinal_Nombre_SE,
                        valor: item5.valor,
                    });
                });
                checkIfComplete();
            },
            error: function () { checkIfComplete(); }
        });
    }

    //Para pantalla Completa
    function togglePantallaCompleta() {
        var elem = document.getElementById("flujoPantallaCompleta");

        if (!document.fullscreenElement) {
            if (elem.requestFullscreen) {
                elem.requestFullscreen();
            } else if (elem.mozRequestFullScreen) {
                /* Firefox */
                elem.mozRequestFullScreen();
            } else if (elem.webkitRequestFullscreen) {
                /* Chrome, Safari y Opera */
                elem.webkitRequestFullscreen();
            } else if (elem.msRequestFullscreen) {
                /* IE/Edge */
                elem.msRequestFullscreen();
            }
        } else {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.mozCancelFullScreen) {
                /* Firefox */
                document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {
                /* Chrome, Safari y Opera */
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                /* IE/Edge */
                document.msExitFullscreen();
            }
        }
    }
    document
        .getElementById("btnPantallaCompleta")
        .addEventListener("click", togglePantallaCompleta);

    // Funcionalidad para el botón de ocultar/mostrar depuración
    $(document).on('click', '#btn-ocultar-debug', function () {
        $('#debug-totales').hide();
    });

    $(document).on('click', '#btn-mostrar-debug', function () {
        if ($('#debug-totales').is(':visible')) {
            $('#debug-totales').hide();
            $(this).html('<i class="bi bi-bug"></i> Debug');
        } else {
            $('#debug-totales').show();
            $(this).html('<i class="bi bi-bug-fill"></i> Ocultar Debug');
            // Forzar actualización de debug con el año actual
            var currentYear = $("#year").val();
            actualizarTarjetasConAño(currentYear);
        }
    });

    //Fin Para pantalla Completa

    //Mensaje
    var messageDiv = document.getElementById("message");
    $(document).ready(function () {
        //svgContainer.innerHTML = '';
        var yearSelect = $("#year").val();
        var dataToSend = { yearSelect: yearSelect };

        let nodes = [];
        let enlaces = [];

        let nodesNodo = [];
        let nodesSector = [];
        let nodesTransformacion = [];
        let nodesTipos = [];
        let nodesUso = [];

        let nodesPrueba = [];
        let enlacesPrueba = [];

        function consultaSankey() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/consulta_Sankey",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Sankey", response);
                        resolve(response);
                    },
                    error: function (error) {
                        // Manejo del error
                        reject(error);
                    },
                });
            });
        }

        function obtieneNodosCaja() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodoscaja",
                    type: "GET",
                    contentType: "application/json",
                    success: function (response) {
                        console.log("Estructura NodosCajaSankey", response);
                        // Puedes descomentar la siguiente línea si es necesario
                        // document.getElementById('sankeySvg').innerHTML = '';

                        let yextra = 0;
                        response.forEach((data) => {
                            const { id, x, y, nombre, width, height } = data;
                            const color = getColorFromNombre(nombre);

                            const newData = { ...data };

                            if (id === "Retroalimentación") {
                                newData.yfinal = y + 395;
                                newData.xfinal = x + 260;
                            } else if (id === "nodoUsosFinales") {
                                newData.yfinal = y;
                                newData.xfinal = x + 410;
                            } else {
                                newData.yfinal = y;
                                newData.xfinal = x;
                            }

                            const { yfinal, xfinal } = newData;
                            const imgcapa = "https://cdn.sassoapps.com/img_sankey/co2.png";

                            // Crea una nueva instancia de WrapperNode y la retorna
                            const nuevoNodo = new WrapperNode(
                                id,
                                xfinal,
                                yfinal,
                                nombre,
                                color,
                                width,
                                height,
                                imgcapa,
                                nodes,
                                enlaces
                            );

                            nodes = nodes.concat(nuevoNodo);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        // Manejo del error
                        console.error("Error al obtener nodos de caja: ", error);
                        reject(error);
                    },
                });
            });
        }

        //Nodos Caja

        //Nodos
        function nodos() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/obtieneNodos",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura BD", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        document.getElementById("sankeySvg");

                        let contador = 0;
                        let contadorTotal = 0;
                        let contadorImp = 0;
                        let contadorExp = 0;
                        let valoress = [];
                        let nombress = [];
                        let id = [];
                        let valorfep = [];
                        let feP_ID2 = 100;
                        let valor2 = 0;
                        let valor_importaciones2 = 0;
                        let valor_exportaciones2 = 0;
                        //let idejemplo = 100;
                        //const nodoA;

                        response.forEach((data) => {
                            const {
                                feP_ID,
                                x,
                                y,
                                feP_Nombre_sin_espacios,
                                valor,
                                width,
                                height,
                                infoDataImp,
                                infoDataExp,
                                valor_importaciones,
                                valor_exportaciones,
                                tooltipPos,
                                año,
                            } = data;

                            contador = contador + 1;
                            contadorTotal = contadorTotal + data.valor;
                            contadorImp = contadorImp + valor_importaciones;
                            contadorExp = contadorExp + valor_exportaciones;

                            // Crear una copia del objeto original
                            const newData = { ...data };
                            valor2 = contadorTotal;
                            valor_importaciones2 = contadorImp;
                            valor_exportaciones2 = contadorExp;
                            newData.cont = contador;
                            newData.valorOfertaTotal = 20;
                            id.push(feP_ID);
                            valorfep.push(valor);

                            console.log("Contador: ", newData.cont);
                            console.log("Valor: ", newData.valor2);
                            // Asignar un nuevo valor dinámico a la propiedad nombreNodo en la copia

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { cont, valorOfertaTotal } = newData;

                            const imagen_sin_espacios = getImageFromNombre(
                                feP_Nombre_sin_espacios
                            );
                            const color = getColorFromNombre(feP_Nombre_sin_espacios); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            const nuevoNodo = new Nodo(
                                feP_ID,
                                x,
                                y,
                                feP_Nombre_sin_espacios,
                                valor,
                                color,
                                imagen_sin_espacios,
                                width,
                                height,
                                infoDataImp,
                                infoDataExp,
                                valor_importaciones,
                                valor_exportaciones,
                                tooltipPos,
                                cont,
                                año,
                                nodes,
                                enlaces
                            );

                            // Verificar si el nodo ya existe en nodes antes de agregarlo
                            //const existeEnNodes = nodesPrueba.some((existingNode) => existingNode.feP_ID === nuevoNodo4.feP_ID);

                            //if (!existeEnNodes) {
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodo);
                            nodesNodo.push(nuevoNodo);
                            //}

                            //const nodoA = new Nodo(/* parámetros para Nodo */);
                        });

                        const nuevoNodoTotal = new NodoTotal(
                            feP_ID2,
                            valor2,
                            valor_importaciones2,
                            valor_exportaciones2,
                            contador,
                            nodes,
                            enlaces
                        );
                        nodes = nodes.concat(nuevoNodoTotal);
                        nodesNodo.push(nuevoNodoTotal);

                        console.log("Nodos:", nodes);

                        for (let z = 0; z <= 11; z++) {
                            const sourceNode = nodesNodo.find((nodo) => nodo.id === id[z]);
                            const targetNode = nodesNodo.find((nodo) => nodo.id === feP_ID2);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];

                            if (z === 0) {
                                pcolors = ["BagazoCana"];
                            } else if (z === 1) {
                                pcolors = ["Biogas"];
                            } else if (z === 2) {
                                pcolors = ["Carbon"];
                            } else if (z === 3) {
                                pcolors = ["Condensados"];
                            } else if (z === 4) {
                                pcolors = ["EnergiaEolica"];
                            } else if (z === 5) {
                                pcolors = ["EnergiaSolar"];
                            } else if (z === 6) {
                                pcolors = ["GasNatural"];
                            } else if (z === 7) {
                                pcolors = ["Geotermia"];
                            } else if (z === 8) {
                                pcolors = ["EnergiaHidrica"];
                            } else if (z === 9) {
                                pcolors = ["Lena"];
                            } else if (z === 10) {
                                pcolors = ["Nucleoenergia"];
                            } else {
                                pcolors = ["Petroleo"];
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);

                            console.log("Valor colores:", particleColors);
                            const widthlinks = 5;
                            const value = valorfep[z];
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        // Aplicar el evento de clic a todos los nodos en el array
                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        let nombren = [];
        let contadorCombus = 0;
        let contadorPlant = 0;
        let contadorEl = 0;

        function sector() {
            //Nodos Sectores
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodossectores",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Sectores", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let contador = 0;
                        let contadorPet = 0;
                        let contval = [];
                        let z = 0;
                        let l = 0;
                        let contadorCoqnom;
                        let contadorRefnom;
                        let contadorRef = 0;
                        let contadorCoq = 0;
                        let contadorPlantnom;
                        let contnom = [];
                        let yvalue = [];
                        let idvalue = [];
                        let nomvalue = [];
                        let imagenes = [];

                        response.forEach((data) => {
                            const { sectorID, sector_Nombre_SE, tipo_SE, valor } = data;

                            if (tipo_SE === "Sector petróleo y gas") {
                                contadorPet = contadorPet + valor;
                                if (
                                    sector_Nombre_SE ===
                                    "Otros productos , no definidos (idustria carbón y bagazo de caña) y otras ramas"
                                ) {
                                    contnom.push("Otros productos");
                                } else {
                                    contnom.push(sector_Nombre_SE);
                                }
                                contval.push(valor);
                            } else {
                                contadorEl = contadorEl + valor;
                                contnom.push(sector_Nombre_SE);
                                contval.push(valor);
                            }

                            if (sector_Nombre_SE === "Coquizadoras") {
                                contadorCoqnom = sector_Nombre_SE;
                                contadorCoq = valor;
                                contadorCombus = contadorCombus + valor;
                            }
                            if (sector_Nombre_SE === "Refinerias y despuntadoras") {
                                contadorRefnom = sector_Nombre_SE;
                                contadorRef = valor;
                                contadorCombus = contadorCombus + valor;
                            }
                            if (sector_Nombre_SE === "Plantas de gas y fraccionadoras") {
                                contadorPlantnom = sector_Nombre_SE;
                                contadorPlant = valor;
                            }

                            contador = contador + 1;

                            const newData = { ...data };
                            newData.idnuevo = sectorID + 220;
                            newData.cont = contador;
                            newData.pet = contadorPet;
                            newData.el = contadorEl;
                            newData.tam = z;
                            newData.tam2 = l;

                            // Destructurar el nuevo objeto con las nuevas propiedades

                            const { idnuevo, cont, pet, el, tam, tam2 } = newData;

                            const color = getColorFromNombre(tipo_SE); // Asume que tienes esta función definida
                        });

                        for (let i = 0; i <= 1; i++) {
                            if (i === 0) {
                                idvalue[i] = 200;
                                yvalue[i] = 330;
                                nomvalue[i] = "Sector petróleo y gas";
                                imagenes[i] = "https://cdn.sassoapps.com/img_sankey/s_petroliferos.png";
                            } else {
                                idvalue[i] = 210;
                                yvalue[i] = 620;
                                nomvalue[i] = "Sector eléctrico";
                                imagenes[i] = "https://cdn.sassoapps.com/img_sankey/electricidadi.png";
                            }

                            const nuevoNodoTooltip = new SectorTooltip(
                                idvalue[i],
                                nomvalue[i],
                                yvalue[i],
                                imagenes[i],
                                contadorPet,
                                contadorEl,
                                contnom,
                                contval,
                                nodes,
                                enlaces
                            );

                            //nodes = nodes.concat(nuevoNodoTooltip);
                            nodes = nodes.concat(nuevoNodoTooltip);
                            nodesSector.push(nuevoNodoTooltip);

                            const sourceNode = nodesNodo.find((nodo) => nodo.id === 100);
                            const targetNode = nodesSector.find(
                                (nodo) => nodo.id === idvalue[i]
                            );
                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (i === 0) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorPet;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contadorEl;
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            var padre = document.getElementById("sankeySvg");

                            //svgElement.appendChild(links);

                            enlaces = enlaces.concat(links);
                        }

                        let idn = [];
                        let yn = [];

                        for (let z = 0; z <= 2; z++) {
                            if (z === 0) {
                                idn[z] = 300;
                                yn[z] = 280;
                                nombren[z] = "Combustible";
                            } else if (z === 1) {
                                idn[z] = 301;
                                yn[z] = 420;
                                nombren[z] = "Calor";
                            } else {
                                idn[z] = 302;
                                yn[z] = 640;
                                nombren[z] = "Electricidad";
                            }
                            const nuevoNodoProvision = new NodoProvisionyProduccion(
                                idn[z],
                                yn[z],
                                nombren[z],
                                contadorCombus,
                                contadorCoq,
                                contadorRef,
                                contadorCoqnom,
                                contadorRefnom,
                                contadorPlant,
                                contadorPlantnom,
                                contadorEl,
                                nodes,
                                enlaces
                            );
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodoProvision);
                            nodesSector.push(nuevoNodoProvision);
                            let srce;

                            if (z === 0) {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[0]);
                            } else if (z === 1) {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[0]);
                            } else {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[1]);
                            }
                            const sourceNodeB = srce;
                            const targetNodeB = nodesSector.find(
                                (nodo) => nodo.id === idn[z]
                            );

                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColorB = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (z === 0) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorCombus;
                            } else if (z === 1) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorPlant;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contadorEl;
                            }

                            const particleColorsB = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinksB = 5;
                            const valueB = vl;
                            //const value2 = contadorEl;
                            const curveB = 0;
                            const typeB = "normal";
                            const linksB = new Link(
                                sourceNodeB,
                                targetNodeB,
                                widthlinksB,
                                backgroundColorB,
                                valueB,
                                curveB,
                                particleColorsB,
                                typeB
                            );

                            enlaces = enlaces.concat(linksB);
                        }

                        // Aplicar el evento de clic a todos los nodos en el array
                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        function transformaciones() {
            //Nodos Transformaciones
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodostransformaciones",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Transformaciones", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let yvalue = 30;
                        let cont = 70;
                        let yvalue2 = 120;
                        let contPetro = 0;
                        let contCent = 0;
                        let petro;
                        let cent;
                        let contador = 0;
                        let idsum = [];
                        let idtot = [];
                        let ytot = [];
                        let capa = 0;
                        let capacont = 0;

                        response.forEach((data) => {
                            const {
                                transformacionID,
                                transformacion_Nombre_SE,
                                tipo,
                                valor,
                            } = data;

                            const newData = { ...data };

                            //Capa extra
                            if (capacont === 0) {
                                for (let f = 0; f <= 2; f++) {
                                    //const { id, x, y, nombre, width, height } = data;

                                    let contid;
                                    let conty;
                                    let contheight;
                                    let contimg;

                                    if (f === 0) {
                                        contid = "Transformacion1";
                                        conty = 310;
                                        contheight = 355;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/co2.png";
                                    } else if (f === 1) {
                                        contid = "Transformacion2";
                                        conty = 670;
                                        contheight = 420;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/rr4.png";
                                    } else {
                                        contid = "Transformacion3";
                                        conty = 60;
                                        contheight = 206;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/co2.png";
                                    }

                                    const idCapa = contid;
                                    const xCapa = 770;
                                    const yCapa = conty;
                                    const nombreCapa = "Transformación";
                                    const colorCapa = getColorFromNombre(nombreCapa);
                                    const widthCapa = 150;
                                    const heightCapa = contheight;
                                    const imgcapa = contimg;

                                    // Crea una nueva instancia de WrapperNode y la retorna
                                    const nuevoCapa = new WrapperNode(
                                        idCapa,
                                        xCapa,
                                        yCapa,
                                        nombreCapa,
                                        colorCapa,
                                        widthCapa,
                                        heightCapa,
                                        imgcapa,
                                        nodes,
                                        enlaces
                                    );

                                    nodes = nodes.concat(nuevoCapa);
                                }
                            }
                            capacont = 1;

                            //Fin de capa extra

                            if (tipo === "Petróleo y gas") {
                                yvalue = yvalue + cont;
                                contPetro = contPetro + valor;
                                petro = tipo;
                                console.log("valor de y:", yvalue);
                            } else {
                                yvalue = yvalue + yvalue2;
                                contCent = contCent + valor;
                                yvalue2 = 70;
                                cent = tipo;
                                console.log("valor de y:", yvalue);
                            }

                            newData.y = yvalue;
                            newData.idfinal = transformacionID + 400;

                            // Destructurar el nuevo objeto con las nuevas propiedades

                            const { y, idfinal } = newData;

                            const color = getColorFromNombre(transformacion_Nombre_SE); // Asume que tienes esta función definida
                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            const nuevoNodoTranformacion = new NodoTransformacion(
                                idfinal,
                                transformacion_Nombre_SE,
                                tipo,
                                valor,
                                color,
                                y,
                                nodes,
                                enlaces
                            );

                            idsum[contador] = idfinal;
                            contador = contador + 1;

                            nodes = nodes.concat(nuevoNodoTranformacion);
                            nodesTransformacion.push(nuevoNodoTranformacion);

                            let srce;

                            for (let z = 0; z <= 1; z++) {
                                if (tipo === "Petróleo y gas" && z === 0) {
                                    srce = nodesSector.find((nodo) => nodo.id === 300);
                                } else if (tipo === "Petróleo y gas" && z === 1) {
                                    srce = nodesSector.find((nodo) => nodo.id === 301);
                                } else {
                                    srce = nodesSector.find((nodo) => nodo.id === 302);
                                    z = 1;
                                }
                                const sourceNode = srce;
                                const targetNode = nodesTransformacion.find(
                                    (nodo) => nodo.id === idfinal
                                );

                                //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                                const backgroundColor = coloresEnergia["FondoNodo"];
                                let pcolors = [];
                                let vl = 0;

                                if (tipo === "Petróleo y gas") {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                } else {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                }

                                const particleColors = pcolors.map(
                                    (key) => coloresEnergia[key]
                                );
                                //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                const widthlinks = 5;
                                const value = valor;
                                //const value2 = contadorEl;
                                const curve = 0;
                                const type = "normal";
                                const links = new Link(
                                    sourceNode,
                                    targetNode,
                                    widthlinks,
                                    backgroundColor,
                                    value,
                                    curve,
                                    particleColors,
                                    type
                                );

                                enlaces = enlaces.concat(links);
                            }
                        });

                        for (let k = 0; k <= 1; k++) {
                            if (k === 0) {
                                idtot[k] = 500;
                                ytot[k] = 165;
                            } else {
                                idtot[k] = 501;
                                ytot[k] = 705;
                            }

                            const nuevoNodoTransTotal = new NodoTransformacionTotal(
                                idtot[k],
                                ytot[k],
                                contPetro,
                                contCent,
                                petro,
                                cent,
                                nodes,
                                enlaces
                            );
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodoTransTotal);
                            nodesTransformacion.push(nuevoNodoTransTotal);
                        }

                        let srce;

                        for (let v = 0; v <= 13; v++) {
                            const sourceNode = nodesTransformacion.find(
                                (nodo) => nodo.id === idsum[v]
                            );

                            if (v <= 2) {
                                srce = nodesTransformacion.find((nodo) => nodo.id === idtot[0]);
                            } else {
                                srce = nodesTransformacion.find((nodo) => nodo.id === idtot[1]);
                            }

                            const targetNode = srce;

                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (v <= 2) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contPetro;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contCent;
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        let nombrearray = [];
        let dist = 0;
        let rnt = 0;

        function tiposEnergia() {
            //Nodos Tipos Energia
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodostiposenergia",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Tipos Energía", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let cont = 0;
                        let idv = 600;
                        let yvalues = 0;
                        let xvalores = [];
                        let yvalores = [];
                        let nombrecont = [];
                        let ycont = [];
                        let idcont = [];
                        let idarray = [];
                        let capacont = 0;
                        response.forEach((data) => {
                            const {
                                cargaPico,
                                intermitente,
                                cargaBase,
                                gasSeco,
                                gasLP,
                                petrolíferos,
                            } = data;

                            //Capa extra
                            if (capacont === 0) {
                                for (let f = 0; f <= 3; f++) {
                                    //const { id, x, y, nombre, width, height } = data;

                                    let contid;
                                    let conty;
                                    let contheight;
                                    let contx;
                                    let imgimpexp;

                                    if (f === 0) {
                                        contid = "Importacion1";
                                        contx = 800;
                                        conty = 310;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/importacion.png";
                                    } else if (f === 1) {
                                        contid = "Importacion2";
                                        contx = 800;
                                        conty = 670;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/importacion.png";
                                    } else if (f === 2) {
                                        contid = "Exportacion1";
                                        contx = 820;
                                        conty = 310;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/exportacion.png";
                                    } else {
                                        contid = "Exportacion2";
                                        contx = 820;
                                        conty = 670;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/exportacion.png";
                                    }

                                    const idCapa = contid;
                                    const xCapa = contx;
                                    const yCapa = conty;
                                    const nombreCapa = "Transformación";
                                    const colorCapa = getColorFromNombre(nombreCapa);
                                    const widthCapa = 150;
                                    const heightCapa = contheight;
                                    const imgcapa = imgimpexp;

                                    // Crea una nueva instancia de WrapperNode y la retorna
                                    const nuevoCapa = new WrapperNode(
                                        idCapa,
                                        xCapa,
                                        yCapa,
                                        nombreCapa,
                                        colorCapa,
                                        widthCapa,
                                        heightCapa,
                                        imgcapa,
                                        nodes,
                                        enlaces
                                    );

                                    nodes = nodes.concat(nuevoCapa);
                                }
                            }
                            capacont = 1;

                            //Fin de capa extra

                            for (let i = 0; i <= 5; i++) {
                                idcont[i] = idv;
                                if (i === 3) {
                                    nombrecont[i] = "Carga Pico";
                                    yvalues = 635;
                                } else if (i === 5) {
                                    nombrecont[i] = "Intermitente";
                                    yvalues = 775;
                                } else if (i === 4) {
                                    nombrecont[i] = "Carga Base";
                                    yvalues = 705;
                                } else if (i === 1) {
                                    nombrecont[i] = "Gas Seco";
                                    nombrearray[i] = "RNT";
                                    yvalues = 170;
                                    yvalores[i] = 687;
                                    xvalores[i] = 1280;
                                    idarray[i] = 710;
                                } else if (i === 2) {
                                    nombrecont[i] = "Gas LP";
                                    nombrearray[i] = "RGD";
                                    yvalues = 240;
                                    yvalores[i] = 687;
                                    xvalores[i] = 1380;
                                    idarray[i] = 720;
                                } else {
                                    nombrecont[i] = "Petrolíferos";
                                    nombrearray[i] = "Distribución";
                                    yvalues = 100;
                                    yvalores[i] = 155;
                                    xvalores[i] = 1360;
                                    idarray[i] = 700;
                                }
                                idv = idv + 1;
                                ycont[i] = yvalues;
                                //yvalues = yvalues + 50;
                            }

                            const newData = { ...data };
                            newData.id = idcont;
                            newData.nombre = nombrecont;
                            newData.y = ycont;

                            idv = idv + 1;
                            cont = cont + 1;

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { id, nombre, y } = newData;

                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            for (let k = 0; k <= 5; k++) {
                                const color = getColorFromNombre(nombre[k]); // Asume que tienes esta función definida
                                const image = getImageFromNombre(nombre[k]);
                                const nuevoNodoTipos = new NodoTiposEnergia(
                                    id[k],
                                    nombre[k],
                                    y[k],
                                    cargaPico,
                                    intermitente,
                                    cargaBase,
                                    gasSeco,
                                    gasLP,
                                    petrolíferos,
                                    color,
                                    image,
                                    nodes,
                                    enlaces
                                );
                                nodes = nodes.concat(nuevoNodoTipos);
                                nodesTipos.push(nuevoNodoTipos);

                                let srce;

                                if (k <= 2) {
                                    srce = nodesTransformacion.find((nodo) => nodo.id === 500);
                                } else {
                                    srce = nodesTransformacion.find((nodo) => nodo.id === 501);
                                }
                                const sourceNode = srce;

                                const targetNode = nodesTipos.find((nodo) => nodo.id === id[k]);

                                const backgroundColor = coloresEnergia["FondoNodo"];
                                let pcolors = [];
                                let vl = 0;

                                if (k === 0) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = petrolíferos;
                                } else if (k === 1) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = gasSeco;
                                } else if (k === 2) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = gasLP;
                                } else if (k === 3) {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = cargaPico;
                                } else if (k === 4) {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = cargaBase;
                                } else {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = intermitente;
                                }

                                const particleColors = pcolors.map(
                                    (key) => coloresEnergia[key]
                                );
                                //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                const widthlinks = 5;
                                const value = vl;
                                //const value2 = contadorEl;
                                const curve = 0;
                                const type = "normal";
                                const links = new Link(
                                    sourceNode,
                                    targetNode,
                                    widthlinks,
                                    backgroundColor,
                                    value,
                                    curve,
                                    particleColors,
                                    type
                                );

                                enlaces = enlaces.concat(links);
                            }

                            dist = petrolíferos + gasSeco + gasLP;
                            rnt = cargaPico + cargaBase + intermitente;
                        });
                        for (let z = 0; z <= 2; z++) {
                            const color2 = getColorFromNombre(nombrearray[z]); // Asume que tienes esta función definida
                            const image2 = getImageFromNombre(nombrearray[z]);
                            const nuevoNodoDist = new NodoDistribucion(
                                idarray[z],
                                nombrearray[z],
                                yvalores[z],
                                xvalores[z],
                                dist,
                                rnt,
                                color2,
                                image2,
                                nodes,
                                enlaces
                            );
                            nodes = nodes.concat(nuevoNodoDist);
                            nodesTipos.push(nuevoNodoDist);
                        }

                        let srce;
                        let trgt;
                        let pcolors = [];
                        let vl = 0;

                        for (let b = 0; b <= 6; b++) {
                            if (b === 0) {
                                srce = nodesTipos.find((nodo) => nodo.id === 600);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 1) {
                                srce = nodesTipos.find((nodo) => nodo.id === 601);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 2) {
                                srce = nodesTipos.find((nodo) => nodo.id === 602);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 3) {
                                srce = nodesTipos.find((nodo) => nodo.id === 603);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else if (b === 4) {
                                srce = nodesTipos.find((nodo) => nodo.id === 604);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else if (b === 5) {
                                srce = nodesTipos.find((nodo) => nodo.id === 605);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else {
                                srce = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[2]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            }
                            const sourceNode = srce;

                            const targetNode = trgt;

                            const backgroundColor = coloresEnergia["FondoNodo"];

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        function usosFinales() {
            //Nodos Usos finales
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodosusofinal",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Uso Final", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let cont = 0;
                        let idv = 800;
                        let newid = 0;
                        let yvalues = 325;
                        let yvalores = [];
                        let nombrecont = [];
                        let nombrearray = [];
                        let ycont = [];
                        let idcont = [];
                        let idarray = [];
                        let vl = 0;
                        let totalfinal = 0;
                        response.forEach((data) => {
                            const {
                                hogares,
                                transporte,
                                serPubCom,
                                agricultura,
                                industrial,
                                sectorEnergia,
                                hogares_Co2,
                                transporte_Co2,
                                serPubCom_Co2,
                                agricultura_Co2,
                                industrial_Co2,
                                sectorEnergia_Co2,
                                año,
                            } = data;

                            for (let i = 0; i <= 5; i++) {
                                idcont[i] = idv;
                                if (i === 3) {
                                    nombrecont[i] = "Agricultura";
                                    vl = agricultura;
                                } else if (i === 5) {
                                    nombrecont[i] = "Sector Energía";
                                    vl = sectorEnergia;
                                } else if (i === 4) {
                                    nombrecont[i] = "Industrial";
                                    vl = industrial;
                                } else if (i === 1) {
                                    nombrecont[i] = "Transporte";
                                    vl = transporte;
                                } else if (i === 2) {
                                    nombrecont[i] = "Serv. Púb. y Com.";
                                    vl = serPubCom;
                                } else {
                                    nombrecont[i] = "Hogares";
                                    vl = hogares;
                                }
                                idv = idv + 1;
                                ycont[i] = yvalues;
                                yvalues = yvalues + 70;
                                //yvalues = yvalues + 50;
                            }

                            yvalues = yvalues + 70;

                            cont = cont + 1;

                            const newData = { ...data };
                            newData.newid = idcont;
                            newData.newy = ycont;
                            newData.newnombre = nombrecont;

                            // idv = idv + 1;
                            cont = cont + 1;

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { newid, newnombre, newy } = newData;

                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            for (let k = 0; k <= 5; k++) {
                                const color = getColorFromNombre(newnombre[k]); // Asume que tienes esta función definida
                                const image = getImageFromNombre(newnombre[k]);
                                const nuevoNodoFinal = new NodoUsoFinal(
                                    newid[k],
                                    newnombre[k],
                                    newy[k],
                                    hogares,
                                    transporte,
                                    serPubCom,
                                    agricultura,
                                    industrial,
                                    sectorEnergia,
                                    hogares_Co2,
                                    transporte_Co2,
                                    serPubCom_Co2,
                                    agricultura_Co2,
                                    industrial_Co2,
                                    sectorEnergia_Co2,
                                    año,
                                    color,
                                    image,
                                    nodes,
                                    enlaces
                                );
                                nodes = nodes.concat(nuevoNodoFinal);
                                nodesUso.push(nuevoNodoFinal);

                                let srce;
                                let pcolors = [];

                                console.log("Nodos de tipos:", nodesTipos);

                                for (let z = 0; z <= 1; z++) {
                                    if (z === 0) {
                                        srce = nodesTipos.find((nodo) => nodo.id === 700);
                                        pcolors = [
                                            "Carbon",
                                            "Condensados",
                                            "GasNatural",
                                            "Petroleo",
                                        ];
                                    } else {
                                        srce = nodesTipos.find((nodo) => nodo.id === 720);
                                        pcolors = [
                                            "Nucleoenergia",
                                            "BagazoCana",
                                            "Biogas",
                                            "EnergiaEolica",
                                            "EnergiaSolar",
                                            "Geotermia",
                                            "EnergiaHidrica",
                                            "Lena",
                                        ];
                                    }
                                    const sourceNode = srce;

                                    const targetNode = nodesUso.find(
                                        (nodo) => nodo.id === newid[k]
                                    );

                                    const backgroundColor = coloresEnergia["FondoNodo"];

                                    const particleColors = pcolors.map(
                                        (key) => coloresEnergia[key]
                                    );
                                    //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                    const widthlinks = 5;
                                    const value = vl;
                                    //const value2 = contadorEl;
                                    const curve = 0;
                                    const type = "normal";
                                    const links = new Link(
                                        sourceNode,
                                        targetNode,
                                        widthlinks,
                                        backgroundColor,
                                        value,
                                        curve,
                                        particleColors,
                                        type
                                    );

                                    enlaces = enlaces.concat(links);
                                }

                                totalfinal = totalfinal + vl;
                            }
                        });

                        const sourceNode = nodes.find(
                            (nodo) => nodo.id === "nodoUsosFinales"
                        );

                        const targetNode = nodes.find(
                            (nodo) => nodo.id === "Transformación"
                        );

                        const backgroundColor = coloresEnergia["FondoNodo"];

                        let pcolors = [
                            "Carbon",
                            "Condensados",
                            "GasNatural",
                            "Petroleo",
                            "Nucleoenergia",
                            "BagazoCana",
                            "Biogas",
                            "EnergiaEolica",
                            "EnergiaSolar",
                            "Geotermia",
                            "EnergiaHidrica",
                            "Lena",
                        ];

                        const particleColors = pcolors.map((key) => coloresEnergia[key]);
                        //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                        const widthlinks = 5;
                        const value = totalfinal;
                        //const value2 = contadorEl;
                        const curve = 18;
                        const type = "retroalimentacion";
                        const links = new Link(
                            sourceNode,
                            targetNode,
                            widthlinks,
                            backgroundColor,
                            value,
                            curve,
                            particleColors,
                            type
                        );

                        enlaces = enlaces.concat(links);

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }
        async function ejecutarConsultaSankeyCompleta() {
            try {
                await consultaSankey();
                await obtieneNodosCaja();
                await nodos();
                await sector();
                await transformaciones();
                await tiposEnergia();
                await usosFinales();
                await tablaFinal(); // ✅ Aquí sí se ejecuta cuando ya se llenó combinedData

            } catch (error) {
                console.error("Error en la ejecución de consulta Sankey:", error);
            }
        }

        obtieneNodosCaja()
            .then(() => consultaSankey())
            .then(() => nodos())
            .then(() => sector())
            .then(() => transformaciones())
            .then(() => tiposEnergia())
            .then(() => usosFinales())
            .then(() => tablaFinal())
            .catch((error) => {
                console.error("Error en la cadena de promesas:", error);
            });

        function tablaFinal() {
            const nivelOrden = {
                "Nivel FEP - Fuentes de Energía Primaria": 1,
                "Nivel FOCAL - Sector Energético": 2,
                "Nivel 1 - Provisión y Producción": 3,
                "Nivel 2 - Transformaciones": 4,
                "Nivel 3 - Tipos de Energía": 5,
                "Nivel 4 - Distribución": 6,
                "Nivel 5 - Uso Final": 7,
            };

            combinedData = [];
            // var combinedData = [];
            // Realiza la primera solicitud AJAX para obtener los datos de la primera tabla
            $.ajax({
                url: "/Sankey/NodosTablaFep",
                type: "GET",
                datatype: "json",
                success: function (data1) {
                    // Filtra los datos de la primera tabla según el año seleccionado
                    var filteredData1 = data1.filter(function (item) {
                        return item.año == yearSelect;
                    });

                    console.log("Datos filtrados: ", yearSelect);

                    let cont = 0;

                    // Combinar los datos de la primera tabla con los datos agrupados y sumados de la segunda tabla
                    filteredData1.forEach(function (item1) {
                        combinedData.push({
                            año: item1.año,
                            nivel: "Nivel FEP - Fuentes de Energía Primaria",
                            dato: item1.feP_Nombre_sin_espacios,
                            valor: item1.valor,
                        });

                        console.log("Tamaño:", nombren.length);

                        //Tercera columna
                        if (cont === 0) {
                            for (let i = 0; i <= nombren.length - 1; i++) {
                                if (nombren[i] === "Combustible") {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorCombus,
                                    });
                                } else if (nombren[i] === "Calor") {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorPlant,
                                    });
                                } else {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorEl,
                                    });
                                }
                            }
                        }
                        cont = 1;
                    });

                    console.log("Primer Combined: ", combinedData);
                    obtenerDatosTablaSector();
                },
            });

            function obtenerDatosTablaSector() {
                // Realiza la segunda solicitud AJAX para obtener los datos de la otra tabla
                $.ajax({
                    url: "/Sankey/NodosTablaSector",
                    type: "GET",
                    datatype: "json",
                    success: function (data2) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData2 = data2.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        // Crear un objeto para realizar la agrupación y cálculo de sumas
                        var groupedData = {};

                        // Procesar los datos de la segunda tabla
                        filteredData2.forEach(function (item2) {
                            // Verificar si el tipo_SE ya existe en el objeto agrupado
                            if (groupedData[item2.tipo_SE]) {
                                // Si existe, agregar el valor al existente
                                groupedData[item2.tipo_SE].valor += item2.valor;
                            } else {
                                // Si no existe, crear una nueva entrada
                                groupedData[item2.tipo_SE] = {
                                    año: item2.año,
                                    nivel: "Nivel FOCAL - Sector Energético",
                                    dato: item2.tipo_SE,
                                    valor: item2.valor,
                                };
                            }
                        });

                        // Convertir el objeto agrupado en un array para combinar con los datos de la primera tabla
                        combinedData = combinedData.concat(Object.values(groupedData));

                        obtenerDatosTablaTransformacion();
                    },
                });
            }

            function obtenerDatosTablaTransformacion() {
                $.ajax({
                    url: "/Sankey/NodosTablaTransformacion",
                    type: "GET",
                    datatype: "json",
                    success: function (data3) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData3 = data3.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        filteredData3.forEach(function (item3) {
                            combinedData.push({
                                año: item3.año,
                                nivel: "Nivel 2 - Transformaciones",
                                dato: item3.transformacion_Nombre_SE,
                                valor: item3.valor,
                            });
                        });

                        obtenerDatosTablaTipos();
                    },
                });
            }

            function obtenerDatosTablaTipos() {
                $.ajax({
                    url: "/Sankey/NodosTablaTipos",
                    type: "GET",
                    datatype: "json",
                    success: function (data4) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData4 = data4.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        let conta = 0;

                        filteredData4.forEach(function (item4) {
                            let nombre;
                            for (let z = 0; z <= 5; z++) {
                                if (z === 0) {
                                    nombre = "Petrolíferos";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.petrolíferos,
                                    });
                                } else if (z === 1) {
                                    nombre = "Gas Seco";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.gasSeco,
                                    });
                                } else if (z === 2) {
                                    nombre = "Gas LP";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.gasLP,
                                    });
                                } else if (z === 3) {
                                    nombre = "Carga Pico";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.cargaPico,
                                    });
                                } else if (z === 4) {
                                    nombre = "Carga Base";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.cargaBase,
                                    });
                                } else {
                                    nombre = "Intermitente";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.intermitente,
                                    });
                                }
                            }

                            //Sexta columna
                            if (conta === 0) {
                                for (let i = 0; i <= nombrearray.length - 1; i++) {
                                    if (nombrearray[i] === "Distribución") {
                                        combinedData.push({
                                            año: item4.año,
                                            nivel: "Nivel 4 - Distribución",
                                            dato: nombrearray[i],
                                            valor: dist,
                                        });
                                    } else {
                                        combinedData.push({
                                            año: item4.año,
                                            nivel: "Nivel 4 - Distribución",
                                            dato: nombrearray[i],
                                            valor: rnt,
                                        });
                                    }
                                }
                            }
                            conta = 1;
                        });

                        obtenerDatosTablaUso();
                    },
                });
            }

            function obtenerDatosTablaUso() {
                $.ajax({
                    url: "/Sankey/NodosTablaUso",
                    type: "GET",
                    datatype: "json",
                    success: function (data5) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData5 = data5.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        filteredData5.forEach(function (item5) {
                            let nombre;
                            for (let z = 0; z <= 5; z++) {
                                if (z === 0) {
                                    nombre = "Hogares";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.hogares,
                                    });
                                } else if (z === 1) {
                                    nombre = "Transporte";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.transporte,
                                    });
                                } else if (z === 2) {
                                    nombre = "Servicio Público y Comercial";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.serPubCom,
                                    });
                                } else if (z === 3) {
                                    nombre = "Agricultura";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.agricultura,
                                    });
                                } else if (z === 4) {
                                    nombre = "Industrial";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.industrial,
                                    });
                                } else {
                                    nombre = "Sector Energía";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.sectorEnergia,
                                    });
                                }
                            }
                        });

                        // Ordenar los datos por nivel
                        combinedData.sort((a, b) => {
                            return nivelOrden[a.nivel] - nivelOrden[b.nivel];
                        });

                        // Ahora que combinedData está completo, calcula los totales
                        calcularTotalesPorNivel();

                        // Inicializa el DataTable utilizando la estructura de datos combinada
                        var table_mun = $("#Tipos").DataTable({
                            lengthMenu: [
                                [10, 50, 100, -1],
                                [10, 50, 100, "Todos"],
                            ],
                            dom: "Blfrtip",
                            buttons: [
                                {
                                    extend: "copyHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "excelHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "csvHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "pdfHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                    customize: function (doc) {
                                        doc.styles.tableHeader.color = "#9fa1a4";
                                        doc.defaultStyle.alignment = "center";
                                        doc.styles.tableHeader.fillColor = "#4c1922";
                                    },
                                },
                            ],
                            data: combinedData, // Utiliza la estructura de datos combinada como origen de datos
                            columns: [
                                { data: "año" },
                                { data: "nivel" },
                                { data: "dato" }, // Utiliza el nombre de la tercera columna
                                {
                                    data: "valor",
                                    render: $.fn.dataTable.render.number(",", ".", 1),
                                }, // Utiliza el nombre de la cuarta columna
                            ],
                            language: {
                                url: "//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json",
                            },
                            // Otras opciones como columnDefs, etc.
                        });
                    },
                });
            }

            function calcularTotalesPorNivel() {
                let totalPorNivel = {
                    "Nivel FEP - Fuentes de Energía Primaria": 0,
                    "Nivel FOCAL - Sector Energético": 0,
                    "Nivel 1 - Provisión y Producción": 0,
                    "Nivel 2 - Transformaciones": 0,
                    "Nivel 3 - Tipos de Energía": 0,
                    "Nivel 4 - Distribución": 0,
                    "Nivel 5 - Uso Final": 0,
                };

                combinedData.forEach(function (item) {
                    if (item.nivel in totalPorNivel) {
                        const valor = parseFloat(item.valor || 0);
                        if (!isNaN(valor)) {
                            totalPorNivel[item.nivel] += valor;
                        }
                    }
                });

                const formatter = new Intl.NumberFormat("es-MX", {
                    minimumFractionDigits: 1,
                    maximumFractionDigits: 1,
                });

                for (let nivel in totalPorNivel) {
                    totalPorNivel[nivel] = formatter.format(totalPorNivel[nivel]);
                }

                console.log("Totales por nivel calculados:", totalPorNivel);

                const iconosPorNivel = {
                    "Nivel FEP - Fuentes de Energía Primaria": "bi-lightning-charge-fill",
                    "Nivel FOCAL - Sector Energético": "bi-bar-chart-line-fill",
                    "Nivel 1 - Provisión y Producción": "bi-truck",
                    "Nivel 2 - Transformaciones": "bi-cpu-fill",
                    "Nivel 3 - Tipos de Energía": "bi-layers-fill",
                    "Nivel 4 - Distribución": "bi-diagram-3-fill",
                    "Nivel 5 - Uso Final": "bi-house-fill",
                };

                const coloresFondo = [
                    "#e3f2fd", "#fce4ec", "#e8f5e9", "#fff3e0", "#ede7f6", "#f3e5f5", "#e0f7fa"
                ];
                // Verifica si hay datos válidos
                const hayDatos = combinedData.length > 0 && combinedData.some(item => !isNaN(parseFloat(item.valor)));

                if (!hayDatos) {
                    $("#tarjetas-totales").html(`
                    <div class="alert alert-warning w-100 text-center" role="alert">
                        No hay datos disponibles para el filtro seleccionado.
                    </div>
                `);
                    return; // Evita seguir si no hay datos
                }

                // Limpiar contenedor y renderizar tarjetas
                $("#tarjetas-totales").html("").removeClass();

                for (let nivel in totalPorNivel) {
                    const nivelId = nivel
                        .toLowerCase()
                        .replace(/ /g, "-")
                        .replace(/[^a-z0-9-]/g, "");

                    // Determinar el ícono apropiado para cada nivel
                    let icono = 'bi-bar-chart-line';
                    switch (nivel) {
                        case 'Nivel FEP - Fuentes de Energía Primaria':
                            icono = 'bi-lightning-charge';
                            break;
                        case 'Nivel FOCAL - Sector Energético':
                            icono = 'bi-building';
                            break;
                        case 'Nivel 1 - Provisión y Producción':
                            icono = 'bi-gear';
                            break;
                        case 'Nivel 2 - Transformaciones':
                            icono = 'bi-arrow-repeat';
                            break;
                        case 'Nivel 3 - Tipos de Energía':
                            icono = 'bi-fuel-pump';
                            break;
                        case 'Nivel 4 - Distribución':
                            icono = 'bi-diagram-3';
                            break;
                        case 'Nivel 5 - Uso Final':
                            icono = 'bi-house-gear';
                            break;
                        default:
                            icono = 'bi-bar-chart-line';
                    }

                    $("#tarjetas-totales").append(`
                        <div class="sankey-card">
                            <div class="card-body">
                                <div class="card-icon">
                                    <i class="bi ${icono} fs-4"></i>
                                </div>
                                <h6 class="card-title">${nivel}</h6>
                                <p class="card-value">${totalPorNivel[nivel]} PJ</p>
                            </div>
                        </div>
                    `);
                }
            }

        }

        // Al hacer clic fuera de un nodo
        sankeySvg.addEventListener("click", (evt) => {
            nodes.forEach((node) => (node.group.style.opacity = 1)); // Restaura la opacidad de todos los nodos
            enlaces.forEach((link) => {
                link.pathElement.style.opacity = 1; // Restaura la opacidad del enlace
                if (link.particles) {
                    link.particles.forEach((particle) => {
                        particle.style.opacity = 1; // Restaura la opacidad de las partículas
                    });
                }
            });
        });
    });

    //Boton Ejecutar
    //Funcion para Evaluar La Solicitud
    $("#btnEjecutar").click(function (event) {
        const svgContainer = (document.getElementById("sankeySvg").innerHTML = "");
        var contenedorAdicional = document.getElementById("highchartsContainer");
        var contenedorAdicional2 = document.getElementById("playButton");
        var contenedorAdicional3 = document.getElementById("yearSlider");
        var contenedorAdicional4 = document.getElementById("newChartContainer");
        var contenedorAdicional5 = document.getElementById("regresarSankeyButton");
        contenedorAdicional.style.display = "none";
        contenedorAdicional2.style.display = "none";
        contenedorAdicional3.style.display = "none";
        contenedorAdicional4.style.display = "none";
        contenedorAdicional5.style.display = "none";
        //svgContainer.innerHTML = '';
        var yearSelect = $("#year").val();
        var dataToSend = { yearSelect: yearSelect };
        // Valida que el campo no este vacío
        // Validar que ninguna de las variables esté vacía o nula
        if (!yearSelect) {
            var messageDiv = document.getElementById("message");
            messageDiv.innerHTML =
                "Debes seleccionar un año primero antes de ejecutar la consulta.";
            messageDiv.classList.remove("d-none"); // Muestra el mensaje
            // Opcional: Ocultar el mensaje después de 5 segundos
            setTimeout(function () {
                messageDiv.classList.add("d-none"); // Oculta el mensaje
            }, 5000); // 5 segundos
            return; // Termina la función aquí para no enviar la solicitud AJAX
        }

        let nodes = [];
        let enlaces = [];

        let nodesNodo = [];
        let nodesSector = [];
        let nodesTransformacion = [];
        let nodesTipos = [];
        let nodesUso = [];

        let nodesPrueba = [];
        let enlacesPrueba = [];

        function consultaSankey() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/consulta_Sankey",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Sankey", response);
                        resolve(response);
                    },
                    error: function (error) {
                        // Manejo del error
                        reject(error);
                    },
                });
            });
        }

        function obtieneNodosCaja() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodoscaja",
                    type: "GET",
                    contentType: "application/json",
                    success: function (response) {
                        console.log("Estructura NodosCajaSankey", response);
                        // Puedes descomentar la siguiente línea si es necesario
                        // document.getElementById('sankeySvg').innerHTML = '';

                        let yextra = 0;
                        response.forEach((data) => {
                            const { id, x, y, nombre, width, height } = data;
                            const color = getColorFromNombre(nombre);

                            const newData = { ...data };

                            if (id === "Retroalimentación") {
                                newData.yfinal = y + 395;
                                newData.xfinal = x + 260;
                            } else if (id === "nodoUsosFinales") {
                                newData.yfinal = y;
                                newData.xfinal = x + 410;
                            } else {
                                newData.yfinal = y;
                                newData.xfinal = x;
                            }

                            const { yfinal, xfinal } = newData;

                            const imgcapa = "https://cdn.sassoapps.com/img_sankey/co2.png";

                            // Crea una nueva instancia de WrapperNode y la retorna
                            const nuevoNodo = new WrapperNode(
                                id,
                                xfinal,
                                yfinal,
                                nombre,
                                color,
                                width,
                                height,
                                imgcapa,
                                nodes,
                                enlaces
                            );

                            nodes = nodes.concat(nuevoNodo);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        // Manejo del error
                        console.error("Error al obtener nodos de caja: ", error);
                        reject(error);
                    },
                });
            });
        }

        //Nodos Caja

        //Nodos
        function nodos() {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/obtieneNodos",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura BD", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        document.getElementById("sankeySvg");

                        let contador = 0;
                        let contadorTotal = 0;
                        let contadorImp = 0;
                        let contadorExp = 0;
                        let valoress = [];
                        let nombress = [];
                        let id = [];
                        let valorfep = [];
                        let feP_ID2 = 100;
                        let valor2 = 0;
                        let valor_importaciones2 = 0;
                        let valor_exportaciones2 = 0;
                        //let idejemplo = 100;
                        //const nodoA;

                        response.forEach((data) => {
                            const {
                                feP_ID,
                                x,
                                y,
                                feP_Nombre_sin_espacios,
                                valor,
                                width,
                                height,
                                infoDataImp,
                                infoDataExp,
                                valor_importaciones,
                                valor_exportaciones,
                                tooltipPos,
                                año,
                            } = data;

                            contador = contador + 1;
                            contadorTotal = contadorTotal + data.valor;
                            contadorImp = contadorImp + valor_importaciones;
                            contadorExp = contadorExp + valor_exportaciones;

                            // Crear una copia del objeto original
                            const newData = { ...data };
                            valor2 = contadorTotal;
                            valor_importaciones2 = contadorImp;
                            valor_exportaciones2 = contadorExp;
                            newData.cont = contador;
                            newData.valorOfertaTotal = 20;
                            id.push(feP_ID);
                            valorfep.push(valor);

                            console.log("Contador: ", newData.cont);
                            console.log("Valor: ", newData.valor2);
                            // Asignar un nuevo valor dinámico a la propiedad nombreNodo en la copia

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { cont, valorOfertaTotal } = newData;

                            const imagen_sin_espacios = getImageFromNombre(
                                feP_Nombre_sin_espacios
                            );
                            const color = getColorFromNombre(feP_Nombre_sin_espacios); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            const nuevoNodo = new Nodo(
                                feP_ID,
                                x,
                                y,
                                feP_Nombre_sin_espacios,
                                valor,
                                color,
                                imagen_sin_espacios,
                                width,
                                height,
                                infoDataImp,
                                infoDataExp,
                                valor_importaciones,
                                valor_exportaciones,
                                tooltipPos,
                                cont,
                                año,
                                nodes,
                                enlaces
                            );

                            // Verificar si el nodo ya existe en nodes antes de agregarlo
                            //const existeEnNodes = nodesPrueba.some((existingNode) => existingNode.feP_ID === nuevoNodo4.feP_ID);

                            //if (!existeEnNodes) {
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodo);
                            nodesNodo.push(nuevoNodo);
                            //}

                            //const nodoA = new Nodo(/* parámetros para Nodo */);
                        });

                        const nuevoNodoTotal = new NodoTotal(
                            feP_ID2,
                            valor2,
                            valor_importaciones2,
                            valor_exportaciones2,
                            contador,
                            nodes,
                            enlaces
                        );
                        nodes = nodes.concat(nuevoNodoTotal);
                        nodesNodo.push(nuevoNodoTotal);

                        console.log("Nodos:", nodes);

                        for (let z = 0; z <= 11; z++) {
                            const sourceNode = nodesNodo.find((nodo) => nodo.id === id[z]);
                            const targetNode = nodesNodo.find((nodo) => nodo.id === feP_ID2);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];

                            if (z === 0) {
                                pcolors = ["BagazoCana"];
                            } else if (z === 1) {
                                pcolors = ["Biogas"];
                            } else if (z === 2) {
                                pcolors = ["Carbon"];
                            } else if (z === 3) {
                                pcolors = ["Condensados"];
                            } else if (z === 4) {
                                pcolors = ["EnergiaEolica"];
                            } else if (z === 5) {
                                pcolors = ["EnergiaSolar"];
                            } else if (z === 6) {
                                pcolors = ["GasNatural"];
                            } else if (z === 7) {
                                pcolors = ["Geotermia"];
                            } else if (z === 8) {
                                pcolors = ["EnergiaHidrica"];
                            } else if (z === 9) {
                                pcolors = ["Lena"];
                            } else if (z === 10) {
                                pcolors = ["Nucleoenergia"];
                            } else {
                                pcolors = ["Petroleo"];
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);

                            console.log("Valor colores:", particleColors);
                            const widthlinks = 5;
                            const value = valorfep[z];
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        // Aplicar el evento de clic a todos los nodos en el array
                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        let nombren = [];
        let contadorCombus = 0;
        let contadorPlant = 0;
        let contadorEl = 0;

        function sector() {
            //Nodos Sectores
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodossectores",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Sectores", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let contador = 0;
                        let contadorPet = 0;
                        let contval = [];
                        let z = 0;
                        let l = 0;
                        let contadorCoqnom;
                        let contadorRefnom;
                        let contadorRef = 0;
                        let contadorCoq = 0;
                        let contadorPlantnom;
                        let contnom = [];
                        let yvalue = [];
                        let idvalue = [];
                        let nomvalue = [];
                        let imagenes = [];

                        response.forEach((data) => {
                            const { sectorID, sector_Nombre_SE, tipo_SE, valor } = data;

                            if (tipo_SE === "Sector petróleo y gas") {
                                contadorPet = contadorPet + valor;
                                if (
                                    sector_Nombre_SE ===
                                    "Otros productos , no definidos (idustria carbón y bagazo de caña) y otras ramas"
                                ) {
                                    contnom.push("Otros productos");
                                } else {
                                    contnom.push(sector_Nombre_SE);
                                }
                                contval.push(valor);
                            } else {
                                contadorEl = contadorEl + valor;
                                contnom.push(sector_Nombre_SE);
                                contval.push(valor);
                            }

                            if (sector_Nombre_SE === "Coquizadoras") {
                                contadorCoqnom = sector_Nombre_SE;
                                contadorCoq = valor;
                                contadorCombus = contadorCombus + valor;
                            }
                            if (sector_Nombre_SE === "Refinerias y despuntadoras") {
                                contadorRefnom = sector_Nombre_SE;
                                contadorRef = valor;
                                contadorCombus = contadorCombus + valor;
                            }
                            if (sector_Nombre_SE === "Plantas de gas y fraccionadoras") {
                                contadorPlantnom = sector_Nombre_SE;
                                contadorPlant = valor;
                            }

                            contador = contador + 1;

                            const newData = { ...data };
                            newData.idnuevo = sectorID + 220;
                            newData.cont = contador;
                            newData.pet = contadorPet;
                            newData.el = contadorEl;
                            newData.tam = z;
                            newData.tam2 = l;

                            // Destructurar el nuevo objeto con las nuevas propiedades

                            const { idnuevo, cont, pet, el, tam, tam2 } = newData;

                            const color = getColorFromNombre(tipo_SE); // Asume que tienes esta función definida
                        });

                        for (let i = 0; i <= 1; i++) {
                            if (i === 0) {
                                idvalue[i] = 200;
                                yvalue[i] = 330;
                                nomvalue[i] = "Sector petróleo y gas";
                                imagenes[i] = "https://cdn.sassoapps.com/img_sankey/s_petroliferos.png";
                            } else {
                                idvalue[i] = 210;
                                yvalue[i] = 620;
                                nomvalue[i] = "Sector eléctrico";
                                imagenes[i] = "https://cdn.sassoapps.com/img_sankey/electricidadi.png";
                            }

                            const nuevoNodoTooltip = new SectorTooltip(
                                idvalue[i],
                                nomvalue[i],
                                yvalue[i],
                                imagenes[i],
                                contadorPet,
                                contadorEl,
                                contnom,
                                contval,
                                nodes,
                                enlaces
                            );

                            //nodes = nodes.concat(nuevoNodoTooltip);
                            nodes = nodes.concat(nuevoNodoTooltip);
                            nodesSector.push(nuevoNodoTooltip);

                            const sourceNode = nodesNodo.find((nodo) => nodo.id === 100);
                            const targetNode = nodesSector.find(
                                (nodo) => nodo.id === idvalue[i]
                            );
                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (i === 0) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorPet;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contadorEl;
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            //svgElement.appendChild(links);

                            enlaces = enlaces.concat(links);
                        }

                        let idn = [];
                        let yn = [];

                        for (let z = 0; z <= 2; z++) {
                            if (z === 0) {
                                idn[z] = 300;
                                yn[z] = 280;
                                nombren[z] = "Combustible";
                            } else if (z === 1) {
                                idn[z] = 301;
                                yn[z] = 420;
                                nombren[z] = "Calor";
                            } else {
                                idn[z] = 302;
                                yn[z] = 640;
                                nombren[z] = "Electricidad";
                            }
                            const nuevoNodoProvision = new NodoProvisionyProduccion(
                                idn[z],
                                yn[z],
                                nombren[z],
                                contadorCombus,
                                contadorCoq,
                                contadorRef,
                                contadorCoqnom,
                                contadorRefnom,
                                contadorPlant,
                                contadorPlantnom,
                                contadorEl,
                                nodes,
                                enlaces
                            );
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodoProvision);
                            nodesSector.push(nuevoNodoProvision);
                            let srce;

                            if (z === 0) {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[0]);
                            } else if (z === 1) {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[0]);
                            } else {
                                srce = nodesSector.find((nodo) => nodo.id === idvalue[1]);
                            }
                            const sourceNodeB = srce;
                            const targetNodeB = nodesSector.find(
                                (nodo) => nodo.id === idn[z]
                            );

                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColorB = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (z === 0) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorCombus;
                            } else if (z === 1) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contadorPlant;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contadorEl;
                            }

                            const particleColorsB = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinksB = 5;
                            const valueB = vl;
                            //const value2 = contadorEl;
                            const curveB = 0;
                            const typeB = "normal";
                            const linksB = new Link(
                                sourceNodeB,
                                targetNodeB,
                                widthlinksB,
                                backgroundColorB,
                                valueB,
                                curveB,
                                particleColorsB,
                                typeB
                            );

                            enlaces = enlaces.concat(linksB);
                        }

                        // Aplicar el evento de clic a todos los nodos en el array
                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        function transformaciones() {
            //Nodos Transformaciones
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodostransformaciones",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Transformaciones", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let yvalue = 30;
                        let cont = 70;
                        let yvalue2 = 120;
                        let contPetro = 0;
                        let contCent = 0;
                        let petro;
                        let cent;
                        let contador = 0;
                        let idsum = [];
                        let idtot = [];
                        let ytot = [];
                        let capa = 0;
                        let capacont = 0;

                        response.forEach((data) => {
                            const {
                                transformacionID,
                                transformacion_Nombre_SE,
                                tipo,
                                valor,
                            } = data;

                            const newData = { ...data };

                            ///Capa extra
                            if (capacont === 0) {
                                for (let f = 0; f <= 2; f++) {
                                    //const { id, x, y, nombre, width, height } = data;

                                    let contid;
                                    let conty;
                                    let contheight;
                                    let contimg;

                                    if (f === 0) {
                                        contid = "Transformacion1";
                                        conty = 310;
                                        contheight = 355;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/co2.png";
                                    } else if (f === 1) {
                                        contid = "Transformacion2";
                                        conty = 670;
                                        contheight = 420;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/rr4.png";
                                    } else {
                                        contid = "Transformacion3";
                                        conty = 60;
                                        contheight = 206;
                                        contimg = "https://cdn.sassoapps.com/img_sankey/co2.png";
                                    }

                                    const idCapa = contid;
                                    const xCapa = 770;
                                    const yCapa = conty;
                                    const nombreCapa = "Transformación";
                                    const colorCapa = getColorFromNombre(nombreCapa);
                                    const widthCapa = 150;
                                    const heightCapa = contheight;
                                    const imgcapa = contimg;

                                    // Crea una nueva instancia de WrapperNode y la retorna
                                    const nuevoCapa = new WrapperNode(
                                        idCapa,
                                        xCapa,
                                        yCapa,
                                        nombreCapa,
                                        colorCapa,
                                        widthCapa,
                                        heightCapa,
                                        imgcapa,
                                        nodes,
                                        enlaces
                                    );

                                    nodes = nodes.concat(nuevoCapa);
                                }
                            }
                            capacont = 1;

                            //Fin de capa extra

                            if (tipo === "Petróleo y gas") {
                                yvalue = yvalue + cont;
                                contPetro = contPetro + valor;
                                petro = tipo;
                                console.log("valor de y:", yvalue);
                            } else {
                                yvalue = yvalue + yvalue2;
                                contCent = contCent + valor;
                                yvalue2 = 70;
                                cent = tipo;
                                console.log("valor de y:", yvalue);
                            }

                            newData.y = yvalue;
                            newData.idfinal = transformacionID + 400;

                            // Destructurar el nuevo objeto con las nuevas propiedades

                            const { y, idfinal } = newData;

                            const color = getColorFromNombre(transformacion_Nombre_SE); // Asume que tienes esta función definida
                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            const nuevoNodoTranformacion = new NodoTransformacion(
                                idfinal,
                                transformacion_Nombre_SE,
                                tipo,
                                valor,
                                color,
                                y,
                                nodes,
                                enlaces
                            );

                            idsum[contador] = idfinal;
                            contador = contador + 1;

                            nodes = nodes.concat(nuevoNodoTranformacion);
                            nodesTransformacion.push(nuevoNodoTranformacion);

                            let srce;

                            for (let z = 0; z <= 1; z++) {
                                if (tipo === "Petróleo y gas" && z === 0) {
                                    srce = nodesSector.find((nodo) => nodo.id === 300);
                                } else if (tipo === "Petróleo y gas" && z === 1) {
                                    srce = nodesSector.find((nodo) => nodo.id === 301);
                                } else {
                                    srce = nodesSector.find((nodo) => nodo.id === 302);
                                    z = 1;
                                }
                                const sourceNode = srce;
                                const targetNode = nodesTransformacion.find(
                                    (nodo) => nodo.id === idfinal
                                );

                                //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                                const backgroundColor = coloresEnergia["FondoNodo"];
                                let pcolors = [];
                                let vl = 0;

                                if (tipo === "Petróleo y gas") {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                } else {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                }

                                const particleColors = pcolors.map(
                                    (key) => coloresEnergia[key]
                                );
                                //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                const widthlinks = 5;
                                const value = valor;
                                //const value2 = contadorEl;
                                const curve = 0;
                                const type = "normal";
                                const links = new Link(
                                    sourceNode,
                                    targetNode,
                                    widthlinks,
                                    backgroundColor,
                                    value,
                                    curve,
                                    particleColors,
                                    type
                                );

                                enlaces = enlaces.concat(links);
                            }
                        });

                        for (let k = 0; k <= 1; k++) {
                            if (k === 0) {
                                idtot[k] = 500;
                                ytot[k] = 165;
                            } else {
                                idtot[k] = 501;
                                ytot[k] = 705;
                            }

                            const nuevoNodoTransTotal = new NodoTransformacionTotal(
                                idtot[k],
                                ytot[k],
                                contPetro,
                                contCent,
                                petro,
                                cent,
                                nodes,
                                enlaces
                            );
                            // Agregar el nuevo nodo a nodes
                            nodes = nodes.concat(nuevoNodoTransTotal);
                            nodesTransformacion.push(nuevoNodoTransTotal);
                        }

                        let srce;

                        for (let v = 0; v <= 13; v++) {
                            const sourceNode = nodesTransformacion.find(
                                (nodo) => nodo.id === idsum[v]
                            );

                            if (v <= 2) {
                                srce = nodesTransformacion.find((nodo) => nodo.id === idtot[0]);
                            } else {
                                srce = nodesTransformacion.find((nodo) => nodo.id === idtot[1]);
                            }

                            const targetNode = srce;

                            //const targetNode2 = nodes.find((nodo)=> nodo.id2 === 210);
                            const backgroundColor = coloresEnergia["FondoNodo"];
                            let pcolors = [];
                            let vl = 0;

                            if (v <= 2) {
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = contPetro;
                            } else {
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = contCent;
                            }

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        let nombrearray = [];
        let dist = 0;
        let rnt = 0;

        function tiposEnergia() {
            //Nodos Tipos Energia
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodostiposenergia",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Tipos Energía", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let cont = 0;
                        let idv = 600;
                        let yvalues = 0;
                        let xvalores = [];
                        let yvalores = [];
                        let nombrecont = [];
                        let ycont = [];
                        let idcont = [];
                        let idarray = [];
                        let capacont = 0;
                        response.forEach((data) => {
                            const {
                                cargaPico,
                                intermitente,
                                cargaBase,
                                gasSeco,
                                gasLP,
                                petrolíferos,
                            } = data;

                            //Capa extra
                            if (capacont === 0) {
                                for (let f = 0; f <= 3; f++) {
                                    //const { id, x, y, nombre, width, height } = data;

                                    let contid;
                                    let conty;
                                    let contheight;
                                    let contx;
                                    let imgimpexp;

                                    if (f === 0) {
                                        contid = "Importacion1";
                                        contx = 800;
                                        conty = 310;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/importacion.png";
                                    } else if (f === 1) {
                                        contid = "Importacion2";
                                        contx = 800;
                                        conty = 670;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/importacion.png";
                                    } else if (f === 2) {
                                        contid = "Exportacion1";
                                        contx = 820;
                                        conty = 310;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/exportacion.png";
                                    } else {
                                        contid = "Exportacion2";
                                        contx = 820;
                                        conty = 670;
                                        contheight = 206;
                                        imgimpexp = "https://cdn.sassoapps.com/img_sankey/exportacion.png";
                                    }

                                    const idCapa = contid;
                                    const xCapa = contx;
                                    const yCapa = conty;
                                    const nombreCapa = "Transformación";
                                    const colorCapa = getColorFromNombre(nombreCapa);
                                    const widthCapa = 150;
                                    const heightCapa = contheight;
                                    const imgcapa = imgimpexp;

                                    // Crea una nueva instancia de WrapperNode y la retorna
                                    const nuevoCapa = new WrapperNode(
                                        idCapa,
                                        xCapa,
                                        yCapa,
                                        nombreCapa,
                                        colorCapa,
                                        widthCapa,
                                        heightCapa,
                                        imgcapa,
                                        nodes,
                                        enlaces
                                    );

                                    nodes = nodes.concat(nuevoCapa);
                                }
                            }
                            capacont = 1;

                            //Fin de capa extra

                            for (let i = 0; i <= 5; i++) {
                                idcont[i] = idv;
                                if (i === 3) {
                                    nombrecont[i] = "Carga Pico";
                                    yvalues = 635;
                                } else if (i === 5) {
                                    nombrecont[i] = "Intermitente";
                                    yvalues = 775;
                                } else if (i === 4) {
                                    nombrecont[i] = "Carga Base";
                                    yvalues = 705;
                                } else if (i === 1) {
                                    nombrecont[i] = "Gas Seco";
                                    nombrearray[i] = "RNT";
                                    yvalues = 170;
                                    yvalores[i] = 687;
                                    xvalores[i] = 1280;
                                    idarray[i] = 710;
                                } else if (i === 2) {
                                    nombrecont[i] = "Gas LP";
                                    nombrearray[i] = "RGD";
                                    yvalues = 240;
                                    yvalores[i] = 687;
                                    xvalores[i] = 1380;
                                    idarray[i] = 720;
                                } else {
                                    nombrecont[i] = "Petrolíferos";
                                    nombrearray[i] = "Distribución";
                                    yvalues = 100;
                                    yvalores[i] = 155;
                                    xvalores[i] = 1360;
                                    idarray[i] = 700;
                                }
                                idv = idv + 1;
                                ycont[i] = yvalues;
                                //yvalues = yvalues + 50;
                            }

                            const newData = { ...data };
                            newData.id = idcont;
                            newData.nombre = nombrecont;
                            newData.y = ycont;

                            idv = idv + 1;
                            cont = cont + 1;

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { id, nombre, y } = newData;

                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            for (let k = 0; k <= 5; k++) {
                                const color = getColorFromNombre(nombre[k]); // Asume que tienes esta función definida
                                const image = getImageFromNombre(nombre[k]);
                                const nuevoNodoTipos = new NodoTiposEnergia(
                                    id[k],
                                    nombre[k],
                                    y[k],
                                    cargaPico,
                                    intermitente,
                                    cargaBase,
                                    gasSeco,
                                    gasLP,
                                    petrolíferos,
                                    color,
                                    image,
                                    nodes,
                                    enlaces
                                );
                                nodes = nodes.concat(nuevoNodoTipos);
                                nodesTipos.push(nuevoNodoTipos);

                                let srce;

                                if (k <= 2) {
                                    srce = nodesTransformacion.find((nodo) => nodo.id === 500);
                                } else {
                                    srce = nodesTransformacion.find((nodo) => nodo.id === 501);
                                }
                                const sourceNode = srce;

                                const targetNode = nodesTipos.find((nodo) => nodo.id === id[k]);

                                const backgroundColor = coloresEnergia["FondoNodo"];
                                let pcolors = [];
                                let vl = 0;

                                if (k === 0) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = petrolíferos;
                                } else if (k === 1) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = gasSeco;
                                } else if (k === 2) {
                                    pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                    vl = gasLP;
                                } else if (k === 3) {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = cargaPico;
                                } else if (k === 4) {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = cargaBase;
                                } else {
                                    pcolors = [
                                        "Nucleoenergia",
                                        "BagazoCana",
                                        "Biogas",
                                        "EnergiaEolica",
                                        "EnergiaSolar",
                                        "Geotermia",
                                        "EnergiaHidrica",
                                        "Lena",
                                    ];
                                    vl = intermitente;
                                }

                                const particleColors = pcolors.map(
                                    (key) => coloresEnergia[key]
                                );
                                //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                const widthlinks = 5;
                                const value = vl;
                                //const value2 = contadorEl;
                                const curve = 0;
                                const type = "normal";
                                const links = new Link(
                                    sourceNode,
                                    targetNode,
                                    widthlinks,
                                    backgroundColor,
                                    value,
                                    curve,
                                    particleColors,
                                    type
                                );

                                enlaces = enlaces.concat(links);
                            }

                            dist = petrolíferos + gasSeco + gasLP;
                            rnt = cargaPico + cargaBase + intermitente;
                        });
                        for (let z = 0; z <= 2; z++) {
                            const color2 = getColorFromNombre(nombrearray[z]); // Asume que tienes esta función definida
                            const image2 = getImageFromNombre(nombrearray[z]);
                            const nuevoNodoDist = new NodoDistribucion(
                                idarray[z],
                                nombrearray[z],
                                yvalores[z],
                                xvalores[z],
                                dist,
                                rnt,
                                color2,
                                image2,
                                nodes,
                                enlaces
                            );
                            nodes = nodes.concat(nuevoNodoDist);
                            nodesTipos.push(nuevoNodoDist);
                        }

                        let srce;
                        let trgt;
                        let pcolors = [];
                        let vl = 0;

                        for (let b = 0; b <= 6; b++) {
                            if (b === 0) {
                                srce = nodesTipos.find((nodo) => nodo.id === 600);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 1) {
                                srce = nodesTipos.find((nodo) => nodo.id === 601);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 2) {
                                srce = nodesTipos.find((nodo) => nodo.id === 602);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[0]);
                                pcolors = ["Carbon", "Condensados", "GasNatural", "Petroleo"];
                                vl = dist;
                            } else if (b === 3) {
                                srce = nodesTipos.find((nodo) => nodo.id === 603);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else if (b === 4) {
                                srce = nodesTipos.find((nodo) => nodo.id === 604);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else if (b === 5) {
                                srce = nodesTipos.find((nodo) => nodo.id === 605);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            } else {
                                srce = nodesTipos.find((nodo) => nodo.id === idarray[1]);
                                trgt = nodesTipos.find((nodo) => nodo.id === idarray[2]);
                                pcolors = [
                                    "Nucleoenergia",
                                    "BagazoCana",
                                    "Biogas",
                                    "EnergiaEolica",
                                    "EnergiaSolar",
                                    "Geotermia",
                                    "EnergiaHidrica",
                                    "Lena",
                                ];
                                vl = rnt;
                            }
                            const sourceNode = srce;

                            const targetNode = trgt;

                            const backgroundColor = coloresEnergia["FondoNodo"];

                            const particleColors = pcolors.map((key) => coloresEnergia[key]);
                            //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                            const widthlinks = 5;
                            const value = vl;
                            //const value2 = contadorEl;
                            const curve = 0;
                            const type = "normal";
                            const links = new Link(
                                sourceNode,
                                targetNode,
                                widthlinks,
                                backgroundColor,
                                value,
                                curve,
                                particleColors,
                                type
                            );

                            enlaces = enlaces.concat(links);
                        }

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        function usosFinales() {
            //Nodos Usos finales
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/Sankey/nodosusofinal",
                    type: "POST",
                    data: JSON.stringify(dataToSend),
                    contentType: "application/json",
                    success: function (response) {
                        // Manejo de la respuesta
                        console.log("Estructura Uso Final", response);
                        // Asegúrate de que el contenedor SVG esté vacío antes de añadir nuevos elementos

                        let cont = 0;
                        let idv = 800;
                        let newid = 0;
                        let yvalues = 325;
                        let yvalores = [];
                        let nombrecont = [];
                        let nombrearray = [];
                        let ycont = [];
                        let idcont = [];
                        let idarray = [];
                        let vl = 0;
                        let totalfinal = 0;
                        response.forEach((data) => {
                            const {
                                hogares,
                                transporte,
                                serPubCom,
                                agricultura,
                                industrial,
                                sectorEnergia,
                                hogares_Co2,
                                transporte_Co2,
                                serPubCom_Co2,
                                agricultura_Co2,
                                industrial_Co2,
                                sectorEnergia_Co2,
                                año,
                            } = data;

                            for (let i = 0; i <= 5; i++) {
                                idcont[i] = idv;
                                if (i === 3) {
                                    nombrecont[i] = "Agricultura";
                                    vl = agricultura;
                                } else if (i === 5) {
                                    nombrecont[i] = "Sector Energía";
                                    vl = sectorEnergia;
                                } else if (i === 4) {
                                    nombrecont[i] = "Industrial";
                                    vl = industrial;
                                } else if (i === 1) {
                                    nombrecont[i] = "Transporte";
                                    vl = transporte;
                                } else if (i === 2) {
                                    nombrecont[i] = "Serv. Púb. y Com.";
                                    vl = serPubCom;
                                } else {
                                    nombrecont[i] = "Hogares";
                                    vl = hogares;
                                }
                                idv = idv + 1;
                                ycont[i] = yvalues;
                                yvalues = yvalues + 70;
                                //yvalues = yvalues + 50;
                            }

                            yvalues = yvalues + 70;

                            cont = cont + 1;

                            const newData = { ...data };
                            newData.newid = idcont;
                            newData.newy = ycont;
                            newData.newnombre = nombrecont;

                            // idv = idv + 1;
                            cont = cont + 1;

                            // Destructurar el nuevo objeto con las nuevas propiedades
                            const { newid, newnombre, newy } = newData;

                            //const color2 = getColorFromNombre(feP_Nombre_sin_espacios2); // Asume que tienes esta función definida
                            // Crea una nueva instancia de Nodo y la añade al SVG
                            for (let k = 0; k <= 5; k++) {
                                const color = getColorFromNombre(newnombre[k]); // Asume que tienes esta función definida
                                const image = getImageFromNombre(newnombre[k]);
                                const nuevoNodoFinal = new NodoUsoFinal(
                                    newid[k],
                                    newnombre[k],
                                    newy[k],
                                    hogares,
                                    transporte,
                                    serPubCom,
                                    agricultura,
                                    industrial,
                                    sectorEnergia,
                                    hogares_Co2,
                                    transporte_Co2,
                                    serPubCom_Co2,
                                    agricultura_Co2,
                                    industrial_Co2,
                                    sectorEnergia_Co2,
                                    año,
                                    color,
                                    image,
                                    nodes,
                                    enlaces
                                );
                                nodes = nodes.concat(nuevoNodoFinal);
                                nodesUso.push(nuevoNodoFinal);

                                let srce;
                                let pcolors = [];

                                console.log("Nodos de tipos:", nodesTipos);

                                for (let z = 0; z <= 2; z++) {
                                    if (z === 0) {
                                        srce = nodesTipos.find((nodo) => nodo.id === 700);
                                        pcolors = [
                                            "Carbon",
                                            "Condensados",
                                            "GasNatural",
                                            "Petroleo",
                                        ];
                                    } else {
                                        srce = nodesTipos.find((nodo) => nodo.id === 720);
                                        pcolors = [
                                            "Nucleoenergia",
                                            "BagazoCana",
                                            "Biogas",
                                            "EnergiaEolica",
                                            "EnergiaSolar",
                                            "Geotermia",
                                            "EnergiaHidrica",
                                            "Lena",
                                        ];
                                    }
                                    const sourceNode = srce;

                                    const targetNode = nodesUso.find(
                                        (nodo) => nodo.id === newid[k]
                                    );

                                    const backgroundColor = coloresEnergia["FondoNodo"];

                                    const particleColors = pcolors.map(
                                        (key) => coloresEnergia[key]
                                    );
                                    //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                                    const widthlinks = 5;
                                    const value = vl;
                                    //const value2 = contadorEl;
                                    const curve = 0;
                                    const type = "normal";
                                    const links = new Link(
                                        sourceNode,
                                        targetNode,
                                        widthlinks,
                                        backgroundColor,
                                        value,
                                        curve,
                                        particleColors,
                                        type
                                    );

                                    enlaces = enlaces.concat(links);
                                }

                                totalfinal = totalfinal + vl;
                            }
                        });

                        const sourceNode = nodes.find(
                            (nodo) => nodo.id === "nodoUsosFinales"
                        );

                        const targetNode = nodes.find(
                            (nodo) => nodo.id === "Transformación"
                        );

                        const backgroundColor = coloresEnergia["FondoNodo"];

                        let pcolors = [
                            "Carbon",
                            "Condensados",
                            "GasNatural",
                            "Petroleo",
                            "Nucleoenergia",
                            "BagazoCana",
                            "Biogas",
                            "EnergiaEolica",
                            "EnergiaSolar",
                            "Geotermia",
                            "EnergiaHidrica",
                            "Lena",
                        ];

                        const particleColors = pcolors.map((key) => coloresEnergia[key]);
                        //const particleColors2 = coloresEnergia["Condensados", "Carbon", "GasNatural", "EnergiaHidrica", "EnergiaSolar", "EnergiaEolica", "Geotermia", "Biogas", "Lena"];
                        const widthlinks = 5;
                        const value = totalfinal;
                        //const value2 = contadorEl;
                        const curve = 18;
                        const type = "retroalimentacion";
                        const links = new Link(
                            sourceNode,
                            targetNode,
                            widthlinks,
                            backgroundColor,
                            value,
                            curve,
                            particleColors,
                            type
                        );

                        enlaces = enlaces.concat(links);

                        nodes.forEach((nodo) => {
                            nodo.addEventListeners(nodes, enlaces);
                        });

                        resolve(response);
                    },
                    error: function (error) {
                        console.error("Error al obtener nodos Sankey:", error);
                        reject(error);
                    },
                });
            });
        }

        consultaSankey()
            .then(() => obtieneNodosCaja())
            .then(() => nodos())
            .then(() => sector())
            .then(() => transformaciones())
            .then(() => tiposEnergia())
            .then(() => usosFinales())
            .then(() => {
                tablaFinal(yearSelect);
                // Actualizar las tarjetas con el año correcto
                actualizarTarjetasConAño(yearSelect);
            })
            .catch((error) => {
                console.error("Error en la cadena de promesas:", error);
            });

        function tablaFinal(selectedYear) {
            const nivelOrden = {
                "Nivel FEP - Fuentes de Energía Primaria": 1,
                "Nivel FOCAL - Sector Energético": 2,
                "Nivel 1 - Provisión y Producción": 3,
                "Nivel 2 - Transformaciones": 4,
                "Nivel 3 - Tipos de Energía": 5,
                "Nivel 4 - Distribución": 6,
                "Nivel 5 - Uso Final": 7,
            };

            combinedData = [];
            // Realiza la primera solicitud AJAX para obtener los datos de la primera tabla
            $.ajax({
                url: "/Sankey/NodosTablaFep",
                type: "GET",
                datatype: "json",
                success: function (data1) {
                    // Filtra los datos de la primera tabla según el año seleccionado
                    var filteredData1 = data1.filter(function (item) {
                        return item.año == selectedYear;
                    });

                    console.log("Datos filtrados: ", selectedYear);

                    let cont = 0;

                    // Combinar los datos de la primera tabla con los datos agrupados y sumados de la segunda tabla
                    filteredData1.forEach(function (item1) {
                        combinedData.push({
                            año: item1.año,
                            nivel: "Nivel FEP - Fuentes de Energía Primaria",
                            dato: item1.feP_Nombre_sin_espacios,
                            valor: item1.valor,
                        });

                        console.log("Tamaño:", nombren.length);

                        //Tercera columna
                        if (cont === 0) {
                            for (let i = 0; i <= nombren.length - 1; i++) {
                                if (nombren[i] === "Combustible") {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorCombus,
                                    });
                                } else if (nombren[i] === "Calor") {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorPlant,
                                    });
                                } else {
                                    combinedData.push({
                                        año: item1.año,
                                        nivel: "Nivel 1 - Provisión y Producción",
                                        dato: nombren[i],
                                        valor: contadorEl,
                                    });
                                }
                            }
                        }
                        cont = 1;
                    });

                    console.log("Primer Combined: ", combinedData);
                    obtenerDatosTablaSector();
                },
            });

            function obtenerDatosTablaSector() {
                // Realiza la segunda solicitud AJAX para obtener los datos de la otra tabla
                $.ajax({
                    url: "/Sankey/NodosTablaSector",
                    type: "GET",
                    datatype: "json",
                    success: function (data2) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData2 = data2.filter(function (item) {
                            return item.año == selectedYear;
                        });

                        // Crear un objeto para realizar la agrupación y cálculo de sumas
                        var groupedData = {};

                        // Procesar los datos de la segunda tabla
                        filteredData2.forEach(function (item2) {
                            // Verificar si el tipo_SE ya existe en el objeto agrupado
                            if (groupedData[item2.tipo_SE]) {
                                // Si existe, agregar el valor al existente
                                groupedData[item2.tipo_SE].valor += item2.valor;
                            } else {
                                // Si no existe, crear una nueva entrada
                                groupedData[item2.tipo_SE] = {
                                    año: item2.año,
                                    nivel: "Nivel FOCAL - Sector Energético",
                                    dato: item2.tipo_SE,
                                    valor: item2.valor,
                                };
                            }
                        });

                        // Convertir el objeto agrupado en un array para combinar con los datos de la primera tabla
                        combinedData = combinedData.concat(Object.values(groupedData));

                        obtenerDatosTablaTransformacion();
                    },
                });
            }

            function obtenerDatosTablaTransformacion() {
                $.ajax({
                    url: "/Sankey/NodosTablaTransformacion",
                    type: "GET",
                    datatype: "json",
                    success: function (data3) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData3 = data3.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        filteredData3.forEach(function (item3) {
                            combinedData.push({
                                año: item3.año,
                                nivel: "Nivel 2 - Transformaciones",
                                dato: item3.transformacion_Nombre_SE,
                                valor: item3.valor,
                            });
                        });

                        obtenerDatosTablaTipos();
                    },
                });
            }

            function obtenerDatosTablaTipos() {
                $.ajax({
                    url: "/Sankey/NodosTablaTipos",
                    type: "GET",
                    datatype: "json",
                    success: function (data4) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData4 = data4.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        let conta = 0;

                        filteredData4.forEach(function (item4) {
                            let nombre;
                            for (let z = 0; z <= 5; z++) {
                                if (z === 0) {
                                    nombre = "Petrolíferos";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.petrolíferos,
                                    });
                                } else if (z === 1) {
                                    nombre = "Gas Seco";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.gasSeco,
                                    });
                                } else if (z === 2) {
                                    nombre = "Gas LP";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.gasLP,
                                    });
                                } else if (z === 3) {
                                    nombre = "Carga Pico";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.cargaPico,
                                    });
                                } else if (z === 4) {
                                    nombre = "Carga Base";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.cargaBase,
                                    });
                                } else {
                                    nombre = "Intermitente";
                                    combinedData.push({
                                        año: item4.año,
                                        nivel: "Nivel 3 - Tipos de Energía",
                                        dato: nombre,
                                        valor: item4.intermitente,
                                    });
                                }
                            }

                            //Sexta columna
                            if (conta === 0) {
                                for (let i = 0; i <= nombrearray.length - 1; i++) {
                                    if (nombrearray[i] === "Distribución") {
                                        combinedData.push({
                                            año: item4.año,
                                            nivel: "Nivel 4 - Distribución",
                                            dato: nombrearray[i],
                                            valor: dist,
                                        });
                                    } else {
                                        combinedData.push({
                                            año: item4.año,
                                            nivel: "Nivel 4 - Distribución",
                                            dato: nombrearray[i],
                                            valor: rnt,
                                        });
                                    }
                                }
                            }
                            conta = 1;
                        });

                        obtenerDatosTablaUso();
                    },
                });
            }

            function obtenerDatosTablaUso() {
                $.ajax({
                    url: "/Sankey/NodosTablaUso",
                    type: "GET",
                    datatype: "json",
                    success: function (data5) {
                        // Filtra los datos de la segunda tabla según el año seleccionado
                        var filteredData5 = data5.filter(function (item) {
                            return item.año == yearSelect;
                        });

                        filteredData5.forEach(function (item5) {
                            let nombre;
                            for (let z = 0; z <= 5; z++) {
                                if (z === 0) {
                                    nombre = "Hogares";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: parseFloat(item5.hogares).toFixed(1),
                                    });
                                } else if (z === 1) {
                                    nombre = "Transporte";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: parseFloat(item5.transporte).toFixed(1),
                                    });
                                } else if (z === 2) {
                                    nombre = "Servicio Público y Comercial";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: parseFloat(item5.serPubCom).toFixed(1),
                                    });
                                } else if (z === 3) {
                                    nombre = "Agricultura";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: parseFloat(item5.agricultura).toFixed(1),
                                    });
                                } else if (z === 4) {
                                    nombre = "Industrial";
                                    combinedData.push({
                                        año: parseFloat(item5.año).toFixed(1),
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: item5.industrial,
                                    });
                                } else {
                                    nombre = "Sector Energía";
                                    combinedData.push({
                                        año: item5.año,
                                        nivel: "Nivel 5 - Uso Final",
                                        dato: nombre,
                                        valor: parseFloat(item5.sectorEnergia).toFixed(1),
                                    });
                                }
                            }
                        });

                        // Si la tabla DataTable ya está inicializada, destrúyela
                        if ($.fn.DataTable.isDataTable("#Tipos")) {
                            $("#Tipos").DataTable().destroy();
                        }

                        // Ordenar los datos por nivel
                        combinedData.sort((a, b) => {
                            return nivelOrden[a.nivel] - nivelOrden[b.nivel];
                        });

                        // Inicializa el DataTable utilizando la estructura de datos combinada
                        var table_mun = $("#Tipos").DataTable({
                            lengthMenu: [
                                [10, 50, 100, -1],
                                [10, 50, 100, "Todos"],
                            ],
                            dom: "Blfrtip",
                            buttons: [
                                {
                                    extend: "copyHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "excelHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "csvHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                },
                                {
                                    extend: "pdfHtml5",
                                    title: "SNIER-Balance Nacional de Energía",
                                    customize: function (doc) {
                                        doc.styles.tableHeader.color = "#9fa1a4";
                                        doc.defaultStyle.alignment = "center";
                                        doc.styles.tableHeader.fillColor = "#4c1922";
                                    },
                                },
                            ],
                            data: combinedData, // Utiliza la estructura de datos combinada como origen de datos
                            columns: [
                                { data: "año" },
                                { data: "nivel" },
                                { data: "dato" }, // Utiliza el nombre de la tercera columna
                                {
                                    data: "valor",
                                    render: $.fn.dataTable.render.number(",", ".", 1),
                                }, // Utiliza el nombre de la cuarta columna
                            ],
                            language: {
                                url: "//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json",
                            },
                            // Otras opciones como columnDefs, etc.
                        });
                    },
                });
            }
        }

        // Al hacer clic fuera de un nodo
        sankeySvg.addEventListener("click", (evt) => {
            nodes.forEach((node) => (node.group.style.opacity = 1)); // Restaura la opacidad de todos los nodos
            enlaces.forEach((link) => {
                link.pathElement.style.opacity = 1; // Restaura la opacidad del enlace
                if (link.particles) {
                    link.particles.forEach((particle) => {
                        particle.style.opacity = 1; // Restaura la opacidad de las partículas
                    });
                }
            });
        });
    });

    function ajustarAlturaContenedorAdicional(nombre, año) {
        // Ocultar la sección de gráficos
        var contenedorAdicional = document.getElementById("highchartsContainer");
        var contenedorAdicional2 = document.getElementById("playButton");
        var contenedorAdicional3 = document.getElementById("yearSlider");
        var contenedorAdicional4 = document.getElementById("newChartContainer");
        contenedorAdicional.style.display = "block"; // Mostrar el contenedor
        contenedorAdicional2.style.display = "block"; // Mostrar el contenedor
        contenedorAdicional3.style.display = "block"; // Mostrar el contenedor
        contenedorAdicional4.style.display = "block"; // Mostrar el contenedor
        contenedorAdicional.style.height = "4000px";
        contenedorAdicional.scrollIntoView({
            behavior: "smooth",
            block: "end",
            inline: "nearest",
        });

        // Mostrar botón y slider
        $("#playButton, #yearSlider").show();

        $(document).ready(function () {
            // Obtener datos para el gráfico de Highcharts
            $.ajax({
                url: "/Sankey/nodosgrafica",
                type: "GET",
                contentType: "application/json",
                success: function (response) {
                    if (nombre === "S.P.C") {
                        nombre = "Serv. Púb. y Com.";
                    } else if (nombre === "Energía") {
                        nombre = "Sector Energía";
                    }

                    //const { usoFinalID, usoFinal_Nombre, consumoID, consumo_Nombre, año, consumoUF_Valor} = response;
                    let nuevonom;
                    let nuevonom2;
                    if (nombre === "Hogares") {
                        nuevonom = "Residencial";
                    } else if (nombre === "Transporte") {
                        nuevonom = "Transporte";
                    } else if (nombre === "Serv. Púb. y Com.") {
                        nuevonom = "Comercial";
                        nuevonom2 = "Público";
                    } else if (nombre === "Agricultura") {
                        nuevonom = "Agropecuario";
                    } else if (nombre === "Industrial") {
                        nuevonom = "Industrial";
                    } else if (nombre === "Sector Energía") {
                        nuevonom = "Sector Energía";
                    }

                    console.log("Nuevo nombre:", nuevonom);

                    // Manejo de la respuesta
                    console.log("Estructura Graficas", response);

                    // Crear un conjunto único de años
                    var uniqueYears = [...new Set(response.map((item) => item.año))];

                    // Mapear los valores correspondientes para cada año
                    var seriesData = uniqueYears.map((year) => {
                        var yearData = response.filter((item) => item.año === year);
                        var consumoSum = yearData.reduce(
                            (sum, item) => sum + item.consumo_Nombre_SE,
                            0
                        );
                        return {
                            name: year.toString(),
                            y: consumoSum,
                        };
                    });

                    // Declarar chart fuera de Highcharts.chart
                    var chart;
                    var chart2;

                    // Temporizador para la animación del tiempo
                    var animationInterval;

                    // Variable para el índice del año actual
                    var currentYearIndex = 0;

                    var playButton = $("#playButton");

                    // Función para manejar el clic en el botón de reproducción
                    function toggleAnimation() {
                        console.log("Toggle Animation Function Called");
                        if (animationInterval) {
                            clearInterval(animationInterval);
                            animationInterval = null;
                            playButton.html(
                                '<img src="https://cdn.sassoapps.com/img_sankey/play.png" alt="Play" style="width: 30%; height: 50%; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">'
                            ); // Cambiar a símbolo de Play
                        } else {
                            playButton.html(
                                '<img src="https://cdn.sassoapps.com/img_sankey/pause.png" alt="Pause" style="width: 30%; height: 50%; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">'
                            ); // Cambiar a símbolo de Pause
                            animationInterval = setInterval(updateYear, 300);
                        }
                    }

                    // Desvincular cualquier manejador de eventos click existente
                    playButton.off("click");

                    // Asigna el evento de clic al botón de reproducción
                    playButton.click(toggleAnimation);

                    // Dentro de la función updateYear, verifica si se debe detener la animación
                    function updateYear() {
                        // Actualizar el año en el eje X
                        chart.xAxis[0].setExtremes(0, currentYearIndex);

                        // Incrementar el índice del año
                        currentYearIndex++;

                        // Detener la animación cuando se alcance el último año
                        if (currentYearIndex >= uniqueYears.length) {
                            clearInterval(animationInterval);
                            animationInterval = null;
                            playButton.html(
                                '<img src="https://cdn.sassoapps.com/img_sankey/play.png" alt="Play" style="width: 30%; height: 50%; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">'
                            );
                            // Reiniciar el índice al principio para el siguiente clic en play
                            currentYearIndex = 0;
                            $("#yearSlider").val(0);
                        }

                        // Actualizar la posición del slider
                        $("#yearSlider").val(currentYearIndex);

                        // Verificar si la animación debe detenerse
                        if (!animationInterval) {
                            playButton.html(
                                '<img src="https://cdn.sassoapps.com/img_sankey/play.png" alt="Play" style="width: 30%; height: 50%; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">'
                            );
                        }
                    }

                    // Fusionar los nombres de consumo
                    var consumosFiltrados = [];
                    if (nuevonom2) {
                        consumosFiltrados = [nuevonom, nuevonom2];
                    } else {
                        consumosFiltrados = [nuevonom];
                    }

                    // Filtrar los datos basándote en los consumos combinados
                    var filteredData = response.filter((item) =>
                        consumosFiltrados.includes(item.usoFinal_Nombre_SE)
                    );

                    // Obtener los valores únicos de consumo_Nombre_SE después de aplicar el filtro
                    var uniqueConsumos = [
                        ...new Set(filteredData.map((item) => item.consumo_Nombre_SE)),
                    ];

                    // Crear las series utilizando solo los valores filtrados
                    var series = uniqueConsumos.map((consumo) => {
                        return {
                            name: consumo,
                            data: uniqueYears.map((year) => {
                                var yearData = filteredData.filter(
                                    (item) =>
                                        item.consumo_Nombre_SE === consumo && item.año === year
                                );
                                // Sumar los valores de consumoUF_Valor
                                var consumoSum = yearData.reduce(
                                    (sum, item) => sum + item.consumoUF_Valor,
                                    0
                                );
                                return consumoSum;
                            }),
                        };
                    });

                    // Crear el gráfico con Highcharts
                    chart = Highcharts.chart("highchartsContainer", {
                        chart: {
                            type: "line",
                        },
                        title: {
                            text: "Consumo de " + nombre + " en (PJ) por Año",
                        },
                        xAxis: {
                            categories: uniqueYears,
                        },
                        yAxis: {
                            title: {
                                text: "Valores de Consumo (PJ)",
                            },
                            labels: {
                                formatter: function () {
                                    // Utiliza el nombre de las categorías como etiquetas en el eje Y
                                    return this.value.toLocaleString();
                                },
                            },
                        },
                        series: series,
                        plotOptions: {
                            series: {
                                animation: {
                                    duration: 300,
                                    easing: "linear",
                                },
                            },
                        },
                        tooltip: {
                            formatter: function () {
                                return (
                                    "<b>" +
                                    this.series.name +
                                    "</b><br/>" +
                                    this.x +
                                    ": " +
                                    parseFloat(this.y).toLocaleString(undefined, {
                                        minimumFractionDigits: 0,
                                        maximumFractionDigits: 3,
                                    }) +
                                    " PJ"
                                );
                            },
                        },
                        navigation: {
                            buttonOptions: {
                                enabled: true,
                            },
                        },
                    });

                    // Barra de control deslizante (slider)
                    var slider = document.getElementById("yearSlider");
                    slider.min = 0;
                    slider.max = uniqueYears.length - 1;
                    slider.step = 1;
                    slider.value = 0;

                    slider.addEventListener("input", function () {
                        currentYearIndex = parseInt(slider.value);
                        chart.xAxis[0].setExtremes(0, currentYearIndex);
                    });

                    // Cuando se modifica el contenido del botón, verifica si debe detenerse la animación
                    playButton.off("click").click(function () {
                        toggleAnimation();
                    });

                    // Aplicar estilo para aumentar el tamaño del emoji
                    playButton.css("font-size", "24px"); // Puedes ajustar el tamaño según tus preferencias

                    // Gráfica 2
                    console.log("Año de grafica 2:", año);

                    // Filtrar los datos según los criterios necesarios
                    var datosFiltrados = response.filter(
                        (item) =>
                            (item.usoFinal_Nombre_SE === nuevonom ||
                                item.usoFinal_Nombre_SE === nuevonom2) &&
                            item.año === año
                    );

                    // Crear un conjunto único de consumos
                    var consumosUnicos = [
                        ...new Set(datosFiltrados.map((item) => item.consumo_Nombre_SE)),
                    ];

                    // Crear las series con los valores de consumoUF_Valor
                    var series2 = [];
                    var acumulado = 0;
                    var categoriasX = [];

                    for (var i = 0; i < consumosUnicos.length; i++) {
                        var consumo = consumosUnicos[i];
                        var consumoData = datosFiltrados.filter(
                            (item) => item.consumo_Nombre_SE === consumo
                        );

                        // Acumular los valores de consumoUF_Valor para cada serie
                        var sumConsumoUF_Valor = consumoData.reduce(
                            (total, item) => total + item.consumoUF_Valor,
                            0
                        );

                        // Agregar los datos acumulados al array de categoríasX
                        categoriasX.push({
                            name:
                                consumo + " (" + sumConsumoUF_Valor.toLocaleString() + " PJ)",
                            low: acumulado,
                            high: acumulado + sumConsumoUF_Valor,
                            color: getColorForConsumo(consumo, nombre), // Agrega una función para obtener el color
                        });

                        // Actualizar el acumulado
                        acumulado += sumConsumoUF_Valor;
                    }

                    // Extraer los nombres de las categorías para utilizarlos en el eje X
                    var nombresCategoriasX = categoriasX.map(
                        (categoria) => categoria.name
                    );

                    // Crear la gráfica con Highcharts
                    Highcharts.chart("newChartContainer", {
                        chart: {
                            type: "columnrange",
                        },
                        title: {
                            text: "Consumo Acumulado por " + nombre + " en " + año + "",
                        },
                        xAxis: {
                            categories: nombresCategoriasX, // Usar los nombres de las categorías como etiquetas en el eje X
                        },
                        yAxis: {
                            title: {
                                text: "Total de Consumo",
                            },
                            labels: {
                                formatter: function () {
                                    // Utiliza el nombre de las categorías como etiquetas en el eje Y
                                    return this.value.toLocaleString();
                                },
                            },
                        },
                        tooltip: {
                            valueSuffix: " PJ", // Ajusta según tus necesidades
                            formatter: function () {
                                return (
                                    "<b>" +
                                    this.point.name +
                                    "</b>: " +
                                    parseFloat(this.point.low).toLocaleString(undefined, {
                                        minimumFractionDigits: 0,
                                        maximumFractionDigits: 3,
                                    }) +
                                    " - " +
                                    parseFloat(this.point.high).toLocaleString(undefined, {
                                        minimumFractionDigits: 0,
                                        maximumFractionDigits: 3,
                                    }) +
                                    " PJ"
                                );
                            },
                        },
                        series: [
                            {
                                data: categoriasX,
                                showInLegend: false, // Esto evita que aparezca en la leyenda
                            },
                        ],
                    });

                    // Mostrar el botón de regresar
                    $("#regresarSankeyButton").show();

                    // Añadir un manejador de eventos para el botón de regresar
                    $("#regresarSankeyButton").click(function () {
                        var contenedorAdicional = document.getElementById(
                            "highchartsContainer"
                        );
                        var contenedorAdicional2 = document.getElementById("playButton");
                        var contenedorAdicional3 = document.getElementById("yearSlider");
                        var contenedorAdicional4 =
                            document.getElementById("newChartContainer");

                        // Ocultar la sección actual de gráficos y controles
                        contenedorAdicional.style.display = "none";
                        contenedorAdicional2.style.display = "none";
                        contenedorAdicional3.style.display = "none";
                        contenedorAdicional4.style.display = "none";

                        // Ocultar el botón de regresar
                        $("#regresarSankeyButton").hide();

                        // Desplazarse de nuevo a la sección de Sankey
                        var sankeySection = document.getElementById("sankeySvg");
                        sankeySection.scrollIntoView({
                            behavior: "smooth",
                            block: "start",
                            inline: "nearest",
                        });
                    });
                },
            });
        });
    }

    //let nodes = [];
    //let enlaces = [];
    //Nodos Excluidos del evento lcik
    const excludedNodes = [800, 801, 802, 803, 804, 805];

    class NodoUsoFinal {
        constructor(
            id,
            nombre,
            y,
            hogares,
            transporte,
            serPubCom,
            agricultura,
            industrial,
            sectorEnergia,
            hogares_Co2,
            transporte_Co2,
            serPubCom_Co2,
            agricultura_Co2,
            industrial_Co2,
            sectorEnergia_Co2,
            año,
            color,
            imgSrc,
            nodes,
            enlaces,
            x = 1517,
            width = 70,
            height = 20,
            co2 = 0,
            imgco2 = "https://cdn.sassoapps.com/img_sankey/co2.png",
            tooltipPosition = "bottom"
        ) {
            this.id = id;
            this.nombre = nombre;
            this.y = y;
            this.hogares = hogares;
            this.transporte = transporte;
            this.serPubCom = serPubCom;
            this.agricultura = agricultura;
            this.industrial = industrial;
            this.sectorEnergia = sectorEnergia;
            this.hogares_Co2 = hogares_Co2;
            this.transporte_Co2 = transporte_Co2;
            this.serPubCom_Co2 = serPubCom_Co2;
            this.agricultura_Co2 = agricultura_Co2;
            this.industrial_Co2 = industrial_Co2;
            this.sectorEnergia_Co2 = sectorEnergia_Co2;
            this.año = año;
            this.x = x;
            this.co2 = co2;
            this.color = color;
            this.imgSrc = imgSrc;
            this.imgco2 = imgco2;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );

            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("id", this.id + "_value");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            if (this.nombre === "Hogares") {
                textValue.textContent = this.hogares.toLocaleString() + " PJ";
                this.co2 = this.hogares_Co2;
            } else if (this.nombre === "Transporte") {
                textValue.textContent = this.transporte.toLocaleString() + " PJ";
                this.co2 = this.transporte_Co2;
            } else if (this.nombre === "Serv. Púb. y Com.") {
                textValue.textContent = this.serPubCom.toLocaleString() + " PJ";
                this.co2 = this.serPubCom_Co2;
            } else if (this.nombre === "Agricultura") {
                textValue.textContent = this.agricultura.toLocaleString() + " PJ";
                this.co2 = this.agricultura_Co2;
            } else if (this.nombre === "Industrial") {
                textValue.textContent = this.industrial.toLocaleString() + " PJ";
                this.co2 = this.industrial_Co2;
            } else {
                textValue.textContent = this.sectorEnergia.toLocaleString() + " PJ";
                this.co2 = this.sectorEnergia_Co2;
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.setAttribute("id", this.id + "_name");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.nombre === "Serv. Púb. y Com.") {
                this.nombre = "S.P.C";
            } else if (this.nombre === "Sector Energía") {
                this.nombre = "Energía";
            }

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("id", this.id + "_image");
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#fff");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    const boldText = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText.setAttribute("font-weight", "bold");
                    boldText.textContent = tooltipInfo.label;
                    // Cambiar el color del texto a rojo, por ejemplo
                    boldText.setAttribute("fill", "#9f2241");
                    text.appendChild(boldText);
                    text.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(text);

                    const textB = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    textB.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    textB.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 10
                    );
                    const boldTextB = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldTextB.setAttribute("font-weight", "bold");
                    boldTextB.textContent = tooltipInfo.value;
                    // Cambiar el color del texto a rojo, por ejemplo
                    boldTextB.setAttribute("fill", "#9f2241");
                    textB.appendChild(boldTextB);
                    textB.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(textB);

                    //Parte 2
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7 + 30
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("fill", "#9f2241"); // Cambiar el color del cuadrado a rojo
                    square.setAttribute("class", "infoBox"); //tooltipBullet
                    tooltip.appendChild(square);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 30
                    );
                    const boldText2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText2.setAttribute("font-weight", "bold");
                    boldText2.textContent = tooltipInfo.label2 + tooltipInfo.value2;
                    text2.appendChild(boldText2);
                    text2.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(text2);
                }

                offsetY += 40;
            }
            group.appendChild(tooltip);
            tooltip.style.visibility = "visible";
        }

        titleMessage(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_title");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_title");
                tooltip.setAttribute("class", "infotitle");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#24201F"); // Color de fondo negro
                rect.setAttribute("stroke", "#BDBCBC"); // Color de borde blanco
                rect.setAttribute("stroke-width", "2"); // Ancho de borde
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.setAttribute("fill", "#BDBCBC");
                    text.textContent = tooltipInfo.label;
                    text.setAttribute("class", "infotitle"); //tooltipText
                    tooltip.appendChild(text);
                }

                offsetY += 40;
            }
            group.appendChild(tooltip);
            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            let titleMessageTimer; // Variable para almacenar el temporizador
            let extraName;

            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseenter", (evt) => {
                // Guardar las coordenadas y dimensiones originales del nodo
                const originalX = this.x;
                const originalY = this.y;
                const originalWidth = this.width;
                const originalHeight = this.height;

                // Mostrar el tooltip
                const tooltipX = originalX + originalWidth + 5; // Posición X a la derecha de la imagen
                const tooltipY = originalY - 27; // Posición Y de la imagen

                // Guardar una referencia al contexto actual
                const self = this;

                // Iniciar el temporizador
                titleMessageTimer = setTimeout(function () {
                    // Llamar a titleMessage
                    self.titleMessage(self.group, {
                        id: self.id,
                        x: self.x + self.width + 5 - 95, // Ajuste de posición
                        y: self.y - 27 - 40, // Ajuste de posición
                        width: 252,
                        height: 30,
                        label: "Click para Ver Consumo Anual e Historico de " + extraName,
                    });
                }, 1000); // Mostrar el mensaje después de 1 segundo

                // Cambiar el color y tamaño del nodo cuando el mouse está encima
                const nodeRect = document.getElementById(this.id + "_rect");
                if (nodeRect) {
                    nodeRect.setAttribute("fill", "#9f2241"); // Cambia al color deseado
                    nodeRect.setAttribute("width", this.width + 2);
                    nodeRect.setAttribute("height", this.height + 2);
                }
                const nodeValue = document.getElementById(this.id + "_value");
                if (nodeValue) {
                    if (this.nombre === "S.P.C") {
                        this.nombre = "Serv. Púb. y Com.";
                        extraName = "S.P.C";
                    } else if (this.nombre === "Energía") {
                        this.nombre = "Sector Energía";
                        extraName = "Energía";
                    } else if (this.nombre === "Hogares") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Transporte") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Agricultura") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Industrial") {
                        extraName = this.nombre;
                    }
                    nodeValue.textContent = "";
                    const boldText = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText.setAttribute("font-weight", "bold");
                    if (this.nombre === "Hogares") {
                        boldText.textContent = this.hogares.toLocaleString() + " PJ";
                    } else if (this.nombre === "Transporte") {
                        boldText.textContent = this.transporte.toLocaleString() + " PJ";
                    } else if (this.nombre === "Serv. Púb. y Com.") {
                        boldText.textContent = this.serPubCom.toLocaleString() + " PJ";
                    } else if (this.nombre === "Agricultura") {
                        boldText.textContent = this.agricultura.toLocaleString() + " PJ";
                    } else if (this.nombre === "Industrial") {
                        boldText.textContent = this.industrial.toLocaleString() + " PJ";
                    } else {
                        boldText.textContent = this.sectorEnergia.toLocaleString() + " PJ";
                    }
                    nodeValue.appendChild(boldText);
                }
                const nodeName = document.getElementById(this.id + "_name");
                if (nodeName) {
                    if (this.nombre === "S.P.C") {
                        this.nombre = "Serv. Púb. y Com.";
                        extraName = "S.P.C";
                    } else if (this.nombre === "Energía") {
                        this.nombre = "Sector Energía";
                        extraName = "Energía";
                    } else if (this.nombre === "Hogares") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Transporte") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Agricultura") {
                        extraName = this.nombre;
                    } else if (this.nombre === "Industrial") {
                        extraName = this.nombre;
                    }
                    nodeName.textContent = "";
                    const boldText = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText.setAttribute("font-weight", "bold");
                    boldText.textContent = this.nombre;
                    boldText.setAttribute("fill", "#9f2241");
                    nodeName.appendChild(boldText);
                }
                const nodeImage = document.getElementById(this.id + "_image");
                if (nodeImage) {
                    nodeImage.setAttribute("width", this.width + 2);
                    nodeImage.setAttribute("height", this.height + 2);
                }

                this.showTooltip(this.group, {
                    id: this.id,
                    x: tooltipX,
                    y: tooltipY,
                    width: 162,
                    height: 60,
                    label: "Emisiones de GEI",
                    value: "(Gases de Efecto Invernadero)",
                    label2: extraName + ": ",
                    value2: this.co2.toLocaleString() + " Gg en CO2e",
                });
            });

            this.group.addEventListener("mouseleave", (evt) => {
                // Cancelar el temporizador cuando el mouse sale del nodo
                clearTimeout(titleMessageTimer);

                // Ocultar el tooltip
                this.hideTooltip(this.id + "_tooltip");
                this.hideTooltip(this.id + "_title");

                // Cambiar el color del nodo de vuelta cuando el mouse sale
                const nodeRect = document.getElementById(this.id + "_rect");
                if (nodeRect) {
                    nodeRect.setAttribute("fill", this.color); // Restaura el color original
                    nodeRect.setAttribute("width", this.width);
                    nodeRect.setAttribute("height", this.height);
                }
                const nodeValue = document.getElementById(this.id + "_value");
                if (nodeValue) {
                    if (this.nombre === "S.P.C") {
                        this.nombre = "Serv. Púb. y Com.";
                    } else if (this.nombre === "Energía") {
                        this.nombre = "Sector Energía";
                    }
                    nodeValue.textContent = "";
                    if (this.nombre === "Hogares") {
                        nodeValue.textContent = this.hogares.toLocaleString() + " PJ";
                    } else if (this.nombre === "Transporte") {
                        nodeValue.textContent = this.transporte.toLocaleString() + " PJ";
                    } else if (this.nombre === "Serv. Púb. y Com.") {
                        nodeValue.textContent = this.serPubCom.toLocaleString() + " PJ";
                    } else if (this.nombre === "Agricultura") {
                        nodeValue.textContent = this.agricultura.toLocaleString() + " PJ";
                    } else if (this.nombre === "Industrial") {
                        nodeValue.textContent = this.industrial.toLocaleString() + " PJ";
                    } else {
                        nodeValue.textContent = this.sectorEnergia.toLocaleString() + " PJ";
                    }
                }
                const nodeName = document.getElementById(this.id + "_name");
                if (nodeName) {
                    if (this.nombre === "S.P.C") {
                        this.nombre = "Serv. Púb. y Com.";
                    } else if (this.nombre === "Energía") {
                        this.nombre = "Sector Energía";
                    }
                    nodeName.textContent = "";
                    nodeName.textContent = this.nombre;
                }
                const nodeImage = document.getElementById(this.id + "_image");
                if (nodeImage) {
                    nodeImage.setAttribute("width", this.width);
                    nodeImage.setAttribute("height", this.height);
                }
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            } else {
                this.group.addEventListener("click", () => {
                    ajustarAlturaContenedorAdicional(this.nombre, this.año);
                    console.log("Se ha dado click al nodo: ", this.nombre);
                });
            }
        }
    }

    class NodoDistribucion {
        constructor(
            id,
            nombre,
            y,
            x,
            dist,
            rnt,
            color,
            imgSrc,
            nodes,
            enlaces,
            width = 70,
            height = 70,
            tooltipPosition = "bottom"
        ) {
            this.id = id;
            this.nombre = nombre;
            this.y = y;
            this.x = x;
            this.dist = dist;
            this.rnt = rnt;
            this.color = color;
            this.imgSrc = imgSrc;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            let f = 0;
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );

            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );
            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            if (this.nombre === "Distribución") {
                textValue.textContent = this.dist.toLocaleString() + " PJ";
            } else {
                textValue.textContent = this.rnt.toLocaleString() + " PJ";
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    const square2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7 + 20
                    );
                    square2.setAttribute("width", 6);
                    square2.setAttribute("height", 6);
                    square2.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square2);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 20
                    );
                    text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                    text2.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text2);
                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }

            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(this.group, {
                    id: this.id,
                    x: this.x,
                    y: this.y + this.height + 5,
                    width: this.width,
                    height: 50,
                    label: "IMP",
                    value: 0,
                    label2: "EXP",
                    value2: 0,
                });
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class NodoTiposEnergia {
        constructor(
            id,
            nombre,
            y,
            cargaPico,
            intermitente,
            cargaBase,
            gasSeco,
            gasLP,
            petroliferos,
            color,
            imgSrc,
            nodes,
            enlaces,
            x = 1160,
            width = 70,
            height = 30,
            tooltipPosition = "bottom"
        ) {
            this.id = id;
            this.nombre = nombre;
            this.y = y;
            this.cargaPico = cargaPico;
            this.intermitente = intermitente;
            this.cargaBase = cargaBase;
            this.gasSeco = gasSeco;
            this.gasLP = gasLP;
            this.petroliferos = petroliferos;
            this.x = x;
            this.color = color;
            this.imgSrc = imgSrc;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            let f = 0;
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );

            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase

            if (this.nombre === "Carga Pico") {
                textValue.textContent = this.cargaPico.toLocaleString() + " PJ";
            } else if (this.nombre === "Intermitente") {
                textValue.textContent = this.intermitente.toLocaleString() + " PJ";
            } else if (this.nombre === "Carga Base") {
                textValue.textContent = this.cargaBase.toLocaleString() + " PJ";
            } else if (this.nombre === "Gas Seco") {
                textValue.textContent = this.gasSeco.toLocaleString() + " PJ";
            } else if (this.nombre === "Gas LP") {
                textValue.textContent = this.gasLP.toLocaleString() + " PJ";
            } else {
                textValue.textContent = this.petroliferos.toLocaleString() + " PJ";
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);
                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }

            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            if (this.id <= 602) {
                const sumaTotal = this.petroliferos + this.gasSeco + this.gasLP;
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x,
                        y: 275,
                        width: this.width + 40,
                        height: 30,
                        label: "SUMA TOTAL",
                        value: sumaTotal.toLocaleString(),
                    });
                });
            } else {
                const sumaTotal = this.cargaPico + this.cargaBase + this.intermitente;
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x,
                        y: 810,
                        width: this.width + 40,
                        height: 30,
                        label: "SUMA TOTAL",
                        value: sumaTotal.toLocaleString(),
                    });
                });
            }

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class NodoTransformacionTotal {
        constructor(
            id,
            y,
            contPetro,
            contCent,
            petro,
            cent,
            nodes,
            enlaces,
            x = 980,
            color = getColorFromNombre(petro),
            imgSrc = getImageFromNombre(petro),
            imgSrc2 = getImageFromNombre(cent),
            width = 70,
            height = 30,
            tooltipPosition = "bottom"
        ) {
            this.contPetro = contPetro;
            this.contCent = contCent;
            (this.petro = petro), (this.cent = cent), (this.nodes = nodes);
            this.enlaces = enlaces;
            this.id = id;
            this.x = x;
            this.y = y;
            this.color = color;
            if (id === 500) {
                this.imgSrc = imgSrc;
            } else {
                this.imgSrc = imgSrc2;
            }
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );
            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            if (this.id === 500) {
                textValue.textContent = this.contPetro.toLocaleString() + " PJ";
            } else {
                textValue.textContent = this.contCent.toLocaleString() + " PJ";
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            if (this.id === 500) {
                textName.textContent = this.petro;
            } else {
                textName.textContent = this.cent;
            }
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    const square2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7 + 20
                    );
                    square2.setAttribute("width", 6);
                    square2.setAttribute("height", 6);
                    square2.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square2);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 20
                    );
                    text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                    text2.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text2);

                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }

            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(this.group, {
                    id: this.id,
                    x: this.x,
                    y: this.y + this.height + 5,
                    width: this.width,
                    height: 50,
                    label: "IMP",
                    value: 0, //Ejemplo
                    label2: "EXP",
                    value2: 0, //Ejemplo
                });
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class NodoTransformacion {
        constructor(
            id,
            nombre,
            tipo,
            valor,
            color,
            y,
            nodes,
            enlaces,
            x = 810,
            imgSrc = getImageFromNombre(nombre),
            width = 70,
            height = 20,
            imgflecha = "https://cdn.sassoapps.com/img_sankey/rr4.png",
            imgco2 = "https://cdn.sassoapps.com/img_sankey/co2.png",
            tooltipPosition = "bottom" // Valor por defecto es 'bottom'
        ) {
            this.id = id;
            this.x = x;
            this.y = y;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.nombre = nombre;
            this.valor = valor;
            this.color = color;
            this.imgSrc = imgSrc;
            this.imgflecha = imgflecha;
            this.imgco2 = imgco2;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );

            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            textValue.textContent = this.valor.toLocaleString() + " PJ";

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");
            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    const square2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 7
                    );
                    square2.setAttribute("width", 6);
                    square2.setAttribute("height", 6);
                    square2.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square2);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 1.5
                    );
                    text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                    text2.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text2);
                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }
            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            if (this.id <= 408) {
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x - this.width - 5,
                        y: this.y - 40,
                        width: this.width,
                        height: 50,
                        label: "IMP",
                        value: 0, //Ejemplo
                        label2: "EXP",
                        value2: 0, //Ejemplo
                    });
                });
            } else {
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x - this.width - 5 + 50,
                        y: this.y - 50,
                        width: this.width,
                        height: 50,
                        label: "IMP",
                        value: 0, //Ejemplo
                        label2: "EXP",
                        value2: 0, //Ejemplo
                    });
                });
            }

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class NodoProvisionyProduccion {
        constructor(
            id,
            y,
            nombre,
            contadorCombus,
            contadorCoq,
            contadorRef,
            contadorCoqnom,
            contadorRefnom,
            contadorPlant,
            contadorPlantnom,
            contadorEl,
            nodes,
            enlaces,
            x = 590, //630
            color = getColorFromNombre(nombre),
            imgSrc = "",
            width = 70,
            height = 30,
            tooltipPosition = "bottom"
        ) {
            this.contadorCombus = contadorCombus;
            this.contadorCoq = contadorCoq;
            this.contadorRef = contadorRef;
            this.contadorCoqnom = contadorCoqnom;
            this.contadorRefnom = contadorRefnom;
            this.contadorPlant = contadorPlant;
            this.contadorPlantnom = contadorPlantnom;
            this.contadorEl = contadorEl;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.id = id;
            this.x = x;
            this.y = y;
            this.nombre = nombre;
            this.color = color;
            this.imgSrc = imgSrc;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );
            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            if (this.nombre === "Combustible") {
                textValue.textContent = this.contadorCombus.toLocaleString() + " PJ";
            } else if (this.nombre === "Calor") {
                textValue.textContent = this.contadorPlant.toLocaleString() + " PJ";
            } else {
                textValue.textContent = this.contadorEl.toLocaleString() + " PJ";
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    if (tooltipInfo.id === 300) {
                        const square2 = document.createElementNS(
                            "http://www.w3.org/2000/svg",
                            "rect"
                        );
                        square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                        square2.setAttribute(
                            "y",
                            parseFloat(rect.getAttribute("y")) + offsetY - 7 + 20
                        );
                        square2.setAttribute("width", 6);
                        square2.setAttribute("height", 6);
                        square2.setAttribute("class", "tooltipBullet");
                        tooltip.appendChild(square2);

                        const text2 = document.createElementNS(
                            "http://www.w3.org/2000/svg",
                            "text"
                        );
                        text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                        text2.setAttribute(
                            "y",
                            parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 20
                        );
                        text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                        text2.setAttribute("class", "tooltipText");
                        tooltip.appendChild(text2);
                    }

                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }

            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            if (this.id === 300) {
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x - 50,
                        y: this.y + this.height + 5,
                        width: this.width + 100,
                        height: 50,
                        label: this.contadorCoqnom,
                        value: this.contadorCoq.toLocaleString(),
                        label2: this.contadorRefnom,
                        value2: this.contadorRef.toLocaleString(),
                    });
                });

                this.group.addEventListener("mouseout", (evt) => {
                    this.hideTooltip(this.id + "_tooltip");
                });
            } else if (this.id === 301) {
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x - 30,
                        y: this.y + this.height + 5,
                        width: this.width + 113,
                        height: 30,
                        label: this.contadorPlantnom,
                        value: this.contadorPlant.toLocaleString(),
                    });
                });

                this.group.addEventListener("mouseout", (evt) => {
                    this.hideTooltip(this.id + "_tooltip");
                });
            } else {
                this.group.addEventListener("mouseover", (evt) => {
                    this.showTooltip(this.group, {
                        id: this.id,
                        x: this.x - 25,
                        y: this.y + this.height + 5,
                        width: this.width + 55,
                        height: 30,
                        label: "Sector eléctrico",
                        value: this.contadorEl.toLocaleString(),
                    });
                });

                this.group.addEventListener("mouseout", (evt) => {
                    this.hideTooltip(this.id + "_tooltip");
                });
            }
            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class SectorTooltip {
        constructor(
            id,
            nombre,
            y,
            imgSrc,
            pet,
            el,
            contnom,
            contval,
            nodes,
            enlaces,
            x = 450,
            width = 70,
            height = 70,
            color = "#E0E0E0",
            tooltipPosition = "bottom"
        ) {
            this.nombre = nombre;
            this.pet = pet;
            this.el = el;
            this.contnom = contnom;
            this.contval = contval;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.x = x;
            this.y = y;
            this.imgSrc = imgSrc;
            this.width = width;
            this.height = height;
            this.tooltipPosition = tooltipPosition;
            this.id = id;
            this.color = color;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");
            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );
            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase
            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            if (this.id === 200) {
                textValue.setAttribute("x", this.x + this.width / 2);
                textValue.setAttribute("y", this.y - 10);
                textValue.setAttribute("text-anchor", "middle");
                textValue.setAttribute("class", "nodeValue"); // Añadir la clase
                textValue.textContent = this.pet.toLocaleString() + " PJ";
            } else {
                textValue.setAttribute("x", this.x + this.width / 2);
                textValue.setAttribute("y", this.y - 10);
                textValue.setAttribute("text-anchor", "middle");
                textValue.setAttribute("class", "nodeValue"); // Añadir la clase
                textValue.textContent = this.el.toLocaleString() + " PJ";
            }

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase

            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            //this.group.style.visibility = "hidden";
            //this.group2.style.visibility = "hidden";
            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");
                //tooltip.setAttribute("style", "position: fixed;"); // o "position: fixed;"
                //tooltip.setAttribute("style", "z-index: 9999;"); // o "position: fixed;"

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition

                if (tooltipInfo.id === 200) {
                    rect.setAttribute("width", tooltipInfo.width + 13);
                    rect.setAttribute("height", tooltipInfo.height);
                    rect.setAttribute("x", tooltipInfo.x - 30);
                    rect.setAttribute("y", tooltipInfo.y);
                } else {
                    rect.setAttribute("width", tooltipInfo.width + 65);
                    rect.setAttribute("height", tooltipInfo.height + 40);
                    rect.setAttribute("x", tooltipInfo.x);
                    rect.setAttribute("y", tooltipInfo.y);
                }
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    if (tooltipInfo.id === 200) {
                        let b = 0;
                        for (let z = 0; z < 6; z++) {
                            const square = document.createElementNS(
                                "http://www.w3.org/2000/svg",
                                "rect"
                            );
                            square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                            square.setAttribute(
                                "y",
                                parseFloat(rect.getAttribute("y")) + offsetY + b - 7
                            );
                            square.setAttribute("width", 6);
                            square.setAttribute("height", 6);
                            square.setAttribute("class", "tooltipBullet");
                            tooltip.appendChild(square);

                            const text = document.createElementNS(
                                "http://www.w3.org/2000/svg",
                                "text"
                            );
                            text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                            text.setAttribute(
                                "y",
                                parseFloat(rect.getAttribute("y")) + offsetY + b - 1.5
                            );
                            b = b + 20;
                            text.textContent =
                                tooltipInfo.label[z] +
                                ": " +
                                tooltipInfo.value[z].toLocaleString();
                            text.setAttribute("class", "tooltipText");
                            tooltip.appendChild(text);
                        }
                    } else {
                        let b = 0;
                        for (let z = 6; z < 14; z++) {
                            const square = document.createElementNS(
                                "http://www.w3.org/2000/svg",
                                "rect"
                            );
                            square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                            square.setAttribute(
                                "y",
                                parseFloat(rect.getAttribute("y")) + offsetY + b - 7
                            );
                            square.setAttribute("width", 6);
                            square.setAttribute("height", 6);
                            square.setAttribute("class", "tooltipBullet");
                            tooltip.appendChild(square);

                            const text = document.createElementNS(
                                "http://www.w3.org/2000/svg",
                                "text"
                            );
                            text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                            text.setAttribute(
                                "y",
                                parseFloat(rect.getAttribute("y")) + offsetY + b - 1.5
                            );
                            b = b + 20;
                            text.textContent =
                                tooltipInfo.label[z] +
                                ": " +
                                tooltipInfo.value[z].toLocaleString();
                            text.setAttribute("class", "tooltipText");
                            tooltip.appendChild(text);
                        }
                    }

                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }
            tooltip.style.visibility = "visible";
            //tooltip.style.zIndex = "9999";

            // Mueve el tooltip al final del contenedor SVG
            group.appendChild(tooltip);
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(this.group, {
                    id: this.id,
                    x: this.x,
                    y: this.y + this.height + 5,
                    width: this.width + 100,
                    height: 130,
                    label: this.contnom,
                    value: this.contval,
                });
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    class NodoTotal {
        constructor(
            id,
            valor,
            imp,
            exp,
            cont,
            nodes,
            enlaces,
            contNodo = cont + 1,
            x = 280,
            y = 450,
            nombre = "Oferta Total",
            color = getColorFromNombre(nombre),
            imgSrc = "https://cdn.sassoapps.com/img_sankey/s_recurso.png",
            width = 100,
            height = 100,
            infoDataImp = "IMP TOTAL",
            infoDataExp = "EXP TOTAL",
            tooltipPosition = "bottom"
        ) {
            this.id = id;
            this.x = x;
            this.y = y;
            this.nombre = nombre;
            this.valor = valor;
            this.color = color;
            this.imgSrc = imgSrc;
            this.width = width;
            this.height = height;
            this.infoDataImp = infoDataImp; // Datos adicionales a mostrar
            this.infoDataExp = infoDataExp;
            this.imp = imp;
            this.exp = exp;
            this.tooltipPosition = tooltipPosition;
            this.cont = cont;
            this.contNodo = contNodo;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );

            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase
            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            textValue.textContent = this.valor.toLocaleString() + " PJ";

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase

            this.group.appendChild(textName);

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");
            console.log("showTooltip called for node with id 2:", tooltipInfo.id);
            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    const square2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 7
                    );
                    square2.setAttribute("width", 6);
                    square2.setAttribute("height", 6);
                    square2.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square2);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 1.5
                    );
                    text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                    text2.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text2);
                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }
            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(this.group, {
                    id: this.id,
                    x: this.x,
                    y: this.y + this.height + 5,
                    width: this.width,
                    height: 50,
                    label: this.infoDataImp,
                    value: this.imp.toLocaleString(),
                    label2: this.infoDataExp,
                    value2: this.exp.toLocaleString(),
                });
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    //Clases
    class Nodo {
        constructor(
            id,
            x,
            y,
            nombre,
            valor,
            color,
            imgSrc,
            width,
            height,
            infoDataImp,
            infoDataExp,
            imp,
            exp,
            tooltipPosition = "bottom", // Valor por defecto es 'bottom'
            cont,
            año,
            nodes,
            enlaces
        ) {
            this.id = id;
            this.x = x;
            this.y = y;
            this.nombre = nombre;
            this.valor = valor;
            this.color = color;
            this.imgSrc = imgSrc;
            this.width = width;
            this.height = height;
            this.infoDataImp = infoDataImp; // Datos adicionales a mostrar
            this.infoDataExp = infoDataExp;
            this.imp = imp;
            this.exp = exp;
            this.tooltipPosition = tooltipPosition;
            this.cont = cont;
            this.año = año;
            this.nodes = nodes;
            this.enlaces = enlaces;
            this.draw(document.getElementById("sankeySvg"));
        }
        draw(svgElement) {
            //const svgContainer = document.getElementById("sankeySvg").innerHTML='';
            //svgElement.innerHTML='';
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.group.setAttribute("id", this.id);
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );
            rect.setAttribute("x", this.x);
            rect.setAttribute("y", this.y);
            rect.setAttribute("width", this.width);
            rect.setAttribute("height", this.height);
            rect.setAttribute("fill", this.color);
            rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
            rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
            rect.setAttribute("id", this.id + "_rect");
            rect.setAttribute("class", "nodeRect"); // Añadir la clase

            this.group.appendChild(rect);

            const textValue = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );

            textValue.setAttribute("x", this.x + this.width / 2);
            textValue.setAttribute("y", this.y - 10);
            textValue.setAttribute("text-anchor", "middle");
            textValue.setAttribute("class", "nodeValue"); // Añadir la clase
            textValue.textContent = this.valor.toLocaleString() + " PJ";

            this.group.appendChild(textValue);

            const textName = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );
            textName.setAttribute("x", this.x + this.width / 2);
            textName.setAttribute("y", this.y - 25);
            textName.setAttribute("text-anchor", "middle");
            textName.textContent = this.nombre;
            textName.setAttribute("class", "nodeName"); // Añadir la clase
            this.group.appendChild(textName);

            if (this.cont === 1) {
                const textName2 = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "text"
                );
                textName2.setAttribute("x", this.x + this.width + 175 / 2);
                textName2.setAttribute("y", 40);
                textName2.setAttribute("text-anchor", "middle");
                textName2.textContent = this.año;
                textName2.setAttribute("class", "nodeName"); // Añadir la clase
                this.group.appendChild(textName2);
            }

            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );
                image.setAttribute("x", this.x);
                image.setAttribute("y", this.y);
                image.setAttribute("width", this.width); // Ajustar al ancho del nodo
                image.setAttribute("height", this.height); // Ajustar al alto del nodo
                image.setAttributeNS(
                    "http://www.w3.org/1999/xlink",
                    "xlink:href",
                    this.imgSrc
                );
                this.group.appendChild(image);
            }

            svgElement.appendChild(this.group);

            // Almacenar una referencia a sí mismo
            this.selfReference = this;

            // Llamar a la función para agregar el manejador de eventos
            this.addEventListeners();
        }

        showTooltip(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");
            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square);

                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.textContent = tooltipInfo.label + ": " + tooltipInfo.value;
                    text.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text);

                    const square2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 7
                    );
                    square2.setAttribute("width", 6);
                    square2.setAttribute("height", 6);
                    square2.setAttribute("class", "tooltipBullet");
                    tooltip.appendChild(square2);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY + 20 - 1.5
                    );
                    text2.textContent = tooltipInfo.label2 + ": " + tooltipInfo.value2;
                    text2.setAttribute("class", "tooltipText");
                    tooltip.appendChild(text2);
                    offsetY += 40;
                }
                group.appendChild(tooltip);
            }
            tooltip.style.visibility = "visible";
        }

        hideTooltip(tooltipId) {
            const tooltip = document.getElementById(tooltipId);
            if (tooltip) {
                tooltip.style.visibility = "hidden";
            }
        }

        addEventListeners(nodes, enlaces) {
            // Mantén tus eventos de mouseover y mouseout intactos
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(this.group, {
                    id: this.id,
                    x: this.x - this.width - 5,
                    y: this.y - 18,
                    width: this.width,
                    height: 50,
                    label: this.infoDataImp,
                    value: this.imp.toLocaleString(),
                    label2: this.infoDataExp,
                    value2: this.exp.toLocaleString(),
                });
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(this.id + "_tooltip");
            });

            //Para Resaltar u Opacar los nodos
            // Al hacer clic en un nodo

            // Verificar si el nodo no está en la lista de nodos excluidos antes de asignar el manejador de eventos
            if (!excludedNodes.includes(this.id)) {
                this.group.addEventListener("click", (evt) => {
                    nodes.forEach((n) => (n.group.style.opacity = 0.1));
                    enlaces.forEach((link) => {
                        if (!link.pathElement) {
                            console.error("Enlace sin pathElement:", link);
                        } else {
                            link.pathElement.style.opacity = 0.1; // Opacar link
                            if (link.particles) {
                                link.particles.forEach((particle) => {
                                    particle.style.opacity = 0.1;
                                });
                            }
                        }
                    });

                    // Resaltar el nodo seleccionado
                    this.group.style.opacity = 1;

                    // Llamar a la función de iluminación descendente
                    highlightDescendantsByParticleColors(this, enlaces);
                    evt.stopPropagation();
                });
            }
        }
    }

    function highlightDescendantsByParticleColors(node, links) {
        // Obtener los colores de las partículas del enlace que sale del nodo seleccionado
        const particleColorsToHighlight = [];
        links.forEach((link) => {
            if (link.source === node) {
                particleColorsToHighlight.push(...link.particleColors);
            }
        });

        // Función recursiva para resaltar nodos y enlaces descendentes
        function highlightDescendants(currentNode) {
            links.forEach((link) => {
                if (
                    link.source === currentNode &&
                    particleColorsToHighlight.some((color) =>
                        link.particleColors.includes(color)
                    )
                ) {
                    link.pathElement.style.opacity = 1; // Resalta el enlace
                    link.target.group.style.opacity = 1;

                    // Resalta las partículas
                    if (link.particles) {
                        link.particles.forEach((particle) => {
                            if (
                                particleColorsToHighlight.includes(
                                    particle.getAttribute("fill")
                                )
                            ) {
                                particle.style.opacity = 1;
                            } else {
                                particle.style.opacity = 0.1; // Atenua las partículas no relacionadas
                            }
                        });
                    }

                    // Excepción para el nodo de retroalimentación
                    if (nodeShouldTriggerFeedback(node)) {
                        const feedbackLink = links.find(
                            (link) => link.type === "retroalimentacion"
                        );
                        if (feedbackLink) {
                            feedbackLink.pathElement.style.opacity = 1; // Resaltar link
                            feedbackLink.particles.forEach((particle) => {
                                if (
                                    particleColorsToHighlight.includes(
                                        particle.getAttribute("fill")
                                    )
                                ) {
                                    particle.style.opacity = 1; // Resaltar partículas que coinciden con el color
                                } else {
                                    particle.style.opacity = 0.1; // Atenuar otras partículas
                                }
                            });
                        }
                    }
                    // Llamar recursivamente para el nodo destino
                    highlightDescendants(link.target);
                }
            });
        }

        // Iniciar la iluminación descendente desde el nodo seleccionado
        highlightDescendants(node);
    }

    function nodeShouldTriggerFeedback(node) {
        // Aquí puedes definir qué nodos específicos deberían activar la retroalimentación.
        // En este ejemplo, todos los nodos activan la retroalimentación. Modifícalo según tus necesidades.
        return true;
    }

    class Link {
        static linkCounter = 0;
        constructor(
            source,
            target,
            width,
            color,
            value,
            offset = 0,
            particleColors = ["#888"],
            type = "normal"
        ) {
            this.source = source;
            this.target = target;
            this.width = width;
            this.color = color;
            this.value = value.toString();
            this.unitText = "PJ";
            this.offset = offset;
            this.particleColors = particleColors;
            this.type = type;
            this.draw(document.getElementById("sankeySvg"));
            this.addEventListeners();
        }

        getControlPoints(start, end) {
            const diffX = end[0] - start[0];
            const diffY = end[1] - start[1];
            const halfX = start[0] + diffX / 2;
            return [
                [halfX, start[1]],
                [halfX, start[1] + diffY],
            ];
        }

        createParticleForLink(particleColor, linkPath) {
            //console.log(
            // "Intentando crear partícula para el enlace",
            //linkPath.id
            //); *@
            if (!linkPath) {
                console.warn("linkPath no proporcionado.");
                return;
            }
            const svgNS = "http://www.w3.org/2000/svg";
            const particle = document.createElementNS(svgNS, "circle");
            particle.setAttribute("r", "3");
            particle.setAttribute("fill", particleColor);

            const particlesGroup = document.getElementById("sankeySvg");
            //console.log("particlesGroup: ",particlesGroup);
            if (!particlesGroup) {
                console.error("particlesGroup no encontrado en el DOM.");
                return;
            }
            //console.log("particlesGroup encontrado en el DOM.");

            particlesGroup.appendChild(particle);
            //console.log("Partícula añadida al particlesGroup.");

            const animateMotion = document.createElementNS(svgNS, "animateMotion");
            const animationDuration = 3 + 5 / (this.value + 1);
            animateMotion.setAttribute("dur", `${animationDuration}s`);
            animateMotion.setAttribute("repeatCount", "indefinite");
            const delay = Math.random() * 5;
            animateMotion.setAttribute("begin", `${delay}s`);

            const mpath = document.createElementNS(svgNS, "mpath");
            mpath.setAttributeNS(
                "http://www.w3.org/1999/xlink",
                "xlink:href",
                "#" + linkPath.id
            );
            animateMotion.appendChild(mpath);
            particle.appendChild(animateMotion);
            //console.log("Partícula creada:", particle);
            return particle;
        }

        generateParticles(linkPath) {
            //console.log("Dentro de generateParticles. linkPath:", linkPath);

            if (!linkPath) {
                console.error("linkPath es indefinido.");
                return;
            }

            //console.log("Antes de crear las partículas");
            //console.log("Valor de this.value:", this.value);
            this.value = parseFloat(this.value.match(/\d+/)[0]);

            const particleCount = Math.ceil(Math.log(this.value + 1) * 5);
            this.particles = [];
            for (let i = 0; i < particleCount; i++) {
                //console.log("Intentando crear la partícula número", i);
                let particleColor = this.particleColors[i % this.particleColors.length];
                //console.log("Valor de particleColor", particleColor);
                const particle = this.createParticleForLink(particleColor, linkPath);
                this.particles.push(particle);
            }
            //console.log("Valor de particleCount:", particleCount);

            //console.log("Después de crear las partículas");
        }

        drawParticles(svgElement) {
            this.particleElements = this.particles.map((particle) => {
                //console.log("Añadiendo partícula al SVG", particle);
                svgElement.appendChild(particle);
                return particle;
            });
        }

        draw(svgElement) {
            let startX, startY, endX, endY;
            let controlPoints; // Declaramos la variable controlPoints aquí para que esté disponible en todo el ámbito de la función

            const margin = 0; // Espacio entre el nodo y el inicio de la 'U'
            const uDepth = 680; // Profundidad de la 'U' más pronunciada

            if (!this.source || !this.target) {
                console.error("Link con source o target no definidos:", this);
                return;
            }
            if (
                this.source.x === undefined ||
                this.source.y === undefined ||
                this.target.x === undefined ||
                this.target.y === undefined
            ) {
                console.error("Link con coordenadas no definidas:", this);
                return;
            }
            if (this.type === "retroalimentacion") {
                startX = this.source.x + this.source.width / 2; // Centro del nodo origen
                startY = this.source.y + this.source.height + margin;

                endX = this.target.x + this.target.width / 2; // Centro del nodo destino
                endY = this.target.y + this.target.height + margin;

                // Puntos de control para la 'U'
                controlPoints = [
                    [startX, startY + uDepth],
                    [endX, endY + uDepth],
                ];
            } else {
                startX = this.source.x + this.source.width;
                startY = this.source.y + this.source.height / 2 + this.offset;

                endX = this.target.x;
                endY = this.target.y + this.target.height / 2 + this.offset;

                // Calcula los puntos de control como lo hacías anteriormente
                controlPoints = this.getControlPoints([startX, startY], [endX, endY]);
            }
            const pathD = `M${startX} ${startY} C${controlPoints[0][0]} ${controlPoints[0][1]} ${controlPoints[1][0]} ${controlPoints[1][1]} ${endX} ${endY}`;

            const path = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "path"
            );
            this.originalColor = this.color;
            const uniqueId = `link_${this.source.id}_to_${this.target.id
                }_${Link.linkCounter++}`;
            path.setAttribute("id", uniqueId);

            path.setAttribute("d", pathD);
            path.setAttribute("stroke", this.color);
            path.setAttribute("stroke-width", this.width);
            path.setAttribute("fill", "none");

            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");

            this.pathElement = path;

            svgElement.appendChild(this.group);
            this.group.appendChild(path);

            // Generar partículas después de dibujar el enlace
            this.generateParticles(path);
            this.drawParticles(svgElement);
            //this.animateParticles();
        }

        addEventListeners() {
            this.group.addEventListener("mouseover", (evt) => {
                this.showTooltip(evt);
            });

            this.group.addEventListener("mouseout", (evt) => {
                this.hideTooltip(evt);
            });
        }

        showTooltip(evt) {
            // Crear un rectángulo para el fondo del tooltip
            const rect = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "rect"
            );
            rect.setAttribute("x", evt.clientX);
            rect.setAttribute("y", evt.clientY);
            rect.setAttribute("width", 60); // Puedes ajustar esto según el tamaño del texto
            rect.setAttribute("height", 25); // Puedes ajustar esto según el tamaño del texto
            rect.style.fill = this.color;
            rect.setAttribute("class", "linkTooltipRect"); // Añadido: Clase del rectángulo

            rect.setAttribute("id", "link_tooltip_rect");

            // Crear un triángulo pequeño que apunte al link
            const triangle = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "polygon"
            );
            triangle.setAttribute(
                "points",
                `${evt.clientX},${evt.clientY + 10} ${evt.clientX - 5},${evt.clientY + 5
                } ${evt.clientX - 5},${evt.clientY + 15}`
            );
            triangle.style.fill = this.color;

            triangle.setAttribute("class", "linkTooltipTriangle"); // Añadido: Clase del triángulo
            triangle.setAttribute("id", "link_tooltip_triangle");

            // Crear texto para el tooltip
            const text = document.createElementNS(
                "http://www.w3.org/2000/svg",
                "text"
            );
            text.setAttribute("x", evt.clientX + 30);
            text.setAttribute("y", evt.clientY + 15);
            text.setAttribute("fill", "#fafafa"); // Cambio aquí: Color del texto
            text.setAttribute("class", "linkTooltipText"); // Añadido: Clase del texto
            text.setAttribute("text-anchor", "middle");

            text.setAttribute("id", "link_tooltip_text");
            text.textContent = this.value + " " + this.unitText;

            // Añadir tooltip al SVG
            const svg = document.getElementById("sankeySvg");
            svg.appendChild(rect);
            svg.appendChild(triangle);
            svg.appendChild(text);
        }

        hideTooltip(evt) {
            const svg = document.getElementById("sankeySvg");
            svg.removeChild(document.getElementById("link_tooltip_rect"));
            svg.removeChild(document.getElementById("link_tooltip_triangle"));
            svg.removeChild(document.getElementById("link_tooltip_text"));
        }
        highlight() {
            // Cambia el color o el estilo del enlace para resaltarlo
            this.pathElement.setAttribute("stroke", "red"); // Cambia el color a rojo, por ejemplo
            this.pathElement.setAttribute("stroke-width", "4");
        }

        resetHighlight() {
            // Restablece el color o el estilo del enlace
            this.pathElement.setAttribute("stroke", this.originalColor);
            this.pathElement.setAttribute("stroke-width", this.originalWidth);
        }

        //Generación de particulas
    }

    class WrapperNode extends Nodo {
        constructor(id, x, y, nombre, color, width, height, imgSrc) {
            super(
                id,
                x,
                y,
                nombre,
                null,
                color,
                imgSrc,
                width,
                height,
                null,
                null,
                null,
                null,
                null
            );
        }

        draw(svgElement) {
            this.group = document.createElementNS("http://www.w3.org/2000/svg", "g");
            this.group.setAttribute("id", this.id);

            if (
                this.id === "Importacion1" ||
                this.id === "Importacion2" ||
                this.id === "Exportacion1" ||
                this.id === "Exportacion2"
            ) {
                console.log("Contenido de imagen");
            } else {
                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );
                rect.setAttribute("x", this.x);
                rect.setAttribute("y", this.y);
                rect.setAttribute("width", this.width);
                rect.setAttribute("height", this.height);
                if (this.id === "Transformacion1") {
                    rect.setAttribute("fill", "#FEE879");
                } else if (this.id === "Transformacion2") {
                    rect.setAttribute("fill", "#A8DFE4");
                } else if (this.id === "Transformacion3") {
                    rect.setAttribute("fill", "#FF94A8");
                } else {
                    rect.setAttribute("fill", "none"); // No relleno
                }
                rect.setAttribute("stroke", this.color); // Color del borde
                rect.setAttribute("stroke-dasharray", "4,2"); // Borde punteado
                rect.setAttribute("rx", "5"); // Redondear las esquinas horizontalmente
                rect.setAttribute("ry", "5"); // Redondear las esquinas verticalmente
                rect.setAttribute("id", this.id + "_rect");
                rect.setAttribute("class", "nodeRect"); // Añadir la clase
                this.group.appendChild(rect);
            }

            if (
                this.id === "Transformacion1" ||
                this.id === "Transformacion2" ||
                this.id === "Transformacion3" ||
                this.id === "Importacion1" ||
                this.id === "Importacion2" ||
                this.id === "Exportacion1" ||
                this.id === "Exportacion2"
            ) {
                console.log("Este ID no contiene nombre");
            } else if (this.id === "nodoUsosFinales") {
                const textName = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "text"
                );
                textName.setAttribute("x", this.x + this.width / 2 - 10);
                textName.setAttribute("y", this.y - 10);
                textName.setAttribute("text-anchor", "middle");
                textName.textContent = this.nombre;
                textName.setAttribute("class", "nodeName"); // Añadir la clase
                this.group.appendChild(textName);
            } else {
                const textName = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "text"
                );
                textName.setAttribute("x", this.x + this.width / 2);
                textName.setAttribute("y", this.y - 10);
                textName.setAttribute("text-anchor", "middle");
                textName.textContent = this.nombre;
                textName.setAttribute("class", "nodeName"); // Añadir la clase
                this.group.appendChild(textName);
            }

            var yearSelect = $("#year").val();
            var dataToSend = { yearSelect: yearSelect };
            let total = 0;
            let electricidad = 0;
            let combustible = 0;

            $.ajax({
                url: "/Sankey/nodosusofinal",
                type: "POST",
                data: JSON.stringify(dataToSend),
                contentType: "application/json",
                success: function (response) {
                    response.forEach((data) => {
                        const {
                            hogares_Co2,
                            transporte_Co2,
                            serPubCom_Co2,
                            agricultura_Co2,
                            industrial_Co2,
                            sectorEnergia_Co2,
                            año,
                        } = data;
                        total =
                            hogares_Co2 +
                            transporte_Co2 +
                            serPubCom_Co2 +
                            agricultura_Co2 +
                            industrial_Co2 +
                            sectorEnergia_Co2;
                    });
                },
            });

            $.ajax({
                url: "/Sankey/nodossectores",
                type: "POST",
                data: JSON.stringify(dataToSend),
                contentType: "application/json",
                success: function (response) {
                    response.forEach((data) => {
                        const { valor_Co2, tipo_SE, año } = data;
                        if (tipo_SE === "Sector petróleo y gas") {
                            combustible = valor_Co2;
                        } else {
                            electricidad = valor_Co2;
                        }
                    });
                },
            });
            if (this.imgSrc) {
                const image = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "image"
                );

                if (this.id === "Transformacion1") {
                    image.setAttribute("id", this.id + "_image");
                    image.setAttribute("x", this.x + 85);
                    image.setAttribute("y", this.y - 69);
                    image.setAttribute("width", this.width - 85); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 255); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                    image.addEventListener("click", (evt) => {
                        const x =
                            parseInt(image.getAttribute("x")) +
                            parseInt(image.getAttribute("width")) +
                            5; // Posición X a la derecha de la imagen
                        const y = parseInt(image.getAttribute("y")) + 27; // Posición Y de la imagen

                        this.co2Emisiones(this.group, {
                            id: this.id,
                            x: x,
                            y: y,
                            width: 175,
                            height: 60,
                            label: "Emisiones de GEI",
                            value: "(Gases de Efecto Invernadero)",
                            label2: "Electricidad: ",
                            value2: electricidad.toLocaleString() + " Gg en CO2e",
                        });
                        console.log("Se dió click a Co2 (Transformación 1)");
                    });
                    let titleMessageTimer; // Variable para almacenar el temporizador
                    image.addEventListener("mouseenter", (evt) => {
                        // Guardar una referencia al contexto actual
                        const self = this;

                        // Iniciar el temporizador
                        titleMessageTimer = setTimeout(function () {
                            // Llamar a titleMessage
                            self.titleMessage(self.group, {
                                id: self.id,
                                x: self.x + self.width + 5 - 15, // Ajuste de posición
                                y: self.y - 27 - 5, // Ajuste de posición
                                width: 235,
                                height: 30,
                                label: "Click para Ver las Emisiones de GEI en Electricidad",
                            });
                        }, 1000); // Mostrar el mensaje después de 1 segundo

                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 83);
                            nodeImage.setAttribute("height", this.height - 253);
                        }
                    });
                    image.addEventListener("mouseleave", (evt) => {
                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");

                        // Cambiar el color del nodo de vuelta cuando el mouse sale
                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 85);
                            nodeImage.setAttribute("height", this.height - 255);
                        }
                    });
                    svgElement.addEventListener("click", (evt) => {
                        // Verificar si se hizo clic fuera del área de la imagen que muestra el tooltip
                        if (!evt.target.closest("g")) {
                            // Eliminar todos los tooltips existentes
                            const tooltips = document.querySelectorAll(".infoBox");
                            tooltips.forEach((tooltip) => {
                                tooltip.remove();
                            });
                        }

                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");
                    });
                } else if (this.id === "Transformacion2") {
                    image.setAttribute("x", this.x + 46);
                    image.setAttribute("y", 268);
                    image.setAttribute("width", this.width - 90); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 370); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                } else if (this.id === "Transformacion3") {
                    image.setAttribute("id", this.id + "_image");
                    image.setAttribute("x", this.x + 85);
                    image.setAttribute("y", this.y - 69);
                    image.setAttribute("width", this.width - 85); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 106); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                    image.addEventListener("click", (evt) => {
                        const x =
                            parseInt(image.getAttribute("x")) +
                            parseInt(image.getAttribute("width")) +
                            5; // Posición X a la derecha de la imagen
                        const y = parseInt(image.getAttribute("y")) + 27; // Posición Y de la imagen

                        this.co2Emisiones(this.group, {
                            id: this.id,
                            x: x,
                            y: y,
                            width: 175,
                            height: 60,
                            label: "Emisiones de GEI",
                            value: "(Gases de Efecto Invernadero)",
                            label2: "Petróleo y gas: ",
                            value2: combustible.toLocaleString() + " Gg en CO2e",
                        });
                        console.log("Se dió click a Co2 (Transformación 3)");
                    });
                    let titleMessageTimer; // Variable para almacenar el temporizador
                    image.addEventListener("mouseenter", (evt) => {
                        // Guardar una referencia al contexto actual
                        const self = this;

                        // Iniciar el temporizador
                        titleMessageTimer = setTimeout(function () {
                            // Llamar a titleMessage
                            self.titleMessage(self.group, {
                                id: self.id,
                                x: self.x + self.width + 5 - 95, // Ajuste de posición
                                y: self.y - 27 - 40, // Ajuste de posición
                                width: 243,
                                height: 30,
                                label: "Click para Ver las Emisiones de GEI en Petroleo y Gas",
                            });
                        }, 1000); // Mostrar el mensaje después de 1 segundo

                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 83);
                            nodeImage.setAttribute("height", this.height - 104);
                        }
                    });
                    image.addEventListener("mouseleave", (evt) => {
                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");

                        // Cambiar el color del nodo de vuelta cuando el mouse sale
                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 85);
                            nodeImage.setAttribute("height", this.height - 106);
                        }
                    });
                    svgElement.addEventListener("click", (evt) => {
                        // Verificar si se hizo clic fuera del área de la imagen que muestra el tooltip
                        if (!evt.target.closest("g")) {
                            // Eliminar todos los tooltips existentes
                            const tooltips = document.querySelectorAll(".infoBox");
                            tooltips.forEach((tooltip) => {
                                tooltip.remove();
                            });
                        }
                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");
                    });
                } else if (this.id === "nodoUsosFinales") {
                    image.setAttribute("id", this.id + "_image");
                    image.setAttribute("x", this.x + 80);
                    image.setAttribute("y", this.y - 69);
                    image.setAttribute("width", this.width - 55); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 340); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                    image.addEventListener("click", (evt) => {
                        const x =
                            parseInt(image.getAttribute("x")) +
                            parseInt(image.getAttribute("width") - 102); // Posición X a la derecha de la imagen
                        const y = parseInt(image.getAttribute("y")) - 40; // Posición Y de la imagen

                        this.co2Emisiones(this.group, {
                            id: this.id,
                            x: x,
                            y: y,
                            width: 190,
                            height: 60,
                            label: "Emisiones de GEI",
                            value: "(Gases de Efecto Invernadero)",
                            label2: "TOTAL: ",
                            value2: total.toLocaleString() + " Gg en CO2e",
                        });
                        console.log("Se dió click a Co2 (Usos finales)");
                    });
                    let titleMessageTimer; // Variable para almacenar el temporizador
                    image.addEventListener("mouseenter", (evt) => {
                        // Guardar una referencia al contexto actual
                        const self = this;

                        // Iniciar el temporizador
                        titleMessageTimer = setTimeout(function () {
                            // Llamar a titleMessage
                            self.titleMessage(self.group, {
                                id: self.id,
                                x: self.x + self.width + 5 - 95, // Ajuste de posición
                                y: self.y - 27 - 40, // Ajuste de posición
                                width: 185,
                                height: 30,
                                label: "Click para Ver las Emisiones de GEI Total",
                            });
                        }, 1000); // Mostrar el mensaje después de 1 segundo

                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 53);
                            nodeImage.setAttribute("height", this.height - 338);
                        }
                    });
                    image.addEventListener("mouseleave", (evt) => {
                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");

                        // Cambiar el color del nodo de vuelta cuando el mouse sale
                        const nodeImage = document.getElementById(this.id + "_image");
                        if (nodeImage) {
                            nodeImage.setAttribute("width", this.width - 55);
                            nodeImage.setAttribute("height", this.height - 340);
                        }
                    });
                    svgElement.addEventListener("click", (evt) => {
                        // Verificar si se hizo clic fuera del área de la imagen que muestra el tooltip
                        if (!evt.target.closest("g")) {
                            // Eliminar todos los tooltips existentes
                            const tooltips = document.querySelectorAll(".infoBox");
                            tooltips.forEach((tooltip) => {
                                tooltip.remove();
                            });
                        }

                        // Cancelar el temporizador cuando el mouse sale del nodo
                        clearTimeout(titleMessageTimer);

                        this.hideTooltip(this.id + "_title");
                    });
                } else if (this.id === "Importacion1") {
                    image.setAttribute("x", this.x + 350);
                    image.setAttribute("y", this.y - 330);
                    image.setAttribute("width", this.width - 100); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 100); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                } else if (this.id === "Importacion2") {
                    image.setAttribute("x", this.x + 350);
                    image.setAttribute("y", this.y - 158);
                    image.setAttribute("width", this.width - 100); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 100); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                } else if (this.id === "Exportacion1") {
                    image.setAttribute("x", this.x + 370);
                    image.setAttribute("y", this.y - 330);
                    image.setAttribute("width", this.width - 100); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 100); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                } else if (this.id === "Exportacion2") {
                    image.setAttribute("x", this.x + 370);
                    image.setAttribute("y", this.y - 158);
                    image.setAttribute("width", this.width - 100); // Ajustar al ancho del nodo
                    image.setAttribute("height", this.height - 100); // Ajustar al alto del nodo
                    image.setAttributeNS(
                        "http://www.w3.org/1999/xlink",
                        "xlink:href",
                        this.imgSrc
                    );
                    this.group.appendChild(image);
                }
            }

            svgElement.appendChild(this.group);
        }

        // Sobrescribimos el método addEventListeners para que no haga nada
        addEventListeners() { }

        co2Emisiones(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_tooltip");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_tooltip");
                tooltip.setAttribute("class", "infoBox");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#FFF");
                rect.setAttribute("rx", "5");
                rect.setAttribute("ry", "5");
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    const boldText = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText.setAttribute("font-weight", "bold");
                    boldText.textContent = tooltipInfo.label;
                    // Cambiar el color del texto a rojo, por ejemplo
                    boldText.setAttribute("fill", "#9f2241");
                    text.appendChild(boldText);
                    text.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(text);

                    const textB = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    textB.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    textB.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 10
                    );
                    const boldTextB = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldTextB.setAttribute("font-weight", "bold");
                    boldTextB.textContent = tooltipInfo.value;
                    // Cambiar el color del texto a rojo, por ejemplo
                    boldTextB.setAttribute("fill", "#9f2241");
                    textB.appendChild(boldTextB);
                    textB.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(textB);

                    //Parte 2
                    const square = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "rect"
                    );
                    square.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    square.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 7 + 30
                    );
                    square.setAttribute("width", 6);
                    square.setAttribute("height", 6);
                    square.setAttribute("class", "infoBox"); //tooltipBullet
                    tooltip.appendChild(square);

                    const text2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text2.setAttribute("x", parseFloat(rect.getAttribute("x")) + 20);
                    text2.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5 + 30
                    );
                    const boldText2 = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "tspan"
                    );
                    boldText2.setAttribute("font-weight", "bold");
                    boldText2.textContent = tooltipInfo.label2 + tooltipInfo.value2;
                    text2.appendChild(boldText2);
                    text2.setAttribute("class", "infoBox"); //tooltipText
                    tooltip.appendChild(text2);
                }

                offsetY += 40;
            }
            group.appendChild(tooltip);
        }

        titleMessage(group, tooltipInfo) {
            console.log("showTooltip called for node with id:", tooltipInfo.id);

            let tooltip = document.getElementById(tooltipInfo.id + "_title");

            if (!tooltip) {
                tooltip = document.createElementNS("http://www.w3.org/2000/svg", "g");
                tooltip.setAttribute("id", tooltipInfo.id + "_title");
                tooltip.setAttribute("class", "infotitle");

                const rect = document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect"
                );

                // Posicionamiento basado en tooltipPosition
                rect.setAttribute("x", tooltipInfo.x);
                rect.setAttribute("y", tooltipInfo.y);
                rect.setAttribute("width", tooltipInfo.width);
                rect.setAttribute("height", tooltipInfo.height);
                rect.setAttribute("fill", "#24201F"); // Color de fondo negro
                rect.setAttribute("stroke", "#BDBCBC"); // Color de borde blanco
                rect.setAttribute("stroke-width", "2"); // Ancho de borde
                tooltip.appendChild(rect);

                let offsetY = 20;
                for (let i = 0; i < 1; i++) {
                    const text = document.createElementNS(
                        "http://www.w3.org/2000/svg",
                        "text"
                    );
                    text.setAttribute("x", parseFloat(rect.getAttribute("x")) + 5);
                    text.setAttribute(
                        "y",
                        parseFloat(rect.getAttribute("y")) + offsetY - 1.5
                    );
                    text.setAttribute("fill", "#BDBCBC");
                    text.textContent = tooltipInfo.label;
                    text.setAttribute("class", "infotitle"); //tooltipText
                    tooltip.appendChild(text);
                }

                offsetY += 40;
            }
            group.appendChild(tooltip);
            tooltip.style.visibility = "visible";
        }
    }

    const coloresEnergia = {
        Petroleo: "#1B263B",
        Condensados: "#4CAF50",
        Carbon: "#424242",
        GasNatural: "#64B5F6",
        EnergiaHidrica: "#1976D2",
        EnergiaSolar: "#FFEB3B",
        EnergiaEolica: "#90CAF9",
        Geotermia: "#8D6E63",
        Biogas: "#2E7D32",
        Lena: "#CDDC39",
        Envoltura: "#a9b3ba",
        FondoNodo: "#E0E0E0",
        Nucleoenergia: "#7FFF00",
        BagazoCana: "#FF7F50",
    };

    function getColorForConsumo(consumo, nombre) {
        if (nombre === "Hogares") {
            switch (consumo) {
                case "Gas licuado":
                    return "#2CAFFE";
                case "Gas seco":
                    return "#544FC5";
                case "Energía eléctrica":
                    return "#00E272";
                case "Leña":
                    return "#FE6A35";
                case "Solar":
                    return "#6B8ABC";
                default:
                    return "#FFD733";
            }
        } else if (nombre === "Transporte") {
            switch (consumo) {
                case "Gas licuado":
                    return "#2CAFFE";
                case "Gas seco":
                    return "#544FC5";
                case "Gasolinas y naftas":
                    return "#00E272";
                case "Querosenos":
                    return "#FE6A35";
                case "Diésel":
                    return "#6B8ABC";
                case "Energía eléctrica":
                    return "#D568FB";
                default:
                    return "#FFD733";
            }
        } else if (nombre === "Serv. Púb. y Com.") {
            switch (consumo) {
                case "Gas licuado":
                    return "#2CAFFE";
                case "Gas seco":
                    return "#544FC5";
                case "Diésel":
                    return "#00E272";
                case "Energía eléctrica":
                    return "#FE6A35";
                case "Solar":
                    return "#6B8ABC";
                default:
                    return "#FFD733";
            }
        } else if (nombre === "Agricultura") {
            switch (consumo) {
                case "Gas licuado":
                    return "#2CAFFE";
                case "Diésel":
                    return "#544FC5";
                case "Energía eléctrica":
                    return "#00E272";
                default:
                    return "#FFD733";
            }
        } else if (nombre === "Industrial") {
            switch (consumo) {
                case "Coque de carbón":
                    return "#2CAFFE";
                case "Coque de petróleo":
                    return "#544FC5";
                case "Gas licuado":
                    return "#00E272";
                case "Gas seco":
                    return "#FE6A35";
                case "Gasolinas y naftas":
                    return "#6B8ABC";
                case "Querosenos":
                    return "#D568FB";
                case "Diésel":
                    return "#2EE0CA";
                case "Combustóleo":
                    return "#FA4B42";
                case "Energía eléctrica":
                    return "#FEB56A";
                default:
                    return "#FFD733";
            }
        } else if (nombre === "Sector Energía") {
            switch (consumo) {
                case "Calor":
                    return "#2CAFFE";
                case "Combustible":
                    return "#544FC5";
                case "Electricidad":
                    return "#00E272";
                default:
                    return "#FFD733";
            }
        }
    }

    function getImageFromNombre(nombre) {
        switch (nombre) {
            case "Carbón": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/carboni.png";
            case "Condensados": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/condensadosi.png";
            case "Gas Natural": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/gasnaturali.png";
            case "Petróleo crudo": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/petroleoi.png";
            case "Nucloenergía": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/nuclear.png";
            case "Bagazo de caña": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/sugar-cane.png";
            case "Biogas": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/biogasi.png";
            case "Energía Eólica": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/vientoi.png";
            case "Energía Solar": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/soli.png";
            case "Geoenergía": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/geotermiai.png";
            case "Hidroenergía": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/aguai.png";
            case "Leña": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/fire.png";
            case "Refinerías": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Petrolíferos_Refineria.png";
            case "Coquizadores": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Petrolíferos_Refineria.png";
            case "Complejos gaseros": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Petrolíferos-29.png";
            case "Carboeléctrica": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Carbon.png";
            case "Combustión Interna": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/combin.png";
            case "Ciclo Combinado":
                return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Combustoleo.png";
            case "Térmica convencional":
                return "https://cdn.sassoapps.com/img_sankey/combin.png";
            case "Hidroeléctrica":
                return "https://cdn.sassoapps.com/img_sankey/hidroelec.png";
            case "Fotovoltaica":
                return "https://cdn.sassoapps.com/img_sankey/foto.png";
            case "Turbo Gas": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/gasyvapor.png";
            case "Vapor": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/gasyvapor.png";
            case "Nucleoeléctrica": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Centrales Electricas.png";
            case "Eólica": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Eolica.png";
            case "Geotermoeléctrica": //quitar el ejemplo cuando se tenga la imagen
                return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Geotérmica.png";
            //case "Centrales eléctricas":
            //return "https://cdn.sassoapps.com/img_sankey/Iconos Electricidad_Centrales Electricas.png"
            //case "Petrolíferos":
            //return "https://cdn.sassoapps.com/img_sankey/Iconos Petrolíferos-29.png" *@
            case "Distribución":
                return "https://cdn.sassoapps.com/img_sankey/s_distribucion.png";
            case "RNT":
                return "https://cdn.sassoapps.com/img_sankey/s_rntyrgd.png";
            case "RGD":
                return "https://cdn.sassoapps.com/img_sankey/rnd.png";
            case "Hogares":
                return "https://cdn.sassoapps.com/img_sankey/hogar.png";
            case "Serv. Púb. y Com.":
                return "https://cdn.sassoapps.com/img_sankey/servicio.png";
            case "Público":
                return "https://cdn.sassoapps.com/img_sankey/servicio.png";
            case "Transporte":
                return "https://cdn.sassoapps.com/img_sankey/camioni.png";
            case "Agricultura":
                return "https://cdn.sassoapps.com/img_sankey/agricultura.png";
            case "Industrial":
                return "https://cdn.sassoapps.com/img_sankey/industria.png";
            case "Sector Energía":
                return "https://cdn.sassoapps.com/img_sankey/sectore.png";
        }
    }

    function getColorFromNombre(nombre) {
        switch (nombre) {
            case "Petróleo crudo":
                return coloresEnergia.Petroleo;
            case "Biomasa":
                return coloresEnergia.Biomasa;
            case "Carbón":
                return coloresEnergia.Carbon;
            case "Gas Natural":
                return coloresEnergia.GasNatural;
            case "Hidroenergía":
                return coloresEnergia.EnergiaHidrica;
            case "Energía Solar":
                return coloresEnergia.EnergiaSolar;
            case "Energía Eólica":
                return coloresEnergia.EnergiaEolica;
            case "Geoenergía":
                return coloresEnergia.Geotermia;
            case "Biogas":
                return coloresEnergia.Biogas;
            case "Uranio":
                return coloresEnergia.Uranio;
            case "Condensados":
                return coloresEnergia.Condensados;
            case "Nucloenergía":
                return coloresEnergia.Nucleoenergia;
            case "Bagazo de caña":
                return coloresEnergia.BagazoCana;
            case "Leña":
                return coloresEnergia.Lena;
            // ... Agrega cualquier otro nodo específico aquí
            case "FEP - Fuentes de Energía Primaria":
                return coloresEnergia.Envoltura;
            case "Usos Finales":
                return coloresEnergia.Envoltura;
            case "Transformación":
                return coloresEnergia.Envoltura;
            case "Retroalimentación":
                return coloresEnergia.Envoltura;
            case "Oferta Total":
                return coloresEnergia.FondoNodo;
            default:
                return coloresEnergia.FondoNodo; // Color por defecto
        }
    }
}