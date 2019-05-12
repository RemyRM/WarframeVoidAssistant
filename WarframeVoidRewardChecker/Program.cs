using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WarframeVoidRewardChecker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //First we want to read in the list containing all viable prime items
            WarframeMarketApi.ReadWarframeMarketAllItemsJson();

            Application.Run(new VoidChecker());
        }
    }
}
