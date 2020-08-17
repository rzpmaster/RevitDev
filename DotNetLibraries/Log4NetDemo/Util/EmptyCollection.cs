using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Util
{
    [Serializable]
    public sealed class EmptyCollection //: ICollection
    {



        private readonly static EmptyCollection s_instance = new EmptyCollection();
    }
}
