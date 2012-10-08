using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using LumiSoft.Net.UDP;
using XBSLink.General;
using XBSLink.General.Delegates;
using XBSLink.General.Delegates.Xlink;

using XBSLink.General.Delegates.Client;



namespace XBSLink.RemoteControl
{

public class xlink_server //:IServer
{

    public xlink_server_console_process.eXlinkPhase _actual_phase { get { return xLinkMsgProcess.actual_phase; } }
   

    #region Xlink Events
    
    public event XlinkDebugMessageHandler XlinkDebugMessage;
    public event XlinkConsoleSendMessageHandler XlinkConsoleSendMessage;
    public event XlinkConsoleJoinCloudHandler XlinkConsoleJoinCloud;
    public event XlinkConsoleLogoutHandler XlinkConsoleLogout;
    public event XlinkConsoleLoginHandler XlinkConsoleLogin;
    public event XlinkChatHandler XlinkConsoleChat;
    public event XlinkConsolePMHandler XlinkConsolePM;

    #endregion

    #region Console Events

    public event ClientGetCloudsHandler ClientGetClouds;
    public event ClientCloudJoinHandler ClientCloudJoin;
    public event ClientCloudCreateJoinHandler ClientCloudCreateJoin;
    public event ClientCloudLeaveHandler ClientCloudLeave;
    public event ClientChatMessageHandler ClientChatMessage;
    public event ClientSendPMHandler ClientSendPM;
    public event ClientStartHandler ClientStart;
    public event ClientStopHandler ClientStop;

    public event ClientDiscoverHandler ClientDiscover;
    public event ClientConnnectHandler ClientConnnect;
    public event ClientDisconnectHandler ClientDisconnect;

    public event ClientFavoriteAddHandler ClientFavoriteAdd;
    public event ClientFavoriteDelHandler ClientFavoriteDel;

      //CLIENT_DISCONNECT = 0x20,
      //      CLIENT_CONNECT = 0x21,
      //      CLIENT_DISCOVER = 0x22,

    #endregion

    public string KAI_CLIENT_LOCAL_DEVICE = "00242BECE7A0";
    public string KAI_CLIENT_LOCAL_NAME = "magurin";


    //public IServerConsoleProcess xLinkMsgProcess { get; set; }
    public xlink_server_console_process xLinkMsgProcess { get; set; }
   
    public IPAddress _console_ip_address;

    public const int standard_port = 31415;
    public const int standard_kay_port = 34522;
    public const int standard_kay_client_port = 34523;

    public int udp_kay_socket_port;
    public EndPoint _local_endpoint;

    List<xlink_msg> xClients;
    List<xlink_msg> xConsoles;

    List<xlink_msg> sender_msg = new List<xlink_msg>();

    // public int udp_kay_socket_port;
    //private Socket udp_kay_socket = null;
    //private Thread receive_thread = null;

    private Thread sender_thread = null;

    UdpServer m_pUdpServer;

    public bool is_exiting = false;

    #region Constructor && INIT

    public xlink_server()
    {
        xClients = new List<xlink_msg>();
        xConsoles = new List<xlink_msg>();
    }

    public bool IsInArray(xlink_msg element, List<xlink_msg> elementArray)
    {
        foreach (var item in elementArray)
        {
            if (element.src_ip.ToString() == item.src_ip.ToString())
                return true;
        }
        return false;
    }

    public void AddClient(xlink_msg client)
    {

        client.src_port =standard_kay_client_port;
        if (!IsInArray(client,xClients))
        {
            xClients.Add(client);
            Console.WriteLine("[ENGINE] ADD CLIENT : " + xClients.Count);
        }

    }

    public void AddConsole(xlink_msg console)
    {
        if (!IsInArray(console, xConsoles))
        {
            xConsoles.Add(console);
            Console.WriteLine("[ENGINE] ADD CONSOLE : " + xConsoles.Count);
        }
        
    }

    public void DeleteConsole(xlink_msg client)
    {
        foreach (var item in xClients)
        {
            if (client.src_ip == item.src_ip)
                xConsoles.Remove(item);
        }
    }

    public void DeleteClient(xlink_msg client)
    {
        foreach (var item in xClients)
        {
            if (client.src_ip == item.src_ip)
                xClients.Remove(item);
        }
    }

    public void Configure(string local_ip_address,xlink_server_console_process process)
    {
        ChangeIPAddresPort(local_ip_address, standard_kay_port);
        xLinkMsgProcess = process;
        ProcessSocket();
    }

