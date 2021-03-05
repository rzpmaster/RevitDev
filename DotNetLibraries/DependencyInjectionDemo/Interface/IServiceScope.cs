using System;

namespace DependencyInjection.Interface
{
    /// <summary>
    /// 每个 Scope 里面都有一个 IServiceProvider
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }
}
