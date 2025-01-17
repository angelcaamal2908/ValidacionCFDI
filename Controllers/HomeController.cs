using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Validacion.Models;
using RestSharp;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Validacion.Models.ViewModels;
using RestSharp;
using Newtonsoft.Json;

namespace Validacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MostrarDatos([FromForm] IFormFile ArchivoExcel)
        {
            Stream stream = ArchivoExcel.OpenReadStream();

            IWorkbook MiExcel = null;

            if (Path.GetExtension(ArchivoExcel.FileName) == ".xlsx")
            {
                MiExcel = new XSSFWorkbook(stream);
            }
            else
            {
                MiExcel = new HSSFWorkbook(stream);
            }

            ISheet HojaExcel = MiExcel.GetSheetAt(0);

            int cantidadFilas = HojaExcel.LastRowNum;

            List<VMContacto> lista = new List<VMContacto>();

            for (int i = 1; i <= cantidadFilas; i++)
            {
                IRow fila = HojaExcel.GetRow(i);

                lista.Add(new VMContacto
                {
                    RfcEmisor = fila.GetCell(0).ToString(),
                    RfcReceptor = fila.GetCell(1).ToString(),
                    FolioFiscal = fila.GetCell(2).ToString(),
                    FechaEmision = DateTime.Parse(fila.GetCell(3).ToString()),
                    Total = decimal.Parse(fila.GetCell(4).ToString()),
                    Estatus = fila.GetCell(5).ToString()
                });
            }

            return StatusCode(StatusCodes.Status200OK, lista);
        }
        [HttpPost]
        public async Task<IActionResult> ValidarCFDI([FromBody] List<VMContacto> comprobantes)
        {
            var resultados = new List<object>(); // Aseguramos que sea una lista desde el inicio.

            try
            {
                if (comprobantes == null || !comprobantes.Any())
                {
                    return BadRequest(new { mensaje = "No se proporcionaron datos válidos para validar." });
                }

                // Configuración del cliente
                var options = new RestClientOptions("https://sandbox.link.kiban.com/api/v2/sat/cfdi_validate?testCaseId=663567bb713cf2110a1106a4");
                var client = new RestClient(options);

                foreach (var comprobante in comprobantes)
                {
                    try
                    {
                        var request = new RestRequest();
                        request.AddHeader("accept", "application/json");
                        request.AddHeader("x-api-key", "1KRDP6G0VYQ6KH-2KHXQ6Z00001Q4-P9GX-1G6NCVK5D");

                        var jsonBody = new
                        {
                            uuid = comprobante.FolioFiscal,
                            rfcEmisor = comprobante.RfcEmisor,
                            rfcReceptor = comprobante.RfcReceptor,
                            monto = comprobante.Total
                        };

                        request.AddJsonBody(JsonConvert.SerializeObject(jsonBody));

                        var response = await client.PostAsync(request);

                        if (response.IsSuccessful)
                        {
                            resultados.Add(new
                            {
                                comprobante.RfcEmisor,
                                comprobante.RfcReceptor,
                                comprobante.FolioFiscal,
                                comprobante.Total,
                                Estatus = "Validado",
                                ResponseData = response.Content
                            });
                        }
                        else
                        {
                            resultados.Add(new
                            {
                                comprobante.RfcEmisor,
                                comprobante.RfcReceptor,
                                comprobante.FolioFiscal,
                                comprobante.Total,
                                Estatus = "No encontrado",
                                ErrorMessage = response.Content ?? "Error desconocido"
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Agrega un registro de error para ese CFDI, indicando "No encontrado"
                        resultados.Add(new
                        {
                            comprobante.RfcEmisor,
                            comprobante.RfcReceptor,
                            comprobante.FolioFiscal,
                            comprobante.Total,
                            Estatus = "No encontrado",
                            ErrorMessage = "Ocurrió un error durante la validación."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "Ocurrió un error inesperado durante la validación.",
                    detalle = ex.Message
                });
            }

            return Ok(resultados); // Devuelve todos los resultados, incluyendo errores individuales.
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
