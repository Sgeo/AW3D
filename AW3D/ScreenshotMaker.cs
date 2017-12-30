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
            ForegroundAW();
            String awpath = aworld.MainModule.FileName;
            awdirectory = Path.GetDirectoryName(awpath);
            PostMessage(aworld.MainWindowHandle, WM_COMMAND, new IntPtr(144), IntPtr.Zero);
            Thread.Sleep(1000);
            IntPtr rememberMe = FindWindow("#32770", "Remember current location");
            //IntPtr rememberMeEdit = FindWindowEx(aworld.MainWindowHandle, IntPtr.Zero, "Edit", "");
            IntPtr rememberMeEdit = FindWindowEx(rememberMe, IntPtr.Zero, "Edit", null);
            Guid guid = Guid.NewGuid();
            //SendKeys.SendWait("3D Screenshot " + guid.ToString());
            //PostMessage(rememberMeEdit, WM_SETTEXT, IntPtr.Zero, "3D Screenshot " + guid.ToString());
            SendTextMessage(rememberMeEdit, "3D Screenshot " + guid.ToString());
            PostMessage(rememberMe, WM_COMMAND, new IntPtr(1), IntPtr.Zero);

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
            watcher = new FileSystemWatcher(Path.Combine(awdirectory, "screenshots"));
            Coords coords = CoordFromGuid(guid);
            watcher.Created += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
            {
                if (fileLeft == null)
                {
                    fileLeft = e.FullPath;
                    Thread.Sleep(100);
                    ForegroundAW();
                    coords.ShiftRight(0.006);
                    TeleportTo(coords);
                }
                else if (fileRight == null)
                {
                    fileRight = e.FullPath;
                    Thread.Sleep(100);
                    Finish();
                }

            });
            watcher.EnableRaisingEvents = true;

            TeleportTo(coords);



        }

        private static void SendTextMessage(IntPtr hwnd, string text)
        {
            foreach (char c in text)
            {
                PostMessage(hwnd, WM_CHAR, new IntPtr(c), IntPtr.Zero);
            }
            
        }

        private void ForegroundAW()
        {
            while (GetForegroundWindow() == aworld.MainWindowHandle)
            {
                // Wait for something to background AW
            }
            Thread.Sleep(100);
            while (GetForegroundWindow() != aworld.MainWindowHandle)
            {
                SetForegroundWindow(aworld.MainWindowHandle);
            }
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

            SendMessage(aworld.MainWindowHandle, WM_COMMAND, new IntPtr(128), IntPtr.Zero);
            Thread.Sleep(100);
            SendKeys.SendWait(coords.ToString());
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100);
        }

        private void Finish()
        {
            Image left = Image.FromFile(fileLeft);
            Image right = Image.FromFile(fileRight);
            Bitmap resultBitmap = new Bitmap(left.Width * 2, left.Height);
            Graphics resultGraphics = Graphics.FromImage(resultBitmap);
            resultGraphics.DrawImage(left, 0, 0);
            resultGraphics.DrawImage(right, right.Width, 0);

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "png files (*.png)|*.png";
            save.FilterIndex = 0;
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            Thread workaroundThread = new System.Threading.Thread(() =>
            {
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
                right.Dispose();
                File.Delete(fileLeft);
                File.Delete(fileRight);
            });
            workaroundThread.SetApartmentState(System.Threading.ApartmentState.STA);
            workaroundThread.Start();
            workaroundThread.Join();
        }

        private Process aworld;
        private string awdirectory;
        private string fileLeft = null;
        private string fileRight = null;
        FileSystemWatcher watcher;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, string lParam);

        private static int WM_COMMAND = 0x0111;
        private static int WM_SETTEXT = 0x000C;
        private static int WM_CHAR = 0x0102;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

    }
}
