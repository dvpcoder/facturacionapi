using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ComprobanteGrupal
    {
        public Comercio Comercio { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int IdUsuarioRegistro { get; set; }
        public DateTime FechaConfirmacionSunat { get; set; }
        public int IdUsuarioConfirmacionSunat { get; set; }
        public string NroTicket { get; set; }
        public string CodigoStatus { get; set; }
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string CodigoErrorSunat { get; set; }

        public DateTime FechaComprobantes { get; set; }
        public List<ComprobanteVenta> ListaComprobantes { get; set; }

        public string RutaCarpetaArchivos { get; set; }
        public string ArchivoXml { get; set; }
        public string ArchivoZip { get; set; }
        public string ArchivoCdr { get; set; }

    }
}