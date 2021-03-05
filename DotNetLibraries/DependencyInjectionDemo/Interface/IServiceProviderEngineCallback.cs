using System;
using DependencyInjection.Interface;
using DependencyInjection.ServiceLookup.CallSite;

namespace DependencyInjection.Interface
{
    internal interface IServiceProviderEngineCallback
    {
        /// <summary>
        /// OnCreate
        /// </summary>
        /// <param name="callSite">服务调用的地点</param>
        void OnCreate(ServiceCallSite callSite);

        /// <summary>
        /// OnResolve
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="scope">服务所在的Scope</param>
        void OnResolve(Type serviceType, IServiceScope scope);
    }
}
