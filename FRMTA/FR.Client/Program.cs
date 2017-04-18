/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/

using FR.Client.Moduls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FR.Client
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}