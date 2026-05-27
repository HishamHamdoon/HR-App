using System.Globalization;

namespace EMP.Web.Utility
{
    /// <summary>Formats Gregorian dates as Hijri (Umm al-Qura) for display.</summary>
    public static class HijriDateHelper
    {
        private static readonly UmAlQuraCalendar Calendar = new();

        private static readonly string[] Months =
        {
            "محرم", "صفر", "ربيع الأول", "ربيع الآخر", "جمادى الأولى", "جمادى الآخرة",
            "رجب", "شعبان", "رمضان", "شوال", "ذو القعدة", "ذو الحجة"
        };

        /// <summary>e.g. "12 رمضان 1447 هـ". Returns empty string for out-of-range dates.</summary>
        public static string ToHijri(DateTime date)
        {
            try
            {
                var y = Calendar.GetYear(date);
                var m = Calendar.GetMonth(date);
                var d = Calendar.GetDayOfMonth(date);
                return $"{d} {Months[m - 1]} {y} هـ";
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Today => ToHijri(DateTime.Now);
    }
}
