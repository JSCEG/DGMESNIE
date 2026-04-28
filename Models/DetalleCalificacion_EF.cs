namespace NSIE.Models
{
    public class DetalleCalificacion_EF
    {
        public IEnumerable<CalificacionFinal> Totales_EF { get; set; }
        public IEnumerable<CalificacionFinal_EF> Detalle_EF { get; set; }
        public IEnumerable<CalificacionFinal_EF> Permisos_Autorizados { get; set; }
        public IEnumerable<Parametros_EF> Parametros_EF { get; set; }
        public IEnumerable<DetalleCalificacionEF_Municipio> Totales_EF_MUN { get; set; }

        //public IEnumerable<ResultadoMunicipio> Resultado_Municipio { get; set; }

        public IEnumerable<CalificacionFinal_EF> Detalle_MUN { get; set; }

    }
}
