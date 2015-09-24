using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public static class Extenstion
    {
        public static CharItem FindSlot(this List<CharItem> Items, int Slot)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Slot == Slot)
                    return Items[i];
            }
            return new CharItem( )
            {
                Slot = -1
            };
        }

        public static CharItem FindID(this List<CharItem> Items, uint ID)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ID == ID)
                    return Items[i];
            }
            return new CharItem( )
            {
                Slot = -1
            };
        }
    }

    public class CharItem
    {
        public uint ID = 0;
        public ushort Model = 0;
        public int Slot = 0;
        public ushort Durabillty = 0;
        public ushort RemainTime = 0;
    }

    public class CharData
    {
        private string FileName = "";

        public Player PlayerClass = null;

        public bool isInPVP = false;
        public Packets.PVPRoom PVPRoom = null;

        public ushort MovingSpeed = 192;
        public bool isOnHorse = false;

        #region MainData
        #region CharData
        public string Name = "";
        public CharGender Gender = CharGender.Female;
        public CharNation Nation = CharNation.Rain;
        public CharJob Job = CharJob.Magician;
        public ulong EXP = 0;
        public byte Level = 0;
        public uint Fame = 0;
        public uint NationRate = 10000;
        public byte Face = 0;
        public byte Hair = 0;

        public ushort Map = 0;
        private Point _Coord = new Point( );
        public Point Coord
        {
            get
            {
                return _Coord;
            }

            set
            {
                _Coord = value;
                Gate CurGate = Data.inGate(Map, _Coord);
                if (CurGate != null)
                {
                    ushort LastMap = this.Map;
                    Map = CurGate.DstMap;
                    Coord = CurGate.DstPos;

                    Packet Teleporting = new Packet(0x0132);
                    Teleporting.WriteInt(Data.GetGameCoord(Map, Coord));
                    Teleporting.WriteUShort(Map);
                    Teleporting.WriteByte(1);
                    PlayerClass.Sock.Send(Teleporting);

                    int MapIndex = Maps.MapsData.Find(LastMap);
                    if (MapIndex != -1)
                        Maps.RemovePlayer(PlayerClass, Maps.MapsData[MapIndex]);
                }
            }
        }

        public uint CurHP = 0;
        public uint CurSP = 0;
        public uint CurMP = 0;
        public uint MaxHP = 0;
        public uint MaxSP = 0;
        public uint MaxMP = 0;
        public ushort Power = 0;
        public ushort PhysicalDef = 0;
        public ushort MagicalDef = 0;
        public ushort AbbillityMin = 0;
        public ushort AbbillityMax = 0;
        public ushort Vitality = 0;
        public ushort Sympathy = 0;
        public ushort Stamina = 0;
        public ushort Intelligence = 0;
        public ushort Dexterity = 0;
        public ushort AbilltyPoint = 0;
        public byte Charisma = 0;
        public byte Luck = 0;
        public ushort LeftSP = 0;
        public ushort TotalSP = 100;
        public uint WonPVPs = 0;
        public uint TotalPVPs = 0;
        public uint Gold = 0;
        #endregion

        public List<ushort> Skills = new List<ushort>( );
        public List<CharItem> ClothesItems = new List<CharItem>( );
        public List<CharItem> GeneralItems = new List<CharItem>( );
        public List<CharItem> RidingItems = new List<CharItem>( );
        #endregion

        public CharData(Player PlayerClass, string Account, string Character)
        {
            int Count = 0;
            this.PlayerClass = PlayerClass;

            FileName = "Accounts\\" + Account + "\\" + Character;
            if (!File.Exists(FileName))
                return;

            byte[] Bytes = File.ReadAllBytes(FileName);
            Stream Stream = new MemoryStream(Bytes);
            BinaryReader Reader = new BinaryReader(Stream);

            #region CharData
            {
                Name = Reader.ReadString( );
                Gender = (CharGender)Reader.ReadByte( );
                Nation = (CharNation)Reader.ReadByte( );
                Job = (CharJob)Reader.ReadByte( );
                EXP = Reader.ReadUInt64( );
                Level = Reader.ReadByte( );
                Fame = Reader.ReadUInt32( );
                NationRate = Reader.ReadUInt32( );

                Face = Reader.ReadByte( );
                Hair = Reader.ReadByte( );

                Map = Reader.ReadUInt16( );
                Coord = Reader.ReadPoint( );

                CurHP = Reader.ReadUInt32( );
                CurSP = Reader.ReadUInt32();
                CurMP = Reader.ReadUInt32( );
                MaxHP = Reader.ReadUInt32( );
                MaxSP = Reader.ReadUInt32();
                MaxMP = Reader.ReadUInt32( );

                Power = Reader.ReadUInt16();
                PhysicalDef = Reader.ReadUInt16( );
                MagicalDef = Reader.ReadUInt16( );
                AbbillityMin = Reader.ReadUInt16( );
                AbbillityMax = Reader.ReadUInt16( );
                Vitality = Reader.ReadUInt16( );
                Sympathy = Reader.ReadUInt16( );
                Stamina = Reader.ReadUInt16();
                Intelligence = Reader.ReadUInt16( );
                Dexterity = Reader.ReadUInt16( );
                AbilltyPoint = Reader.ReadUInt16( );
                Charisma = Reader.ReadByte( );
                Luck = Reader.ReadByte( );

                LeftSP = Reader.ReadUInt16();
                TotalSP = Reader.ReadUInt16();

                WonPVPs = Reader.ReadUInt32( );
                TotalPVPs = Reader.ReadUInt32( );
                Gold = Reader.ReadUInt32( );
            }
            #endregion

            #region Skills
            {
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    Skills.Add(Reader.ReadUInt16( ));
                }
            }
            #endregion

            #region ClothesItems
            {
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    ClothesItems.Add(new CharItem( )
                    {
                        Model = Reader.ReadUInt16( ),
                        Slot = Reader.ReadInt32( ),
                        Durabillty = Reader.ReadUInt16( ),
                        RemainTime = Reader.ReadUInt16( )
                    });
                }
            }
            #endregion

            #region GeneralItems
            {
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    GeneralItems.Add(new CharItem( )
                    {
                        Model = Reader.ReadUInt16( ),
                        Slot = Reader.ReadInt32( ),
                        Durabillty = Reader.ReadUInt16( ),
                        RemainTime = Reader.ReadUInt16( )
                    });
                }
            }
            #endregion

            #region RidingItems
            {
                Count = Reader.ReadInt32( );
                for (int i = 0; i < Count; i++)
                {
                    RidingItems.Add(new CharItem( )
                    {
                        Model = Reader.ReadUInt16( ),
                        Slot = Reader.ReadInt32( ),
                        Durabillty = Reader.ReadUInt16( ),
                        RemainTime = Reader.ReadUInt16( )
                    });
                }
            }
            #endregion

            #region InitItems
            {
                uint ID = 0;
                foreach (CharItem x in ClothesItems)
                {
                    x.ID = ++ID;
                }

                foreach (CharItem x in GeneralItems)
                {
                    x.ID = ++ID;
                }
            }
            #endregion

            Reader.Close( );
        }

        public void Save( )
        {
            if (File.Exists(FileName))
                Save(FileName);
        }

        public void Save(string FileName)
        {
            Stream Stream = File.Open(FileName, FileMode.OpenOrCreate);
            BinaryWriter Writer = new BinaryWriter(Stream);

            #region CharData
            {
                Writer.Write(Name);
                Writer.Write((byte)Gender);
                Writer.Write((byte)Nation);
                Writer.Write((byte)Job);
                Writer.Write(EXP);
                Writer.Write(Level);
                Writer.Write(Fame);
                Writer.Write(NationRate);

                Writer.Write(Face);
                Writer.Write(Hair);

                Writer.Write(Map);
                Writer.Write(Coord);

                Writer.Write(CurHP);
                Writer.Write(CurSP);
                Writer.Write(CurMP);
                Writer.Write(MaxHP);
                Writer.Write(MaxSP);
                Writer.Write(MaxMP);

                Writer.Write(Power);
                Writer.Write(PhysicalDef);
                Writer.Write(MagicalDef);
                Writer.Write(AbbillityMin);
                Writer.Write(AbbillityMax);
                Writer.Write(Vitality);
                Writer.Write(Sympathy);
                Writer.Write(Stamina);
                Writer.Write(Intelligence);
                Writer.Write(Dexterity);
                Writer.Write(AbilltyPoint);
                Writer.Write(Charisma);
                Writer.Write(Luck);

                Writer.Write(LeftSP);
                Writer.Write(TotalSP);

                Writer.Write(WonPVPs);
                Writer.Write(TotalPVPs);
                Writer.Write(Gold);
            }
            #endregion

            #region Skills
            {
                Writer.Write(Skills.Count);
                foreach (ushort ID in Skills)
                {
                    Writer.Write(ID);
                }
            }
            #endregion

            #region ClothesItems
            {
                Writer.Write(ClothesItems.Count);
                foreach (CharItem x in ClothesItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            #region GeneralItems
            {
                Writer.Write(GeneralItems.Count);
                foreach (CharItem x in GeneralItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            #region RidingItems
            {
                Writer.Write(RidingItems.Count);
                foreach (CharItem x in RidingItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            Writer.Close( );
        }
        public void CreateCharData()
        {
            Stream Stream = File.Open(FileName, FileMode.OpenOrCreate);
            BinaryWriter Writer = new BinaryWriter(Stream);

            #region CharData
            {
                Writer.Write(Name);
                Writer.Write((byte)Gender);
                Writer.Write((byte)Nation);
                Job = (byte)Nation == 1 ? (CharJob)1 : (CharJob)0;
                Writer.Write((byte)Job);
                Writer.Write(EXP);
                Writer.Write((byte)1);//Level
                Writer.Write(Fame);
                Writer.Write((uint)0);//NationRate

                Writer.Write(Face);
                Writer.Write(Hair);

                Map = (byte)Nation == 1 ? (ushort)7 : (ushort)5;
                Writer.Write(Map);

                _Coord = (byte)Nation == 1 ? new Point(221, 222) : new Point(130, 121);
                Writer.Write(_Coord);//coord

                Writer.Write((uint)150);//CurHP
                Writer.Write((uint)150);//CurSP
                Writer.Write((uint)150);//CurMP
                Writer.Write((uint)150);//MaxHP
                Writer.Write((uint)150);//MaxSP
                Writer.Write((uint)150);//MaxMP

                Writer.Write((ushort)13);//Power

                PhysicalDef = Nation == CharNation.Millena ? (ushort)5 : (ushort)2;
                MagicalDef = Nation == CharNation.Millena ? (ushort)2 : (ushort)5;
                Writer.Write(PhysicalDef);//PhysicalDef
                Writer.Write(MagicalDef);//MagicalDef

                Writer.Write((ushort)10);//AbbillityMin
                Writer.Write((ushort)12);//AbbillityMax
                Writer.Write((ushort)9);//Vitality
                Writer.Write((ushort)13);//Sympathy
                Writer.Write((ushort)12);//Stamina
                Writer.Write((ushort)12);//Intelligence
                Writer.Write((ushort)13);//Dexterity
                Writer.Write((ushort)0);//AbilltyPoint
                Writer.Write((byte)0);//Charisma
                Writer.Write(Luck);

                Writer.Write(LeftSP);
                Writer.Write(TotalSP);

                Writer.Write(WonPVPs);
                Writer.Write(TotalPVPs);
                Writer.Write(Gold);
            }
            #endregion

            #region Skills
            {
                Writer.Write(Skills.Count);
                foreach (ushort ID in Skills)
                {
                    Writer.Write(ID);
                }
            }
            #endregion

            #region ClothesItems
            {
                Writer.Write(ClothesItems.Count);
                foreach (CharItem x in ClothesItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            #region GeneralItems
            {
                Writer.Write(GeneralItems.Count);
                foreach (CharItem x in GeneralItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            #region RidingItems
            {
                Writer.Write(RidingItems.Count);
                foreach (CharItem x in RidingItems)
                {
                    Writer.Write(x.Model);
                    Writer.Write(x.Slot);
                    Writer.Write(x.Durabillty);
                    Writer.Write(x.RemainTime);
                }
            }
            #endregion

            Writer.Close();
        }
    }
}
