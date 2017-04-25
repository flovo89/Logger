using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Logger.Common.Collections.Generic;
using Logger.Common.Conversion;
using Logger.Common.IO.Files;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.DataTypes
{
    public static class StringExtensions
    {
        #region Constants

        private static readonly string[] BooleanFalseValues =
        {
            "false", "no", "0", "off", "none", "nothing", "null", "[null]", "nein", "non", "f", "n"
        };

        private static readonly string[] BooleanTrueValues =
        {
            "true", "yes", "1", "on", "ja", "oui", "si", "t", "y", "j"
        };

        #endregion




        #region Static Properties/Indexer

        private static Dictionary<string, Encoding> BodyNameEncodings { get; set; }

        private static Dictionary<string, Encoding> CodePageEncodings { get; set; }

        private static CultureInfo[] Cultures { get; set; }

        private static Dictionary<string, Encoding> DisplayNameEncodings { get; set; }

        private static EncodingInfo[] Encodings { get; set; }

        private static Dictionary<string, CultureInfo> FourLetterCultures { get; set; }

        private static Dictionary<string, Encoding> HeaderNameEncodings { get; set; }

        private static Dictionary<string, CultureInfo> LcidCultures { get; set; }

        private static Dictionary<string, CultureInfo> NameCultures { get; set; }

        private static Dictionary<string, Encoding> NameEncodings { get; set; }

        private static Dictionary<string, CultureInfo> ThreeLetterIsoLanguageNameCultures { get; set; }

        private static Dictionary<string, CultureInfo> ThreeLetterWindowsLanguageNameCultures { get; set; }

        private static Dictionary<string, CultureInfo> TwoLetterIsoLanguageNameCultures { get; set; }

        private static Dictionary<string, Encoding> WebNameEncodings { get; set; }

        private static Dictionary<string, Encoding> WindowsCodePageEncodings { get; set; }

        #endregion




        #region Static Methods

        public static bool IsBoolean (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            bool? value = StringExtensions.GetBoolean(str);

            return value != null;
        }

        public static bool IsCultureInfo (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            CultureInfo cultureInfo = StringExtensions.GetCultureInfo(str);

            return cultureInfo != null;
        }

        public static bool IsDirectoryPath (this string str, bool allowWildcards)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            DirectoryPath path = StringExtensions.GetDirectoryPath(str, allowWildcards);

            return path != null;
        }

        public static bool IsDirectoryPath (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            DirectoryPath path = StringExtensions.GetDirectoryPath(str, true);

            return path != null;
        }

        public static bool IsEmpty (this string str)
        {
            return str.IsEmpty(false);
        }

        public static bool IsEmpty (this string str, bool doNotTrimBeforeCheck)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            str = doNotTrimBeforeCheck ? str : str.Trim();

            return string.IsNullOrEmpty(str);
        }

        public static bool IsEncoding (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Encoding encoding = StringExtensions.GetEncoding(str);

            return encoding != null;
        }

        public static bool IsEnumeration (this string str, Type enumType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            object value = StringExtensions.GetEnumeration(str, enumType);

            return value != null;
        }

        public static string Join (this IEnumerable<string> str)
        {
            return str.Join(string.Empty, 0, -1);
        }

        public static string Join (this IEnumerable<string> str, string separator)
        {
            return str.Join(separator, 0, -1);
        }

        public static string Join (this IEnumerable<string> str, char separator)
        {
            return str.Join(new string(separator, 1), 0, -1);
        }

        public static string Join (this IEnumerable<string> str, int startIndex, int count)
        {
            return str.Join(string.Empty, startIndex, count);
        }

        public static string Join (this IEnumerable<string> str, string separator, int startIndex, int count)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            List<string> strList = str.ToList();
            strList.RemoveWhere(x => x == null);

            return string.Join(separator, strList.ToArray(), startIndex, count == -1 ? strList.Count : count);
        }

        public static string Join (this IEnumerable<string> str, char separator, int startIndex, int count)
        {
            return str.Join(new string(separator, 1), startIndex, count);
        }

        public static string Keep (this string str, Predicate<char> predicate)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            StringBuilder sb = new StringBuilder();

            foreach (char c in str)
            {
                if (predicate(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string RemoveLineBreaks (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        public static string[] Split (this string str, char separator)
        {
            return str.Split(separator, StringSplitOptions.None);
        }

        public static string[] Split (this string str, char separator, StringSplitOptions options)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Split(new[]
            {
                separator
            }, options);
        }

        public static string[] SplitLineBreaks (this string str)
        {
            return str.SplitLineBreaks(StringSplitOptions.None);
        }

        public static string[] SplitLineBreaks (this string str, StringSplitOptions options)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Replace("\r", string.Empty).Split('\n', options);
        }

        public static int StartsWithCount (this string str, char token, StringComparison comparisonOptions)
        {
            return str.StartsWithCount(new string(token, 1), comparisonOptions);
        }

        public static int StartsWithCount (this string str, string token, StringComparison comparisonOptions)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token.IsEmpty(true))
            {
                throw new EmptyStringArgumentException(nameof(token));
            }

            int count = 0;
            int index = 0;
            while (true)
            {
                if (index + token.Length > str.Length)
                {
                    break;
                }

                string comparedPiece = str.Substring(index, token.Length);

                if (string.Equals(token, comparedPiece, comparisonOptions))
                {
                    count++;
                    index += token.Length;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        public static bool ToBoolean (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            bool? value = StringExtensions.GetBoolean(str);

            if (value == null)
            {
                throw new ConversionNotPossibleException(typeof(string), typeof(bool));
            }

            return value.Value;
        }

        public static CultureInfo ToCultureInfo (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            CultureInfo cultureInfo = StringExtensions.GetCultureInfo(str);

            if (cultureInfo == null)
            {
                throw new ConversionNotPossibleException(typeof(string), typeof(CultureInfo));
            }

            cultureInfo = new CultureInfo(cultureInfo.ToString());

            return cultureInfo;
        }

        public static DirectoryPath ToDirectoryPath (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            DirectoryPath path = StringExtensions.GetDirectoryPath(str, true);

            if (path == null)
            {
                throw new ConversionNotPossibleException(typeof(string), typeof(DirectoryPath));
            }

            return path;
        }

        public static Encoding ToEncoding (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Encoding encoding = StringExtensions.GetEncoding(str);

            if (encoding == null)
            {
                throw new ConversionNotPossibleException(typeof(string), typeof(Encoding));
            }

            return encoding;
        }

        public static object ToEnumeration (this string str, Type enumType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            object value = StringExtensions.GetEnumeration(str, enumType);

            if (value == null)
            {
                throw new ConversionNotPossibleException(typeof(string), enumType);
            }

            return value;
        }

        public static Guid ToGuid (this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Guid? value = StringExtensions.GetGuid(str);

            if (value == null)
            {
                throw new ConversionNotPossibleException(typeof(string), typeof(Guid));
            }

            return value.Value;
        }

        public static string ToPathCompatible (this string str)
        {
            return str.ToPathCompatible(false, null);
        }

        public static string ToPathCompatible (this string str, bool allowWhitespaces)
        {
            return str.ToPathCompatible(allowWhitespaces, null);
        }

        public static string ToPathCompatible (this string str, bool allowWhitespaces, char? whitespaceReplacement)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            HashSet<char> invalidCharacters = new HashSet<char>();
            invalidCharacters.AddRange(Path.GetInvalidFileNameChars());
            invalidCharacters.AddRange(Path.GetInvalidPathChars());
            invalidCharacters.Add(Path.DirectorySeparatorChar);
            invalidCharacters.Add(Path.AltDirectorySeparatorChar);
            invalidCharacters.Add(Path.PathSeparator);
            invalidCharacters.Add(Path.VolumeSeparatorChar);

            StringBuilder strBuilder = new StringBuilder();

            for (int i1 = 0; i1 < str.Length; i1++)
            {
                if (!allowWhitespaces && char.IsWhiteSpace(str[i1]))
                {
                    continue;
                }

                if (invalidCharacters.Contains(str[i1]))
                {
                    continue;
                }

                strBuilder.Append(str[i1]);
            }

            return strBuilder.ToString();
        }

        private static bool? GetBoolean (string str)
        {
            str = str.ToLowerInvariant().Trim();

            bool value = false;

            if (bool.TryParse(str, out value))
            {
                return value;
            }

            foreach (string booleanFalseValue in StringExtensions.BooleanFalseValues)
            {
                if (string.Equals(booleanFalseValue, str, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            foreach (string booleanTrueValue in StringExtensions.BooleanTrueValues)
            {
                if (string.Equals(booleanTrueValue, str, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return null;
        }

        private static CultureInfo GetCultureInfo (string str)
        {
            str = str.Trim();
            CultureInfo result = null;

            if (str.IsEmpty())
            {
                return null;
            }

            if (StringExtensions.FourLetterCultures.ContainsKey(str))
            {
                result = StringExtensions.FourLetterCultures[str];
            }
            else if (StringExtensions.LcidCultures.ContainsKey(str))
            {
                result = StringExtensions.LcidCultures[str];
            }
            else if (StringExtensions.NameCultures.ContainsKey(str))
            {
                result = StringExtensions.NameCultures[str];
            }
            else if (StringExtensions.ThreeLetterIsoLanguageNameCultures.ContainsKey(str))
            {
                result = StringExtensions.ThreeLetterIsoLanguageNameCultures[str];
            }
            else if (StringExtensions.ThreeLetterWindowsLanguageNameCultures.ContainsKey(str))
            {
                result = StringExtensions.ThreeLetterWindowsLanguageNameCultures[str];
            }
            else if (StringExtensions.TwoLetterIsoLanguageNameCultures.ContainsKey(str))
            {
                result = StringExtensions.TwoLetterIsoLanguageNameCultures[str];
            }
            /*else
        {
            try
            {
                result = new CultureInfo(str);
            }
            catch {}
        }*/

            return result;
        }

        private static DirectoryPath GetDirectoryPath (string str, bool allowWildcards)
        {
            if (DirectoryPath.IsDirectoryPath(str, allowWildcards))
            {
                return new DirectoryPath(str);
            }

            return null;
        }

        private static Encoding GetEncoding (string str)
        {
            str = str.Trim();
            Encoding result = null;

            if (StringExtensions.CodePageEncodings.ContainsKey(str))
            {
                result = StringExtensions.CodePageEncodings[str];
            }
            else if (StringExtensions.WindowsCodePageEncodings.ContainsKey(str))
            {
                result = StringExtensions.WindowsCodePageEncodings[str];
            }
            else if (StringExtensions.WebNameEncodings.ContainsKey(str))
            {
                result = StringExtensions.WebNameEncodings[str];
            }
            else if (StringExtensions.BodyNameEncodings.ContainsKey(str))
            {
                result = StringExtensions.BodyNameEncodings[str];
            }
            else if (StringExtensions.HeaderNameEncodings.ContainsKey(str))
            {
                result = StringExtensions.HeaderNameEncodings[str];
            }
            else if (StringExtensions.NameEncodings.ContainsKey(str))
            {
                result = StringExtensions.NameEncodings[str];
            }
            else if (StringExtensions.DisplayNameEncodings.ContainsKey(str))
            {
                result = StringExtensions.DisplayNameEncodings[str];
            }
            /*else
        {
            try
            {
                result = Encoding.GetEncoding(str);
            }
            catch {}
        }*/

            return result;
        }

        private static object GetEnumeration (string str, Type enumType)
        {
            str = str.Trim();

            if (str.IsEmpty())
            {
                return null;
            }

            try
            {
                return Enum.Parse(enumType, str, true);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static Guid? GetGuid (string str)
        {
            str = str.ToUpperInvariant().Trim();

            Guid value;
            if (Guid.TryParse(str, out value))
            {
                return value;
            }

            return null;
        }

        #endregion
    }
}
