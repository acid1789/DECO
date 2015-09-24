using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class Chatting
    {
        public static void Whisper(Packet packet, Player player)
        {
            string CharName = packet.ReadString(17);
            byte Length = packet.ReadByte();
            packet.ReadByte();
            string Text = packet.ReadString(Length);

            int PlayerIndex = MainClass.Players.Find(CharName);
            if (PlayerIndex != -1)
            {
                Packet ToPacket = new Packet(0x0223);
                ToPacket.WriteString(CharName, 17);
                ToPacket.WriteByte(0);
                ToPacket.WriteByte(1);
                player.Sock.Send(ToPacket);

                Packet FromPacket = new Packet(0x0224);
                FromPacket.WriteString(player.CharName, 17);
                FromPacket.WriteByte(0);
                FromPacket.WriteByte(Length);
                FromPacket.WriteString(Text, Length);
                MainClass.Players[PlayerIndex].Sock.Send(FromPacket);
            }
        }
    }
}
