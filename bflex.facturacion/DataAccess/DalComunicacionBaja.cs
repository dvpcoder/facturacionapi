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
    public class DalComunicacionBaja
    {
        //a partir de aqui replicar tambien los sp
        public static async Task<int> ObtenerNumeroComunicacionBaja(string SerieComunicacion, Comercio comercio)
        {
            DatabaseHelper helper = null;
            Int32 resultado = 0;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("Serie", SerieComunicacion);
                helper.AddParameter("IdComercio", comercio.IdComercio);
                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpObtenerNumeroComunicacionBaja2", System.Data.CommandType.StoredProcedure
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

        public static async Task<int> InsertarComunicacionBaja(ComunicacionBaja comunicacion)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                await helper.BeginTransaction();

                helper.AddParameter("@IdComercio", comunicacion.Comercio.IdComercio);
                helper.AddParameter("@Serie", comunicacion.Serie);
                helper.AddParameter("@Numero", comunicacion.Numero);
                helper.AddParameter("@FechaFacturas", comunicacion.FechaFacturas);
                helper.AddParameter("@IdUsuarioRegistro", comunicacion.IdUsuarioRegistro);
                helper.AddParameter("@NroTicket", comunicacion.NroTicket);

                id = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpInsertarComunicacionBaja", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));

                if (id > 0)
                {
                    int temp = 0;
                    foreach (ComprobanteVenta factura in comunicacion.ListaFacturas)
                    {
                        helper.AddParameter("@IdComunicacionBaja", id);
                        helper.AddParameter("@IdComprobanteVenta", factura.IdComprobanteVenta);
                        helper.AddParameter("@IdDocumentoLocal", factura.IdDocumentoExterno);
                        temp += Convert.ToInt32(await helper.ExecuteScalar(
                            "fact_dvpInsertarDetalleComunicacionBaja", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        ));
                    }

                    if (temp == comunicacion.ListaFacturas.Count)
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
                var localException = new clsException(ex, comunicacion.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        public static async Task<int> ActualizarEnvioComunicacionBaja(ComunicacionBaja comunicacion)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdComunicacionBaja", comunicacion.IdComunicacionBaja);
                helper.AddParameter("@CodigoStatus", comunicacion.CodigoStatus);
                helper.AddParameter("@IdUsuarioConfirmacionSunat", comunicacion.IdUsuarioConfirmacionSunat);
                helper.AddParameter("@EstadoSunat", comunicacion.EstadoSunat);
                helper.AddParameter("@MensajeSunat", comunicacion.MensajeSunat);
                helper.AddParameter("@CodigoErrorSunat", comunicacion.CodigoErrorSunat);
                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpActualizarEnvioComunicacionBaja", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comunicacion.Comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return resultado;
        }

        public static async Task<ComunicacionBaja> ObtenerComunicacionBajaPorDocumentoExterno(Comercio comercio, int idComprobante)
        {
            ComunicacionBaja obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdComprobanteVenta", idComprobante);
                //helper.AddParameter("@IdComercio", comercio.idComercio);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerCabeceraComunicacionPorIdComprobante", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new ComunicacionBaja();
                    obj.Comercio = comercio;
                    obj.IdComunicacionBaja = Validacion.DBToInt32(ref reader, "IdComunicacionBaja");
                    obj.Serie = Validacion.DBToString(ref reader, "Serie");
                    obj.Numero = Validacion.DBToInt32(ref reader, "Numero");
                    obj.NroTicket = Validacion.DBToString(ref reader, "NroTicket");
                    obj.FechaFacturas = Validacion.DBToDateTime(ref reader, "FechaFacturas");
                    obj.FechaFacturas2 = obj.FechaFacturas.ToString("dd/MM/yyyy");
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
                var localException = new clsException(ex, comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
                if (reader != null) reader.Close();
            }
            return obj;
        }

        //public static List<ComprobanteVenta> ObtenerDetalleResumenPorIdResumen(int idResumen)
        //{
        //    List<ComprobanteVenta> detalle = new List<ComprobanteVenta>();
        //    DatabaseHelper helper = null;
        //    SqlDataReader reader = null;

        //    try
        //    {
        //        helper = new DatabaseHelper(Conexion.obtenerConexion());
        //        helper.AddParameter("@IdResumenBoletas", idResumen);
        //        reader = (SqlDataReader)helper.ExecuteReader(
        //            "fact_dvpObtenerDetalleResumenPorIdResumen", System.Data.CommandType.StoredProcedure
        //        );

        //        while (reader.Read())
        //        {
        //            ComprobanteVenta obj = new ComprobanteVenta();
        //            obj.IdDocumentoExterno = Validacion.DBToInt32(ref reader, "IdDocumentoLocal");
        //            obj.CodigoEstadoResumen = Validacion.DBToString(ref reader, "CodigoEstadoResumen");
        //            detalle.Add(obj);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var localException = new clsException(ex.ToString(), "DalComprobanteVenta -> ObtenerDetalleComprobanteVentaPorId()");
        //    }
        //    finally
        //    {
        //        if (helper != null)
        //            helper.Dispose();
        //        reader.Close();
        //    }
        //    return detalle;
        //}
    }
}