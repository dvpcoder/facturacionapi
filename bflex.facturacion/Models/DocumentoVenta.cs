using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class DocumentoVenta
    {
        public Int32 IdDocumentoVenta { get; set; }
        public String NroDoc { get; set; }
        public String NroSerie { get; set; }
        public String Observacion { get; set; }
        public String Total { get; set; }
        public String Igv { get; set; }
        
        public Cliente Cliente { get; set; }
        public Moneda Moneda { get; set; }
        public PuntoEmision PuntoEmision { get; set; }       
        public TipoComprobante Documento { get; set; }
        public Usuario UsuarioRegistra { get; set; }
        public String FechaEmision { get; set; }
        
        public List<DetalleDocumentoVenta> DetalleDocumento { get; set; }
        public TipoNotaCreditoDebito MotivoEmisionNotaCD { get; set; }
        public String CadenaDocumentoVentaDocAsociados { get; set; }
        
        public String ComisionPropina { get; set; }
        public String Icbper { get; set; }
        public String MontoExonerado { get; set; }
        public String GuiaRemision { get; set; }
        public String DescuentoReal { get; set; }
        public String OrdenCompra { get; set; }
        public EstadoDocumento Estado { get; set; }
        public String MotivoAnulacion { get; set; }
        public String MontoGratuito { get; set; }
        public String TipoPagoCadena { get; set; }
        public String MontoPercepcion { get; set; }
        public String FechaVencimiento { get; set; }
        public List<CuotaCredito> DetalleCuota { get; set; }
        public Int32 Puntaje { get; set; }

        //public String SubTotal { get; set; }
        //public String FechaRegistra { get; set; }
        //public String EstadoSunat { get; set; }

        //public Decimal TCambio { get; set; }
        //public Int32 DiasCredito { get; set; }
        //public DateTime FechaDocVenta { get; set; }
        //public String MontoEnLetras { get; set; }
        //public Boolean AplicoAnticipo { get; set; }
        //public Boolean TieneAnticipos { get; set; }
        //public Boolean MultipleOS { get; set; }
        //public String NroOrdenServicioAsociadaExplicitamente { get; set; }
        //public Boolean EsNroDocManual { get; set; }
        //public Boolean EsTipoPagoUnico { get; set; }
        //public Decimal PorcentajeIGVAfecto { get; set; }
        //public Int32 IdTipoCambio { get; set; }
        //public Boolean EsPagoContado { get; set; }
        //public String ImportePago { get; set; }
        //public Decimal TotalOS_DVConMultipleOS { get; set; }
        //public Int32 EstadoSedacion { get; set; }
        //public Boolean OSConSedacion { get; set; }
        //public Decimal MontoTotalDocumentoAsociado { get; set; } // TOTAL ANTICIPADO?
        //public Boolean TieneListadoDocumentoVentaDocAsociados { get; set; }
        //public Decimal IGV_PorcentajeAfecto { get; set; }

        //public String CuentaCorrienteCadena { get; set; }
        //public Decimal TotalPagado { get; set; }
        //public Decimal TotalSaldo { get; set; }
        //public String DescripcionToolTip { get; set; }
        //public Decimal ValorTotal { get; set; }
        //public Int32 DiasVencidos { get; set; }
        //public String TipoCliente { get; set; }
        //public String TipoProveedor { get; set; }
        //public Boolean CorreoEnviado { get; set; }
        //public String NombreCliente { get; set; }
        //public Decimal totalNeto { get; set; }
        //public Int32 IdDocumentoVentaComodin { get; set; }
        //public String FechaEmision2 { get; set; }
    }
}