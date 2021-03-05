using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.ServiceLookup.CallSite
{
    internal class ConstantCallSite : ServiceCallSite
    {
        internal object DefaultValue { get; }

        public ConstantCallSite(Type serviceType, object defaultValue) : base(ResultCache.None)
        {
            DefaultValue = defaultValue;
        }

        public override Type ServiceType => DefaultValue.GetType();
        public override Type ImplementationType => DefaultValue.GetType();
        public override CallSiteKind Kind { get; } = CallSiteKind.Constant;
    }
}
