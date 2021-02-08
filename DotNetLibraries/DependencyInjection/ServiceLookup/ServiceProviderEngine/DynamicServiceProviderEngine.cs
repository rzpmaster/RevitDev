using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DependencyInjection.Interface;
using DependencyInjection.ServiceLookup.CallSite;

namespace DependencyInjection.ServiceLookup.ServiceProviderEngine
{
    internal class DynamicServiceProviderEngine : CompiledServiceProviderEngine
    {
        public DynamicServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
        {
            var callCount = 0;
            return scope =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    Task.Run(() => base.RealizeService(callSite));
                }
                return RuntimeResolver.Resolve(callSite, scope);
            };
        }
    }
}
