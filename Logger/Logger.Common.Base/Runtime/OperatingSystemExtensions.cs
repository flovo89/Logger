using System;




namespace Logger.Common.Base.Runtime
{
    public static class OperatingSystemExtensions
    {
        #region Static Methods

        public static Version GetServicePackVersion (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            string servicePack = operatingSystem.ServicePack.Trim().RemoveLineBreaks().Replace(",", ".").Keep(x => char.IsDigit(x) || ( x == '.' ));
            Version servicePackVersion = new Version(servicePack);
            return servicePackVersion;
        }

        public static bool IsWindows10OrNewer (this OperatingSystem operatingSystem)
        {
            return ( operatingSystem.Platform == PlatformID.Win32NT ) && ( operatingSystem.Version.Major >= 10 );
        }

        public static bool IsWindows7 (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return operatingSystem.IsWindows7OrNewer() && !operatingSystem.IsWindows8OrNewer();
        }

        public static bool IsWindows7OrNewer (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return ( operatingSystem.Platform == PlatformID.Win32NT ) && ( ( operatingSystem.Version.Major > 6 ) || ( ( operatingSystem.Version.Major == 6 ) && ( operatingSystem.Version.Minor >= 1 ) ) );
        }

        public static bool IsWindows8 (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return operatingSystem.IsWindows8OrNewer() && !operatingSystem.IsWindows10OrNewer();
        }

        public static bool IsWindows8OrNewer (this OperatingSystem operatingSystem)
        {
            return ( operatingSystem.Platform == PlatformID.Win32NT ) && ( ( operatingSystem.Version.Major > 6 ) || ( ( operatingSystem.Version.Major == 6 ) && ( operatingSystem.Version.Minor >= 2 ) ) );
        }

        public static bool IsWindowsVista (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return operatingSystem.IsWindowsVistaOrNewer() && !operatingSystem.IsWindows7OrNewer();
        }

        public static bool IsWindowsVistaOrNewer (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return ( operatingSystem.Platform == PlatformID.Win32NT ) && ( operatingSystem.Version.Major >= 6 );
        }

        public static bool IsWindowsXp (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return operatingSystem.IsWindowsXpOrNewer() && !operatingSystem.IsWindowsVistaOrNewer();
        }

        public static bool IsWindowsXpOrNewer (this OperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
            {
                throw new ArgumentNullException(nameof(operatingSystem));
            }

            return ( operatingSystem.Platform == PlatformID.Win32NT ) && ( ( operatingSystem.Version.Major > 5 ) || ( ( operatingSystem.Version.Major == 5 ) && ( operatingSystem.Version.Minor >= 1 ) ) );
        }

        #endregion
    }
}
