//using bflex.facturacion.EfactOSE;
using bflex.facturacion.NubefactOSE;
using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Threading.Tasks;

namespace bflex.facturacion.SunatCore
{
    public class ServicioNubefact : SoapFacturacion
    {

        billServiceClient servicio;
        Comercio comercio;

        public ServicioNubefact(Comercio _comercio, int produccion)
        {
            ServicePointManager.UseNagleAlgorithm = true;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;
            EndpointBehavior credenciales;
            comercio = _comercio;

            if (produccion == 1)
            {
                //servicio = new BillServiceClient("ServicioEfact", "http://ose.efact.pe:80/ol-ti-itcpe/billService");
                //credenciales = new EndpointBehavior(comercio.Ruc, comercio.PasswordSunat);
                servicio = new billServiceClient("ServicioNubefact", "https://ose.nubefact.com/ol-ti-itcpe/billService?wsdl");
                credenciales = new EndpointBehavior(string.Concat(comercio.Ruc, comercio.UsuarioOSE), comercio.PasswordOSE);
            }
            else
            {
                //servicio = new BillServiceClient("ServicioEfact", "http://ose-gw1.efact.pe:80/ol-ti-itcpe/billService");
                //credenciales = new EndpointBehavior(comercio.Ruc, comercio.UsuarioSunat);
                servicio = new billServiceClient("ServicioNubefact", "https://demo-ose.nubefact.com/ol-ti-itcpe/billService");
                credenciales = new EndpointBehavior(string.Concat(comercio.Ruc, "MODDATOS"), "MODDATOS");
            }

            servicio.Endpoint.EndpointBehaviors.Add(credenciales);
        }

        #region facturacionv2

