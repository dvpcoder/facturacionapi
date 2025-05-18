using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Usuario
    {
        public Int32 id { get; set; }
        public String usuario { get; set; }
        public String pwd { get; set; }
        public String nombre { get; set; }
        public Comercio comercio { get; set; }
    }
}