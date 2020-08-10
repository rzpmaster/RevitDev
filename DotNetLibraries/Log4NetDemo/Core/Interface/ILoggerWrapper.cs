using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// Logger包装器
    /// </summary>
    public interface ILoggerWrapper
    {
        ILogger Logger { get; }
    }
}
