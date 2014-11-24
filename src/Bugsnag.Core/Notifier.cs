﻿using Bugsnag.Core.Payload;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Bugsnag.Core
{
    public class Notifier : INotifier
    {
        public const string Name = ".NET Bugsnag Notifier (ALPHA)";
        public static readonly Uri Url = new Uri("https://bugsnag.com");

        public static readonly string Version =
            Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        private static readonly IWebProxy DetectedProxy = WebRequest.DefaultWebProxy;
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        private IConfiguration Config { get; set; }
        private NotificationFactory Factory { get; set; }

        public Notifier(IConfiguration config)
        {
            Config = config;
            Factory = new NotificationFactory(config);
        }

        public void Send(Event errorEvent)
        {
            var notification = Factory.CreateFromError(errorEvent);
            if (notification != null)
                Send(notification);
        }

        private void Send(Notification notification)
        {
            //  Post JSON to server:
            var request = WebRequest.Create(Config.FinalUrl);

            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";
            request.Proxy = DetectedProxy;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(notification, JsonSettings);
                streamWriter.Write(json);
                streamWriter.Flush();
            }
            request.GetResponse().Close();
        }
    }
}
