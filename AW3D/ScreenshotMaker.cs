using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public void Activate()
        {
            fileLeft = null;
            fileRight = null;
            aworld = Process.GetProcessesByName("aworld").First();
            SetForegroundWindow(aworld.MainWindowHandle);
            String awpath = aworld.MainModule.FileName;
            awdirectory = Path.GetDirectoryName(awpath);
            SendKeys.SendWait("%t");
            Thread.Sleep(100);
            SendKeys.SendWait("r");
            Thread.Sleep(100);
            Guid guid = Guid.NewGuid();
            SendKeys.SendWait("3D Screenshot " + guid.ToString());
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100);

            FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(awdirectory, "screenshots"));
            Coords coords = CoordFromGuid(guid);
            watcher.Created += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
            {
                if(fileLeft == null)
                {
                    fileLeft = e.FullPath;
                    Thread.Sleep(100);
                    SetForegroundWindow(aworld.MainWindowHandle);
                    coords.ShiftRight(0.06);
                    TeleportTo(coords);
                } else if (fileRight == null)
                {
                    fileRight = e.FullPath;
                }

            });
            watcher.EnableRaisingEvents = true;

            
            TeleportTo(coords);



        }


        private Coords CoordFromGuid(Guid guid)
        {
            using (StreamReader teleportStream = new StreamReader(Path.Combine(awdirectory, "teleport.txt")))
            {
                String line;
                while ((line = teleportStream.ReadLine()) != null)
                {
                    if (line.EndsWith("3D Screenshot " + guid.ToString()))
                    {
                        return Coords.Parse(line);
                    }
                }
                return null;
            }
        }

        private void TeleportTo(Coords coords)
        {
            SendKeys.SendWait("%t");
            Thread.Sleep(100);
            SendKeys.SendWait("t");
            Thread.Sleep(100);
            SendKeys.SendWait(coords.ToString());
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100);
        }

        private Process aworld;
        private string awdirectory;
        private string fileLeft = null;
        private string fileRight = null;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
