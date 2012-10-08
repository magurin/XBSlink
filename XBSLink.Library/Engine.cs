using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SharpPcap;
using SharpPcap.LibPcap;
using XBSLink.Properties;

using XBSLink.Cloud;
using XBSLink.Constants;
using XBSLink.Library;
using XBSLink.Message;
using XBSLink.NAT;
using XBSLink.NAT.Stun;
using XBSLink.Node;
using XBSLink.Node.List;
using XBSLink.Node.Message;
using XBSLink.RemoteControl;
using XBSLink.Setting;
using XBSLink.Sniffer;
using XBSLink.UDP;
using XBSLink.General;
using XBSLink.Library.RemoteControl;

namespace XBSLink
{
   public  class Engine
    {

        List<PhysicalAddress> MacList = null;

        public static xbs_udp_listener udp_listener = null;
        public static xbs_sniffer sniffer = null;
        public static xbs_node_list node_list = null;
        public static xbs_nat NAT = null;

        private xbs_upnp upnp = null;

        public static IPAddress external_ip = null;
        public static IPAddress internal_ip = null;

        private bool use_UPnP = true;

        private const int MAX_WAIT_START_ENGINE_SECONDS = 6;

        //private DateTime app_start_time;
        private DateTime start_engine_started_at;

        public static bool abort_start_engine = false;
        public bool engine_started = false;

        //private bool autoswitch_on_chat_message = false;

        public static xbs_cloudlist cloudlist = null;
        public static Object askedCloudServerForHelloMessage_locker = new Object();

        private DateTime last_update_check = new DateTime(0);
        private DateTime last_nodelist_update = new DateTime(0);
        private DateTime last_nat_ippool_update = new DateTime(0);

        private DateTime last_resizeNATIPPoolHeaderHeader = DateTime.Now;
        private DateTime last_resizeCloudListHeader = DateTime.Now;
        private DateTime last_resizeNodeListHeader = DateTime.Now;

        SharpPcap.CaptureDeviceList pcap_devices = null;

        private Dictionary<IPAddress, GatewayIPAddressInformationCollection> network_device_gateways = new Dictionary<IPAddress, GatewayIPAddressInformationCollection>();


        //public delegate void StatusBarMessageHandler(string message_text);
        //public event StatusBarMessageHandler StatusBarMessage;

        public delegate void RefreshCloudListViewHandler(xbs_cloud[] clouds);
        public event RefreshCloudListViewHandler RefreshCloudListView;

        public delegate void AlertMessageHandler(string message, string tittle, eAlerts alertType );
        public event AlertMessageHandler AlertMessage;

        public enum eAlerts{
            MsgBox=0,  Alert=1, StatusBar=2, MsgBoxError=3,MsgBoxAlert=4, Exception=10
        }

        List<String> remoteHosts = null;
        List<string> localIP = new List<string>();

        public static xlink_server XlinkServer;

        Thread EngineThread;

        public void Start()
        {

            // globally turn off Proxy auto detection
            WebRequest.DefaultWebProxy = null;

            MacList = new List<PhysicalAddress>();
            
            initializeNodeList();

            cloudlist = new xbs_cloudlist();
            upnp = new xbs_upnp();
            NAT = new xbs_nat();

            remoteHosts = new List<string>();
            //ShowVersionInfoMessages();

            internal_ip = IPAddress.Parse(xbs_constants.localIP);
            
            initializeCaptureDeviceList();

            initializeLocalIPList();
         
            initializeRegistryValues();
            
            initializateRemoteControl();

        }

        public void initializateChatAudio()
        {

        }


        public void initializeNodeList()
        {
            node_list = new xbs_node_list();
            node_list.DeleteNodes += node_list_DeleteNodes;
        }

        void node_list_DeleteNodes(List<xbs_node> nodes)
        {
            foreach (var item in nodes)
            {
                udp_listener_DeleteNode(null,item.nickname);
            }
        }

        void udp_listener_DeleteNode(xbs_udp_message udp_msg, string NickName)
        {
            Console.WriteLine("[ENGINE/LEAVEUSER]" + NickName);
            XlinkServer.XBS_LeaveUser( null, NickName);
        }

