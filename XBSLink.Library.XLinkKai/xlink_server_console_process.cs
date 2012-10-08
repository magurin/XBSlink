using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using XBSLink.General;


namespace XBSLink.RemoteControl
{
    public class xlink_server_console_process //: IServerConsoleProcess
    {

        eXlinkPhase _actual_phase = eXlinkPhase.InitialStatus;

        public xlink_server _parent { get; set; }
        //public IServer _parent { get; set; }
        //public IPAddress _sender_ip;
        //public int _sender_port;

        public enum eXlinkPhase
        {
            InitialStatus = 0, Logged = 1, PrevChatMode = 2, ChatMode = 3, Disconnected = 4
        }
      
        public eXlinkPhase actual_phase
        {
            get
            {
                return _actual_phase;
            }
            set
            {
                _actual_phase = value;
            }
        }


        public void SetServer(xlink_server Parent)
        {
            _parent = Parent;
        }
        //public void SetServer(IServer Parent)
        //{
        //    _parent = Parent;
        //}
       
        public void ChangeSenderIPAddresPort(IPAddress console_ip_address, int console_port)
        {
            //_sender_ip = console_ip_address;
            //_sender_port = console_port;
        }

        #region PROCESS MESSAGE

        public void ProcessReceivedMessage(xlink_msg udp_msg)
        {
            //ChangeSenderIPAddresPort(udp_msg.src_ip, udp_msg.src_port);

            //Añadimos al listado de consolas MODIFICAR PARA QUE SOLO AÑADA CUANDO LOGUEE
            if (udp_msg.IsClientPacket())
                _parent.AddClient(udp_msg);
            else if (udp_msg.IsKayPacket())
                _parent.AddConsole(udp_msg);

     //CONSOLES ============================================================================================= 

            #region CONSOLES
            //=============================   PRIMER PAQUETE DISCOVER  =====================================
            if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_DISCOVER)
            {
                //actual_phase = eXlinkPhase.InitialStatus;
                KaySendMessageActualConsole(udp_msg, "KAI_CLIENT_ENGINE_HERE;");
               // _parent.ProcessDebugMessage(String.Format(" * XBOX -> DETECTED KAI_CLIENT_DISCOVER FROM {0}!!!!!! ADD CONFIG PARAM WITH CONSOLE IP !!!!!!!!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);

                //=============================   LOGIN DISCOVER =====================================
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_ATTACH)
            {
                KaySendMessageActualConsole(udp_msg, "KAI_CLIENT_ENGINE_IN_USE;");

                //_parent.ProcessDebugMessage(String.Format(" * XBOX -> DETECTED KAI_CLIENT_TAKEOVER FROM {0}!!!!!! ADD CONFIG PARAM WITH CONSOLE IP !!!!!!!!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_TAKEOVER)
            {
                KaySendMessageActualConsole(udp_msg, "KAI_CLIENT_ATTACH;");
               // _parent.ProcessDebugMessage(String.Format(" * XBOX -> SISTEMA DETECTADO! -> DETECTED KAI_CLIENT_ENGINE_IN_USE  {0}!!!!!! ADD CONFIG PARAM WITH CONSOLE IP !!!!!!!!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);

                //Lanzamos el evento de attach
                actual_phase = eXlinkPhase.Logged;

                //=============================   BOTON LOGIN =====================================
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_GETSTATE)
            {
              
                KaySendMessageActualConsole(udp_msg,new string[] {
                      "KAI_CLIENT_LOGGED_IN;",
                      "KAI_CLIENT_CODEPAGE; 0;",
                      "KAI_CLIENT_CODEPAGE; 0;",
                      "KAI_CLIENT_SESSION_KEY;3njEXQPnEQAZE2U7wHHKAGeP6hQ=;",
                      "KAI_CLIENT_VECTOR;XBSLINK;;",
                      "KAI_CLIENT_STATUS;XBSLink is Online..;",
                      String.Format("KAI_CLIENT_USER_DATA;{0};", _parent.KAI_CLIENT_LOCAL_NAME),
                      "KAI_CLIENT_ARENA_STATUS;1;1;",
                      "KAI_CLIENT_CONNECTED_MESSENGER;",
                      "KAI_CLIENT_CHATMODE;;",
                      "KAI_CLIENT_ADMIN_PRIVILEGES;;",
                      "KAI_CLIENT_MODERATOR_PRIVILEGES;;;",
                       xlink_client_messages_helper.KAI_CLIENT_ADD_CONTACT("magurin")
                  });

                //Creamos los clouds
                //SendMessageActualConsole(udp_msg, xlink_client_messages_helper.KAI_CLIENT_DETACH(_parent.KAI_CLIENT_LOCAL_DEVICE));
                KaySendMessageActualConsole(udp_msg, String.Format("KAI_CLIENT_LOCAL_DEVICE;{0};", _parent.KAI_CLIENT_LOCAL_DEVICE));
                //KaySendMessageActualConsole(udp_msg, xlink_client_messages_helper.KAI_CLIENT_ATTACH());
                Console.Write( String.Format("[XBOX] LOGIN KAI_CLIENT_GETSTATE  ({0})", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);
                _parent.ProcessLogin(udp_msg);

            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_LOGOUT)
            {

                //_parent.DeleteConsole(udp_msg);

                KaySendMessageActualConsole(udp_msg, xlink_client_messages_helper.KAI_CLIENT_DETACH(_parent.KAI_CLIENT_LOCAL_DEVICE));
                //=============================   LOGOUT =====================================

                actual_phase = eXlinkPhase.Disconnected;

                _parent.ProcessLogout(udp_msg);
                Console.Write(String.Format(" * XBOX -> CLOSSING APLICACION! -> DETECTED KAI_CLIENT_LOGOUT  {0}!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_CHATMODE)
            {
                //=============================   FASE DE CHAT =====================================
                UTF8Encoding oDecoding = new UTF8Encoding();
                var CommandMsg = oDecoding.GetString(udp_msg.Data);
                string[] parameters = CommandMsg.Split(';');
                if (parameters.Length > 2)
                {
                    //NOT USED 
                }

                //ENTRANDO EN MODO CHAT
                actual_phase = eXlinkPhase.ChatMode;

                Console.Write(String.Format(" * XBOX -> CHAT MODE -> DETECTED KAI_CLIENT_CHATMODE  {0}!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);
            }
            //================================= JOIN A CHANNEL ==========================
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_VECTOR)
            {
                UTF8Encoding oDecoding = new UTF8Encoding();
                var CommandMsg = oDecoding.GetString(udp_msg.Data);
                string[] parameters = CommandMsg.Split(';');
                if (parameters.Length > 2)
                {
                    var command = parameters[1].Trim();
                    if (command != "" && command != "Arena")
                    {
                        //DeleteAllSystemUsers();
                        _parent.ConsoleProcessJoinCloud(udp_msg,command, parameters[2]);
                    }
                    //else
                    //    AddSystemMainUsers();
                }


            }

            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_GET_VECTORS)
            {

                UTF8Encoding oDecoding = new UTF8Encoding();
                var CommandMsg = oDecoding.GetString(udp_msg.Data);
                string[] parameters = CommandMsg.Split(';');
                if (parameters.Length > 2)
                {
                    var command = parameters[1].Trim();
                    //if (command != "" && command != "Arena" && command != "XBSLINK")
                    //    DeleteAllSystemUsers();
                    //else if (command == "XBSLINK")
                    //    AddSystemMainUsers();
                }

                 Console.Write(String.Format(" * XBOX -> CHAT MODE -> DETECTED KAI_CLIENT_CHATMODE  {0}!", udp_msg.src_ip), xlink_msg.xbs_message_sender.XBOX);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_CHAT)
            {

                UTF8Encoding oDecoding = new UTF8Encoding();
                var CommandMsg = oDecoding.GetString(udp_msg.Data);
                string[] parameters = CommandMsg.Split(';');
                if (parameters.Length > 2)
                {
                    var command = parameters[1].Trim();
                    if (command != "")
                    {
                        _parent.ConsoleProcessChat(udp_msg,command);
                    }

                }

            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.KAI_CLIENT_PM)
            {
                UTF8Encoding oDecoding = new UTF8Encoding();
                var CommandMsg = oDecoding.GetString(udp_msg.Data);
                string[] parameters = CommandMsg.Split(';');
                if (parameters.Length > 2)
                {
                    var command = parameters[1].Trim();
                    if (command != "")
                    {
                        _parent.ConsoleProcessPM(udp_msg,parameters[1], parameters[2]);
                    }
                }

            }

  #endregion

    //CLIENTS ============================================================================================= 

            #region CLIENTS
            
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_CLOUDS_GET)
            {
                _parent.cClientGetClouds(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_CLOUD_JOIN)
            {
                _parent.cConsoleCloudJoin(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_CLOUD_LEAVE)
            {
                _parent.cConsoleCloudLeave(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_SEND_CHAT_MESSAGE)
            {
                _parent.cConsoleChatMessage(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_SEND_PM)
            {
                _parent.cConsoleSendPM(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_START)
            {
                _parent.cConsoleStart(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_STOP)
            {
                _parent.cConsoleStop(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_CLOUD_CREATE_JOIN)
            {
                _parent.cConsoleCloudCreateJoin(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_DISCOVER)
            {
                _parent.cConsoleDiscover(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_DISCONNECT)
            {
                _parent.cConsoleDisconnect(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_CONNECT)
            {
                _parent.cConsoleConnect(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_FAVORITE_ADD)
            {
                _parent.cConsoleFavoriteAdd(udp_msg);
            }
            else if (udp_msg.msg_type == xlink_msg.xbs_xlink_message_type.CLIENT_FAVORITE_DEL)
            {
                _parent.cConsoleFavoriteDel(udp_msg);
            }

        }
            #endregion

        #endregion

        #region Process Methods

        //public void KaySendPMMessage(xlink_msg node, string username, string message)
        //{
        //    // String.Format("KAI_CLIENT_PM;{0};{1};", username, message.Replace(";", "")
            
        //}

        //public void KaySendChatMessage(xlink_msg node, string username, string message)
        //{
            
        //}

        public void KaySendUpdateCloud(xlink_msg node, string CloudName, int Players, bool isPrivate, int MaxPlayers)
        {
            //"KAI_CLIENT_SUB_VECTOR;FIFA;5;XBSLINK;-1;6;",
            //"KAI_CLIENT_USER_SUB_VECTOR;GRAN TURISMO;3;XBSLINK;-1;6;Tengo una vaca",
            //SendMessageActualConsole(node,  String.Format("KAI_CLIENT_SUB_VECTOR_UPDATE;{0};{1};XBSLINK;", CloudName, Players));
            KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_SUB_VECTOR_UPDATE(CloudName, Players, isPrivate, MaxPlayers));
        }

        //public void KaySendCreateCloud(xlink_msg node, string CloudName, int Players, bool isPrivate, int MaxPlayers)
        //{

        //    KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_USER_SUB_VECTOR(CloudName, Players, isPrivate, MaxPlayers));
        //    //SendMessageActualConsole(node,String.Format("KAI_CLIENT_USER_SUB_VECTOR;{0};{1};XBSLINK;{2};{3};{4}", CloudName, Players, ((isPrivate) ? "1" : "-1"), MaxPlayers, ((isPrivate) ? "PASSWORD PROTECTED" : "Public Arena")));
        //}

        //public string[] KaySendDetach()
        //{
        //    return new string[]
        //    KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_DETACH(_parent.KAI_CLIENT_LOCAL_DEVICE));
        //    //SendMessageActualConsole(node,String.Format("KAI_CLIENT_DETACH;{0};", _parent.KAI_CLIENT_LOCAL_DEVICE));
        //    KaySendMessageActualConsole(node, xlink_client_messages_helper.KAI_CLIENT_ATTACH());
        //    //SendMessageActualConsole(node,"KAI_CLIENT_ATTACH;");
        //}

        /// <summary>
        /// LAUNCHED FROM ENGINE
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="username"></param>
        //public void KayLeaveUserFromVector(xlink_msg msg, string username)
        //{

        //    KaySendMessageActualConsole(msg, xlink_client_messages_helper.KAI_CLIENT_LEAVES_CHAT(username));
        //    //SendMessageActualConsole( msg,String.Format("KAI_CLIENT_LEAVES_CHAT;General Chat;{0};", username));

        //    KaySendMessageActualConsole(msg, xlink_client_messages_helper.KAI_CLIENT_LEAVES_VECTOR(username));
        //    //SendMessageActualConsole( msg,String.Format("KAI_CLIENT_LEAVES_VECTOR;{0};", username));

        //   // _parent.SendMessageToQueue(xlink_client_messages_helper.ServerUserOffline(username));


        //}

        /// <summary>
        /// LAUNCHED FROM ENGINE
        /// </summary>
        /// <param name="username"></param>
        //public void KayLeaveUserFromVector(string username)
        //{
        //    KayLeaveUserFromVector(null, username);
        //}

        /// <summary>
        /// LAUNCHED FROM ENGINE
        /// </summary>
        /// <param name="username"></param>
        //public void KayJoinUserToVector(xlink_msg msg, string username)
        //{

        //    KaySendMessageActualConsole(msg, xlink_client_messages_helper.KAI_CLIENT_JOINS_VECTOR(username));

        //    //SendMessageActualConsole(msg,String.Format("KAI_CLIENT_JOINS_VECTOR;{0};", username));

        //    KaySendMessageActualConsole(msg, xlink_client_messages_helper.KAI_CLIENT_JOINS_CHAT(username));

        //    //SendMessageActualConsole(msg,String.Format("KAI_CLIENT_JOINS_CHAT;General Chat;{0};", username));

        //    _parent.SendMessageToQueueClientsConnected(xlink_client_messages_helper.ServerUserOnline(username));

        //}

        /// <summary>
        /// LAUNCHED FROM ENGINE
        /// </summary>
        /// <param name="username"></param>
        //public void KayJoinUserToVector(string username)
        //{
        //    KayJoinUserToVector(null, username);
        //}


        public void KaySendMessageActualConsole(xlink_msg udp_msg, string message)
        {
            _parent.SendMessageToQueue(udp_msg, message, xlink_server.TypeSendQueueTo.SendToAllConsoles,false );

            //Evento de Engine//Fuera aplicativo
            _parent.ConsoleProcessSendMessage(udp_msg,message);
        }

        //public void SendMessageActualClients(xlink_msg udp_msg, string message)
        //{
        //    _parent.SendMessageToQueueClientsConnected(message);
        //}


       //public void SendMessageActualConsole(string message)
       // {
       //     _parent.SendMsgCola(new xlink_msg(  _sender_ip, _sender_port, message));
       //     _parent.ConsoleProcessSendMessage(message, _sender_ip, _sender_port);
       // }

        //public void SendMessageActualClients(xlink_msg msg, string[] messages)
        //{
        //    foreach (var tmp in messages)
        //    {
        //        SendMessageActualClients(msg, tmp);
        //    }
        //}

        public void KaySendMessageActualConsole(xlink_msg msg, string[] messages)
        {
            foreach (var tmp in messages)
            {
                KaySendMessageActualConsole(msg, tmp);
            }
        }

       


        #endregion


    }

}