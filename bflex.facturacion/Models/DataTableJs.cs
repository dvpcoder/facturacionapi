using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class DataTableJs
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public Array data { get; set; }
        public int length { get; set; }
        public int start { get; set; }
        public List<Order> order { get; set; }
        public Search search { get; set; }
        public List<Column> columns { get; set; }
        public int end { get { return start + length; } }
        public string colorden
        {
            get
            {
                int i = 0;
                string valor = "NO-ORDER";
                if (order != null)
                {
                    foreach (Column col in columns)
                    {
                        if (order.Count > 0)
                        {
                            if (i == order[0].column)
                            {
                                if (col.name == "") valor = "NO-ORDER";
                                else valor = col.name + " " + order[0].dir.ToUpper();
                            }
                        }
                        i++;
                    }
                }
                return valor;
            }
        }

        public DataTableJs()
        {
            length = 10;
            data = new Array[0];
            search = new Search();
            columns = new List<Column>();

        }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
        public string descolumorder { get { return column + " " + dir.ToUpper(); } }
    }

    //public class Search
    //{
    //    public string value { get; set; }
    //    public bool regex { get; set; }
    //}

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
        public Column()
        {
            search = new Search();
        }
    }
    
}