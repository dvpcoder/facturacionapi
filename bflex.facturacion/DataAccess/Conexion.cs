using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.DataAccess
{
    public class Conexion
    {
        public static String obtenerConexion()
        {
            return "Data Source=.;Initial Catalog=MyDatabase;uid=myUser;pwd=myPassword";
        }
    }
}