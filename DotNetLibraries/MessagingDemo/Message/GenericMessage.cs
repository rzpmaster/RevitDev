using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Message
{
    public class GenericMessage<T> : MessageBase
    {
        public GenericMessage(T content)
        {
            this.Content = content;
        }

        public GenericMessage(object sender, T content)
            : base(sender)
        {
            Content = content;
        }

        public GenericMessage(object sender, object target, T content)
            : base(sender, target)
        {
            Content = content;
        }

        public T Content { get; protected set; }
    }
}
