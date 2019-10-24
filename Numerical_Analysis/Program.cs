using System;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApplication1;

namespace Numerical_Analysis
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
            Application.Run(new Analysis());
        }
    }
}