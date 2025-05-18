using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class PuntoEmision
    {
        public Int32 idPuntoEmision { get; set; }
        public String nombre { get; set; }
        public String codigo { get; set; }
        public String direccion { get; set; }
        public String email { get; set; }
        public String celular { get; set; }
        public String ubigeo { get; set; }
        public Comercio comercio { get; set; }
    }
}