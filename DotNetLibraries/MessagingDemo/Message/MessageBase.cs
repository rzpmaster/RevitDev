﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Message
{
    public class MessageBase
    {
        public MessageBase()
        {
        }

        public MessageBase(object sender)
        {
            this.Sender = sender;
        }

        public MessageBase(object sender, object target)
            : this(sender)
        {
            Target = target;
        }

        public object Sender { get; protected set; }

        public object Target { get; protected set; }
    }
}
