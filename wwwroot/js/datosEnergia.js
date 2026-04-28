async function generarSankey() {

  const res = await fetch('/SankeySener/GetEnergyData');
  const data = await res.json();

  console.log('Datos de energía recibidos:', data);

  window.energyData = {
      Datos: data.datos.map(padre => ({
          "Nodo Padre": padre.nodoPadre,
          "Nodos Hijo": padre.nodosHijo,
          descripcion: padre.descripcion,
          id_padre: padre.id_padre,
          color: padre.color
      }))
  };

  console.log('Datos de energía adaptados:', window.energyData);

}