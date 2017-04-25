using System;
using System.Globalization;
using System.Text;




namespace Logger.Common.DataTypes
{
    public static class TimeSpanExtensions
    {
        #region Static Methods

        public static bool IsMax (this TimeSpan timeSpan)
        {
            return timeSpan.Ticks == TimeSpan.MaxValue.Ticks;
        }

        public static bool IsMin (this TimeSpan timeSpan)
        {
            return timeSpan.Ticks == TimeSpan.MinValue.Ticks;
        }

        public static bool IsNegative (this TimeSpan timeSpan)
        {
            return timeSpan.Ticks < 0;
        }

        public static bool IsPositive (this TimeSpan timeSpan)
        {
            return timeSpan.Ticks >= 0;
        }

        public static bool IsZero (this TimeSpan timeSpan)
        {
            return timeSpan.Ticks == 0;
        }

        public static string ToTechnical (this TimeSpan timeSpan)
        {
            return timeSpan.ToTechnical(string.Empty);
        }

        public static string ToTechnical (this TimeSpan timeSpan, char separator)
        {
            return timeSpan.ToTechnical(new string(separator, 1));
        }

        public static string ToTechnical (this TimeSpan timeSpan, string separator)
        {
            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            StringBuilder dateTimeString = new StringBuilder();

            dateTimeString.Append(timeSpan.ToString("%d", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(timeSpan.ToString("hh", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(timeSpan.ToString("mm", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(timeSpan.ToString("ss", CultureInfo.InvariantCulture));
            dateTimeString.Append(separator);
            dateTimeString.Append(timeSpan.ToString("fff", CultureInfo.InvariantCulture));

            return dateTimeString.ToString();
        }

        #endregion
    }
}
