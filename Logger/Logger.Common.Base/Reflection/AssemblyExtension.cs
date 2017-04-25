using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Logger.Common.DataTypes;
using Logger.Common.IO.Files;




namespace Logger.Common.Reflection
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

        public static Guid? GetGuid (this Assembly assembly, AssemblyGuidFlags flags)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] attributes = assembly.GetCustomAttributes(typeof(GuidAttribute), true);

            if (( attributes.Length > 0 ) && ( ( flags & AssemblyGuidFlags.TryGuidAttribute ) == AssemblyGuidFlags.TryGuidAttribute ))
            {
                return ( (GuidAttribute)attributes[0] ).Value.ToGuid();
            }

            if (( flags & AssemblyGuidFlags.UseAssemblyName ) == AssemblyGuidFlags.UseAssemblyName)
            {
                AssemblyName assemblyName = assembly.GetName();

                string guidInformationString = ( flags & AssemblyGuidFlags.IgnoreVersion ) == AssemblyGuidFlags.IgnoreVersion ? assemblyName.Name : assemblyName.FullName;
                byte[] guidInformationBytes = Encoding.UTF8.GetBytes(guidInformationString);

                byte[] guidBytes = new byte[16];

                for (int i1 = 0; i1 < guidInformationBytes.Length; i1++)
                {
                    guidBytes[i1 % 16] = guidInformationBytes[i1];
                }

                Guid guid = new Guid(guidBytes);
                return guid;
            }

            return null;
        }

        public static DirectoryPath GetLocation (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            FilePath file = assembly.GetFile();

            return file == null ? null : file.Directory;
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

        public static Version GetProductVersion (this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] attributes1 = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), true);
            object[] attributes2 = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            object[] attributes3 = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true);

            if (attributes1.Length > 0)
            {
                return new Version(( (AssemblyVersionAttribute)attributes1[0] ).Version);
            }

            if (attributes2.Length > 0)
            {
                return new Version(( (AssemblyFileVersionAttribute)attributes2[0] ).Version);
            }

            if (attributes3.Length > 0)
            {
                return new Version(( (AssemblyInformationalVersionAttribute)attributes3[0] ).InformationalVersion);
            }

            return null;
        }

        #endregion
    }
}
