using System;

namespace DependencyInjection.Interface
{
    internal interface IServiceProviderEngine : IServiceProvider, IDisposable
    {
        IServiceScope RootScope { get; }
    }
}
