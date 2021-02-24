using DependencyInjection.Interface;

namespace Microsoft.Extensions.DependencyInjection.Fakes
{
    public struct StructService
    {
        public StructService(IServiceScopeFactory scopeFactory)
        {
        }
    }
}