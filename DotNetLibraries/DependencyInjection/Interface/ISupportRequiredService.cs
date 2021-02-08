using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Interface
{
    public interface ISupportRequiredService
    {
        object GetRequiredService(Type serviceType);
    }
}
