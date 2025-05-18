using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class RespuestaContenedor
    {
        //ESTA ESTRUCTURA ES PARTE DE FACTV2
        public string Serie { get; set; }
        public int Numero { get; set; }
        public string FechaComprobantes { get; set; }
        public string FechaRegistro { get; set; }
        public string FechaConfirmacionSunat { get; set; }
        public string NroTicket { get; set; }
        public string CodigoStatus { get; set; }
        public string EstadoSunat { get; set; }
        public string MensajeSunat { get; set; }
        public string OrigenRespuesta { get; set; }
        public string CodigoRespuesta { get; set; }
        public string ExcepcionApi { get; set; }
        public List<ComprobanteRespuestaMin> ListaComprobantes { get; set; }

        public RespuestaContenedor() { }

        //no se puede modificar el nombre de los campos, tendriamos que usar otros al menos en este caso...
        public RespuestaContenedor(ComprobanteGrupal grupo, bool conversion)
        {
            Serie = grupo.Serie;
            Numero = grupo.Numero;
            FechaComprobantes = grupo.FechaComprobantes.ToShortDateString();
            FechaRegistro = grupo.FechaRegistro.ToString();
            NroTicket = grupo.NroTicket;
            ListaComprobantes = new List<ComprobanteRespuestaMin>();

            if (grupo.FechaConfirmacionSunat != DateTime.MinValue)
                FechaConfirmacionSunat = grupo.FechaConfirmacionSunat.ToString();

            foreach (ComprobanteVenta obj in grupo.ListaComprobantes)
            {
                ListaComprobantes.Add(new ComprobanteRespuestaMin()
                {
                    IdLocal = obj.IdDocumentoExterno,
                    Serie = obj.Serie,
                    Numero = obj.Numero,
                    Status = obj.CodigoEstadoResumen
                });
            }

            if (!String.IsNullOrWhiteSpace(grupo.CodigoErrorSunat))
            {
                string[] sub = grupo.CodigoErrorSunat.Split('.');
                OrigenRespuesta = sub[0];
                CodigoRespuesta = sub[1];
            }

            if (conversion)
            {
                if (!String.IsNullOrWhiteSpace(grupo.CodigoStatus))
                {
                    if (grupo.CodigoStatus.Contains("98"))
                    {
                        if (DateTime.Now < grupo.FechaRegistro.AddHours(1))
                        {
                            grupo.MensajeSunat = "Aún no se ha procesado el lote de comprobantes.";
                            grupo.EstadoSunat = "En espera";
                        }
                        else
                        {
                            grupo.EstadoSunat = "Rechazado";
                        }
                    }
                    else if (grupo.CodigoStatus.Equals("0"))
                    {
                        grupo.EstadoSunat = "Aceptado";
                    }
                    else if (CodigoRespuesta.Equals("2282") || CodigoRespuesta.Equals("2987"))
                    {
                        bool enviados = true;

                        foreach (ComprobanteRespuestaMin doc in ListaComprobantes)
                        {
                            string cadena = doc.Serie + "-" + Convert.ToInt32(doc.Numero).ToString();
                            if (!grupo.MensajeSunat.Contains(cadena))
                            {
                                enviados = false;
                                break;
                            }
                        }

                        if (enviados)
                        {
                            if (CodigoRespuesta.Equals("2282"))
                                grupo.MensajeSunat = "Comprobantes aceptados en el resumen " + grupo.Serie + "-" + grupo.Numero;
                            else
                                grupo.MensajeSunat = "Comprobantes anulados en el lote " + grupo.Serie + "-" + grupo.Numero;

                            //habia codigo 2323 que tambien se colocaba para resumenes con comprobantes rechazados pero nunca se usó
                            grupo.EstadoSunat = "Aceptado";
                        }

                        else grupo.EstadoSunat = "Rechazado";
                    }
                    else
                    {
                        grupo.EstadoSunat = "Rechazado";
                    }
                }
                else
                {
                    grupo.CodigoStatus = "99";
                    grupo.EstadoSunat = "Rechazado";
                }

                if (grupo.EstadoSunat.Equals("Aceptado") || grupo.EstadoSunat.Equals("Rechazado"))
                {
                    FechaConfirmacionSunat = DateTime.Now.ToString();
                }
            }

            EstadoSunat = grupo.EstadoSunat;
            MensajeSunat = grupo.MensajeSunat;
            CodigoStatus = grupo.CodigoStatus;
        }
    }
}