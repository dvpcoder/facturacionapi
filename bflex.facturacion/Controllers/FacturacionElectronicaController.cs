using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using bflex.facturacion.SunatCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Rotativa;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Threading.Tasks;

namespace bflex.facturacion.Controllers
{
    //version en desuso, lo usan algunos clientes
    public class FacturacionElectronicaController : Controller
    {
        public string ObtenerRutaArchivos(ComprobanteVenta comprobante, Comercio comercio)
        {
            string rutaFinal = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/" +
            comprobante.TipoComprobante.carpeta + "/" + comprobante.FechaEmision.ToString("yyyyMM") + "/" +
            comprobante.Serie + "-" + comprobante.Numero + "/");

            if (!Directory.Exists(rutaFinal))
                Directory.CreateDirectory(rutaFinal);
            
            return rutaFinal;
        }

        public async Task<ActionResult> OpInformarComprobanteSunat(DocumentoVenta documento, int idComercio, int produccion)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);
                    if (comprobante != null)
                    {
                        string ruta = ObtenerRutaArchivos(comprobante, comercio);
                        string rutaBase = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                        string nombreArchivo = comercio.Ruc + "-" + comprobante.CodigoTipoComprobante + "-" +
                            comprobante.Serie + "-" + comprobante.Numero;

                        if (comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                        {
                            if (produccion == 1)
                            {
                                bool esFactura = comprobante.Serie[0] == 'F' || comprobante.CodigoTipoComprobante.Equals("01");
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

                                bool vencido = comprobante.FechaEmision < minimo;
                                bool mismoPeriodo = comprobante.FechaEmision.Month == minimo.Month;

                                if (vencido && esFactura && mismoPeriodo)
                                {
                                    throw new Exception(
                                        "--Comprobante registrado. Puede regularizar fechas para su aprobación."
                                    );
                                }

                                if (vencido && !esFactura)
                                {
                                    throw new Exception(
                                        "--Comprobante registrado. Puede regularizar por resumen de boletas."
                                    );
                                }
                            }

                            await DalComprobanteVenta.InsertarDocumentoVenta(documento, comercio.IdComercio);
                            GeneradorXml.GenerarDocumentoXMLUBL21Comprobante(comprobante, comercio, rutaBase);
                            GeneradorXml.FirmarXml(rutaBase, 0, nombreArchivo, ruta, comercio);

                            SoapFacturacion servicio;
                            if (!comercio.Nubefact)
                                servicio = new ServicioSunat(comercio, produccion);
                            else
                                servicio = new ServicioNubefact(comercio, produccion);

                            servicio.EnviarComprobanteDetallado(ref respuesta, ruta, nombreArchivo);

                            comprobante.MensajeSunat = respuesta.MensajeSunat;
                            comprobante.CodigoErrorSunat = respuesta.CodigoErrorSunat;

                            if (String.IsNullOrWhiteSpace(respuesta.CodigoErrorSunat))
                                throw new Exception("--Error no controlado en la conexión");

                            if (respuesta.CodigoErrorSunat.Contains("."))
                            {
                                string code = respuesta.CodigoErrorSunat.Split('.')[1];
                                int valor = -1;
                                if (Int32.TryParse(code, out valor))
                                {
                                    if (respuesta.CodigoErrorSunat.Equals("sunat.0") || valor > 4000 || valor == 1033)
                                    {
                                        if (respuesta.CodigoErrorSunat.Equals("Client.1033"))
                                            respuesta.EstadoSunat = "En espera";
                                        else
                                        {
                                            respuesta.EstadoSunat = "Aceptado";
                                            comprobante.IdUsuarioConfirmacionSunat = documento.UsuarioRegistra.id;
                                        }
                                    }
                                    else if (valor == 1032)
                                    {
                                        if (!comprobante.Valido)
                                            respuesta.EstadoSunat = "Anulado";
                                        else
                                            respuesta.EstadoSunat = "Rechazado";
                                        comprobante.IdUsuarioConfirmacionSunat = documento.UsuarioRegistra.id;
                                    }
                                    else respuesta.EstadoSunat = "Con error";
                                }
                                else respuesta.EstadoSunat = "Con error";
                            }
                            else respuesta.EstadoSunat = "Sin envío";

                            comprobante.EstadoSunat = respuesta.EstadoSunat;
                        }
                        else
                        {
                            respuesta.EstadoSunat = comprobante.EstadoSunat;
                            respuesta.MensajeSunat = comprobante.MensajeSunat;
                            respuesta.CodigoErrorSunat = comprobante.CodigoErrorSunat;
                        }

                        if (comprobante.EstadoSunat == "En espera")
                        {
                            ConsultaSunat consulta = new ConsultaSunat(comercio);
                            consulta.ConsultarComprobanteInformado(ref respuesta, ruta, nombreArchivo, comercio.Ruc, comprobante);

                            comprobante.MensajeSunat = respuesta.MensajeSunat;
                            comprobante.CodigoErrorSunat = respuesta.CodigoErrorSunat;

                            if (String.IsNullOrWhiteSpace(respuesta.CodigoErrorSunat))
                                throw new Exception("--Error no controlado en la conexión");

                            if (respuesta.CodigoErrorSunat.Equals("sunat.0"))
                            {
                                respuesta.EstadoSunat = "Aceptado";
                                comprobante.IdUsuarioConfirmacionSunat = documento.UsuarioRegistra.id;
                            }
                            else respuesta.EstadoSunat = "Con error";

                            comprobante.EstadoSunat = respuesta.EstadoSunat;
                        }

                        if (produccion == 1)
                        {
                            int resultado = await DalComprobanteVenta.ActualizarEnvioComprobanteVenta(comprobante);
                            if (resultado <= 0)
                            {
                                respuesta.ExcepcionApi = "Error al actualizar los resultados.";
                            }
                        }
                    }
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpInformarBoletasPorResumenDiario(
            int idComercio, List<DocumentoVenta> listaDocumentos, int produccion, string fecha, int idUsuario
        )
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            ResumenBoletas resumen = new ResumenBoletas();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    resumen.ListaBoletas = new List<ComprobanteVenta>();
                    string[] fechaSeparada = fecha.Split('/');
                    resumen.FechaBoletas = new DateTime(
                        Convert.ToInt32(fechaSeparada[2]), Convert.ToInt32(fechaSeparada[1]), Convert.ToInt32(fechaSeparada[0])
                    );
                    DateTime minimo = DateTime.Now.AddDays(-7);
                    bool restriccion = produccion == 1 && resumen.FechaBoletas < new DateTime(minimo.Year, minimo.Month, minimo.Day);

                    foreach (DocumentoVenta documento in listaDocumentos)
                    {
                        documento.UsuarioRegistra = new Usuario() { id = idUsuario };
                        if (documento.DetalleDocumento == null || documento.DetalleDocumento.Count == 0 || documento.DetalleDocumento[0] == null)
                            documento.DetalleDocumento = new List<DetalleDocumentoVenta>();

                        ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);

                        if (comprobante.Valido && (comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error"))
                        {
                            comprobante.CodigoEstadoResumen = "1";
                            resumen.ListaBoletas.Add(comprobante);
                            await DalComprobanteVenta.InsertarDocumentoVenta(documento, comercio.IdComercio);
                            if (resumen.ListaBoletas.Count == 500) break;
                        }

                        if (!comprobante.Valido && !restriccion)
                        {
                            if (comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                            {
                                comprobante.CodigoEstadoResumen = "1";
                                resumen.ListaBoletas.Add(comprobante);
                                await DalComprobanteVenta.InsertarDocumentoVenta(documento, comercio.IdComercio);
                                if (resumen.ListaBoletas.Count == 500) break;

                                ComprobanteVenta copia = new ComprobanteVenta();
                                foreach (PropertyInfo property in typeof(ComprobanteVenta).GetProperties().Where(p => p.CanWrite))
                                {
                                    property.SetValue(copia, property.GetValue(comprobante, null), null);
                                }
                                copia.CodigoEstadoResumen = "3";
                                resumen.ListaBoletas.Add(copia);
                                if (resumen.ListaBoletas.Count == 500) break;
                            }
                            else if (comprobante.EstadoSunat == "Aceptado")
                            {
                                comprobante.CodigoEstadoResumen = "3";
                                resumen.ListaBoletas.Add(comprobante);
                                await DalComprobanteVenta.InsertarDocumentoVenta(documento, comercio.IdComercio);
                                if (resumen.ListaBoletas.Count == 500) break;
                            }
                        }
                    }

                    if (resumen.ListaBoletas.Count > 0)
                    {
                        resumen.Serie = DateTime.Now.ToString("yyyyMMdd");
                        resumen.Numero = await DalResumenBoletas.ObtenerNumeroResumenBoletas(resumen.Serie, comercio);
                        resumen.Comercio = new Comercio { 
                            IdComercio = idComercio, CarpetaServidor = comercio.CarpetaServidor
                        };
                        resumen.IdUsuarioRegistro = idUsuario;

                        string rutaBase = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                        string ruta = rutaBase + "Resumenes/" + resumen.FechaBoletas.ToString("yyyyMM") + "/" + resumen.Serie + "-" +
                            resumen.Numero + "/";
                        string nombreArchivo = comercio.Ruc + "-RC-" + resumen.Serie + "-" + resumen.Numero;

                        if (!Directory.Exists(ruta))
                            Directory.CreateDirectory(ruta);

                        GeneradorXml.GenerarXMLResumenBoletas(resumen, comercio, rutaBase);
                        GeneradorXml.FirmarXml(rutaBase, 0, nombreArchivo, ruta, comercio);

                        SoapFacturacion servicio;
                        if (!comercio.Nubefact)
                            servicio = new ServicioSunat(comercio, produccion);
                        else
                            servicio = new ServicioNubefact(comercio, produccion);

                        servicio.Iniciar();
                        resumen.NroTicket = servicio.EnviarGrupoComprobantes(ref respuesta, ruta, nombreArchivo);

                        if (!String.IsNullOrWhiteSpace(resumen.NroTicket))
                        {
                            if (produccion == 1)
                            {
                                int resultado = await DalResumenBoletas.InsertarResumenBoletas(resumen);
                                if (resultado > 0)
                                {
                                    resumen.IdResumenBoletas = resultado;
                                    resumen.FechaBoletas2 = fecha;
                                    resumen.FechaRegistro = DateTime.Now;

                                    Thread.Sleep(5000);
                                    servicio.ConsultarTicketGrupal(ref respuesta, ruta, nombreArchivo, resumen.NroTicket);

                                    if (String.IsNullOrWhiteSpace(respuesta.CodigoErrorSunat))
                                        throw new Exception("--Error no controlado en la conexión");

                                    if (respuesta.CodigoErrorSunat.Contains("sunat."))
                                    {
                                        await GestorFacturacion.ActualizarEstadoEnvioResumen(resumen, respuesta, idUsuario);
                                    }
                                    else
                                    {
                                        resumen.EstadoSunat = "En espera";
                                        respuesta.MensajeSunat = "Se creó el resumen satisfactoriamente.";
                                    }
                                }

                                else respuesta.ExcepcionApi = "Error al guardar en la base de datos.";
                            }

                            else respuesta.MensajeSunat = "Se creó resumen de prueba satisfactoriamente.";
                        }
                        else if (String.IsNullOrWhiteSpace(respuesta.MensajeSunat))
                        {
                            respuesta.CodigoErrorSunat = "sunat.-1";
                            respuesta.MensajeSunat = "Sunat no pudo registrar el resumen de boletas.";
                        }
                        servicio.Finalizar();
                        //si no hay el ticket ya debe de haber cargado una excepcion y solo toca retornar
                    }
                    else
                    {
                        //buscar del primer doc no admitido a que resumen le pertenece para devolverlo...?
                        resumen = await DalResumenBoletas.ObtenerResumenBoletasPorDocumentoExterno(
                            comercio, listaDocumentos[0].IdDocumentoVenta, false
                        );
                        if (resumen != null)
                        {
                            resumen.ListaBoletas = await DalResumenBoletas.ObtenerDetalleResumenPorIdResumen(resumen.IdResumenBoletas);
                            if (resumen.EstadoSunat != "En espera")
                                respuesta.MensajeSunat = resumen.MensajeSunat;
                            else
                                respuesta.MensajeSunat = "Se notificó el resumen en espera más próximo.";
                        }
                        else
                        {
                            respuesta.ExcepcionApi = "No se puede enviar los comprobantes seleccionados. No se encontró resumen que los contenga.";
                        }
                    }
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Substring(0, 2) == "--")
                    respuesta.ExcepcionApi = ex.Message;
                else
                    respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta, resumen }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpConsultarTicketResumen(int idComercio, string serie, int numero, int idUsuario)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            ResumenBoletas resumen = new ResumenBoletas();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    resumen = await DalResumenBoletas.ObtenerResumenPorNumeracionComercio(serie, numero, comercio);

                    if (resumen != null)
                    {
                        if (resumen.EstadoSunat == "En espera")
                        {
                            string ruta = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/Resumenes/" +
                                resumen.FechaBoletas.ToString("yyyyMM") + "/" + resumen.Serie + "-" + resumen.Numero + "/");
                            string nombreArchivo = comercio.Ruc + "-RC-" + resumen.Serie + "-" + resumen.Numero;

                            SoapFacturacion servicio;
                            if (!comercio.Nubefact)
                                servicio = new ServicioSunat(comercio, 1);
                            else
                                servicio = new ServicioNubefact(comercio, 1);

                            servicio.Iniciar();
                            servicio.ConsultarTicketGrupal(ref respuesta, ruta, nombreArchivo, resumen.NroTicket);
                            servicio.Finalizar();

                            await GestorFacturacion.ActualizarEstadoEnvioResumen(resumen, respuesta, idUsuario);
                        }
                        else
                        {
                            respuesta.MensajeSunat = resumen.MensajeSunat;
                        }
                    }

                    else respuesta.ExcepcionApi = "No se ha podido encontrar el resumen";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                //respuesta.ExcepcionBflex = ex.Message;
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta, resumen }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpGestionarArchivosResumen(int idComercio, string serie, int numero, string tipo)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    string rutaWeb = "/Content/files/" + comercio.CarpetaServidor + "/Resumenes/" + serie.Substring(0, 6) +
                        "/" + serie + "-" + numero + "/";
                    string rutaFisica = Server.MapPath("~" + rutaWeb);

                    string archivo = comercio.Ruc + "-RC-" + serie + "-" + numero + ".zip";
                    if (tipo == "cdr") archivo = "CDR-" + archivo;

                    FileInfo file = new FileInfo(rutaFisica + archivo);
                    if (file.Exists) respuesta.rutaArchivo = rutaWeb + archivo;
                    else respuesta.ExcepcionApi = "No se encuentra el archivo especificado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                //respuesta.ExcepcionBflex = ex.Message;
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpGestionarBajaComprobante(int idComercio, DocumentoVenta documento)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            ComunicacionBaja comunicacion = new ComunicacionBaja();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    string rutaBase = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                    string ruta = "", nombreArchivo = "";

                    documento.DetalleDocumento = new List<DetalleDocumentoVenta>();
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);

                    SoapFacturacion servicio;
                    if (!comercio.Nubefact)
                        servicio = new ServicioSunat(comercio, 1);
                    else
                        servicio = new ServicioNubefact(comercio, 1);

                    servicio.Iniciar();

                    if (comprobante.EstadoSunat == "Aceptado" && !comprobante.Valido)
                    {
                        if (!String.IsNullOrWhiteSpace(comprobante.MotivoAnulacion))
                        {
                            comunicacion.ListaFacturas = new List<ComprobanteVenta>();
                            comunicacion.ListaFacturas.Add(comprobante);
                            comunicacion.Serie = DateTime.Now.ToString("yyyyMMdd");
                            comunicacion.Numero = await DalComunicacionBaja.ObtenerNumeroComunicacionBaja(
                                comunicacion.Serie, comercio
                            );
                            comunicacion.Comercio = new Comercio {
                                IdComercio = idComercio, CarpetaServidor = comercio.CarpetaServidor
                            };
                            comunicacion.IdUsuarioRegistro = documento.UsuarioRegistra.id;
                            comunicacion.FechaFacturas = comprobante.FechaEmision;

                            ruta = rutaBase + "Comunicaciones/" + comunicacion.Serie.Substring(0, 6) + "/" + 
                                comunicacion.Serie + "-" + comunicacion.Numero + "/";
                            nombreArchivo = comercio.Ruc + "-RA-" + comunicacion.Serie + "-" + comunicacion.Numero;

                            if (!Directory.Exists(ruta))
                                Directory.CreateDirectory(ruta);

                            GeneradorXml.GenerarXMLComunicacionBaja(comunicacion, comercio, rutaBase);
                            GeneradorXml.FirmarXml(rutaBase, 0, nombreArchivo, ruta, comercio);

                            comunicacion.NroTicket = servicio.EnviarGrupoComprobantes(ref respuesta, ruta, nombreArchivo);

                            if (!String.IsNullOrWhiteSpace(comunicacion.NroTicket))
                            {
                                int resultado = await DalComunicacionBaja.InsertarComunicacionBaja(comunicacion);
                                if (resultado > 0)
                                {
                                    comunicacion.IdComunicacionBaja = resultado;
                                    comunicacion.FechaFacturas2 = comunicacion.FechaFacturas.ToString("dd/MM/yyyy");
                                    comunicacion.FechaRegistro = DateTime.Now;
                                    comunicacion.EstadoSunat = "En espera";
                                    comprobante.EstadoSunat = "Por anular";

                                    respuesta.MensajeSunat = "Se ha iniciado la anulación del comprobante.";
                                    Thread.Sleep(5000);
                                }

                                else respuesta.ExcepcionApi = "Error al guardar en la base de datos.";
                            }
                            else if (String.IsNullOrWhiteSpace(respuesta.MensajeSunat))
                            {
                                respuesta.CodigoErrorSunat = "sunat.-1";
                                respuesta.MensajeSunat = "Sunat no pudo registrar la comunicación de baja.";
                            }
                        }

                        else respuesta.ExcepcionApi = "El motivo de anulación de la factura es requerido.";
                    }
                    
                    if (comprobante.EstadoSunat == "Por anular")
                    {
                        if (comunicacion.ListaFacturas == null)
                        {
                            comunicacion = await DalComunicacionBaja.ObtenerComunicacionBajaPorDocumentoExterno(
                                comercio, comprobante.IdComprobanteVenta
                            );
                            ruta = rutaBase + "Comunicaciones/" + comunicacion.Serie.Substring(0, 6) + "/" +
                                comunicacion.Serie + "-" + comunicacion.Numero + "/";
                            nombreArchivo = comercio.Ruc + "-RA-" + comunicacion.Serie + "-" + comunicacion.Numero;
                        }

                        if (comunicacion != null)
                        {
                            servicio.ConsultarTicketGrupal(ref respuesta, ruta, nombreArchivo, comunicacion.NroTicket);

                            comunicacion.CodigoErrorSunat = respuesta.CodigoErrorSunat;
                            comunicacion.MensajeSunat = respuesta.MensajeSunat;

                            if (!String.IsNullOrWhiteSpace(respuesta.CodigoStatus))
                            {
                                comunicacion.CodigoStatus = respuesta.CodigoStatus;

                                if (respuesta.CodigoStatus.Contains("98"))
                                {
                                    if (DateTime.Now.ToString("yyyyMMdd") == comunicacion.Serie)
                                    {
                                        comunicacion.MensajeSunat = "La comunicación aún no ha sido procesada";
                                        comunicacion.EstadoSunat = "En espera";
                                    }
                                    else
                                    {
                                        comunicacion.EstadoSunat = "Rechazado";
                                    }
                                }
                                else if (respuesta.CodigoStatus.Equals("0"))
                                {
                                    comunicacion.EstadoSunat = "Aceptado";
                                }
                                else if (respuesta.CodigoErrorSunat.Equals("sunat.2323"))
                                {
                                    comunicacion.EstadoSunat = "Aceptado";
                                    comunicacion.MensajeSunat = "Comprobación satisfactoria de anulación";
                                }
                                else
                                {
                                    comunicacion.EstadoSunat = "Rechazado";
                                }
                            }
                            else
                            {
                                comunicacion.CodigoStatus = "99";
                                comunicacion.EstadoSunat = "Rechazado";
                            }

                            if (comunicacion.EstadoSunat == "Aceptado" || comunicacion.EstadoSunat == "Rechazado")
                            {
                                comunicacion.IdUsuarioConfirmacionSunat = documento.UsuarioRegistra.id;
                                comunicacion.FechaConfirmacionSunat = DateTime.Now;

                                int resultado = await DalComunicacionBaja.ActualizarEnvioComunicacionBaja(comunicacion);
                                if (resultado <= 0)
                                    respuesta.ExcepcionApi = "Error al actualizar los resultados.";
                                else
                                    comprobante.EstadoSunat = "Anulado";
                            }
                        }

                        else respuesta.ExcepcionApi = "No se ha podido recuperar el estado de anulación.";
                    }

                    servicio.Finalizar();

                    if (comprobante.EstadoSunat == "Anulado")
                    {
                        if (comunicacion.ListaFacturas == null)
                        {
                            comunicacion = await DalComunicacionBaja.ObtenerComunicacionBajaPorDocumentoExterno(
                                comercio, comprobante.IdComprobanteVenta
                            );
                        }

                        if (comunicacion != null)
                            respuesta.MensajeSunat = comunicacion.MensajeSunat;
                        else
                            respuesta.ExcepcionApi = "No se ha podido recuperar el estado de anulación.";
                    }                
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta, comunicacion }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpObtenerCdrFacturas(int idComercio, string serie, string numero)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    ComprobanteVenta comprobante = await DalComprobanteVenta.ObtenerComprobantePorNumeracionComercio(
                        serie, numero, comercio, ""
                    );

                    if (comprobante != null)
                    {
                        comprobante.ActualizarTipoComprobante();
                        string rutaWeb = "/Content/files/" + comercio.CarpetaServidor + "/" + 
                            comprobante.TipoComprobante.carpeta + "/" + comprobante.FechaEmision.ToString("yyyyMM") + "/" + 
                            serie + "-" + numero + "/";
                        string rutaFisica = Server.MapPath("~" + rutaWeb);
                        string archivo = "CDR-" + comercio.Ruc + "-" + comprobante.CodigoTipoComprobante + "-" + serie + "-" + 
                            numero + ".zip";

                        FileInfo file = new FileInfo(rutaFisica + archivo);
                        if (file.Exists) respuesta.rutaArchivo = rutaWeb + archivo;
                        else respuesta.ExcepcionApi = "No se encuentra el archivo especificado.";
                    }
                    
                    else respuesta.ExcepcionApi = "No se encuentra el documento solicitado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpObtenerXmlComprobantes (int idComercio, DocumentoVenta documento)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);
                    if (comprobante != null)
                    {
                        string rutaFisica = ObtenerRutaArchivos(comprobante, comercio);
                        string rutaWeb = rutaFisica.Substring(rutaFisica.IndexOf("Content")).Replace("\\", "/");
                        string archivo = comercio.Ruc + "-" + comprobante.CodigoTipoComprobante + "-" +
                            comprobante.Serie + "-" + comprobante.Numero;

                        FileInfo file = new FileInfo(rutaFisica + archivo + ".zip");
                        if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                        {
                            string rutaBase = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                            GeneradorXml.GenerarDocumentoXMLUBL21Comprobante(comprobante, comercio, rutaBase);
                            GeneradorXml.FirmarXml(rutaBase, 0, archivo, rutaFisica, comercio);
                        }
                        
                        respuesta.rutaArchivo = "/" + rutaWeb + archivo + ".zip";
                    }

                    else respuesta.ExcepcionApi = "No se encuentra el documento solicitado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpObtenerPdfComprobantes(int idComercio, DocumentoVenta documento)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);
                    if (comprobante != null)
                    {
                        string rutaFisica = ObtenerRutaArchivos(comprobante, comercio);
                        string rutaWeb = rutaFisica.Substring(rutaFisica.IndexOf("Content")).Replace("\\", "/");
                        string archivo = comprobante.Serie + "-" + comprobante.Numero;

                        FileInfo file = new FileInfo(rutaFisica + archivo + ".pdf");
                        if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                        {
                            GestorFacturacion.ObtenerCodigoQr(comprobante, comercio, rutaFisica, archivo);
                            ActionAsPdf impresion = new ActionAsPdf("ObtenerVistaComprobantePdf", new {
                                idComercio, serie = comprobante.Serie, numero = comprobante.Numero, rutaWeb
                            });
                            byte[] datos = impresion.BuildFile(ControllerContext);

                            if (datos.Length > 0)
                            {
                                using (FileStream fs = new FileStream(
                                    rutaFisica + archivo + ".pdf", FileMode.Create, FileAccess.Write
                                ))
                                {
                                    fs.Write(datos, 0, datos.Length);
                                }
                            }
                            else respuesta.ExcepcionApi = "Por alguna razón no se puede crear el pdf.";
                        }

                        respuesta.rutaArchivo = "/" + rutaWeb + archivo + ".pdf";
                    }

                    else respuesta.ExcepcionApi = "No se encuentra el documento solicitado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ObtenerVistaComprobantePdf(int idComercio, string serie, string numero, string rutaWeb)
        {
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);
                ComprobanteVenta comprobante = await DalComprobanteVenta.ObtenerComprobantePorNumeracionComercio(
                    serie, numero, new Comercio { IdComercio = idComercio }, ""
                );
                comprobante.DetalleVenta = await DalComprobanteVenta.ObtenerDetalleComprobanteVentaPorId(comprobante.IdComprobanteVenta);
                comprobante.DetalleCuota = await DalComprobanteVenta.ObtenerDetalleCuotaPorIdComprobante(comprobante.IdComprobanteVenta);
                comprobante.ActualizarTipoComprobante();

                ViewBag.Comprobante = comprobante;
                ViewBag.Comercio = comercio;
                ViewBag.RutaQr = "~/" + rutaWeb + "QR-" + serie + "-" + numero + ".png";
                ViewBag.NumeroLetras = Utilitarios.NumeroALetras(comprobante.Total.ToString());

                if (comprobante.CodigoTipoComprobante == "01" || comprobante.CodigoTipoComprobante == "03")
                    return View("PdfFacturaBoletaV1");
                else if (comprobante.CodigoTipoComprobante == "07" || comprobante.CodigoTipoComprobante == "08")
                    return View("PdfNotaCreditoDebitoV1");
                else return Content("No es un comprobante correcto");
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                return Content("Ocurrió un error inesperado");
            }
        }

        public async Task<ActionResult> OpEnviarArchivosPorMail(int idComercio, DocumentoVenta documento)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    ComprobanteVenta comprobante = await GestorFacturacion.GestionarComprobanteSunat(comercio, documento, respuesta);
                    if (comprobante != null)
                    {
                        if (!String.IsNullOrWhiteSpace(comprobante.EmailCliente))
                        {
                            string rutaFisica = ObtenerRutaArchivos(comprobante, comercio);
                            string archivoPdf = comprobante.Serie + "-" + comprobante.Numero;
                            string archivoXml = comercio.Ruc + "-" + comprobante.CodigoTipoComprobante + "-" +
                                comprobante.Serie + "-" + comprobante.Numero;
                            string archivoCdr = "CDR-" + archivoXml;
                            string[] adjuntos = new string[3];
                            FileInfo file;

                            file = new FileInfo(rutaFisica + archivoPdf + ".pdf");
                            if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                            {
                                GestorFacturacion.ObtenerCodigoQr(comprobante, comercio, rutaFisica, archivoPdf);
                                ActionAsPdf impresion = new ActionAsPdf("ObtenerVistaComprobantePdf", new
                                {
                                    idComercio, serie = comprobante.Serie, numero = comprobante.Numero, 
                                    rutaWeb = rutaFisica.Substring(rutaFisica.IndexOf("Content")).Replace("\\", "/")
                                });
                                byte[] datos = impresion.BuildFile(ControllerContext);

                                using (FileStream fs = new FileStream(
                                    rutaFisica + archivoPdf + ".pdf", FileMode.Create, FileAccess.Write
                                ))
                                {
                                    fs.Write(datos, 0, datos.Length);
                                }
                            }
                            adjuntos[0] = rutaFisica + archivoPdf + ".pdf";

                            file = new FileInfo(rutaFisica + archivoXml + ".zip");
                            if (!file.Exists || comprobante.EstadoSunat == "Sin envío" || comprobante.EstadoSunat == "Con error")
                            {
                                string rutaBase = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                                GeneradorXml.GenerarDocumentoXMLUBL21Comprobante(comprobante, comercio, rutaBase);
                                GeneradorXml.FirmarXml(rutaBase, 0, archivoXml, rutaFisica, comercio);
                            }
                            adjuntos[1] = rutaFisica + archivoXml + ".zip";

                            file = new FileInfo(rutaFisica + archivoCdr);
                            if(comprobante.Serie.Substring(0, 1).Equals("F") && file.Exists)
                            {
                                adjuntos[2] = rutaFisica + archivoCdr + ".zip";
                            }

                            string rutaMailer = Server.MapPath("~/Content/files/mailer.html");
                            string contenido = System.IO.File.ReadAllText(rutaMailer);
                            contenido = contenido.Replace("{NombreCliente}", comprobante.NombreCompletoCliente);
                            contenido = contenido.Replace("{TipoDoc}", comprobante.TipoComprobante.nombre);
                            contenido = contenido.Replace("{NroDoc}", comprobante.Serie + "-" + comprobante.Numero);
                            contenido = contenido.Replace("{anio}", DateTime.Now.Year.ToString());
                            contenido = contenido.Replace("{elaborado}", comercio.ElaboradoPor);
                            string asunto = comercio.NombreComercial + " te ha enviado la " + 
                                comprobante.TipoComprobante.nombre + " Electrónica Nro: " + comprobante.Serie + "-" + 
                                comprobante.Numero;

                            Mailer mailer = new Mailer();
                            mailer.EnviarCorreo(comprobante.EmailCliente, asunto, comercio, adjuntos, contenido);

                            respuesta.MensajeSunat = "Se han enviado correctamente los archivos al correo electrónico";
                        }

                        else respuesta.ExcepcionApi = "No se recepcionó el mail del cliente o es erróneo.";
                    }

                    else respuesta.ExcepcionApi = "No se encuentra el documento solicitado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> OpObtenerRegistroVentas(int idComercio, string periodo, string tipo, string correo)
        {
            RespuestaServicio respuesta = new RespuestaServicio();
            Comercio comercio = new Comercio();

            try
            {
                comercio = await DalComercio.ObtenerComercioPorId(idComercio);

                if (comercio != null)
                {
                    List<ComprobanteVenta> listaVenta = await DalComprobanteVenta.ObtenerVentasPorComercioPeriodo(comercio, periodo);
                    if (listaVenta.Count > 0)
                    {
                        string nombreTxt = "", nombreExcel = "";
                        string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/regventas/");
                        string rutaWeb = rutaFisica.Substring(rutaFisica.IndexOf("Content")).Replace("\\", "/");

                        if (!Directory.Exists(rutaFisica))
                            Directory.CreateDirectory(rutaFisica);

                        List<string> dataVentas = GestorFacturacion.GetReporteVentasDataStandard(listaVenta, periodo);

                        if (tipo == "excel" || tipo == "email")
                        {
                            nombreExcel = "regventas_" + comercio.Ruc + "_" + periodo + ".xlsx";
                            string[] encabezados = {
                                "REGISTRO DE VENTAS", comercio.RazonSocial, "RUC: " + comercio.Ruc, "PERIODO: " + periodo
                            };
                            int[] colDecimal = { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

                            DataTable dt = GestorFacturacion.ObtenerModeloDTRegistroVentas();
                            foreach (string reg in dataVentas)
                            {
                                string registro = reg.Remove(reg.Length - 1);
                                DataRow fila = dt.NewRow();
                                fila.ItemArray = registro.Split('|');
                                dt.Rows.Add(fila);
                            }

                            GestorFacturacion.CrearReporteExcel(encabezados, dt, colDecimal, rutaFisica + nombreExcel);
                        }

                        if (tipo == "txt" || tipo == "email")
                        {
                            string libro = "140100";
                            string OIMG = "1111";
                            nombreTxt = "LE" + comercio.Ruc + periodo + "00" + libro + "00" + OIMG + ".txt";
                            dataVentas.RemoveAt(dataVentas.Count - 1);

                            using (StreamWriter writer = new StreamWriter(rutaFisica + nombreTxt))
                            {
                                foreach (string registro in dataVentas)
                                {
                                    writer.WriteLine(registro);
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(respuesta.ExcepcionApi))
                        {
                            if (tipo == "txt")
                                respuesta.rutaArchivo = "/" + rutaWeb + nombreTxt;
                            else if (tipo == "excel")
                                respuesta.rutaArchivo = "/" + rutaWeb + nombreExcel;
                            else if (tipo == "email")
                            {
                                string[] adjuntos = { (rutaFisica + nombreTxt), (rutaFisica + nombreExcel) };
                                string rutaMailer = Server.MapPath("~/Content/files/mailer-reportes.html");
                                string contenido = System.IO.File.ReadAllText(rutaMailer);
                                contenido = contenido.Replace("{NombreEmpresa}", comercio.RazonSocial);
                                contenido = contenido.Replace("{periodo}", periodo);
                                contenido = contenido.Replace("{anio}", DateTime.Now.Year.ToString());
                                contenido = contenido.Replace("{elaborado}", comercio.ElaboradoPor);
                                string asunto = "Se ha enviado el reporte de ventas " + periodo;

                                Mailer mailer = new Mailer();
                                mailer.EnviarCorreo(correo, asunto, comercio, adjuntos, contenido);
                                respuesta.MensajeSunat = "Se han enviado correctamente los archivos al correo electrónico";
                            }
                            else respuesta.ExcepcionApi = "No es un formato esperado.";
                        }

                    }

                    else respuesta.ExcepcionApi = "El periodo solicitado no trajo ningún resultado.";
                }

                else respuesta.ExcepcionApi = "La identificacion del comercio es incorrecta.";
            }
            catch (Exception ex)
            {
                respuesta.ExcepcionApi = "Ha ocurrido un error inesperado en Nautilus.";
                var localException = new clsException(ex, comercio.CarpetaServidor);
            }
            return Json(new { respuesta }, JsonRequestBehavior.AllowGet);
        }
    }
}