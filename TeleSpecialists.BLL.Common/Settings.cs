using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common.Extensions;
using TeleSpecialists.BLL.Common.Helpers;

namespace TeleSpecialists.BLL.Common
{
    public class Settings
    {
        public static string DefaultTimeZone { get { return GetVal("DefaultTimeZone"); } }

        public static bool EnableJavascriptConsoleLogging { get { return GetVal("EnableJavascriptConsoleLogging") == "1" ? true : false; } }

        public static string SignalRAuthKey { get { return GetVal("SignalRAuthKey"); } }

        public static bool EnableCustomErrors { get { return GetVal("EnableCustomErrors") == "1" ? true : false; } }

        public static RPCMode RPCMode { get { return ((RPCMode)(string.IsNullOrEmpty(GetVal("RPCMode")) ? 0 : GetVal("RPCMode").ToInt())); } }

        public static string ConnectionString { get { return GetVal("TeleSpecialistsContext"); } }

        private static string GetVal(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
