using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagingDemo.Message;

namespace MessagingDemo.Test.Stubs
{
    public class TestMessageBase : MessageBase, ITestMessage
    {
        public TestMessageBase(object sender)
            : base(sender)
        {
        }

        public string Content
        {
            get;
            set;
        }
    }
}
