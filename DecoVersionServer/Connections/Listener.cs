﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DecoVersionServer
{
    public class Listener
    {
        Socket ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ushort m_Port = 0;
        public delegate void ConnectedEventHandler(Server Sock);
        public event ConnectedEventHandler Connected;

        public void Listen(ushort Port)
        {
            m_Port = Port;

            ListenerSock.Dispose( );
            ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            ListenerSock.Bind(new IPEndPoint(IPAddress.Any, Port));
            ListenerSock.Listen(0);
            ListenerSock.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        private void OnClientConnect(IAsyncResult AcceptAsync)
        {
            try
            {
                Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Sock = ListenerSock.EndAccept(AcceptAsync);
                Server ServerSock = new Server(Sock);
                //RaiseEvent if event is linked
                if (Connected != null)
                {
                    Connected(ServerSock);
                }
            } catch
            {

            } finally
            {
                ListenerSock.Dispose( );
                ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                ListenerSock.Bind(new IPEndPoint(IPAddress.Any, m_Port));
                ListenerSock.Listen(0);
                ListenerSock.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
        }
    }
}
