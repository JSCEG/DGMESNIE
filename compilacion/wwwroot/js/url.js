// Defino fecha de dÃ­as pasados para indicadores financieros no disponibles en fecha actual.
var fecha2 = new Date();
fecha2.setDate(fecha2.getDate() - 4);
fecha2 = fecha2.toJSON().slice(0,10);

// Solicitudes
function  buildUrl(fecha) {
	var API = "6327e9508b0fbe4b2d427f45178cadb5ef3402e5";
	return URL = {
		//'InfoGasBrentWtiGrafico':"http://server.reportediario.cne.cl/server/Boletin/Lista/InfoGasBrentWtiGrafico?fecha="+fecha,
		//'InfoElectricaDemandaMaximaGrafico': "http://server.reportediario.cne.cl/server/Boletin/Lista/InfoElectricaDemandaMaximaGrafico?fecha="+fecha,
		//'InfoElectricaGeneracionProgramada':'http://server.reportediario.cne.cl/server/Boletin/Lista/InfoElectricaGeneracionProgramada?fecha='+fecha,
		//'InfoElectricaCostoMarginal':"http://server.reportediario.cne.cl/server/Boletin/Lista/InfoElectricaCostoMarginal?fecha="+fecha,
		//'Yenes':'http://cne.cloudapi.junar.com/datastreams/invoke/SERIE-HISTO-YEN-2015?auth_key='+API,
		//'UTM':'https://api.desarrolladores.energiaabierta.cl/indicadores-diarios/v1/utm.ajson/?auth_key='+API+'&fecha-inicio='+fecha2,
		//'DOLAR':'https://api.desarrolladores.energiaabierta.cl/indicadores-diarios/v1/dolar.ajson//?auth_key='+API+'&fecha-inicio='+fecha2,
		//'UF':'https://api.desarrolladores.energiaabierta.cl/indicadores-diarios/v1/uf.ajson/?auth_key='+API+'&fecha-inicio='+fecha2,
		//'EURO' : 'https://api.desarrolladores.energiaabierta.cl/indicadores-diarios/v1/euro.ajson/?auth_key='+API+'&fecha-inicio='+fecha2,
		//'WTI' : 'http://cne.cloudapi.junar.com/datastreams/invoke/CRUDE-OIL-NATUR-GAS?auth_key='+API,
		//--Se agrega WTI 07-07-2021
		//'WTI':"http://server.reportediario.cne.cl/server/Boletin/Lista/InfoGasBrentWtiGrafico?fecha="+fecha,
		//'InfoGasInternacional' : 'http://server.reportediario.cne.cl/server/Boletin/Lista/InfoGasInternacional?fecha='+fecha,
		
		
		
		'InfoGasBrentWtiGrafico':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/brent?page=1&limit=2&order=D',
		'InfoElectricaDemandaMaximaGrafico': 'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/dx_programada?page=1&limit=2&order=D',
		'InfoElectricaGeneracionProgramada':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/gx_programada?page=1&limit=2&order=D',
		'InfoElectricaCostoMarginal':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/cmg_esperado?page=1&limit=2&order=D',
		'UTM':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/utm?page=1&limit=2&order=D',
		'DOLAR': 'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/dolar?page=1&limit=2&order=D',
		'UF':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/uf?page=1&limit=2&order=D',
		'EURO' :'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/euro?page=1&limit=2&order=D',
		'WTI':'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/wti?page=1&limit=2&order=D',
		'InfoGasInternacional' : 'https://apim.ea.cne.cl:8244/indicadores_diarios/v1/henry_hub?page=1&limit=2&order=D',
		

	}
}