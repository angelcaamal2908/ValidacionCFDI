using System;
using System.Collections.Generic;

namespace Validacion.Models;

public partial class Vmcontacto
{
    public string RfcEmisor { get; set; } = null!;

    public string RfcReceptor { get; set; } = null!;

    public string FolioFiscal { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public decimal Total { get; set; }

    public string Estatus { get; set; } = null!;
}
