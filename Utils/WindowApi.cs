using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceExplorer.Utils
{
    public static class WindowApi
    {

        public static void ShowWindow(IntPtr windowHandle)
        {
            var placement = new NativeMethods.Windowplacement();
            NativeMethods.GetWindowPlacement(windowHandle, ref placement);

            NativeMethods.ShowWindow(windowHandle, placement.ShowCmd == 2 ? NativeMethods.ShowWindowEnum.Restore : NativeMethods.ShowWindowEnum.ShowMaximized);

            NativeMethods.SetForegroundWindow(windowHandle);
        }
        public static void HideWindow(IntPtr windowHandle)
        {
            NativeMethods.ShowWindow(windowHandle, NativeMethods.ShowWindowEnum.Hide);

            //SetForegroundWindow(windowHandle);
        }
    }
}
