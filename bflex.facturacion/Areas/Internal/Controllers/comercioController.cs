using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using bflex.facturacion.SunatCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace bflex.facturacion.Areas.Internal.Controllers
{
    public class comercioController : Controller
    {
        public string ObtenerDataPeticion()
        {
            return HttpContext.Request.ServerVariables["HTTP_USER_AGENT"];
        }

        public async Task<ActionResult> Insertar(Comercio comercio)
        {
            try
            {
                string autorizacion = await ConexionExterna.GestionarTokenPHP(
                    new string[] { "ruc" }, new string[] { comercio.Ruc }
                );
                if (autorizacion.Length < 10)
                    throw new Exception("No se obtuvo el token");

                comercio.AccessToken = autorizacion;
                int resultado = await DalComercio.InsertarComercioCompleto(comercio);

                string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/");
                Directory.CreateDirectory(rutaFisica);

                return Content(resultado.ToString());
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                return Content("Error: " + ex.Message);
            }
        }

        public async Task<ActionResult> Guardar(string comercio, HttpPostedFileBase file)
        {
            try
            {
                dynamic resultado = new JavaScriptSerializer().DeserializeObject(comercio);

                Comercio obj = new Comercio();
                obj.Cuenta = new Cuenta();
                obj.Cuenta.IdCuenta = resultado["Cuenta"]["IdCuenta"];
                obj.Ruc = resultado["Ruc"];
                obj.RazonSocial = resultado["RazonSocial"];
                obj.NombreComercial = resultado["NombreComercial"];
                obj.Email = resultado["Email"];
                obj.Celular = resultado["Celular"];
                obj.RepresentanteLegal = resultado["RepresentanteLegal"];
                obj.IdentidadRepresentante = resultado["IdentidadRepresentante"];
                obj.DescripcionNegocio = resultado["DescripcionNegocio"];
                obj.ElaboradoPor = resultado["ElaboradoPor"];
                obj.CarpetaServidor = resultado["CarpetaServidor"];
                obj.PasswordCertificado = resultado["PasswordCertificado"];
                obj.UsuarioSunat = resultado["UsuarioSunat"];
                obj.PasswordSunat = resultado["PasswordSunat"];
                obj.Normativa = resultado["Normativa"];
                obj.CalleFiscal = resultado["CalleFiscal"];
                obj.UrbanizacionFiscal = resultado["UrbanizacionFiscal"];
                obj.DepartamentoFiscal = resultado["DepartamentoFiscal"];
                obj.ProvinciaFiscal = resultado["ProvinciaFiscal"];
                obj.DistritoFiscal = resultado["DistritoFiscal"];
                obj.UbigeoFiscal = resultado["UbigeoFiscal"];

                string rutaFisica = Server.MapPath("~/Content/files/" + obj.CarpetaServidor + "/");
                if (!Directory.Exists(rutaFisica))
                    Directory.CreateDirectory(rutaFisica);

                string nombre = file.FileName.Substring(0, file.FileName.IndexOf('.'));
                obj.ArchivoCertificado = (nombre.Length > 46 ? nombre.Substring(0, 46) : nombre) + ".pfx";
                file.SaveAs(rutaFisica + obj.ArchivoCertificado);

                string autorizacion = await ConexionExterna.GestionarTokenPHP(
                    new string[] { "ruc" }, new string[] { obj.Ruc }
                );
                if (autorizacion.Length < 10)
                    throw new Exception("Error al guardar el comercio");
                
                obj.AccessToken = autorizacion;
                int idComercio = await DalComercio.GestionarComercio(obj);

                string[] keys = new string[] { "ruc", "aud" };
                string[] values = new string[] { obj.Ruc, ObtenerDataPeticion() };
                string token = await ConexionExterna.GestionarTokenPHP(keys, values);
                if (String.IsNullOrWhiteSpace(token))
                    throw new Exception("Error al crear la sesión");

                return Content(token);
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                return Content("Error: " + ex.Message);
            }
        }

        public async Task<ActionResult> AsignarToken(string ruc)
        {
            try
            {
                string autorizacion = await ConexionExterna.GestionarTokenPHP(new string[] { "ruc" }, new string[] { ruc });
                if (autorizacion.Length < 10)
                    throw new Exception("No se obtuvo el token");

                int resultado = await DalComercio.AsignarTokenComercio(ruc, autorizacion);
                return Content(resultado.ToString());
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                return Content("Error: " + ex.Message);
            }
        }

        public async Task<ActionResult> AutenticarUsuario(string email, string contrasenia, string ruc)
        {
            try
            {
                //TODO: añadir MD5 en el guardado de usuarios
                Comercio comercio = await DalComercio.AutenticarUsuarioLogeado(email, contrasenia, ruc);
                if (comercio == null)
                    throw new Exception("User o password erróneos.");

                string[] keys = new string[] { "ruc", "aud" };
                string[] values = new string[] { ruc, ObtenerDataPeticion() };
                string token = await ConexionExterna.GestionarTokenPHP(keys, values);
                if (String.IsNullOrWhiteSpace(token))
                    throw new Exception("No se pudo autorizar la operación");

                List<string> periodos = await DalComercio.ObtenerPeriodosVenta(comercio.IdComercio);
                return Json( new { comercio, periodos, token }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                return Content("Error: " + ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerExcelRegistroVentas(string token, string periodo)
        {
            Comercio comercio = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, metadata, "excelVentas");

                string nombreArchivo = "regventas_" + comercio.Ruc + "_" + periodo + ".xlsx";
                string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/regventas/");
                if (!Directory.Exists(rutaFisica))
                    Directory.CreateDirectory(rutaFisica);

                await GestorFacturacion.CrearArchivoExcelVentas(comercio, periodo, rutaFisica + nombreArchivo);

                return File(rutaFisica + nombreArchivo, "application/vnd.ms-excel", nombreArchivo);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerTxtRegistroVentas(string token, string periodo)
        {
            Comercio comercio = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, metadata, "excelVentas");

                string nombreArchivo = "LE" + comercio.Ruc + periodo + "00140100001111.txt";
                string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/regventas/");
                if (!Directory.Exists(rutaFisica))
                    Directory.CreateDirectory(rutaFisica);

                await GestorFacturacion.CrearArchivoTxtVentas(comercio, periodo, rutaFisica + nombreArchivo, false);
                
                return File(rutaFisica + nombreArchivo, "text/plain", nombreArchivo);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> ObtenerTxtRegistroVentasRVIE(string token, string periodo)
        {
            Comercio comercio = null;
            try
            {
                string metadata = ObtenerDataPeticion();
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, metadata, "excelVentas");

                string nombreArchivo = "LE" + comercio.Ruc + periodo + "00140400021112.txt";
                string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/regventas/");
                if (!Directory.Exists(rutaFisica))
                    Directory.CreateDirectory(rutaFisica);

                await GestorFacturacion.CrearArchivoTxtVentas(comercio, periodo, rutaFisica + nombreArchivo, true);

                return File(rutaFisica + nombreArchivo, "text/plain", nombreArchivo);
            }
            catch (Exception ex)
            {
                string carpeta = comercio != null ? comercio.CarpetaServidor : "";
                var localException = new clsException(ex, carpeta);
                return Content(ex.Message);
            }
        }

        public async Task<ActionResult> EnviarArchivosRegistroVentas(string token, string periodo, string mail)
        {
            Comercio comercio = null;
            try
            {
                if (String.IsNullOrWhiteSpace(mail))
                    throw new Exception("No se ha especificado un correo");

                string metadata = ObtenerDataPeticion();
                comercio = await GestorAutorizacion.AutorizarComercioDesdeToken(token, metadata, "excelVentas");

                string nombreExcel = "regventas_" + comercio.Ruc + "_" + periodo + ".xlsx";
                string nombreTxt = "LE" + comercio.Ruc + periodo + "00140100001111.txt";
                string rutaFisica = Server.MapPath("~/Content/files/" + comercio.CarpetaServidor + "/regventas/");
                if (!Directory.Exists(rutaFisica))
                    Directory.CreateDirectory(rutaFisica);

                await GestorFacturacion.CrearArchivoExcelVentas(comercio, periodo, rutaFisica + nombreExcel);
                await GestorFacturacion.CrearArchivoTxtVentas(comercio, periodo, rutaFisica + nombreTxt, false);
                string[] adjuntos = new string[] { rutaFisica + nombreExcel, rutaFisica + nombreTxt };

                string rutaMailer = Server.MapPath("~/Content/files/mailer-reportes.html");
                string contenido = System.IO.File.ReadAllText(rutaMailer);
                contenido = contenido.Replace("{NombreEmpresa}", comercio.RazonSocial);
                contenido = contenido.Replace("{periodo}", periodo);
                contenido = contenido.Replace("{anio}", DateTime.Now.Year.ToString());
                contenido = contenido.Replace("{elaborado}", comercio.ElaboradoPor);
                string asunto = "Se ha enviado el reporte de ventas " + periodo;

                Mailer mailer = new Mailer();
                mailer.EnviarCorreo(mail, asunto, comercio, adjuntos, contenido);

                return Content("Envío correcto");
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