using System;

namespace DependencyInjection.Interface
{
    /// <summary>
    /// 服务引擎
    /// </summary>
    internal interface IServiceProviderEngine : IServiceProvider, IDisposable
    {
        /// <summary>
        /// 根 Scope
        /// </summary>
        IServiceScope RootScope { get; }
    }
}
