using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios.Interfaces;

public interface ICarteraConvocatoriaImportService
{
    Task<CarteraConvocatoriaImportDocument> LeerAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<CarteraConvocatoriaSeleccionDocument> LeerSeleccionAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<CarteraConvocatoriaMarcasDocument> LeerMarcasAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);
}