        void udp_listener_AddNode(xbs_udp_message udp_msg, string NickName, string client_version, string last_ping_delay_ms)
        {
            Console.WriteLine("[ENGINE/ADDUSER] : " + NickName + " VERSION: " + client_version + " PING: " + last_ping_delay_ms);
            XlinkServer.XBS_JoinUser(null, NickName,client_version, last_ping_delay_ms);
        }

        public void initializateRemoteControl()
        {
            System.Console.WriteLine("Starting Remote Control..");
            XlinkServer = new xlink_server_cloud();
           
            var tmp = new xlink_server_cloud_console_process();
            tmp.SetServer(XlinkServer);
            XlinkServer.Configure(xbs_constants.localIP, tmp);

            XlinkServer.XlinkConsoleChat += XlinkServer_XlinkConsoleChat;
            XlinkServer.XlinkConsoleLogin += XlinkServer_XlinkConsoleLogin;
            XlinkServer.XlinkConsolePM += GetSendMessagePM;
            XlinkServer.XlinkConsoleJoinCloud += XlinkServer_XlinkConsoleJoinCloud;
            XlinkServer.XlinkDebugMessage += XlinkServer_XlinkDebugMessage;
            XlinkServer.XlinkConsoleSendMessage += XlinkServer_XlinkConsoleSendMessage;
            XlinkServer.XlinkConsoleLogout += XlinkServer_XlinkConsoleLogout;

            //CLIENT
            XlinkServer.ClientGetClouds += Client_GetClouds;
            XlinkServer.ClientCloudLeave += Client_CloudLeave;
            XlinkServer.ClientChatMessage += Client_ChatMessage;
            XlinkServer.ClientSendPM += Client_SendPM;
            XlinkServer.ClientCloudJoin += Client_CloudJoin;
            XlinkServer.ClientStart += Client_ConsoleStart;
            XlinkServer.ClientStop += Client_ConsoleStop;
            XlinkServer.ClientCloudCreateJoin += Client_CloudCreateJoin;
            XlinkServer.ClientConnnect += Client_Connnect;
            XlinkServer.ClientDisconnect += Client_Disconnect;
            XlinkServer.ClientDiscover += Client_Discover;
            XlinkServer.ClientFavoriteAdd += Client_FavoriteAdd;
            XlinkServer.ClientFavoriteDel += Client_FavoriteDel;

            XlinkServer.Start();
        }



        void Client_FavoriteDel(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/FAVORITEDELETE] (NOT IMPLEMENTED)");
        }

