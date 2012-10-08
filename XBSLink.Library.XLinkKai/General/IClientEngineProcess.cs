using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XBSLink.General
{
    interface IClientEngineProcess
    {
        void SendMessage(xlink_msg udp_msg,string message);
        void ClientCloudJoin(xlink_msg udp_msg, string Cloud);
        void ClientCloudGet(xlink_msg udp_msg);
        void ClientCloudLeave(xlink_msg udp_msg);
        void ClientSendChatMessage(xlink_msg udp_msg,string Message);
        void ClientSendPM(xlink_msg udp_msg,string UserName, string Message);
        void ClientStart(xlink_msg udp_msg);
        void ClientStop(xlink_msg udp_msg);

    }
}
