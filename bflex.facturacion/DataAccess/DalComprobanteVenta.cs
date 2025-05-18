using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace bflex.facturacion.DataAccess
{
    public class DalComprobanteVenta
    {
        public static async Task<List<DetalleComprobanteVenta>> ObtenerDetalleComprobanteVentaPorId (int idComprobante)
        {
            List<DetalleComprobanteVenta> detalle = new List<DetalleComprobanteVenta>();
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idComprobanteVenta", idComprobante);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerDetalleComprobanteVentaPorId", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    DetalleComprobanteVenta obj = new DetalleComprobanteVenta();
                    obj.CodigoUnidad = Validacion.DBToString(ref reader, "CodigoUnidad");
                    obj.Cantidad = Validacion.DBToDecimal(ref reader, "Cantidad");
                    obj.NombreProducto = Validacion.DBToString(ref reader, "NombreProducto").Trim();
                    obj.CodigoProducto = Validacion.DBToString(ref reader, "CodigoProducto");
                    obj.PrecioUnitario = Validacion.DBToDecimal(ref reader, "PrecioUnitario");
                    obj.MontoIgv = Validacion.DBToDecimal(ref reader, "MontoIgv");
                    obj.PorcentajeIgv = Validacion.DBToDecimal(ref reader, "PorcentajeIgv");
                    obj.ValorReferencial = Validacion.DBToDecimal(ref reader, "ValorReferencial");
                    obj.ValorUnitario = Validacion.DBToDecimal(ref reader, "ValorUnitario");
                    obj.Total = Validacion.DBToDecimal(ref reader, "Total");
                    obj.MontoBruto = Validacion.DBToDecimal(ref reader, "MontoBruto");
                    obj.MontoIcbper = Validacion.DBToDecimal(ref reader, "MontoIcbper");
                    obj.ValorIcbperUnitario = Validacion.DBToDecimal(ref reader, "PrecioUnitarioBolsa");
                    obj.MontoDescuentoBase = Validacion.DBToDecimal(ref reader, "MontoDescuentoBase");
                    obj.PorcentajeDescuentoBase = Validacion.DBToDecimal(ref reader, "PorcentajeDescuento");
                    obj.CodigoTributo = Validacion.DBToString(ref reader, "CodigoTributo");
                    obj.NombreTributo = Validacion.DBToString(ref reader, "NombreTributo");
                    obj.CodigoTipoTributo = Validacion.DBToString(ref reader, "CodigoTipoTributo");
                    obj.CodigoAfectacion = Validacion.DBToString(ref reader, "CodigoAfectacion");
                    obj.CodigoPrecio = Validacion.DBToString(ref reader, "CodigoPrecio");
                    obj.PresentacionUnidad = Validacion.DBToString(ref reader, "PresentacionUnidad");
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

        public static async Task<List<CuotaCredito>> ObtenerDetalleCuotaPorIdComprobante (int idComprobante)
        {
            List<CuotaCredito> cuotas = new List<CuotaCredito>();
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idComprobanteVenta", idComprobante);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerDetalleCuotaPorIdComprobante", System.Data.CommandType.StoredProcedure
                );
                
                while (await reader.ReadAsync())
                {
                    CuotaCredito obj = new CuotaCredito();
                    obj.FechaPago = Validacion.DBToDateTime(ref reader, "FechaPago");
                    obj.Monto = Validacion.DBToDecimal(ref reader, "Monto");
                    cuotas.Add(obj);
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
            return cuotas;
        }

        public static async Task<ComprobanteVenta> ObtenerComprobantePorNumeracionComercio(
            string serie, string numero, Comercio comercio, string tipoDoc
        )
        {
            ComprobanteVenta obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@serie", serie);
                helper.AddParameter("@numero", numero);
                helper.AddParameter("@idComercio", comercio.IdComercio);
                if (!String.IsNullOrWhiteSpace(tipoDoc))
                    helper.AddParameter("@tipoDoc", tipoDoc);

                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComprobantePorNumeracionComercio", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new ComprobanteVenta();
                    //aqui ya obtuve el comercio y no necesitaba el accesstoken porque todo se hacia con id
                    obj.comercio = comercio;
                    obj.Serie = serie;
                    obj.Numero = numero;
                    obj.IdComprobanteVenta = Validacion.DBToInt32(ref reader, "IdComprobanteVenta");
                    obj.CodigoTipoComprobante = Validacion.DBToString(ref reader, "CodigoTipoComprobante");
                    obj.FechaCreacion = Validacion.DBToDateTime(ref reader, "FechaCreacion");
                    obj.FechaConfirmacionSunat = Validacion.DBToDateTime(ref reader, "FechaConfirmacionSunat");
                    obj.IdUsuarioCreacion = Validacion.DBToInt32(ref reader, "IdUsuarioCreacion");
                    obj.IdUsuarioConfirmacionSunat = Validacion.DBToInt32(ref reader, "IdUsuarioConfirmacionSunat");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                    obj.Valido = Validacion.DBToBoolean(ref reader, "Valido");
                    obj.MotivoAnulacion = Validacion.DBToString(ref reader, "MotivoAnulacion");
                    obj.CodigoMoneda = Validacion.DBToString(ref reader, "CodigoMoneda");
                    obj.DocumentoIdentidadCliente = Validacion.DBToString(ref reader, "DocumentoIdentidadCliente").Trim();
                    obj.CodigoIdentidadCliente = Validacion.DBToString(ref reader, "CodigoIdentidadCliente");
                    obj.NombreCompletoCliente = Validacion.DBToString(ref reader, "NombreCompletoCliente").Trim();
                    obj.CodigoOperacionVenta = Validacion.DBToString(ref reader, "CodigoOperacionVenta");
                    obj.MontoIgv = Validacion.DBToDecimal(ref reader, "MontoIgv");
                    obj.MontoGravado = Validacion.DBToDecimal(ref reader, "MontoGravado");
                    obj.MontoCargoAdicional = Validacion.DBToDecimal(ref reader, "MontoCargoAdicional");
                    obj.MontoIcbper = Validacion.DBToDecimal(ref reader, "MontoIcbper");
                    obj.MontoExonerado = Validacion.DBToDecimal(ref reader, "MontoExonerado");
                    obj.MontoInafecto = Validacion.DBToDecimal(ref reader, "MontoInafecto");
                    obj.MontoDescuentoBase = Validacion.DBToDecimal(ref reader, "MontoDescuento");
                    obj.MontoGratuito = Validacion.DBToDecimal(ref reader, "MontoGratuito");
                    obj.MontoPercepcion = Validacion.DBToDecimal(ref reader, "MontoPercepcion");
                    obj.MontoRetencion = Validacion.DBToDecimal(ref reader, "MontoRetencion");
                    obj.MontoDetraccion = Validacion.DBToDecimal(ref reader, "MontoDetraccion");
                    obj.SubTotal = Validacion.DBToDecimal(ref reader, "Subtotal");
                    obj.Total = Validacion.DBToDecimal(ref reader, "Total");
                    obj.FechaEmision = Validacion.DBToDateTime(ref reader, "FechaEmision");
                    obj.OrdenCompra = Validacion.DBToString(ref reader, "OrdenCompra");
                    obj.GuiaRemision = Validacion.DBToString(ref reader, "GuiaRemision");
                    obj.ComprobanteAfectado = Validacion.DBToString(ref reader, "ComprobanteAfectado");
                    obj.CodigoTipoNotaCreditoDebito = Validacion.DBToString(ref reader, "CodigoTipoNotaCreditoDebito");
                    obj.MotivoNotaCreditoDebito = Validacion.DBToString(ref reader, "MotivoNotaCreditoDebito").Trim();
                    obj.CodigoTipoComprobanteAfectado = Validacion.DBToString(ref reader, "CodigoTipoComprobanteAfectado");
                    obj.CodigoEstablecimiento = Validacion.DBToString(ref reader, "CodigoEstablecimiento");
                    obj.PorcentajeDescuentoBase = Validacion.DBToDecimal(ref reader, "PorcentajeDescuento");
                    obj.PorcentajeDetraccion = Validacion.DBToDecimal(ref reader, "PorcentajeDetraccion");
                    obj.PorcentajeCargoAdicional = Validacion.DBToDecimal(ref reader, "PorcentajeCargoAdicional");
                    obj.CodigoDetraccion = Validacion.DBToString(ref reader, "CodigoDetraccion");
                    obj.DireccionCliente = Validacion.DBToString(ref reader, "DireccionCliente").Trim();
                    obj.EmailCliente = Validacion.DBToString(ref reader, "EmailCliente");
                    obj.Observaciones = Validacion.DBToString(ref reader, "Observaciones").Trim();
                    obj.FormaPago = Validacion.DBToString(ref reader, "FormaPago");
                    //aqui no esta el detalleventa porque lo jalan en el gestor de documentos, no debe importar
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

        public static async Task<ComprobanteVenta> ObtenerComprobantePorNumeracionRuc(string serie, string numero, string ruc, string tipoDoc)
        {
            ComprobanteVenta obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@serie", serie);
                helper.AddParameter("@numero", numero);
                helper.AddParameter("@ruc", ruc);
                //podria hacer que si no hay nada me traiga bol/fac por defecto
                if (!String.IsNullOrWhiteSpace(tipoDoc))
                    helper.AddParameter("@tipoDoc", tipoDoc);

                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComprobantePorNumeracionRuc", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new ComprobanteVenta();
                    obj.comercio = new Comercio()
                    {
                        //esto solo se hace para la autorizacion de comprobante unico, se carga el token para comparar
                        //se carga el id para traer al comercio luego de tener el doc y se añade el ruc porque se puede
                        IdComercio = Validacion.DBToInt32(ref reader, "IdComercio"),
                        AccessToken = Validacion.DBToString(ref reader, "AccessToken"),
                        Ruc = ruc
                    };
                    obj.Serie = serie;
                    obj.Numero = numero;
                    obj.IdComprobanteVenta = Validacion.DBToInt32(ref reader, "IdComprobanteVenta");
                    obj.CodigoTipoComprobante = Validacion.DBToString(ref reader, "CodigoTipoComprobante");
                    obj.FechaCreacion = Validacion.DBToDateTime(ref reader, "FechaCreacion");
                    obj.FechaConfirmacionSunat = Validacion.DBToDateTime(ref reader, "FechaConfirmacionSunat");
                    obj.IdUsuarioCreacion = Validacion.DBToInt32(ref reader, "IdUsuarioCreacion");
                    obj.IdUsuarioConfirmacionSunat = Validacion.DBToInt32(ref reader, "IdUsuarioConfirmacionSunat");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                    obj.CodigoEstadoResumen = Validacion.DBToString(ref reader, "CodigoEstadoResumen");
                    obj.Valido = Validacion.DBToBoolean(ref reader, "Valido");
                    obj.MotivoAnulacion = Validacion.DBToString(ref reader, "MotivoAnulacion");
                    obj.CodigoMoneda = Validacion.DBToString(ref reader, "CodigoMoneda");
                    obj.DocumentoIdentidadCliente = Validacion.DBToString(ref reader, "DocumentoIdentidadCliente").Trim();
                    obj.CodigoIdentidadCliente = Validacion.DBToString(ref reader, "CodigoIdentidadCliente");
                    obj.NombreCompletoCliente = Validacion.DBToString(ref reader, "NombreCompletoCliente").Trim();
                    obj.CodigoOperacionVenta = Validacion.DBToString(ref reader, "CodigoOperacionVenta");
                    obj.MontoIgv = Validacion.DBToDecimal(ref reader, "MontoIgv");
                    obj.MontoGravado = Validacion.DBToDecimal(ref reader, "MontoGravado");
                    obj.MontoCargoAdicional = Validacion.DBToDecimal(ref reader, "MontoCargoAdicional");
                    obj.MontoIcbper = Validacion.DBToDecimal(ref reader, "MontoIcbper");
                    obj.MontoExonerado = Validacion.DBToDecimal(ref reader, "MontoExonerado");
                    obj.MontoInafecto = Validacion.DBToDecimal(ref reader, "MontoInafecto");
                    obj.MontoDescuentoBase = Validacion.DBToDecimal(ref reader, "MontoDescuento");
                    obj.MontoGratuito = Validacion.DBToDecimal(ref reader, "MontoGratuito");
                    obj.MontoPercepcion = Validacion.DBToDecimal(ref reader, "MontoPercepcion");
                    obj.MontoRetencion = Validacion.DBToDecimal(ref reader, "MontoRetencion");
                    obj.MontoDetraccion = Validacion.DBToDecimal(ref reader, "MontoDetraccion");
                    obj.SubTotal = Validacion.DBToDecimal(ref reader, "Subtotal");
                    obj.Total = Validacion.DBToDecimal(ref reader, "Total");
                    obj.FechaEmision = Validacion.DBToDateTime(ref reader, "FechaEmision");
                    obj.OrdenCompra = Validacion.DBToString(ref reader, "OrdenCompra");
                    obj.GuiaRemision = Validacion.DBToString(ref reader, "GuiaRemision");
                    obj.ComprobanteAfectado = Validacion.DBToString(ref reader, "ComprobanteAfectado");
                    obj.CodigoTipoNotaCreditoDebito = Validacion.DBToString(ref reader, "CodigoTipoNotaCreditoDebito");
                    obj.MotivoNotaCreditoDebito = Validacion.DBToString(ref reader, "MotivoNotaCreditoDebito").Trim();
                    obj.CodigoTipoComprobanteAfectado = Validacion.DBToString(ref reader, "CodigoTipoComprobanteAfectado");
                    obj.CodigoEstablecimiento = Validacion.DBToString(ref reader, "CodigoEstablecimiento");
                    obj.PorcentajeDescuentoBase = Validacion.DBToDecimal(ref reader, "PorcentajeDescuento");
                    obj.PorcentajeDetraccion = Validacion.DBToDecimal(ref reader, "PorcentajeDetraccion");
                    obj.PorcentajeCargoAdicional = Validacion.DBToDecimal(ref reader, "PorcentajeCargoAdicional");
                    obj.CodigoDetraccion = Validacion.DBToString(ref reader, "CodigoDetraccion");
                    obj.DireccionCliente = Validacion.DBToString(ref reader, "DireccionCliente").Trim();
                    obj.EmailCliente = Validacion.DBToString(ref reader, "EmailCliente");
                    obj.Observaciones = Validacion.DBToString(ref reader, "Observaciones");
                    obj.FormaPago = Validacion.DBToString(ref reader, "FormaPago");
                    obj.DetalleVenta = await ObtenerDetalleComprobanteVentaPorId(obj.IdComprobanteVenta);
                    obj.DetalleCuota = await ObtenerDetalleCuotaPorIdComprobante(obj.IdComprobanteVenta);
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

        public static async Task<ComprobanteRespuestaMin> ObtenerNumeracionAutomaticaNC(string serie, Comercio comercio)
        {
            ComprobanteRespuestaMin resultado = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("Serie", serie);
                helper.AddParameter("IdComercio", comercio.IdComercio);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "temp_ObtenerNumeracionAutomaticaNC", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    resultado = new ComprobanteRespuestaMin();
                    resultado.Serie = Validacion.DBToString(ref reader, "Serie");
                    resultado.Numero = Validacion.DBToString(ref reader, "Numero");
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
            }
            return resultado;
        }

        //se puede mandar el campo valido como falso solo para boletas y relacionados
        //al ser false no tendra detalle y no recorrera nada, solo insertará
        public static async Task<int> InsertarComprobanteVenta (ComprobanteVenta comprobante)
        {
            int id = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                await helper.BeginTransaction();

                helper.AddParameter("@IdComercio", comprobante.comercio.IdComercio);
                helper.AddParameter("@CodigoTipoComprobante", comprobante.CodigoTipoComprobante);
                helper.AddParameter("@CodigoMoneda", comprobante.CodigoMoneda);
                helper.AddParameter("@DocumentoIdentidadCliente", comprobante.DocumentoIdentidadCliente);
                helper.AddParameter("@CodigoIdentidadCliente", comprobante.CodigoIdentidadCliente);
                helper.AddParameter("@NombreCompletoCliente", comprobante.NombreCompletoCliente);
                helper.AddParameter("@CodigoOperacionVenta", comprobante.CodigoOperacionVenta);
                helper.AddParameter("@MontoIgv", comprobante.MontoIgv);
                helper.AddParameter("@MontoGravado", comprobante.MontoGravado);
                helper.AddParameter("@MontoCargoAdicional", comprobante.MontoCargoAdicional); //<-
                helper.AddParameter("@MontoIcbper", comprobante.MontoIcbper);
                helper.AddParameter("@MontoExonerado", comprobante.MontoExonerado);
                helper.AddParameter("@MontoInafecto", comprobante.MontoInafecto);
                helper.AddParameter("@MontoDescuento", comprobante.MontoDescuentoBase); //<-
                helper.AddParameter("@MontoGratuito", comprobante.MontoGratuito);
                helper.AddParameter("@MontoPercepcion", comprobante.MontoPercepcion);
                helper.AddParameter("@MontoRetencion", comprobante.MontoRetencion);
                helper.AddParameter("@MontoDetraccion", comprobante.MontoDetraccion);
                helper.AddParameter("@MontoExportacion", comprobante.MontoExportacion);
                helper.AddParameter("@SubTotal", comprobante.SubTotal);
                helper.AddParameter("@Total", comprobante.Total);
                helper.AddParameter("@Serie", comprobante.Serie.ToUpper());
                helper.AddParameter("@Numero", comprobante.Numero);
                helper.AddParameter("@FechaEmision", comprobante.FechaEmision);
                helper.AddParameter("@IdUsuarioCreacion", comprobante.IdUsuarioCreacion);
                helper.AddParameter("@OrdenCompra", comprobante.OrdenCompra); 
                helper.AddParameter("@GuiaRemision", comprobante.GuiaRemision); 
                helper.AddParameter("@ComprobanteAfectado", comprobante.ComprobanteAfectado);
                helper.AddParameter("@CodigoTipoNotaCreditoDebito", comprobante.CodigoTipoNotaCreditoDebito); 
                helper.AddParameter("@MotivoNotaCreditoDebito", comprobante.MotivoNotaCreditoDebito); 
                helper.AddParameter("@CodigoTipoComprobanteAfectado", comprobante.CodigoTipoComprobanteAfectado);
                //helper.AddParameter("@FechaComprobanteAfectado", comprobante.FechaComprobanteAfectado);
                helper.AddParameter("@CodigoEstablecimiento", comprobante.CodigoEstablecimiento);
                helper.AddParameter("@PorcentajeDescuento", comprobante.PorcentajeDescuentoBase);
                helper.AddParameter("@PorcentajeDetraccion", comprobante.PorcentajeDetraccion);
                helper.AddParameter("@PorcentajeCargoAdicional", comprobante.PorcentajeCargoAdicional);
                helper.AddParameter("@CodigoDetraccion", comprobante.CodigoDetraccion);
                helper.AddParameter("@Valido", comprobante.Valido);
                helper.AddParameter("@MotivoAnulacion", comprobante.MotivoAnulacion);
                helper.AddParameter("@DireccionCliente", comprobante.DireccionCliente);
                helper.AddParameter("@EmailCliente", comprobante.EmailCliente);
                helper.AddParameter("@FormaPago", comprobante.FormaPago);
                helper.AddParameter("@Observaciones", comprobante.Observaciones);
                helper.AddParameter("@IdCliente", comprobante.IdCliente);
                helper.AddParameter("@Puntaje", comprobante.Puntaje);

                id = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpInsertarComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));

                if (id > 0)
                {
                    int temp = 0;
                    foreach (DetalleComprobanteVenta detalle in comprobante.DetalleVenta)
                    {
                        helper.AddParameter("@IdComprobanteVenta", id);
                        helper.AddParameter("@CodigoUnidad", detalle.CodigoUnidad);
                        helper.AddParameter("@Cantidad", detalle.Cantidad);
                        helper.AddParameter("@NombreProducto", detalle.NombreProducto);
                        helper.AddParameter("@CodigoProducto", detalle.CodigoProducto);
                        helper.AddParameter("@PrecioUnitario", detalle.PrecioUnitario);
                        helper.AddParameter("@MontoIgv", detalle.MontoIgv);
                        helper.AddParameter("@PorcentajeIgv", detalle.PorcentajeIgv);
                        helper.AddParameter("@ValorReferencial", detalle.ValorReferencial);
                        helper.AddParameter("@ValorUnitario", detalle.ValorUnitario);
                        helper.AddParameter("@Total", detalle.Total);
                        helper.AddParameter("@MontoBruto", detalle.MontoBruto);
                        helper.AddParameter("@MontoIcbper", detalle.MontoIcbper);
                        helper.AddParameter("@PrecioUnitarioBolsa", detalle.ValorIcbperUnitario);
                        helper.AddParameter("@MontoDescuentoBase", detalle.MontoDescuentoBase);
                        helper.AddParameter("@PorcentajeDescuento", detalle.PorcentajeDescuentoBase);
                        helper.AddParameter("@CodigoTributo", detalle.CodigoTributo);
                        helper.AddParameter("@NombreTributo", detalle.NombreTributo);
                        helper.AddParameter("@CodigoTipoTributo", detalle.CodigoTipoTributo);
                        helper.AddParameter("@CodigoAfectacion", detalle.CodigoAfectacion);
                        helper.AddParameter("@CodigoPrecio", detalle.CodigoPrecio);
                        helper.AddParameter("@PresentacionUnidad", detalle.PresentacionUnidad);

                        temp += Convert.ToInt32(await helper.ExecuteScalar(
                            "fact_dvpInsertarDetalleComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        ));
                    }

                    //y no se hará comprobación si se ingresó o no la cuota
                    foreach(CuotaCredito cuota in comprobante.DetalleCuota)
                    {
                        helper.AddParameter("@IdComprobanteVenta", id);
                        helper.AddParameter("@Monto", cuota.Monto);
                        helper.AddParameter("@FechaPago", cuota.FechaPago);

                        await helper.ExecuteNonQuery(
                            "fact_dvpInsertarCuotaCredito", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        );
                    }

                    if (temp == comprobante.DetalleVenta.Count)
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
                //helper.RollbackTransaction();
                var localException = new clsException(ex, comprobante.comercio.CarpetaServidor);
                //throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return id;
        }

        //actualizar tambien sera un solo metodo con transaccion
        //nada mas que si tiene detalle borra los anteriores y si no tiene ya no se borran
        //si es boleta se puede actualizar a no valido desde aqui 
        //el completo desde comprobante simple permitirá cambiar a true si es que todavia no esta informada
        //desde resumen debe dejar el completo en el valor que estuvo
        public static async Task<int> ActualizarComprobanteVenta(ComprobanteVenta comprobante)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                await helper.BeginTransaction();

                helper.AddParameter("@IdComprobanteVenta", comprobante.IdComprobanteVenta);
                helper.AddParameter("@CodigoMoneda", comprobante.CodigoMoneda);
                helper.AddParameter("@DocumentoIdentidadCliente", comprobante.DocumentoIdentidadCliente);
                helper.AddParameter("@CodigoIdentidadCliente", comprobante.CodigoIdentidadCliente);
                helper.AddParameter("@NombreCompletoCliente", comprobante.NombreCompletoCliente);
                helper.AddParameter("@CodigoOperacionVenta", comprobante.CodigoOperacionVenta);
                helper.AddParameter("@MontoIgv", comprobante.MontoIgv);
                helper.AddParameter("@MontoGravado", comprobante.MontoGravado);
                helper.AddParameter("@MontoCargoAdicional", comprobante.MontoCargoAdicional);
                helper.AddParameter("@MontoIcbper", comprobante.MontoIcbper);
                helper.AddParameter("@MontoExonerado", comprobante.MontoExonerado);
                helper.AddParameter("@MontoInafecto", comprobante.MontoInafecto);
                helper.AddParameter("@MontoDescuento", comprobante.MontoDescuentoBase);
                helper.AddParameter("@MontoGratuito", comprobante.MontoGratuito);
                helper.AddParameter("@MontoPercepcion", comprobante.MontoPercepcion);
                helper.AddParameter("@MontoRetencion", comprobante.MontoRetencion);
                helper.AddParameter("@MontoDetraccion", comprobante.MontoDetraccion);
                helper.AddParameter("@MontoExportacion", comprobante.MontoExportacion);
                helper.AddParameter("@SubTotal", comprobante.SubTotal);
                helper.AddParameter("@Total", comprobante.Total);
                helper.AddParameter("@FechaEmision", comprobante.FechaEmision);
                helper.AddParameter("@OrdenCompra", comprobante.OrdenCompra); //
                helper.AddParameter("@GuiaRemision", comprobante.GuiaRemision); //
                helper.AddParameter("@ComprobanteAfectado", comprobante.ComprobanteAfectado);//
                helper.AddParameter("@CodigoTipoNotaCreditoDebito", comprobante.CodigoTipoNotaCreditoDebito); //
                helper.AddParameter("@MotivoNotaCreditoDebito", comprobante.MotivoNotaCreditoDebito); //
                helper.AddParameter("@CodigoTipoComprobanteAfectado", comprobante.CodigoTipoComprobanteAfectado); //
                //helper.AddParameter("@FechaComprobanteAfectado", comprobante.FechaComprobanteAfectado);
                helper.AddParameter("@CodigoEstablecimiento", comprobante.CodigoEstablecimiento);
                helper.AddParameter("@PorcentajeDescuento", comprobante.PorcentajeDescuentoBase);
                helper.AddParameter("@PorcentajeDetraccion", comprobante.PorcentajeDetraccion);
                helper.AddParameter("@PorcentajeCargoAdicional", comprobante.PorcentajeCargoAdicional);
                helper.AddParameter("@CodigoDetraccion", comprobante.CodigoDetraccion);
                helper.AddParameter("@Valido", comprobante.Valido);
                helper.AddParameter("@MotivoAnulacion", comprobante.MotivoAnulacion);
                helper.AddParameter("@DireccionCliente", comprobante.DireccionCliente);
                helper.AddParameter("@EmailCliente", comprobante.EmailCliente);
                helper.AddParameter("@FormaPago", comprobante.FormaPago);
                helper.AddParameter("@Observaciones", comprobante.Observaciones);

                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpActualizarComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));

                if (resultado > 0)
                {
                    int temp = 0;

                    if (comprobante.DetalleVenta.Count > 0)
                    {
                        helper.AddParameter("@IdComprobanteVenta", comprobante.IdComprobanteVenta);
                        await helper.ExecuteNonQuery(
                            "fact_dvpLimpiarDetalleComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        );

                        foreach (DetalleComprobanteVenta detalle in comprobante.DetalleVenta)
                        {
                            helper.AddParameter("@IdComprobanteVenta", comprobante.IdComprobanteVenta);
                            helper.AddParameter("@CodigoUnidad", detalle.CodigoUnidad);
                            helper.AddParameter("@Cantidad", detalle.Cantidad);
                            helper.AddParameter("@NombreProducto", detalle.NombreProducto);
                            helper.AddParameter("@CodigoProducto", detalle.CodigoProducto);
                            helper.AddParameter("@PrecioUnitario", detalle.PrecioUnitario);
                            helper.AddParameter("@MontoIgv", detalle.MontoIgv);
                            helper.AddParameter("@PorcentajeIgv", detalle.PorcentajeIgv);
                            helper.AddParameter("@ValorReferencial", detalle.ValorReferencial);
                            helper.AddParameter("@ValorUnitario", detalle.ValorUnitario);
                            helper.AddParameter("@Total", detalle.Total);
                            helper.AddParameter("@MontoBruto", detalle.MontoBruto);
                            helper.AddParameter("@MontoIcbper", detalle.MontoIcbper);
                            helper.AddParameter("@PrecioUnitarioBolsa", detalle.ValorIcbperUnitario);
                            helper.AddParameter("@MontoDescuentoBase", detalle.MontoDescuentoBase);
                            helper.AddParameter("@PorcentajeDescuento", detalle.PorcentajeDescuentoBase);
                            helper.AddParameter("@CodigoTributo", detalle.CodigoTributo);
                            helper.AddParameter("@NombreTributo", detalle.NombreTributo);
                            helper.AddParameter("@CodigoTipoTributo", detalle.CodigoTipoTributo);
                            helper.AddParameter("@CodigoAfectacion", detalle.CodigoAfectacion);
                            helper.AddParameter("@CodigoPrecio", detalle.CodigoPrecio);
                            helper.AddParameter("@PresentacionUnidad", detalle.PresentacionUnidad);

                            temp += Convert.ToInt32(await helper.ExecuteScalar(
                                "fact_dvpInsertarDetalleComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                            ));
                        }
                    }

                    foreach (CuotaCredito cuota in comprobante.DetalleCuota)
                    {
                        helper.AddParameter("@IdComprobanteVenta", comprobante.IdComprobanteVenta);
                        helper.AddParameter("@Monto", cuota.Monto);
                        helper.AddParameter("@FechaPago", cuota.FechaPago);

                        await helper.ExecuteNonQuery(
                            "fact_dvpInsertarCuotaCredito", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                        );
                    }

                    if (temp == comprobante.DetalleVenta.Count)
                    {
                        helper.CommitTransaction();
                    }
                    else
                    {
                        helper.RollbackTransaction();
                        resultado = -2;
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
                var localException = new clsException(ex, comprobante.comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return resultado;
        }
        
        //otro metodo sera para actualizar campos de estado que es despues del envio de sunat o despues de resumenes
        public static async Task<int> ActualizarEnvioComprobanteVenta(ComprobanteVenta comprobante)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdComprobanteVenta", comprobante.IdComprobanteVenta);
                helper.AddParameter("@IdUsuarioConfirmacionSunat", comprobante.IdUsuarioConfirmacionSunat);
                helper.AddParameter("@EstadoSunat", comprobante.EstadoSunat);
                helper.AddParameter("@MensajeSunat", comprobante.MensajeSunat);
                helper.AddParameter("@CodigoErrorSunat", comprobante.CodigoErrorSunat);
                resultado = Convert.ToInt32(
                    await helper.ExecuteScalar("fact_dvpActualizarEnvioComprobanteVenta", System.Data.CommandType.StoredProcedure)
                );
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, comprobante.comercio.CarpetaServidor);
                throw ex;
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
            return resultado;
        }

        public static async Task<List<ComprobanteVenta>> ObtenerVentasPorComercioPeriodo(Comercio comercio, string periodo)
        {
            List<ComprobanteVenta> lista = new List<ComprobanteVenta>();
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@idComercio", comercio.IdComercio);
                helper.AddParameter("@periodo", periodo);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerVentasPorComercioPeriodo", System.Data.CommandType.StoredProcedure
                );

                while (reader.Read())
                {
                    //despues de terminar la consulta y tener el reporte para ver el estado de estas propiedades
                    CultureInfo.CurrentCulture = new CultureInfo("en-US");
                    //esto quizas deberia ser al escribir los datos en el txt
                    ComprobanteVenta obj = new ComprobanteVenta();
                    obj.IdComprobanteVenta = Validacion.DBToInt32(ref reader, "IdComprobanteVenta");
                    obj.FechaEmision = Validacion.DBToDateTime(ref reader, "FechaEmision");
                    obj.CodigoTipoComprobante = Validacion.DBToString(ref reader, "CodigoTipoComprobante");
                    obj.Serie = Validacion.DBToString(ref reader, "Serie");
                    obj.Numero = Validacion.DBToString(ref reader, "Numero");
                    obj.CodigoIdentidadCliente = Validacion.DBToString(ref reader, "CodigoIdentidadCliente");
                    obj.DocumentoIdentidadCliente = Validacion.DBToString(ref reader, "DocumentoIdentidadCliente");
                    obj.NombreCompletoCliente = Validacion.DBToString(ref reader, "NombreCompletoCliente");
                    obj.MontoExportacion = Validacion.DBToDecimal(ref reader, "MontoExportacion");
                    obj.MontoGravado = Validacion.DBToDecimal(ref reader, "MontoGravado");
                    obj.MontoDescuentoBase = Validacion.DBToDecimal(ref reader, "MontoDescuentoBase");
                    obj.MontoIgv = Validacion.DBToDecimal(ref reader, "MontoIgv");
                    obj.MontoExonerado = Validacion.DBToDecimal(ref reader, "MontoExonerado");
                    obj.MontoInafecto = Validacion.DBToDecimal(ref reader, "MontoInafecto");
                    obj.MontoIcbper = Validacion.DBToDecimal(ref reader, "MontoIcbper");
                    obj.MontoCargoAdicional = Validacion.DBToDecimal(ref reader, "MontoCargoAdicional");
                    obj.Total = Validacion.DBToDecimal(ref reader, "Total");
                    obj.CodigoMoneda = Validacion.DBToString(ref reader, "CodigoMoneda");
                    obj.FechaComprobanteAfectado = Validacion.DBToDateTime(ref reader, "FechaComprobanteAfectado");
                    obj.CodigoTipoComprobanteAfectado = Validacion.DBToString(ref reader, "CodigoTipoComprobanteAfectado");
                    obj.ComprobanteAfectado = Validacion.DBToString(ref reader, "ComprobanteAfectado");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.FormaPago = Validacion.DBToString(ref reader, "FormaPago");

                    if (obj.FormaPago.ToUpper() == "CREDITO")
                        obj.DetalleCuota = await ObtenerDetalleCuotaPorIdComprobante(obj.IdComprobanteVenta);

                    obj.comercio = new Comercio()
                    {
                        IdComercio = comercio.IdComercio,
                        CarpetaServidor = comercio.CarpetaServidor
                    };
                    lista.Add(obj);
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
            return lista;
        }

        public static async Task<ComprobanteVenta> ObtenerComprobantePorBusquedaEspecializada(ComprobanteVenta consulta)
        {
            ComprobanteVenta obj = null;
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@RUCEMISOR", consulta.comercio.Ruc);
                helper.AddParameter("@CODIGOTIPOCOMPROBANTE", consulta.CodigoTipoComprobante);
                helper.AddParameter("@SERIECOMPROBANTE", consulta.Serie);
                helper.AddParameter("@NUMEROCOMPROBANTE", consulta.Numero);
                helper.AddParameter("@CODIGOIDENTIDADCLIENTE", consulta.CodigoIdentidadCliente);
                if (!String.IsNullOrWhiteSpace(consulta.DocumentoIdentidadCliente))
                    helper.AddParameter("@DOCUMENTOIDENTIDADCLIENTE", consulta.DocumentoIdentidadCliente);
                helper.AddParameter("@FECHAEMISION", consulta.FechaEmision);
                helper.AddParameter("@TOTAL", consulta.Total);
                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerComprobantePorBusquedaEspecializada", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    obj = new ComprobanteVenta();
                    obj.IdComprobanteVenta = Validacion.DBToInt32(ref reader, "IdComprobanteVenta");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.comercio = new Comercio()
                    {
                        Ruc = consulta.comercio.Ruc
                        //CarpetaServidor = Validacion.DBToString(ref reader, "CarpetaServidor")
                    };
                    obj.CodigoTipoComprobante = consulta.CodigoTipoComprobante;
                    obj.Serie = consulta.Serie;
                    obj.Numero = consulta.Numero;
                    obj.CodigoIdentidadCliente = consulta.CodigoIdentidadCliente;
                    obj.FechaEmision = consulta.FechaEmision;
                    //obj.ActualizarTipoComprobante();
                }
                return obj;
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
        }

        public static async Task<List<ComprobanteRespuesta>> ObtenerListaComprobantesPorComercioConFiltros(
            string docIdentidadCliente, string estadoSunat, string tipoComprobante, string serie, string numero,
            int tipoFechas, DateTime fechaInicio, DateTime fechaFin, string rucEmisor
        )
        {
            List<ComprobanteRespuesta> lista = new List<ComprobanteRespuesta>();
            DatabaseHelper helper = null;
            SqlDataReader reader = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                
                if (!String.IsNullOrWhiteSpace(tipoComprobante))
                    helper.AddParameter("@TIPOCOMPROBANTE", tipoComprobante);
                if (!String.IsNullOrWhiteSpace(serie))
                    helper.AddParameter("@SERIE", serie);
                if (!String.IsNullOrWhiteSpace(numero))
                    helper.AddParameter("@NUMERO", numero);
                if (!String.IsNullOrWhiteSpace(docIdentidadCliente))
                    helper.AddParameter("@DOCIDENTIDADCLIENTE", docIdentidadCliente);
                if (!String.IsNullOrWhiteSpace(estadoSunat))
                    helper.AddParameter("@ESTADOSUNAT", estadoSunat);

                helper.AddParameter("@RUCEMISOR", rucEmisor);
                helper.AddParameter("@TIPOFECHAS", tipoFechas);
                helper.AddParameter("@FECHAINICIO", fechaInicio);
                helper.AddParameter("@FECHAFIN", fechaFin);

                reader = (SqlDataReader)await helper.ExecuteReader(
                    "fact_dvpObtenerListaComprobantesComercioFiltros", System.Data.CommandType.StoredProcedure
                );

                while (await reader.ReadAsync())
                {
                    ComprobanteRespuesta obj = new ComprobanteRespuesta();
                    obj.CodigoTipoComprobante = Validacion.DBToString(ref reader, "CodigoTipoComprobante");
                    obj.FechaEmision = Validacion.DBToDateTime(ref reader, "FechaEmision").ToString();
                    obj.FechaEnvio = Validacion.DBToDateTime(ref reader, "FechaCreacion").ToString();
                    obj.FechaConfirmacion = Validacion.DBToDateTime(ref reader, "FechaConfirmacionSunat").ToString();
                    obj.Serie = Validacion.DBToString(ref reader, "Serie");
                    obj.Numero = Validacion.DBToString(ref reader, "Numero");
                    obj.CodigoIdentidadCliente = Validacion.DBToString(ref reader, "CodigoIdentidadCliente");
                    obj.DocumentoIdentidadCliente = Validacion.DBToString(ref reader, "DocumentoIdentidadCliente");
                    obj.NombreCompletoCliente = Validacion.DBToString(ref reader, "NombreCompletoCliente");
                    obj.MontoIgv = Validacion.DBToDecimal(ref reader, "MontoIgv");
                    obj.Total = Validacion.DBToDecimal(ref reader, "Total");
                    obj.Valido = Validacion.DBToBoolean(ref reader, "Valido");
                    obj.EstadoSunat = Validacion.DBToString(ref reader, "EstadoSunat");
                    obj.CodigoMoneda = Validacion.DBToString(ref reader, "CodigoMoneda");
                    obj.MensajeSunat = Validacion.DBToString(ref reader, "MensajeSunat");
                    obj.CodigoErrorSunat = Validacion.DBToString(ref reader, "CodigoErrorSunat");
                    //obj.ActualizarTipoComprobante();
                    lista.Add(obj);
                }
                return lista;
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
        }

        public static async Task InsertarDocumentoVenta(DocumentoVenta documento, int idComercio)
        {
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdDocumentoLocal", documento.IdDocumentoVenta);
                helper.AddParameter("@IdComercio", idComercio);
                helper.AddParameter("@NroSerieDoc", documento.NroSerie);
                helper.AddParameter("@NroDoc", documento.NroDoc);
                helper.AddParameter("@Total", Convert.ToDecimal(documento.Total));
                helper.AddParameter("@Igv", Convert.ToDecimal(documento.Igv));
                helper.AddParameter("@Propina", Convert.ToDecimal(documento.ComisionPropina));
                helper.AddParameter("@Icbper", Convert.ToDecimal(documento.Icbper));
                helper.AddParameter("@Exonerado", Convert.ToDecimal(documento.MontoExonerado));
                helper.AddParameter("@Gratuito", Convert.ToDecimal(documento.MontoGratuito));
                helper.AddParameter("@Descuento", Convert.ToDecimal(documento.DescuentoReal));
                helper.AddParameter("@IdEstado", documento.Estado.id);

                int id = Convert.ToInt32(await helper.ExecuteScalar("fact_dvpInsertarDocumentoVenta", System.Data.CommandType.StoredProcedure));

                if (id > 0 && documento.DetalleDocumento != null)
                {
                    foreach (DetalleDocumentoVenta detalle in documento.DetalleDocumento)
                    {
                        helper.AddParameter("@IdDocumentoVenta", id);
                        helper.AddParameter("@Producto", detalle.Presentacion.nombre);
                        helper.AddParameter("@Cantidad", Convert.ToDecimal(detalle.Cantidad));
                        helper.AddParameter("@Precio", Convert.ToDecimal(detalle.Precio));
                        helper.AddParameter("@ValorVenta", Convert.ToDecimal(detalle.ValorVenta));
                        helper.AddParameter("@CostoVenta", Convert.ToDecimal(detalle.CostoVenta));
                        helper.AddParameter("@Icbper", Convert.ToDecimal(detalle.Icbper));
                        helper.AddParameter("@Descuento", Convert.ToDecimal(detalle.DescuentoReal));
                        await helper.ExecuteNonQuery("fact_dvpInsertarDetalleDocumentoVenta", System.Data.CommandType.StoredProcedure);
                    }
                }
                
            }
            catch (Exception ex)
            {
                var localException = new clsException(ex, null);
            }
            finally
            {
                if (helper != null) helper.Dispose();
            }
        }

        public static async Task<int> AnularComprobanteVenta (int id, string motivo)
        {
            int resultado = 0;
            DatabaseHelper helper = null;

            try
            {
                helper = new DatabaseHelper(Conexion.obtenerConexion());
                helper.AddParameter("@IdComprobanteVenta", id);
                if (!String.IsNullOrWhiteSpace(motivo))
                    helper.AddParameter("@MotivoAnulacion", motivo);

                resultado = Convert.ToInt32(await helper.ExecuteScalar(
                    "fact_dvpAnularComprobanteVenta", System.Data.CommandType.StoredProcedure, ConnectionState.KeepOpen
                ));
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
    }
}