using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Cuenta
    {
        public int IdCuenta { get; set; }
        public string Email { get; set; }
        public string Contrasenia{ get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Domicilio { get; set; }
        public string Telefono { get; set; }
        public string Estado{ get; set; }
    }
}