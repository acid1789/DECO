using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class Msngr
    {
        public static void MemberInsert(Packet packet, Player player)
        {
            string CharName = packet.ReadString(17);

            int PlayerIndex = MainClass.Players.Find(CharName);
            if (PlayerIndex != -1)
            {
                Packet Response = new Packet(0x7E03);
                Response.WriteString(player.CharName, 17);
                MainClass.Players[PlayerIndex].Sock.Send(Response);
            }
        }
    }
}
