namespace NSIE.Models
{
    public class EnergyFlatDto
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public string ParentDescription { get; set; }
        public string ParentColor { get; set; }

        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public string Tipo { get; set; }
        public string ChildDescription { get; set; }
        public string ChildColor { get; set; }

        public int Year { get; set; }
        public double Value { get; set; }
    }

    // INSERT DTO
    public class EnergyRequest
    {
        public List<EnergyParentDto> Datos { get; set; }
    }

    public class EnergyParentDto
    {
        public string NodoPadre { get; set; }
        public string descripcion { get; set; }
        public string color { get; set; }
        public int id_padre { get; set; }

        public List<EnergyChildDto> NodosHijo { get; set; }
    }

    public class EnergyChildDto
    {
        public string NodoHijo { get; set; }
        public string tipo { get; set; }
        public string descripcion { get; set; }
        public int id_hijo { get; set; }
        public string color { get; set; }

        // 🔥 AQUÍ GUARDAMOS LOS AÑOS DINÁMICOS
        public Dictionary<string, object> Valores { get; set; }
    }

}