﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Message.MessageImpl
{
    public class NotificationMessage<T> : GenericMessage<T>
    {
        public NotificationMessage(T content, string notification)
           : base(content)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, T content, string notification)
            : base(sender, content)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, object target, T content, string notification)
            : base(sender, target, content)
        {
            Notification = notification;
        }

        public string Notification
        {
            get;
            private set;
        }
    }
}
