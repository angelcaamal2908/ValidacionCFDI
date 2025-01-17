using System;
using System.Collections.Generic;

namespace Validacion.Models;

public partial class LogsValidacion
{
    public int Id { get; set; }

    public string? RfcEmisor { get; set; }

    public string? RfcReceptor { get; set; }

    public string? FolioFiscal { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Estatus { get; set; }

    public string? ErrorMessage { get; set; }
}
