using System;
using System.Globalization;
using System.IO;
using System.Text;

using Logger.Common.Base.IO.Text;
using Logger.Common.Base.Properties;




namespace Logger.Common.Base.DataTypes
{
    public static class ExceptionExtensions
    {
        #region Constants

        private const string DefaultIndent = " ";

        private const string NullString = "[null]";

        private const string StackTracePrefix = "-> ";

        private const string TargetSiteSeparator = ".";

        #endregion




        #region Static Methods

        public static string ToDetailedString (this Exception exception, char indentChar)
        {
            return exception.ToDetailedString(new string(indentChar, 1));
        }

        public static string ToDetailedString (this Exception exception)
        {
            return exception.ToDetailedString(null);
        }

        public static string ToDetailedString (this Exception exception, string indentString)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            indentString = indentString ?? ExceptionExtensions.DefaultIndent;

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (IndentedTextWriter writer = new IndentedTextWriter(stringWriter))
                {
                    writer.IndentEmptyLines = false;
                    writer.IndentLevel = 0;
                    writer.IndentString = indentString;

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_Message);
                    writer.WriteLine(exception.Message.Trim());

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_Type);
                    writer.WriteLine(exception.GetType().AssemblyQualifiedName);

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_Source);
                    writer.WriteLine(exception.Source == null ? ExceptionExtensions.NullString : exception.Source.Trim());

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_TargetSite);
                    if (exception.TargetSite == null)
                    {
                        writer.WriteLine(ExceptionExtensions.NullString);
                    }
                    else
                    {
                        writer.Write(exception.TargetSite.DeclaringType == null ? ExceptionExtensions.NullString : exception.TargetSite.DeclaringType.AssemblyQualifiedName.Trim());
                        writer.Write(ExceptionExtensions.TargetSiteSeparator);
                        writer.WriteLine(exception.TargetSite.Name.Trim());
                    }

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_HelpLink);
                    writer.WriteLine(exception.HelpLink == null ? ExceptionExtensions.NullString : exception.HelpLink.Trim());

                    writer.Write(Properties.Resources.ExceptionExtensions_DetailedException_StackTrace);
                    if (exception.StackTrace == null)
                    {
                        writer.WriteLine(ExceptionExtensions.NullString);
                    }
                    else
                    {
                        string[] lines = exception.StackTrace.SplitLineBreaks(StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 0)
                        {
                            writer.WriteLine(ExceptionExtensions.NullString);
                        }
                        else
                        {
                            writer.WriteLine();
                            for (int i1 = 0; i1 < lines.Length; i1++)
                            {
                                string line = lines[i1];
                                writer.Write(ExceptionExtensions.StackTracePrefix);
                                writer.WriteLine(line.Trim());
                            }
                        }
                    }

                    if (exception.InnerException != null)
                    {
                        writer.WriteLine(Properties.Resources.ExceptionExtensions_DetailedException_InnerException);
                        writer.IndentLevel++;
                        writer.WriteLine(exception.InnerException.ToDetailedString(indentString));
                        writer.IndentLevel--;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        #endregion
    }
}
