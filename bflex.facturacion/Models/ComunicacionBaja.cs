using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ComunicacionBaja : ComprobanteGrupal
    {
        public int IdComunicacionBaja { get; set; }
        public DateTime FechaFacturas { get; set; }
        public string FechaFacturas2 { get; set; }
        public List<ComprobanteVenta> ListaFacturas { get; set; }

        public void ActualizarUbicacionArchivos()
        {
            RutaCarpetaArchivos = "~/Content/files/" + Comercio.CarpetaServidor + "/Comunicaciones/" +
                FechaFacturas.ToString("yyyyMM") + "/" + Serie + "-" + Numero + "/";
            ArchivoXml = Comercio.Ruc + "-RA-" + Serie + "-" + Numero;
            ArchivoZip = ArchivoXml + ".zip";
            ArchivoCdr = "CDR-" + ArchivoZip;

            FechaComprobantes = FechaFacturas;
            ListaComprobantes = ListaFacturas;
        }
    }
}