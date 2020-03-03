using Infrastructure.Enums;
using Model.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
//using MoonAPNS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Push
{
    /// <summary>
    /// push helper class
    /// </summary>
    public class PushHelper
    {
        /// <summary>
        /// push delegate
        /// </summary>
        /// <param name="oldToken">old token</param>
        /// <param name="newToken">new token</param>
        /// <param name="devicePlatform">device platform</param>
        public delegate void PushDelegate(string oldToken, string newToken, int devicePlatform);
        /// <summary>
        /// subscription expired delegate
        /// </summary>
        /// <param name="oldToken"></param>
        /// <param name="devicePlatform"></param>
        public delegate void SubscriptionExpiredDelegate(string oldToken, int devicePlatform);
        /// <summary>
        /// subscription changed event
        /// </summary>
        public static event PushDelegate SubscriptionChanged;
        /// <summary>
        /// subscription expired event
        /// </summary>
        public static event SubscriptionExpiredDelegate SubscriptionExpired;
        public static Logger log;
        /// <summary>
        /// send push notification
        /// </summary>
        /// <param name="token">device token</param>
        /// <param name="device">device platform</param>
        /// <param name="message">push message</param>
        /// <returns></returns>
        public static bool SendPush(string token, DevicePlatform device, string message, int pushId)
        {
            bool messageSent = false;
            try
            {
                switch (device)
                {
                    case DevicePlatform.IOS:
                        messageSent = true; //SendIosPush(token, message, pushId);
                        break;
                    case DevicePlatform.ANDROID:
                        messageSent = true; //SendAndroidPush(token, message, pushId);
                        break;
                }
            }
            catch
            {
                //TODO:Log error
                messageSent = false;
            }

            return (messageSent);
        }

        /// <summary>
        /// send push notification
        /// </summary>
        /// <param name="push"></param>
        /// <param name="deviceLst"></param>
        /// <returns></returns>
        public static bool SendPush(PushNotificationEntity push, List<DeviceEntity> deviceLst, string Client, string Environment)
        {
            log = new Logger();
            bool messageSent = false;
            try
            {
                List<DeviceEntity> iosDeviceLst = deviceLst.Where(d => d.Platform == (int)DevicePlatform.IOS).ToList();
                List<DeviceEntity> androidDeviceLst = deviceLst.Where(d => d.Platform == (int)DevicePlatform.ANDROID).ToList();

                //send Ios
                if (iosDeviceLst.Count > 0)
                {
                    //start apns broker Android
                    ApnsConfiguration apnsConfig = GetApnsConfiguration(Client, Environment);
                    var apnsBroker = new ApnsServiceBroker(apnsConfig);
                    apnsBroker.OnNotificationFailed += IoSNotificationFailed;
                    apnsBroker.OnNotificationSucceeded += IoSNotificationSucceeded;
                    apnsBroker.Start();

                    foreach (var device in iosDeviceLst)
                    {
                        SendIosPush(push, device.Token, apnsBroker);
                    }
                    //stop service brokers
                    apnsBroker.Stop();
                    log.Debug("Send!!!");
                }

                //send android
                if (androidDeviceLst.Count > 0)
                {
                    //start fcm broker Android
                    GcmConfiguration gcmConfig = GetGcmConfiguration(Client);
                    var gcmBroker = new GcmServiceBroker(gcmConfig);
                    gcmBroker.OnNotificationFailed += GcmNotificationFailed;
                    gcmBroker.OnNotificationSucceeded += GcmNotificationSucceeded;
                    gcmBroker.Start();
                    List<string> Devices = new List<string>();
                    
                    foreach (var item in androidDeviceLst)
                    {
                        Devices.Add(item.Token);
                    }

                    SendAndroidPush(push, Devices, gcmBroker);

                    gcmBroker.Stop();
                }

                messageSent = true;
            }
            catch (Exception)
            {
                //TODO: Log Error
            }

            return (messageSent);
        }

        /// <summary>
        /// gcm notification succeeded
        /// </summary>
        /// <param name="notification">notification</param>
        private static void GcmNotificationSucceeded(GcmNotification notification)
        {
            //TODO: Log success
        }

        /// <summary>
        /// gcm notification failed event
        /// </summary>
        /// <param name="notification">notification</param>
        /// <param name="exception">exception</param>
        private static void GcmNotificationFailed(GcmNotification notification, AggregateException exception)
        {

            exception.Handle(ex =>
            {
                if (ex is GcmNotificationException)
                {
                    var notificationException = (GcmNotificationException)ex;
                    //SendMail(notification.Data.ToString(), string.Format("NotificationException => MessageID: {0} - Description: {1}",
                    //    notificationException.Message, notificationException.Description));
                }
                else if (ex is GcmMulticastResultException)
                {
                    var multicastException = (GcmMulticastResultException)ex;
                    foreach (var failed in multicastException.Failed)
                    {
                        var notificationKey = failed.Key;
                        var desc = failed.Value;
                        //SendMail(notification.Data.ToString(), string.Format("MulticastException => MessageID: {0} - Description: {1}", 
                        //    notificationKey.MessageId, desc.Message));

                    }
                }
                else if (ex is DeviceSubscriptionExpiredException)
                {
                    var deviceExpiredException = (DeviceSubscriptionExpiredException)ex;

                    var oldId = deviceExpiredException.OldSubscriptionId;
                    var newId = deviceExpiredException.NewSubscriptionId;

                    if (!string.IsNullOrWhiteSpace(newId))
                    {
                        if (null != SubscriptionChanged)
                            SubscriptionChanged(oldId, newId, (int)DevicePlatform.ANDROID);
                    }
                    else
                    {
                        if (null != SubscriptionChanged)
                            SubscriptionExpired(oldId, (int)DevicePlatform.ANDROID);
                    }
                }

                return (true);
            });
        }

        /// <summary>
        /// send push notification to android devices
        /// </summary>
        /// <param name="token">device token</param>
        /// <param name="message">message</param>        
        private static void SendAndroidPush(PushNotificationEntity push, List<string> tokens, GcmServiceBroker gcmBroker)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "title" , push.Title },
                    { "body" , push.Description }
                });

                var notification = new GcmNotification
                {
                    Tag = push.Title,
                    RegistrationIds = tokens,
                    Notification = JObject.Parse(json)
                };

                if (null != gcmBroker)
                {
                    if (notification.IsDeviceRegistrationIdValid())
                        gcmBroker.QueueNotification(notification);
                }
            }
            catch
            {
                //TODO: Log Exception
            }
        }

        #region send Android by List

        ///// <summary>
        ///// send push notification to android devices
        ///// </summary>
        ///// <param name="deviceTokenLst">device token list</param>
        ///// <param name="message">message</param>
        ///// <param name="pushId">push id</param>
        ///// <param name="gcmBroker">gcm broker</param>
        //private static void SendAndroidPush(PushNotificationEntity push, List<string> deviceTokenLst, GcmServiceBroker gcmBroker)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(new Dictionary<string, object>
        //        {
        //            { "title" , push.Title },
        //            { "body" , push.Description }
        //        });

        //        var notification = new GcmNotification
        //        {
        //            Tag = push.Title,
        //            RegistrationIds = deviceTokenLst,
        //            Notification = JObject.Parse(json)
        //        };


        //        if (null != gcmBroker)
        //        {
        //            if (notification.IsDeviceRegistrationIdValid())
        //            {
        //                gcmBroker.QueueNotification(notification);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        //TODO: Log Exception
        //    }
        //}
        #endregion

        /// <summary>
        /// get google push notification configuration
        /// </summary>
        /// <returns>apns configuration</returns>
        private static GcmConfiguration GetGcmConfiguration(string Client)
        {
            string gcmAuthorization = string.Empty;
            string senderId = string.Empty;
            string applicationID = string.Empty;
            switch (Client)
            {
                case "Hirota":
                    {
                        gcmAuthorization = ConfigurationManager.AppSettings["GcmAuthorization" + Client];
                        senderId = ConfigurationManager.AppSettings["GcmSenderId" + Client];
                        applicationID = ConfigurationManager.AppSettings["applicationID" + Client];
                        break;
                    }
                case "Muffato":
                    {
                        gcmAuthorization = ConfigurationManager.AppSettings["GcmAuthorization" + Client];
                        senderId = ConfigurationManager.AppSettings["GcmSenderId" + Client];
                        applicationID = ConfigurationManager.AppSettings["applicationID" + Client];
                        break;
                    }
                case "Wake Me Up":
                    {
                        gcmAuthorization = ConfigurationManager.AppSettings["GcmAuthorization" + Client];
                        senderId = ConfigurationManager.AppSettings["GcmSenderId" + Client];
                        applicationID = ConfigurationManager.AppSettings["applicationID" + Client];
                        break;
                    }
                case "Dengo":
                    {
                        gcmAuthorization = ConfigurationManager.AppSettings["GcmAuthorization" + Client];
                        senderId = ConfigurationManager.AppSettings["GcmSenderId" + Client];
                        applicationID = ConfigurationManager.AppSettings["applicationID" + Client];
                        break;
                    }
                case "Telhanorte":
                    {
                        gcmAuthorization = ConfigurationManager.AppSettings["GcmAuthorization" + Client];
                        senderId = ConfigurationManager.AppSettings["GcmSenderId" + Client];
                        applicationID = ConfigurationManager.AppSettings["applicationID" + Client];
                        break;
                    }
            }

            string GcmUrl = ConfigurationManager.AppSettings["GcmUrl"];

            // Configuration
            var config = new GcmConfiguration(senderId, gcmAuthorization, applicationID)
            {
                GcmUrl = GcmUrl
            };

            return (config);
        }

        /// <summary>
        /// send push notification to ios devices
        /// </summary>
        /// <param name="token">device token</param>
        /// <param name="message">message</param>        
        private static void SendIosPush(PushNotificationEntity push, string token, ApnsServiceBroker apnsBroker)
        {
            try
            {
                if (null != apnsBroker)
                {

                    var alert = new Dictionary<string, object>
                    {
                        { "title", push.Title },
                        { "body", push.Description },
                        { "actionlockey", "PLAY" }
                    };

                    var aps = new Dictionary<string, object>
                    {
                        { "alert", alert },
                        { "badge", 5 },
                        { "sound", "chime.aiff" }
                    };

                    var payload = new Dictionary<string, object>
                    {
                        { "aps", aps },
                        { "confId", "20" },
                        { "pageFormat", "Webs" },
                        { "pageTitle", "Evalu" },
                        { "webviewURL", ""},
                        { "notificationBlastID", "" },
                        { "pushtype", "" },
                        { "content-available", "" }
                    };

                    var jsonx = JsonConvert.SerializeObject(payload);
                    var notification = new ApnsNotification()
                    {
                        DeviceToken = token,
                        Payload = JObject.Parse(jsonx)
                    };

                    if (notification.IsDeviceRegistrationIdValid())
                    {
                        apnsBroker.QueueNotification(notification);
                        //log.Debug("start Send");
                    }
                }
            }
            catch
            {
                //TODO: Log Exception
            }
        }

        /// <summary>
        /// check ios device token availability
        /// </summary>
        public static void CheckIosDeviceTokenAvailability(string Client, string Environment)
        {
            log = new Logger();
            try
            {
                ApnsConfiguration config = GetApnsConfiguration(Client, Environment);

                var feedback = new FeedbackService(config);
                feedback.FeedbackReceived += VerifyIosAvailability;
                feedback.Check();
                log.Debug("saiu aqui");
            }
            catch (Exception ex)
            {
                log.Debug("deu merda aqui!!");
                log.Debug(ex.Message + "||" + ex.InnerException);
                throw new ApplicationException("2");
                //LOG Exception
            }
        }

        /// <summary>
        /// check ios device availability and manage it at database
        /// </summary>
        /// <param name="deviceToken">device token</param>
        /// <param name="timestamp">time stamp</param>
        private static void VerifyIosAvailability(string deviceToken, DateTime timestamp)
        {
            if (null != SubscriptionExpired)
                SubscriptionExpired(deviceToken, (int)DevicePlatform.IOS);
        }


        /// <summary>
        /// get apple push notification configuration
        /// </summary>
        /// <returns>apns configuration</returns>
        private static ApnsConfiguration GetApnsConfiguration(string Client, string Environment)
        {

            log = new Logger();
            ApnsConfiguration config;
            string pushCertificate = string.Empty;
            string path = AppDomain.CurrentDomain.BaseDirectory;

            switch (Client) {
                case "Hirota":
                    {
                        pushCertificate = ConfigurationManager.AppSettings["PushCertificate" + Environment + Client];
                        break;
                    }
                case "Muffato":
                    {
                        pushCertificate = ConfigurationManager.AppSettings["PushCertificate" + Environment + Client];
                        break;
                    }
                case "Wake Me Up":
                    {
                        pushCertificate = ConfigurationManager.AppSettings["PushCertificate" + Environment + Client];
                        break;
                    }
                case "Dengo":
                    {
                        pushCertificate = ConfigurationManager.AppSettings["PushCertificate" + Environment + Client];
                        break;
                    }
                case "Telhanorte":
                    {
                        pushCertificate = ConfigurationManager.AppSettings["PushCertificate" + Environment + Client];
                        break;
                    }
            }
            string p12File = Path.Combine(path, pushCertificate);
            string certPassword = ConfigurationManager.AppSettings["CertificatePassword"];

            if (Environment.Equals("Production"))
                config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, p12File, certPassword);
            else
                config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, p12File, certPassword);


            return (config);
        }

        #region APNS events
        /// <summary>
        /// ios notification succeeded
        /// </summary>
        /// <param name="notification">notificatoin</param>
        private static void IoSNotificationSucceeded(ApnsNotification notification)
        {
            //TODO: log notification succeeded
        }

        /// <summary>
        /// ios push notification failed event
        /// </summary>
        /// <param name="notification">notification</param>
        /// <param name="exception">exception</param>
        private static void IoSNotificationFailed(ApnsNotification notification, AggregateException exception)
        {

            if (null != notification)
            {
                string deviceId = notification.DeviceToken;
                if (null != SubscriptionExpired)
                    SubscriptionExpired(deviceId, (int)DevicePlatform.IOS);
            }
            exception.Handle(ex =>
            {
                if (ex is ApnsNotificationException)
                {
                    var notificationException = (ApnsNotificationException)ex;

                    var apnsNotification = notificationException.Notification;
                    var statusCode = notificationException.ErrorStatusCode;

                    //TODO: log error
                }
                //else
                //TODO: Log unknown error

                return (true);
            });
        }
        #endregion

        /// <summary>
        /// Send email with password
        /// </summary>
        /// <param name="user">new user</param>
        private static void SendMail(string notificationTitle, string msg)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    MailMessage message = new MailMessage();
                    string senderEmail = ConfigurationManager.AppSettings["SystemMail"];
                    message.From = new MailAddress(senderEmail, "[" + ConfigurationManager.AppSettings["loja"] + "] - LOG");

                    //message.To.Add(ConfigurationManager.AppSettings["SystemMail"]);

                    message.Subject = "[" + ConfigurationManager.AppSettings["loja"] + "] Falha no envio da notificação";
                    message.Body = string.Format("NOTIFICAÇÃO: {0}\r\nERRO: {1}", notificationTitle, msg);

                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(message);
                });
            }
            catch
            {
                //LOG exception
            }
        }
    }
}
