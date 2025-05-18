using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace bflex.facturacion.Models
{
    public class clsException
    {
        public clsException(string mensaje, string origen)
        {
            DateTime now = DateTime.Now;
            string ErrorMessage = "LOG -> " + now.ToString() + ": " + origen + " = " + mensaje; //+ 
            File.AppendAllText("C:\\invokerFiles\\error_facturador.log", ErrorMessage +
                "********************************************" + Environment.NewLine);
        }

        public clsException(Exception ex, string carpetaComercio)
        {
            if (String.IsNullOrWhiteSpace(carpetaComercio))
                carpetaComercio = "general";

            string ruta = "C:\\invokerFiles\\asincrono_log\\" + carpetaComercio + "\\" + DateTime.Now.ToString("yyyyMM") + "\\";
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);

            string archivo = "error_" + DateTime.Now.ToString("ddHHmm") + ".log";

            string detalleError = "ERROR (" + DateTime.Now.ToString() + ") => " + ex.Message + Environment.NewLine;
            detalleError += "Origen: " + ex.Source + Environment.NewLine;

            string[] traza = Regex.Split(ex.StackTrace, "\r\n");
            foreach (string linea in traza)
                detalleError += linea + Environment.NewLine;

            detalleError += "********************************************" + Environment.NewLine;
            File.AppendAllText(ruta + archivo, detalleError);
        }

        public clsException(string mensaje)
        {
            string ruta = "C:\\invokerFiles\\asincrono_log\\nuevoError.log";
            File.AppendAllText(ruta, mensaje);
        }

        public clsException(DocumentoVenta documento)
        {
            string ruta = "C:\\invokerFiles\\facturacion_log\\general\\" + DateTime.Now.ToString("yyyyMM") + "_doc\\";
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);

            string archivo = "error_" + DateTime.Now.ToString("ddhhmmss") + ".log";

            string detalleError = new JavaScriptSerializer().Serialize(documento) + Environment.NewLine;

            detalleError += "********************************************" + Environment.NewLine;
            File.AppendAllText(ruta + archivo, detalleError);
        }
    }
}