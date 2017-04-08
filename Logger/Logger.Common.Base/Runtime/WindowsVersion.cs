using System;




namespace Logger.Common.Base.Runtime
{
    public static class WindowsVersion
    {
        #region Static Properties/Indexer

        public static bool IsRunningWindows10OrNewer
        {
            get
            {
                return Environment.OSVersion.IsWindows10OrNewer();
            }
        }

        public static bool IsRunningWindows7
        {
            get
            {
                return Environment.OSVersion.IsWindows7();
            }
        }

        public static bool IsRunningWindows7OrNewer
        {
            get
            {
                return Environment.OSVersion.IsWindows7OrNewer();
            }
        }

        public static bool IsRunningWindows8
        {
            get
            {
                return Environment.OSVersion.IsWindows8();
            }
        }

        public static bool IsRunningWindows8OrNewer
        {
            get
            {
                return Environment.OSVersion.IsWindows8OrNewer();
            }
        }

        public static bool IsRunningWindowsVista
        {
            get
            {
                return Environment.OSVersion.IsWindowsVista();
            }
        }

        public static bool IsRunningWindowsVistaOrNewer
        {
            get
            {
                return Environment.OSVersion.IsWindowsVistaOrNewer();
            }
        }

        public static bool IsRunningWindowsXp
        {
            get
            {
                return Environment.OSVersion.IsWindowsXp();
            }
        }

        public static bool IsRunningWindowsXpOrNewer
        {
            get
            {
                return Environment.OSVersion.IsWindowsXpOrNewer();
            }
        }

        public static Version ServicePackVersion
        {
            get
            {
                return Environment.OSVersion.GetServicePackVersion();
            }
        }

        #endregion
    }
}
