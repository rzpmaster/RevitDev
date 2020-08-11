using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Context
{
    public abstract class ContextPropertiesBase
    {
        public abstract object this[string key] { get; set; }
    }
}
