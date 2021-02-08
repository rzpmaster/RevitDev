using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Test.Fakes
{
    public class FakeDisposeCallback
    {
        public List<object> Disposed { get; } = new List<object>();
    }

    public class FakeDisposableCallbackService : IDisposable
    {
        private static int _globalId;
        private readonly int _id;
        private readonly FakeDisposeCallback _callback;

        public FakeDisposableCallbackService(FakeDisposeCallback callback)
        {
            _id = _globalId++;
            _callback = callback;
        }

        public void Dispose()
        {
            _callback.Disposed.Add(this);
        }

        public override string ToString()
        {
            return _id.ToString();
        }
    }

    public class FakeDisposableCallbackOuterService : FakeDisposableCallbackService, IFakeOuterService
    {
        public FakeDisposableCallbackOuterService(
            IFakeService singleService,
            IEnumerable<IFakeMultipleService> multipleServices,
            FakeDisposeCallback callback) : base(callback)
        {
            SingleService = singleService;
            MultipleServices = multipleServices;
        }

        public IFakeService SingleService { get; }
        public IEnumerable<IFakeMultipleService> MultipleServices { get; }
    }

    public class FakeDisposableCallbackInnerService : FakeDisposableCallbackService, IFakeMultipleService
    {
        public FakeDisposableCallbackInnerService(FakeDisposeCallback callback) : base(callback)
        {
        }
    }
}
