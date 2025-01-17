using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Validacion.Models;
using RestSharp;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Validacion.Models.ViewModels;
using Newtonsoft.Json;
using EFCore.BulkExtensions;
using System.Linq;

namespace Validacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly CfdiregContext _dbocontext;

        public HomeController(CfdiregContext Context)
        {
            _dbocontext = Context;
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

            List<CFDI> lista = new List<CFDI>();

            for (int i = 1; i <= cantidadFilas; i++)
            {
                IRow fila = HojaExcel.GetRow(i);

                lista.Add(new CFDI
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
        public IActionResult EnviarDatos([FromForm] IFormFile ArchivoExcel)
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

            List<Cfdi> lista = new List<Cfdi>();

            for (int i = 1; i <= cantidadFilas; i++)
            {
                IRow fila = HojaExcel.GetRow(i);

                lista.Add(new Cfdi
                {
                    RfcEmisor = fila.GetCell(0).ToString(),
                    RfcReceptor = fila.GetCell(1).ToString(),
                    FolioFiscal = fila.GetCell(2).ToString(),
                    FechaEmision = DateTime.Parse(fila.GetCell(3).ToString()),
                    Total = decimal.Parse(fila.GetCell(4).ToString()),
                    Estatus = fila.GetCell(5).ToString()
                });
            }
            _dbocontext.BulkInsert(lista);
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCFDI([FromBody] List<Cfdi> comprobantes)
        {
            var resultados = new List<LogValidacion>();

            try
            {
                if (comprobantes == null || !comprobantes.Any())
                {
                    return BadRequest(new { mensaje = "No se proporcionaron datos válidos para validar." });
                }

                // Mapeo de CFDI a ViewModel CFDI
                var comprobantesVm = MapearListaCFDI(comprobantes);

                // Configuración del cliente
                var options = new RestClientOptions("https://sandbox.link.kiban.com/api/v2/sat/cfdi_validate?testCaseId=663567bb713cf2110a1106a4");
                var client = new RestClient(options);

                foreach (var comprobante in comprobantesVm)
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
                            // Actualizamos el Estatus del CFDI en la base de datos
                            var cfdiEnDb = _dbocontext.Cfdis.FirstOrDefault(c => c.FolioFiscal == comprobante.FolioFiscal);
                            if (cfdiEnDb != null)
                            {
                                cfdiEnDb.Estatus = "Validado"; // Actualizamos el Estatus
                                _dbocontext.SaveChanges(); // Guardamos los cambios
                            }

                            // Guardamos el log de validación
                            resultados.Add(new LogValidacion
                            {
                                RfcEmisor = comprobante.RfcEmisor,
                                RfcReceptor = comprobante.RfcReceptor,
                                FolioFiscal = comprobante.FolioFiscal,
                                Total = comprobante.Total,
                                Estatus = "Validado",
                                ResponseData = response.Content
                            });
                        }
                        else
                        {
                            // Actualizamos el Estatus del CFDI en la base de datos
                            var cfdiEnDb = _dbocontext.Cfdis.FirstOrDefault(c => c.FolioFiscal == comprobante.FolioFiscal);
                            if (cfdiEnDb != null)
                            {
                                cfdiEnDb.Estatus = "No encontrado"; // Actualizamos el Estatus
                                _dbocontext.SaveChanges(); // Guardamos los cambios
                            }

                            // Guardamos el log de validación con el error
                            resultados.Add(new LogValidacion
                            {
                                RfcEmisor = comprobante.RfcEmisor,
                                RfcReceptor = comprobante.RfcReceptor,
                                FolioFiscal = comprobante.FolioFiscal,
                                Total = comprobante.Total,
                                Estatus = "No encontrado",
                                ErrorMessage = response.Content ?? "Error desconocido"
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Actualizamos el Estatus del CFDI en la base de datos
                        var cfdiEnDb = _dbocontext.Cfdis.FirstOrDefault(c => c.FolioFiscal == comprobante.FolioFiscal);
                        if (cfdiEnDb != null)
                        {
                            cfdiEnDb.Estatus = "No encontrado"; // Estatus en caso de error
                            _dbocontext.SaveChanges(); // Guardamos los cambios
                        }

                        // Guardamos el log de validación con el error
                        resultados.Add(new LogValidacion
                        {
                            RfcEmisor = comprobante.RfcEmisor,
                            RfcReceptor = comprobante.RfcReceptor,
                            FolioFiscal = comprobante.FolioFiscal,
                            Total = comprobante.Total,
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

        [HttpGet]
        public IActionResult ConsultarCFDIs(string rfcEmisor, string rfcReceptor, string estatus)
        {
            // Filtramos según los parámetros proporcionados.
            var query = _dbocontext.Cfdis.AsQueryable();

            if (!string.IsNullOrEmpty(rfcEmisor))
            {
                query = query.Where(c => c.RfcEmisor.Contains(rfcEmisor));
            }

            if (!string.IsNullOrEmpty(rfcReceptor))
            {
                query = query.Where(c => c.RfcReceptor.Contains(rfcReceptor));
            }

            if (!string.IsNullOrEmpty(estatus))
            {
                query = query.Where(c => c.Estatus.Contains(estatus));
            }

            // Ejecutamos la consulta y obtenemos la lista de CFDIs
            var cfdos = query.ToList();

            // Retornamos los CFDIs encontrados
            return Ok(cfdos);
        }

        [HttpGet]
        public IActionResult DescargarCFDIs(string rfcEmisor, string rfcReceptor, string estatus)
        {
            // Filtramos según los parámetros proporcionados.
            var query = _dbocontext.Cfdis.AsQueryable();

            if (!string.IsNullOrEmpty(rfcEmisor))
            {
                query = query.Where(c => c.RfcEmisor.Contains(rfcEmisor));
            }

            if (!string.IsNullOrEmpty(rfcReceptor))
            {
                query = query.Where(c => c.RfcReceptor.Contains(rfcReceptor));
            }

            if (!string.IsNullOrEmpty(estatus))
            {
                query = query.Where(c => c.Estatus.Contains(estatus));
            }

            // Ejecutamos la consulta y obtenemos la lista de CFDIs
            var cfdos = query.ToList();

            // Mapeamos la lista de CFDIs a ViewModel CFDIs
            var cfdosVm = MapearListaCFDI(cfdos);

            // Creamos el archivo Excel
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("CFDIs");

            // Creamos la fila de encabezados
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("RFC Emisor");
            headerRow.CreateCell(1).SetCellValue("RFC Receptor");
            headerRow.CreateCell(2).SetCellValue("Folio Fiscal");
            headerRow.CreateCell(3).SetCellValue("Fecha de Emisión");
            headerRow.CreateCell(4).SetCellValue("Total");
            headerRow.CreateCell(5).SetCellValue("Estatus");

            // Llenamos las filas con los datos de los CFDIs filtrados
            for (int i = 0; i < cfdosVm.Count; i++)
            {
                var cfdi = cfdosVm[i];
                IRow row = sheet.CreateRow(i + 1);

                row.CreateCell(0).SetCellValue(cfdi.RfcEmisor);
                row.CreateCell(1).SetCellValue(cfdi.RfcReceptor);
                row.CreateCell(2).SetCellValue(cfdi.FolioFiscal);
                row.CreateCell(3).SetCellValue(cfdi.FechaEmision.ToString("yyyy-MM-dd"));
                row.CreateCell(4).SetCellValue(cfdi.Total.ToString("F2"));
                row.CreateCell(5).SetCellValue(cfdi.Estatus);
            }

            // Convertimos el archivo Excel a un MemoryStream
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                byte[] fileBytes = ms.ToArray();

                // Devolvemos el archivo Excel como una respuesta de descarga
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CFDIs.xlsx");
            }
        }

        // Métodos de mapeo
        private CFDI MapearCFDI(Cfdi cfdi)
        {
            return new CFDI
            {
                Id = cfdi.Id,
                RfcEmisor = cfdi.RfcEmisor,
                RfcReceptor = cfdi.RfcReceptor,
                FolioFiscal = cfdi.FolioFiscal,
                FechaEmision = cfdi.FechaEmision ?? DateTime.MinValue, // Asegúrate de manejar los valores nulos de fecha.
                Total = cfdi.Total ?? 0m, // Asegúrate de manejar los valores nulos de decimal.
                Estatus = cfdi.Estatus
            };
        }

        private List<CFDI> MapearListaCFDI(List<Cfdi> listaCfdi)
        {
            return listaCfdi.Select(cfdi => MapearCFDI(cfdi)).ToList();
        }
    }
}
