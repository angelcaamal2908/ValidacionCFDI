namespace Validacion.Models.ViewModels
{
    public class VMContacto
    {
        public string RfcEmisor { get; set; }
        public string RfcReceptor { get; set; }
        public string FolioFiscal { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; }
    }
}
