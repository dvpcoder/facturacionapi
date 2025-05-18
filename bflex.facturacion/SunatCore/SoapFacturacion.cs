using bflex.facturacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace bflex.facturacion.SunatCore
{
    public abstract class SoapFacturacion
    {
        public abstract void EnviarComprobanteDetallado(ref RespuestaServicio respuesta, string ruta, string nombreArchivo);

        public abstract Task EnviarComprobanteDetallado(ComprobanteVenta obj, string ruta);

        public abstract void Iniciar();

        public abstract string EnviarGrupoComprobantes(ref RespuestaServicio respuesta, string ruta, string nombreArchivo);

        public abstract void EnviarGrupoComprobantes(ComprobanteGrupal grupo, string ruta);

        public abstract void ConsultarTicketGrupal(ref RespuestaServicio respuesta, string ruta, string nombreArchivo, string nroTicket);

        public abstract Task ConsultarTicketGrupal(ComprobanteGrupal grupo, string ruta);

        public abstract void Finalizar();
    }
}