using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoVersionServer
{
    class MainClass
    {
        public static Listener ListenSock = new Listener( );

        public static void InitClass( )
        {
            ListenSock.Connected += Connected;
            ListenSock.Listen(11300);
        }

        public static void Connected(Server sender)
        {
            sender.PacketReceived += new Server.PacketEventHandler((Server _sender, Packet packet) =>
            {
                Program.frm1.AddLog("(C - S)  " + packet.ToString( ));
                PacketReceived(_sender, packet);
            });

            sender.PacketSent += new Server.PacketEventHandler((Server _sender, Packet packet) =>
            {
                Program.frm1.AddLog("(S - C)  " + packet.ToString( ));
            });
        }

        public static void PacketReceived(Server sender, Packet packet)
        {
            switch (packet.Opcode)
            {
                case 0xFFC9: // VersionPacket
                {
                    Packet VersionResponse = new Packet(0xFFCE);
                    VersionResponse.WriteByte(1);
                    sender.Send(VersionResponse);
                }
                break;
            }
        }
    }
}
