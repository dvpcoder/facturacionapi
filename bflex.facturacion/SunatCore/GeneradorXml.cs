using bflex.facturacion.Models;
using Ionic.Zip;
using Microsoft.Web.Services3.Security.X509;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace bflex.facturacion.SunatCore
{
    public class GeneradorXml
    {
        public static void GenerarDocumentoXMLUBL21Comprobante(ComprobanteVenta obj, Comercio comercio, string ruta)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            String contenidoXML = "";
            String titulo = "";
            String prop = "";
            String prop2 = "";

            if (obj.CodigoTipoComprobante.Equals("01") || obj.CodigoTipoComprobante.Equals("03"))
            {
                titulo = "Invoice";
                prop = "InvoicedQuantity";
                prop2 = "LegalMonetaryTotal";
            }
            else if (obj.CodigoTipoComprobante.Equals("07"))
            {
                titulo = "CreditNote";
                prop = "CreditedQuantity";
                prop2 = "LegalMonetaryTotal";
            }
            else if (obj.CodigoTipoComprobante.Equals("08"))
            {
                titulo = "DebitNote";
                prop = "DebitedQuantity";
                prop2 = "RequestedMonetaryTotal";
            }

            contenidoXML = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"no\"?> " +
                "<" + titulo + " xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:" + titulo +
                "-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" ";

            if (comercio.Efact)
                contenidoXML += "xmlns:ext=\"urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2\">";
            else
                contenidoXML += "xmlns:ccts=\"urn:un:unece:uncefact:documentation:2\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:ext=\"urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2\" xmlns:qdt=\"urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2\" xmlns:udt=\"urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";

            contenidoXML +=
                "  <ext:UBLExtensions> " +
                "    <ext:UBLExtension> " +
                "      <ext:ExtensionContent> " +
                "      </ext:ExtensionContent> " +
                "    </ext:UBLExtension> " +
                "  </ext:UBLExtensions> " +
                "  <cbc:UBLVersionID>2.1</cbc:UBLVersionID> " +
                "  <cbc:CustomizationID>2.0</cbc:CustomizationID> ";

            if (obj.CodigoTipoComprobante.Equals("01") || obj.CodigoTipoComprobante.Equals("03"))
            {
                contenidoXML +=
                    "  <cbc:ProfileID schemeAgencyName=\"PE:SUNAT\" schemeName=\"SUNAT:Identificador de Tipo de Operaci?n\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo17\">" +
                        obj.CodigoOperacionVenta + "</cbc:ProfileID> ";
            }

            contenidoXML +=
                "  <cbc:ID>" + obj.Serie + "-" + obj.Numero + "</cbc:ID> " +
                "  <cbc:IssueDate>" + obj.FechaEmision.ToString("yyyy-MM-dd") + "</cbc:IssueDate> " +
                "  <cbc:IssueTime>" + obj.FechaEmision.ToString("HH:mm:ss") + "</cbc:IssueTime> ";

            if (obj.CodigoTipoComprobante.Equals("01") || obj.CodigoTipoComprobante.Equals("03"))
            {
                contenidoXML +=
                "  <cbc:InvoiceTypeCode listID=\"" + obj.CodigoOperacionVenta + "\" listAgencyName=\"PE:SUNAT\" listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01\">" +
                    obj.CodigoTipoComprobante + "</cbc:InvoiceTypeCode> ";
            }

            contenidoXML +=
                "  <cbc:Note languageLocaleID=\"1000\">" + Utilitarios.NumeroALetras(obj.Total.ToString()) + "</cbc:Note>" +
                "  <cbc:DocumentCurrencyCode listAgencyName=\"United Nations Economic Commission for Europe\" listName=\"Currency\" listID=\"ISO 4217 Alpha\">" +
                    obj.CodigoMoneda + "</cbc:DocumentCurrencyCode>";

            if (!String.IsNullOrWhiteSpace(obj.OrdenCompra))
            {
                contenidoXML +=
                    "  <cac:OrderReference> " +
                    "    <cbc:ID>" + obj.OrdenCompra + "</cbc:ID> " +
                    "  </cac:OrderReference>";
            }

            if (!String.IsNullOrWhiteSpace(obj.GuiaRemision))
            {
                contenidoXML +=
                    "  <cac:DespatchDocumentReference> " +
                    "    <cbc:ID>" + obj.GuiaRemision + "</cbc:ID> " +
                    "    <cbc:DocumentTypeCode listAgencyName=\"PE:SUNAT\" listName=\"SUNAT:Identificador de guía relacionada\" listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01\">09</cbc:DocumentTypeCode>" +
                    "  </cac:DespatchDocumentReference>";
            }

            if (obj.CodigoTipoComprobante.Equals("07") || obj.CodigoTipoComprobante.Equals("08"))
            {
                contenidoXML +=
                    "  <cac:DiscrepancyResponse> " +
                    "    <cbc:ReferenceID>" + obj.ComprobanteAfectado + "</cbc:ReferenceID> " +
                    "    <cbc:ResponseCode listAgencyName=\"PE:SUNAT\" listName=\"Tipo de nota de credito\"  listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo09\">" +
                        obj.CodigoTipoNotaCreditoDebito + "</cbc:ResponseCode> " +
                    "    <cbc:Description><![CDATA[" + obj.MotivoNotaCreditoDebito + "]]></cbc:Description> " +
                    "  </cac:DiscrepancyResponse> " +
                    "  <cac:BillingReference> " +
                    "    <cac:InvoiceDocumentReference> " +
                    "      <cbc:ID>" + obj.ComprobanteAfectado + "</cbc:ID> " +
                    "      <cbc:DocumentTypeCode listAgencyName=\"PE:SUNAT\" listName=\"Tipo de Documento\"  listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01\">" +
                            obj.CodigoTipoComprobanteAfectado + "</cbc:DocumentTypeCode> " +
                    "    </cac:InvoiceDocumentReference> " +
                    "  </cac:BillingReference> ";
            }

            contenidoXML +=
                "  <cac:Signature> " +
                "    <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "    <cbc:Note>Elaborado por " + comercio.ElaboradoPor + "</cbc:Note> " +
                "    <cac:SignatoryParty> " +
                "      <cac:PartyIdentification> " +
                "        <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "      </cac:PartyIdentification> " +
                "      <cac:PartyName> " +
                "        <cbc:Name><![CDATA[" + comercio.RazonSocial + "]]></cbc:Name> " +
                "      </cac:PartyName> " +
                "    </cac:SignatoryParty> " +
                "    <cac:DigitalSignatureAttachment> " +
                "      <cac:ExternalReference> " +
                "        <cbc:URI>" + comercio.Ruc + "-" + obj.Serie + "-" + obj.Numero + "</cbc:URI> " +
                "      </cac:ExternalReference> " +
                "    </cac:DigitalSignatureAttachment> " +
                "  </cac:Signature> " +

                "  <cac:AccountingSupplierParty> " +
                "    <cac:Party> " +
                "      <cac:PartyIdentification> " +
                "        <cbc:ID schemeAgencyName=\"PE:SUNAT\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06\" schemeID=\"6\">" +
                            comercio.Ruc + "</cbc:ID> " +
                "      </cac:PartyIdentification> " +
                "      <cac:PartyName> " +
                "        <cbc:Name><![CDATA[" + comercio.NombreComercial + "]]></cbc:Name> " +
                "      </cac:PartyName> " +
                "      <cac:PartyLegalEntity> " +
                "        <cbc:RegistrationName><![CDATA[" + comercio.RazonSocial + "]]></cbc:RegistrationName> " +
                "        <cac:RegistrationAddress> " +
                "          <cbc:ID schemeAgencyName=\"PE:INEI\" schemeName=\"Ubigeos\">" + comercio.UbigeoFiscal + "</cbc:ID> " +
                "          <cbc:AddressTypeCode>" + obj.CodigoEstablecimiento + "</cbc:AddressTypeCode> ";

            if (!String.IsNullOrWhiteSpace(comercio.UrbanizacionFiscal))
            {
                contenidoXML += "<cbc:CitySubdivisionName><![CDATA[" + comercio.UrbanizacionFiscal + "]]></cbc:CitySubdivisionName> ";
            }

            contenidoXML +=
                "          <cbc:CityName>" + comercio.ProvinciaFiscal + "</cbc:CityName> " +
                "          <cbc:CountrySubentity>" + comercio.DepartamentoFiscal + "</cbc:CountrySubentity> " +
                "          <cbc:District>" + comercio.DistritoFiscal + "</cbc:District> " +
                "          <cac:AddressLine> " +
                "            <cbc:Line><![CDATA[" + comercio.CalleFiscal + "]]></cbc:Line> " +
                "          </cac:AddressLine> " +
                "          <cac:Country> " +
                "            <cbc:IdentificationCode listAgencyName=\"United Nations Economic Commission for Europe\" listName=\"Country\" listID=\"ISO 3166-1\">PE</cbc:IdentificationCode> " +
                "          </cac:Country> " +
                "        </cac:RegistrationAddress> " +
                "      </cac:PartyLegalEntity> " +
                "    </cac:Party> " +
                "  </cac:AccountingSupplierParty> " +

                "  <cac:AccountingCustomerParty> " +
                "    <cac:Party> " +
                "      <cac:PartyIdentification> " +
                "        <cbc:ID schemeAgencyName=\"PE:SUNAT\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06\" schemeID=\"" +
                            obj.CodigoIdentidadCliente + "\">" + obj.DocumentoIdentidadCliente + "</cbc:ID> " +
                "      </cac:PartyIdentification> " +
                "      <cac:PartyLegalEntity> " +
                "        <cbc:RegistrationName><![CDATA[" + obj.NombreCompletoCliente + "]]></cbc:RegistrationName> " +
                "      </cac:PartyLegalEntity> " +
                "    </cac:Party> " +
                "  </cac:AccountingCustomerParty> ";

            if (obj.MontoPercepcion > 0)
            {
                contenidoXML +=
                    "  <cac:PaymentTerms> " +
                    "    <cbc:ID>Percepcion</cbc:ID> " +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + 
                        (obj.SubTotal + obj.MontoPercepcion).ToString("0.00") + "</cbc:Amount> " +
                    "  </cac:PaymentTerms> ";
            }

            if (obj.MontoDetraccion > 0)
            {
                contenidoXML +=
                    "  <cac:PaymentMeans> " +
                    "    <cbc:ID>Detraccion</cbc:ID> " +
                    "    <cbc:PaymentMeansCode>001</cbc:PaymentMeansCode> " +
                    "    <cac:PayeeFinancialAccount> " +
                    "      <cbc:ID>" + comercio.CuentaDetraccion + "</cbc:ID> " +
                    "    </cac:PayeeFinancialAccount> " +
                    "  </cac:PaymentMeans> " +
                    "  <cac:PaymentTerms> " +
                    "    <cbc:ID>Detraccion</cbc:ID> " +
                    "    <cbc:PaymentMeansID>" + obj.CodigoDetraccion + "</cbc:PaymentMeansID> " +
                    "    <cbc:PaymentPercent>" + obj.PorcentajeDetraccion + "</cbc:PaymentPercent> " +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoDetraccion.ToString("0.00") + "</cbc:Amount> " +
                    "  </cac:PaymentTerms> ";
            }

            if (
                obj.CodigoTipoComprobante.Equals("01") ||
                (obj.CodigoTipoComprobante.Equals("07") && obj.CodigoTipoNotaCreditoDebito.Equals("13"))
            )
            {
                contenidoXML +=
                    "  <cac:PaymentTerms> " +
                    "    <cbc:ID>FormaPago</cbc:ID> " +
                    "    <cbc:PaymentMeansID>" + obj.FormaPago + "</cbc:PaymentMeansID> ";

                if (obj.FormaPago.Equals("Credito"))
                {
                    contenidoXML += "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" +
                        obj.DetalleCuota.Sum(x => x.Monto).ToString("0.00") + "</cbc:Amount> ";

                    int n = 0;
                    foreach (CuotaCredito cuota in obj.DetalleCuota)
                    {
                        n++;
                        contenidoXML +=
                        "  </cac:PaymentTerms> " +
                        "  <cac:PaymentTerms> " +
                        "    <cbc:ID>FormaPago</cbc:ID> " +
                        "    <cbc:PaymentMeansID>Cuota" + n.ToString().PadLeft(3, '0') + "</cbc:PaymentMeansID> " +
                        "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + cuota.Monto.ToString("0.00") + "</cbc:Amount> " +
                        "    <cbc:PaymentDueDate>" + cuota.FechaPago.ToString("yyyy-MM-dd") + "</cbc:PaymentDueDate> ";
                    }
                }
                
                contenidoXML += "  </cac:PaymentTerms> ";
            }

            if (obj.MontoDescuentoBase > 0)
            {
                contenidoXML +=
                    "  <cac:AllowanceCharge> " +
                    "    <cbc:ChargeIndicator>false</cbc:ChargeIndicator> " +
                    "    <cbc:AllowanceChargeReasonCode>02</cbc:AllowanceChargeReasonCode> " +
                    "    <cbc:MultiplierFactorNumeric>" + obj.PorcentajeDescuentoBase.ToString("0.0000") + "</cbc:MultiplierFactorNumeric>" +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoDescuentoBase.ToString("0.00") + "</cbc:Amount> " +
                    "    <cbc:BaseAmount currencyID=\"" + obj.CodigoMoneda + "\">" +
                        (obj.MontoGravado + obj.MontoDescuentoBase).ToString("0.00") + "</cbc:BaseAmount> " +
                    "  </cac:AllowanceCharge> ";
            }

            if (obj.MontoRetencion > 0)
            {
                contenidoXML +=
                    "  <cac:AllowanceCharge> " +
                    "    <cbc:ChargeIndicator>false</cbc:ChargeIndicator> " +
                    "    <cbc:AllowanceChargeReasonCode>62</cbc:AllowanceChargeReasonCode> " +
                    "    <cbc:MultiplierFactorNumeric>0.03</cbc:MultiplierFactorNumeric>" +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoRetencion.ToString("0.00") + "</cbc:Amount> " +
                    "    <cbc:BaseAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.Total.ToString("0.00") + "</cbc:BaseAmount> " +
                    "  </cac:AllowanceCharge> ";
            }

            if (obj.MontoCargoAdicional > 0)
            {
                //AQUI SE HA ESTANDARIZADO LA RAZON 46 PERO PODRIA HABER ALGUN OTRO CARGO CON OTRO VALOR
                //EL CARGOADICIONAL PASA A SER LA PROPINA PERO ESTO PODRIA CAMBIARSE
                //ES DECIR ESTAR COMPUESTO POR DIVERSOS CODIGOS QUE SUMADOS DEN UN NUEVO CARGO ADICIONAL
                contenidoXML +=
                    "  <cac:AllowanceCharge> " +
                    "    <cbc:ChargeIndicator>true</cbc:ChargeIndicator> " +
                    "    <cbc:AllowanceChargeReasonCode>46</cbc:AllowanceChargeReasonCode> " +
                    "    <cbc:MultiplierFactorNumeric>" + obj.PorcentajeCargoAdicional.ToString("0.0000") + "</cbc:MultiplierFactorNumeric>" +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoCargoAdicional.ToString("0.00") + "</cbc:Amount> " +
                    "    <cbc:BaseAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.SubTotal.ToString("0.00") + "</cbc:BaseAmount> " +
                    "  </cac:AllowanceCharge> ";
            }

            if (obj.MontoPercepcion > 0)
            {
                contenidoXML +=
                    "  <cac:AllowanceCharge> " +
                    "    <cbc:ChargeIndicator>true</cbc:ChargeIndicator> " +
                    "    <cbc:AllowanceChargeReasonCode>51</cbc:AllowanceChargeReasonCode> " +
                    "    <cbc:MultiplierFactorNumeric>0.02</cbc:MultiplierFactorNumeric>" +
                    "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoPercepcion.ToString("0.00") + "</cbc:Amount> " +
                    "    <cbc:BaseAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.SubTotal.ToString("0.00") + "</cbc:BaseAmount> " +
                    "  </cac:AllowanceCharge> ";
            }

            //ESTA LINEA EN REALIDAD ES LA SUMA DE TODOS LOS IMPUESTOS CONOCIDOS: IGV, ISC E IVAP
            contenidoXML +=
                "  <cac:TaxTotal> " +
                "    <cbc:TaxAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoIgv.ToString("0.00") + "</cbc:TaxAmount> ";

            foreach (TributoVenta tributo in obj.DetalleTributario)
            {
                contenidoXML += "    <cac:TaxSubtotal> ";

                if (tributo.MontoBase > 0 || obj.MontoDescuentoBase > 0)
                {
                    contenidoXML += "      <cbc:TaxableAmount currencyID=\"" + obj.CodigoMoneda + "\">" +
                        tributo.MontoBase.ToString("0.00") + "</cbc:TaxableAmount> ";
                }

                contenidoXML +=
                "      <cbc:TaxAmount currencyID=\"" + obj.CodigoMoneda + "\">" +
                        tributo.MontoTributo.ToString("0.00") + "</cbc:TaxAmount> " +
                "      <cac:TaxCategory> " +
                "        <cac:TaxScheme> " +
                "          <cbc:ID schemeAgencyName=\"PE:SUNAT\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05\">" +
                            tributo.CodigoTributo + "</cbc:ID> " +
                "          <cbc:Name>" + tributo.NombreTributo + "</cbc:Name> " +
                "          <cbc:TaxTypeCode>" + tributo.CodigoTipoTributo + "</cbc:TaxTypeCode> " +
                "        </cac:TaxScheme> " +
                "      </cac:TaxCategory> " +
                "    </cac:TaxSubtotal> ";
            }

            contenidoXML +=
                "  </cac:TaxTotal> " +
                "  <cac:" + prop2 + "> ";

            if (obj.CodigoTipoComprobante.Equals("01") || obj.CodigoTipoComprobante.Equals("03"))
            {
                contenidoXML +=
                "    <cbc:LineExtensionAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.SubTotal.ToString("0.00") + "</cbc:LineExtensionAmount>" +
                "    <cbc:TaxInclusiveAmount currencyID=\"" + obj.CodigoMoneda + "\">" +
                    (obj.SubTotal + obj.MontoIgv + obj.MontoIcbper).ToString("0.00") + "</cbc:TaxInclusiveAmount>";

                if (obj.MontoCargoAdicional > 0)
                    contenidoXML +=
                    "    <cbc:ChargeTotalAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.MontoCargoAdicional.ToString("0.00") + "</cbc:ChargeTotalAmount> ";

            }

            contenidoXML +=
                "    <cbc:PayableAmount currencyID=\"" + obj.CodigoMoneda + "\">" + obj.Total.ToString("0.00") + "</cbc:PayableAmount> " +
                "  </cac:" + prop2 + "> ";

            int c = 1;
            foreach (DetalleComprobanteVenta linea in obj.DetalleVenta)
            {
                contenidoXML +=
                    "  <cac:" + titulo + "Line> " +
                    "    <cbc:ID>" + c + "</cbc:ID> " +
                    "    <cbc:" + prop + " unitCodeListAgencyName=\"United Nations Economic Commission for Europe\" unitCodeListID=\"UN/ECE rec 20\" unitCode=\"" +
                        linea.CodigoUnidad + "\">" + linea.Cantidad.ToString("0.0000000000") + "</cbc:" + prop + ">" +
                    "    <cbc:LineExtensionAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.ValorReferencial.ToString("0.00") + "</cbc:LineExtensionAmount> " +

                    "    <cac:PricingReference> " +
                    "      <cac:AlternativeConditionPrice> " +
                    "        <cbc:PriceAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.PrecioUnitario.ToString("0.00") + "</cbc:PriceAmount> " +
                    "        <cbc:PriceTypeCode listAgencyName=\"PE:SUNAT\" listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16\">" +
                                linea.CodigoPrecio + "</cbc:PriceTypeCode> " +
                    "      </cac:AlternativeConditionPrice> " +
                    "    </cac:PricingReference> ";

                if (linea.MontoDescuentoBase > 0)
                {
                    contenidoXML +=
                        "    <cac:AllowanceCharge> " +
                        "    <cbc:ChargeIndicator>false</cbc:ChargeIndicator> " +
                        "    <cbc:AllowanceChargeReasonCode>00</cbc:AllowanceChargeReasonCode> " +
                        "    <cbc:MultiplierFactorNumeric>" + linea.PorcentajeDescuentoBase.ToString("0.0000") + "</cbc:MultiplierFactorNumeric> " +
                        "    <cbc:Amount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.MontoDescuentoBase.ToString("0.00") + "</cbc:Amount> " +
                        "    <cbc:BaseAmount currencyID=\"" + obj.CodigoMoneda + "\">" +
                            (linea.MontoBruto + linea.MontoDescuentoBase).ToString("0.00") + "</cbc:BaseAmount> " +
                        "    </cac:AllowanceCharge> ";
                }

                contenidoXML +=
                    "    <cac:TaxTotal> " +
                    "      <cbc:TaxAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.MontoIgv.ToString("0.00") + "</cbc:TaxAmount> " +
                    "      <cac:TaxSubtotal> " +
                    "        <cbc:TaxableAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.ValorReferencial.ToString("0.00") + "</cbc:TaxableAmount> " +
                    "        <cbc:TaxAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.MontoIgv.ToString("0.00") + "</cbc:TaxAmount> " +
                    "        <cac:TaxCategory> " +
                    "          <cbc:Percent>" + linea.PorcentajeIgv.ToString("0.00") + "</cbc:Percent> " +
                    "          <cbc:TaxExemptionReasonCode listAgencyName=\"PE:SUNAT\" listURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07\">" +
                                linea.CodigoAfectacion + "</cbc:TaxExemptionReasonCode> " +
                    "          <cac:TaxScheme> " +
                    "            <cbc:ID schemeAgencyName=\"PE:SUNAT\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05\">" +
                                    linea.CodigoTributo + "</cbc:ID> " +
                    "            <cbc:Name>" + linea.NombreTributo + "</cbc:Name> " +
                    "            <cbc:TaxTypeCode>" + linea.CodigoTipoTributo + "</cbc:TaxTypeCode> " +
                    "          </cac:TaxScheme> " +
                    "        </cac:TaxCategory> " +
                    "      </cac:TaxSubtotal> ";

                if (linea.MontoIcbper > 0)
                {
                    contenidoXML +=
                    "      <cac:TaxSubtotal> " +
                    "        <cbc:TaxAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.MontoIcbper.ToString("0.00") + "</cbc:TaxAmount> " +
                    "        <cbc:BaseUnitMeasure unitCode=\"NIU\">" + linea.Cantidad.ToString("0") + "</cbc:BaseUnitMeasure>" +
                    "        <cac:TaxCategory> " +
                    "          <cbc:PerUnitAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.ValorIcbperUnitario.ToString("0.00") + "</cbc:PerUnitAmount> " +
                    "          <cac:TaxScheme> " +
                    "            <cbc:ID schemeAgencyName=\"PE:SUNAT\" schemeURI=\"urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05\">7152</cbc:ID> " +
                    "            <cbc:Name>ICBPER</cbc:Name> " +
                    "            <cbc:TaxTypeCode>OTH</cbc:TaxTypeCode> " +
                    "          </cac:TaxScheme> " +
                    "        </cac:TaxCategory> " +
                    "      </cac:TaxSubtotal> ";
                }

                contenidoXML +=
                    "    </cac:TaxTotal> " +
                    "    <cac:Item> " +
                    "      <cbc:Description><![CDATA[" + linea.NombreProducto + "]]></cbc:Description> ";

                if (obj.CodigoOperacionVenta == "0200")
                {
                    contenidoXML +=
                    "      <cac:CommodityClassification> " +
                    "        <cbc:ItemClassificationCode listID=\"UNSPSC\" listAgencyName=\"GS1 US\" listName=\"Item Classification\">" +
                                linea.CodigoProducto + "</cbc:ItemClassificationCode> " +
                    "      </cac:CommodityClassification>";
                }

                contenidoXML +=
                    "    </cac:Item> " +
                    "    <cac:Price> " +
                    "      <cbc:PriceAmount currencyID=\"" + obj.CodigoMoneda + "\">" + linea.ValorUnitario.ToString("0.0000000000") + "</cbc:PriceAmount> " +
                    "    </cac:Price> " +
                    "  </cac:" + titulo + "Line> ";

                c++;
            }

            contenidoXML += "</" + titulo + ">";

            XmlDocument archivo = new XmlDocument();
            archivo.LoadXml(contenidoXML);
            archivo.Save(ruta + "datos.xml");
        }

        public static void GenerarXMLResumenBoletas(ResumenBoletas resumen, Comercio comercio, string ruta)
        {
            String contenidoXML =
                "<SummaryDocuments xmlns=\"urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:ext=\"urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2\" xmlns:sac=\"urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"> " +
                "  <ext:UBLExtensions> " +
                "    <ext:UBLExtension> " +
                "      <ext:ExtensionContent> " +
                "      </ext:ExtensionContent> " +
                "    </ext:UBLExtension> " +
                "  </ext:UBLExtensions> " +
                "  <cbc:UBLVersionID>2.0</cbc:UBLVersionID> " +
                "  <cbc:CustomizationID>1.1</cbc:CustomizationID> " +
                "  <cbc:ID>RC-" + resumen.Serie + "-" + resumen.Numero + "</cbc:ID> " +
                "  <cbc:ReferenceDate>" + resumen.FechaBoletas.ToString("yyyy-MM-dd") + "</cbc:ReferenceDate> " +
                "  <cbc:IssueDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</cbc:IssueDate> " +
                "  <cac:Signature> " +
                "    <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "    <cbc:Note>Elaborado por " + comercio.ElaboradoPor + "</cbc:Note> " +
                "    <cac:SignatoryParty> " +
                "      <cac:PartyIdentification> " +
                "        <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "      </cac:PartyIdentification> " +
                "      <cac:PartyName> " +
                "        <cbc:Name><![CDATA[" + comercio.RazonSocial + "]]></cbc:Name> " +
                "      </cac:PartyName> " +
                "    </cac:SignatoryParty> " +
                "    <cac:DigitalSignatureAttachment> " +
                "      <cac:ExternalReference> " +
                "        <cbc:URI>" + comercio.Ruc + "-" + resumen.Serie + "-" + resumen.Numero + "</cbc:URI> " +
                "      </cac:ExternalReference> " +
                "    </cac:DigitalSignatureAttachment> " +
                "  </cac:Signature> " +
                "  <cac:AccountingSupplierParty> " +
                "    <cbc:CustomerAssignedAccountID>" + comercio.Ruc + "</cbc:CustomerAssignedAccountID> " +
                "    <cbc:AdditionalAccountID>6</cbc:AdditionalAccountID> " +
                "      <cac:Party> " +
                "        <cac:PartyLegalEntity> " +
                "          <cbc:RegistrationName><![CDATA[" + comercio.RazonSocial + "]]></cbc:RegistrationName> " +
                "        </cac:PartyLegalEntity> " +
                "      </cac:Party> " +
                "    </cac:AccountingSupplierParty> ";

            int i = 1;
            foreach (ComprobanteVenta boleta in resumen.ListaBoletas)
            {
                String detalle =
                "      <sac:SummaryDocumentsLine> " +
                "        <cbc:LineID>" + i + "</cbc:LineID> " +
                "        <cbc:DocumentTypeCode>" + boleta.CodigoTipoComprobante + "</cbc:DocumentTypeCode> " +
                "        <cbc:ID>" + boleta.Serie + "-" + boleta.Numero + "</cbc:ID> " +
                "        <cac:AccountingCustomerParty> " +
                "          <cbc:CustomerAssignedAccountID>" + boleta.DocumentoIdentidadCliente + "</cbc:CustomerAssignedAccountID> " +
                "          <cbc:AdditionalAccountID>" + boleta.CodigoIdentidadCliente + "</cbc:AdditionalAccountID> " +
                "        </cac:AccountingCustomerParty> ";

                if (boleta.CodigoTipoComprobante.Equals("07") || boleta.CodigoTipoComprobante.Equals("08"))
                {
                    detalle +=
                    "        <cac:BillingReference> " +
                    "          <cac:InvoiceDocumentReference> " +
                    "            <cbc:ID>" + boleta.ComprobanteAfectado + "</cbc:ID> " +
                    "            <cbc:DocumentTypeCode>03</cbc:DocumentTypeCode> " +
                    "          </cac:InvoiceDocumentReference> " +
                    "        </cac:BillingReference> ";
                }

                detalle +=
                "        <cac:Status> " +
                "          <cbc:ConditionCode>" + boleta.CodigoEstadoResumen + "</cbc:ConditionCode> " +
                "        </cac:Status> " +
                "        <sac:TotalAmount currencyID=\"" + boleta.CodigoMoneda + "\">" + boleta.Total.ToString("0.00") + "</sac:TotalAmount> ";

                foreach (TributoVenta tributo in boleta.DetalleTributario)
                {
                    if (!String.IsNullOrEmpty(tributo.CodigoTributoResumen))
                    {
                        detalle +=
                        "    <sac:BillingPayment> " +
                        "      <cbc:PaidAmount currencyID=\"" + boleta.CodigoMoneda + "\">" + tributo.MontoBase.ToString("0.00") + "</cbc:PaidAmount> " +
                        "      <cbc:InstructionID>" + tributo.CodigoTributoResumen + "</cbc:InstructionID> " +
                        "    </sac:BillingPayment> ";
                    }
                }

                //aqui solo van cargos, los descuentos se supone ya estan restados de los otros montos
                if (boleta.MontoCargoAdicional > 0)
                {
                    detalle +=
                    "    <cac:AllowanceCharge> " +
                    "      <cbc:ChargeIndicator>true</cbc:ChargeIndicator> " +
                    "      <cbc:Amount currencyID=\"" + boleta.CodigoMoneda + "\">" + boleta.MontoCargoAdicional.ToString("0.00") + "</cbc:Amount> " +
                    "    </cac:AllowanceCharge> ";
                }

                //aqui recien empiezan en realidad los tributos, y no los pagos
                //donde el igv es obligatorio, si no va nada se pone 0 y el icbper es otro con el que se trabajo
                if (boleta.DetalleTributario.Count(x => x.NombreTributo.Equals("IGV")) == 0)
                {
                    boleta.DetalleTributario.Add(new TributoVenta()
                    {
                        CodigoTributo = "1000",
                        NombreTributo = "IGV",
                        CodigoTipoTributo = "VAT",
                        MontoTributo = 0
                    });
                }

                foreach (TributoVenta tributo in boleta.DetalleTributario)
                {
                    detalle +=
                    "        <cac:TaxTotal> " +
                    "          <cbc:TaxAmount currencyID=\"" + boleta.CodigoMoneda + "\">" + tributo.MontoTributo.ToString("0.00") + "</cbc:TaxAmount> " +
                    "          <cac:TaxSubtotal> " +
                    "            <cbc:TaxAmount currencyID=\"" + boleta.CodigoMoneda + "\">" + tributo.MontoTributo.ToString("0.00") + "</cbc:TaxAmount> " +
                    "            <cac:TaxCategory> " +
                    "              <cac:TaxScheme> " +
                    "                <cbc:ID>" + tributo.CodigoTributo + "</cbc:ID> " +
                    "                <cbc:Name>" + tributo.NombreTributo + "</cbc:Name> " +
                    "                <cbc:TaxTypeCode>" + tributo.CodigoTipoTributo + "</cbc:TaxTypeCode> " +
                    "              </cac:TaxScheme> " +
                    "            </cac:TaxCategory> " +
                    "          </cac:TaxSubtotal> " +
                    "        </cac:TaxTotal> ";
                }

                detalle += "      </sac:SummaryDocumentsLine> ";

                contenidoXML += detalle;
                i++;
            }

            contenidoXML += "</SummaryDocuments>";

            XmlDocument archivo = new XmlDocument();
            archivo.LoadXml(contenidoXML);
            archivo.Save(ruta + "datos.xml");
        }

        public static void GenerarXMLComunicacionBaja(ComunicacionBaja comunicacion, Comercio comercio, string ruta)
        {

            String contenidoXML =
                "<VoidedDocuments xmlns=\"urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:ext=\"urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2\" xmlns:sac=\"urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"> " +
                "  <ext:UBLExtensions> " +
                "    <ext:UBLExtension> " +
                "      <ext:ExtensionContent> " +
                "      </ext:ExtensionContent> " +
                "    </ext:UBLExtension> " +
                "  </ext:UBLExtensions> " +
                "  <cbc:UBLVersionID>2.0</cbc:UBLVersionID> " +
                "  <cbc:CustomizationID>1.0</cbc:CustomizationID> " +
                "  <cbc:ID>RA-" + comunicacion.Serie + "-" + comunicacion.Numero + "</cbc:ID> " +
                "  <cbc:ReferenceDate>" + comunicacion.FechaFacturas.ToString("yyyy-MM-dd") + "</cbc:ReferenceDate> " +
                "  <cbc:IssueDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</cbc:IssueDate> " +
                "  <cac:Signature> " +
                "    <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "    <cbc:Note>Elaborado por " + comercio.ElaboradoPor + "</cbc:Note> " +
                "    <cac:SignatoryParty> " +
                "      <cac:PartyIdentification> " +
                "        <cbc:ID>" + comercio.Ruc + "</cbc:ID> " +
                "      </cac:PartyIdentification> " +
                "      <cac:PartyName> " +
                "        <cbc:Name><![CDATA[" + comercio.RazonSocial + "]]></cbc:Name> " +
                "      </cac:PartyName> " +
                "    </cac:SignatoryParty> " +
                "    <cac:DigitalSignatureAttachment> " +
                "      <cac:ExternalReference> " +
                "        <cbc:URI>" + comercio.Ruc + "-" + comunicacion.Serie + "-" + comunicacion.Numero + "</cbc:URI> " +
                "      </cac:ExternalReference> " +
                "    </cac:DigitalSignatureAttachment> " +
                "  </cac:Signature> " +
                "  <cac:AccountingSupplierParty> " +
                "    <cbc:CustomerAssignedAccountID>" + comercio.Ruc + "</cbc:CustomerAssignedAccountID> " +
                "    <cbc:AdditionalAccountID>6</cbc:AdditionalAccountID> " +
                "    <cac:Party> " +
                "      <cac:PartyLegalEntity> " +
                "        <cbc:RegistrationName><![CDATA[" + comercio.RazonSocial + "]]></cbc:RegistrationName> " +
                "      </cac:PartyLegalEntity> " +
                "    </cac:Party> " +
                "  </cac:AccountingSupplierParty> ";

            int i = 1;
            foreach (ComprobanteVenta factura in comunicacion.ListaFacturas)
            {
                String detalle =
                "      <sac:VoidedDocumentsLine> " +
                "        <cbc:LineID>" + i + "</cbc:LineID> " +
                "        <cbc:DocumentTypeCode>" + factura.CodigoTipoComprobante + "</cbc:DocumentTypeCode> " +
                "        <sac:DocumentSerialID>" + factura.Serie + "</sac:DocumentSerialID> " +
                "        <sac:DocumentNumberID>" + factura.Numero + "</sac:DocumentNumberID> " +
                "        <sac:VoidReasonDescription>" + factura.MotivoAnulacion + "</sac:VoidReasonDescription> " +
                "      </sac:VoidedDocumentsLine> ";

                contenidoXML += detalle;
                i++;
            }

            contenidoXML += "</VoidedDocuments>";

            XmlDocument archivo = new XmlDocument();
            archivo.LoadXml(contenidoXML);
            archivo.Save(ruta + "datos.xml");
        }

        public static void FirmarXml(string rutaBase, int tipoEstructura, string archivo, string rutaEspecifica, Comercio comercio)
        {
            FileInfo file = new FileInfo(rutaBase + comercio.ArchivoCertificado);
            if (!file.Exists)
                throw new Exception("--El comercio no cuenta con certificado.");

            byte[] certificado = File.ReadAllBytes(rutaBase + comercio.ArchivoCertificado);
            var objCertificado = new X509Certificate2();
            objCertificado.Import(certificado, comercio.PasswordCertificado, X509KeyStorageFlags.MachineKeySet);
            
            byte[] xml = File.ReadAllBytes(rutaBase + "datos.xml");
            XmlDocument objXml = new XmlDocument();
            string resultado = "";

            using (MemoryStream memoria = new MemoryStream(xml))
            {
                objXml.PreserveWhitespace = true;
                objXml.Load(memoria);
                XmlNode nodoExtension = objXml.GetElementsByTagName(
                    "ExtensionContent", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"
                ).Item(tipoEstructura);

                if (nodoExtension == null)
                    throw new InvalidOperationException("No se pudo encontrar el nodo ExtensionContent en el XML");
                nodoExtension.RemoveAll();

                /////// PREPARACION DE LA FIRMA /////////
                SignedXml firmaXml = new SignedXml(objXml);

                //firmaXml.SigningKey = (RSA)objCertificador.PrivateKey;
                firmaXml.SigningKey = objCertificado.GetRSAPrivateKey();
                Signature firma = firmaXml.Signature;

                XmlDsigEnvelopedSignatureTransform DEST = new XmlDsigEnvelopedSignatureTransform();
                Reference referencia = new Reference(string.Empty);
                referencia.AddTransform(DEST);
                firma.SignedInfo.AddReference(referencia);

                KeyInfo info = new KeyInfo();
                KeyInfoX509Data x509Data = new KeyInfoX509Data(objCertificado);
                x509Data.AddSubjectName(objCertificado.Subject);
                info.AddClause(x509Data);
                firma.KeyInfo = info;

                firma.Id = comercio.ElaboradoPor;
                firmaXml.ComputeSignature();
                /////// FIN DE PREPARACION DE LA FIRMA ///////

                nodoExtension.AppendChild(firmaXml.GetXml());
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Encoding = Encoding.GetEncoding("ISO-8859-1")
                };

                using (MemoryStream subMemoria = new MemoryStream())
                {
                    using (XmlWriter writer = XmlWriter.Create(subMemoria, settings))
                        objXml.WriteTo(writer);
                    resultado = Convert.ToBase64String(subMemoria.ToArray());
                }
            }

            File.WriteAllBytes(rutaEspecifica + archivo + ".xml", Convert.FromBase64String(resultado));

            MemoryStream m = new MemoryStream(Convert.FromBase64String(resultado));
            using (ZipFile fileZip = new ZipFile())
            {
                fileZip.AddEntry(archivo + ".xml", m);
                fileZip.Save(rutaEspecifica + archivo + ".zip");
            }
            m.Close();
        }

        public static void ConstruirCDR(ref RespuestaServicio respuesta, String archivo, String ruta, Byte[] bytesSalida)
        {
            FileStream fs = new FileStream(ruta + "CDR-" + archivo + ".zip", FileMode.Create);
            fs.Write(bytesSalida, 0, bytesSalida.Length);
            fs.Close();

            using (MemoryStream memoria = new MemoryStream(bytesSalida))
            {
                using (ZipFile zip = ZipFile.Read(memoria))
                {
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        if (!entry.FileName.EndsWith(".xml")) continue;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            entry.Extract(ms);
                            ms.Position = 0;
                            String resultado = new StreamReader(ms).ReadToEnd();

                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(resultado);

                            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                            xmlnsManager.AddNamespace(
                                "ar", "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2"
                            );
                            xmlnsManager.AddNamespace(
                                "cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
                            );
                            xmlnsManager.AddNamespace(
                                "cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
                            );
                            respuesta.CodigoErrorSunat = "sunat." + xmlDoc.SelectSingleNode(
                                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", xmlnsManager
                            ).InnerText;
                            respuesta.MensajeSunat = xmlDoc.SelectSingleNode(
                                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:Description", xmlnsManager
                            ).InnerText;
                        }
                    }
                }
            }
        }

        #region facturacionv2

        public static async Task<Respuesta> ConstruirCDR (string archivoCdr, string ruta, Byte[] bytesSalida)
        {
            Respuesta respuesta = new Respuesta();
            respuesta.OrigenRespuesta = "sunat";

            FileStream fs = new FileStream(ruta + archivoCdr, FileMode.Create);
            fs.Write(bytesSalida, 0, bytesSalida.Length);
            fs.Close();

            using (MemoryStream memoria = new MemoryStream(bytesSalida))
            {
                using (ZipFile zip = ZipFile.Read(memoria))
                {
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        if (!entry.FileName.EndsWith(".xml")) continue;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            entry.Extract(ms);
                            ms.Position = 0;
                            String resultado = await new StreamReader(ms).ReadToEndAsync();

                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(resultado);

                            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                            xmlnsManager.AddNamespace(
                                "ar", "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2"
                            );
                            xmlnsManager.AddNamespace(
                                "cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
                            );
                            xmlnsManager.AddNamespace(
                                "cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
                            );
                            respuesta.CodigoRespuesta = xmlDoc.SelectSingleNode(
                                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", xmlnsManager
                            ).InnerText;
                            respuesta.MensajeSunat = xmlDoc.SelectSingleNode(
                                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:Description", xmlnsManager
                            ).InnerText;
                        }
                    }
                }
            }

            return respuesta;
        }

        //esto es de ultima hora, por mientras
        public static async Task<Respuesta> RecuperarCDR (string rutaCompleta)
        {
            XmlDocument documento = new XmlDocument();
            Respuesta respuesta = new Respuesta();
            respuesta.OrigenRespuesta = "sunat";

            FileInfo file = new FileInfo(rutaCompleta);
            if (!file.Exists)
            {
                throw new Exception("--No se encuentra el cdr de confirmación.");
            }

            using (ZipFile zip = ZipFile.Read(rutaCompleta))
            {
                foreach (ZipEntry entry in zip.Entries)
                {
                    if (!entry.FileName.EndsWith(".xml")) continue;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        entry.Extract(ms);
                        ms.Position = 0;
                        string contenidoXml = await new StreamReader(ms).ReadToEndAsync();
                        documento.LoadXml(contenidoXml);
                    }
                }
            }

            XmlNamespaceManager selector = new XmlNamespaceManager(documento.NameTable);
            selector.AddNamespace("ar", "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2");
            selector.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            selector.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            respuesta.CodigoRespuesta = documento.SelectSingleNode(
                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", selector
            ).InnerText;
            respuesta.MensajeSunat = documento.SelectSingleNode(
                "/ar:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:Description", selector
            ).InnerText;

            return respuesta;
        }

        #endregion

    }
}