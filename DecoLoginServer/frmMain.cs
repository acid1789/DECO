using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecoLoginServer
{
    public partial class frmMain : Form
    {
        public frmMain( )
        {
            InitializeComponent( );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AccountControl.GetLoginState("rozalin5001", "mi55555");
            MainClass.InitClass( );
        }

        public void AddLog(string Text)
        {
            lstLog.Items.Add(Text);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstLog.SelectedIndex != -1)
                Clipboard.SetText(lstLog.Text);
        }
    }
}
