using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class DetalleDocumentoVenta
    {
        public Int32 IdDetalleDocumentoVenta { get; set; }
        public Int32 IdDocumentoVenta { get; set; }
        public Presentacion Presentacion { get; set; }
        public UnidadMedida UnidadMedida { get; set; }
        public String Cantidad { get; set; }
        public String Precio { get; set; }
        public String ValorVenta { get; set; }
        public String CostoVenta { get; set; }
        public String Icbper { get; set; }
        public String DescuentoReal { get; set; }

        public String MontoIgv { get; set; }
        public String Total { get; set; }

        //public Decimal PorcentajeAfecto { get; set; }
        //public Int32 Item { get; set; }
        //public String Descripcion { get; set; }
        //public String TipoNroDocumentoAsociado { get; set; }
        //public Boolean AplicoAnticipo { get; set; }
        //public Decimal Costo { get; set; }
    }
}