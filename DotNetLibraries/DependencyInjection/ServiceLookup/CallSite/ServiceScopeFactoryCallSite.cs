using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.Interface;

namespace DependencyInjection.ServiceLookup.CallSite
{
    internal class ServiceScopeFactoryCallSite : ServiceCallSite
    {
        public ServiceScopeFactoryCallSite() : base(ResultCache.None)
        {
        }

        public override Type ServiceType { get; } = typeof(IServiceScopeFactory);
        public override Type ImplementationType { get; } = typeof(ServiceProviderEngine.ServiceProviderEngine);
        public override CallSiteKind Kind { get; } = CallSiteKind.ServiceScopeFactory;
    }
}
