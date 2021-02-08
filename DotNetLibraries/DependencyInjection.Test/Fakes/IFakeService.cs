using System.Collections.Generic;

namespace DependencyInjection.Test.Fakes
{
    public interface IFakeService { }
    public interface IFakeMultipleService : IFakeService { }
    public interface IFakeScopedService : IFakeService { }
    public interface IFakeServiceInstance : IFakeService { }
    public interface IFakeSingletonService : IFakeService { }
    public interface IFakeOpenGenericService<TValue>
    {
        TValue Value { get; }
    }
    public interface IFakeEveryService :
        IFakeService,
        IFakeMultipleService,
        IFakeScopedService,
        IFakeServiceInstance,
        IFakeSingletonService,
        IFakeOpenGenericService<PocoClass>
    {
    }
    public class PocoClass { }

    public interface IFakeOuterService
    {
        IFakeService SingleService { get; }

        IEnumerable<IFakeMultipleService> MultipleServices { get; }
    }

    interface IFactoryService
    {
        IFakeService FakeService { get; }

        int Value { get; }
    }
}
