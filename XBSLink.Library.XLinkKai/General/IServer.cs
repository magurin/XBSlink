using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using XBSLink.General.Delegates.Console;
using XBSLink.General.Delegates.Xlink;

namespace XBSLink.General
{
   public interface IServer
   {

       #region Xlink Events
       event XlinkDebugMessageHandler XlinkDebugMessage;
     event XlinkConsoleSendMessageHandler XlinkConsoleSendMessage;
     event XlinkConsoleJoinCloudHandler XlinkConsoleJoinCloud;
     event XlinkConsoleLogoutHandler XlinkConsoleLogout;
     event XlinkConsoleLoginHandler XlinkConsoleLogin;
     event XlinkChatHandler XlinkConsoleChat;
     event XlinkConsolePMHandler XlinkConsolePM;

    #endregion

    #region Console Events



     event ConsoleGetCloudsHandler ConsoleGetClouds;
     event ConsoleCloudJoinHandler ConsoleCloudJoin;
     event ConsoleCloudLeaveHandler ConsoleCloudLeave;
     event ConsoleChatMessageHandler ConsoleChatMessage;
     event ConsoleSendPMHandler ConsoleSendPM;
     event ConsoleStartHandler ConsoleStart;
     event ConsoleStopHandler ConsoleStop;

         #endregion

     IServerConsoleProcess xLinkMsgProcess { get; set; }

     string KAI_CLIENT_LOCAL_DEVICE { get; set; }
     string KAI_CLIENT_LOCAL_NAME { get; set; }


    void XBS_SendChat(xlink_msg udp_msg,string user_name, string Text);
    void XBS_SendPM(xlink_msg udp_msg, string user_name, string Text);
    void XBS_ChannelCreate(xlink_msg udp_msg, string CloudName, int Players, bool isPrivate, int MaxPlayers);
    void XBS_Detach(xlink_msg udp_msg);
    void XBS_LeaveUser(xlink_msg udp_msg, string username);
    void XBS_JoinUser(xlink_msg udp_msg, string username);

     void Start();
     


     void Configure(string local_ip_address, IServerConsoleProcess process);

     void SendMsgThread(xlink_msg msg);
      void  SendMsgCola(xlink_msg msg);
      void ProcessLogout(xlink_msg udp_msg);
      void ProcessLogin(xlink_msg udp_msg);
      void ProcessDebugMessage(string Message, xlink_msg.xbs_message_sender sender);
      void ConsoleProcessJoinCloud(xlink_msg udp_msg, string CloudName);
      void ConsoleProcessJoinCloud(xlink_msg udp_msg, string CloudName, string CloudPassword);
      void ConsoleProcessChat(xlink_msg udp_msg, string Message);
      void ConsoleProcessPM(xlink_msg udp_msg, string Username, string Message);
      void ConsoleProcessSendMessage(xlink_msg udp_msg, string message, IPAddress console_ip_address, int console_port);

      void cConsoleStop(xlink_msg msg);
      void cConsoleStart(xlink_msg msg);
      void cConsoleSendPM(xlink_msg msg);
      void cConsoleGetClouds(xlink_msg msg);
      void cConsoleCloudJoin(xlink_msg msg);
      void cConsoleCloudLeave(xlink_msg msg);
      void cConsoleChatMessage(xlink_msg msg);
    }
}
