using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XBSLink.Properties;
using XBSLink.Cloud;
using XBSLink.Setting;

namespace XBSLink.Constants
{
    public class xbs_constants
    {

        public static bool UPnP = false;


        public static string RemotePort = "31415";
        public static string RemoteHost = "otherhost.dyndns.org";

        public static bool nat_enable = false;
        public static bool filter_wellknown_ports = true;

        public static bool NAT_enablePS3mode = true;

        public static bool forward_all_high_port_broadcast = false;
        public static bool preventSystemStandby = true;

        public static bool checkForUpdates = true;
        public static string localIP = "10.67.2.1";
        public static string xbslink_version = "0.1";
        public static bool enable_MAC_list = false;
        public static bool mac_restriction = true;
        public static bool excludeGatewayIPs = true;
        public static string chatNickname = "magurin";
        public static bool useCloudServerForPortCheck = true;
        public static string local_Port = "31415";
        public static int captureDeviceSelected = 2;

        public static string cloudlist = "http://www.secudb.de/~seuffert/xbslink/cloudlist/";

        public static string REG_SPECIAL_MAC_LIST = "";
        public static string REG_REMOTE_HOST_HISTORY = "";
        public static string REG_NAT_IP_POOL = "";

        public static string captureDevice = "Conexión de área local (Network adapter 'Realtek PCIe GBE Family Controller' on local host)";

        public static void GetMemoryValues()
        {
            Settings s = XBSLink.Properties.Settings.Default;

            REG_SPECIAL_MAC_LIST = s.REG_SPECIAL_MAC_LIST;
            REG_REMOTE_HOST_HISTORY = s.REG_REMOTE_HOST_HISTORY;
            REG_NAT_IP_POOL = s.REG_NAT_IP_POOL;

            captureDevice = s.REG_CAPTURE_DEVICE_NAME; //Capture device

            localIP = s.REG_LOCAL_IP; //Local IP
            local_Port = s.REG_LOCAL_PORT.ToString(); //PORT
            RemoteHost = s.REG_REMOTE_HOST;
            RemotePort = s.REG_REMOTE_PORT.ToString();
            UPnP = s.REG_USE_UPNP;
            enable_MAC_list = s.REG_ENABLE_SPECIAL_MAC_LIST;
            mac_restriction = s.REG_ONLY_FORWARD_SPECIAL_MACS;
            //checkBox_chatAutoSwitch.Checked = s.REG_CHAT_AUTOSWITCH;
            //checkBox_chat_notify.Checked = s.REG_CHAT_SOUND_NOTIFICATION;
            //checkBox_newNodeSound.Checked = s.REG_NEW_NODE_SOUND_NOTIFICATION;
            cloudlist = (s.REG_CLOUDLIST_SERVER.Length != 0) ? s.REG_CLOUDLIST_SERVER : xbs_cloudlist.DEFAULT_CLOUDLIST_SERVER; //Cloud list server
            useCloudServerForPortCheck = s.REG_USE_CLOUDLIST_SERVER_TO_CHECK_INCOMING_PORT;

            checkForUpdates = s.REG_CHECK4UPDATES;
            nat_enable = s.REG_NAT_ENABLE;
            filter_wellknown_ports = s.REG_FILTER_WELLKNOWN_PORTS;
            NAT_enablePS3mode = s.REG_PS3_COMPAT_MODE_ENABLE;
            excludeGatewayIPs = s.REG_SNIFFER_EXCLUDE_GATWAY_IPS;
            //checkBox_chat_nodeInfoMessagesInChat.Checked = s.REG_CHAT_NODEINFOMESSAGES;
            forward_all_high_port_broadcast = s.REG_SNIFFER_FORWARD_ALL_HIGH_PORT_BROADCASTS;
            //checkBox_minimize2systray.Checked = s.REG_MINIMIZE2SYSTRAY;

            preventSystemStandby = s.REG_PREVENT_SYSTEM_STANDY;
            
            //xbs_chat.message_when_nodes_join_or_leave = s.REG_CHAT_NODEINFOMESSAGES;
            //checkBox_showNewsFeed.Checked = s.REG_SHOW_NEWS_FEED;
            //textBox_newsFeedUri.Text = s.REG_NEWS_FEED_URI;
            //checkBox_switchToNewsTab.Checked = s.REG_NEWS_FEED_SWITCH_TO_TAB;

            //if (checkBox_enable_MAC_list.Checked)
            //    checkBox_mac_restriction.Enabled = true;


            chatNickname = (s.REG_CHAT_NICKNAME.Length != 0) ? s.REG_CHAT_NICKNAME : "DEFAULT_USER";

            //if (textBox_chatNickname.Text == "")
            //    textBox_chatNickname.Text = xbs_chat.STANDARD_NICKNAME;
        }

    

        //captureDevice.SelectedIndex
    }
}
