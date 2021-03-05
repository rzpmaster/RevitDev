using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.ServiceLookup.ILEmit
{
    internal class ILEmitResolverBuilderContext
    {
        public ILGenerator Generator { get; set; }
        public List<object> Constants { get; set; }
        public List<Func<IServiceProvider, object>> Factories { get; set; }
    }
}
