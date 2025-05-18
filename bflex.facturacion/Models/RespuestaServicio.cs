using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class RespuestaServicio
    {
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string CodigoErrorSunat { get; set; }
        public string ExcepcionApi { get; set; }
        public string CodigoStatus { get; set; }
        public string rutaArchivo { get; set; }
        public ResumenBoletas resumen { get; set; }
        public ComunicacionBaja comunicacion { get; set; }
        public Empresa empresa { get; set; }
        //un comercio o persona con campos nuevos...

        public string token { get; set; }
        //public string fecha { get; set; }
        //public bool incluirXml { get; set; }
        //public bool incluirCdr { get; set; }
        //public string archivo { get; set; }
        //public string estadoComprobante { get; set; }
        //public int idUsuario { get; set; }
    }
}