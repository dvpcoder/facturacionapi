using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace bflex.facturacion.DataAccess
{
    public class DatabaseHelper : IDisposable
    {
        private string strConnectionString;
        private DbConnection objConnection;
        private DbCommand objCommand;
        private DbProviderFactory objFactory = null;
        private bool boolHandleErrors;
        private string strLastError;
        private bool boolLogError;
        private string strLogFile;
        private DbTransaction objTransaction; //Declarar variable tipo transaccion

        public DatabaseHelper(string connectionstring, Providers provider)
        {
            strConnectionString = connectionstring;
            switch (provider)
            {
                case Providers.SqlServer:
                    objFactory = SqlClientFactory.Instance;
                    break;
                case Providers.OleDb:
                    objFactory = OleDbFactory.Instance;
                    break;
                //case Providers.Oracle:
                //    objFactory = OracleClientFactory.Instance;
                //    break;
                case Providers.ODBC:
                    objFactory = OdbcFactory.Instance;
                    break;
                case Providers.ConfigDefined:
                    string providername = ConfigurationManager.ConnectionStrings["connectionstring"].ProviderName;
                    switch (providername)
                    {
                        case "System.Data.SqlClient":
                            objFactory = SqlClientFactory.Instance;
                            break;
                        case "System.Data.OleDb":
                            objFactory = OleDbFactory.Instance;
                            break;
                        //case "System.Data.OracleClient":
                        //    objFactory = OracleClientFactory.Instance;
                        //    break;
                        case "System.Data.Odbc":
                            objFactory = OdbcFactory.Instance;
                            break;
                    }
                    break;

            }
            objConnection = objFactory.CreateConnection();
            objCommand = objFactory.CreateCommand();

            objConnection.ConnectionString = strConnectionString;
            objCommand.Connection = objConnection;
        }

        public DatabaseHelper(Providers provider)
            : this(ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString, provider)
        {
        }

        public DatabaseHelper(string connectionstring)
            : this(connectionstring, Providers.SqlServer)
        {
        }

        public DatabaseHelper()
            : this(ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString, Providers.ConfigDefined)
        {
        }

        public bool HandleErrors
        {
            get
            {
                return boolHandleErrors;
            }
            set
            {
                boolHandleErrors = value;
            }
        }

        public string LastError
        {
            get
            {
                return strLastError;
            }
        }

        public bool LogErrors
        {
            get
            {
                return boolLogError;
            }
            set
            {
                boolLogError = value;
            }
        }

        public string LogFile
        {
            get
            {
                return strLogFile;
            }
            set
            {
                strLogFile = value;
            }
        }

        public int AddParameter(string name, object value)
        {
            DbParameter p = objFactory.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            return objCommand.Parameters.Add(p);
        }

        public int AddParameter(DbParameter parameter)
        {
            return objCommand.Parameters.Add(parameter);
        }

        public DbCommand Command
        {
            get
            {
                return objCommand;
            }
        }

        public async Task BeginTransaction()
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                await objConnection.OpenAsync();
            }
            objTransaction = objConnection.BeginTransaction();
            objCommand.Transaction = objTransaction;
        }

        public void CommitTransaction()
        {
            //objCommand.Transaction.Commit();
            objTransaction.Commit(); // poner esta linea
            objConnection.Close();
        }

        public void RollbackTransaction()
        {
            objCommand.Transaction.Rollback();
            objConnection.Close();
        }

        //public int ExecuteNonQuery(string query)
        //{
        //    return ExecuteNonQuery(query, CommandType.Text, ConnectionState.CloseOnExit);
        //}

        public async Task<int> ExecuteNonQuery(string query, CommandType commandtype)
        {
            return await ExecuteNonQuery(query, commandtype, ConnectionState.CloseOnExit);
        }

        //public int ExecuteNonQuery(string query, ConnectionState connectionstate)
        //{
        //    return ExecuteNonQuery(query, CommandType.Text, connectionstate);
        //}

        public async Task<int> ExecuteNonQuery(string query, CommandType commandtype, ConnectionState connectionstate)
        {
            objCommand.CommandText = query;
            objCommand.CommandType = commandtype;
            int i = -1;
            try
            {
                if (objTransaction != null && objCommand.Transaction == null) //poner esta instruccion
                {
                    objCommand.Transaction = objTransaction;
                }

                if (objConnection.State == System.Data.ConnectionState.Closed)
                {
                    await objConnection.OpenAsync();
                }
                i = await objCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
                //HandleExceptions(ex);
            }
            finally
            {
                objCommand.Parameters.Clear();
                if (connectionstate == ConnectionState.CloseOnExit)
                {
                    if (objTransaction == null) // poner esta instruccion
                    {
                        objConnection.Close();
                    }
                }
            }

            return i;
        }

        //public object ExecuteScalar(string query)
        //{
        //    return ExecuteScalar(query, CommandType.Text, ConnectionState.CloseOnExit);
        //}

        public async Task<object> ExecuteScalar(string query, CommandType commandtype)
        {
            return await ExecuteScalar(query, commandtype, ConnectionState.CloseOnExit);
        }

        //public object ExecuteScalar(string query, ConnectionState connectionstate)
        //{
        //    return ExecuteScalar(query, CommandType.Text, connectionstate);
        //}

        public async Task<object> ExecuteScalar(string query, CommandType commandtype, ConnectionState connectionstate)
        {
            objCommand.CommandText = query;
            objCommand.CommandType = commandtype;
            objCommand.CommandTimeout = 500000;
            object o = null;

            try
            {
                if (objConnection.State == System.Data.ConnectionState.Closed)
                {
                    await objConnection.OpenAsync();
                }
                o = await objCommand.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                //HandleExceptions(ex);
                throw ex;
            }
            finally
            {
                objCommand.Parameters.Clear();
                if (connectionstate == ConnectionState.CloseOnExit)
                {
                    objConnection.Close();
                }
            }

            return o;
        }

        //public DbDataReader ExecuteReader(string query)
        //{
        //    return ExecuteReader(query, CommandType.Text, ConnectionState.CloseOnExit);
        //}

        public async Task<DbDataReader> ExecuteReader(string query, CommandType commandtype)
        {
            return await ExecuteReader(query, commandtype, ConnectionState.CloseOnExit);
        }

        //public DbDataReader ExecuteReader(string query, ConnectionState connectionstate)
        //{
        //    return ExecuteReader(query, CommandType.Text, connectionstate);
        //}

        public async Task<DbDataReader> ExecuteReader(string query, CommandType commandtype, ConnectionState connectionstate)
        {
            objCommand.CommandText = query;
            objCommand.CommandType = commandtype;
            objCommand.CommandTimeout = 500000;
            DbDataReader reader = null;
            try
            {
                if (objConnection.State == System.Data.ConnectionState.Closed)
                {
                    await objConnection.OpenAsync();
                }
                if (connectionstate == ConnectionState.CloseOnExit)
                {
                    reader = await objCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                    //reader = objCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = await objCommand.ExecuteReaderAsync();
                    //reader = objCommand.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //HandleExceptions(ex);
            }
            finally
            {
                objCommand.Parameters.Clear();
            }

            return reader;
        }

        public DataSet ExecuteDataSet(string query)
        {
            return ExecuteDataSet(query, CommandType.Text, ConnectionState.CloseOnExit);
        }

        public DataSet ExecuteDataSet(string query, CommandType commandtype)
        {
            return ExecuteDataSet(query, commandtype, ConnectionState.CloseOnExit);
        }

        public DataSet ExecuteDataSet(string query, ConnectionState connectionstate)
        {
            return ExecuteDataSet(query, CommandType.Text, connectionstate);
        }

        public DataSet ExecuteDataSet(string query, CommandType commandtype, ConnectionState connectionstate)
        {
            DbDataAdapter adapter = objFactory.CreateDataAdapter();
            objCommand.CommandText = query;
            objCommand.CommandType = commandtype;
            objCommand.CommandTimeout = 500000;
            adapter.SelectCommand = objCommand;
            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
            finally
            {
                objCommand.Parameters.Clear();
                if (connectionstate == ConnectionState.CloseOnExit)
                {
                    if (objConnection.State == System.Data.ConnectionState.Open)
                    {
                        objConnection.Close();
                    }
                }
            }
            return ds;
        }

        private void HandleExceptions(Exception ex)
        {
            if (LogErrors)
            {
                WriteToLog(ex.Message);
            }
            if (HandleErrors)
            {
                strLastError = ex.Message;
            }
            else
            {
                throw ex;
            }
        }

        private void WriteToLog(string msg)
        {
            StreamWriter writer = File.AppendText(LogFile);
            writer.WriteLine(DateTime.Now.ToString() + " - " + msg);
            writer.Close();
        }

        public void Dispose()
        {
            objConnection.Close();
            objConnection.Dispose();
            objCommand.Dispose();
        }

    }

    public enum Providers
    {
        SqlServer, OleDb, Oracle, ODBC, ConfigDefined
    }

    public enum ConnectionState
    {
        KeepOpen, CloseOnExit
    }
}