using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Common.Extensions
{
    public static class AttributeExtension
    {
        public static DateTime ToEST(this DateTime dateTime)
        {
            ///TODO: return dateTime.ToTimezoneFromUtc("Eastern Standard Time");

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime.ToUniversalTime(), easternZone);
        }
        public static int ToInt(this object obj)
        {
            try
            {
                if (obj != null)
                {
                    return Convert.ToInt32(obj);
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static string ToDescription(this Enum value)
        {
            var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }
    }
}
