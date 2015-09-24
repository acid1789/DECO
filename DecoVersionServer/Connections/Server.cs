using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecoVersionServer
{
    public class Server
    {
        uint SendCounter = 0;
        PacketSecurity ScSec = new PacketSecurity(true);
        PacketSecurity CsSec = new PacketSecurity(false);

        #region Events
        public delegate void PacketEventHandler(Server sender, Packet packet);
        public event PacketEventHandler PacketReceived;
        public event PacketEventHandler PacketSent;

        public delegate void DisconnectedEventHandler(Server sender);
        public event DisconnectedEventHandler Disconnected;
        #endregion

        byte[] Buffer = new byte[8192];
        bool isClosing = false;

        public Socket Sock = null;

        public Server(Socket _Sock)
        {
            Sock = _Sock;
            Sock.BeginReceive(Buffer, 0, 8192, SocketFlags.None, new AsyncCallback(WaitForData), Sock);
            ScSec = new PacketSecurity(true);
            CsSec = new PacketSecurity(false);
        }

        private void WaitForData(IAsyncResult DataAsync)
        {
            if (!isClosing)
            {
                Socket Worker = null;
                try
                {
                    Worker = (Socket)DataAsync.AsyncState;
                    int rcvdBytes = Worker.EndReceive(DataAsync);
                    if (rcvdBytes > 0)
                    {
                        int Length = 0;

                        byte[] newBuffer = SubArray(Buffer, 0, BitConverter.ToUInt16(Buffer, 1));
                        Packet packet = new Packet(newBuffer, false, out Length);

                        //RaiseEvent if event is linked
                        if (PacketReceived != null)
                        {
                            PacketReceived(this, packet);
                        }
                    }
                    else
                    {
                        isClosing = true;
                        //RaisEvent
                        if (Disconnected != null)
                        {
                            Disconnected(this);
                        }
                    }
                } catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.ConnectionReset || se.SocketErrorCode == SocketError.InvalidArgument) //Client Disconnected
                    {
                        isClosing = true;
                        //RaisEvent
                        if (Disconnected != null)
                        {
                            Disconnected(this);
                        }

                        //Mark worker as null to stop reciveing.
                        Worker = null;
                    }
                    else
                    {

                    }
                } finally
                {
                    if (Worker != null)
                    {
                        try
                        {
                            Buffer = new byte[8192];
                            Worker.BeginReceive(Buffer, 0, 8192, SocketFlags.None, new AsyncCallback(WaitForData), Worker);
                        } catch
                        {

                        }
                    }
                }
            }
        }

        private void Send(byte[] buffer)
        {
            if (!isClosing) //if not closing and also packetprocessing
            {
                if (Sock != null && Sock.Connected)
                {
                    try
                    {
                        /*byte[] SendBuff = new byte[buffer.Length + 2];
                        ScSec.EncryptPacket(buffer).CopyTo(SendBuff, 2);
                        BitConverter.GetBytes((ushort)buffer.Length).CopyTo(SendBuff, 0);*/
                        Sock.Send(buffer);
                    } catch
                    {

                    }
                }
            }
        }
        public void Send(Packet packet)
        {
            if (Sock != null && Sock.Connected)
            {
                using (MemoryStream Stream = new MemoryStream( ))
                using (BinaryWriter PacketBuilder = new BinaryWriter(Stream))
                {
                    //RaiseEvent if event is linked
                    if (PacketSent != null)
                    {
                        PacketSent(this, packet);
                    }

                    SendCounter++;
                    PacketBuilder.Write((byte)0x00);// Sec Bytes Count
                    PacketBuilder.Write((ushort)packet.Data.Length); // Data Bytes Count
                    PacketBuilder.Write(packet.Opcode); // Opcode
                    PacketBuilder.Write(SendCounter); // Counter
                    PacketBuilder.Write((byte)0x0E); // Header Length
                    PacketBuilder.Write(1);
                    PacketBuilder.Write(packet.Data);
                    Send(Stream.ToArray( ));
                }
            }
        }

        public byte[] SubArray(byte[] Data, int Offset, int Len)
        {
            byte[] Result = new byte[Len];
            System.Buffer.BlockCopy(Data, Offset, Result, 0, Len);
            return Result;
        }
    }
}
