using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Common.Helpers
{
    public  class DBHelper
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static int ExecuteNonQuery(string storedProcedureName, params SqlParameter[] arrParam)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentNullException("storedProcedureName");
            }

            int retVal = 0;
            SqlParameter firstOutputParameter = null;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Define the command
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedureName;
                    cmd.CommandTimeout = conn.ConnectionTimeout;

                    // Handle the parameters
                    if (arrParam != null)
                    {
                        foreach (SqlParameter param in arrParam)
                        {
                            cmd.Parameters.Add(param);

                            // Find the first integer out parameter
                            if (firstOutputParameter == null &&
                                    param.Direction == ParameterDirection.Output &&
                                    param.SqlDbType == SqlDbType.Int)
                                firstOutputParameter = param;
                        }
                    }

                    // Execute the stored procedure
                    cmd.ExecuteNonQuery();

                    // Return the first output parameter value
                    if (firstOutputParameter != null)
                        retVal = (int)firstOutputParameter.Value;
                }
            }
            return retVal;
        }

        public static DataTable ExecuteSqlDataAdapter(string storedProcedureName, params SqlParameter[] arrParam)
        {
            DataTable dataTable = null;


            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentNullException("storedProcedureName");
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Define the command
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedureName;
                    cmd.CommandTimeout = 90;


                    // Handle the parameters
                    if (arrParam != null)
                    {
                        foreach (SqlParameter param in arrParam)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }
                    dataTable = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }


        public static DataSet ExecuteSqlDataSet(string storedProcedureName, params SqlParameter[] arrParam)
        {
            DataSet ds = null;


            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentNullException("storedProcedureName");
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Define the command
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedureName;
                    cmd.CommandTimeout = conn.ConnectionTimeout;

                    // Handle the parameters
                    if (arrParam != null)
                    {
                        foreach (SqlParameter param in arrParam)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }
                    ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
            }
            return ds;
        }
    }
}
 
