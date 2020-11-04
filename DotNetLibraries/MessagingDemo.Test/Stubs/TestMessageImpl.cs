using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo.Test.Stubs
{
    public class TestMessageImpl : TestMessageBase
    {
        public TestMessageImpl(object sender)
            : base(sender)
        {
        }

        public bool Result
        {
            get;
            set;
        }
    }
}
