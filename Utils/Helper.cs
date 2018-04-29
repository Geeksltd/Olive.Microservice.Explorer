using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Process = System.Diagnostics.Process;

namespace MicroserviceExplorer.Utils
{
    public static class Helper
    {
        public static IEnumerable<DTE2> GetVsInstances()
        {
            var vsList = new List<DTE2>();

            var retVal = NativeMethods.GetRunningObjectTable(0, out var rot);

            if (retVal != 0) return Enumerable.Empty<DTE2>();

            rot.EnumRunning(out var enumMoniker);

            var fetched = IntPtr.Zero;
            var moniker = new IMoniker[1];
            while (enumMoniker.Next(1, moniker, fetched) == 0)
            {
                NativeMethods.CreateBindCtx(0, out var bindCtx);
                moniker[0].GetDisplayName(bindCtx, null, out var displayName);
                //Console.WriteLine("Display Name: {0}", displayName);
                var isVisualStudio = displayName.StartsWith("!VisualStudio");
                if (!isVisualStudio) continue;

                rot.GetObject(moniker[0], out var obj);
                var dte = obj as DTE2;
                vsList.Add(dte);                
            }
            return vsList;
        }

        public static void Launch(string url)
        {
            try
            {
                Process.Start("cmd", "/C start" + " " + url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
