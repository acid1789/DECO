using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class Movement
    {
        public static void BeginMove(Packet packet, Player player)
        {
            int GameCoord = packet.ReadInt( );

            Point Pos = Data.GetReallyCoord(player.CharData.Map, GameCoord);
            NPC NPCData = NPCs.IsNPC(player, Pos);

            if (NPCData != null)
            {
                #region Response (NPC)
                Packet Response = new Packet(0x0252);
                Response.WriteByte(1);
                Response.WriteUShort(NPCData.Type);
                player.Sock.Send(Response);
                #endregion
            }
            else
            {
                #region Response (Move)
                Packet Response = new Packet(0x0122);
                Response.WriteInt(GameCoord); // Coord
                Response.WriteUShort(player.CharData.MovingSpeed); // Speed
                Response.WriteByte(1); // Flag
                player.Sock.Send(Response);
                #endregion

                #region Others
                Packet Others = new Packet(0x0123);
                Others.WriteULong(player.ID);
                Others.WriteInt(GameCoord);
                Others.WriteUShort(1); // Speed

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

        public static void UpdatePosition(Packet packet, Player player)
        {
            int GameCoord = packet.ReadInt( );

            player.CharData.Coord = Data.GetReallyCoord(player.CharData.Map, GameCoord);
        }

        public static void RideHorse(Packet packet, Player player)
        {
            Packet Response = new Packet(0x0472);
            Response.WriteByte(1);
            Response.WriteUInt(1);
            Response.WriteUInt(1);
            player.Sock.Send(Response);

            int MapIndex = Maps.MapsData.Find(player.CharData.Map);
            if (MapIndex != -1)
            {
                foreach (Player x in Maps.MapsData[MapIndex].Players)
                {
                    if (x.ID == player.ID)
                        continue;

                    Packet Others = new Packet(0x0472);
                    Others.WriteByte(1);
                    Others.WriteUInt(0);
                    Others.WriteUInt(player.ID);
                    x.Sock.Send(Response);
                }
            }
        }
    }
}