    public void Start()
    {

        is_exiting = false;

        ////receive_thread = new Thread(new ThreadStart(while_receiver));
        ////receive_thread.IsBackground = true;
        ////receive_thread.Priority = ThreadPriority.Normal;
        ////receive_thread.Start();

        sender_thread = new Thread(new ThreadStart(while_sender));
        sender_thread.IsBackground = true;
        sender_thread.Priority = ThreadPriority.Normal;
        sender_thread.Start();

        System.Console.WriteLine(" * initialized CONSOLE udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
        ProcessDebugMessage(null, " * initialized CONSOLE udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
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
    }

    #endregion

    #region OUT TO ENGINE EVENTS

    public void ProcessDebugMessage(xlink_msg udp_msg,  string Message, xlink_msg.xbs_message_sender sender)
    {
        if (XlinkDebugMessage != null)
            XlinkDebugMessage(udp_msg,Message, sender);
    }

    public void ConsoleProcessSendMessage(xlink_msg udp_msg, string message)
    {
        if (XlinkConsoleSendMessage != null)
            XlinkConsoleSendMessage(udp_msg,message);
    }

    public void ConsoleProcessJoinCloud(xlink_msg udp_msg,string CloudName)
    {
        ConsoleProcessJoinCloud(udp_msg,CloudName, "");
    }

    public void ConsoleProcessJoinCloud(xlink_msg udp_msg,string CloudName, string CloudPassword)
    {
        //XB

        if (XlinkConsoleJoinCloud != null)
            XlinkConsoleJoinCloud(udp_msg,CloudName, CloudPassword);
    }

    public void ConsoleProcessChat(xlink_msg udp_msg,string Message)
    {

        XBS_SendMyChat(udp_msg, Message);

        if (XlinkConsoleChat != null)
            XlinkConsoleChat(udp_msg,Message);
    }

    public void ConsoleProcessPM(xlink_msg udp_msg,string Username, string Message)
    {
        //Always send
        if (XlinkConsolePM != null)
            XlinkConsolePM(udp_msg,Username, Message, true);
    }

     public void ProcessLogout(xlink_msg udp_msg)
    {
        if (XlinkConsoleLogout != null)
            XlinkConsoleLogout(udp_msg);
    }

    public void ProcessLogin(xlink_msg udp_msg)
    {
        if (XlinkConsoleLogin != null)
            XlinkConsoleLogin(udp_msg);
    }

    #endregion

    #region Socket

    public void ChangeIPAddresPort(string console_ip_address, int console_port)
    {
        _console_ip_address = IPAddress.Parse(console_ip_address);
        udp_kay_socket_port = console_port;
    }

    void ProcessSocket()
    {

        _local_endpoint = new IPEndPoint(_console_ip_address, udp_kay_socket_port);

        m_pUdpServer = new UdpServer();
        m_pUdpServer.Bindings = new IPEndPoint[] { (IPEndPoint)_local_endpoint };
        m_pUdpServer.PacketReceived += m_pUdpServer_PacketReceived;
        m_pUdpServer.Start();

        ProcessDebugMessage(null," * initialized udp listener on port " + udp_kay_socket_port, xlink_msg.xbs_message_sender.UDP_LISTENER);
    }

    void m_pUdpServer_PacketReceived(UdpPacket_eArgs e)
    {
        xlink_msg msg = new xlink_msg(e.Data);

        //if (msg.IsKayPacket())
        //    msg.CleanArray();

        if (msg != null)
        {
            //remote_endpoint = (IPEndPoint)ep;
            msg.src_ip = e.RemoteEndPoint.Address;
            msg.src_port = e.RemoteEndPoint.Port;

            xLinkMsgProcess.ProcessReceivedMessage(msg);
        }
    }


    public void ProcessMessageQueue(xlink_msg msg)
    {
        if (msg != null)
        {
            if (msg.src_ip != null)
            {

                try
                {
                    EndPoint ep = (EndPoint)new IPEndPoint(msg.src_ip, msg.src_port);
                    //udp_kay_socket.SendTo(bytes, bytes.Length, SocketFlags.None, ep);

                    m_pUdpServer.SendPacket(msg.Data, 0, msg.Data.Length, (IPEndPoint)ep);

                    if (XlinkDebugMessage != null)
                        XlinkDebugMessage(msg,"!! Sending : " + msg.ToString(), xlink_msg.xbs_message_sender.XBOX);
                }
                catch (SocketException sock_ex)
                {
                    if (XlinkDebugMessage != null)
                        XlinkDebugMessage(msg, "!! ERROR SENDING SOCKET UDP CONSOLE: " + sock_ex.Message, xlink_msg.xbs_message_sender.FATAL_ERROR);
                }
                catch (Exception ex)
                {
                    if (XlinkDebugMessage != null)
                        XlinkDebugMessage(msg, "!! ERROR SENDING SOCKET UDP CONSOLE: " + ex.Message, xlink_msg.xbs_message_sender.FATAL_ERROR);
                }
            }
        }
    }

    public enum TypeSendQueueTo
    {
        SendToAllConsoles = 2, SendToAllClients = 1, SendToOnlyClient = 0
    }

    public void SendMessageToQueue(xlink_msg udp_msg, string[] msg, TypeSendQueueTo SendToAll, bool ExceptSendUser)
    {
        foreach (var item in msg)
            SendMessageToQueue(udp_msg, item, SendToAll, ExceptSendUser);
    }
 
    /// <summary>
    /// If udp_msg is null there are a system msg
    /// </summary>
    /// <param name="udp_msg"></param>
    /// <param name="msg"></param>
    /// <param name="SendToAll"></param>
    public void SendMessageToQueue(xlink_msg udp_msg, string msg, TypeSendQueueTo SendToAll, bool ExceptSendUser)
    {

            if (SendToAll == TypeSendQueueTo.SendToAllClients)
            {
                if (ExceptSendUser)
                    SendMessageToQueueClientsConnected(udp_msg, msg);
                else
                    SendMessageToQueueClientsConnected(null, msg);
            }
            else if (SendToAll == TypeSendQueueTo.SendToAllConsoles)
            {
                if (ExceptSendUser)
                    SendMessageToQueueConsoles(udp_msg, msg);
                else
                    SendMessageToQueueConsoles(null, msg);
            }
            else
                SendMessageToQueue(udp_msg, msg);

    }
    
    /// <summary>
    /// If udp_msg is null there are a system msg
    /// </summary>
    /// <param name="udp_msg"></param>
    /// <param name="msg"></param>
    /// <param name="SendToAll"></param>
    public void SendMessageToQueue(xlink_msg udp_msg, string msg)
    {
        if (udp_msg != null)
        {
            xlink_msg temp_msg = new xlink_msg(udp_msg, msg);
            lock (sender_msg)
                sender_msg.Add(temp_msg);
        }
        else
            Console.WriteLine("ERROR udp_msg = null >> " + msg);
    }

    private void SendMessageToQueueClientsConnected(xlink_msg except_user, string msg)
    {
        if (except_user != null)
        {
            foreach (var item in xClients)
            {
                if (except_user.src_ip.ToString() != item.src_ip.ToString())
                    SendMessageToQueue(item, msg);
            }
        }
        else
        {
            foreach (var item in xClients)
            {
                SendMessageToQueue(item, msg);
            }
        }
      
    }

    private void SendMessageToQueueConsoles(xlink_msg except_user, string msg)
    {
        if (except_user != null)
        {
            foreach (var item in xConsoles)
            {
                if (except_user.src_ip.ToString() != item.src_ip.ToString())
                    SendMessageToQueue(item, msg);
            }
        }
        else
        {
            foreach (var item in xConsoles)
                    SendMessageToQueue(item, msg);
        }
      
    }

    void while_sender()
    {
        while (!is_exiting)
        {
            if (sender_msg.Count > 0)
            {
                ProcessMessageQueue(sender_msg[0]);
                sender_msg.Remove(sender_msg[0]);
            }
            Thread.Sleep(400);
        }

    }

    #endregion

    #region FROM ENGINE IN COMMANDS

    public void XBS_SendMyChat(xlink_msg udp_msg,string message)
    {
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAI_CLIENT_CHAT("", message), TypeSendQueueTo.SendToAllConsoles, true); //NO ESTÁ CLARO
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.ServerMyChatMessage(message), TypeSendQueueTo.SendToAllClients, true);
        
    }

