using System.Runtime.InteropServices;




namespace Logger.Common.Runtime
{
    public static class WindowsSession
    {
        #region Constants

        private const uint FlagsForceIfHung = 0x10;

        private const uint FlagsLogOff = 0x00;

        private const uint FlagsPowerOff = 0x08;

        private const uint FlagsReboot = 0x02;

        private const uint ReasonFlagUserDefined = 0x40000000;

        private const uint ReasonMajorApplication = 0x00040000;

        private const uint ReasonMinorOther = 0x00000000;

        #endregion




        #region Static Methods

        public static void Logoff (bool force)
        {
            WindowsSession.AdjustPrivileges();

            if (force)
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsLogOff | WindowsSession.FlagsForceIfHung, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
            else
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsLogOff, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
        }

        public static void Restart (bool force)
        {
            WindowsSession.AdjustPrivileges();

            if (force)
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsReboot | WindowsSession.FlagsForceIfHung, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
            else
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsReboot, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
        }

        public static void Shutdown (bool force)
        {
            WindowsSession.AdjustPrivileges();

            if (force)
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsPowerOff | WindowsSession.FlagsForceIfHung, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
            else
            {
                WindowsSession.ExitWindowsEx(WindowsSession.FlagsPowerOff, WindowsSession.ReasonMajorApplication | WindowsSession.ReasonMinorOther | WindowsSession.ReasonFlagUserDefined);
            }
        }

        private static void AdjustPrivileges ()
        {
            Privileges.EnablePrivilege(SecurityEntity.SeShutdownName);
        }

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool ExitWindowsEx (uint uFlags, uint dwReason);

        #endregion
    }
}
