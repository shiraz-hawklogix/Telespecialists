using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.Extensions
{
    public static class OwinContextExtensions
    {
        //public static string GetUserId(this IOwinContext ctx)
        //{
        //    var result = "-1";
        //    var claim = ctx.Authentication.User.Claims.FirstOrDefault(c => c.Type == "UserID");
        //    if (claim != null)
        //    {
        //        result = claim.Value;
        //    }
        //    return result;
        //}

        public static List<Dictionary<string, object>> SqlToList(this DbContext context, string sqlQuery, DbTransaction transaction = null, string includeColumns = "")
        {
            using (var cmd = context.Database.Connection.CreateCommand())
            {
                context.Database.CommandTimeout = 1800;

                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                if (transaction != null) cmd.Transaction = transaction;

                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.ReaderToList(includeColumns);
                }
            }
        }

        public static List<Dictionary<string, object>> ReaderToList(this DbDataReader reader, string includeColumns = "")
        {
            var expandolist = new List<Dictionary<string, object>>();
            var listColumns = new List<string>();

            // convert columns to list base on comma
            if (!string.IsNullOrEmpty(includeColumns))
                listColumns = includeColumns.Split(',').ToList();

            while (reader.Read())
            {
                IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);

                    if (listColumns.Count() == 0 || listColumns.Contains(columnName))
                    {
                        expando.Add(Convert.ToString(columnName), reader.GetValue(i));
                    }
                }

                expandolist.Add(new Dictionary<string, object>(expando));
            }

            return expandolist;
        }
    }
}