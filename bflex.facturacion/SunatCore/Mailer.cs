using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace bflex.facturacion.SunatCore
{
    public class Mailer
    {
        public void EnviarCorreo(string mailDestino, string asunto, Comercio comercio, string[] adjuntos, string contenido)
        {
            string mailOrigen = "facturacion_electronica@mysupportproject.pe";
            string password = "MyPasswordMail";

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = new NetworkCredential(mailOrigen, password);
                smtp.Port = 587;

                MailAddress origen = new MailAddress(mailOrigen, "Facturación Electrónica " + comercio.ElaboradoPor);
                MailAddress destino = new MailAddress(mailDestino);

                using (MailMessage mensaje = new MailMessage(origen, destino))
                {
                    mensaje.IsBodyHtml = true;
                    mensaje.Subject = asunto;
                    mensaje.Bcc.Add(new MailAddress("admin@mysupportproject.pe"));
                    mensaje.Bcc.Add(new MailAddress("clients@mysupportproject.pe"));
                    mensaje.Body = contenido;

                    if (!String.IsNullOrWhiteSpace(comercio.Email) && !mailDestino.Equals(comercio.Email))
                        mensaje.Bcc.Add(new MailAddress(comercio.Email));

                    if (adjuntos != null && adjuntos.Length > 0)
                    {
                        foreach (String rutaArchivo in adjuntos)
                        {
                            if (!String.IsNullOrWhiteSpace(rutaArchivo))
                            {
                                Attachment adjunto = new Attachment(rutaArchivo);
                                adjunto.ContentId = Guid.NewGuid().ToString();
                                mensaje.Attachments.Add(adjunto);
                            }
                        }
                    }

                    smtp.Send(mensaje);
                }
            }
        }
    }
}