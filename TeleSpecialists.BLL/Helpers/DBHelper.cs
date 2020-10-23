using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
namespace TeleSpecialists.BLL.Helpers
{

    public static class DBHelper
    {
        [DbFunction("DataModel.Store", "GetUserFullName")]
        public static string GetUserFullName(string @userId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "GetRoleName")]
        public static string GetRoleName(string @roleId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "FormatSeconds")]
        public static string FormatSeconds(long? @timeInSeconds)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
        [DbFunction("DataModel.Store", "FormatSeconds_v2")]
        public static string FormatSeconds(DateTime? @date1, DateTime? @date2)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
        [DbFunction("DataModel.Store", "DiffSeconds")]
        public static int DiffSeconds(DateTime? @date1, DateTime? @date2)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
        [DbFunction("DataModel.Store", "FormatDateTime")]
        public static string FormatDateTime(DateTime @dt, bool @bIncludeTime)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "FormatDateTime")]
        public static string FormatDateTime(DateTime? @dt, bool @bIncludeTime)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "GetPhysiciansInitialsCount")]
        public static string GetPhysiciansInitialsCount(string @physIntials)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "GetInitials")]
        public static string GetInitials(string @inputval)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        [DbFunction("DataModel.Store", "UDF_ConvertUtcToLocalByTimezoneIdentifier")]
        public static DateTime ConvertToFacilityTimeZone(string @TargetTimezoneIdentifier, DateTime? @UtcDate)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        public static int ExecuteNonQuery(string storedProcedureName, params SqlParameter[] arrParam)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentNullException("storedProcedureName");
            }

            int retVal = 0;
            SqlParameter firstOutputParameter = null;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
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

        [DbFunction("DataModel.Store", "CheckPhysBlastOnShift")]
        public static string CheckPhysBlastOnShift(DateTime @BlastDateTime, string @PhyId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        
        public static void UpdateSelectedColumns(string KeyName, string Val, string EntityName, Dictionary<string, string> dict)
        {
            var query = GetUpdateQuery(KeyName, Val, EntityName, dict);
            if (!string.IsNullOrEmpty(query))
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();

                    // Define the command
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Parameters.AddWithValue(KeyName, Val);
                        foreach (var item in dict)
                        {
                            if (item.Value == null || item.Value == DateTime.MinValue.ToString())
                            {
                                cmd.Parameters.AddWithValue(item.Key, DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                            }
                            
                        }

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query;
                        cmd.CommandTimeout = conn.ConnectionTimeout;

                        // Execute the stored procedure
                        cmd.ExecuteNonQuery();
                        // Return the first output parameter value


                    }
                }
            }

        }

        public static void InsertSelectedColumns(string EntityName, Dictionary<string, string> dict)
        {
            var query = GetInsertQuery(EntityName, dict);
            if (!string.IsNullOrEmpty(query))
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();

                    // Define the command
                    using (SqlCommand cmd = new SqlCommand())
                    {                       
                        foreach (var item in dict)
                        {
                            cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query;
                        cmd.CommandTimeout = conn.ConnectionTimeout;

                        // Execute the stored procedure
                        cmd.ExecuteNonQuery();
                        // Return the first output parameter value


                    }
                }
            }

        }

        private static string GetUpdateQuery(string KeyName, string Val, string EntityName, Dictionary<string, string> dict)
        {
            string query = "", columns = "";
            int count = dict.Count() - 1;
            int current = 0;

            foreach (var item in dict)
            {

                if (current < count)
                {
                    columns += string.Format(" {0} = @{0},", item.Key);
                }
                else
                {
                    columns += string.Format(" {0} = @{0}", item.Key);
                }
                current++;
            }

            if (dict.Count() > 0)
            {
                query = string.Format("UPDATE {0} SET {1} WHERE {2} = @{2}", EntityName, columns, KeyName);
                return query;

            }

            return "";
        }

        private static string GetInsertQuery( string EntityName, Dictionary<string, string> dict)
        {
            string query = "", columns = "";
            int count = dict.Count() - 1;
            int current = 0;

            foreach (var item in dict)
            {

                if (current < count)
                {
                    columns += string.Format("@{0},", item.Key);
                }
                else
                {
                    columns += string.Format("@{0}", item.Key);
                }
                current++;
            }

            if (dict.Count() > 0)
            {               
                query = string.Format("INSERT INTO {0}({1}) Values({2})", EntityName, columns.Replace("@",""), columns);
                return query;

            }

            return "";
        }


    }
}
