﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Extensions
{

    public static class AttributesHelperExtension
    {
        /// <summary>
        /// Responsible for returning Description attribute as string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum value)
        {
            var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }
         
        public static string FormatDateTime(this DateTime value)
        {
            return value.ToString("MM/dd/yyyy HH:mm:ss");
        }
        public static string FormatDate(this DateTime value)
        {
            return value.ToString("MM/dd/yyyy");
        }
        public static string FormatTime(this DateTime value)
        {
            return value.ToString("HH:mm");
        }
        public static DateTime ToUniversalTimeZone(this DateTime dateTime, string currentTimeZone = "Eastern Standard Time")
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, easternZone);
        }
        public static DateTime ToEST(this DateTime dateTime)
        {
            ///TODO: return dateTime.ToTimezoneFromUtc("Eastern Standard Time");

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime.ToUniversalTime(), easternZone);
        }

        //public static DateTime ToFacilityTimezone(this DateTime dateTime, string timeZone)
        //{
        //    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        //    return TimeZoneInfo.ConvertTime(dateTime.ToUniversalTime(), timeZoneInfo);
        //}

        public static DateTime ToTimezoneFromUtc(this DateTime dateTime, string timeZone)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
        }

        public static DateTime ToLocalTimezone(this DateTime dateTime)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
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
        public static long ToLong(this object obj)
        {
            try
            {
                if (obj != null)
                {
                    return Convert.ToInt64(obj);
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static DateTime? ToDateTime(this string obj)
        {
            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(obj, out dt);

            if (dt == DateTime.MinValue)
                return null;

            return dt;
        }

        public static string FormatTimeSpan(this TimeSpan? tSpan)
        {
            var formatedSpan = "00:00:00";
            if (tSpan.HasValue)
            {
                tSpan = tSpan.Value.Duration();
                var days = tSpan.Value.Days * 24;

                formatedSpan = (string.Format("{0:00}:{1:00}:{2:00}", (tSpan.Value.Hours + days), tSpan.Value.Minutes, tSpan.Value.Seconds));
                //formatedSpan = (tSpan.Value.Hours + days) + ":" + tSpan.Value.Minutes + ":" + tSpan.Value.Seconds;
            }
            return formatedSpan;
        }


        public static decimal ToDecimal(this object obj)
        {
            try
            {
                if (obj != null)
                {
                    return Convert.ToDecimal(obj);
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static double ToDouble(this object obj)
        {
            try
            {
                if (obj != null)
                {
                    return Convert.ToDouble(obj);
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }



        public static string ToYesNo(this bool obj)
        {
            return obj == true ? "Yes" : "No";
        }
        public static bool ToBool(this object obj)
        {
            try
            {
                if (obj != null)
                {
                    if (obj.ToString() == "1") return true;
                    if (obj.ToString() == "0") return false;
                    if (obj.ToString().ToLower() == "true") return true;
                    if (obj.ToString().ToLower() == "false") return false;
                    if (obj.ToString().ToLower() == "yes") return true;
                    if (obj.ToString().ToLower() == "no") return false;

                    return Convert.ToBoolean(obj);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetAbbreviation(this string inputString)
        {

            var array_string = inputString.Split(' ');
            var result = "";
            foreach (var subStr in array_string)
            {
                result += subStr.First().ToString().ToUpper();
            }
            return result;
        }
    }
}
