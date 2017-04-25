using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;




namespace Logger.Common.Runtime
{
    public static class WindowsApi
    {
        #region Constants

        private const uint FormatMessageAllocateBuffer = 0x00000100;

        private const uint FormatMessageFromSystem = 0x00001000;

        private const uint FormatMessageIgnoreInserts = 0x00000200;

        #endregion




        #region Static Methods

        public static string GetErrorMessage (int errorCode)
        {
            return WindowsApi.GetErrorMessage(errorCode, null);
        }

        public static string GetErrorMessage (int errorCode, CultureInfo language)
        {
            IntPtr messageBuffer = IntPtr.Zero;

            try
            {
                uint chars = WindowsApi.FormatMessage(WindowsApi.FormatMessageFromSystem | WindowsApi.FormatMessageAllocateBuffer | WindowsApi.FormatMessageIgnoreInserts, IntPtr.Zero, (uint)errorCode, language == null ? 0 : language.LCID, ref messageBuffer, 0, IntPtr.Zero);

                if (chars == 0)
                {
                    throw new Win32Exception(WindowsApi.GetLastErrorCode());
                }

                string message = Marshal.PtrToStringUni(messageBuffer);

                return message;
            }
            finally
            {
                if (messageBuffer != IntPtr.Zero)
                {
                    WindowsApi.LocalFree(messageBuffer);
                    messageBuffer = IntPtr.Zero;
                }
            }
        }

        public static int GetLastErrorCode ()
        {
            return Marshal.GetLastWin32Error();
        }

        [DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint FormatMessage (uint dwFlags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr pArguments);

        [DllImport ("kernel32.dll", SetLastError = false)]
        private static extern IntPtr LocalFree (IntPtr hMem);

        #endregion
    }
}
