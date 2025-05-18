using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class ConfiguracionGeneral
    {
        public Decimal ValorIgvActual { get; set; }
        public Decimal ValorIcbperActual { get; set; }
        public ConfiguracionGeneral()
        {
            ValorIgvActual = (decimal)0.18;
            ValorIcbperActual = (decimal)0.3;
        }
    }
}