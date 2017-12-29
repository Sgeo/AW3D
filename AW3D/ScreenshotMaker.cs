using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AW3D
{
    public class ScreenshotMaker
    {
        private static ScreenshotMaker instance;
        public static ScreenshotMaker Instance
        {
            get
            {
                if (instance == null) instance = new ScreenshotMaker();
                return instance;
            }
        }

        public void TakeScreenshot()
        {
            Process aworld = Process.GetProcessesByName("aworld").First();
            SetForegroundWindow(aworld.MainWindowHandle);
            SendKeys.SendWait("%t");
            Thread.Sleep(100);
            SendKeys.SendWait("r");
            Thread.Sleep(100);
            Guid guid = Guid.NewGuid();
            SendKeys.SendWait("3D Screenshot " + guid.ToString());
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
