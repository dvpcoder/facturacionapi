using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Moneda
    {
        public int idUnidadMonetaria { get; set; }
        public string nombre { get; set; }
        public string codigo { get; set; }
        public string simbolo { get; set; }
    }

    public class TipoComprobante
    {
        public string nombre { get; set; }
        public string CodigoSunat { get; set; }
        public string abreviatura { get; set; }
        public string carpeta { get; set; }
    }

    public class TipoNotaCreditoDebito
    {
        public int idTipoNotaCreditoDebito { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public char tipo { get; set; }
    }

    public class TipoIdentidad
    {
        public string codigo { get; set; }
        public string nombre { get; set; }
    }

    public class UnidadMedida
    {
        public string codigoSunat { get; set; }
        public string nombre { get; set; }
        public string abreviatura { get; set; }
    }

    public class EstadoDocumento
    {
        public int id { get; set; }
        public string descripcion { get; set; }
    }

    //falta categoria internacional de productos
}