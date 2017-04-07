using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Logger.Common.Base.Collections.Generic;
using Logger.Common.Base.Conversion;
using Logger.Common.Base.IO.Files;
using Logger.Common.Base.ObjectModel.Exceptions;




public static class StringExtensions
{
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

    public static bool IsCultureInfo (this string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        CultureInfo cultureInfo = StringExtensions.GetCultureInfo(str);

        return cultureInfo != null;
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

    #endregion
}
