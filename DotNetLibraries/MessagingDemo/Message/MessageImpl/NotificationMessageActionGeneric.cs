using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Message.MessageImpl
{
    public class NotificationMessageAction<TCallbackParameter> : NotificationMessageWithCallback
    {
        public NotificationMessageAction(string notification, Action<TCallbackParameter> callback)
                : base(notification, callback)
        {
        }

        public NotificationMessageAction(object sender, string notification, Action<TCallbackParameter> callback)
           : base(sender, notification, callback)
        {
        }

        public NotificationMessageAction(object sender, object target, string notification, Action<TCallbackParameter> callback)
            : base(sender, target, notification, callback)
        {
        }

        public void Execute(TCallbackParameter parameter)
        {
            base.Execute(parameter);
        }
    }
}
