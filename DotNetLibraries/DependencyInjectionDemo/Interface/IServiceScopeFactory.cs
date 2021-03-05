namespace DependencyInjection.Interface
{
    /// <summary>
    /// ScopeFactory
    /// </summary>
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }
}
