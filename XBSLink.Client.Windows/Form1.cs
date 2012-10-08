using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LumiSoft.Net.UDP;
using XBSLink.clases;
using XBSLink.General;



namespace XBSLinkClient
{
    public partial class Form1 : Form
    {

        string xbs_server = "10.67.2.1";
        //string client = "10.67.2.1";


        WindowsClientEngine clientEngine;
        WindowsClientEngineProcess clientEngineProcess;
        public Form1()
        {
            InitializeComponent();

            InitializeUdpServer();

            //InitializateChatAudio();
        }
       
        void InitializeUdpServer()
        {
            clientEngine = new WindowsClientEngine(xbs_server);
            clientEngine.ProcessReceivedMessage += clientEngine_ProcessReceivedMessage;
            clientEngine.XlinkDebugMessage += clientEngine_XlinkDebugMessage;
            clientEngine.Start();

            //Lanzamos los canales
            clientEngineProcess = new WindowsClientEngineProcess(clientEngine);
            

        }

        void clientEngine_XlinkDebugMessage(string message_debug, xlink_msg.xbs_message_sender sender)
        {
            
        }


        void CloudListViewInit()
        {
            listView_clouds.Items.Clear();
            ImageList il = new ImageList();
            il.Images.Add(XBSLink.Properties.Resources.icon_key);
            listView_clouds.SmallImageList = il;
        }


        void CloudListViewFill(xbs_cloud[] data)
        {
            if (data.Length > 0)
            {
                CloudListViewInit();
                foreach (xbs_cloud cloud in data)
                {
                    ListViewItem lv_item = new ListViewItem(cloud.name);
                    lv_item.SubItems.Add(cloud.node_count.ToString());
                    lv_item.SubItems.Add(cloud.max_nodes.ToString());
                    if (cloud.isPrivate)
                        lv_item.ImageIndex = 0;
                    listView_clouds.Items.Add(lv_item);
                }
                // toolTip2.Show(clouds.Length + " clouds loaded.", buttonLoadCloudlist, 0, -20, 2000);
            }
            //else
            //    toolTip2.Show("no clouds available on server.", buttonLoadCloudlist, 0, -20, 2000);
                    
        }



        void clientEngine_ProcessReceivedMessage(xlink_msg msg)
        {

            if (msg != null)
            {

            switch (msg.msg_type)
            {
             
                case xlink_msg.xbs_xlink_message_type.SERVER_GET_CLOUDS:

                    xlink_get_clouds_message tmp = new xlink_get_clouds_message(msg);
                    Invoke((MethodInvoker)delegate
                    {
                        CloudListViewFill(tmp.Clouds);
                    });

                    break;
                default:
                    break;
            }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientEngineProcess.ClientCloudGet();
        }

    }
}

//void InitializateChatAudio()
//{
//node_client = new XBSLink.Node.xbs_node(IPAddress.Parse(client), 22);
//node_client = new XBSLink.Node.xbs_node(IPAddress.Parse(server), 22);

//Client = new chat_audio();
//Client.clientAccepted += Client_clientAccepted;
//Client.clientConnectServer += Client_clientConnectServer;
//Client.clientDisconnect += Client_clientDisconnect;
//Client.clientServerDisconnect += Client_clientServerDisconnect;
//Client.ChangeType(uType.client);

//Server = new chat_audio();
//Server.serverAcceptNode += Server_serverAcceptNode;
//Server.serverCreateServer += Server_serverCreateServer;
//Server.serverClientDisconnect += Server_serverClientDisconnect;
//Server.ChangeType(uType.server);


//Server.ServerCreateServer();
//

// }


//void Server_serverCreateServer()
//{
//   // textBox1.AppendText("Servidor creado: " + node_server.ip_public + Environment.NewLine);
//    Client.ClientConnectServer(node_server);
//}

//void Client_clientConnectServer(XBSLink.Node.xbs_node server_node)
//{
//   // textBox1.AppendText("Connectando cliente a servidor: " + node_server.ip_public + Environment.NewLine);
//    Server.ServerAcceptNode(node_client);
//}

//   void Server_serverAcceptNode(XBSLink.Node.xbs_node client)
//{
//   // textBox1.AppendText("SERVIDOR PETICION ACEPTADA CLIENTE:" + node_client.ip_public + Environment.NewLine);
//    Client.ClientAccepted(node_server);
//    //throw new NotImplementedException();
//}

//void Server_serverClientDisconnect(XBSLink.Node.xbs_node client)
//{
//    throw new NotImplementedException();
//}




//void Client_clientServerDisconnect(XBSLink.Node.xbs_node client)
//{
//    //throw new NotImplementedException();
//}

//void Client_clientDisconnect()
//{
//    //throw new NotImplementedException();
//}



//void Client_clientAccepted(XBSLink.Node.xbs_node client)
//{
//    throw new NotImplementedException();
//}
