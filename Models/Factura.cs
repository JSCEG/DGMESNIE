namespace NSIE.Models
{
    public class Factura
    {
        public int Id { get; set; }  // ID de orden
        public string PdfFileName { get; set; }  // Nombre del archivo PDF

        public string FechaHoraEmision { get; set; }
        public string UUID { get; set; }
        public string Proveedor { get; set; }
        public string RegimenFiscalProveedor { get; set; }
        public string RFCProveedor { get; set; }
        public string Cliente { get; set; }
        public string RFCCliente { get; set; }
        public string RegimenFiscalCliente { get; set; }
        public string UsoCFDI { get; set; }
        public string FormaPago { get; set; }
        public string MetodoPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string ClaveMoneda { get; set; }
        public string Exportacion { get; set; }
        public string FolioSerie { get; set; }
        public string ProductoClave { get; set; }
        public string ProductoCantidad { get; set; }
        public string ProductoUnidad { get; set; }
        public string ProductoDescripcion { get; set; }
        public string ProductoPrecioUnitario { get; set; }
        public string ProductoImporte { get; set; }
        public string ProductoDescuento { get; set; }
        public string Subtotal { get; set; }
        public string IVA { get; set; }
        public string Total { get; set; }
        public string SelloDigitalCFDI { get; set; }
        public string SelloDigitalSAT { get; set; }
        public string CadenaOriginalSAT { get; set; }
        public string EfectoComprobante { get; set; }
        public string CondicionesPago { get; set; }
        public string FechaHoraCertificacion { get; set; }
    }

}
