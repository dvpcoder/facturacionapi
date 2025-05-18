using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class DetalleComprobanteVenta
    {
        public string CodigoUnidad { get; set; }
        public decimal Cantidad { get; set; }
        public string NombreProducto { get; set; }
        public string CodigoProducto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal PorcentajeIgv { get; set; }
        public decimal ValorReferencial { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Total { get; set; }
        public decimal MontoBruto { get; set; }
        public decimal MontoIcbper { get; set; }
        public decimal MontoDescuentoBase { get; set; }
        public decimal PorcentajeDescuentoBase { get; set; }
        public decimal ValorIcbperUnitario { get; set; }
        public bool EsInafecto { get; set; }
        public string CodigoTributo { get; set; }
        public string NombreTributo { get; set; }
        public string CodigoTipoTributo { get; set; }
        public string CodigoAfectacion { get; set; }
        public string CodigoPrecio { get; set; }
        public string PresentacionUnidad { get; set; }
    }
}