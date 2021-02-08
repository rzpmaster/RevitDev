using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.Extension;
using DependencyInjection.Interface;
using DependencyInjection.Test.Fakes;
using DependencyInjection.Tools;
using Xunit;

namespace DependencyInjection.Test.Specifection
{
    public abstract class ActivatorUtilitiesTests
    {
        public delegate object CreateInstanceFunc(IServiceProvider provider, Type type, object[] args);
        protected abstract IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection);

        private static object CreateInstanceDirectly(IServiceProvider provider, Type type, object[] args)
        {
            return ActivatorUtilities.CreateInstance(provider, type, args);
        }

        private static object CreateInstanceFromFactory(IServiceProvider provider, Type type, object[] args)
        {
            var factory = ActivatorUtilities.CreateFactory(type, args.Select(a => a.GetType()).ToArray());
            return factory(provider, args);
        }

        private static T CreateInstance<T>(CreateInstanceFunc func, IServiceProvider provider, params object[] args)
        {
            return (T)func(provider, typeof(T), args);
        }

        public static IEnumerable<object[]> CreateInstanceFuncs
        {
            get
            {
                yield return new[] { (CreateInstanceFunc)CreateInstanceDirectly };
                yield return new[] { (CreateInstanceFunc)CreateInstanceFromFactory };
            }
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorEnablesYouToCreateAnyTypeWithServicesEvenWhenNotInIocContainer(CreateInstanceFunc createFunc)
        {
            // Arrange
            var serviceCollection = new TestServiceCollection()
                .AddTransient<IFakeService, FakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            var anotherClass = CreateInstance<AnotherClass>(createFunc, serviceProvider);

            Assert.NotNull(anotherClass.FakeService);
        }
    }

    class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}
