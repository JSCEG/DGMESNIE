using System.Collections.Generic;
using System.Text;

namespace NSIE.Models
{
    public class CatalogoBD
    {
        public Dictionary<string, List<string>> Tablas { get; set; }
        public List<Relacion> Relaciones { get; set; }

        public CatalogoBD(Dictionary<string, List<string>> tablas, List<Relacion> relaciones)
        {
            Tablas = tablas;
            Relaciones = relaciones;
        }

        public string ObtenerEstructuraFiltrada(string pregunta)
        {
            var estructuraFiltrada = new StringBuilder("La base de datos contiene las siguientes tablas y columnas relevantes:\n");

            foreach (var tabla in Tablas)
            {
                if (pregunta.ToLower().Contains(tabla.Key.ToLower()))
                {
                    estructuraFiltrada.AppendLine($"- {tabla.Key} ({string.Join(", ", tabla.Value)})");
                }
            }

            estructuraFiltrada.AppendLine("\nLas relaciones entre tablas son:\n");
            foreach (var relacion in Relaciones)
            {
                estructuraFiltrada.AppendLine($"{relacion.TablaHija}.{relacion.ColumnaHija} -> {relacion.TablaPadre}.{relacion.ColumnaPadre}");
            }

            return estructuraFiltrada.ToString();
        }
    }
}







