using bflex.facturacion.DataAccess;
using bflex.facturacion.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading.Tasks;

namespace bflex.facturacion.SunatCore
{
    public class GestorFacturacion
    {
        #region facturacionv2

        public static void CrearArchivoQR(ComprobanteVenta comprobante, string rutaFisica)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(comprobante.CodificacionQr, QRCodeGenerator.ECCLevel.Q))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (Bitmap bitmap = new Bitmap(qrCode.GetGraphic(20), new Size(150, 150)))
                        {
                            bitmap.Save(rutaFisica + comprobante.ArchivoQr, ImageFormat.Png);
                        }
                    }
                }
            }
        }

        public static async Task CrearArchivoExcelVentas(Comercio comercio, string periodo, string rutaArchivo)
        {
            List<ComprobanteVenta> listaVenta = await DalComprobanteVenta.ObtenerVentasPorComercioPeriodo(comercio, periodo);
            if (listaVenta.Count == 0)
                throw new Exception("El periodo solicitado no trajo ningún resultado.");

            List<string> dataVentas = GetReporteVentasDataStandard(listaVenta, periodo);

            string[] encabezados = {
                "REGISTRO DE VENTAS", comercio.RazonSocial, "RUC: " + comercio.Ruc, "PERIODO: " + periodo
            };
            int[] colDecimal = { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

            DataTable dt = ObtenerModeloDTRegistroVentas();
            foreach (string reg in dataVentas)
            {
                string registro = reg.Remove(reg.Length - 1);
                DataRow fila = dt.NewRow();
                fila.ItemArray = registro.Split('|');
                dt.Rows.Add(fila);
            }

            CrearReporteExcel(encabezados, dt, colDecimal, rutaArchivo);
        }

        public static async Task CrearArchivoTxtVentas(Comercio comercio, string periodo, string rutaArchivo, bool isNew)
        {
            List<ComprobanteVenta> listaVenta = await DalComprobanteVenta.ObtenerVentasPorComercioPeriodo(comercio, periodo);
            if (listaVenta.Count == 0)
                throw new Exception("El periodo solicitado no trajo ningún resultado.");

            var dataVentas = new List<string>();

            if (isNew)
                dataVentas = GetReporteVentasDataRVIE(comercio, listaVenta, periodo);
            else
            {
                dataVentas = GetReporteVentasDataStandard(listaVenta, periodo);
                dataVentas.RemoveAt(dataVentas.Count - 1);
            }

            using (StreamWriter writer = new StreamWriter(rutaArchivo))
            {
                foreach (string registro in dataVentas)
                {
                    writer.WriteLine(registro);
                }
            }
        }

        public static async Task<ComprobanteVenta> GestionComprobante(ComprobanteVenta comprobanteExterno, Comercio comercio)
        {
            ComprobanteVenta comprobanteRegistrado = await DalComprobanteVenta.ObtenerComprobantePorNumeracionRuc(
                comprobanteExterno.Serie, comprobanteExterno.Numero, comercio.Ruc, comprobanteExterno.CodigoTipoComprobante
            );

            if (comprobanteRegistrado == null)
            {
                comprobanteExterno.EstadoSunat = "Sin envío";

                comprobanteExterno.comercio = comercio;
                comprobanteExterno.ActualizarCodigosEstructura();
                comprobanteExterno.ActualizarAtributosEstructura();

                int resultado = await DalComprobanteVenta.InsertarComprobanteVenta(comprobanteExterno);
                if (resultado <= 0)
                    throw new Exception("Error al guardar en la base de datos.");

                comprobanteExterno.IdComprobanteVenta = resultado;
                return comprobanteExterno;
            }
            else if (comprobanteRegistrado.EstadoSunat == "Con error" || comprobanteRegistrado.EstadoSunat == "Sin envío")
            {
                comprobanteExterno.IdComprobanteVenta = comprobanteRegistrado.IdComprobanteVenta;
                comprobanteExterno.FechaCreacion = comprobanteRegistrado.FechaCreacion;
                comprobanteExterno.IdUsuarioCreacion = comprobanteRegistrado.IdUsuarioCreacion;
                comprobanteExterno.EstadoSunat = comprobanteRegistrado.EstadoSunat;
                comprobanteExterno.MensajeSunat = comprobanteRegistrado.MensajeSunat;
                comprobanteExterno.CodigoErrorSunat = comprobanteRegistrado.CodigoErrorSunat;

                comprobanteExterno.comercio = comercio;
                comprobanteExterno.ActualizarCodigosEstructura();
                comprobanteExterno.ActualizarAtributosEstructura();

                int resultado = await DalComprobanteVenta.ActualizarComprobanteVenta(comprobanteExterno);
                if (resultado <= 0)
                    throw new Exception("Error al guardar en la base de datos.");

                return comprobanteExterno;
            }
            else 
            {
                if (comprobanteRegistrado.EstadoSunat == "Aceptado" && comprobanteExterno.Anulado)
                {
                    comprobanteRegistrado.Valido = false;
                    comprobanteRegistrado.MotivoAnulacion = comprobanteExterno.MotivoAnulacion;

                    int resultado = await DalComprobanteVenta.AnularComprobanteVenta(
                        comprobanteRegistrado.IdComprobanteVenta, comprobanteRegistrado.MotivoAnulacion
                    );
                    if (resultado <= 0)
                        throw new Exception("Error al guardar en la base de datos.");
                }
                
                comprobanteRegistrado.comercio = comercio;
                comprobanteRegistrado.IdDocumentoExterno = comprobanteExterno.IdDocumentoExterno;
                comprobanteRegistrado.ActualizarAtributosEstructura();

                return comprobanteRegistrado;
            }
        }

        public static async Task ActualizarComercioAnexo(ComprobanteVenta comprobante)
        {
            if (Convert.ToInt32(comprobante.CodigoEstablecimiento) > 0)
            {
                int idAnexo = Convert.ToInt32(comprobante.CodigoEstablecimiento);
                comprobante.CodigoEstablecimiento = await DalComercio.ActualizarComercioPorIdAnexo(comprobante.comercio, idAnexo);

                if (String.IsNullOrWhiteSpace(comprobante.CodigoEstablecimiento))
                    comprobante.CodigoEstablecimiento = "0000";
            }
        }

        public static async Task<ComprobanteVenta> ObtenerComprobanteParaAnulacion(string serie, string numero, string tipoDoc, string motivo, Comercio comercio)
        {
            ComprobanteVenta porAnular = await DalComprobanteVenta.ObtenerComprobantePorNumeracionRuc(serie, numero, comercio.Ruc, tipoDoc);
            if (porAnular == null)
                throw new Exception("--Debe informar primero el comprobante para poder anularlo.");

            porAnular.comercio = comercio;
            porAnular.Valido = false;
            porAnular.MotivoAnulacion = motivo;
            porAnular.ActualizarAtributosEstructura();

            int resultado = await DalComprobanteVenta.AnularComprobanteVenta(porAnular.IdComprobanteVenta, motivo);
            if (resultado <= 0)
                throw new Exception("Error al guardar en la base de datos.");

            return porAnular;
        }

        public static async Task<ComprobanteVenta> ObtenerNotaCreditoAnulacion(string serie, string numero, Comercio comercio)
        {
            ComprobanteVenta afectado = await DalComprobanteVenta.ObtenerComprobantePorNumeracionRuc(serie, numero, comercio.Ruc, "");
            if (afectado == null)
                throw new Exception("--te has equivocado");

            ComprobanteRespuestaMin min = await DalComprobanteVenta.ObtenerNumeracionAutomaticaNC(serie, comercio);

            ComprobanteVenta notaCredito = new ComprobanteVenta()
            {
                Anulado = false,
                ComprobanteAfectado = serie + "-" + numero,
                comercio = comercio,
                CodigoEstablecimiento = "0000",
                CodigoIdentidadCliente = afectado.CodigoIdentidadCliente,
                CodigoMoneda = "PEN",
                CodigoTipoComprobante = "07",
                CodigoTipoComprobanteAfectado = afectado.CodigoTipoComprobante,
                CodigoTipoNotaCreditoDebito = "01",
                DetalleVenta = afectado.DetalleVenta,
                DireccionCliente = afectado.DireccionCliente,
                DocumentoIdentidadCliente = afectado.DocumentoIdentidadCliente,
                EstadoSunat = "Sin envío",
                FechaEmision = DateTime.Now.AddDays(-3),
                FechaComprobanteAfectado = afectado.FechaEmision,
                FormaPago = "Contado",
                MontoGravado = afectado.MontoGravado,
                MontoIgv = afectado.MontoIgv,
                MotivoNotaCreditoDebito = "Error de destino",
                NombreCompletoCliente = afectado.NombreCompletoCliente,
                Numero = min.Numero,
                Serie = min.Serie,
                SubTotal = afectado.SubTotal,
                Total = afectado.Total
            };

            notaCredito.ActualizarCodigosEstructura();
            notaCredito.ActualizarAtributosEstructura();

            int resultado = await DalComprobanteVenta.InsertarComprobanteVenta(notaCredito);
            if (resultado <= 0)
                throw new Exception("--Error al guardar en la base de datos.");
            else
                notaCredito.IdComprobanteVenta = resultado;

            return notaCredito;
        }

        #endregion

        public static async Task<ComprobanteVenta> GestionarComprobanteSunat(Comercio comercio, DocumentoVenta documento, RespuestaServicio respuesta)
        {
            ComprobanteVenta comprobanteRegistrado = await DalComprobanteVenta.ObtenerComprobantePorNumeracionComercio(
                documento.NroSerie, documento.NroDoc, comercio, documento.Documento.CodigoSunat
            );
            //DalComprobanteVenta.InsertarDocumentoVenta(documento, comercio.idComercio);

            ComprobanteVenta comprobanteExterno = new ComprobanteVenta(documento);

            if (comprobanteRegistrado == null)
            {
                comprobanteExterno.comercio = new Comercio()
                {
                    IdComercio = comercio.IdComercio,
                    CarpetaServidor = comercio.CarpetaServidor
                };

                if (comprobanteExterno.CodigoTipoComprobante.Equals("66"))
                {
                    comprobanteExterno.EstadoSunat = "Aceptado";
                    comprobanteExterno.MensajeSunat = "Datos de puntaje guardados correctamente.";
                    comprobanteExterno.CodigoErrorSunat = "v1";
                }
                else
                    comprobanteExterno.EstadoSunat = "Sin envío";

                int resultado = await DalComprobanteVenta.InsertarComprobanteVenta(comprobanteExterno);
                if (resultado <= 0)
                {
                    respuesta.ExcepcionApi = "Error al guardar en la base de datos.";
                    return null;
                }
                else
                {
                    comprobanteExterno.IdComprobanteVenta = resultado;
                    return comprobanteExterno;
                }
            }
            else if (comprobanteRegistrado.EstadoSunat == "Con error" || comprobanteRegistrado.EstadoSunat == "Sin envío")
            {
                comprobanteExterno.IdComprobanteVenta = comprobanteRegistrado.IdComprobanteVenta;
                comprobanteExterno.FechaCreacion = comprobanteRegistrado.FechaCreacion;
                comprobanteExterno.IdUsuarioCreacion = comprobanteRegistrado.IdUsuarioCreacion;
                comprobanteExterno.EstadoSunat = comprobanteRegistrado.EstadoSunat;
                comprobanteExterno.MensajeSunat = comprobanteRegistrado.MensajeSunat;
                comprobanteExterno.CodigoErrorSunat = comprobanteRegistrado.CodigoErrorSunat;
                comprobanteExterno.CodigoEstadoResumen = comprobanteRegistrado.CodigoEstadoResumen;

                int resultado = await DalComprobanteVenta.ActualizarComprobanteVenta(comprobanteExterno);
                if (resultado <= 0)
                {
                    respuesta.ExcepcionApi = "Error al guardar en la base de datos.";
                    return null;
                }
                else return comprobanteExterno;
            }
            else
            {
                bool actualizar = false;
                comprobanteRegistrado.IdDocumentoExterno = comprobanteExterno.IdDocumentoExterno;
                comprobanteRegistrado.ActualizarValoresTributarios();
                comprobanteRegistrado.ActualizarTipoComprobante();

                comprobanteRegistrado.DetalleVenta = await DalComprobanteVenta.ObtenerDetalleComprobanteVentaPorId(
                    comprobanteRegistrado.IdComprobanteVenta
                );

                if (comprobanteRegistrado.DetalleVenta.Count == 0 && comprobanteExterno.DetalleVenta.Count > 0)
                {
                    comprobanteRegistrado.DetalleVenta = comprobanteExterno.DetalleVenta;
                    actualizar = true;
                }

                comprobanteRegistrado.DetalleCuota = await DalComprobanteVenta.ObtenerDetalleCuotaPorIdComprobante(
                    comprobanteRegistrado.IdComprobanteVenta
                );

                if (!comprobanteExterno.Valido && comprobanteRegistrado.EstadoSunat == "Aceptado")
                {
                    comprobanteRegistrado.Valido = false;
                    comprobanteRegistrado.MotivoAnulacion = comprobanteExterno.MotivoAnulacion;
                    actualizar = true;
                }

                if (!String.IsNullOrWhiteSpace(comprobanteExterno.EmailCliente))
                {
                    comprobanteRegistrado.EmailCliente = comprobanteExterno.EmailCliente;
                    actualizar = true;
                }

                if (actualizar)
                {
                    int resultado = await DalComprobanteVenta.ActualizarComprobanteVenta(comprobanteRegistrado);
                    if (resultado <= 0)
                    {
                        respuesta.ExcepcionApi = "Error al guardar en la base de datos.";
                        return null;
                    }
                }

                return comprobanteRegistrado;
            }
        }

        public static async Task ActualizarEstadoEnvioResumen(ResumenBoletas resumen, RespuestaServicio respuesta, int idUsuario)
        {
            resumen.CodigoErrorSunat = respuesta.CodigoErrorSunat;
            resumen.MensajeSunat = respuesta.MensajeSunat;

            if (!String.IsNullOrWhiteSpace(respuesta.CodigoStatus))
            {
                resumen.CodigoStatus = respuesta.CodigoStatus;

                if (respuesta.CodigoStatus.Contains("98"))
                {
                    if (DateTime.Now < resumen.FechaRegistro.AddHours(1))
                    {
                        resumen.MensajeSunat = "El resumen aún no ha sido procesado";
                        resumen.EstadoSunat = "En espera";
                    }
                    else
                    {
                        resumen.EstadoSunat = "Rechazado";
                    }
                }
                else if (respuesta.CodigoStatus.Equals("0"))
                {
                    resumen.EstadoSunat = "Aceptado";
                }
                else if (respuesta.CodigoStatus.Equals("2282") || respuesta.CodigoStatus.Equals("2987"))
                {
                    bool enviados = true;

                    foreach (ComprobanteVenta doc in resumen.ListaComprobantes)
                    {
                        string cadena = doc.Serie + "-" + Convert.ToInt32(doc.Numero).ToString();
                        if (!resumen.MensajeSunat.Contains(cadena))
                        {
                            enviados = false;
                            break;
                        }
                    }

                    if (enviados)
                    {
                        resumen.EstadoSunat = "Aceptado";
                        resumen.MensajeSunat = "Comprobación satisfactoria de resumen " + resumen.Serie + "-" + resumen.Numero;
                        respuesta.MensajeSunat = resumen.MensajeSunat;
                    }

                    else resumen.EstadoSunat = "Rechazado";
                }
                else
                {
                    resumen.EstadoSunat = "Rechazado";
                }
            }
            else
            {
                resumen.CodigoStatus = "99";
                resumen.EstadoSunat = "Rechazado";
            }

            if (resumen.EstadoSunat == "Aceptado" || resumen.EstadoSunat == "Rechazado")
            {
                resumen.IdUsuarioConfirmacionSunat = idUsuario;
                resumen.FechaConfirmacionSunat = DateTime.Now;

                int resultado = await DalResumenBoletas.ActualizarEnvioResumenBoletas(resumen);
                if (resultado <= 0)
                    respuesta.ExcepcionApi = "Error al actualizar los resultados.";
            }

            new clsException(resumen.Serie + "-" + resumen.Numero, resumen.EstadoSunat);
        }

        public static void ObtenerCodigoQr(ComprobanteVenta comprobante, Comercio comercio, string rutaFisica, string archivo)
        {
            string tipoIdentidad = "OTROS";
            if (comprobante.CodigoIdentidadCliente.Equals("1"))
                tipoIdentidad = "DNI";
            else if (comprobante.CodigoIdentidadCliente.Equals("6"))
                tipoIdentidad = "RUC";

            string codificacion = comercio.Ruc + " | " + comprobante.TipoComprobante.abreviatura + " | " + comprobante.Serie +
                " | " + comprobante.Numero + " | " + comprobante.MontoIgv.ToString("0.00") + " | " +
                comprobante.Total.ToString("0.00") + " | " + comprobante.FechaEmision.ToShortDateString() +
                " | " + tipoIdentidad + " | " + comprobante.DocumentoIdentidadCliente + " | ";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(codificacion, QRCodeGenerator.ECCLevel.Q))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (Bitmap bitmap = new Bitmap(qrCode.GetGraphic(20), new Size(150, 150)))
                        {
                            bitmap.Save(rutaFisica + "QR-" + archivo + ".png", ImageFormat.Png);
                        }
                    }
                }
            }
        }

        

        public static DateTime ObtenerFechaDesdeCadena(string cadena)
        {
            string[] partes = cadena.Split('/');
            return new DateTime(Convert.ToInt32(partes[2]), Convert.ToInt32(partes[1]), Convert.ToInt32(partes[0]));
        }

        //public static void CrearArchivoXml(ComprobanteVenta comprobante, string rutaFisica)
        //{
        //    GeneradorXml.GenerarDocumentoXMLUBL21Comprobante(comprobante, comprobante.comercio, rutaFisica);
        //    GeneradorXml.FirmarXml(rutaFisica, 0, comprobante.ArchivoXml, rutaFisica, comprobante.comercio);
        //}

        public static string GetMainFieldsReporteVentas(ComprobanteVenta comprobante)
        {
            string registro = string.Empty;

            if (comprobante.EstadoSunat != "Aceptado")
            {
                comprobante.MontoExportacion = 0;
                comprobante.MontoGravado = 0;
                comprobante.MontoDescuentoBase = 0;
                comprobante.MontoIgv = 0;
                comprobante.MontoExonerado = 0;
                comprobante.MontoInafecto = 0;
                comprobante.MontoIcbper = 0;
                comprobante.MontoCargoAdicional = 0;
                comprobante.Total = 0;
            }
            else if (comprobante.CodigoTipoComprobante == "07")
            {
                comprobante.MontoExportacion *= -1;
                comprobante.MontoGravado *= -1;
                comprobante.MontoDescuentoBase *= -1;
                comprobante.MontoIgv *= -1;
                comprobante.MontoExonerado *= -1;
                comprobante.MontoInafecto *= -1;
                comprobante.MontoIcbper *= -1;
                comprobante.MontoCargoAdicional *= -1;
                comprobante.Total *= -1;
            }

            registro += comprobante.FechaEmision.ToString("dd/MM/yyyy") + "|";

            if (comprobante.DetalleCuota != null)
                registro += comprobante.DetalleCuota.OrderBy(x => x.FechaPago).Last().FechaPago.ToString("dd/MM/yyyy");
            registro += "|";
            
            registro += comprobante.CodigoTipoComprobante + "|";
            registro += comprobante.Serie + "|";
            registro += comprobante.Numero + "|";

            registro += "|"; //esto es para agregar comprob. en cantidad, pero manejamos detallado siempre

            registro += comprobante.CodigoIdentidadCliente + "|";
            registro += comprobante.DocumentoIdentidadCliente + "|";
            registro += comprobante.NombreCompletoCliente + "|";

            registro += comprobante.MontoExportacion.ToString("0.00") + "|";
            registro += comprobante.MontoGravado.ToString("0.00") + "|";
            registro += comprobante.MontoDescuentoBase.ToString("0.00") + "|";
            registro += comprobante.MontoIgv.ToString("0.00") + "|";

            registro += "0.00|"; //revisar descuento al igv...

            registro += comprobante.MontoExonerado.ToString("0.00") + "|";
            registro += comprobante.MontoInafecto.ToString("0.00") + "|";

            registro += "0.00|"; //isc
            registro += "0.00|"; //base ivap
            registro += "0.00|"; //ivap

            registro += comprobante.MontoIcbper.ToString("0.00") + "|";
            //los descuentos que afectan a la base van actuando directamente sobre montos inafectos y exonerados
            registro += comprobante.MontoCargoAdicional.ToString("0.00") + "|";
            registro += comprobante.Total.ToString("0.00") + "|";
            registro += comprobante.CodigoMoneda + "|";

            if (comprobante.CodigoMoneda != "PEN")
                registro += comprobante.TipoCambio.ToString("0.000");
            registro += "|"; //en soles ahora puede ir el tipo de cambio vacío

            if (!String.IsNullOrEmpty(comprobante.ComprobanteAfectado))
            {
                //TODO: guardar la fecha en el registro, en vez de consultarla del registro del comprobante afectado
                registro += comprobante.FechaComprobanteAfectado.ToString("dd/MM/yyyy") + "|";
                registro += comprobante.CodigoTipoComprobanteAfectado + "|";
                registro += comprobante.ComprobanteAfectado.Substring(0, 4) + "|";
                registro += comprobante.ComprobanteAfectado.Substring(5, 8) + "|";
            }
            else registro += "||||";

            registro += "|"; //contratos joint-venture?

            return registro;
        }

        public static List<string> GetReporteVentasDataStandard(List<ComprobanteVenta> lista, string periodo)
        {
            List<string> resultado = new List<string>();

            foreach (ComprobanteVenta comprobante in lista)
            {
                string registro = "";

                registro += periodo + "00|";
                registro += comprobante.IdComprobanteVenta + "|";
                registro += "M" + comprobante.IdComprobanteVenta + "|";
                
                registro += GetMainFieldsReporteVentas(comprobante);

                registro += "|"; //esto es solo para recibos
                registro += "|"; //TODO: medio de pago

                registro += comprobante.EstadoSunat == "Aceptado" ? "1|" : "2|";

                resultado.Add(registro);
            }

            string totales = "|||||||||||TOTALES|";
            totales += lista.Sum(x => x.MontoExportacion).ToString("0.00") + "|";
            totales += lista.Sum(x => x.MontoGravado).ToString("0.00") + "|";
            totales += lista.Sum(x => x.MontoDescuentoBase).ToString("0.00") + "|";
            totales += lista.Sum(x => x.MontoIgv).ToString("0.00") + "|";
            totales += "0.00|";
            totales += lista.Sum(x => x.MontoExonerado).ToString("0.00") + "|";
            totales += lista.Sum(x => x.MontoInafecto).ToString("0.00") + "|";
            totales += "0.00|";
            totales += "0.00|";
            totales += "0.00|";
            totales += lista.Sum(x => x.MontoIcbper).ToString("0.00") + "|";
            totales += lista.Sum(x => x.MontoCargoAdicional).ToString("0.00") + "|";
            totales += lista.Sum(x => x.Total).ToString("0.00") + "|";
            totales += "||||||||||";

            resultado.Add(totales);
            return resultado;
        }

        public static List<string> GetReporteVentasDataRVIE(Comercio comercio, List<ComprobanteVenta> lista, string periodo)
        {
            //para todos los casos, los anulados van en 0 y las NC van en negativo
            //segun la documentacion, el formato 4 es el formato clásico
            //el formato 5 es el simplificado que tiene menos columnas, podría generarse desde un nuevo metodo
            
            //el formato 2 es el nuevo que se generará a continuación y tiene ciertas diferencias con el formato clásico
            //inicia con 2 campos nuevos, luego va el periodo, una columna no usada y a partir de ahi es lo mismo desde la columna 4
            //los campos 33, 34 y 35 ya no van en el formato 2
            //el formato 2 y 3 son exactamente iguales, lo que cambia es el nombre del archivo
            //el formato 1 agrega unas cuantas columnas al final en comparacion con el 2 y el 3

            List<string> resultado = new List<string>();

            foreach (ComprobanteVenta comprobante in lista)
            {
                string registro = "";

                registro += comercio.Ruc + "|";
                registro += comercio.RazonSocial + "|";
                registro += periodo + "||";
                //aqui va un campo que segun se entiende siempre debe ir en blanco

                registro += GetMainFieldsReporteVentas(comprobante);

                resultado.Add(registro);
            }

            return resultado;
        }

        public static DataTable ObtenerModeloDTRegistroVentas()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PERIODO");
            dt.Columns.Add("COD. UNICO");
            dt.Columns.Add("ASIENTO");
            dt.Columns.Add("FECHA");
            dt.Columns.Add("F. VENC.");
            dt.Columns.Add("TIPO COMP.");
            dt.Columns.Add("SERIE COMP.");
            dt.Columns.Add("NRO. COMP.");
            dt.Columns.Add("CONSOLID.");
            dt.Columns.Add("TIPO IDENT.");
            dt.Columns.Add("DOC. IDENT.");
            dt.Columns.Add("CLIENTE");
            dt.Columns.Add("EXPORT.", Type.GetType("System.Decimal"));
            dt.Columns.Add("BASE IGV", Type.GetType("System.Decimal"));
            dt.Columns.Add("DESC. BASE", Type.GetType("System.Decimal"));
            dt.Columns.Add("IGV", Type.GetType("System.Decimal"));
            dt.Columns.Add("DESC. IGV", Type.GetType("System.Decimal"));
            dt.Columns.Add("OP. EXON.", Type.GetType("System.Decimal"));
            dt.Columns.Add("OP. INAF.", Type.GetType("System.Decimal"));
            dt.Columns.Add("ISC", Type.GetType("System.Decimal"));
            dt.Columns.Add("BASE IVAP", Type.GetType("System.Decimal"));
            dt.Columns.Add("IVAP", Type.GetType("System.Decimal"));
            dt.Columns.Add("ICBPER", Type.GetType("System.Decimal"));
            dt.Columns.Add("CARGOS", Type.GetType("System.Decimal"));
            dt.Columns.Add("TOTAL", Type.GetType("System.Decimal"));
            dt.Columns.Add("MONEDA");
            dt.Columns.Add("T. CAMBIO");
            dt.Columns.Add("FECHA AFECT.");
            dt.Columns.Add("TIPO AFECT.");
            dt.Columns.Add("SERIE AFECT.");
            dt.Columns.Add("NRO. AFECT.");
            dt.Columns.Add("CONTRATO");
            dt.Columns.Add("ERROR");
            dt.Columns.Add("M. PAGO");
            dt.Columns.Add("ESTADO");
            return dt;
        }

        public static void CrearReporteExcel(string[] encabezados, DataTable dt, int[] colDecimales, string ruta)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet hojaCalculo = package.Workbook.Worksheets.Add(encabezados[0]);
                int empezarDesde = encabezados.Length + 2;
                hojaCalculo.Cells["A" + empezarDesde].LoadFromDataTable(dt, true);

                // solo es para ajustar el ancho a la columna de mayor longitud
                if (dt.Rows.Count > 0)
                {
                    int contadorCol = 1;
                    foreach (DataColumn columnaDatos in dt.Columns)
                    {
                        ExcelRange columnaExcel = hojaCalculo.Cells[
                            hojaCalculo.Dimension.Start.Row, contadorCol, hojaCalculo.Dimension.End.Row, contadorCol
                        ];

                        int maxLength = columnaExcel.Max(celda => (celda.Value != null ? celda.Value.ToString().Count() : 9));
                        if (maxLength < 150)
                            hojaCalculo.Column(contadorCol).AutoFit();
                        else
                            hojaCalculo.Column(contadorCol).Width = 150;

                        if (colDecimales.Contains(contadorCol))
                            columnaExcel.Style.Numberformat.Format = "0.00";

                        contadorCol++;
                    }
                }

                using (ExcelRange encabezadoExcel = hojaCalculo.Cells[empezarDesde, 1, empezarDesde, dt.Columns.Count])
                {
                    encabezadoExcel.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    encabezadoExcel.Style.Font.Bold = true;
                    encabezadoExcel.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    encabezadoExcel.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                using (ExcelRange totalesExcel = hojaCalculo.Cells[
                    empezarDesde + dt.Rows.Count, 1, empezarDesde + dt.Rows.Count, dt.Columns.Count])
                {
                    totalesExcel.Style.Font.Bold = true;
                }

                if (dt.Rows.Count > 0)
                {
                    using (ExcelRange datosExcel = hojaCalculo.Cells[empezarDesde, 1, empezarDesde + dt.Rows.Count, dt.Columns.Count])
                    {
                        datosExcel.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        datosExcel.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        datosExcel.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        datosExcel.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        datosExcel.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        datosExcel.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        datosExcel.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        datosExcel.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }
                }

                for (int i = 0; i < encabezados.Length; i++)
                {
                    hojaCalculo.Cells["A" + (i + 1).ToString()].Value = encabezados[i];
                    hojaCalculo.Cells["A" + (i + 1).ToString()].Style.Font.Size = 14;
                }

                byte[] resultado = package.GetAsByteArray();
                File.WriteAllBytes(ruta, resultado);
            }
        }

        
    }
}