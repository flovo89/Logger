using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;




namespace Logger.Common.Windows
{
    public static class SystemWindows
    {
        #region Constants

        private const uint SmtoAbortifhung = 0x0002;

        private const uint SmtoNormal = 0x000;

        private const uint SwHide = 0;

        private const uint SwpNoactivate = 0x0010;

        private const uint SwpNomove = 0x0002;

        private const uint SwpNosize = 0x0001;

        private const uint SwpNozorder = 0x0004;

        private const uint SwShow = 5;

        private const uint SwShowmaximized = 3;

        private const uint SwShowminimized = 2;

        private const uint SwShownormal = 1;

        private const uint WmGettext = 0x000D;

        private const uint WmGettextlength = 0x000E;

        private static readonly IntPtr HwndBottom = new IntPtr(1);

        #endregion




        #region Static Constructor/Destructor

        static SystemWindows ()
        {
            SystemWindows.FindChildWindowsSyncRoot = new object();
            SystemWindows.FindWindowsSyncRoot = new object();
        }

        #endregion




        #region Static Properties/Indexer

        private static object FindChildWindowsSyncRoot { get; set; }

        private static List<IntPtr> FindChildWindowsWindows { get; set; }

        private static object FindWindowsSyncRoot { get; set; }

        private static List<IntPtr> FindWindowsWindows { get; set; }

        #endregion




        #region Static Methods

        public static void EnableWindow (IntPtr hWnd, bool enable)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.EnableWindowInternal(hWnd, enable);
        }

        public static IntPtr[] FindChildWindows (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return new IntPtr[0];
            }

