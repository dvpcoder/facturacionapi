using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class TributoVenta
    {
        public string CodigoTributo { get; set; }
        public string CodigoTributoResumen { get; set; }
        public string NombreTributo { get; set; }
        public string CodigoTipoTributo { get; set; }
        public decimal MontoBase { get; set; } 
        public decimal MontoTributo { get; set; }
    }
}