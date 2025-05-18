using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ResumenBoletas : ComprobanteGrupal
    {
        public int IdResumenBoletas { get; set; }
        public DateTime FechaBoletas { get; set; }
        public string FechaBoletas2 { get; set; }
        public List<ComprobanteVenta> ListaBoletas { get; set; }
        public void ActualizarUbicacionArchivos()
        {
            RutaCarpetaArchivos = "~/Content/files/" + Comercio.CarpetaServidor + "/Resumenes/" + 
                FechaBoletas.ToString("yyyyMM") + "/" + Serie + "-" + Numero + "/";
            ArchivoXml = Comercio.Ruc + "-RC-" + Serie + "-" + Numero;
            ArchivoZip = ArchivoXml + ".zip";
            ArchivoCdr = "CDR-" + ArchivoZip;

            FechaComprobantes = FechaBoletas;
            ListaComprobantes = ListaBoletas;
        }
    }
}