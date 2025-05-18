using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using bflex.facturacion.SunatCore;
using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace bflex.facturacion.Controllers
{
    public class facturacionController : Controller
    {
        public string ObtenerDataPeticion()
        {
            return HttpContext.Request.ServerVariables["HTTP_USER_AGENT"];
        }

        public string ObtenerRutaPrincipal(string carpeta)
        {
            try
            {
                string rutaPrincipal = Server.MapPath(carpeta);
                if (!Directory.Exists(rutaPrincipal))
                    Directory.CreateDirectory(rutaPrincipal);
                return rutaPrincipal;
            }
            catch (Exception ex)
            {
                new clsException(ex, "");
                throw new Exception("Problemas para crear carpetas");
            }
        }

        public void CrearArchivoPdf(ComprobanteVenta comprobante, string rutaFisica)
        {
            GestorFacturacion.CrearArchivoQR(comprobante, rutaFisica);
            ActionAsPdf impresion = new ActionAsPdf("ObtenerVistaComprobantePdf", new
            {
                idComercio = comprobante.comercio.IdComercio,
                serie = comprobante.Serie,
                numero = comprobante.Numero
            });
            byte[] datos = impresion.BuildFile(ControllerContext);

            if (datos.Length == 0)
                throw new Exception("No se pudo construir el archivo");

            using (FileStream fs = new FileStream(rutaFisica + comprobante.ArchivoPdf, FileMode.Create, FileAccess.Write))
                fs.Write(datos, 0, datos.Length);
        }

        public async Task CrearArchivoXmlComprobante(ComprobanteVenta comprobante, string rutaFisica)
        {
            string rutaBase = Server.MapPath("~/Content/files/" + comprobante.comercio.CarpetaServidor + "/");
            await GestorFacturacion.ActualizarComercioAnexo(comprobante);

            GeneradorXml.GenerarDocumentoXMLUBL21Comprobante(comprobante, comprobante.comercio, rutaBase);
            GeneradorXml.FirmarXml(
                rutaBase, 0, comprobante.ArchivoXml, rutaFisica, comprobante.comercio
            );
        }

        public async Task<ActionResult> ObtenerVistaComprobantePdf(int idComercio, string serie, string numero)
        {
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);
                ComprobanteVenta comprobante = await DalComprobanteVenta.ObtenerComprobantePorNumeracionRuc(serie, numero, comercio.Ruc, "");
                comprobante.comercio = comercio;
                comprobante.ActualizarAtributosEstructura();
                await GestorFacturacion.ActualizarComercioAnexo(comprobante);

                if (comprobante.CodigoTipoComprobante == "01" || comprobante.CodigoTipoComprobante == "03")
                    return View("PdfFacturaBoletaV1", comprobante);
                else if (comprobante.CodigoTipoComprobante == "07" || comprobante.CodigoTipoComprobante == "08")
                    return View("PdfNotaCreditoDebitoV1", comprobante);
                else return Content("No es un comprobante correcto");
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                return Content("Ocurrió un error inesperado");
            }
        }

        public async Task<ActionResult> ObtenerPdfComprobante (string token, string serie, string nro)
        {
            ComprobanteVenta comprobante = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comprobante = await GestorAutorizacion.AutorizarComprobanteDesdeToken(token, serie, nro, metadata);
                string rutaPrincipal = ObtenerRutaPrincipal(comprobante.RutaCarpetaArchivos);

                FileInfo file = new FileInfo(rutaPrincipal + comprobante.ArchivoPdf);
                if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                {
                    CrearArchivoPdf(comprobante, rutaPrincipal);
                }

                return File(rutaPrincipal + comprobante.ArchivoPdf, "application/pdf", comprobante.ArchivoPdf);
            }
            catch (Exception ex)
            {
                string carpeta = comprobante != null ? comprobante.comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerXmlComprobante (string token, string serie, string nro)
        {
            ComprobanteVenta comprobante = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comprobante = await GestorAutorizacion.AutorizarComprobanteDesdeToken(token, serie, nro, metadata);
                string rutaPrincipal = ObtenerRutaPrincipal(comprobante.RutaCarpetaArchivos);

                FileInfo file = new FileInfo(rutaPrincipal + comprobante.ArchivoZip);
                if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                {
                    await CrearArchivoXmlComprobante(comprobante, rutaPrincipal);
                }

                return File(rutaPrincipal + comprobante.ArchivoZip, "application/zip", comprobante.ArchivoZip);
            }
            catch (Exception ex)
            {
                string carpeta = comprobante != null ? comprobante.comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerCdrComprobante(string token, string serie, string nro)
        {
            ComprobanteVenta comprobante = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comprobante = await GestorAutorizacion.AutorizarComprobanteDesdeToken(token, serie, nro, metadata);
                string rutaPrincipal = ObtenerRutaPrincipal(comprobante.RutaCarpetaArchivos);

                FileInfo file = new FileInfo(rutaPrincipal + comprobante.ArchivoCdr);
                if (!file.Exists)
                {
                    if (
                        comprobante.EstadoSunat == "Aceptado" || comprobante.EstadoSunat == "Anulado" || 
                        comprobante.EstadoSunat == "En espera" || comprobante.EstadoSunat == "Por anular"
                    )
                        throw new Exception("--Este comprobante se notificó en un resumen diario de boletas.");
                    else
                        throw new Exception("--Este comprobante aún no ha sido autorizado por Sunat");
                }

                return File(rutaPrincipal + comprobante.ArchivoCdr, "application/zip", comprobante.ArchivoCdr);
            }
            catch (Exception ex)
            {
                string carpeta = comprobante != null ? comprobante.comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> EnviarArchivosComprobante(string token, string serie, string nro, string mail)
        {
            ComprobanteVenta comprobante = null;
            try
            {
                if (String.IsNullOrWhiteSpace(mail))
                    throw new Exception("--No se ha especificado un correo");

                string metadata = ObtenerDataPeticion();
                comprobante = await GestorAutorizacion.AutorizarComprobanteDesdeToken(token, serie, nro, metadata);
                string rutaPrincipal = ObtenerRutaPrincipal(comprobante.RutaCarpetaArchivos);
                string[] adjuntos = new string[3];
                FileInfo file;

                file = new FileInfo(rutaPrincipal + comprobante.ArchivoPdf);
                if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                {
                    CrearArchivoPdf(comprobante, rutaPrincipal);
                }

                file = new FileInfo(rutaPrincipal + comprobante.ArchivoZip);
                if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                {
                    await CrearArchivoXmlComprobante(comprobante, rutaPrincipal);
                }

                adjuntos[0] = rutaPrincipal + comprobante.ArchivoPdf;
                adjuntos[1] = rutaPrincipal + comprobante.ArchivoZip;

                file = new FileInfo(rutaPrincipal + comprobante.ArchivoCdr);
                if (file.Exists)
                    adjuntos[2] = rutaPrincipal + comprobante.ArchivoCdr;

                string rutaMailer = Server.MapPath("~/Content/files/mailer.html");
                string contenido = System.IO.File.ReadAllText(rutaMailer);
                contenido = contenido.Replace("{NombreCliente}", comprobante.NombreCompletoCliente);
                contenido = contenido.Replace("{TipoDoc}", comprobante.TipoComprobante.nombre);
                contenido = contenido.Replace("{NroDoc}", comprobante.Serie + "-" + comprobante.Numero);
                contenido = contenido.Replace("{anio}", DateTime.Now.Year.ToString());
                contenido = contenido.Replace("{elaborado}", comprobante.comercio.ElaboradoPor);
                string asunto = comprobante.comercio.NombreComercial + " te ha enviado la " +
                    comprobante.TipoComprobante.nombre + " Electrónica Nro: " + comprobante.Serie + "-" +
                    comprobante.Numero;

                Mailer mailer = new Mailer();
                mailer.EnviarCorreo(mail, asunto, comprobante.comercio, adjuntos, contenido);

                return Content("Envío correcto");
            }
            catch (Exception ex)
            {
                string carpeta = comprobante != null ? comprobante.comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
               
                return Content(ex.Message);
            }
        }

        public async Task ValidarRutaFtp(string ruta, string usuario, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ruta);
                request.Credentials = new NetworkCredential(usuario, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync()) { return; }

            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse && ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ruta);
                    request.Credentials = new NetworkCredential(usuario, password);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync()) { return; }
                }
                else throw;
            }
        }

        public async Task<ActionResult> GenerarLinkPdfComprobante(string token, string serie, string nro)
        {
            ComprobanteVenta comprobante = null;
            string error = "0";
            string valor = "";

            try
            {
                string metadata = ObtenerDataPeticion();
                comprobante = await GestorAutorizacion.AutorizarComprobanteDesdeToken(token, serie, nro, metadata);
                string rutaOrigen = ObtenerRutaPrincipal(comprobante.RutaCarpetaArchivos);

                FileInfo file = new FileInfo(rutaOrigen + comprobante.ArchivoPdf);
                if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                {
                    CrearArchivoPdf(comprobante, rutaOrigen);
                }

                string usuario = "myUser";
                string password = "myPassword";
                string rutaBaseFtp = "ftp://ftp.mysupportproject.pe/public_html/PDFs/";

                string rutaDestinoFtp = rutaBaseFtp + comprobante.comercio.Ruc + "/";
                await ValidarRutaFtp(rutaDestinoFtp, usuario, password);

                rutaDestinoFtp += comprobante.FechaEmision.ToString("yyyyMM") + "/";
                await ValidarRutaFtp(rutaDestinoFtp, usuario, password);

                byte[] fileContents;
                using (FileStream sourceStream = new FileStream(rutaOrigen + comprobante.ArchivoPdf, FileMode.Open, FileAccess.Read))
                {
                    fileContents = new byte[sourceStream.Length];
                    await sourceStream.ReadAsync(fileContents, 0, fileContents.Length);
                }

                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(rutaDestinoFtp + comprobante.ArchivoPdf);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(usuario, password);
                    request.UseBinary = true;
                    request.UsePassive = true;
                    request.KeepAlive = false;
                    request.ContentLength = fileContents.Length;

                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                    }
                }
                catch (WebException ex)
                {
                    string mensaje = ex.Message;
                    if (ex.Response is FtpWebResponse ftpResponse)
                        mensaje += $" [{ftpResponse.StatusDescription}]";
                    throw new Exception(mensaje);
                }

                string final = rutaDestinoFtp + comprobante.ArchivoPdf;
                valor = final.Replace(rutaBaseFtp, "https://mysupportproject.pe/PDFs/");
            }
            catch (Exception ex)
            {
                error = "1";
                valor = ex.Message;
                string carpeta = comprobante != null ? comprobante.comercio.CarpetaServidor : "";
                new clsException(ex, carpeta);
            }
            return Json(new { error, valor }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> InformarComprobante(ComprobanteVenta comprobante, string token, string ruc, int produccion)
        {
            Comercio comercio = null;
            Respuesta respuesta = new Respuesta();

            try
            {
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), ruc);
                ComprobanteVenta obj = await GestorFacturacion.GestionComprobante(comprobante, comercio);

                if (obj.EstadoSunat == "Con error" && obj.CodigoErrorSunat.Contains("1033"))
                {
                    string rutaPrincipal = ObtenerRutaPrincipal(obj.RutaCarpetaArchivos);
                    Respuesta temp = await GeneradorXml.RecuperarCDR(rutaPrincipal + comprobante.ArchivoCdr);
                    temp.ComunicarComprobante(comprobante);

                    respuesta = new Respuesta(obj, true);
                    if (obj.EstadoSunat == "Aceptado" || obj.EstadoSunat == "Rechazado")
                        obj.IdUsuarioConfirmacionSunat = comprobante.IdUsuarioCreacion;

                    if (produccion == 1)
                    {
                        int resultado = await DalComprobanteVenta.ActualizarEnvioComprobanteVenta(obj);
                        if (resultado <= 0)
                            throw new Exception("Error al guardar en la base de datos.");
                    }
                }

                else if (obj.EstadoSunat == "Sin envío" || obj.EstadoSunat == "Con error")
                {
                    if (produccion == 1)
                    {
                        bool esFactura = obj.Serie[0] == 'F' || obj.CodigoTipoComprobante.Equals("01");
                        DateTime minimo;

                        if (esFactura)
                        {
                            DateTime temp = DateTime.Now.AddDays(-3);
                            minimo = new DateTime(temp.Year, temp.Month, temp.Day);
                        }
                        else
                        {
                            minimo = DateTime.Now.AddDays(-5);
                        }

                        bool vencido = obj.FechaEmision < minimo;
                        bool mismoPeriodo = obj.FechaEmision.Month == minimo.Month;

                        if (vencido && esFactura && mismoPeriodo)
                        {
                            throw new Exception("--Comprobante registrado. Puede regularizar fechas para su aprobación.");
                        }
                        
                        if (vencido && !esFactura)
                        {
                            throw new Exception("--Comprobante registrado. Puede regularizar por resumen de boletas.");
                        }

                        if (obj.CodigoIdentidadCliente != "6" && esFactura)
                        {
                            throw new Exception("--El comprobante presentado tiene errores en su registro. Corregir los datos del cliente.");
                        }
                    }

                    string ruta = ObtenerRutaPrincipal(obj.RutaCarpetaArchivos);
                    await CrearArchivoXmlComprobante(obj, ruta);

                    SoapFacturacion servicio;
                    if (!comercio.Nubefact)
                        servicio = new ServicioSunat(comercio, produccion);
                    else
                        servicio = new ServicioNubefact(comercio, produccion);

                    await servicio.EnviarComprobanteDetallado(obj, ruta);

                    if (String.IsNullOrWhiteSpace(obj.CodigoErrorSunat))
                        throw new Exception("--Sunat no pudo devolver una respuesta.");

                    respuesta = new Respuesta(obj, true);
                    if (obj.EstadoSunat == "Aceptado" || obj.EstadoSunat == "Rechazado")
                        obj.IdUsuarioConfirmacionSunat = comprobante.IdUsuarioCreacion;

                    if (produccion == 1)
                    {
                        int resultado = await DalComprobanteVenta.ActualizarEnvioComprobanteVenta(obj);
                        if (resultado <= 0)
                            throw new Exception("Error al guardar en la base de datos.");
                    }
                }

                else respuesta = new Respuesta(obj, false);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);

                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Error inesperado durante la ejecución";
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> InformarPorResumenDiario(
            List<ComprobanteVenta> listaComprobantes, string fecha, string token, string ruc, int produccion
        )
        {
            Comercio comercio = null;
            RespuestaContenedor respuesta = new RespuestaContenedor();
            ResumenBoletas resumen = new ResumenBoletas();

            try
            {
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), ruc);
                
                resumen.ListaBoletas = new List<ComprobanteVenta>();
                resumen.FechaBoletas = GestorFacturacion.ObtenerFechaDesdeCadena(fecha);
                List<ComprobanteVenta> inadmitidos = new List<ComprobanteVenta>();
                    
                DateTime minimo = DateTime.Now.AddDays(-7);
                bool restriccion = produccion == 1 && resumen.FechaBoletas < new DateTime(minimo.Year, minimo.Month, minimo.Day);

                foreach (ComprobanteVenta obj in listaComprobantes)
                {
                    bool esBoleta = obj.Serie[0] == 'B' || obj.CodigoTipoComprobante.Equals("03");
                    if (!esBoleta)
                    {
                        throw new Exception("--Solo pueden incluirse Boletas o Notas Credito/Debito relacionadas.");
                    }

                    //estandarizo la fecha para todos sin importar lo que mando el usuario en detalle
                    obj.Fecha_Emision = fecha;
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionComprobante(obj, comercio);
                    bool inicial = comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error";
                    bool sePuedeAnular = !comprobante.Valido && !restriccion;
                    bool añadido = false;

                    if (inicial && (comprobante.Valido || sePuedeAnular))
                    {
                        comprobante.CodigoEstadoResumen = "1";
                        resumen.ListaBoletas.Add(comprobante);
                        añadido = true;
                        if (resumen.ListaBoletas.Count == 500) break;
                    }

                    if (sePuedeAnular && (comprobante.EstadoSunat == "Aceptado" || inicial))
                    {
                        ComprobanteVenta copia = new ComprobanteVenta();
                        foreach (PropertyInfo property in typeof(ComprobanteVenta).GetProperties().Where(p => p.CanWrite))
                        {
                            property.SetValue(copia, property.GetValue(comprobante, null), null);
                        }

                        copia.CodigoEstadoResumen = "3";
                        resumen.ListaBoletas.Add(copia);
                        añadido = true;
                        if (resumen.ListaBoletas.Count == 500) break;
                    }

                    //se usa otra lista porque cada doc es procesado con su estado actual en bd
                    if (!añadido) inadmitidos.Add(comprobante);
                }

                if (resumen.ListaBoletas.Count > 0)
                {
                    resumen.Serie = DateTime.Now.ToString("yyyyMMdd");
                    resumen.Numero = await DalResumenBoletas.ObtenerNumeroResumenBoletas(resumen.Serie, comercio);
                    resumen.Comercio = comercio;
                    resumen.IdUsuarioRegistro = resumen.ListaBoletas[0].IdUsuarioCreacion;

                    resumen.ActualizarUbicacionArchivos();
                    string ruta = ObtenerRutaPrincipal(resumen.RutaCarpetaArchivos);
                    string rutaBase = Server.MapPath("~/Content/files/" + resumen.Comercio.CarpetaServidor + "/");

                    GeneradorXml.GenerarXMLResumenBoletas(resumen, comercio, rutaBase);
                    GeneradorXml.FirmarXml(rutaBase, 0, resumen.ArchivoXml, ruta, comercio);

                    SoapFacturacion servicio;
                    if (!comercio.Nubefact)
                        servicio = new ServicioSunat(comercio, produccion);
                    else
                        servicio = new ServicioNubefact(comercio, produccion);

                    servicio.Iniciar();
                    servicio.EnviarGrupoComprobantes(resumen, ruta);
                    if (String.IsNullOrWhiteSpace(resumen.NroTicket))
                        throw new Exception("--" + resumen.CodigoErrorSunat + " | " + resumen.MensajeSunat);
   
                    if (produccion == 1)
                    {
                        int resultado = await DalResumenBoletas.InsertarResumenBoletas(resumen);
                        if (resultado <= 0)
                            throw new Exception("Error al guardar en la base de datos.");

                        resumen.IdResumenBoletas = resultado;
                        resumen.FechaRegistro = DateTime.Now;

                        await Task.Delay(5000);
                        await servicio.ConsultarTicketGrupal(resumen, ruta);

                        respuesta = new RespuestaContenedor(resumen, true);
                        if (resumen.EstadoSunat == "Aceptado" || resumen.EstadoSunat == "Rechazado")
                        {
                            resumen.IdUsuarioConfirmacionSunat = resumen.IdUsuarioRegistro;
                            resultado = await DalResumenBoletas.ActualizarEnvioResumenBoletas(resumen);
                            if (resultado <= 0)
                                throw new Exception("Error al guardar en la base de datos.");
                        }
                    }
                    else
                    {
                        resumen.MensajeSunat = "Resumen de prueba creado satisfactoriamente.";
                        resumen.EstadoSunat = "Aceptado";
                        resumen.CodigoErrorSunat = "Client.1";
                        respuesta = new RespuestaContenedor(resumen, false);
                    }
                    
                    servicio.Finalizar();
                }
                else 
                {
                    inadmitidos = inadmitidos.OrderByDescending(x => x.Valido).ThenByDescending(y => y.EstadoSunat).ToList();
                    foreach (ComprobanteVenta comprobante in inadmitidos)
                    {
                        //tengo que buscar por el id en el api ya que a veces mandan de diferentes bds
                        resumen = await DalResumenBoletas.ObtenerResumenBoletasPorDocumentoExterno(comercio, comprobante.IdComprobanteVenta, true);
                        if (resumen != null) 
                        {
                            resumen.FechaComprobantes = resumen.FechaBoletas;
                            resumen.ListaComprobantes = await DalResumenBoletas.ObtenerDetalleResumenPorIdResumen(resumen.IdResumenBoletas);
                            respuesta = new RespuestaContenedor(resumen, false);

                            break; 
                        }
                    }

                    if (resumen == null)
                    {
                        respuesta = new RespuestaContenedor();

                        //si se mando a anular y nunca estuvo en otro resumen
                        if (!inadmitidos[0].Valido)
                            respuesta.ExcepcionApi = "No se puede anular por resumen de boletas por haber pasado la fecha límite.";

                        //si no se admitio y no se mando para anular es porque tiene otro estado final
                        else
                            respuesta.ExcepcionApi = "Debe actualizar el estado del comprobante por medio del método principal: InformarComprobante.";
                        
                        respuesta.ListaComprobantes = new List<ComprobanteRespuestaMin>();

                        foreach (ComprobanteVenta obj in inadmitidos)
                        {
                            respuesta.ListaComprobantes.Add(new ComprobanteRespuestaMin()
                            {
                                IdLocal = obj.IdDocumentoExterno,
                                Serie = obj.Serie,
                                Numero = obj.Numero,
                                Status = obj.EstadoSunat
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);

                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Error inesperado durante la ejecución";
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ConsultarEstadoResumen(string token, string ruc, string serie, int numero)
        {
            Comercio comercio = null;
            RespuestaContenedor respuesta = new RespuestaContenedor();
            ResumenBoletas resumen = null;

            try
            {
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), ruc);
                resumen = await DalResumenBoletas.ObtenerResumenPorNumeracionComercio(serie, numero, comercio);

                if (resumen == null)
                    throw new Exception("--No se pudo encontrar el resumen solicitado.");

                resumen.ListaBoletas = await DalResumenBoletas.ObtenerDetalleResumenPorIdResumen(resumen.IdResumenBoletas);
                resumen.ActualizarUbicacionArchivos();
                string ruta = ObtenerRutaPrincipal(resumen.RutaCarpetaArchivos);

                if (resumen.EstadoSunat == "En espera")
                {
                    SoapFacturacion servicio;
                    if (!comercio.Nubefact)
                        servicio = new ServicioSunat(comercio, 1);
                    else
                        servicio = new ServicioNubefact(comercio, 1);

                    servicio.Iniciar();
                    await servicio.ConsultarTicketGrupal(resumen, ruta);
                    servicio.Finalizar();

                    respuesta = new RespuestaContenedor(resumen, true);
                    if (resumen.EstadoSunat == "Aceptado" || resumen.EstadoSunat == "Rechazado")
                    {
                        resumen.IdUsuarioConfirmacionSunat = resumen.IdUsuarioRegistro;
                        int resultado = await DalResumenBoletas.ActualizarEnvioResumenBoletas(resumen);
                        if (resultado <= 0)
                            throw new Exception("Error al guardar en la base de datos.");
                    }
                }
                else
                {
                    respuesta = new RespuestaContenedor(resumen, false);
                }
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);

                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Error inesperado durante la ejecución";
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ObtenerArchivoResumen(string token, string serie, int numero, string tipo)
        {
            Comercio comercio = null;
            ResumenBoletas resumen = null;

            try
            {
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), "excelVentas");
                resumen = await DalResumenBoletas.ObtenerResumenPorNumeracionComercio(serie, numero, comercio);

                if (resumen == null)
                    throw new Exception("--No se pudo encontrar el resumen solicitado.");

                resumen.ActualizarUbicacionArchivos();
                string ruta = ObtenerRutaPrincipal(resumen.RutaCarpetaArchivos);
                string nombre = "";

                if (tipo == "cdr")
                {
                    ruta += resumen.ArchivoCdr;
                    nombre = resumen.ArchivoCdr;
                }
                else
                {
                    ruta += resumen.ArchivoZip;
                    nombre = resumen.ArchivoZip;
                }

                FileInfo file = new FileInfo(ruta);
                if (!file.Exists)
                {
                    throw new Exception("--No se encuentra el archivo solicitado.");
                }

                return File(ruta, "application/zip", nombre);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> AnularPorComunicacionBaja(string serie, string numero, string tipoDoc, string motivo, string token, string ruc)
        {
            Comercio comercio = null;
            RespuestaContenedor respuesta = null;
            ComunicacionBaja comunicacion = null;

            try
            {
                if (serie[0] == 'B')
                    throw new Exception("--Solo pueden anularse Facturas o Notas Credito/Debito relacionadas por este medio.");

                if (string.IsNullOrWhiteSpace(motivo))
                    throw new Exception("--Debe incluir el motivo de anulación del comprobante a anular.");

                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), ruc);
                ComprobanteVenta obj = await GestorFacturacion.ObtenerComprobanteParaAnulacion(serie, numero, tipoDoc, motivo, comercio);

                SoapFacturacion servicio;
                if (!comercio.Nubefact)
                    servicio = new ServicioSunat(comercio, 1);
                else
                    servicio = new ServicioNubefact(comercio, 1);

                servicio.Iniciar();
                string rutaPrincipal = "";

                if (obj.EstadoSunat == "Aceptado")
                {
                    DateTime minimo = DateTime.Now.AddDays(-7);
                    if (obj.FechaEmision < new DateTime(minimo.Year, minimo.Month, minimo.Day))
                    {
                        throw new Exception("--Se venció el plazo para anular por comunicación de baja. Sírvase usar una nota de crédito.");
                    }

                    comunicacion = new ComunicacionBaja();
                    comunicacion.Serie = DateTime.Now.ToString("yyyyMMdd");
                    comunicacion.Numero = await DalComunicacionBaja.ObtenerNumeroComunicacionBaja(comunicacion.Serie, comercio);
                    comunicacion.Comercio = comercio;
                    comunicacion.IdUsuarioRegistro = obj.IdUsuarioConfirmacionSunat;
                    comunicacion.FechaFacturas = obj.FechaEmision;
                    comunicacion.ListaFacturas = new List<ComprobanteVenta>();
                    comunicacion.ListaFacturas.Add(obj);
                    
                    comunicacion.ActualizarUbicacionArchivos();
                    rutaPrincipal = ObtenerRutaPrincipal(comunicacion.RutaCarpetaArchivos);
                    string rutaBase = Server.MapPath("~/Content/files/" + comunicacion.Comercio.CarpetaServidor + "/");

                    GeneradorXml.GenerarXMLComunicacionBaja(comunicacion, comercio, rutaBase);
                    GeneradorXml.FirmarXml(rutaBase, 0, comunicacion.ArchivoXml, rutaPrincipal, comercio);

                    servicio.EnviarGrupoComprobantes(comunicacion, rutaPrincipal);
                    if (String.IsNullOrWhiteSpace(comunicacion.NroTicket))
                    {
                        throw new Exception("--Sunat no pudo devolver una respuesta.");
                    }

                    int resultado = await DalComunicacionBaja.InsertarComunicacionBaja(comunicacion);
                    if (resultado <= 0)
                        throw new Exception("Error al guardar en la base de datos.");

                    comunicacion.IdComunicacionBaja = resultado;
                    comunicacion.FechaRegistro = DateTime.Now;
                    obj.EstadoSunat = "Por anular";
                    Thread.Sleep(5000);
                }

                else if (obj.EstadoSunat == "Por anular" || obj.EstadoSunat == "Anulado")
                {
                    comunicacion = await DalComunicacionBaja.ObtenerComunicacionBajaPorDocumentoExterno(comercio, obj.IdComprobanteVenta);
                    comunicacion.ListaFacturas = new List<ComprobanteVenta>();
                    comunicacion.ListaFacturas.Add(obj);

                    //se actualizan los campos particulares
                    comunicacion.ActualizarUbicacionArchivos();
                    rutaPrincipal = ObtenerRutaPrincipal(comunicacion.RutaCarpetaArchivos);
                }

                if (obj.EstadoSunat == "Por anular")
                {
                    await servicio.ConsultarTicketGrupal(comunicacion, rutaPrincipal);

                    respuesta = new RespuestaContenedor(comunicacion, true);
                    if (comunicacion.EstadoSunat == "Aceptado" || comunicacion.EstadoSunat == "Rechazado")
                    {
                        comunicacion.IdUsuarioConfirmacionSunat = comunicacion.IdUsuarioRegistro;
                        int resultado = await DalComunicacionBaja.ActualizarEnvioComunicacionBaja(comunicacion);
                        if (resultado <= 0)
                            throw new Exception("Error al guardar en la base de datos.");
                    }
                }

                servicio.Finalizar();

                if (comunicacion != null && respuesta == null)
                {
                    respuesta = new RespuestaContenedor(comunicacion, false);
                }

                if (obj.EstadoSunat == "Rechazado")
                {
                    throw new Exception("--El comprobante ya fue rechazado por Sunat, no necesita anularlo.");
                }
                else if (respuesta == null)
                {
                    throw new Exception("--Debe informar primero el comprobante para poder anularlo.");
                }
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);

                respuesta = new RespuestaContenedor();
                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Error inesperado durante la ejecución";
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AnularComprobante(string serie, string numero, string token, string ruc)
        {
            Comercio comercio = null;
            Respuesta respuesta = new Respuesta();

            try
            {
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, ObtenerDataPeticion(), ruc);
                ComprobanteVenta obj = await GestorFacturacion.ObtenerNotaCreditoAnulacion(serie, numero, comercio);
                
                string ruta = ObtenerRutaPrincipal(obj.RutaCarpetaArchivos);
                await CrearArchivoXmlComprobante(obj, ruta);

                SoapFacturacion servicio = new ServicioSunat(comercio, 1);
                await servicio.EnviarComprobanteDetallado(obj, ruta);

                if (String.IsNullOrWhiteSpace(obj.CodigoErrorSunat))
                    throw new Exception("--Sunat no pudo devolver una respuesta.");

                respuesta = new Respuesta(obj, true);
                if (obj.EstadoSunat == "Aceptado" || obj.EstadoSunat == "Rechazado")
                    obj.IdUsuarioConfirmacionSunat = 21;

                int resultado = await DalComprobanteVenta.ActualizarEnvioComprobanteVenta(obj);
                if (resultado <= 0)
                    throw new Exception("--Error al guardar en la base de datos.");
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);

                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Error inesperado durante la ejecución";
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ConsultarPorRuc(string ruc, string token)
        {
            RespuestaServicio respuesta = new RespuestaServicio();

            try
            {
                if (ruc.Length != 11)
                    respuesta.ExcepcionApi = "El ruc especificado es incorrecto.";
                else
                {
                    string[] keys = { "token", "ruc" };
                    string[] values = { token, ruc };
                    string output = await ConexionExterna.GestionarTokenPHP(keys, values);

                    if (String.IsNullOrWhiteSpace(output))
                        respuesta.ExcepcionApi = "No se pudo procesar la consulta.";
                    else
                    {
                        dynamic resultado = new JavaScriptSerializer().DeserializeObject(output);
                        if (resultado["estado"] != "ok")
                            respuesta.ExcepcionApi = resultado["estado"];
                        else if (resultado["empresa"] == null || resultado["empresa"]["ruc"] == "")
                            respuesta.ExcepcionApi = "La búsqueda no arrojó ningún resultado.";
                        else
                        {
                            int autorizacion = await DalComercio.AutorizarOperacionComercio(resultado["ruc"], token, "ruc", ruc);
                            if (autorizacion == -1)
                                respuesta.ExcepcionApi = "Su token ha expirado.";
                            else if (autorizacion == -2)
                                respuesta.ExcepcionApi = "No ha contratado el plan para realizar esta operación.";
                            else if (autorizacion == -3)
                                respuesta.ExcepcionApi = "Ha alcanzado el límite de operaciones en el mes.";
                            else
                            {
                                Empresa empresa = new Empresa()
                                {
                                    ruc = ruc,
                                    razonSocial = resultado["empresa"]["razonSocial"],
                                    estadoContribuyente = resultado["empresa"]["estadoContribuyente"],
                                    condicionDomicilio = resultado["empresa"]["condicionDomicilio"],
                                    tipoVia = resultado["empresa"]["tipoVia"],
                                    nombreVia = resultado["empresa"]["nombreVia"],
                                    numero = resultado["empresa"]["numero"],
                                    kilometro = resultado["empresa"]["kilometro"],
                                    tipoZona = resultado["empresa"]["tipoZona"],
                                    codigoZona = resultado["empresa"]["codigoZona"],
                                    manzana = resultado["empresa"]["manzana"],
                                    lote = resultado["empresa"]["lote"],
                                    departamento = resultado["empresa"]["departamento"],
                                    interior = resultado["empresa"]["interior"],
                                    ubigeo = resultado["empresa"]["ubigeo"]
                                };
                                respuesta.empresa = empresa;
                                respuesta.MensajeSunat = "Autorización conforme.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = ex.Message;
                var localException = new clsException(ex, null);
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> ConsultarPorDni(string dni, string token)
        {
            string valor = "";
            int codigoResultado = 0;

            try
            {
                if (dni.Length != 8)
                    throw new Exception("--El dni especificado es incorrecto.");
                
                string[] keys = { "token", "dni" };
                string[] values = { token, dni };
                string output = await ConexionExterna.GestionarTokenPHP(keys, values);

                if (String.IsNullOrWhiteSpace(output))
                    throw new Exception("--No se pudo procesar la consulta.");
                    
                dynamic resultado = new JavaScriptSerializer().DeserializeObject(output);
                
                if (resultado["estado"] != "ok")
                    throw new Exception("--" + resultado["estado"]);
                
                if (resultado["persona"] == "")
                    throw new Exception("--La búsqueda no arrojó ningún resultado.");
                
                codigoResultado = await DalComercio.AutorizarOperacionComercio(resultado["ruc"], token, "ruc", dni);

                if (codigoResultado == -1)
                    throw new Exception("--Su token ha expirado.");

                if (codigoResultado == -2)
                    throw new Exception("--No ha contratado el plan para realizar esta operación");

                if (codigoResultado == -3)
                    throw new Exception("--Ha alcanzado el límite de operaciones en el mes.");
                
                codigoResultado = 1;
                valor = resultado["persona"];
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                if (ex.Message.Substring(0, 2) == "--")
                    valor = ex.Message;
                else
                    valor = "Error inesperado durante la ejecución";
            }
            return Json(new { valor, codigoResultado }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ObtenerPersonaPorDni (string dni, string token)
        {
            return await ConsultarPorDni(dni, token);
        }
    }
}