using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoLoginServer
{
    class MainClass
    {
        public enum ServerState : byte
        {
            Inspecting = 0,
            ConditionWell = 1,
            Normal = 2,
            VeryCrowded = 3,
            ExtremelyCrowded = 4
        }

        public static Listener ListenSock = new Listener( );

        public static void InitClass( )
        {
            ListenSock.Connected += Connected;
            ListenSock.Listen(11299);
        }

        public static void Connected(Server sender)
        {
            sender.PacketReceived += new Server.PacketEventHandler((Server _sender, Packet packet) =>
            {
                Program.frm1.AddLog("(C - S)  " + packet.ToString( ));
                PacketReceived(_sender, packet);
            });

            sender.PacketSent += new Server.PacketEventHandler((Server _sender, Packet packet) =>
            {
                Program.frm1.AddLog("(S - C)  " + packet.ToString( ));
            });
        }

        public static void PacketReceived(Server sender, Packet packet)
        {
            switch (packet.Opcode)
            {
                case 0x7FD3: // Login Request                    
                {
                    string User = packet.ReadString(65);
                    string Pass = packet.ReadString(65);

                    LoginState State = AccountControl.GetLoginState(User, Pass);
                    switch (State)
                    {
                        case LoginState.Good:
                        {
                            Packet LoginResponse = new Packet(0x7FDA);
                            LoginResponse.WriteString(User.ToUpper( ), 31);
                            LoginResponse.WriteUInt(1); //Servers Count
                            LoginResponse.WriteString("abcdefghi", 11);

                            LoginResponse.WriteUShort(1);
                            LoginResponse.WriteString("Playing Server", 17);
                            LoginResponse.WriteString("", 23);
                            LoginResponse.WriteString("127.0.0.1", 16);
                            LoginResponse.WriteUShort(11305);
                            LoginResponse.WriteUShort(0);
                            LoginResponse.WriteByte((byte)ServerState.ConditionWell);
                            sender.Send(LoginResponse);
                        }
                        break;

                        case LoginState.NotID:
                        {
                            Packet LoginError = new Packet(0x7FD9);
                            LoginError.WriteUInt((uint)LoginState.NotID);
                            LoginError.WriteString("", 200);
                            LoginError.WriteString("", 200);
                            sender.Send(LoginError);
                            sender.Sock.Disconnect(true);
                        }
                        break;

                        case LoginState.WrongPass:
                        {
                            Packet LoginError = new Packet(0x7FD9);
                            LoginError.WriteUInt((uint)LoginState.WrongPass);
                            LoginError.WriteString("", 200);
                            LoginError.WriteString("", 200);
                            sender.Send(LoginError);
                            sender.Sock.Disconnect(true);
                        }
                        break;
                    }
                }
                break;
            }
        }
    }
}
