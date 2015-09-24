using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class Party
    {
        public static void PartyRequest(Packet packet, Player player)
        {
            string CharName = packet.ReadString(17);
            byte Type = packet.ReadByte( );

            int PlayerIndex = MainClass.Players.Find(CharName);
            if (PlayerIndex != -1)
            {
                Packet Response = new Packet(0x0711);
                Response.WriteString(player.CharName, 17);
                Response.WriteByte(Type);
                MainClass.Players[PlayerIndex].Sock.Send(Response);
            }
        }
    }
}
