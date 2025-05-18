using System;
using System.Collections.Generic;
using System.Text;

namespace bflex.facturacion.Models
{
    public class Columns
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; } = new Search();

        public Columns() { }
    }
}
