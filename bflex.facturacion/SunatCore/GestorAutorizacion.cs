using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace bflex.facturacion.SunatCore
{
    public class GestorAutorizacion
    {
        public static async Task<ComprobanteVenta> AutorizarComprobanteDesdeToken(string token, string serie, string nro, string meta)
        {
            if (String.IsNullOrWhiteSpace(token))
                throw new Exception("--No se ha especificado un token");

            string output = await ConexionExterna.GestionarTokenPHP(new string[] { "token" }, new string[] { token });
            if (String.IsNullOrWhiteSpace(output))
                throw new Exception("--No se puede autorizar la operación [server]");

            dynamic resultado = new JavaScriptSerializer().DeserializeObject(output);
            if (resultado["estado"] != "ok")
                throw new Exception("--" + resultado["estado"]);

            if (resultado["label"] != "developer bflex" && resultado["meta"] != meta)
                throw new Exception("--No se puede autorizar la operación [app]");

            ComprobanteVenta comprobante = new ComprobanteVenta();

            if (string.IsNullOrWhiteSpace(serie))
            {
                serie = resultado["serie"];
                nro = resultado["nro"];
            }

            //OBTENER PRIMERO COMPROBANTE Y COMPROBAR CON TOKEN DE COMERCIO
            comprobante = await DalComprobanteVenta.ObtenerComprobantePorNumeracionRuc(serie, nro, resultado["ruc"], "");

            if (comprobante == null)
                throw new Exception("--No se puede autorizar la operación [document]");

            if (resultado["label"] == "developer bflex" && comprobante.comercio.AccessToken != token)
                throw new Exception("--No se puede autorizar la operación [cred]");

            if (comprobante.DetalleVenta.Count == 0)
                throw new Exception("--Los archivos de este documento no estan preparados");

            comprobante.comercio = await DalComercio.ObtenerComercioPorId(comprobante.comercio.IdComercio);
            //FIN DE OBTENCION

            comprobante.ActualizarAtributosEstructura();
            return comprobante;
        }

        public static async Task<Comercio> AutorizarComercioDesdeToken(string token, string meta, string ruc)
        {
            if (String.IsNullOrWhiteSpace(token))
                throw new Exception("--No se ha especificado un token");

            string output = await ConexionExterna.GestionarTokenPHP(new string[] { "token" }, new string[] { token });
            if (String.IsNullOrWhiteSpace(output))
                throw new Exception("--No se puede autorizar la operación [server]");

            dynamic resultado = new JavaScriptSerializer().DeserializeObject(output);
            if (resultado["estado"] != "ok")
                throw new Exception("--" + resultado["estado"]);

            if (resultado["label"] != "developer bflex" && resultado["meta"] != meta)
                throw new Exception("--No se puede autorizar la operación [app]");

            if (ruc != "excelVentas" && ruc != resultado["ruc"])
                throw new Exception("--No se puede autorizar la operación [target]");

            Comercio comercio = await DalComercio.ObtenerComercioPorRuc(resultado["ruc"]);

            if (comercio == null)
                throw new Exception("--No se puede autorizar la operación [lost]");

            if (resultado["label"] == "developer bflex" && comercio.AccessToken != token)
                throw new Exception("--No se puede autorizar la operación [cred]");

            return comercio;
        }
    }
}