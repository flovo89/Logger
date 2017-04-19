using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;




namespace Logger.Common.Base.Windows
{
    public static class WindowExtensions
    {
        #region Static Methods

        public static void EnableWindow (this Window window, bool enable)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.EnableWindow(hWnd, enable);
        }

        public static Screen GetScreen (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            Screen screen = Screen.FromHandle(window.GetWindowHandle());
            return screen;
        }

        public static int GetScreenIndex (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            Screen screen = window.GetScreen();
            Screen[] allScreens = Screen.AllScreens;
            for (int i1 = 0; i1 < allScreens.Length; i1++)
            {
                if (allScreens[i1].DeviceName.Equals(screen.DeviceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i1;
                }
            }

            return 0;
        }

        public static IntPtr GetWindowHandle (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            WindowInteropHelper helper = new WindowInteropHelper(window);

            return helper.Handle;
        }

        public static void HideWindow (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.HideWindow(hWnd);
        }

        public static void MaximizeWindow (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MaximizeWindow(hWnd);
        }

        public static void MinimizeWindow (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MinimizeWindow(hWnd);
        }

        public static void MoveWindowToBackground (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MoveWindowToBackground(hWnd);
        }

        public static void MoveWindowToForeground (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MoveWindowToForeground(hWnd);
        }

        public static void MoveWindowToPrimaryScreen (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MoveWindowToPrimaryScreen(hWnd);
        }

        public static void MoveWindowToScreen (this Window window, Screen screen)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MoveWindowToScreen(hWnd, screen);
        }

        public static void MoveWindowToScreen (this Window window, int screenIndex)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.MoveWindowToScreen(hWnd, screenIndex);
        }

        public static void NormalizeWindow (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.NormalizeWindow(hWnd);
        }

        public static void RelocateWindow (this Window window, int x, int y, int width, int height)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.RelocateWindow(hWnd, x, y, width, height);
        }

        public static void ShowWindow (this Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr hWnd = window.GetWindowHandle();

            SystemWindows.ShowWindow(hWnd);
        }

        #endregion
    }
}
