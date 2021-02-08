using System;
using DependencyInjection.Interface;
using DependencyInjection.ServiceLookup.CallSite;

namespace DependencyInjection.Interface
{
    internal interface IServiceProviderEngineCallback
    {
        void OnCreate(ServiceCallSite callSite);
        void OnResolve(Type serviceType, IServiceScope scope);
    }
}
