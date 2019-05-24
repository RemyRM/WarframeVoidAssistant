using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Tesseract;
using System.Collections.Generic;

namespace WarframeVoidRewardChecker
{
    public partial class VoidChecker : Form
    {
        #region DllImports
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        #endregion

        #region structs
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        static RECT rect;

        static Process warframeProcess;
        static IntPtr windowHandle;

        static int windowHeight;
        static int windowWidth;
        static int yOffset;
        static int[] xOffsets;

        internal static List<string> results = new List<string>();

        /// <summary>
        /// Setup the screenshot area
        /// </summary>
        public VoidChecker()
        {
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            Init();

            Size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);

            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;

            Thread t = new Thread(InputLoop);
            t.Start();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        static void Init()
        {
            GetWarframeProcess();

            rect = new RECT();
            GetWindowRect(windowHandle, ref rect);

            windowHeight = rect.Bottom - rect.Top;
            windowWidth = rect.Right - rect.Left;
            yOffset = (int)Math.Floor(windowHeight / 2.355);

            xOffsets = new int[]
            {
                (int)Math.Floor(windowWidth / 18.0),
                (int)Math.Floor(windowWidth / 3.55),
                (int)Math.Floor(windowWidth / 1.98),
                (int)Math.Floor(windowWidth / 1.37)
            };

            Debug.WriteLine("Left: " + rect.Left + " Right: " + rect.Right + " top: " + rect.Top + " bottom: " + rect.Bottom);
            Debug.WriteLine("width: " + windowWidth);
            Debug.WriteLine("height:" + windowHeight);
        }

        /// <summary>
        /// Get the warframe process
        /// </summary>
        static int iter = 0;
        static void GetWarframeProcess()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("Warframe.x64");

                warframeProcess = processes.FirstOrDefault();
                if (warframeProcess != null)
                {
                    windowHandle = warframeProcess.MainWindowHandle;
                    Debug.WriteLine("Warframe was found");
                }
                else
                {
                    if (iter > 10)
                    {
                        //start new warframe process
                    }
                    Debug.WriteLine("Warframe was not found, retrying");
                    iter++;
                    GetWarframeProcess();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e);
            }
        }

        /// <summary>
        /// Main loop that waits for a keypress
        /// </summary>
        static void InputLoop()
        {
            Console.WriteLine("Input loop is ready");
            while (true)
            {
                if (GetAsyncKeyState(Keys.F10) < 0)
                {
                    foreach (int x in xOffsets)
                    {
                        TakeScreenShot(x, yOffset);
                    }

                    foreach (string s in results)
                    {
                        Console.WriteLine(s);
                    }
                    //Thread.Sleep(2000);
                    Console.WriteLine("-------------------------");
                    results.Clear();
                    //Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Takes a screenshot of the current process window offset by x and y
        /// </summary>
        /// <param name="x">Location offset along the x axis</param>
        /// <param name="y">Location offset along the y axis</param>
        static void TakeScreenShot(int x, int y)
        {

            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(400, 30, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            //Copy the pixels from the screen into the Graphics
            gfxScreenshot.CopyFromScreen(x, y, 0, 0, new Size(rect.Right - rect.Left + 20, rect.Bottom - rect.Top), CopyPixelOperation.SourceCopy);

            Random rand = new Random();
            int randNum = rand.Next(0, 10000);
            string path = @"D:\Dev\C#\WarframeVoidRewardChecker\WarframeVoidRewardChecker\Resources\Images\Screenshot_" + randNum + ".png";

            //bmpScreenshot = ToGrayScale(bmpScreenshot);
            //bmpScreenshot = RemoveBackground(bmpScreenshot);

            bmpScreenshot.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            TesseractOCR.Tesseract(path);
        }

        /// <summary>
        /// Removes any pixels of which the blue value is darker than 30. Since it's a grayscale image this is a constant darkness.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private static Bitmap RemoveBackground(Bitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.B < 30)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }

            return bmp;
        }

        /// <summary>
        /// Create a grayscale version of an image
        /// </summary>
        /// <param name="Bmp"></param>
        /// <returns></returns>
        static Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
            {
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            }
            return Bmp;
        }
    }
}