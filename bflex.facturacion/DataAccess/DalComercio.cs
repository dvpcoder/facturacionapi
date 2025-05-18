using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace bflex.facturacion.DataAccess
{
    public class DalComercio
    {
        public static async Task<Comercio> ObtenerComercioPorId(int idComercio)
        {
            Comercio obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idComercio", idComercio);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComercioPorId", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new Comercio();
                    obj.IdComercio = idComercio;
                    obj.AccessToken = Validacion.DBToString(ref reader, "AccessToken");
                    obj.Ruc = Validacion.DBToString(ref reader, "Ruc");
                    obj.RazonSocial = Validacion.DBToString(ref reader, "RazonSocial").Trim();
                    obj.NombreComercial = Validacion.DBToString(ref reader, "NombreComercial").Trim();
                    obj.CalleFiscal = Validacion.DBToString(ref reader, "CalleFiscal").Trim();
                    obj.UrbanizacionFiscal = Validacion.DBToString(ref reader, "UrbanizacionFiscal").Trim();
                    obj.DepartamentoFiscal = Validacion.DBToString(ref reader, "DepartamentoFiscal").Trim();
                    obj.ProvinciaFiscal = Validacion.DBToString(ref reader, "ProvinciaFiscal").Trim();
                    obj.DistritoFiscal = Validacion.DBToString(ref reader, "DistritoFiscal").Trim();
                    obj.UbigeoFiscal = Validacion.DBToString(ref reader, "UbigeoFiscal").Trim();
                    obj.CarpetaServidor = Validacion.DBToString(ref reader, "CarpetaServidor");
                    obj.ArchivoCertificado = Validacion.DBToString(ref reader, "ArchivoCertificado");
                    obj.PasswordCertificado = Validacion.DBToString(ref reader, "PasswordCertificado");
                    obj.UsuarioSunat = Validacion.DBToString(ref reader, "UsuarioSunat");
                    obj.PasswordSunat = Validacion.DBToString(ref reader, "PasswordSunat");
                    obj.Normativa = Validacion.DBToString(ref reader, "Normativa");
                    obj.Efact = Validacion.DBToBoolean(ref reader, "Efact");
                    obj.Nubefact = Validacion.DBToBoolean(ref reader, "Nubefact");
                    obj.PorcentajePropina = Validacion.DBToDecimal(ref reader, "PorcentajePropina");
                    obj.PorcentajeIgv = Validacion.DBToDecimal(ref reader, "PorcentajeIgv");
                    obj.CuentaDetraccion = Validacion.DBToString(ref reader, "CuentaDetraccion");
                    obj.UsuarioOSE = Validacion.DBToString(ref reader, "UsuarioOSE");
                    obj.PasswordOSE = Validacion.DBToString(ref reader, "PasswordOSE");
                    obj.SitioWeb = Validacion.DBToString(ref reader, "SitioWeb");
                    obj.Email = Validacion.DBToString(ref reader, "Email");
                    obj.Celular = Validacion.DBToString(ref reader, "Celular");
                    obj.ElaboradoPor = Validacion.DBToString(ref reader, "ElaboradoPor");
                    obj.RutaLogo = "~/Content/files/" + obj.CarpetaServidor + "/logo.jpg";
                    //RepresentanteLegal, DescripcionNegocio, SubDominioEcommerce, LinkFacebook, LinkYoutube, LinkInstagram
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return obj;
        }

        public static async Task<Comercio> ObtenerComercioPorRuc(string ruc)
        {
            Comercio obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@ruc", ruc);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComercioPorRuc", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new Comercio();
                    obj.IdComercio = Validacion.DBToInt32(ref reader, "IdComercio");
                    obj.AccessToken = Validacion.DBToString(ref reader, "AccessToken");
                    obj.Ruc = ruc;
                    obj.RazonSocial = Validacion.DBToString(ref reader, "RazonSocial").Trim();
                    obj.NombreComercial = Validacion.DBToString(ref reader, "NombreComercial").Trim();
                    obj.CalleFiscal = Validacion.DBToString(ref reader, "CalleFiscal").Trim();
                    obj.UrbanizacionFiscal = Validacion.DBToString(ref reader, "UrbanizacionFiscal").Trim();
                    obj.DepartamentoFiscal = Validacion.DBToString(ref reader, "DepartamentoFiscal").Trim();
                    obj.ProvinciaFiscal = Validacion.DBToString(ref reader, "ProvinciaFiscal").Trim();
                    obj.DistritoFiscal = Validacion.DBToString(ref reader, "DistritoFiscal").Trim();
                    obj.UbigeoFiscal = Validacion.DBToString(ref reader, "UbigeoFiscal").Trim();
                    obj.CarpetaServidor = Validacion.DBToString(ref reader, "CarpetaServidor");
                    obj.ArchivoCertificado = Validacion.DBToString(ref reader, "ArchivoCertificado");
                    obj.PasswordCertificado = Validacion.DBToString(ref reader, "PasswordCertificado");
                    obj.UsuarioSunat = Validacion.DBToString(ref reader, "UsuarioSunat");
                    obj.PasswordSunat = Validacion.DBToString(ref reader, "PasswordSunat");
                    obj.Normativa = Validacion.DBToString(ref reader, "Normativa");
                    obj.Efact = Validacion.DBToBoolean(ref reader, "Efact");
                    obj.Nubefact = Validacion.DBToBoolean(ref reader, "Nubefact");
                    obj.PorcentajePropina = Validacion.DBToDecimal(ref reader, "PorcentajePropina");
                    obj.PorcentajeIgv = Validacion.DBToDecimal(ref reader, "PorcentajeIgv");
                    obj.CuentaDetraccion = Validacion.DBToString(ref reader, "CuentaDetraccion");
                    obj.UsuarioOSE = Validacion.DBToString(ref reader, "UsuarioOSE");
                    obj.PasswordOSE = Validacion.DBToString(ref reader, "PasswordOSE");
                    obj.SitioWeb = Validacion.DBToString(ref reader, "SitioWeb");
                    obj.Email = Validacion.DBToString(ref reader, "Email");
                    obj.Celular = Validacion.DBToString(ref reader, "Celular");
                    obj.ElaboradoPor = Validacion.DBToString(ref reader, "ElaboradoPor");
                    obj.RutaLogo = "~/Content/files/" + obj.CarpetaServidor + "/logo.jpg";
                    //RepresentanteLegal, DescripcionNegocio, SubDominioEcommerce, LinkFacebook, LinkYoutube, LinkInstagram
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return obj;
        }

        public static async Task<int> InsertarComercioInicial(Comercio comercio)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdCuenta", comercio.Cuenta.IdCuenta);
                helper.AddParameter("@Ruc", comercio.Ruc);
                helper.AddParameter("@RazonSocial", comercio.RazonSocial);
                helper.AddParameter("@NombreComercial", comercio.NombreComercial);
                helper.AddParameter("@Email", comercio.Email);
                helper.AddParameter("@Celular", comercio.Celular);
                helper.AddParameter("@RepresentanteLegal", comercio.RepresentanteLegal);
                helper.AddParameter("@IdentidadRepresentante", comercio.IdentidadRepresentante);
                helper.AddParameter("@DescripcionNegocio", comercio.DescripcionNegocio);
                helper.AddParameter("@ElaboradoPor", comercio.ElaboradoPor);
                helper.AddParameter("@AccessToken", comercio.AccessToken);
                id = Convert.ToInt32(await helper.ExecuteScalar("fact_dvpInsertarComercioInicial", System.Data.CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> InsertarComercioCompleto(Comercio comercio)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdCuenta", comercio.Cuenta.IdCuenta);
                helper.AddParameter("@Ruc", comercio.Ruc);
                helper.AddParameter("@RazonSocial", comercio.RazonSocial);
                helper.AddParameter("@NombreComercial", comercio.NombreComercial);
                helper.AddParameter("@Email", comercio.Email);
                helper.AddParameter("@Celular", comercio.Celular);
                helper.AddParameter("@RepresentanteLegal", comercio.RepresentanteLegal);
                helper.AddParameter("@IdentidadRepresentante", comercio.IdentidadRepresentante);
                helper.AddParameter("@ElaboradoPor", comercio.ElaboradoPor);
                helper.AddParameter("@AccessToken", comercio.AccessToken);
                helper.AddParameter("@CarpetaServidor", comercio.CarpetaServidor);
                helper.AddParameter("@ArchivoCertificado", comercio.ArchivoCertificado);
                helper.AddParameter("@PasswordCertificado", comercio.PasswordCertificado);
                helper.AddParameter("@UsuarioSunat", comercio.UsuarioSunat);
                helper.AddParameter("@PasswordSunat", comercio.PasswordSunat);
                helper.AddParameter("@DepartamentoFiscal", comercio.DepartamentoFiscal);
                helper.AddParameter("@ProvinciaFiscal", comercio.ProvinciaFiscal);
                helper.AddParameter("@DistritoFiscal", comercio.DistritoFiscal);
                helper.AddParameter("@UbigeoFiscal", comercio.UbigeoFiscal);

                if(!String.IsNullOrWhiteSpace(comercio.DescripcionNegocio))
                    helper.AddParameter("@DescripcionNegocio", comercio.DescripcionNegocio);
                if (!String.IsNullOrWhiteSpace(comercio.Normativa))
                    helper.AddParameter("@Normativa", comercio.Normativa);
                if (!String.IsNullOrWhiteSpace(comercio.CalleFiscal))
                    helper.AddParameter("@CalleFiscal", comercio.CalleFiscal);
                if (!String.IsNullOrWhiteSpace(comercio.UrbanizacionFiscal))
                    helper.AddParameter("@UrbanizacionFiscal", comercio.UrbanizacionFiscal);

                id = Convert.ToInt32(await helper.ExecuteScalar("fact_dvpInsertarComercioCompleto", System.Data.CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> GestionarComercio(Comercio comercio)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdCuenta", comercio.Cuenta.IdCuenta);
                helper.AddParameter("@Ruc", comercio.Ruc);
                helper.AddParameter("@RazonSocial", comercio.RazonSocial);
                helper.AddParameter("@NombreComercial", comercio.NombreComercial);
                helper.AddParameter("@Email", comercio.Email);
                helper.AddParameter("@Celular", comercio.Celular);
                helper.AddParameter("@RepresentanteLegal", comercio.RepresentanteLegal);
                helper.AddParameter("@IdentidadRepresentante", comercio.IdentidadRepresentante);
                helper.AddParameter("@ElaboradoPor", comercio.ElaboradoPor);
                helper.AddParameter("@AccessToken", comercio.AccessToken);
                helper.AddParameter("@CarpetaServidor", comercio.CarpetaServidor);
                helper.AddParameter("@ArchivoCertificado", comercio.ArchivoCertificado);
                helper.AddParameter("@PasswordCertificado", comercio.PasswordCertificado);
                helper.AddParameter("@UsuarioSunat", comercio.UsuarioSunat);
                helper.AddParameter("@PasswordSunat", comercio.PasswordSunat);
                helper.AddParameter("@DepartamentoFiscal", comercio.DepartamentoFiscal);
                helper.AddParameter("@ProvinciaFiscal", comercio.ProvinciaFiscal);
                helper.AddParameter("@DistritoFiscal", comercio.DistritoFiscal);
                helper.AddParameter("@UbigeoFiscal", comercio.UbigeoFiscal);

                if (!String.IsNullOrWhiteSpace(comercio.DescripcionNegocio))
                    helper.AddParameter("@DescripcionNegocio", comercio.DescripcionNegocio);
                if (!String.IsNullOrWhiteSpace(comercio.Normativa))
                    helper.AddParameter("@Normativa", comercio.Normativa);
                if (!String.IsNullOrWhiteSpace(comercio.CalleFiscal))
                    helper.AddParameter("@CalleFiscal", comercio.CalleFiscal);
                if (!String.IsNullOrWhiteSpace(comercio.UrbanizacionFiscal))
                    helper.AddParameter("@UrbanizacionFiscal", comercio.UrbanizacionFiscal);

                id = Convert.ToInt32(await helper.ExecuteScalar("fact_dvpGestionarComercio", System.Data.CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> AsignarTokenComercio(string ruc, string accessToken)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@Ruc", ruc);
                helper.AddParameter("@AccessToken", accessToken);
                id = Convert.ToInt32(await helper.ExecuteScalar("fact_dvpAsignarTokenComercio", System.Data.CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> AutorizarOperacionComercio (string ruc, string token, string tipo, string valor)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@ruc", ruc);
                helper.AddParameter("@token", token);
                helper.AddParameter("@tipo", tipo);
                if (!String.IsNullOrWhiteSpace(valor))
                {
                    helper.AddParameter("@valor", valor);
                }
                resultado = Convert.ToInt32(await helper.ExecuteScalar("fact_AutorizarOperacionComercio", System.Data.CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return resultado;
        }

        public static async Task<Comercio> AutenticarUsuarioLogeado(string email, string contrasenia, string ruc)
        {
            DatabaseHelper helper = null;
            SqlDataReader reader = null;
            Comercio comercio = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@email", email);
                helper.AddParameter("@contrasenia", contrasenia);
                helper.AddParameter("@ruc", ruc);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpAutenticarUsuarioLogeado", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    comercio = new Comercio();
                    comercio.IdComercio = Validacion.DBToInt32(ref reader, "IdComercio");
                    comercio.Ruc = ruc;
                    comercio.RazonSocial = Validacion.DBToString(ref reader, "razonSocial");
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return comercio;
        }

        public static async Task<List<string>> ObtenerPeriodosVenta(int idComercio)
        {
            DatabaseHelper helper = null;
            SqlDataReader reader = null;
            List<string> periodos = new List<string>();

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idComercio", idComercio);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerPeriodosVenta", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    string anio = Validacion.DBToString(ref reader, "anio");
                    string mes = Validacion.DBToInt32(ref reader, "mes").ToString("00");
                    periodos.Add(anio + mes);
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return periodos;
        }

        public static async Task<string> ActualizarComercioPorIdAnexo(Comercio comercio, int idAnexo)
        {
            DatabaseHelper helper = null;
            SqlDataReader reader = null;
            string codigoEstablecimiento = "";

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idAnexoComercio", idAnexo);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComercioAnexoPorId", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    codigoEstablecimiento = Validacion.DBToString(ref reader, "CodigoEstablecimiento");
                    comercio.NombreComercial = Validacion.DBToString(ref reader, "NombreComercial");
                    comercio.Email = Validacion.DBToString(ref reader, "Email");
                    comercio.Celular = Validacion.DBToString(ref reader, "Celular");
                    comercio.CalleFiscal = Validacion.DBToString(ref reader, "Calle");
                    comercio.UrbanizacionFiscal = Validacion.DBToString(ref reader, "Urbanizacion");
                    comercio.DepartamentoFiscal = Validacion.DBToString(ref reader, "Departamento");
                    comercio.ProvinciaFiscal = Validacion.DBToString(ref reader, "Provincia");
                    comercio.DistritoFiscal = Validacion.DBToString(ref reader, "Distrito");
                    comercio.UbigeoFiscal = Validacion.DBToString(ref reader, "Ubigeo");
                    comercio.RutaLogo = "~/Content/files/" + comercio.CarpetaServidor + "/logo-" + idAnexo + ".jpg";
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            
            return codigoEstablecimiento;
        }
    }
}