        public override async Task EnviarComprobanteDetallado(ComprobanteVenta comprobante, string ruta)
        {
            byte[] zipEntrada = File.ReadAllBytes(ruta + comprobante.ArchivoZip);

            try
            {
                servicio.Open();
                //byte[] zipSalida = servicio.sendBill(comprobante.ArchivoZip, zipEntrada, "");
                sendBillResponse response = await servicio.sendBillAsync(comprobante.ArchivoZip, zipEntrada, "");
                byte[] zipSalida = response.applicationResponse;
                servicio.Close();

                if (zipSalida.Length > 0)
                {
                    Respuesta respuesta = await GeneradorXml.ConstruirCDR(comprobante.ArchivoCdr, ruta, zipSalida);
                    respuesta.ComunicarComprobante(comprobante);
                }
                else
                {
                    comprobante.CodigoErrorSunat = "sunat.empty";
                    comprobante.MensajeSunat = "Sunat procesó el envío pero no devolvió respuesta. Reintentar.";
                }
            }
            catch (FaultException ex)
            {
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        comprobante.CodigoErrorSunat = "sunat.";
                    else
                        comprobante.CodigoErrorSunat = "";

                    comprobante.MensajeSunat = ex.Code.Name;
                }

                comprobante.MensajeSunat += " - Nubefact";
                comprobante.CodigoErrorSunat += ex.Message;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrWhiteSpace(comprobante.CodigoErrorSunat))
                    comprobante.CodigoErrorSunat = "sunat.connect";
                comprobante.MensajeSunat = ex.Message;
            }
        }

        public override void EnviarGrupoComprobantes(ComprobanteGrupal grupo, string ruta)
        {
            Byte[] zipEntrada = File.ReadAllBytes(ruta + grupo.ArchivoZip);

            try
            {
                grupo.NroTicket = servicio.sendSummary(grupo.ArchivoZip, zipEntrada, "");
            }
            catch (FaultException ex)
            {
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        grupo.CodigoErrorSunat += "sunat.";
                    else
                        grupo.CodigoErrorSunat = "";

                    grupo.CodigoErrorSunat += ex.Code.Name;
                }

                else grupo.CodigoErrorSunat = "sunat.-1";

                grupo.MensajeSunat = ex.Message;
            }
        }

        public override async Task ConsultarTicketGrupal(ComprobanteGrupal grupo, string ruta)
        {
            try
            {
                //statusResponse response = servicio.getStatus(grupo.NroTicket);
                getStatusResponse response = await servicio.getStatusAsync(grupo.NroTicket);

                if (response != null && response.status != null)
                {
                    //grupo.CodigoStatus = response.statusCode;
                    grupo.CodigoStatus = response.status.statusCode;

                    if (response.status.content != null && response.status.content.Length > 0)
                    {
                        Respuesta respuesta = await GeneradorXml.ConstruirCDR(grupo.ArchivoCdr, ruta, response.status.content);
                        respuesta.ComunicarGrupo(grupo);
                    }
                    else
                    {
                        grupo.CodigoErrorSunat = "sunat.empty";
                        grupo.MensajeSunat = "Sunat procesó el envío pero no devolvió respuesta. Reintentar.";
                    }
                }
                else
                {
                    grupo.CodigoErrorSunat = "sunat.error";
                    grupo.MensajeSunat = "Sunat devolvió una respuesta incorrecta.";
                }
            }
            //aqui no hay errores de evaluacion porque esto se hace en la primera fase nada mas en nubefact
            catch (Exception ex)
            {
                if (String.IsNullOrWhiteSpace(grupo.CodigoErrorSunat))
                    grupo.CodigoErrorSunat = "sunat.connect";
                grupo.MensajeSunat = ex.Message;
            }
        }

        #endregion

        public override void EnviarComprobanteDetallado(ref RespuestaServicio respuesta, String rutaEspecifica, String nombreArchivo)
        {
            Byte[] zipEntrada = File.ReadAllBytes(rutaEspecifica + nombreArchivo + ".zip");

            try
            {
                servicio.Open();
                Byte[] zipSalida = servicio.sendBill(nombreArchivo + ".zip", zipEntrada, "");
                servicio.Close();

                if (zipSalida.Length > 0)
                    GeneradorXml.ConstruirCDR(ref respuesta, nombreArchivo, rutaEspecifica, zipSalida);
                else
                {
                    respuesta.CodigoErrorSunat = "sunat.-1";
                    respuesta.MensajeSunat = "No se pudo contactar con Sunat. Intentar más tarde.";
                }
            }
            catch (FaultException ex)
            {
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        respuesta.CodigoErrorSunat += "sunat.";
                    else
                        respuesta.CodigoErrorSunat = "";

                    respuesta.MensajeSunat = ex.Code.Name;
                }

                respuesta.MensajeSunat += " - Nubefact";
                respuesta.CodigoErrorSunat += ex.Message;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrWhiteSpace(respuesta.CodigoErrorSunat))
                    respuesta.CodigoErrorSunat = "Client.0";
                respuesta.MensajeSunat = ex.Message;
            }
        }

        public override void Iniciar()
        {
            servicio.Open();
        }

        public override void Finalizar()
        {
            servicio.Close();
        }

        public override string EnviarGrupoComprobantes(ref RespuestaServicio respuesta, String rutaEspecifica, String nombreArchivo)
        {
            Byte[] zipEntrada = File.ReadAllBytes(rutaEspecifica + nombreArchivo + ".zip");
            string ticket = "";

            try
            {
                ticket = servicio.sendSummary(nombreArchivo + ".zip", zipEntrada, "");
                //la condicion la ejecuto en el controller ya que aqui si devuelvo un valor comparable
            }
            catch (FaultException ex)
            {
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        respuesta.CodigoErrorSunat += "sunat.";
                    respuesta.CodigoErrorSunat += ex.Code.Name;
                }
                else respuesta.CodigoErrorSunat = "sunat.-2";
                respuesta.MensajeSunat = ex.Message;
                respuesta.ExcepcionApi = ex.Message;
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = ex.Message;
            }
            return ticket;
        }

        public override void ConsultarTicketGrupal(
            ref RespuestaServicio respuesta, String rutaEspecifica, String nombreArchivo, String ticket
        )
        {
            try
            {
                statusResponse response = servicio.getStatus(ticket);
                if (response != null)
                {
                    respuesta.CodigoStatus = response.statusCode;
                    if (response.content != null && response.content.Length > 0)
                    {
                        GeneradorXml.ConstruirCDR(ref respuesta, nombreArchivo, rutaEspecifica, response.content);
                    }
                    else
                    {
                        respuesta.CodigoErrorSunat = "sunat.-1";
                        respuesta.MensajeSunat = "Envío satisfactorio, sin CDR.";
                    }
                }
                else
                {
                    respuesta.CodigoErrorSunat = "sunat.-2";
                    respuesta.MensajeSunat = "Sin respuesta de parte de Sunat.";
                }
            }
            catch (FaultException ex)
            {
                if (ex.Code != null && !String.IsNullOrWhiteSpace(ex.Code.Name))
                {
                    if (!ex.Code.Name.Contains("Client."))
                        respuesta.CodigoErrorSunat += "sunat.";
                    respuesta.MensajeSunat += ex.Code.Name;
                }

                respuesta.MensajeSunat += " - Nubefact";
                respuesta.CodigoErrorSunat += ex.Message;
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