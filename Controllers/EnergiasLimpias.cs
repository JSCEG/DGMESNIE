using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class EnergiasLimpias : Controller
    {
        private readonly IRepositorioEnergiasLimpias repositorioEnergiasLimpias;
        public EnergiasLimpias(IRepositorioEnergiasLimpias repositorioEnergiasLimpias)
        {

            this.repositorioEnergiasLimpias = repositorioEnergiasLimpias;
        }
        public async Task<IActionResult> Certificados()
        {
            var marcoRegulatorio = await repositorioEnergiasLimpias.ObtenerMarcoRegulatorio();

            // Filtrar los totales de Resoluciones y Acuerdos
            int totalResoluciones = marcoRegulatorio.Count(i => i.Tipo.Trim().Equals("Resolución", StringComparison.OrdinalIgnoreCase));
            int totalAcuerdos = marcoRegulatorio.Count(i => i.Tipo.Trim().Equals("Acuerdo", StringComparison.OrdinalIgnoreCase));

            // Pasamos los totales a la vista
            ViewData["TotalResoluciones"] = totalResoluciones;
            ViewData["TotalAcuerdos"] = totalAcuerdos;

            return View(marcoRegulatorio);
        }

        public IActionResult FactorEmision()
        {
            var factoresEmision = new List<FactorEmision>
    {
        new FactorEmision { Anio = 2017, Valor = 0.582, PdfUrl = Url.Content("https://www.gob.mx/cms/uploads/attachment/file/304573/Factor_de_Emisi_n_del_Sector_El_ctrico_Nacional_1.pdf") },
        new FactorEmision { Anio = 2018, Valor = 0.527, PdfUrl = Url.Content("https://www.gob.mx/cms/uploads/attachment/file/442714/Aviso_Factor_de_Emisiones_2018.pdf") },
        new FactorEmision { Anio = 2019, Valor = 0.505, PdfUrl = Url.Content("https://www.gob.mx/cms/uploads/attachment/file/537538/2019.pdf") },
        new FactorEmision { Anio = 2020, Valor = 0.494, PdfUrl = Url.Content("https://www.gob.mx/cms/uploads/attachment/file/630245/Aviso_FE_SEN_2020.pdf") },
        new FactorEmision { Anio = 2021, Valor = 0.423, PdfUrl = Url.Content("https://www.gob.mx/cms/uploads/attachment/file/706482/1.-Aviso_FE_CRE_2021.pdf") },
        new FactorEmision { Anio = 2022, Valor = 0.435, PdfUrl =  Url.Content("https://www.gob.mx/cms/uploads/attachment/file/806468/4_-Aviso_FE_2022__1_.pdf") }, // Reemplaza "#" con la ruta correcta del PDF si lo tienes
        new FactorEmision { Anio = 2023, Valor = 0.438, PdfUrl =  Url.Content("https://www.gob.mx/cms/uploads/attachment/file/895937/Aviso_FE-SEN23.pdf") }  // Reemplaza "#" con la ruta correcta del PDF si lo tienes
    };

            // Calcular la regresión lineal (predicción para 2024-2030)
            double[] anios = factoresEmision.Select(f => (double)f.Anio).ToArray();
            double[] valores = factoresEmision.Select(f => f.Valor).ToArray();
            double promedioX = anios.Average();
            double promedioY = valores.Average();
            double sumatoriaXY = 0;
            double sumatoriaXX = 0;

            for (int i = 0; i < anios.Length; i++)
            {
                sumatoriaXY += (anios[i] - promedioX) * (valores[i] - promedioY);
                sumatoriaXX += (anios[i] - promedioX) * (anios[i] - promedioX);
            }

            double pendiente = sumatoriaXY / sumatoriaXX;
            double intercepto = promedioY - pendiente * promedioX;

            var factoresPronosticados = new List<FactorEmision>();
            for (int anio = 2024; anio <= 2024; anio++)
            {
                double valorPronosticado = pendiente * anio + intercepto;
                factoresPronosticados.Add(new FactorEmision
                {
                    Anio = anio,
                    Valor = valorPronosticado,
                    PdfUrl = null // No hay PDF para estos años, es un pronóstico
                });
            }

            var model = new FactorEmisionViewModel
            {
                FactoresEmision = factoresEmision.Concat(factoresPronosticados).ToList()
            };

            return View(model);
        }

    }
}
