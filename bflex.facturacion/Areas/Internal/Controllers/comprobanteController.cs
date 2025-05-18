using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using bflex.facturacion.SunatCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace bflex.facturacion.Areas.Internal.Controllers
{
    public class comprobanteController : Controller
    {
        public string ObtenerDataPeticion()
        {
            return HttpContext.Request.ServerVariables["HTTP_USER_AGENT"];
        }

        public async Task<ActionResult> ConsultarComprobante(ComprobanteVenta consulta)
        {
            try
            {
                consulta.Numero = Convert.ToInt32(consulta.Numero).ToString("00000000");
                consulta.FechaEmision = GestorFacturacion.ObtenerFechaDesdeCadena(consulta.Fecha_Emision);

                ComprobanteVenta comprobante = await DalComprobanteVenta.ObtenerComprobantePorBusquedaEspecializada(consulta);
                if (comprobante == null)
                    throw new Exception("Comprobante no encontrado");

                string meta = ObtenerDataPeticion();
                string[] keys = new string[] { "ruc", "serie", "nro", "aud" };
                string[] values = new string[] { comprobante.comercio.Ruc, comprobante.Serie, comprobante.Numero, meta };

                string token = await ConexionExterna.GestionarTokenPHP(keys, values);
                if (String.IsNullOrWhiteSpace(token))
                    throw new Exception("No se pudo autorizar la operación");

                string mensajeSunat, codigoErrorSunat = "";

                if (!String.IsNullOrWhiteSpace(comprobante.CodigoErrorSunat) && comprobante.CodigoErrorSunat != "sunat.0")
                {
                    string[] valores = comprobante.CodigoErrorSunat.Split('.');
                    if (valores.Length > 1) codigoErrorSunat = valores[1];
                    else codigoErrorSunat = comprobante.CodigoErrorSunat;

                    mensajeSunat = comprobante.MensajeSunat;
                }
                else mensajeSunat = "Comprobante encontrado";

                return Json(new { mensajeSunat, codigoErrorSunat, estadoSunat = comprobante.EstadoSunat, token }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                clsException exception = new clsException(ex, null);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerListaComprobantes(
            [FromBody]beDataTable prm_tabla, string token, int tipoFechas, string fechaInicio, string fechaFin, 
            string docIdentidadCliente, string estadoSunat, string tipoComprobante, string serie, string numero
        )
        {
            Comercio comercio = null;
            try
            {
                if (String.IsNullOrWhiteSpace(fechaInicio) || String.IsNullOrWhiteSpace(fechaFin))
                    throw new Exception("Debe especificar un rango de fecha para obtener los comprobantes.");

                DateTime fecha_inicio = GestorFacturacion.ObtenerFechaDesdeCadena(fechaInicio);
                DateTime fecha_fin = GestorFacturacion.ObtenerFechaDesdeCadena(fechaFin);
                if (tipoFechas != 2) tipoFechas = 1;

                string metadata = ObtenerDataPeticion();
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, metadata, "excelVentas");

                List<ComprobanteRespuesta> listaTotal = await DalComprobanteVenta.ObtenerListaComprobantesPorComercioConFiltros(
                    docIdentidadCliente, estadoSunat, tipoComprobante, serie, numero, tipoFechas, fecha_inicio, 
                    fecha_fin, comercio.Ruc
                );

                beDataTable nuevaTabla = new beDataTable();
                nuevaTabla.data = listaTotal.ToArray();
                nuevaTabla.recordsTotal = listaTotal.Count.ToString();
                nuevaTabla.recordsFiltered = listaTotal.Count.ToString();

                return Json(nuevaTabla, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }
    }

}