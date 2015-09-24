using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public static class Extenstions
    {
        public static int Find(this List<Map> Maps, ushort MapID)
        {
            for (int i = 0; i < Maps.Count; i++)
            {
                if (MapID == Maps[i].MapID)
                    return i;
            }
            return -1;
        }

        public static Point ReadPoint(this BinaryReader Reader)
        {
            return new Point(Reader.ReadInt32( ), Reader.ReadInt32( ));
        }

        public static void Write(this BinaryWriter Writer, Point value)
        {
            Writer.Write(value.X);
            Writer.Write(value.Y);
        }

        public static int Find(this List<Player> Players, string CharName)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].CharName == CharName)
                    return i;
            }

            return -1;
        }
    }

    public class MobRange
    {
        public Polygon RangePolygon = null;
        public List<ushort> Mobs = new List<ushort>( );
        public byte CurMobCount = 0;
        public byte MaxMobCount = 0;
        public ushort HeadMob = 0;
    }

    public class Map
    {
        #region NextID
        private Queue<uint> DeletedIDs = new Queue<uint>( );

        private uint _NextID = 0;
        public uint NextID
        {
            get
            {
                if (DeletedIDs.Count( ) > 0)
                    return DeletedIDs.Dequeue( );
                return _NextID++;
            }
        }
        #endregion

        public ushort MapID = 0;

        public List<Mob> Mobs = new List<Mob>( );
        public List<Player> Players = new List<Player>( );
        public List<Gate> Gates = new List<Gate>( );
        public List<NPC> NPCs = new List<NPC>( );
        public List<MobRange> MobRanges = new List<MobRange>( );

        public void NewMob(Mob New)
        {
            Mobs.Add(New);
            foreach (Player x in Players)
            {
                Packet packet = new Packet(0x5002);
                packet.WriteUInt(New.ID);
                packet.WriteUShort(New.Type);
                packet.WriteInt(Data.GetGameCoord(x.CharData.Map, New.Pos));
                packet.WriteUInt(10000); // HP
                packet.WriteString("", 9);
                packet.WriteFloat(New.Angle);
                packet.WriteString("", 100);
                x.Sock.Send(packet);
            }
        }
    }

    class Maps
    {
        public static List<Map> MapsData = new List<Map>( );

        public static void InitMaps( )
        {
            int Count = 0;
            string[] MapsFiles = Directory.GetFiles("Maps");
            foreach (string x in MapsFiles)
            {
                Map New = new Map( );
                BinaryReader Reader = new BinaryReader(File.Open(x, FileMode.Open));

                New.MapID = UInt16.Parse(Path.GetFileNameWithoutExtension(x));

                #region Gates
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    New.Gates.Add(new Gate( )
                    {
                        GatePos = Reader.ReadPoint( ),
                        DstPos = Reader.ReadPoint( ),
                        DstMap = Reader.ReadUInt16( ),
                        Conditions = Reader.ReadInt32( )
                    });
                }
                #endregion

                #region NPCs
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    New.NPCs.Add(new NPC(
                        Reader.ReadUInt16( ),
                        Reader.ReadPoint( ),
                        Reader.ReadSingle( )));
                }
                #endregion

                #region Ranges
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    MobRange NewRange = new MobRange( );

                    List<PointF> PtList = new List<PointF>( );
                    byte PointsCount = Reader.ReadByte( );
                    for (int a = 0; a < PointsCount; a++)
                    {
                        PtList.Add((PointF)Reader.ReadPoint( ));
                    }
                    NewRange.RangePolygon = new Polygon(PtList);

                    NewRange.MaxMobCount = Reader.ReadByte( );
                    byte MobsCount = Reader.ReadByte( );
                    for (int a = 0; a < MobsCount; a++)
                    {
                        NewRange.Mobs.Add(Reader.ReadUInt16( ));
                    }
                    NewRange.HeadMob = Reader.ReadUInt16( );

                    New.MobRanges.Add(NewRange);
                }
                #endregion

                MapsData.Add(New);

                Reader.Close( );
            }


            foreach(Map y in MapsData)
            {
                foreach (NPC x in y.NPCs)
                {
                    x.ID = y.NextID;
                }
            }
        }

        public static void AddPlayer(Player player,Map map)
        {
            foreach (NPC x in map.NPCs)
            {
                Packet packet = new Packet(0x5002);
                packet.WriteUInt(x.ID);
                packet.WriteUShort(x.Type);
                packet.WriteInt(Data.GetGameCoord(player.CharData.Map, x.Pos));
                packet.WriteUInt(10000); // HP
                packet.WriteByte(1);
                packet.WriteUShort(x.Type);
                packet.WriteByte(1);
                packet.WriteByte(1);
                packet.WriteUInt(1);
                packet.WriteFloat(x.Angle);
                packet.WriteByte(5);
                packet.WriteUInt(1001);
                packet.WriteFloat(x.Angle);
                packet.WriteString("", 50);
                packet.WriteFloat(x.Angle);
                packet.WriteString("", 100);
                player.Sock.Send(packet);
            }

            foreach (Mob x in map.Mobs)
            {
                Packet packet = new Packet(0x5002);
                packet.WriteUInt(x.ID);
                packet.WriteUShort(x.Type);
                packet.WriteInt(Data.GetGameCoord(player.CharData.Map, x.Pos));
                packet.WriteUInt(10000); // HP
                packet.WriteString("ABCDEFGHI", 9);
                packet.WriteFloat(x.Angle);
                packet.WriteString("", 100);
                player.Sock.Send(packet);
            }

            foreach (Player x in map.Players)
            {
                #region FromOther
                Packet packet = new Packet(0x000F);
                packet.WriteUInt(x.ID);
                packet.WriteString(x.CharData.Name, 17);
                packet.WriteInt(Data.GetStyle(x.CharData.Gender, x.CharData.Nation, x.CharData.Face, x.CharData.Hair));
                packet.WriteByte((byte)x.CharData.Job);
                packet.WriteByte(0);
                packet.WriteUInt(0);
                packet.WriteByte(x.CharData.Level); // Level
                packet.WriteInt(Data.GetGameCoord(x.CharData.Map, x.CharData.Coord));
                packet.WriteUInt(x.CharData.CurHP); // CurHP
                packet.WriteUInt(x.CharData.CurMP); // CurMP
                packet.WriteUInt(x.CharData.MaxHP); // MaxHP
                packet.WriteUInt(x.CharData.MaxMP); // MaxMP
                packet.WriteByte(0);
                packet.WriteUShort(1); // Speed
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(0).Model); // top
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(1).Model); // bottom
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(2).Model);// right hand weapon
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(3).Model);// left hand weapon
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(4).Model); // hat
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(5).Model); // wing & suit
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(6).Model); // Gauntlet
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(7).Model); // boot
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(8).Model); // 1st Necklace
                packet.WriteUInt(x.CharData.ClothesItems.FindSlot(9).Model); // 2nd Necklace
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                for (int i = 0; i < 20; i++) // Buffs
                {
                    packet.WriteUShort(0);
                }
                packet.WriteUShort(0); // State
                packet.WriteUInt(0);
                packet.WriteByte(0);
                packet.WriteString("", 100);
                player.Sock.Send(packet);
                #endregion

                #region ToOther
                packet = new Packet(0x000F);
                packet.WriteUInt(player.ID);
                packet.WriteString(player.CharData.Name, 17);
                packet.WriteInt(Data.GetStyle(player.CharData.Gender, player.CharData.Nation, player.CharData.Face, player.CharData.Hair));
                packet.WriteByte((byte)player.CharData.Job);
                packet.WriteByte(0);
                packet.WriteUInt(0);
                packet.WriteByte(player.CharData.Level); // Level
                packet.WriteInt(Data.GetGameCoord(player.CharData.Map, player.CharData.Coord));
                packet.WriteUInt(player.CharData.CurHP); // CurHP
                packet.WriteUInt(player.CharData.CurMP); // CurMP
                packet.WriteUInt(player.CharData.MaxHP); // MaxHP
                packet.WriteUInt(player.CharData.MaxMP); // MaxMP
                packet.WriteByte(0);
                packet.WriteUShort(1); // Speed
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(0).Model); // top
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(1).Model); // bottom
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(2).Model);// right hand weapon
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(3).Model);// left hand weapon
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(4).Model); // hat
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(5).Model); // wing & suit
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(6).Model); // Gauntlet
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(7).Model); // boot
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(8).Model); // 1st Necklace
                packet.WriteUInt(player.CharData.ClothesItems.FindSlot(9).Model); // 2nd Necklace
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                for (int i = 0; i < 20; i++) // Buffs
                {
                    packet.WriteUShort(0);
                }
                packet.WriteUShort(0); // State
                packet.WriteUInt(0);
                packet.WriteByte(0);
                packet.WriteString("", 100);
                x.Sock.Send(packet);
                #endregion
            }

            map.Players.Add(player);
        }

        public static void RemovePlayer(Player player, Map map)
        {
            Packet Disappear = new Packet(0x000E);
            Disappear.WriteUInt(player.ID);
            Disappear.WriteByte(1);

            foreach (Player x in map.Players)
            {
                if (x.ID != player.ID)
                    x.Sock.Send(Disappear);
            }

            map.Players.Remove(player);
        }
    }
}
