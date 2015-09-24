using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer.Packets
{
    class ItemControl
    {
        public static void TakeOff(Packet packet, Player player)
        {
            uint ItemID = packet.ReadUInt( );
            byte Slot = packet.ReadByte( );

            byte ClothesSlot = 0;
            byte Error = 1;

            #region Parse
            CharItem Item = player.CharData.ClothesItems.FindID(ItemID);
            if (Item.Slot != -1)
            {
                ClothesSlot = (byte)Item.Slot;
                player.CharData.ClothesItems.Remove(Item);
                Item.Slot = Slot;
                player.CharData.GeneralItems.Add(Item);
            }
            else
            {
                Error = 2;
            }
            #endregion

            #region Response
            Packet Response = new Packet(0x0416);
            Response.WriteUInt(ItemID);
            Response.WriteByte(Slot);
            Response.WriteByte(Error);
            Response.WriteUInt(10);
            Response.WriteUShort(20);
            Response.WriteUShort(player.CharData.PhysicalDef); // Pyshical Defense
            Response.WriteUShort(player.CharData.MagicalDef); // Magical Defense
            Response.WriteByte(30);
            Response.WriteUShort(player.CharData.AbbillityMin); // AbbillityMin
            Response.WriteUShort(player.CharData.AbbillityMax); // AbbillityMax
            Response.WriteByte(40);
            Response.WriteByte(50);
            Response.WriteUShort(player.CharData.Vitality); // Vitality
            Response.WriteUShort(player.CharData.Sympathy); // Sympathy
            Response.WriteUShort(player.CharData.Intelligence); // Intelligence
            Response.WriteUShort(60);
            Response.WriteUShort(player.CharData.Dexterity); // Dexterity

            Response.WriteUInt(player.CharData.MaxHP); // Max HP
            Response.WriteUInt(70);
            Response.WriteUInt(player.CharData.MaxMP); // Max MP
            Response.WriteByte(80);
            player.Sock.Send(Response);
            #endregion

            #region Others
            if (Error == 1)
            {
                Packet Others = new Packet(0x0417);
                Others.WriteUInt(player.ID);
                Others.WriteByte(ClothesSlot);

                int MapIndex = Maps.MapsData.Find(player.CharData.Map);
                if (MapIndex != -1)
                {
                    foreach (Player x in Maps.MapsData[MapIndex].Players)
                    {
                        if (x.ID != player.ID)
                            x.Sock.Send(Others);
                    }
                }
            }
            #endregion
        }

        public static void Equip(Packet packet, Player player)
        {
            uint ItemID = packet.ReadUInt( );
            byte Slot = packet.ReadByte( );

            byte Error = 1;

            #region Parse
            CharItem Item = player.CharData.GeneralItems.FindID(ItemID);
            CharItem ClothesItem = player.CharData.ClothesItems.FindSlot(Slot);
            if (ClothesItem.Slot == -1)
            {
                if (Item.Slot != -1)
                {
                    player.CharData.GeneralItems.Remove(Item);
                    Item.Slot = Slot;
                    player.CharData.ClothesItems.Add(Item);
                }
                else
                {
                    Item = player.CharData.RidingItems.FindID(ItemID);
                    if (Item.Slot != -1)
                    {
                        player.CharData.RidingItems.Remove(Item);
                        Item.Slot = Slot;
                        player.CharData.ClothesItems.Add(Item);
                    }
                    else
                    {
                        Error = 2;
                    }
                }
            }
            else
            {
                if (Item.Slot != -1)
                {
                    player.CharData.GeneralItems.Remove(Item);
                    player.CharData.GeneralItems.Add(ClothesItem);
                    player.CharData.GeneralItems.Last( ).Slot = Item.Slot;

                    player.CharData.ClothesItems.Remove(ClothesItem);
                    player.CharData.ClothesItems.Add(Item);
                    player.CharData.ClothesItems.Last( ).Slot = Slot;
                }
                else
                {
                    Item = player.CharData.RidingItems.FindID(ItemID);
                    if (Item.Slot != -1)
                    {
                        player.CharData.RidingItems.Remove(Item);
                        player.CharData.RidingItems.Add(ClothesItem);
                        player.CharData.RidingItems.Last( ).Slot = Item.Slot;

                        player.CharData.ClothesItems.Remove(ClothesItem);
                        player.CharData.ClothesItems.Add(Item);
                        player.CharData.ClothesItems.Last( ).Slot = Slot;
                    }
                    else
                    {
                        Error = 2;
                    }
                }
            }
            #endregion

            if (ClothesItem.Slot == -1)
            {
                #region Response (Equip)
                Packet Response = new Packet(0x0412);
                Response.WriteUInt(ItemID);
                Response.WriteByte(Slot);
                Response.WriteByte(Error);
                Response.WriteUInt(10);
                Response.WriteUShort(20);
                Response.WriteUShort(player.CharData.PhysicalDef); // Pyshical Defense
                Response.WriteUShort(player.CharData.MagicalDef); // Magical Defense
                Response.WriteByte(30);
                Response.WriteUShort(player.CharData.AbbillityMin); // AbbillityMin
                Response.WriteUShort(player.CharData.AbbillityMax); // AbbillityMax
                Response.WriteByte(40);
                Response.WriteByte(50);
                Response.WriteUShort(player.CharData.Vitality); // Vitality
                Response.WriteUShort(player.CharData.Sympathy); // Sympathy
                Response.WriteUShort(player.CharData.Intelligence); // Intelligence
                Response.WriteUShort(60);
                Response.WriteUShort(player.CharData.Dexterity); // Dexterity

                Response.WriteUInt(player.CharData.MaxHP); // Max HP
                Response.WriteUInt(70);
                Response.WriteUInt(player.CharData.MaxMP); // Max MP
                Response.WriteByte(80);
                player.Sock.Send(Response);
                #endregion
            }
            else
            {
                #region Response (Change Equip)
                Packet Response = new Packet(0x0413);
                Response.WriteUInt(ClothesItem.ID);
                Response.WriteUInt(Item.ID);
                Response.WriteByte((byte)ClothesItem.Slot);
                Response.WriteByte((byte)Item.Slot);
                Response.WriteByte(Error);
                Response.WriteUInt(10);
                Response.WriteUShort(20);
                Response.WriteUShort(player.CharData.PhysicalDef); // Pyshical Defense
                Response.WriteUShort(player.CharData.MagicalDef); // Magical Defense
                Response.WriteByte(30);
                Response.WriteUShort(player.CharData.AbbillityMin); // AbbillityMin
                Response.WriteUShort(player.CharData.AbbillityMax); // AbbillityMax
                Response.WriteByte(40);
                Response.WriteByte(50);
                Response.WriteUShort(player.CharData.Vitality); // Vitality
                Response.WriteUShort(player.CharData.Sympathy); // Sympathy
                Response.WriteUShort(player.CharData.Intelligence); // Intelligence
                Response.WriteUShort(60);
                Response.WriteUShort(player.CharData.Dexterity); // Dexterity

                Response.WriteUInt(player.CharData.MaxHP); // Max HP
                Response.WriteUInt(70);
                Response.WriteUInt(player.CharData.MaxMP); // Max MP
                Response.WriteByte(80);
                player.Sock.Send(Response);
                #endregion
            }

            #region Others
            if (Error == 1)
            {
                Packet Others = new Packet(0x0414);
                Others.WriteUInt(player.ID);
                Others.WriteUShort(Item.Model);
                Others.WriteUShort(10);
                Others.WriteByte((byte)Item.Slot);

                int MapIndex = Maps.MapsData.Find(player.CharData.Map);
                if (MapIndex != -1)
                {
                    foreach (Player x in Maps.MapsData[MapIndex].Players)
                    {
                        if (x.ID != player.ID)
                            x.Sock.Send(Others);
                    }
                }
            }
            #endregion
        }

        public static void ChangeSlot(Packet packet, Player player)
        {
            uint ItmID1 = packet.ReadUInt( );
            uint ItmID2 = packet.ReadUInt( );
            byte Slot = packet.ReadByte( );
            byte Check = packet.ReadByte( );

            byte Error = 1;


            if (Check == 0x01)
            {
                #region Parse
                if (ItmID2 != 0)
                {
                    CharItem Itm1 = player.CharData.GeneralItems.FindID(ItmID1);
                    CharItem Itm2 = player.CharData.GeneralItems.FindID(ItmID2);
                    if (Itm1.Slot == -1 || Itm2.Slot == -1)
                    {
                        Error = 0;
                    }
                    else
                    {
                        Itm1.Slot += Itm2.Slot;
                        Itm2.Slot = Itm1.Slot - Itm2.Slot;
                        Itm1.Slot -= Itm2.Slot;
                    }
                }
                else
                {
                    CharItem Itm1 = player.CharData.GeneralItems.FindID(ItmID1);
                    if (Itm1.Slot == -1)
                    {
                        Error = 0;
                    }
                    else
                    {
                        Itm1.Slot = Slot;
                    }
                }
                #endregion

                #region Response
                Packet Response = new Packet(0x0454);
                Response.WriteUInt(ItmID1);
                Response.WriteUInt(ItmID2);
                Response.WriteByte(Slot);
                Response.WriteByte(1);
                Response.WriteByte(Error);
                player.Sock.Send(Response);
                #endregion
            }
        }
    }
}
