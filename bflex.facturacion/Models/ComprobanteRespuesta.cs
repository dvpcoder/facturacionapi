using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ComprobanteRespuesta
    {
        public string CodigoTipoComprobante { get; set; }
        public string CodigoMoneda { get; set; }
        public string DocumentoIdentidadCliente { get; set; }
        public string CodigoIdentidadCliente { get; set; }
        public string NombreCompletoCliente { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal Total { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string CodigoErrorSunat { get; set; }
        public bool Valido { get; set; }
        public string FechaEmision { get; set; }
        public string FechaEnvio { get; set; }
        public string FechaConfirmacion { get; set; }
        //TipoIdentidad": "RUC",
    }

    public class ComprobanteRespuestaMin
    {
        public int IdLocal { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string Status { get; set; }
    }
}