            lock (SystemWindows.FindChildWindowsSyncRoot)
            {
                if (SystemWindows.FindChildWindowsWindows == null)
                {
                    SystemWindows.FindChildWindowsWindows = new List<IntPtr>();
                }

                SystemWindows.FindChildWindowsWindows.Clear();

                SystemWindows.EnumChildWindows(hWnd, SystemWindows.FindChildWindowsProc, IntPtr.Zero);

                return SystemWindows.FindChildWindowsWindows.ToArray();
            }
        }

        public static IntPtr[] FindWindows ()
        {
            lock (SystemWindows.FindWindowsSyncRoot)
            {
                if (SystemWindows.FindWindowsWindows == null)
                {
                    SystemWindows.FindWindowsWindows = new List<IntPtr>();
                }

                SystemWindows.FindWindowsWindows.Clear();

                SystemWindows.EnumWindows(SystemWindows.FindWindowsProc, IntPtr.Zero);

                return SystemWindows.FindWindowsWindows.ToArray();
            }
        }

        public static DateTime GetLastInput ()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            SystemWindows.GetLastInputInfo(ref info);

            double idleTicks = ( Environment.TickCount - info.dwTime ) * -1.0;

            DateTime inputTimestamp = DateTime.Now.AddMilliseconds(idleTicks);
            return inputTimestamp;
        }

        public static Process GetProcess (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return null;
            }

            int processId = 0;

            SystemWindows.GetWindowThreadProcessId(hWnd, ref processId);

            Process[] processes = Process.GetProcesses();

            Process process = ( from p in processes where p.Id == processId select p ).FirstOrDefault();

            return process;
        }

        public static string GetWindowTitle (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return null;
            }

            int length = -1;

            SystemWindows.SendMessageTimeout1(hWnd, SystemWindows.WmGettextlength, IntPtr.Zero, IntPtr.Zero, SystemWindows.SmtoAbortifhung | SystemWindows.SmtoNormal, 1000, ref length);

            if (length == -1)
            {
                return null;
            }

            if (length == 0)
            {
                return string.Empty;
            }

            if (length > 10240)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(length + 1);

            IntPtr temp = new IntPtr();

            SystemWindows.SendMessageTimeout2(hWnd, SystemWindows.WmGettext, new IntPtr(sb.Capacity), sb, SystemWindows.SmtoAbortifhung | SystemWindows.SmtoNormal, 1000, ref temp);

            if (temp.ToInt64() == -1)
            {
                return null;
            }

            if (temp.ToInt64() == 0)
            {
                return string.Empty;
            }

            string title = sb.ToString();

            return title;
        }

        public static void HideWindow (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwHide);
        }

        public static void MaximizeWindow (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShowmaximized);
        }

        public static void MinimizeWindow (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShowminimized);
        }

        public static void MoveWindowToBackground (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.SetWindowPos(hWnd, SystemWindows.HwndBottom, 0, 0, 0, 0, SystemWindows.SwpNosize | SystemWindows.SwpNomove | SystemWindows.SwpNoactivate);
        }

        public static void MoveWindowToForeground (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.SetForegroundWindow(hWnd);
            SystemWindows.BringWindowToTop(hWnd);
            SystemWindows.SetActiveWindow(hWnd);
        }

        public static void MoveWindowToPrimaryScreen (IntPtr hWnd)
        {
            SystemWindows.MoveWindowToScreen(hWnd, null);
        }

        public static void MoveWindowToScreen (IntPtr hWnd, int screenIndex)
        {
            if (screenIndex < -1)
            {
                screenIndex = 0;
            }

            if (screenIndex >= Screen.AllScreens.Length)
            {
                screenIndex = Screen.AllScreens.Length - 1;
            }

            SystemWindows.MoveWindowToScreen(hWnd, screenIndex == -1 ? null : Screen.AllScreens[screenIndex]);
        }

        public static void MoveWindowToScreen (IntPtr hWnd, Screen screen)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            screen = screen ?? Screen.PrimaryScreen;

            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            SystemWindows.GetWindowInfo(hWnd, ref info);

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.Length = (uint)Marshal.SizeOf(placement);
            SystemWindows.GetWindowPlacement(hWnd, ref placement);

            bool isMaximized = placement.ShowCmd == SystemWindows.SwShowmaximized;
            bool isMinimized = placement.ShowCmd == SystemWindows.SwShowminimized;
            bool isNormal = placement.ShowCmd == SystemWindows.SwShownormal;

            int width = info.rcWindow.Right - info.rcWindow.Left;
            int height = info.rcWindow.Bottom - info.rcWindow.Top;

            if (isMaximized || isMinimized)
            {
                SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShownormal);
            }

            SystemWindows.SetWindowPos(hWnd, IntPtr.Zero, screen.WorkingArea.Left, screen.WorkingArea.Top, 0, 0, SystemWindows.SwpNozorder | SystemWindows.SwpNosize);

            if (isNormal)
            {
                SystemWindows.SetWindowPos(hWnd, IntPtr.Zero, screen.WorkingArea.Left + ( screen.WorkingArea.Width - width ) / 2, screen.WorkingArea.Top + ( screen.WorkingArea.Height - height ) / 2, 0, 0, SystemWindows.SwpNozorder | SystemWindows.SwpNosize);
            }
            else if (isMaximized)
            {
                SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShowmaximized);
            }
            else if (isMinimized)
            {
                SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShowminimized);
            }
        }

        public static void NormalizeWindow (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShownormal);
        }

        public static void RelocateWindow (IntPtr hWnd, int x, int y, int width, int height)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            SystemWindows.MoveWindow(hWnd, x, y, width, height, true);
        }

        public static void ShowWindow (IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            SystemWindows.ShowWindow(hWnd, (int)SystemWindows.SwShow);
        }

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool BringWindowToTop (IntPtr hWnd);

        [DllImport ("user32.dll", SetLastError = false, EntryPoint = "EnableWindow")]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool EnableWindowInternal (IntPtr hWnd, [MarshalAs (UnmanagedType.Bool)] bool enable);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool EnumChildWindows (IntPtr hWnd, EnumChildWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool EnumWindows (EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private static bool FindChildWindowsProc (IntPtr hWnd, ref IntPtr lParam)
        {
            SystemWindows.FindChildWindowsWindows.Add(hWnd);

            return true;
        }

        private static bool FindWindowsProc (IntPtr hWnd, ref IntPtr lParam)
        {
            SystemWindows.FindWindowsWindows.Add(hWnd);

            return true;
        }

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo (ref LASTINPUTINFO plii);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool GetWindowInfo (IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement (IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport ("user32.dll", SetLastError = false)]
        private static extern uint GetWindowThreadProcessId (IntPtr hWnd, ref int lpdwProcessId);

        [DllImport ("User32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool MoveWindow (IntPtr handle, int x, int y, int width, int height, [MarshalAs (UnmanagedType.Bool)] bool redraw);

        [DllImport ("user32.dll", SetLastError = false, EntryPoint = "SendMessageTimeout")]
        private static extern IntPtr SendMessageTimeout1 (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, ref int lpdwResult);

        [DllImport ("user32.dll", SetLastError = false, EntryPoint = "SendMessageTimeout", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageTimeout2 (IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam, uint fuFlags, uint uTimeout, ref IntPtr lpdwResult);

        [DllImport ("user32.dll", SetLastError = false)]
        private static extern IntPtr SetActiveWindow (IntPtr hWnd);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow (IntPtr hWnd);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport ("user32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool ShowWindow (IntPtr hWnd, int nCmdShow);

        #endregion




        #region Type: EnumChildWindowsProc

        private delegate bool EnumChildWindowsProc (IntPtr hWnd, ref IntPtr lParam);

        #endregion




        #region Type: EnumWindowsProc

        private delegate bool EnumWindowsProc (IntPtr hWnd, ref IntPtr lParam);

        #endregion




        #region Type: POINT

        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [StructLayout (LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;

            public int Y;
        }

        #endregion




        [StructLayout (LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }




        #region Type: RECT

        [StructLayout (LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }

        #endregion




        #region Type: WINDOWINFO

        [StructLayout (LayoutKind.Sequential)]
        private struct WINDOWINFO
        {
            public uint cbSize;

            public RECT rcWindow;

            public RECT rcClient;

            public uint dwStyle;

            public uint dwExStyle;

            public uint dwWindowStatus;

            public uint cxWindowBorders;

            public uint cyWindowBorders;

            public ushort atomWindowType;

            public ushort wCreatorVersion;
        }

        #endregion




        #region Type: WINDOWPLACEMENT

        [StructLayout (LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public uint Length;

            public uint Flags;

            public uint ShowCmd;

            public POINT MinPosition;

            public POINT MaxPosition;

            public RECT NormalPosition;
        }




        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore InconsistentNaming

        #endregion
    }
}
