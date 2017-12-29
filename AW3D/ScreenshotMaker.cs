using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
            watcher = new FileSystemWatcher(Path.Combine(awdirectory, "screenshots"));
            Coords coords = CoordFromGuid(guid);
            watcher.Created += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
            {
                if(fileLeft == null)
                {
                    fileLeft = e.FullPath;
                    Thread.Sleep(100);
                    SetForegroundWindow(aworld.MainWindowHandle);
                    coords.ShiftRight(0.006);
                    TeleportTo(coords);
                } else if (fileRight == null)
                {
                    fileRight = e.FullPath;
                    Finish();
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

        private void Finish()
        {
            Image left = Image.FromFile(fileLeft);
            //Image right = Image.FromFile(fileRight);
            Bitmap resultBitmap = new Bitmap(left.Width * 2, left.Height);
            Graphics resultGraphics = Graphics.FromImage(resultBitmap);
            resultGraphics.DrawImage(left, 0, 0);
            //resultGraphics.DrawImage(right, left.Width, 0);

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "png files (*.png)|*.png";
            save.FilterIndex = 0;
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (save.ShowDialog() == DialogResult.OK)
            {
                using (Stream stream = save.OpenFile())
                {
                    resultBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            resultGraphics.Dispose();
            resultBitmap.Dispose();
            left.Dispose();
            //right.Dispose();
        }

        private Process aworld;
        private string awdirectory;
        private string fileLeft = null;
        private string fileRight = null;
        FileSystemWatcher watcher;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
