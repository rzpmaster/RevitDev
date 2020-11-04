using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Message.MessageImpl
{
    public class NotificationMessage : MessageBase
    {
        public NotificationMessage(string notification)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, string notification)
            : base(sender)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, object target, string notification)
            : base(sender, target)
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
