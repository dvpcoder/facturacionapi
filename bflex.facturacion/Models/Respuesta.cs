using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Respuesta
    {
        //ESTA ESTRUCTURA ES PARTE DE FACTV2

        public string ExcepcionApi { get; set; }
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string OrigenRespuesta { get; set; }
        public string CodigoRespuesta { get; set; }

        public Respuesta(ComprobanteVenta comprobante, bool conversion)
        {
            MensajeSunat = comprobante.MensajeSunat;
            string[] sub = comprobante.CodigoErrorSunat.Split('.');
            OrigenRespuesta = sub[0];
            if (sub.Length > 1)
                CodigoRespuesta = sub[1];

            if (conversion)
            {
                if (OrigenRespuesta == "sunat")
                {
                    int valor = -1;
                    if (Int32.TryParse(CodigoRespuesta, out valor))
                    {
                        if (valor == 0 || valor > 4000)
                            comprobante.EstadoSunat = "Aceptado";
                        else if (valor < 0)
                            comprobante.EstadoSunat = "Sin envío";
                        //else if ((valor >= 2020 && valor <= 2075) || (valor >= 2120 && valor <= 2800) || (valor >= 3100 && valor <= 3320))
                        else
                            comprobante.EstadoSunat = "Con error";
                    }
                    else
                        comprobante.EstadoSunat = "Sin envío";
                }
                else if (comprobante.CodigoErrorSunat == "Client.1032")
                {
                    if (comprobante.Valido)
                        comprobante.EstadoSunat = "Rechazado";
                    else
                        comprobante.EstadoSunat = "Anulado";
                }
                else if (comprobante.CodigoErrorSunat == "Client.1033")
                    comprobante.EstadoSunat = "Aceptado";
                else
                    comprobante.EstadoSunat = "Con error";
            }

            EstadoSunat = comprobante.EstadoSunat;
        }

        public Respuesta() { }

        public void ComunicarComprobante (ComprobanteVenta comprobante)
        {
            comprobante.MensajeSunat = MensajeSunat;
            comprobante.CodigoErrorSunat = OrigenRespuesta + "." + CodigoRespuesta;
        }

        public void ComunicarGrupo (ComprobanteGrupal grupo)
        {
            grupo.MensajeSunat = MensajeSunat;
            grupo.CodigoErrorSunat = OrigenRespuesta + "." + CodigoRespuesta;
        }

        //sin embargo no merece instanciar una respuesta de resumen o comunicacion
        //porque estas se devolveran completas
    }

    
}