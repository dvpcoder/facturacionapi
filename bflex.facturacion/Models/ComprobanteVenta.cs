using bflex.facturacion.SunatCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ComprobanteVenta
    {
        public ComprobanteVenta() { }

        public ComprobanteVenta(DocumentoVenta copia)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            CodigoTipoComprobante = copia.Documento.CodigoSunat;

            if (copia.Moneda.idUnidadMonetaria == 2) CodigoMoneda = "PEN";
            else CodigoMoneda = "USD";

            DocumentoIdentidadCliente = copia.Cliente.nroDocumento.Trim();

            if (!String.IsNullOrWhiteSpace(copia.Cliente.Tipo))
                CodigoIdentidadCliente = copia.Cliente.Tipo;
            else if (copia.Cliente.nroDocumento.Trim().Length == 8) 
                CodigoIdentidadCliente = "1";
            else if (copia.Cliente.nroDocumento.Trim().Length == 11) 
                CodigoIdentidadCliente = "6";
            else CodigoIdentidadCliente = "0";

            NombreCompletoCliente = copia.Cliente.nombreCompleto.Trim();

            MontoIgv = Convert.ToDecimal(copia.Igv);
            MontoIcbper = Convert.ToDecimal(copia.Icbper);

            //este es un descuento que afecta la base, luego podría haber descuentoAdicional que no afecta la base
            MontoDescuentoBase = Convert.ToDecimal(copia.DescuentoReal);

            //desde el erp no se puede ingresar docs con exoneracion, se realiza mas bien un calculo
            //el valor se ingresaba desde otro sistema y por eso quedo como un campo
            MontoExonerado = Convert.ToDecimal(copia.MontoExonerado);

            //con la propina es el mismo escenario, excepto que no hay ningun calculo
            //la propina es un cargo adicional, luego podria haber descuentoBase que afecta al calculo de la base
            //asi mismo podria haber otros campos que se integren en este cargo adicional como el FISE
            MontoCargoAdicional = Convert.ToDecimal(copia.ComisionPropina);
                
            MontoGratuito = Convert.ToDecimal(copia.MontoGratuito);
            MontoPercepcion = Convert.ToDecimal(copia.MontoPercepcion);

            if (MontoPercepcion > 0)
                CodigoOperacionVenta = "2001";
            else if (MontoExportacion > 0)
                CodigoOperacionVenta = "0200";
            else CodigoOperacionVenta = "0101";

            Total = Convert.ToDecimal(copia.Total);

            //se deberia poder ingresar el valor directamente, esta columna no existe en el erp
            MontoGravado = Total - MontoIgv - MontoCargoAdicional - MontoIcbper - MontoExonerado;

            //como se habia mencionado en los comercios ya se estaba usando los montos exonerados
            //este fix podria afectar a otros comercios
            if (MontoIgv == 0 && Total > 0 && MontoExonerado == 0)
            {
                MontoExonerado = MontoGravado;
                MontoGravado = 0;
            }

            SubTotal = MontoGravado + MontoExonerado;

            Serie = copia.NroSerie;
            Numero = copia.NroDoc;

            string temp = copia.FechaEmision.Substring(6, copia.FechaEmision.Length - 8);
            FechaEmision = new DateTime(1970, 1, 1).AddMilliseconds(Double.Parse(temp)).AddHours(-5);

            if (FechaEmision < new DateTime(2021, 7, 1))
                throw new Exception("El comprobante enviado tiene una fecha de emisión muy antigua para el servicio");

            OrdenCompra = copia.OrdenCompra;
            GuiaRemision = copia.GuiaRemision;

            if (CodigoTipoComprobante == "07" || CodigoTipoComprobante == "08")
            {
                ComprobanteAfectado = copia.CadenaDocumentoVentaDocAsociados;
                CodigoTipoNotaCreditoDebito = copia.MotivoEmisionNotaCD.codigo;
                MotivoNotaCreditoDebito = copia.Observacion;
                if (Serie.Substring(0, 1) == "F") CodigoTipoComprobanteAfectado = "01";
                else CodigoTipoComprobanteAfectado = "03";
            }

            CodigoEstablecimiento = "0000";

            //en este caso la base es cuando todavia no se descontaba
            //luego que se descuenta queda una nueva base al que se le aplicaran recien los impuestos
            if (MontoDescuentoBase > 0)
                PorcentajeDescuentoBase = MontoDescuentoBase / (MontoGravado + MontoDescuentoBase);
                
            //esta es una regla general que puedo sobreeescribir a gusto mas tarde, pero es la regla normal
            if (copia.Estado.id != 23) Valido = true;
            IdUsuarioCreacion = copia.UsuarioRegistra.id;

            DireccionCliente = copia.Cliente.direccion;
            EmailCliente = copia.Cliente.email;
            Observaciones = copia.Observacion;
            DetalleCuota = new List<CuotaCredito>();

            FormaPago = char.ToUpper(copia.TipoPagoCadena[0]) + copia.TipoPagoCadena.Substring(1).ToLower();

            if (FormaPago.Equals("Credito"))
            {
                if (copia.DetalleCuota != null)
                {
                    DetalleCuota = copia.DetalleCuota;

                    foreach (CuotaCredito cuota in DetalleCuota)
                    {
                        cuota.FechaPago = GestorFacturacion.ObtenerFechaDesdeCadena(cuota.Fecha_Pago);
                    }
                }
                else if (CodigoTipoComprobante.Equals("01"))
                {
                    throw new Exception("Si la venta es al credito, debe tener al menos una cuota.");
                }
            }

            if (!Valido)
            {
                if (String.IsNullOrWhiteSpace(copia.MotivoAnulacion))
                    MotivoAnulacion = "Error de datos.";
                else
                    MotivoAnulacion = copia.MotivoAnulacion;
            }

            IdDocumentoExterno = copia.IdDocumentoVenta;
            Puntaje = copia.Puntaje;
            IdCliente = copia.Cliente.idCliente;
            DetalleVenta = new List<DetalleComprobanteVenta>();

            if (copia.DetalleDocumento != null)
            {
                foreach (DetalleDocumentoVenta d in copia.DetalleDocumento)
                {
                    DetalleComprobanteVenta detalle = new DetalleComprobanteVenta();

                    detalle.Cantidad = Convert.ToDecimal(d.Cantidad);
                    detalle.CodigoUnidad = d.UnidadMedida.codigoSunat;

                    //valores que paga el cliente final
                    detalle.Total = Convert.ToDecimal(d.CostoVenta);
                    detalle.MontoBruto = Convert.ToDecimal(d.ValorVenta);
                    detalle.MontoIcbper = Convert.ToDecimal(d.Icbper);

                    if (!String.IsNullOrWhiteSpace(d.DescuentoReal))
                    {
                        detalle.MontoDescuentoBase = Convert.ToDecimal(d.DescuentoReal);
                        detalle.PorcentajeDescuentoBase = detalle.MontoDescuentoBase / (detalle.MontoBruto + detalle.MontoDescuentoBase);
                    }

                    detalle.ValorUnitario = (detalle.MontoBruto + detalle.MontoDescuentoBase) / detalle.Cantidad;
                    detalle.MontoIgv = detalle.Total - detalle.MontoBruto - detalle.MontoIcbper;
                    detalle.ValorIcbperUnitario = detalle.MontoIcbper / detalle.Cantidad;

                    if (detalle.MontoIgv == 0) detalle.PorcentajeIgv = 0;
                    else if (d.Presentacion.nombre.Equals("SERVICIO") || d.Presentacion.nombre.Equals("SERVICIOS") 
                        || d.Presentacion.nombre.Equals("FLETE DE REPARTO"))
                    {
                        detalle.PorcentajeIgv = 18;
                        detalle.MontoBruto = detalle.Total / (decimal)1.18;
                        detalle.MontoIgv = detalle.Total - detalle.MontoBruto;
                    }
                    else detalle.PorcentajeIgv = Convert.ToInt32(detalle.MontoIgv / detalle.MontoBruto * 100);

                    if (detalle.PorcentajeIgv > 11) detalle.PorcentajeIgv = 18;
                    else if (detalle.PorcentajeIgv <= 11 && detalle.PorcentajeIgv > 1) detalle.PorcentajeIgv = 10;

                    //este es un codigo que eventualmente debe desaparecer
                    if (Convert.ToDecimal(d.Precio) == 0)
                    {
                        d.Precio = "5";
                        MontoGratuito += (5 * detalle.Cantidad);
                    }

                    if (detalle.Total > 0)
                    {
                        //cuando es una linea normal recalculare el precio por si acaso ya que vendra del precio descontado
                        //y por si acaso se les ocurra no mandarlo por error
                        detalle.PrecioUnitario = (detalle.MontoBruto + detalle.MontoIgv) / detalle.Cantidad;
                        detalle.ValorReferencial = detalle.MontoBruto;
                        detalle.CodigoPrecio = "01";
                    }
                    else
                    {
                        //cuando es una linea gratuita el precio es al que suele venderse
                        //si no se envia ningun precio se esta mandando como 5 soles
                        detalle.PrecioUnitario = Convert.ToDecimal(d.Precio);
                        detalle.ValorReferencial = detalle.PrecioUnitario * detalle.Cantidad;
                        detalle.CodigoPrecio = "02";
                    }

                    if (CodigoOperacionVenta == "0200")
                    {
                        detalle.CodigoTributo = "9995";
                        detalle.NombreTributo = "EXP";
                        detalle.CodigoTipoTributo = "FRE";
                        detalle.CodigoAfectacion = "40";
                    }
                    else if (detalle.MontoIgv > 0)
                    {
                        detalle.CodigoTributo = "1000";
                        detalle.NombreTributo = "IGV";
                        detalle.CodigoTipoTributo = "VAT";
                        detalle.CodigoAfectacion = "10";
                    }
                    else if (detalle.Total > 0)
                    {
                        detalle.CodigoTributo = "9997";
                        detalle.NombreTributo = "EXO";
                        detalle.CodigoTipoTributo = "VAT";
                        detalle.CodigoAfectacion = "20";
                    }
                    else
                    {
                        detalle.CodigoTributo = "9996";
                        detalle.NombreTributo = "GRA";
                        detalle.CodigoTipoTributo = "FRE";
                        detalle.CodigoAfectacion = "37";
                    }

                    detalle.NombreProducto = d.Presentacion.nombre;

                    if (!String.IsNullOrEmpty(d.Presentacion.codigoSunat))
                        detalle.CodigoProducto = d.Presentacion.codigoSunat;

                    DetalleVenta.Add(detalle);
                }
            }

            //en la version antigua llama a estos 2 metodos complementarios por separado
            ActualizarValoresTributarios();
            ActualizarTipoComprobante();
        }

        public void ActualizarValoresTributarios ()
        {
            //esto es exclusivo para el xml
            DetalleTributario = new List<TributoVenta>();

            //cuando esto es exportacion es el unico que debe ir, los demas no, por eso va asi
            if (MontoExportacion > 0)
            {
                DetalleTributario.Add(new TributoVenta()
                {
                    CodigoTributo = "9995",
                    NombreTributo = "EXP",
                    CodigoTipoTributo = "FRE",
                    MontoBase = MontoExportacion,
                    MontoTributo = 0,
                    CodigoTributoResumen = "04"
                });
            }
            else
            {
                //esto no tiene sentido si esta exonerado o inafecto pero de momento será asi
                if (MontoGravado > 0 || MontoDescuentoBase > 0)
                {
                    DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "1000",
                        NombreTributo = "IGV",
                        CodigoTipoTributo = "VAT",
                        MontoBase = MontoGravado,
                        MontoTributo = MontoIgv,
                        CodigoTributoResumen = "01"
                    });
                }

                if (MontoGratuito > 0)
                {
                    DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "9996",
                        NombreTributo = "GRA",
                        CodigoTipoTributo = "FRE",
                        MontoBase = MontoGratuito,
                        MontoTributo = 0,
                        CodigoTributoResumen = "05"
                    });
                }

                if (MontoExonerado > 0)
                {
                    DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "9997",
                        NombreTributo = "EXO",
                        CodigoTipoTributo = "VAT",
                        MontoBase = MontoExonerado,
                        MontoTributo = 0,
                        CodigoTributoResumen = "02"
                    });
                }

                if (MontoInafecto > 0)
                {
                    DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "9998",
                        NombreTributo = "INA",
                        CodigoTipoTributo = "FRE",
                        MontoBase = MontoInafecto,
                        MontoTributo = 0,
                        CodigoTributoResumen = "03"
                    });
                }

                if (MontoIcbper > 0)
                {
                    DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "7152",
                        NombreTributo = "ICBPER",
                        CodigoTipoTributo = "OTH",
                        MontoBase = 0,
                        MontoTributo = MontoIcbper
                    });
                }
            }
        }

        public void ActualizarTipoComprobante()
        {
            //esto es para archivos, mails y media en general
            TipoComprobante = new TipoComprobante();

            if (CodigoTipoComprobante.Equals("01"))
            {
                TipoComprobante.abreviatura = "FAC";
                TipoComprobante.carpeta = "Facturas";
                TipoComprobante.nombre = "Factura";
            }
            else if (CodigoTipoComprobante.Equals("03"))
            {
                TipoComprobante.abreviatura = "BOL";
                TipoComprobante.carpeta = "Boletas";
                TipoComprobante.nombre = "Boleta de Venta";
            }
            else if (CodigoTipoComprobante.Equals("07"))
            {
                TipoComprobante.abreviatura = "NCR";
                TipoComprobante.carpeta = "NotasCredito";
                TipoComprobante.nombre = "Nota de Crédito";
            }
            else if (CodigoTipoComprobante.Equals("08"))
            {
                TipoComprobante.abreviatura = "NDB";
                TipoComprobante.carpeta = "NotasDebito";
                TipoComprobante.nombre = "Nota de Débito";
            }

            if (CodigoIdentidadCliente.Equals("1"))
                TipoIdentidad = "DNI";
            else if (CodigoIdentidadCliente.Equals("6"))
                TipoIdentidad = "RUC";
            else if (CodigoIdentidadCliente.Equals("4"))
                TipoIdentidad = "C. de Extranjería";
            else TipoIdentidad = "Documento de Identidad";
        }

        public void ActualizarUbicacionArchivos()
        {
            RutaCarpetaArchivos = "~/Content/files/" + comercio.CarpetaServidor + "/" + TipoComprobante.carpeta +
                "/" + FechaEmision.ToString("yyyyMM") + "/" + Serie + "-" + Numero + "/";
            ArchivoPdf = Serie + "-" + Numero + ".pdf";
            ArchivoQr = "QR" + "-" + Serie + "-" + Numero + ".png";
            ArchivoXml = comercio.Ruc + "-" + CodigoTipoComprobante + "-" + Serie + "-" + Numero;
            ArchivoZip = ArchivoXml + ".zip";
            ArchivoCdr = "CDR-" + ArchivoZip;
            CodificacionQr = comercio.Ruc + " | " + TipoComprobante.abreviatura + " | " + Serie + " | " + Numero +
                " | " + MontoIgv.ToString("0.00") + " | " + Total.ToString("0.00") + " | " +
                FechaEmision.ToShortDateString() + " | " + TipoIdentidad + " | " + DocumentoIdentidadCliente + " | ";
        }

        public void ObtenerTotalEnLetras()
        {
            MontoEnLetras = Utilitarios.NumeroALetras(Total.ToString());
        }

        public void ActualizarAtributosEstructura()
        {
            ActualizarValoresTributarios();
            ActualizarTipoComprobante();
            ActualizarUbicacionArchivos();
            ObtenerTotalEnLetras();
        }

        public void ActualizarCodigosEstructura() 
        {
            Valido = !Anulado;

            if (String.IsNullOrWhiteSpace(CodigoIdentidadCliente))
            {
                if (DocumentoIdentidadCliente.Length == 8) CodigoIdentidadCliente = "1";
                else if (DocumentoIdentidadCliente.Length == 11) CodigoIdentidadCliente = "6";
                else CodigoIdentidadCliente = "0";
            }

            NombreCompletoCliente = NombreCompletoCliente.Trim();

            if (MontoPercepcion > 0) CodigoOperacionVenta = "2001";
            else if (MontoDetraccion > 0) CodigoOperacionVenta = "1001";
            else if (MontoExportacion > 0) CodigoOperacionVenta = "0200";
            else CodigoOperacionVenta = "0101";

            //para no hacerme problemas con el script, pero se supone que esta cosa siempre llegara vacia
            if (FechaEmision == DateTime.MinValue)
            {
                string[] partes1 = Fecha_Emision.Split('/');
                string[] partes2 = Hora_Emision.Split(':');
                FechaEmision = new DateTime(
                    Convert.ToInt32(partes1[2]), Convert.ToInt32(partes1[1]), Convert.ToInt32(partes1[0]),
                    Convert.ToInt32(partes2[0]), Convert.ToInt32(partes2[1]), Convert.ToInt32(partes2[2])
                );

                //definir fecha de envio minimo
                if (FechaEmision < new DateTime(2021, 7, 1))
                    throw new Exception("--El comprobante enviado tiene una fecha más antigua que la fecha de apertura de este servicio.");

                if (FechaEmision < DateTime.Now.AddYears(-2))
                    throw new Exception("--El comprobante enviado tiene una antiguedad mayor a 2 años.");
            }

            if (MontoDescuentoBase > 0)
                PorcentajeDescuentoBase = MontoDescuentoBase / (SubTotal + MontoDescuentoBase);

            if (MontoCargoAdicional > 0)
                PorcentajeCargoAdicional = MontoCargoAdicional / SubTotal;

            if (String.IsNullOrWhiteSpace(CodigoEstablecimiento) || CodigoEstablecimiento.Equals("0"))
                CodigoEstablecimiento = "0000";

            if (DetalleVenta == null)
                throw new Exception("--Debe indicar el detalle del comprobante");

            foreach (DetalleComprobanteVenta detalle in DetalleVenta)
            {
                if (detalle.Total > 0)
                {
                    detalle.ValorReferencial = detalle.MontoBruto;
                    detalle.CodigoPrecio = "01";
                }
                else
                {
                    detalle.ValorReferencial = detalle.PrecioUnitario * detalle.Cantidad;
                    detalle.CodigoPrecio = "02";
                }

                if (MontoExportacion > 0)
                {
                    detalle.CodigoTributo = "9995";
                    detalle.NombreTributo = "EXP";
                    detalle.CodigoTipoTributo = "FRE";
                    detalle.CodigoAfectacion = "40";
                }
                else if (detalle.MontoIgv > 0)
                {
                    detalle.CodigoTributo = "1000";
                    detalle.NombreTributo = "IGV";
                    detalle.CodigoTipoTributo = "VAT";
                    detalle.CodigoAfectacion = "10";
                }
                else if (detalle.EsInafecto)
                {
                    detalle.CodigoTributo = "9998";
                    detalle.NombreTributo = "INA";
                    detalle.CodigoTipoTributo = "FRE";
                    detalle.CodigoAfectacion = "30";
                }
                else if (detalle.Total > 0)
                {
                    detalle.CodigoTributo = "9997";
                    detalle.NombreTributo = "EXO";
                    detalle.CodigoTipoTributo = "VAT";
                    detalle.CodigoAfectacion = "20";
                }
                else
                {
                    detalle.CodigoTributo = "9996";
                    detalle.NombreTributo = "GRA";
                    detalle.CodigoTipoTributo = "FRE";
                    detalle.CodigoAfectacion = "37";
                }

                int len = detalle.CodigoUnidad.Length;
                if (len < 2 || len > 3) detalle.CodigoUnidad = "NIU";

                if (detalle.MontoIgv == 0) detalle.PorcentajeIgv = 0;
                else if (detalle.PorcentajeIgv == 0)
                    detalle.PorcentajeIgv = Convert.ToInt32(detalle.MontoIgv / detalle.MontoBruto * 100);

                if (detalle.MontoDescuentoBase > 0)
                    detalle.PorcentajeDescuentoBase = detalle.MontoDescuentoBase / (detalle.MontoBruto + detalle.MontoDescuentoBase);

                if (detalle.MontoIcbper > 0)
                    detalle.ValorIcbperUnitario = detalle.MontoIcbper / detalle.Cantidad;
            }

            if (FormaPago.Equals("Credito"))
            {
                if (DetalleCuota != null)
                {
                    foreach (CuotaCredito cuota in DetalleCuota)
                    {
                        cuota.FechaPago = GestorFacturacion.ObtenerFechaDesdeCadena(cuota.Fecha_Pago);
                    }
                }
                else if (CodigoTipoComprobante.Equals("01"))
                {
                    throw new Exception("--Si la venta es al credito, debe tener al menos una cuota.");
                }
                else
                {
                    DetalleCuota = new List<CuotaCredito>();
                }
            }
            else
            {
                DetalleCuota = new List<CuotaCredito>();
            }
        }

        public int IdComprobanteVenta { get; set; }
        public Comercio comercio { get; set; }
        public string CodigoTipoComprobante { get; set; }
        public string CodigoMoneda { get; set; }
        public string DocumentoIdentidadCliente { get; set; }
        public string CodigoIdentidadCliente { get; set; }
        public string NombreCompletoCliente { get; set; }
        public string CodigoOperacionVenta { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal MontoGravado { get; set; }
        public decimal MontoCargoAdicional { get; set; }
        public decimal MontoIcbper { get; set; }
        public decimal MontoExonerado { get; set; }
        public decimal MontoInafecto { get; set; }
        public decimal MontoDescuentoBase { get; set; }
        public decimal MontoGratuito { get; set; }
        public decimal MontoExportacion { get; set; }
        public decimal MontoPercepcion { get; set; }
        public decimal MontoRetencion { get; set; }
        public decimal MontoDetraccion { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdUsuarioCreacion { get; set; }
        public DateTime FechaConfirmacionSunat { get; set; }
        public int IdUsuarioConfirmacionSunat { get; set; }
        public string OrdenCompra { get; set; }
        public string GuiaRemision { get; set; }
        public string ComprobanteAfectado { get; set; }
        public string CodigoTipoNotaCreditoDebito { get; set; }
        public string MotivoNotaCreditoDebito { get; set; }
        public string CodigoTipoComprobanteAfectado { get; set; }
        public DateTime FechaComprobanteAfectado { get; set; }
        public string CodigoEstablecimiento { get; set; }
        public decimal PorcentajeDescuentoBase { get; set; }
        public decimal PorcentajeDetraccion { get; set; }
        public decimal PorcentajeCargoAdicional { get; set; }
        public string CodigoDetraccion { get; set; }
        public decimal TipoCambio { get; set; }
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string CodigoErrorSunat { get; set; }
        public string CodigoEstadoResumen { get; set; }
        public bool Valido { get; set; }
        public bool Anulado { get; set; }
        public string MotivoAnulacion { get; set; }
        public int IdDocumentoExterno { get; set; }
        public List<DetalleComprobanteVenta> DetalleVenta { get; set; }
        public List<TributoVenta> DetalleTributario { get; set; }
        public List<CuotaCredito> DetalleCuota { get; set; }
        public string DireccionCliente { get; set; }
        public string EmailCliente { get; set; }
        public string FormaPago { get; set; }
        public string Observaciones { get; set; }
        public TipoComprobante TipoComprobante { get; set; }
        public string Fecha_Emision { get; set; }
        public string Hora_Emision { get; set; }

        public int IdCliente { get; set; }
        public int Puntaje { get; set; }

        public string RutaCarpetaArchivos { get; set; }
        public string ArchivoPdf { get; set; }
        public string ArchivoXml { get; set; }
        public string ArchivoZip { get; set; }
        public string ArchivoCdr { get; set; }
        public string ArchivoQr { get; set; }
        public string CodificacionQr { get; set; }
        public string TipoIdentidad { get; set; }
        public string MontoEnLetras { get; set; }
    }
}