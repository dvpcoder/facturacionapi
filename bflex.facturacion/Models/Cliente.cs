using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Cliente
    {
        public int idCliente { get; set; }
        public string nombreCompleto { get; set; }
        public string nroDocumento { get; set; }
        public string direccion { get; set; }
        public string email { get; set; }

        //public TipoIdentidad tipoIdentidad { get; set; }
        public string Tipo { get; set; }
        //public String telefono { get; set; }
    }
}