    /// <summary>
    /// MESSAGE FROM USER
    /// </summary>
    /// <param name="udp_msg"></param>
    /// <param name="user_name"></param>
    /// <param name="Text"></param>
    public void XBS_SendUserChat(xlink_msg udp_msg, string user_name, string Text)
    {
        //KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_CHAT(username, message));
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAI_CLIENT_CHAT(user_name, Text), TypeSendQueueTo.SendToAllConsoles, true);
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.ServerUserChatMessage(user_name, Text), TypeSendQueueTo.SendToAllClients, true);
        
    }

    public void XBS_SendPM(xlink_msg udp_msg, string user_name, string Text)
    {

        SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAI_CLIENT_PM(user_name, Text), TypeSendQueueTo.SendToAllConsoles, true);
        udp_msg.src_port = standard_kay_client_port;
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.ServerUserPMMessage(user_name, Text), TypeSendQueueTo.SendToAllClients, true);
        //KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_PM(username, message));
        //xLinkMsgProcess.SendPMMessage(udp_msg,user_name, Text);
    }

    public void XBS_ChannelCreate(xlink_msg udp_msg, string CloudName, int Players, bool isPrivate, int MaxPlayers)
    {

        SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAI_CLIENT_USER_SUB_VECTOR(CloudName, Players, isPrivate, MaxPlayers), TypeSendQueueTo.SendToAllConsoles, true);
        
       
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.SERVER_CLOUD_CREATE_JOIN(CloudName, Players, isPrivate, MaxPlayers), TypeSendQueueTo.SendToAllClients, true);
    }

    public void XBS_Detach(xlink_msg udp_msg)
    {

        SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAY_GET_DETACH(KAI_CLIENT_LOCAL_DEVICE), TypeSendQueueTo.SendToAllConsoles, true);
        udp_msg.src_port = standard_kay_client_port;
        SendMessageToQueue(udp_msg, xlink_client_messages_helper.SERVER_STOP(), TypeSendQueueTo.SendToAllClients, true);

    }

        public void XBS_LeaveUser(xlink_msg udp_msg,string username)
     {

         SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAY_GET_LEAVE_USER_FROM_VECTOR(username), TypeSendQueueTo.SendToAllConsoles, true);
         SendMessageToQueue(udp_msg, xlink_client_messages_helper.ServerUserOffline(username), TypeSendQueueTo.SendToAllClients, true);

     }

        public void XBS_JoinUser(xlink_msg udp_msg,string username, string client_version, string last_ping_delay_ms)
        {

            SendMessageToQueue(udp_msg, xlink_client_messages_helper.KAY_GET_USER_JOIN_TO_VECTOR(username), TypeSendQueueTo.SendToAllConsoles, true);
            SendMessageToQueue(udp_msg, xlink_client_messages_helper.ServerUserOnline(username, client_version, last_ping_delay_ms), TypeSendQueueTo.SendToAllClients, true);

        }

    #endregion

    #region CLIENT COMMANDS

       public void cConsoleStart(xlink_msg msg)
        {


            if (ClientStart != null)
                ClientStart(msg);
        }


       public void cConsoleConnect(xlink_msg msg)
       {

           SendMessageToQueue(msg,
             xlink_msg.getHeaderMessageFromType(xlink_msg.xbs_xlink_message_type.SERVER_ACCEPT)
             );

           if (ClientConnnect != null)
               ClientConnnect(msg);
       }


       public void cConsoleDisconnect(xlink_msg msg)
       {


           if (ClientDisconnect != null)
               ClientDisconnect(msg);
       }

       public void cConsoleDiscover(xlink_msg msg)
       {

           SendMessageToQueue(msg,
                xlink_client_messages_helper.ServerInfoServer(
                "XBSLINK TEST SERVER",
                "active",
                "magurin",
                _local_endpoint.ToString(),
                udp_kay_socket_port.ToString(),
                "v1.0"), xlink_server.TypeSendQueueTo.SendToOnlyClient, false
                );

           if (ClientDiscover != null)
               ClientDiscover(msg);
       }

       public void cConsoleFavoriteAdd(xlink_msg msg)
       {

           if (ClientFavoriteAdd != null)
               ClientFavoriteAdd(msg);
       }

       public void cConsoleFavoriteDel(xlink_msg msg)
       {
           if (ClientFavoriteAdd != null)
               ClientFavoriteAdd(msg);
       }


       public void cConsoleStop(xlink_msg msg)
       {
           //

           if (ClientStop != null)
               ClientStop(msg);
       }

       public void cConsoleSendPM(xlink_msg msg)
       {


           if (ClientSendPM != null)
               ClientSendPM(msg);
       }

    /// <summary>
    /// CLIENT PETITION
    /// </summary>
    /// <param name="msg"></param>
       public void cClientGetClouds(xlink_msg msg)
       {
           //
           if (ClientGetClouds != null)
               ClientGetClouds(msg);
       }

       public void cConsoleCloudCreateJoin(xlink_msg msg)
       {



           if (ClientCloudCreateJoin != null)
               ClientCloudCreateJoin(msg);
       }

       public void cConsoleCloudJoin(xlink_msg msg)
       {
           if (ClientCloudJoin != null)
               ClientCloudJoin(msg);
       }

       public void cConsoleCloudLeave(xlink_msg msg)
       {


           if (ClientCloudLeave != null)
               ClientCloudLeave(msg);
       }

       public void cConsoleChatMessage(xlink_msg msg)
       {
           
           if (ClientChatMessage != null)
               ClientChatMessage(msg);
       }

     

    #endregion

}

}