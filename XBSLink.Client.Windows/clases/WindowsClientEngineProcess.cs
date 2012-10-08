using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net.UDP;
using XBSLink.General;

namespace XBSLink.clases
{
    class WindowsClientEngineProcess 
    {

        string server_ip = "10.67.2.1";
        public int REMOTE_SERVER_PORT = 34522;
        public int LOCAL_CLIENT_PORT = 34523;

        UdpServer s;

        public WindowsClientEngineProcess(WindowsClientEngine Engine)
        {
            s = Engine.m_pUdpServer;
            //server_ip = ServerIp;
            //server_port = server_port;
        }

        public WindowsClientEngineProcess(UdpServer socket, string ServerIp, int ServerPort)
        {
            s = socket;
            server_ip = ServerIp;
            REMOTE_SERVER_PORT = ServerPort;
        }

        public void SendMessage(string message)
        {
            WindowsClientEngine.SendMessage(s, server_ip, REMOTE_SERVER_PORT, message);
        }

        public void ClientCloudJoin(string Cloud)
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientCloudJoin(Cloud));
        }

        public void ClientCloudGet()
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientCloudGet());
        }

        public void ClientCloudLeave()
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientCloudLeave());
        }


        public void ClientSendChatMessage(string Message)
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientSendChatMessage(Message));

        }

        public void ClientSendPM(string UserName, string Message)
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientSendPM(UserName, Message));
        }

        public void ClientStart()
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientStart());
        }

        public void ClientStop()
        {
            SendMessage(xlink_client_messages_clouds_helper.ClientStop());
        }

    }
}
