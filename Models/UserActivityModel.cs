namespace NSIE.Models
{
    public class UserActivityModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string PageName { get; set; }
        public string Tipo { get; set; }  // Tipo de interacción: click, input, etc.
        public string Elemento { get; set; }  // Elemento: button, textbox, etc.
        public string IdElemento { get; set; }  // ID del elemento interactuado
        public string Valor { get; set; }  // Valor asociado a la interacción (ej. texto ingresado)
        public string AdditionalData { get; set; }
        public DateTime Timestamp { get; set; }  // Propiedad para almacenar la marca de tiempo de la actividad

    }
}
