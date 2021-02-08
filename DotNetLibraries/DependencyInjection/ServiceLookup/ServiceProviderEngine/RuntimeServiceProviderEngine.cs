using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.Interface;
using DependencyInjection.ServiceLookup.CallSite;

namespace DependencyInjection.ServiceLookup.ServiceProviderEngine
{
    internal class RuntimeServiceProviderEngine : ServiceProviderEngine
    {
        public RuntimeServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
        {
            return scope =>
            {
                Func<ServiceProviderEngineScope, object> realizedService = p => RuntimeResolver.Resolve(callSite, p);

                RealizedServices[callSite.ServiceType] = realizedService;
                return realizedService(scope);
            };
        }
    }
}
