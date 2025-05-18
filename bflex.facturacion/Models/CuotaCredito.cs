using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class CuotaCredito
    {
        public int IdCuotaCredito { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Fecha_Pago { get; set; }
        public ComprobanteVenta Comprobante { get; set; }
    }
}