        void Client_FavoriteAdd(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/FAVORITEADD] (NOT IMPLEMENTED)");
        }

        void Client_Discover(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/DISCOVER]");
            
        }

        void Client_Disconnect(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/DISCONNECT]");
            XlinkServer.DeleteClient(udp_msg);
        }

        void Client_Connnect(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/CONNECT]");
        }

        void Client_CloudCreateJoin(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            WriteHeader(udp_msg, "[CLIENT/CLOUDCREATEJOIN] : " + udp_msg.data_msg);

            xlink_create_cloud_create_join_message msg = new xlink_create_cloud_create_join_message(udp_msg);
            join_cloud(msg._cloudname, msg._maxusers, msg._password);
            //msg.cloudName =
        }

        void Client_ConsoleStop(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            WriteHeader(udp_msg, "[CLIENT/ENGINESTOP]");
            engine_stop( udp_msg);
        }

        void Client_ConsoleStart(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            WriteHeader(udp_msg, "[CLIENT/ENGINESTART]");
            XlinkServer_XlinkConsoleLogin( udp_msg);
        }

        void Client_CloudJoin(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            WriteHeader(udp_msg, "[CLIENT/CLOUDJOIN] : " + udp_msg.data_msg);
            XlinkServer_XlinkConsoleJoinCloud(udp_msg,udp_msg.GetParameters()[1], "");
        }

        void Client_SendPM(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            var parametros = udp_msg.GetParameters();
            WriteHeader(udp_msg, "[CLIENT/SENDPM] : " + udp_msg.data_msg);
            GetSendMessagePM(udp_msg,parametros[0], parametros[1], false);
        }

        void Client_ChatMessage(xlink_msg udp_msg)
        {

            xlink_client_send_chat_message tmp = new xlink_client_send_chat_message(udp_msg);
            //Se envia a FSD/Clientes
            XlinkServer.XBS_SendMyChat(udp_msg, tmp._message);
            //Se envia por engine
            SendMessageToAllNodes(tmp._message);

            WriteHeader(udp_msg, "[CLIENT/CHATMSG] : " + udp_msg.data_msg);
           
            //XlinkServer_XlinkConsoleChat(udp_msg, "Pruebas");
        }

        void Client_CloudLeave(xlink_msg udp_msg)
        {
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            WriteHeader(udp_msg, "[CLIENT/CLOUDLEAVE]");
            LeaveChannel();
        }

        string GetStrClouds(xlink_msg udp_msg)
        {
            return xlink_client_messages_clouds_helper.GetClouds(cloudlist.getCloudlistArray());
        }

        void WriteHeader(xlink_msg udp_msg, string tittle)
        {
            Console.WriteLine("(" + udp_msg.src_ip.ToString() + ":" + udp_msg.src_port.ToString() + ") : " +  tittle );
        }

        void Client_GetClouds(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[CLIENT/GETCLOUDS] : " + udp_msg.data_msg);

            //Cambio el puerto para el envio a clientes IMPORTANTE
            udp_msg.src_port = xlink_server.standard_kay_client_port;
            foreach (var item in cloudlist.getCloudlistArray())
            {
                WriteHeader(udp_msg, "[SERVER/ADD_CLOUD] : " + xlink_client_messages_clouds_helper.SERVER_ADD_CLOUD(item));
                XlinkServer.SendMessageToQueue(udp_msg, xlink_client_messages_clouds_helper.SERVER_ADD_CLOUD(item));
            } ;

            //(XlinkServer.xLinkMsgProcess as xlink_server_cloud_console_process).SendActualCloudList(udp_msg,GetCloudList());

        }

        void XlinkServer_XlinkConsoleLogout(xlink_msg udp_msg)
        {
            engine_stop(udp_msg);
        }


        #region RemoteControl

        void SendMessageToAllNodes(string Message)
        {
            node_list.sendChatMessageToAllNodes(Message);
        }

        void XlinkServer_XlinkConsoleSendMessage(xlink_msg udp_msg, string message)
        {

            //EscribeCabecera(udp_msg, "[XLINK/SENDMESSAGE] :" + udp_msg.data_msg);
            SendMessageToAllNodes(message);
#if DEBUG
            //xbs_messages.addInfoMessage("360 SEND MSG " + message, xbs_message_sender.X360);
#endif

        }

        void XlinkServer_XlinkDebugMessage(xlink_msg udp_msg, string message_debug, xlink_msg.xbs_message_sender sender)
        {

        }

        #region Channel

        void LeaveChannel()
        {
            bool ret = cloudlist.LeaveCloud();
            if (ret)
            {
                //Console.WriteLine("left " + cloudlist.);
                sniffer.clearKnownMACsFromRemoteNodes();
                sniffer.setPdevFilter();
            }
        }

        void JoinChannel(string CloudName, string CloudPassword)
        {
            LeaveChannel();
            var encontrado = cloudlist.findCloud(CloudName);
            if (encontrado != null)
                join_cloud(CloudName, encontrado.max_nodes.ToString(), CloudPassword);
        }


        #endregion


        void XlinkServer_XlinkConsoleJoinCloud(xlink_msg udp_msg, string CloudName, string CloudPassword)
        {
            WriteHeader(udp_msg, "[XLINK/JOINCLOUD] :" + udp_msg.data_msg);
            JoinChannel(CloudName, CloudPassword);
        }

        void XlinkServer_XlinkConsoleLogin(xlink_msg udp_msg)
        {
            WriteHeader(udp_msg, "[XLINK/LOGIN] : " + udp_msg.data_msg);

            if (engine_started)
                engine_stop(udp_msg);

            engine_start(udp_msg);
        }

        void XlinkServer_XlinkConsoleChat(xlink_msg udp_msg,string message)
        {
            WriteHeader(udp_msg,"[XLINK/CHAT] : "  + message);
            XlinkServer.XBS_SendMyChat(udp_msg, message);
        }


        void udp_listener_ChatMessage(xbs_udp_message udp_msg, string nickname, string message)
        {
            WriteHeader(udp_msg.GetXlinkMsg(), "[ENGINE/RECEIVECHAT] : " + message);
            XlinkServer.XBS_SendUserChat(udp_msg.GetXlinkMsg(), nickname, message);
        }


        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="message"></param>
        void GetSendMessagePM(xlink_msg udp_msg, string UserName, string message, bool IsSended)
        {
                xbs_node elemento = node_list.findNode(UserName);
                if (elemento != null)
                {
                    if (IsSended)
                        elemento.sendMessagePM(message);
                    else
                    {
                        //Si lo hemos enviado nosotros
                        //Lo enviamos a la consola
                        udp_listener.XBS_SendPM(udp_msg,UserName, message);
                    }

                }
            
        }

       /// <summary>
       /// IN CONSOLE CHAT
       /// </summary>
       /// <param name="udp_msg"></param>
       /// <param name="Message"></param>
        //void SendChatMessage(xlink_msg udp_msg, string Message)
        //{
        //    //xbs_chat.sendChatMessage(Message);
        //    //xbs_chat.addLocalMessage(Message);
        //}


       
        private void initializeRegistryValues()
        {
            System.Console.WriteLine("Starting registry values..");
            //xbs_constants.GetMemoryValues();

            if (xbs_constants.REG_SPECIAL_MAC_LIST != null)
                setMacListFromString(xbs_constants.REG_SPECIAL_MAC_LIST);

            if (xbs_constants.REG_REMOTE_HOST_HISTORY != null)
                setRemoteHostHistoryFromString(xbs_constants.REG_REMOTE_HOST_HISTORY);

            //xbs_chat.message_when_nodes_join_or_leave = true;

            //if (xbs_constants.REG_NAT_IP_POOL != null)
            //{
            //    setNATIPPoolFromString(xbs_constants.REG_NAT_IP_POOL);
            //    updateNATIPPoolListView();
            //}
        }

        private bool initializeCaptureDeviceList()
        {
            System.Console.WriteLine("Starting Capture Devices..");
            try
            {
                pcap_devices = CaptureDeviceList.Instance;
            }
            catch (PcapException pex)
            {
            }

            //if (System.Environment.OSVersion.Platform == PlatformID.Win32NT && (pcap_devices == null || pcap_devices.Count < 1))
            //{
            //    res = MessageBox.Show(Resources.message_no_capture_devices_startNPF, "XBSlink error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            //    if (res == DialogResult.Yes)
            //    {
            //        startNPFdriver();
            //        try
            //        {
            //            pcap_devices = CaptureDeviceList.New();
            //        }
            //        catch (PcapException pex)
            //        {
            //        }
            //    }
            //}

            //if (pcap_devices != null)
            //    foreach (LibPcapLiveDevice dev in pcap_devices)
            //        comboBox_captureDevice.Items.Add(dev.Interface.FriendlyName + " (" + dev.Interface.Description + ")");

            //if (comboBox_captureDevice.Items.Count > 0)
            //    comboBox_captureDevice.SelectedIndex = 0;
            //else
            //{
            //    if (System.Environment.OSVersion.Platform == PlatformID.Unix)
            //        MessageBox.Show(Resources.message_no_capture_devices_unix, "XBSlink error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    else
            //        MessageBox.Show(Resources.message_no_capture_devices, "XBSlink error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return false;
            //}
            return true;
        }

        private void setRemoteHostHistoryFromString(String remoteHostList)
        {
            remoteHosts.Clear();
            if (remoteHostList.Length == 0)
                return;
            foreach (String remoteHost in remoteHostList.Split(','))
                remoteHosts.Add(remoteHost);
        }

        private void initializeLocalIPList()
        {
            NetworkInterface[] network_interfaces = NetworkInterface.GetAllNetworkInterfaces();
            int local_ip_count = 0;
            int preferred_local_ip = -1;
            IPInterfaceProperties ip_properties;
            foreach (NetworkInterface ni in network_interfaces)
            {
                ip_properties = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation uniCast in ip_properties.UnicastAddresses)
                    if (!IPAddress.IsLoopback(uniCast.Address) && uniCast.Address.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        if (uniCast.Address.ToString().Split('.')[0] != "169")
                        {
                            local_ip_count++;
                            localIP.Add(uniCast.Address.ToString());
                            network_device_gateways[uniCast.Address] = ip_properties.GatewayAddresses;
                            if (ni.OperationalStatus == OperationalStatus.Up && (ip_properties.GatewayAddresses.Count > 0))
                            {
                                if (!ip_properties.GatewayAddresses[0].Address.Equals(new IPAddress(0)))
                                    preferred_local_ip = local_ip_count;
                            }
                        }
                    }
            }
            
            //if (localIP.Items.Count > 0)
            //{
            //    localIP.SelectedIndex = (preferred_local_ip == -1) ? 0 : preferred_local_ip - 1;
            //}

        }

        public void engine_stop(xlink_msg udp_msg)
        {
            if (cloudlist != null)
                if (cloudlist.part_of_cloud)
                    cloudlist.LeaveCloud();


            EngineThread.Abort();
            EngineThread = null;

            //xbs_settings.settings.Save();

            if (sniffer != null)
            {
                sniffer.close();
                sniffer = null;
            }
            if (udp_listener != null)
            {
                node_list.sendLogOff();
                udp_listener.shutdown();
                udp_listener = null;
            }
            if (upnp != null)
            {
                if (upnp.isUPnPavailable())
                    upnp.upnp_deleteAllPortMappings();
                upnp.upnp_stopDiscovery();
            }
            engine_started = false;
            xbs_messages.addInfoMessage("Engine stopped.", xbs_message_sender.GENERAL);

            //listView_nodes.Items.Clear();
            //treeView_nodeinfo.Nodes.Clear();
            NAT.ip_pool.freeAllIPs();
            //updateNATIPPoolListView();

            //button_start_engine.Text = "Start Engine";
            //textBox_maininfo.Text = "Engine not started.";
            //textBox_chatEntry.ReadOnly = true;
            //textBox_chatEntry.Clear();
            //textBox_CloudName.Enabled = false;
            //textBox_CloudPassword.Enabled = false;
            //textBox_CloudMaxNodes.Enabled = false;
            //button_CloudJoin.Enabled = false;
            //button_CloudLeave.Enabled = false;
            //textBox_chatNickname.ReadOnly = false;
            //button_reset_settings.Enabled = true;         
        }

        public void engine_start(xlink_msg udp_msg)
        {

            EngineThread = new Thread(() =>
                {    
                    //clearMessagesAndNotifications();
            // show Messages to User
            //tabControl1.SelectedTab = tabPage_messages;
            xbs_messages.addInfoMessage("starting Engine", xbs_message_sender.GENERAL);
            //if (checkbox_UPnP.Checked)
            //    upnp.upnp_startDiscovery();
            start_engine_started_at = DateTime.Now;

            TimeSpan elapes_time = DateTime.Now - start_engine_started_at;
            bool upnp_discovery_finished = (xbs_constants.UPnP && upnp.isUPnPavailable()) || !xbs_constants.UPnP;
            if (upnp_discovery_finished || elapes_time.TotalSeconds >= MAX_WAIT_START_ENGINE_SECONDS)
            {
                //Iniciamos el engine
                resume_start_engine(udp_msg);

                //Actualizamos los clouds
                if (udp_msg != null)
                    GetSendXlinkCloudList(udp_msg);
                else
                    GetCloudList();
            }
                
                }
        );

            EngineThread.Priority = ThreadPriority.Normal;
            EngineThread.Start();
        }

        public void resume_start_engine(xlink_msg udp_msg)
        {
            //if (ExceptionMessage.ABORTING)
            //    return;
            engine_started = false;
            ICaptureDevice pdev;
            if (pcap_devices.Count == 0)
            {

                if (AlertMessage != null)
                {
                    AlertMessage("XBSlink did not find any available network adapters in your system."
                    + Environment.NewLine
                    + "Does your user have enough system rights?", "XBSlink error", eAlerts.MsgBoxError);
                    abort_start_engine = true;
                }
               
                return;
            }

            try
            {
                pdev = pcap_devices[xbs_constants.captureDeviceSelected];
            }
            catch (Exception)
            {
                if (AlertMessage != null)
                    AlertMessage("XBSlink could not set the capture device.", "XBSlink error", eAlerts.MsgBoxError);
                abort_start_engine = true;
                return;
            }

            try
            {

                if (udp_listener != null)
                {
                    udp_listener.AddNode -= udp_listener_AddNode;
                    udp_listener.DeleteNode -= udp_listener_DeleteNode;
                    udp_listener.ChatMessage -= udp_listener_ChatMessage;
                }

                udp_listener = new xbs_udp_listener(internal_ip, UInt16.Parse( xbs_constants.local_Port ));

                udp_listener.AddNode += udp_listener_AddNode;
                udp_listener.DeleteNode += udp_listener_DeleteNode;
                udp_listener.ChatMessage += udp_listener_ChatMessage;
            }
            catch (Exception e)
            {
                xbs_messages.addInfoMessage("!! Socket Exception: could not bind to port " + xbs_constants.local_Port , xbs_message_sender.GENERAL, xbs_message_type.FATAL_ERROR);
                xbs_messages.addInfoMessage("!! the UDP socket is not ready to send or receive packets. Please check if another application is running on this port.", xbs_message_sender.GENERAL, xbs_message_type.FATAL_ERROR);

                if (AlertMessage != null)
                    AlertMessage(e.Message, "XBSlink error", eAlerts.MsgBoxError);
                abort_start_engine = true;
            }

            if (abort_start_engine )
            {
                udp_listener = null;
                return;
            }

            try
            {
                if (use_UPnP && upnp.isUPnPavailable())
                {
                    external_ip = upnp.upnp_getPublicIP();
                    upnp.upnp_create_mapping(Mono.Nat.Protocol.Udp, udp_listener.udp_socket_port, udp_listener.udp_socket_port);
                }
            }
            catch (Exception)
            {
                xbs_messages.addInfoMessage("!! UPnP port mapping failed", xbs_message_sender.GENERAL, xbs_message_type.ERROR);
            }

            if (external_ip == null)
                external_ip = xbs_upnp.getExternalIPAddressFromWebsite();

            IPAddress local_node_ip = (external_ip == null) ? internal_ip : external_ip;
            node_list.local_node = new xbs_node(local_node_ip, udp_listener.udp_socket_port);
            node_list.local_node.nickname = xbs_constants.chatNickname;

            try
            {
                sniffer = new xbs_sniffer((LibPcapLiveDevice)pdev, xbs_constants.enable_MAC_list, MacList, xbs_constants.mac_restriction, node_list, NAT, network_device_gateways[internal_ip], xbs_constants.excludeGatewayIPs);
                sniffer.start_capture();
            }
            catch (ArgumentException aex)
            {
                xbs_messages.addInfoMessage("!! starting Packet sniffer failed (1): " + aex.Message, xbs_message_sender.GENERAL, xbs_message_type.ERROR);
                abort_start_engine = true;
                udp_listener = null;
                sniffer = null;
                return;
            }
            catch (PcapException pcex)
            {
                xbs_messages.addInfoMessage("!! starting Packet sniffer failed (2): " + pcex.Message, xbs_message_sender.GENERAL, xbs_message_type.ERROR);
                abort_start_engine = true;
                udp_listener = null;
                sniffer = null;
                return;
            }

            try
            {
                if (xbs_constants.useCloudServerForPortCheck)
                    checkIncomingPortWithCloudServer();
            }
            catch (Exception)
            {
                xbs_messages.addInfoMessage("!! open port check failed", xbs_message_sender.GENERAL, xbs_message_type.WARNING);
                abort_start_engine = true;
                return;
            }

            xbs_messages.addInfoMessage("engine ready. waiting for incoming requests.", xbs_message_sender.GENERAL);
            //textBox_CloudName.Enabled = true;
            //textBox_CloudPassword.Enabled = true;
            //textBox_CloudMaxNodes.Enabled = true;
            //button_CloudJoin.Enabled = true;
            //button_CloudLeave.Enabled = false;
            engine_started = true;
            //button_start_engine.Enabled = true;
            //button_start_engine.Text = "Stop Engine";
        }

      

        private void checkIncomingPortWithCloudServer()
        {
            xbs_messages.addInfoMessage("contacting cloud server...", xbs_message_sender.GENERAL);

            if (cloudlist.askCloudServerForHello(XBSLink.Properties.Settings.Default.REG_CLOUDLIST_SERVER, node_list.local_node.ip_public, node_list.local_node.port_public))
            {
                lock (udp_listener._locker_HELLO)
                {
                    if (!xbs_upnp.isPortReachable)
                        Monitor.Wait(udp_listener._locker_HELLO, 1000);
                }

                if (xbs_upnp.isPortReachable == false)
                {
                    xbs_messages.addInfoMessage("!! cloudlist server HELLO timeout. incoming Port is CLOSED", xbs_message_sender.GENERAL, xbs_message_type.WARNING);

                    if (AlertMessage != null)
                        AlertMessage("Your XBSlink is not reachable from the internet (port closed)." + Environment.NewLine +
                            "Please configure your router and firewall to forward a port to your computer or use UPnP where available." + Environment.NewLine + Environment.NewLine +
                            "Other XBSlink will not be able to connect to you directly!",
                            "XBSlink incoming port information", eAlerts.MsgBoxAlert);
                }
                else
                    xbs_messages.addInfoMessage("incoming Port is OPEN", xbs_message_sender.GENERAL);
            }
        }

        #region Macs

        private void setMacListFromString(String mac_list)
        {
            MacList.Clear();
            if (mac_list.Length == 0)
                return;
            String[] macs = mac_list.Split(',');
            foreach (String mac in macs)
                addMacToMacList(mac);
        }


        bool FindMacAddress(String mac)
        {
            foreach (var item in MacList)
            {
                if (item.ToString() == mac)
                    return true;
            }
            return false;
        }

        private void addMacToMacList(String mac)
        {
            if (!FindMacAddress(mac))
            {
                MacList.Add(PhysicalAddress.Parse(mac));
                setSnifferMacList();
            }
        }

        private void setSnifferMacList()
        {
            if (sniffer != null)
                sniffer.setSpecialMacPacketFilter(MacList);
        }

        #endregion

        #region Clouds

        public void join_cloud(string CloudName, string MaxNodes, string Password)
        {
            join_cloud(xbs_constants.cloudlist, CloudName, MaxNodes, Password);
        }

        public void join_cloud(string CloudList, string CloudName, string MaxNodes, string Password)
        {
            bool ret = cloudlist.JoinOrCreateCloud(CloudList, CloudName, MaxNodes, Password, node_list.local_node.ip_public, node_list.local_node.port_public, node_list.local_node.nickname, xbs_upnp.isPortReachable, xbs_settings.xbslink_version);
            if (ret)
            {
                //toolTip2.Show("joined " + textBox_CloudName.Text, button_CloudJoin, 0, -20, 2000);
                //button_CloudLeave.Enabled = true;
                //button_CloudJoin.Enabled = false;
                //textBox_CloudName.Enabled = false;
                //textBox_CloudMaxNodes.Enabled = false;
                //textBox_CloudPassword.Enabled = false;
                //switch_tab = tabPage_info;
            }
        }

        public xbs_cloud[] GetCloudList()
        {
            return GetCloudList(xbs_constants.cloudlist);
        }


        public xbs_cloud[] GetSendXlinkCloudList(xlink_msg udp_msg)
        {
            var elementos = GetCloudList();
            if (XlinkServer != null)
            {
                (XlinkServer.xLinkMsgProcess as xlink_server_cloud_console_process).SendActualCloudList(udp_msg,elementos.ToList());
            }
            return elementos;
        }

        public xbs_cloud[] GetCloudList(string cloudlist_url)
        {
            bool ret = cloudlist.loadCloudlistFromURL(cloudlist_url);

            if (ret)
                return cloudlist.getCloudlistArray();
            return null;
        }

        public void LoadSetCloudList(string cloudlist_url)
        {
            var clouds = GetCloudList(cloudlist_url);
            if (clouds != null)
            {
                if (RefreshCloudListView != null)
                    RefreshCloudListView(cloudlist.getCloudlistArray());
                XBSLink.Properties.Settings.Default.REG_CLOUDLIST_SERVER = cloudlist_url;
            }
        }


        #endregion

    }
}
