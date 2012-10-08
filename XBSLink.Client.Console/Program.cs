using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XBSLink.General;

namespace XBSLink.Client.Console
{
    class Program
    {

        public static void ChangeChannel(XBSLink.Engine engine, string name, string max_nodes, string password, string num_nodes)
        {
            System.Console.WriteLine(String.Format("Entrando en > '{0}' ({3}/{1}) ...", name, max_nodes, password, num_nodes));
            engine.join_cloud(name, max_nodes, password);
            System.Console.WriteLine(String.Format("Ha entrado en '{0}'.",name));
        }

        public static void ListChannels(XBSLink.Engine engine)
        {
            System.Console.WriteLine("Listing clouds...");
            xbs_cloud[] clouds = engine.GetSendXlinkCloudList(null);

            if (clouds.Length > 0)
            {

                for (int i = 0; i < clouds.Length; i++)
                {
                    System.Console.WriteLine(String.Format("[{3}] > {0} ({2}/{1})", clouds[i].name, clouds[i].max_nodes, clouds[i].node_count, i, clouds[i].isPrivate));
                }

                System.Console.WriteLine("");
                System.Console.WriteLine("Select and option:");

                var pulsado = System.Console.ReadLine();
                var tecla = Int32.Parse(pulsado);

                ChangeChannel(engine, clouds[tecla].name, clouds[tecla].max_nodes.ToString(), "", clouds[tecla].node_count.ToString());

                //engine.join_cloud(clouds[tecla].name, 
                //    clouds[tecla].max_nodes.ToString(),
                //    "");

               
            }
            else
                System.Console.WriteLine("No existen canales activos.");

        }

        static void Main(string[] args)
        {

            XBSLink.Engine engine = new XBSLink.Engine();
            //eee.RefreshCloudListView += eee_RefreshCloudListView;
            //eee.AlertMessage += eee_AlertMessage;
           
            engine.Start();
            engine.engine_start(null);

            System.Console.WriteLine("Starting Engine");

            while (!engine.engine_started)
            {
            }

            System.Console.WriteLine("Engine Started.");
            
            //ListChannels(engine);

            

            System.Console.WriteLine("Pulse una tecla para finalizar la aplicacion.");
            System.Console.ReadLine();
          
        }
    }
}
