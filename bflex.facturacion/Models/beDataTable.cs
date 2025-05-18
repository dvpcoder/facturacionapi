using System;
using System.Collections.Generic;
using System.Text;

namespace bflex.facturacion.Models
{ 
    public class beDataTable
    {
        public int draw { get; set; } = 0;
        public string recordsTotal { get; set; } = "0";
        public string recordsFiltered { get; set; } = "0";
        public Array data { get; set; } = new Array[0];
        public int length { get; set; } = 10;
        public int start { get; set; } = 0;
        public List<order> order { get; set; }
        public Search search { get; set; } = new Search();
        public List<Columns> columns { get; set; } = new List<Columns>();
        public int end
        {
            get { return start + length; }
        }
        public string colorden
        {
            get
            {
                int i = 0;
                string valor = "NO-ORDER";// string.Empty;
                if (order != null)
                {
                    foreach (Columns col in columns)
                    {

                        if (order.Count > 0)
                        {
                            if (i == order[0].column)
                            {
                                if (col.name == "") { valor = "NO-ORDER"; }
                                else { valor = col.name + " " + order[0].dir.ToUpper(); }
                            }
                        }
                        i++;
                    }
                }
                return valor;
            }
        }

        public int IdFiltro { get; set; } = 0;

        //public JDataTable()
        //{
        //    recordsTotal = "0";
        //    recordsFiltered = "0";
        //    length = 10;
        //    start = 0;
        //    data = new Array[0];
        //    search = new Search();
        //    draw = 1;
        //}
    }
}
