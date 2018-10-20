using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SGL
{
    internal static class NativeMethods
    {
        internal const int SW_HIDE = 0;
        internal const int SW_SHOW = 5;

        internal const int GWL_STYLE = -16;
        internal const int WS_SYSMENU = 0x80000;

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        internal static void HideCloseButton(Window window)
        {
            var hWnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
