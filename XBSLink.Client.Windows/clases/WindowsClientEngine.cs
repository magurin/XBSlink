using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LumiSoft.Net.UDP;
using XBSLink.General;

namespace XBSLink.clases
{
    class WindowsClientEngine
    {
        public const int standard_port = 31415;
        public const int standard_kay_port = 34522;
        public const int standard_kay_client_port = 34523;

        public int udp_kay_socket_port;
        public EndPoint _local_endpoint;

       public UdpServer m_pUdpServer;

        public string KAI_CLIENT_LOCAL_DEVICE = "00242BECE7A0";
        public string KAI_CLIENT_LOCAL_NAME = "magurin";

       public bool is_exiting;
       public IPAddress _xbs_link_ip;
       private Thread sender_thread = null;

       public delegate void XlinkDebugMessageHandler(string message_debug, xlink_msg.xbs_message_sender sender);
       public event XlinkDebugMessageHandler XlinkDebugMessage;

       public delegate void ProcessReceivedMessageHandler(xlink_msg msg);
       public event ProcessReceivedMessageHandler ProcessReceivedMessage;



       public WindowsClientEngine(string xbs_link_ip)
    {
        ChangeIPAddresPort(xbs_link_ip, standard_kay_port);
        InitializeSocket();
        //xLinkMsgProcess = new xlink_server_console_process(this);

    }
      
       public void Start()
       {

           is_exiting = false;

           sender_thread = new Thread(new ThreadStart(while_sender));
           sender_thread.IsBackground = true;
           sender_thread.Priority = ThreadPriority.Normal;
           sender_thread.Start();

           System.Console.WriteLine(" * initialized CONSOLE udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
           //ProcessDebugMessage(" * initialized CONSOLE udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
       }

       void InitializeSocket()
       {
           _local_endpoint = new IPEndPoint(_xbs_link_ip, standard_kay_client_port);
           m_pUdpServer = new UdpServer();
           m_pUdpServer.Bindings = new IPEndPoint[] { (IPEndPoint)_local_endpoint };
           m_pUdpServer.PacketReceived += m_pUdpServer_PacketReceived;
           m_pUdpServer.Start();

           ProcessDebugMessage(" * initialized udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
       }


       void m_pUdpServer_PacketReceived(UdpPacket_eArgs e)
       {
           xlink_msg msg = new xlink_msg(e.Data);
           if (msg != null)
           {
               //remote_endpoint = (IPEndPoint)ep;
               msg.src_ip = e.RemoteEndPoint.Address;
               msg.src_port = e.RemoteEndPoint.Port;

               //ProcesamosMSG
               if (ProcessReceivedMessage!=null)
               ProcessReceivedMessage(msg);
           }
       }

       List<xlink_msg> sender_msg = new List<xlink_msg>();

       void while_sender()
       {
           while (!is_exiting)
           {
               udp_sender();
               Thread.Sleep(400);
           }

       }
     
       void udp_sender()
       {
           if (sender_msg.Count > 0)
           {
               SendMessage(sender_msg[0]);
               sender_msg.Remove(sender_msg[0]);
           }
       }


       public static void SendMessage(UdpServer s, IPAddress Ip, int port, byte[] Data)
       {
           try
           {
               EndPoint ep = (EndPoint)new IPEndPoint(Ip, port);
               //udp_kay_socket.SendTo(bytes, bytes.Length, SocketFlags.None, ep);
               s.SendPacket(Data, 0, Data.Length, (IPEndPoint)ep);

               //if (XlinkDebugMessage != null)
               //    XlinkDebugMessage("!! Sending : " + msg.ToString(), xlink_msg.xbs_message_sender.XBOX);
           }
           catch (SocketException sock_ex)
           {
               //if (XlinkDebugMessage != null)
               //    XlinkDebugMessage("!! ERROR SENDING SOCKET UDP CONSOLE: " + sock_ex.Message, xlink_msg.xbs_message_sender.FATAL_ERROR);
           }
           catch (Exception ex)
           {
               //if (XlinkDebugMessage != null)
               //    XlinkDebugMessage("!! ERROR SENDING SOCKET UDP CONSOLE: " + ex.Message, xlink_msg.xbs_message_sender.FATAL_ERROR);
           }
       }

       public static void SendMessage(UdpServer s, IPAddress Ip, int port, string msgText)
       {
           var Data = xlink_msg.getUTF8BytesFromString(msgText);
           SendMessage(s, Ip, port, Data);
       }


       public static void SendMessage(UdpServer s, string Ip, int port, string msgText)
       {
           SendMessage(s, IPAddress.Parse(Ip), port, msgText);
       }

       public void SendMessage(xlink_msg msg)
       {
           if (msg.src_ip != null)
           {
               SendMessage(m_pUdpServer, msg.src_ip, msg.src_port, msg.Data);
           }

       }

       public void Stop()
       {
           is_exiting = true;
           //if (receive_thread.IsAlive)
           //    receive_thread.Abort();
           //receive_thread = null;

           if (sender_thread.IsAlive)
               sender_thread.Abort();
           sender_thread = null;

       }

       public void Shutdown()
       {
           is_exiting = true;
           Stop();

           m_pUdpServer.Stop();
           m_pUdpServer.Dispose();

           //udp_kay_socket.Shutdown(SocketShutdown.Both);
           //udp_kay_socket.Close();

           //if (udp_kay_socket.Connected)
           //{
           //    ProcessDebugMessage("Winsock error: " + Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()), xlink_msg.xbs_message_sender.FATAL_ERROR);
           //}

       }


       public void ProcessDebugMessage(string Message, xlink_msg.xbs_message_sender sender)
       {
           if (XlinkDebugMessage != null)
               XlinkDebugMessage(Message, sender);
       }

       public void ChangeIPAddresPort(string xbs_link_ip, int console_port)
       {
           _xbs_link_ip = IPAddress.Parse(xbs_link_ip);
           udp_kay_socket_port = console_port;
       }


    }
}
