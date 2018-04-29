using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceExplorer.Utils
{
    public static class MyEnumWindows
    {
        public delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        static readonly List<string> windowTitles = new List<string>();

        public static List<string> GetWindowTitles(bool includeChildren)
        {
            NativeMethods.EnumWindows(EnumWindowsCallback, includeChildren ? (IntPtr)1 : IntPtr.Zero);
            return windowTitles;
        }

        static bool EnumWindowsCallback(IntPtr testWindowHandle, IntPtr includeChildren)
        {
            var title = GetWindowTitle(testWindowHandle);
            if (TitleMatches(title))
            {
                windowTitles.Add(title);
            }
            if (includeChildren.Equals(IntPtr.Zero) == false)
            {
                NativeMethods.EnumChildWindows(testWindowHandle, EnumWindowsCallback, IntPtr.Zero);
            }
            return true;
        }

        static string GetWindowTitle(IntPtr windowHandle)
        {
            const uint smtoAbortifhung = 0x0002;
            const uint wmGettext = 0xD;
            const int maxStringSize = 32768;
            var title = string.Empty;
            var memoryHandle = Marshal.AllocCoTaskMem(maxStringSize);
            Marshal.Copy(title.ToCharArray(), 0, memoryHandle, title.Length);
            NativeMethods.SendMessageTimeout(windowHandle, wmGettext, (IntPtr)maxStringSize, memoryHandle, smtoAbortifhung, (uint)1000, out _);
            title = Marshal.PtrToStringAuto(memoryHandle);
            Marshal.FreeCoTaskMem(memoryHandle);
            return title;
        }

        static bool TitleMatches(string title) => title.Contains("e");
    }
}
