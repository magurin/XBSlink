using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XBSLink.Node;

namespace XBSLink.Library.ChatAudio
{

    public enum uClientStatus : int
    {
        Disconnected = 0, WaitingAutorization = 1, Connected = 2
    }

    public enum uType : int
    {
        none = 0, server = 1, client = 2
    }

    public class chat_audio
    {


        public delegate void serverCreateServerHandler();
        public event serverCreateServerHandler serverCreateServer;

        public delegate void clientConnectServerHandler(xbs_node server_node);
        public event clientConnectServerHandler clientConnectServer;

        public delegate void ServerAcceptNodeHandler(xbs_node client);
        public event ServerAcceptNodeHandler serverAcceptNode;

        public delegate void clientAcceptedHandler(xbs_node client);
        public event clientAcceptedHandler clientAccepted;

        public delegate void clientDisconnectHandler();
        public event clientDisconnectHandler clientDisconnect;

        public delegate void clientServerDisconnectHandler(xbs_node client);
        public event clientServerDisconnectHandler clientServerDisconnect;

        public delegate void serverClientDisconnectHandler(xbs_node client);
        public event serverClientDisconnectHandler serverClientDisconnect;

        List<xbs_node> nodes;

        xbs_node server;  
       
        uType uType;

        uClientStatus uStatus;

        public chat_audio()
        {
            uType = uType.none;
            nodes = new List<xbs_node>();

            server = null;
        }

        public void ResetValues()
        {
            uType = uType.none;
            uStatus = uClientStatus.Disconnected;
            nodes = new List<xbs_node>();
            server = null;
        }

        public void ChangeType(uType newState)
        {
            uType = newState;
            ResetValues();
            //Reenviar a todos el cambio de estado
        }

        public void ChangeState(uClientStatus newState)
        {
            uStatus = newState;
            //ResetValues();
            //Reenviar a todos el cambio de estado
        }

        #region Client Peticiones

        public void ServerCreateServer()
        {
            ChangeType(uType.server);

            if (serverCreateServer != null)
                serverCreateServer();
        }

        public void ClientConnectServer(xbs_node node_to_connect)
        {

            ChangeType(uType.client);
            //Connectamos con el servidor
          
                if (uStatus != uClientStatus.Disconnected)
                    ChangeState(uClientStatus.Disconnected);

                server = node_to_connect;

                uStatus = uClientStatus.WaitingAutorization;

                if (clientConnectServer != null)
                    clientConnectServer(node_to_connect);
        }

        public void ServerAcceptNode(xbs_node node_to_connect)
        {
            if (uType == ChatAudio.uType.server)
            {
                if (!CheckIsNode(node_to_connect))
                    nodes.Add(node_to_connect);
                //Enviamos la autorización de aceptada
                //SendAuth
                if (serverAcceptNode != null)
                    serverAcceptNode(node_to_connect);
            }
            else
                Console.Write("No está en modo servidor");
        }

        public void ClientAccepted(xbs_node node_to_connect)
        {
            if (uType == ChatAudio.uType.client)
            {

                if (CheckSameNode(server, node_to_connect))
                {
                    uStatus = uClientStatus.Connected;
                }

                //Lanzamos el evento
                if (clientAccepted != null)
                    clientAccepted(node_to_connect);

            }
            else
                Console.Write("No está en modo cliente");
        }

        public void ServerClientDisconnect(xbs_node client)
        {
            if (uType == ChatAudio.uType.server)
            {

                lock (nodes)
                {
                    nodes.Remove(GetNode(client));
                }
                

                if (serverClientDisconnect != null)
                    serverClientDisconnect(client);
            }
            else
                Console.Write("No está en modo servidor");
        }

        public void ClientDisconnect()
        {
            if (uType == ChatAudio.uType.client)
            {
                Disconnect();

                if (clientDisconnect != null)
                    clientDisconnect();
            }
            else
                Console.Write("No está en modo cliente");
        }

        public void ClientServerDisconnect(xbs_node node_to_connect)
        {
            if (CheckSameNode(server, node_to_connect))
                Disconnect();

            if (clientServerDisconnect != null)
                clientServerDisconnect(node_to_connect);
        }

	#endregion

        void Disconnect()
        {
            uStatus = uClientStatus.Disconnected;
            ResetValues();
        }

        bool CheckSameNode(xbs_node first, xbs_node second)
        {
            return (first.ip_public == second.ip_public);
        }

        bool CheckIsNode(xbs_node node_to_connect)
        {
            return (GetNode(node_to_connect) != null);
        }

        xbs_node GetNode(xbs_node node_to_connect)
        {
            foreach (var item in nodes)
            {
                if (CheckSameNode(item, node_to_connect))
                    return item;
            }
            return null;
        }
     

    }
}
