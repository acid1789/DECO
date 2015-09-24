using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class Communication
    {
        public static void Expressing(Packet packet, Player player)
        {
            byte Type = packet.ReadByte( );

            #region Response
            Packet Response = new Packet(0x0230);
            Response.WriteUInt(1); // Char ID ( if the char is the same it will be 1)
            Response.WriteByte(Type);
            player.Sock.Send(Response);
            #endregion

            #region Others
            Packet Others = new Packet(0x0230);
            Others.WriteUInt(player.ID);
            Others.WriteByte(Type);

            int MapIndex = Maps.MapsData.Find(player.CharData.Map);
            if (MapIndex != -1)
            {
                foreach (Player x in Maps.MapsData[MapIndex].Players)
                {
                    if (x.ID != player.ID)
                        x.Sock.Send(Others);
                }
            }
            #endregion
        }
    }
}
