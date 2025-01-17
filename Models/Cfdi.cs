using System;
using System.Collections.Generic;

namespace Validacion.Models;

public partial class Cfdi
{
    public int Id { get; set; }

    public string? RfcEmisor { get; set; }

    public string? RfcReceptor { get; set; }

    public string? FolioFiscal { get; set; }

    public DateTime? FechaEmision { get; set; }

    public decimal? Total { get; set; }

    public string? Estatus { get; set; }
}
