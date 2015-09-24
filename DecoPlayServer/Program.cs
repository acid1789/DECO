using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecoPlayServer
{
    static class Program
    {
        public static frmMain frm1 = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main( )
        {
            Application.EnableVisualStyles( );
            Application.SetCompatibleTextRenderingDefault(false);
            frmMain.CheckForIllegalCrossThreadCalls = false;
            frm1 = new frmMain( );
            Application.Run(frm1);
        }
    }
}
