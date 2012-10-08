using System;
using System.Collections.Generic;
using System.Text;


namespace XBSLink.General
{
   public class xlink_client_messages_clouds_helper : xlink_client_messages_helper
    {

        #region GetClouds


        public static string GetClouds(xbs_cloud[] clouds)
        {
            return xlink_msg.getHeaderMessageFromType(xlink_msg.xbs_xlink_message_type.SERVER_GET_CLOUDS) + GetStrFromArrayClouds(clouds);
        }

         public static string GetClouds(List<xbs_cloud> clouds)
        {
            return GetClouds(clouds.ToArray());
        }

        public static xbs_cloud[] GetArrayCloudsFromStr(string clouds)
        {
            var clouds_splitted = clouds.Split('|');
            xbs_cloud[] dev = new xbs_cloud[clouds_splitted.Length];
            for (int i = 0; i < clouds_splitted.Length; i++)
            {
                var parameters = clouds_splitted[i].Split(';');
                dev[i] = new xbs_cloud(parameters[0], int.Parse(parameters[1]), int.Parse(parameters[2]), bool.Parse(parameters[3]));
            }
            return dev;
        }

        public static string SERVER_ADD_CLOUD(xbs_cloud cloud)
        { 
            return xlink_msg.getHeaderMessageFromType(xlink_msg.xbs_xlink_message_type.SERVER_ADD_CLOUD) + GetStrCloudLine(cloud);
        }

        public static string GetStrFromArrayClouds(List<xbs_cloud> clouds)
        {
            return GetStrFromArrayClouds(clouds.ToArray());
        }

        private static string GetStrCloudLine(xbs_cloud cloud)
        {
            return String.Format("{0};{1};{2};{3}", cloud.name, cloud.node_count.ToString(), cloud.max_nodes.ToString(), cloud.isPrivate);
        }

        private static string GetStrFromArrayClouds(xbs_cloud[] clouds)
        {
            string dev = "";
            //var clouds = cloudlist.getCloudlistArray();
            for (int i = 0; i < clouds.Length; i++)
            {
                if (i > 0) dev += "|";
                dev +=  GetStrCloudLine( clouds[i]);
            }
            return dev;
        }

        #endregion

    }
}
