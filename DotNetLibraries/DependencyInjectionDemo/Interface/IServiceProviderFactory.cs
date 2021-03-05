using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Interface
{
    public interface IServiceProviderFactory<TContainerBuilder>
    {
        TContainerBuilder CreateBuilder(IServiceCollection services);

        IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder);
    }
}
