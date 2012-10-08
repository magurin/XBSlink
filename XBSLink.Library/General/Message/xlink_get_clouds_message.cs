﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using XBSLink.General;

namespace XBSLink.General
{
    public class xlink_get_clouds_message : xlink_msg
    {

        public xbs_cloud[] _clouds { get; set; }

        public void Assign(string Parameters)
        {
            _clouds = xlink_client_messages_clouds_helper.GetArrayCloudsFromStr(Parameters);
        }

        public xlink_get_clouds_message(xlink_msg msg)
            : base(msg)
        {
            Assign(parameters_msg);
        }

        public xlink_get_clouds_message(string ipAddress, int port, string msgText)
            : base(ipAddress, port, msgText)
        {
            Assign(parameters_msg);
        }

        public xlink_get_clouds_message(IPAddress ipAddress, int port, string msgText)
            : base(ipAddress, port, msgText)
        {
            Assign(parameters_msg);
        }

        void RefreshContent()
        {
            Data = getUTF8BytesFromString(XBSLink.General.xlink_client_messages_clouds_helper.GetClouds(_clouds));
        }


    }

}
