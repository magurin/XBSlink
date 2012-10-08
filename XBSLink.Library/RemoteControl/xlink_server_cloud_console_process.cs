using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XBSLink.General;
using XBSLink.RemoteControl;

namespace XBSLink.Library.RemoteControl
{
    public class xlink_server_cloud_console_process : xlink_server_console_process
    {

        public void SendCreateCloud(xlink_msg udp_msg, xbs_cloud Canal)
        {
            SendCreateCloud(udp_msg,_parent, Canal);
        }

        public void SendActualCloudList(xlink_msg udp_msg, List<xbs_cloud> oChannels)
        {
            SendActualCloudList(udp_msg,_parent, oChannels);
        }


        public void SendActualCloudList(xlink_msg udp_msg, xbs_cloud[] oChannels)
        {
            SendActualCloudList(udp_msg,_parent, oChannels);
        }

        public static void SendCreateCloud(xlink_msg udp_msg, xlink_server server, xbs_cloud Canal)
        {
            server.XBS_ChannelCreate(udp_msg, Canal.name, Canal.node_count, Canal.isPrivate, Canal.max_nodes);
        }

        public static void SendActualCloudList(xlink_msg udp_msg, xlink_server server, List<xbs_cloud> oChannels)
        {
            //Si hay los eliminamos
            foreach (var item in oChannels)
                SendCreateCloud(udp_msg,server, item);
        }

        public static void SendActualCloudList(xlink_msg udp_msg, xlink_server server, xbs_cloud[] oChannels)
        {
            //Si hay los eliminamos
            foreach (var item in oChannels)
                SendCreateCloud(udp_msg,server, item);
        }
    }
}
