using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XBSLink.General
{
    public interface IServerConsoleProcess
    {
        XBSLink.RemoteControl.xlink_server_console_process.eXlinkPhase actual_phase { get; set; }
        void SendPMMessage(xlink_msg udp_msg, string username, string message);
        void SendChatMessage(xlink_msg udp_msg, string username, string message);
        void SendUpdateCloud(xlink_msg udp_msg, string CloudName, int Players, bool isPrivate, int MaxPlayers);
        void SendCreateCloud(xlink_msg udp_msg, string CloudName, int Players, bool isPrivate, int MaxPlayers);
        void SendDetach(xlink_msg udp_msg);
        
        void SendMessageActualConsole(xlink_msg udp_msg, string message);
        void SendMessageActualConsole(xlink_msg udp_msg, string[] messages);
        void LeaveUserFromVector(xlink_msg udp_msg, string username);
        void JoinUserToVector(xlink_msg udp_msg, string username);
        void ProcessReceivedMessage(xlink_msg udp_msg);
        void SetServer(IServer Parent);
    }
}
