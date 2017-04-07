using System;
using System.Drawing;
using System.Reflection;

using Logger.Common.Base.IO.Files;




namespace Logger.Common.Base.Reflection
{
    public static class AssemblyExtension
    {
        #region Static Methods

        public static string GetCompany (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);

            if (attributes.Length == 0)
            {
                return null;
            }

            return ( (AssemblyCompanyAttribute)attributes[0] ).Company;
        }

        public static string GetCopyright (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);

            if (attributes.Length == 0)
            {
                return null;
            }

            return ( (AssemblyCopyrightAttribute)attributes[0] ).Copyright;
        }

        public static Icon GetDefaultIcon (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            FilePath file = assembly.GetFile();

            if (file == null)
            {
                return null;
            }

            try
            {
                return Icon.ExtractAssociatedIcon(file.Path);
            }
            catch
            {
                return null;
            }
        }

        public static FilePath GetFile (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            string location = null;

            try
            {
                location = assembly.Location;
            }
            catch
            {
                return null;
            }

            if (location.IsEmpty())
            {
                return null;
            }

            return new FilePath(location);
        }

        public static string GetProductName (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

            if (attributes.Length == 0)
            {
                return null;
            }

            return ( (AssemblyProductAttribute)attributes[0] ).Product;
        }

        #endregion
    }
}
