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
    public class DalResumenBoletas
    {
        public static async Task<int> ObtenerNumeroResumenBoletas(string SerieResumen, Comercio comercio)
        {
            DatabaseHelper helper = null;
            Int32 resultado = 0;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("Serie", SerieResumen);
                helper.AddParameter("IdComercio", comercio.IdComercio);
                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpObtenerNumeroResumenBoletas2", System.Data.CommandType.StoredProcedure
                ));
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
            return resultado;
        }

        public static async Task<ResumenBoletas> ObtenerResumenPorNumeracionComercio(string serie, int numero, Comercio comercio)
        {
            ResumenBoletas obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@serie", serie);
                helper.AddParameter("@numero", numero);
                helper.AddParameter("@idComercio", comercio.IdComercio);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerResumenPorNumeracionComercio", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new ResumenBoletas();
                    obj.Comercio = comercio;
                    obj.Serie = serie;
                    obj.Numero = numero;
                    obj.IdResumenBoletas = Validacion.DBToInt32(ref reader, "IdResumenBoletas");
                    obj.NroTicket = Validacion.DBToString(ref reader, "NroTicket");
                    obj.FechaRegistro = Validacion.DBToDateTime(ref reader, "FechaRegistro");
                    obj.FechaBoletas = Validacion.DBToDateTime(ref reader, "FechaBoletas");
                    obj.FechaConfirmacionSunat = Validacion.DBToDateTime(ref reader, "FechaConfirmacionSunat");
                    obj.IdUsuarioConfirmacionSunat = Validacion.DBToInt32(ref reader, "IdUsuarioConfirmacionSunat");
                    obj.CodigoStatus = Validacion.DBToString(ref reader, "CodigoStatus");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, obj.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return obj;
        }

        public static async Task<int> InsertarResumenBoletas(ResumenBoletas resumen)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                await helper.BeginTransaction();

                helper.AddParameter("@IdComercio", resumen.Comercio.IdComercio);
                helper.AddParameter("@Serie", resumen.Serie);
                helper.AddParameter("@Numero", resumen.Numero);
                helper.AddParameter("@FechaBoletas", resumen.FechaBoletas);
                helper.AddParameter("@IdUsuarioRegistro", resumen.IdUsuarioRegistro);
                helper.AddParameter("@NroTicket", resumen.NroTicket);

                id = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpInsertaResumenBoletas", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));

                if (id > 0)
                {
                    int temp = 0;
                    foreach (ComprobanteVenta boleta in resumen.ListaBoletas)
                    {
                        helper.AddParameter("@IdResumenBoletas", id);
                        helper.AddParameter("@IdComprobanteVenta", boleta.IdComprobanteVenta);
                        helper.AddParameter("@IdDocumentoLocal", boleta.IdDocumentoExterno);
                        helper.AddParameter("@CodigoEstadoResumen", boleta.CodigoEstadoResumen);

                        temp += Convert.ToInt32(await helper.ExecuteScalar(
                            "fact_dvpInsertarDetalleResumenBoletas", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        ));
                    }

                    if (temp == resumen.ListaBoletas.Count)
                    {
                        helper.CommitTransaction();
                    }
                    else
                    {
                        helper.RollbackTransaction();
                        id = -2;
                    }
                }
                else
                {
                    helper.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                helper.RollbackTransaction();
                var localException = new clsException(ex, resumen.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> ActualizarEnvioResumenBoletas(ResumenBoletas resumen)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdResumenBoletas", resumen.IdResumenBoletas);
                helper.AddParameter("@CodigoStatus", resumen.CodigoStatus);
                helper.AddParameter("@IdUsuarioConfirmacionSunat", resumen.IdUsuarioConfirmacionSunat);
                helper.AddParameter("@EstadoSunat", resumen.EstadoSunat);
                helper.AddParameter("@MensajeSunat", resumen.MensajeSunat);
                helper.AddParameter("@CodigoErrorSunat", resumen.CodigoErrorSunat);
                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpActualizarEnvioResumenBoletas", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, resumen.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return resultado;
        }

        public static async Task<ResumenBoletas> ObtenerResumenBoletasPorDocumentoExterno (Comercio comercio, int idComprobante, bool nuevo)
        {
            ResumenBoletas obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());

                if (nuevo)
                {
                    helper.AddParameter("@IdComprobanteVenta", idComprobante);
                    reader = (SqlDataReader)await helper.ExecuteReader(
                        "fact_dvpObtenerCabeceraResumenPorIdComprobante", System.Data.CommandType.StoredProcedure
                    );
                }
                else
                {
                    //esta cosa no funciona bien, lo ideal seria buscar por serie y numero
                    helper.AddParameter("@IdComercio", comercio.IdComercio);
                    helper.AddParameter("@IdDocumentoLocal", idComprobante);
                    reader = (SqlDataReader)await helper.ExecuteReader(
                        "fact_dvpObtenerCabeceraResumenPorDocExterno", System.Data.CommandType.StoredProcedure
                    );
                }

                while (await reader.ReadAsync())
                {
                    obj = new ResumenBoletas();
                    obj.Comercio = comercio;
                    obj.IdResumenBoletas = Validacion.DBToInt32(ref reader, "IdResumenBoletas");
                    obj.Serie = Validacion.DBToString(ref reader, "Serie");
                    obj.Numero = Validacion.DBToInt32(ref reader, "Numero");
                    obj.NroTicket = Validacion.DBToString(ref reader, "NroTicket");
                    obj.FechaBoletas = Validacion.DBToDateTime(ref reader, "FechaBoletas");
                    obj.FechaBoletas2 = obj.FechaBoletas.ToString("dd/MM/yyyy");
                    obj.FechaConfirmacionSunat = Validacion.DBToDateTime(ref reader, "FechaConfirmacionSunat");
                    obj.FechaRegistro = Validacion.DBToDateTime(ref reader, "FechaRegistro");
                    obj.IdUsuarioConfirmacionSunat = Validacion.DBToInt32(ref reader, "IdUsuarioConfirmacionSunat");
                    obj.IdUsuarioRegistro = Validacion.DBToInt32(ref reader, "IdUsuarioRegistro");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                    obj.CodigoStatus = Validacion.DBToString(ref reader, "CodigoStatus");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                }
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, obj.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return obj;
        }

        public static async Task<List<ComprobanteVenta>> ObtenerDetalleResumenPorIdResumen(int idResumen)
        {
            List<ComprobanteVenta> detalle = new List<ComprobanteVenta>();
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdResumenBoletas", idResumen);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerDetalleResumenPorIdResumen", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    ComprobanteVenta obj = new ComprobanteVenta();
                    obj.IdDocumentoExterno = Validacion.DBToInt32(ref reader, "IdDocumentoLocal");
                    obj.CodigoEstadoResumen = Validacion.DBToString(ref reader, "CodigoEstadoResumen");
                    obj.Serie = Validacion.DBToString(ref reader, "Serie");
                    obj.Numero = Validacion.DBToString(ref reader, "Numero");
                    detalle.Add(obj);
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
            return detalle;
        }
    }
}