using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    public enum RoomMode
    {
        DeathMatch = 0,
        BossMatch = 1,
        CaptureTheFlag = 2,
        CaptureTheSpot = 3,
        Survival = 4
    }

    public class PVPRoom
    {
        #region NextID
        private static Queue<uint> DeletedIDs = new Queue<uint>();

        private static uint _NextID = 0;
        public static uint NextID
        {
            get
            {
                if (DeletedIDs.Count() > 0)
                    return DeletedIDs.Dequeue();
                return _NextID++;
            }
        }
        #endregion

        public uint ID = 0;
        public string Name = "";
        public string Password = "";
        public RoomMode Mode = RoomMode.DeathMatch;
        public ushort Map = 24;
        public byte MaxParticipant = 2;
        public byte Time = 3;
        public bool Item = true;
        public bool Observer = true;
        public bool isInGame = false;

        public List<Player> Players = new List<Player>();

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);

            if (Players.Count == 0)
            {
                PVP.RemoveRoom(this);
            }
        }
    }

    class PVP
    {
        #region NextID
        private static Queue<uint> DeletedIDs = new Queue<uint>();

        private static uint _NextID = 0;
        public static uint NextID
        {
            get
            {
                if (DeletedIDs.Count() > 0)
                    return DeletedIDs.Dequeue();
                return _NextID++;
            }
        }
        #endregion

        public static List<Player> Players = new List<Player>();
        public static List<PVPRoom> Rooms = new List<PVPRoom>();

        public static void AddPlayer(Player player)
        {
            Players.Add(player);

            foreach (Player x in Players)
            {
                Packet AddPacket = new Packet(0x805);
                AddPacket.WriteString(player.CharName, 17);
                x.Sock.Send(AddPacket);
            }
        }

        public static void RemovePlayer(Player player)
        {
            foreach (Player x in Players)
            {
                Packet RemovePacket = new Packet(0x806);
                RemovePacket.WriteString(player.CharName, 17);
                x.Sock.Send(RemovePacket);
            }

            Players.Remove(player);
        }

        public static void AddRoom(PVPRoom room)
        {
            Rooms.Add(room);

            #region Packet
            foreach (Player x in Players)
            {
                ushort OthersInfo = (ushort)((room.Password == "" ? 0 : 1) +
                    ((int)room.Mode << 1) +
                    (room.Players.Count << 7) +
                    (room.MaxParticipant << 11) +
                    ((room.isInGame ? 1 : 0) << 15));


                Packet Others = new Packet(0x0810);
                Others.WriteUInt(room.ID);
                Others.WriteString(room.Name, 40);
                Others.WriteUShort(OthersInfo);
                x.Sock.Send(Others);
            }
            #endregion
        }

        public static void RemoveRoom(PVPRoom room)
        {
            #region Packet
            foreach (Player x in Players)
            {
                Packet Others = new Packet(0x0811);
                Others.WriteUInt(room.ID);
                x.Sock.Send(Others);
            }
            #endregion

            Rooms.Remove(room);
        }

        public static void Open(Packet packet, Player player)
        {
            if (packet.ReadByte() == 1)
            {
                player.CharData.isInPVP = true;
                AddPlayer(player);

                int MapIndex = Maps.MapsData.Find(player.CharData.Map);
                if (MapIndex != -1)
                    Maps.RemovePlayer(player, Maps.MapsData[MapIndex]);

                #region Packet
                Packet PVPInfo = new Packet(0x0804);

                PVPInfo.WriteUShort((ushort)Rooms.Count);
                PVPInfo.WriteUShort((ushort)Players.Count);

                foreach (PVPRoom x in Rooms)
                {

                    ushort Info = (ushort)((x.Password == "" ? 0 : 1) +
                        ((int)x.Mode << 1) +
                        (x.Players.Count << 7) +
                        (x.MaxParticipant << 11) +
                        ((x.isInGame ? 1 : 0) << 15));

                    PVPInfo.WriteUInt(x.ID);
                    PVPInfo.WriteString(x.Name, 40);
                    PVPInfo.WriteUShort(Info);
                    PVPInfo.WriteByte(1);
                }

                foreach (Player x in Players)
                    PVPInfo.WriteString(x.CharName, 17);

                player.Sock.Send(PVPInfo);
                #endregion
            }
        }

        public static void Close(Packet packet, Player player)
        {
            player.CharData.isInPVP = false;
            RemovePlayer(player);

            #region Packet
            Packet Teleporting = new Packet(0x0132);
            Teleporting.WriteInt(Data.GetGameCoord(player.CharData.Map, player.CharData.Coord));
            Teleporting.WriteUShort(player.CharData.Map);
            Teleporting.WriteByte(1);
            player.Sock.Send(Teleporting);
            #endregion
        }

        public static void CreateRoom(Packet packet, Player player)
        {
            PVPRoom newRoom = new PVPRoom();

            newRoom.Name = packet.ReadString(40);
            newRoom.Password = packet.ReadString(10);
            ushort Info = packet.ReadUShort();
            newRoom.Map = packet.ReadUShort();
            newRoom.Observer = packet.ReadByte() != 0;

            // HasPassword is [((Info >> 0) & 0x01) != 0]
            newRoom.Mode = (RoomMode)((Info >> 1) & 0x1F);
            newRoom.MaxParticipant = (byte)((Info >> 6) & 0x0F);
            newRoom.Time = (byte)((Info >> 10) & 0x1F);
            newRoom.Item = ((Info >> 15) & 0x01) != 0;

            newRoom.ID = NextID;

            newRoom.AddPlayer(player);
            player.CharData.PVPRoom = newRoom;
            PVP.RemovePlayer(player);

            AddRoom(newRoom);

            #region Response
            Packet Response = new Packet(0x0809);
            Response.WriteByte(1);
            Response.WriteUShort((ushort)newRoom.ID);
            player.Sock.Send(Response);
            #endregion
        }

        public static void ExitRoom(Packet packet, Player player)
        {
            uint ID = packet.ReadUInt();

            player.CharData.PVPRoom.RemovePlayer(player);
            player.CharData.PVPRoom = null;
            PVP.AddPlayer(player);

            #region Packet
            Packet PVPInfo = new Packet(0x0804);

            PVPInfo.WriteUShort((ushort)Rooms.Count);
            PVPInfo.WriteUShort((ushort)Players.Count);

            foreach (PVPRoom x in Rooms)
            {
                ushort Info = (ushort)((x.Password == "" ? 0 : 1) +
                        ((int)x.Mode << 1) +
                        (x.Players.Count << 7) +
                        (x.MaxParticipant << 11) +
                        ((x.isInGame ? 1 : 0) << 15));

                PVPInfo.WriteUInt(x.ID);
                PVPInfo.WriteString(x.Name, 40);
                PVPInfo.WriteUShort(Info);
                PVPInfo.WriteByte(1);
            }

            foreach (Player x in Players)
                PVPInfo.WriteString(x.CharName, 17);

            player.Sock.Send(PVPInfo);
            #endregion
        }
    }
}
