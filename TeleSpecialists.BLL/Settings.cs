using System;
using System.Configuration;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL
{
    public  class Settings
    {
        public static string DefaultTimeZone { get { return GetVal("DefaultTimeZone"); } }

        public static bool EnableJavascriptConsoleLogging { get { return GetVal("EnableJavascriptConsoleLogging") == "1" ? true : false; } }

        public static string SignalRAuthKey { get { return GetVal("SignalRAuthKey"); } }

        public static bool EnableCustomErrors { get { return GetVal("EnableCustomErrors") == "1" ? true : false; } }

        public static RPCMode  RPCMode { get { return ((RPCMode)(string.IsNullOrEmpty(GetVal("RPCMode")) ? 0 : GetVal("RPCMode").ToInt())); } }

        private static string GetVal(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

       
    }
}