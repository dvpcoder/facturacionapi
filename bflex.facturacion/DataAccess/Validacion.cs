using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace bflex.facturacion.DataAccess
{
    static public class Validacion
    {
        #region Validaciones para Tipo de dato 

        public static DateTime DBToDateTime(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? DateTime.MinValue : Convert.ToDateTime(reader[ColumnName]);
        }

        public static string DBToString(ref SqlDataReader reader, string ColumnName)
        {
            return Convert.ToString(reader[ColumnName]);
        }

        public static int DBToInt32(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (Int32)0 : (Int32)reader[ColumnName];
        }

        public static int DBToSmallInt(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (Int16)0 : (Int16)reader[ColumnName];
        }

        public static int DBToTinyInt(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (Byte)0 : (Byte)reader[ColumnName];
        }

        public static Int64 DBToInt64(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (Int64)0 : (Int64)reader[ColumnName];
        }


        public static decimal DBToDecimal(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (decimal)0 : (decimal)reader[ColumnName];
        }

        public static short DBToInt16(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? (short)0 : (short)reader[ColumnName];
        }

        public static bool DBToBoolean(ref SqlDataReader reader, string ColumnName)
        {
            return (reader.IsDBNull(reader.GetOrdinal(ColumnName))) ? false : (bool)reader[ColumnName];
        }

        #endregion
    }
}