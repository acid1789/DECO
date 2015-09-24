
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace DecoPlayServer
{
    class MainClass
    {
        #region NextID
        private static Queue<uint> DeletedIDs = new Queue<uint>( );

        private static uint _NextID = 2;
        public static uint NextID
        {
            get
            {
                if (DeletedIDs.Count( ) > 0)
                    return DeletedIDs.Dequeue( );
                return _NextID++;
            }
        }
        #endregion

        public static Listener ListenSock = new Listener( );
        public static List<Player> Players = new List<Player>( );

        public static void InitServer( )
        {
            Maps.InitMaps( );
            ListenSock.Connected += Connected;
            ListenSock.Listen(11305);
        }

        public static void Disconnected(Server sender)
        {
            Player x = Players.Find(sender);

            if (x.CharData != null)
            {
                if (x.CharData.isInPVP)
                {
                    Packets.PVP.RemovePlayer(x);
                }
                else
                {
                    int MapIndex = Maps.MapsData.Find(x.CharData.Map);
                    if (MapIndex != -1)
                        Maps.RemovePlayer(x, Maps.MapsData[MapIndex]);
                }

                x.CharData.Save( );
            }

            DeletedIDs.Enqueue(x.ID);
            Players.Remove(x);
        }

        public static void Connected(Server sender)
        {
            sender.Disconnected += Disconnected;
            sender.PacketReceived += PacketReceived;
            sender.PacketSent += PacketSent;
            Players.Add(new Player(sender, NextID));
        }

        public static void PacketSent(Server sender, Packet packet)
        {
            Player player = Players.Find(sender);

            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                    break;
                Thread.Sleep(100);
                player = Players.Find(sender);
            }

            if (player == null)
                sender.Kick( );

            if (player.CharName != "")
                Program.frm1.AddLog(player.CharName + " : (S - C)  " + packet.ToString( ));
            else
                Program.frm1.AddLog(player.AccountName + " : (S - C)  " + packet.ToString( ));
        }

        public static void PacketReceived(Server sender, Packet packet)
        {
            //Program.frm1.AddLog("(C - S)  " + packet.ToString());

            Player player = Players.Find(sender);
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                    break;
                Thread.Sleep(100);
                player = Players.Find(sender);
            }

            if (player == null)
                sender.Kick();

            if (player.CharName != "")
                Program.frm1.AddLog(player.CharName + " : (C - S)  " + packet.ToString());
            else
                Program.frm1.AddLog(player.AccountName + " : (C - S)  " + packet.ToString());

            switch (packet.Opcode)
            {
                case 0x7FD4: // Login Request
                    {
                        player.AccountName = packet.ReadString(31);

                        AccountData AccData = new AccountData(player.AccountName);

                        Packet LoginResponse = new Packet(0x7FD2);
                        LoginResponse.WriteString(player.AccountName, 31);
                        LoginResponse.WriteString("test", 31);
                        sender.Send(LoginResponse);

                        LoginResponse = new Packet(0x0001);
                        LoginResponse.WriteUInt(2);

                        int z = AccData.Characters.Count;

                        while (z < 2)
                        {
                            //3rd Char
                            LoginResponse.WriteUInt(10);
                            for (int i = 0; i < 128; i++)
                                LoginResponse.WriteByte(0);
                            z++;
                        }

                        foreach (CharData x in AccData.Characters)
                        {
                            LoginResponse.WriteUInt(1);
                            LoginResponse.WriteString(x.Name, 17);
                            LoginResponse.WriteInt(Data.GetStyle(x.Gender, x.Nation, x.Face, x.Hair));

                            LoginResponse.WriteByte((byte)x.Job);//Job
                            LoginResponse.WriteByte(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteByte(x.Level); // level 
                            LoginResponse.WriteUShort(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(0).Model); // top
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(1).Model); // bottom
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(2).Model);// right hand weapon
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(3).Model);// left hand weapon
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(4).Model); // hat
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(5).Model); // wing & suit
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(6).Model); // Gauntlet
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(7).Model); // boot
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(8).Model); // 1st Necklace
                            LoginResponse.WriteUInt(x.ClothesItems.FindSlot(9).Model); // 2nd Necklace
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUInt(0);
                            LoginResponse.WriteUShort(0);
                        }

                        sender.Send(LoginResponse);
                    }
                    break;

                case 0x0002:
                    {
                        packet.ReadInt();
                        string  CharName = packet.ReadString(17);
                        int nStyle = packet.ReadInt();

                        CharData class_CharData = new CharData(player, player.AccountName, CharName);
                        class_CharData.Name = CharName;
                        class_CharData.Nation = Data.GetCharNation(nStyle);
                        class_CharData.Gender = Data.GetCharGender(nStyle);
                        class_CharData.Hair = Data.GetCharHair(nStyle, class_CharData.Nation, class_CharData.Gender);
                        class_CharData.Face = Data.GetCharFace(nStyle, class_CharData.Nation, class_CharData.Gender);

                        class_CharData.CreateCharData();//save character data to file.

                        Packet LoginResponse = new Packet(0x0003);
                        sender.Send(LoginResponse);

                    }
                    break;

                case 0x0004:
                    {
                        string CharName = packet.ReadString(17);
                        string FileName = "Accounts\\" + player.AccountName + "\\" + CharName;
                        if (File.Exists(FileName))
                            File.Delete(FileName);

                        Packet LoginResponse = new Packet(0x0005);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        LoginResponse.WriteUInt(1);
                        sender.Send(LoginResponse);

                    }
                    break;

                case 0x0010:
                    {
                        player.CharName = packet.ReadString(17);
                        Packet LoginResponse = new Packet(0x0013);
                        LoginResponse.WriteUInt(1);
                        sender.Send(LoginResponse);
                    }
                    break;

                case 0x0014:
                    player.SendCharData();
                    break;

                case 0x0124:
                    Packets.Movement.UpdatePosition(packet, player);
                    break;

                case 0x0121:
                    Packets.Movement.BeginMove(packet, player);
                    break;

                case 0x0453:
                    Packets.ItemControl.ChangeSlot(packet, player);
                    break;

                case 0x0411:
                    Packets.ItemControl.Equip(packet, player);
                    break;

                case 0x0415:
                    Packets.ItemControl.TakeOff(packet, player);
                    break;

                case 0x0229:
                    Packets.Communication.Expressing(packet, player);
                    break;

                case 0x7E02:
                    Packets.Msngr.MemberInsert(packet, player);
                    break;

                case 0x0710:
                    Packets.Party.PartyRequest(packet, player);
                    break;

                case 0x0801:
                    Packets.PVP.Open(packet, player);
                    break;

                case 0x0807:
                    Packets.PVP.Close(packet, player);
                    break;

                case 0x0808:
                    Packets.PVP.CreateRoom(packet, player);
                    break;

                case 0x0816:
                    Packets.PVP.ExitRoom(packet, player);
                    break;

                case 0x0471:
                    Packets.Movement.RideHorse(packet, player);
                    break;

                case 0x0133:
                    {
                        int MapIndex = Maps.MapsData.Find(player.CharData.Map);
                        if (MapIndex != -1)
                            Maps.AddPlayer(player, Maps.MapsData[MapIndex]);
                    }
                    break;

                case 0x0222:
                    Packets.Chatting.Whisper(packet, player);
                    break;

                case 0x0220:
                    {
                        byte Length = packet.ReadByte();
                        string Text = packet.ReadString(Length);

                        if (!player.CharData.isInPVP)
                        {
                            int MapIndex = Maps.MapsData.Find(player.CharData.Map);
                            foreach (Player x in Maps.MapsData[MapIndex].Players)
                            {
                                Packet newPacket = new Packet(0x0221);
                                newPacket.WriteUInt(player.ID);
                                newPacket.WriteString(player.CharData.Name, 17);
                                newPacket.WriteByte(0);
                                newPacket.WriteByte(Length);
                                newPacket.WriteString(Text, Length);
                                x.Sock.Send(newPacket);
                            }
                        }
                        else
                        {
                            foreach (Player x in Packets.PVP.Players)
                            {
                                Packet newPacket = new Packet(0x0221);
                                newPacket.WriteUInt(player.ID);
                                newPacket.WriteString(player.CharData.Name, 17);
                                newPacket.WriteByte(63);
                                newPacket.WriteByte(Length);
                                newPacket.WriteString(Text, Length);
                                x.Sock.Send(newPacket);
                            }
                        }
                    }
                    break;

                #region DropEld
                case 0x0406:
                    {
                        byte Result = 0;

                        uint Dropped = packet.ReadUInt();
                        if (player.CharData.Gold >= Dropped)
                        {
                            player.CharData.Gold -= Dropped;
                            Result = 1;
                        }

                        Packet Response = new Packet(0x0407);
                        Response.WriteByte(Result);
                        Response.WriteUInt(player.CharData.Gold);
                        player.Sock.Send(Response);
                    }
                    break;
                #endregion
            }
        }
    }
}
