using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Test.Fakes
{
    public class FakeService : IFakeEveryService, IDisposable
    {
        public PocoClass Value { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(FakeService));
            }

            Disposed = true;
        }
    }

    public class FakeTwoMultipleService : IFakeMultipleService
    {
    }

    public class FakeOuterService : IFakeOuterService
    {
        public FakeOuterService(
            IFakeService singleService,
            IEnumerable<IFakeMultipleService> multipleServices)
        {
            SingleService = singleService;
            MultipleServices = multipleServices;
        }

        public IFakeService SingleService { get; }

        public IEnumerable<IFakeMultipleService> MultipleServices { get; }
    }

    public class FakeOpenGenericService<TVal> : IFakeOpenGenericService<TVal>
    {
        public FakeOpenGenericService(TVal value)
        {
            Value = value;
        }

        public TVal Value { get; }
    }
}
