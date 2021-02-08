using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.Interface;
using DependencyInjection.ServiceLookup.CallSite;
using DependencyInjection.ServiceLookup.Expressions;

namespace DependencyInjection.ServiceLookup.ServiceProviderEngine
{
    internal abstract class CompiledServiceProviderEngine : ServiceProviderEngine
    {
        public ExpressionResolverBuilder ExpressionResolverBuilder { get; }

        public CompiledServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
            ExpressionResolverBuilder = new ExpressionResolverBuilder(RuntimeResolver, this, Root);
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
        {
            var realizedService = ExpressionResolverBuilder.Build(callSite);
            RealizedServices[callSite.ServiceType] = realizedService;
            return realizedService;
        }
    }
}
