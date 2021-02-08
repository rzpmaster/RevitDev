namespace DependencyInjection
{
    internal enum CallSiteKind
    {
        Factory,
        Constructor,
        Constant,
        IEnumerable,
        ServiceProvider,
        Scope,
        Transient,
        CreateInstance,
        ServiceScopeFactory,
        Singleton
    }

    internal enum CallSiteResultCacheLocation
    {
        Root,
        Scope,
        Dispose,
        None
    }

    internal enum ServiceProviderMode
    {
        Dynamic,
        Runtime,
        Expressions,
        ILEmit
    }
}
