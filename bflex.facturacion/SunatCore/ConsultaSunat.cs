using bflex.facturacion.Models;
using bflex.facturacion.SunatConsultas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;

namespace bflex.facturacion.SunatCore
{
    public class ConsultaSunat
    {
        billServiceClient servicio;
        Comercio comercio;

        public ConsultaSunat(Comercio _comercio)
        {
            ServicePointManager.UseNagleAlgorithm = true;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;
            EndpointBehavior credenciales;

            servicio = new billServiceClient();
            comercio = _comercio;

            credenciales = new EndpointBehavior(string.Concat(comercio.Ruc, comercio.UsuarioSunat), comercio.PasswordSunat);
            servicio.Endpoint.EndpointBehaviors.Add(credenciales);
        }

        public void ConsultarComprobanteInformado(
            ref RespuestaServicio respuesta, String rutaEspecifica, String nombreArchivo, String ruc, ComprobanteVenta comprobante
        )
        {
            try
            {
                servicio.Open();
                statusResponse response = servicio.getStatusCdr(
                    ruc, comprobante.CodigoTipoComprobante, comprobante.Serie, Convert.ToInt32(comprobante.Numero)
                );
                servicio.Close();

                if (response != null)
                {
                    if (response.content.Length > 0)
                    {
                        GeneradorXml.ConstruirCDR(ref respuesta, nombreArchivo, rutaEspecifica, response.content);
                    }
                }
                else
                {
                    respuesta.CodigoErrorSunat = "sunat.-1";
                    respuesta.MensajeSunat = "Sunat no pudo devolver el CDR. Intentar más tarde.";
                }
            }
            catch (FaultException ex)
            {
                //este servicio solo trae el cdr si es que el doc esta aceptado, sino tira error
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        respuesta.CodigoErrorSunat = "sunat.";
                    respuesta.CodigoErrorSunat += ex.Code.Name;
                }
                else respuesta.CodigoErrorSunat = "sunat.-2";
                respuesta.MensajeSunat = ex.Message;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrWhiteSpace(respuesta.CodigoErrorSunat))
                    respuesta.CodigoErrorSunat = "Client.0";
                respuesta.MensajeSunat = ex.Message;
            }
        }
    }
}