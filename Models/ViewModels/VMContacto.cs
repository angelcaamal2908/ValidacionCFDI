namespace Validacion.Models.ViewModels
{


    public class CFDI
    {
        public int Id { get; set; }
        public string RfcEmisor { get; set; }
        public string RfcReceptor { get; set; }
        public string FolioFiscal { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; }
    }

    public class LogValidacion
    {
        public string RfcEmisor { get; set; }
        public string RfcReceptor { get; set; }
        public string FolioFiscal { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; }
        public string ResponseData { get; set; }
        public string ErrorMessage { get; set; } // Agrega esta propiedad
    }


}

