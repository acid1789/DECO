using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecoPlayServer
{
    public partial class frmMain : Form
    {
        public frmMain( )
        {
            InitializeComponent( );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MainClass.InitServer( );
            Mobs.SpawnThread.Start( );
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

        private void button1_Click(object sender, EventArgs e)
        {
            MainClass.Players[0].SendCharData();
            /*Packet FromPacket = new Packet(0x256);
            FromPacket.WriteInt(5);
            MainClass.Players[0].Sock.Send(FromPacket);*/

            /*Packet aPacket = new Packet(0x255);
            aPacket.WriteByte(0x3E);
            aPacket.WriteUShort(2000);
            aPacket.WriteInt(20000);
            MainClass.Players[0].Sock.Send(aPacket);*/

            /*Packet Teleporting = new Packet(0x809);
            Teleporting.WriteByte(1);
            Teleporting.WriteUShort(110);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x409);
            Teleporting.WriteUInt(0x00002B04);
            Teleporting.WriteByte(0);
            Teleporting.WriteByte(1);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x804);
            Teleporting.WriteUShort(0);
            Teleporting.WriteUShort(0);

            Teleporting.WriteUInt(47);
            Teleporting.WriteString("123456789012345678901234567890", 40);
            Teleporting.WriteUShort(1);
            Teleporting.WriteByte(1);

            Teleporting.WriteString("GM SToRM", 17);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x462);
            Teleporting.WriteByte(1);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x460);
            Teleporting.WriteByte(1);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x363);
            Teleporting.WriteByte(30);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x361);
            Teleporting.WriteUShort(162);
            Teleporting.WriteUShort(164);
            Teleporting.WriteUShort(165);
            Teleporting.WriteByte(1);
            MainClass.Players[0].Sock.Send(Teleporting);*/

            /*Packet Teleporting = new Packet(0x472);
            Teleporting.WriteByte(1);
            Teleporting.WriteUInt(1);
            Teleporting.WriteUInt(1);
            MainClass.Players[0].Sock.Send(Teleporting);*/
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Player x in MainClass.Players)
            {
                x.CharData.Save( );
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ushort Map = ushort.Parse(textBox1.Text);
            Point Coord = new Point(int.Parse(textBox2.Text), int.Parse(textBox3.Text));

            CharData x = MainClass.Players[0].CharData;
            Server y = MainClass.Players[0].Sock;
            Packet Teleporting = new Packet(0x0132);
            Teleporting.WriteInt(Data.GetGameCoord(Map, Coord));
            Teleporting.WriteUShort(Map);
            Teleporting.WriteByte(1);
            y.Send(Teleporting);
            int MapIndex = Maps.MapsData.Find(x.Map);
            if (MapIndex != -1)
                Maps.RemovePlayer(MainClass.Players[0], Maps.MapsData[MapIndex]);
            x.Map = Map;
            x.Coord = Coord;
        }
    }
}
