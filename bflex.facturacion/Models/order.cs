using System;
using System.Collections.Generic;
using System.Text;

namespace bflex.facturacion.Models
{
    public class order
    {
        public int column { get; set; }
        public string dir { get; set; }
        public string descolumorder
        {
            get { return column + " " + dir.ToUpper(); }
        }
    }
}
