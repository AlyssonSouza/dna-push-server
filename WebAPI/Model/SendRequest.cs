using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Model
{
    public class SendRequest
    {
        public List<DeviceEntity> DeviceList { get; set; }
        public PushNotificationEntity Push { get; set; }
        public string Client { get; set; }
    }
}