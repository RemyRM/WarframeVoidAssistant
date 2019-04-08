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
    public partial class Form1 : Form
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

        struct CharCombi
        {
            public readonly char endChar;
            public readonly char startChar;

            internal CharCombi(char a, char b)
            {
                endChar = a;
                startChar = b;
            }
        }
        #endregion

        static RECT rect;
        static readonly CharCombi[] charSets = new CharCombi[]
        {
            new CharCombi('U', 'R'),
            new CharCombi('u', 'r')
        };

        static Process warframeProcess;
        static IntPtr windowHandle;

        static int windowHeight;
        static int windowWidth;
        static int yOffset;
        static int[] xOffsets;

        static List<string> results = new List<string>();

        /// <summary>
        /// Setup the screenshot area
        /// </summary>
        public Form1()
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
                    foreach (int i in xOffsets)
                    {
                        TakeScreenShot(i, yOffset);
                    }

                    foreach (string s in results)
                    {
                        Console.WriteLine(s);
                    }
                    Thread.Sleep(2000);
                    Console.ReadLine();
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
            var bmpScreenshot = new Bitmap(350, 30, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            //Copy the pixels from the screen into the Graphics
            gfxScreenshot.CopyFromScreen(x, y, 0, 0, new Size(rect.Right - rect.Left + 20, rect.Bottom - rect.Top), CopyPixelOperation.SourceCopy);


            Random rand = new Random();
            int randNum = rand.Next(0, 10000);
            string path = @"D:\Dev\C#\WarframeVoidRewardChecker\WarframeVoidRewardChecker\Resources\Images\Screenshot_" + randNum + ".png";
            bmpScreenshot.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            Tesseract(path);
        }

        /// <summary>
        /// Read the text contents from a screenshot
        /// </summary>
        /// <param name="testImagePath"></param>
        static void Tesseract(string testImagePath)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    using (var img = Pix.LoadFromFile(testImagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();

                            #region checkIfNeeded
                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            //Console.WriteLine("Text (GetText): \r\n{0}", text);
                            //Console.WriteLine("Text (iterator):");
                            //using (var iter = page.GetIterator())
                            //{
                            //    iter.Begin();

                            //    do
                            //    {
                            //        do
                            //        {
                            //            do
                            //            {
                            //                do
                            //                {
                            //                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                            //                    {
                            //                        //Console.WriteLine("<BLOCK>");
                            //                    }

                            //                    //Console.Write(iter.GetText(PageIteratorLevel.Word));
                            //                    //Console.Write(" ");

                            //                    if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                            //                    {
                            //                        //Console.WriteLine();
                            //                    }
                            //                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                            //                if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                            //                {
                            //                    //Console.WriteLine();
                            //                }
                            //            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                            //        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                            //    } while (iter.Next(PageIteratorLevel.Block));
                            //}
                            #endregion

                            ValidateResults(ref text);

                            results.Add(text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Debug.WriteLine("Unexpected Error: " + e.Message);
                Debug.WriteLine("Details: ");
                Debug.WriteLine(e.ToString());
            }
            //Console.Write("Press any key to continue . . . ");
            //Console.ReadKey(true);
        }

        /// <summary>
        /// Validate if the text obtained by tesseract is valid
        /// </summary>
        /// <param name="input"></param>
        static void ValidateResults(ref string input)
        {
            //Some character combinations end up getting recognised as whitespace, remove those whitespaces.
            foreach (CharCombi cc in charSets)
            {
                if (input.Contains("BARREL"))
                {
                    if (input.Contains($"{cc.endChar} {cc.startChar}"))
                    {
                        input = input.Replace($"{cc.endChar} {cc.startChar}", $"{cc.endChar}{cc.startChar}");
                    }
                }
            }

            //Remove any newlines Tesseract finds
            while (input.Contains("\n"))
            {
                input = input.Replace("\n", "");
            }

            if (CompareInputAgainstDictionary(input))
            {
                Console.WriteLine("Wowee!");
                return;
            }
            else
            {
                Console.WriteLine("admFail");

            }
        }

        static bool CompareInputAgainstDictionary(string input)
        {
            List<WarframeMarketItemClass> itemClass = new List<WarframeMarketItemClass>();
            itemClass = WarframeMarketApi.GetAllPrimeItems();

            return itemClass.Exists(o => o.item_name.Equals(input));
        }
    }
}