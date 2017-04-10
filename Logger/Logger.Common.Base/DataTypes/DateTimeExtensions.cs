using System;
using System.Globalization;
using System.Text;




namespace Logger.Common.Base.DataTypes
{
    public static class DateTimeExtensions
    {
        #region Static Methods

        public static bool IsMax (this DateTime dateTime)
        {
            return dateTime.Ticks == DateTime.MaxValue.Ticks;
        }

        public static bool IsMin (this DateTime dateTime)
        {
            return dateTime.Ticks == DateTime.MinValue.Ticks;
        }

        public static string ToTechnical (this DateTime dateTime)
        {
            return dateTime.ToTechnical(string.Empty);
        }

        public static string ToTechnical (this DateTime dateTime, char separator)
        {
            return dateTime.ToTechnical(new string(separator, 1));
        }

        public static string ToTechnical (this DateTime dateTime, string separator)
        {
            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            StringBuilder dateTimeString = new StringBuilder();

            dateTimeString.Append(dateTime.ToString("yyyy", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("MM", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("dd", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("HH", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("mm", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("ss", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(dateTime.ToString("fff", CultureInfo.InvariantCulture));

            return dateTimeString.ToString();
        }

        #endregion
    }
}
