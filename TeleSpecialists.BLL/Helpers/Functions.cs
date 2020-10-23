using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Extensions;
using TimeZoneNames;

namespace TeleSpecialists.BLL.Helpers
{
    public class Functions
    {
        public static string ClearPhoneFormat(string phone)
        {
            if (!string.IsNullOrEmpty(phone))
            {
                phone = phone.Replace("(", string.Empty)
                             .Replace(")", string.Empty)
                             .Replace(" ", string.Empty)
                             .Replace("-", string.Empty);

            }
            return phone;
        }

        public static DateTime? TimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            try {
                //DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
                //return dtDateTime;

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return origin.AddSeconds(unixTimeStamp / 1000); // convert from milliseconds to seconds
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static string GetRandomKey()
        {
            var keyString = "abcdefghijklmnopqrstuvwxyz!@#$_1234567890".ToCharArray();
            int N = keyString.Length;
            Random rd = new Random();
            int iLength = 20;
            StringBuilder sb = new StringBuilder(iLength);
            for (int i = 0; i < iLength; i++)
            {
                sb.Append(keyString.ElementAt(rd.Next(N)));
            }
            return sb.ToString();
        }

        public static string EncodeTo64UTF8(string m_enc)
        {
            byte[] toEncodeAsBytes =
            System.Text.ASCIIEncoding.UTF8.GetBytes(m_enc);
            string returnValue =
            System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string DecodeFrom64(string m_enc)
        {
            if (!string.IsNullOrEmpty(m_enc?.Trim()))
            {
                byte[] encodedDataAsBytes =
                System.Convert.FromBase64String(m_enc);
                string returnValue =
                System.Text.ASCIIEncoding.UTF8.GetString(encodedDataAsBytes);
                return returnValue;
            }
            else return string.Empty;
        }


        public static double GetTimeZoneOffset(string timeZone)
        {
            var objTimeZone = @TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            if (objTimeZone.IsDaylightSavingTime(DateTime.Now))
            {
                return (objTimeZone.BaseUtcOffset.TotalHours + 1.0);
            }
            return objTimeZone.BaseUtcOffset.TotalHours;
        }

        public static string GetSubtractedDateFormated(DateTime? dtStartTime, DateTime? dtEndTime)
        {
            TimeSpan? result = null;
            if (dtStartTime.HasValue && dtEndTime.HasValue)
            {
                result = dtEndTime.Value - dtStartTime.Value;
            }
            return result.FormatTimeSpan();
        }

        public static string ReadFile(string Path)
        {
            string OutPut = "";
            FileInfo inf = new FileInfo(Path);
            if (inf.Exists)
            {
                using (StreamReader reader = new StreamReader(Path))
                {
                    OutPut = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }
            return OutPut;
        }


        public static string FormatAsPhoneNumber(string PhoneNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(PhoneNumber))
                {
                    return Regex.Replace(PhoneNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3");
                }

                return PhoneNumber;
            }
            catch (Exception )
            {
                return PhoneNumber;
            }
        }

        public static string ConvertToJson(Type e)
        {
            var dict = new Dictionary<string, int>();
            foreach (var val in Enum.GetValues(e))
            {

                var name = Enum.GetName(e, val);
                dict.Add(name, val.ToInt());

            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict,Newtonsoft.Json.Formatting.Indented);
            return json;

        }
        public static string GetTimeZoneAbbreviation(string timeZoneString)
        {
            TimeZoneInfo theTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneString);

            string tzid = theTimeZoneInfo.Id;                // example: "Eastern Standard time"
            string lang = CultureInfo.CurrentCulture.Name;   // example: "en-US"
            var abbreviations = TZNames.GetNamesForTimeZone(tzid, lang);
            return abbreviations.Standard.GetAbbreviation();
        }
        public static string ConvertToFacilityTimeZone(DateTime? dt, string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
            {
                timeZone = BLL.Settings.DefaultTimeZone;
            }

            if (dt.HasValue)
                return dt.Value.ToTimezoneFromUtc(timeZone).FormatDateTime();

            return "";
        }

        public static void GenerateDocument(string Html, string FilePath)
        {
            Document doc = new Document();
            doc.HtmlExportOptions.ImageEmbedded = true;          
            using (TextReader sr = new StringReader(Html))
            {
                doc.LoadHTML(sr, Spire.Doc.Documents.XHTMLValidationType.None);
            }
            doc.SaveToFile(FilePath, FileFormat.Docx);        
        }
    }
}
