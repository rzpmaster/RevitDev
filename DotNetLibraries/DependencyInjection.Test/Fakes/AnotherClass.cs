using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Test.Fakes
{
    public class AnotherClass
    {
        public AnotherClass(IFakeService fakeService)
        {
            FakeService = fakeService;
        }

        public IFakeService FakeService { get; }
    }

    public class AnotherClassAcceptingData
    {
        public AnotherClassAcceptingData(IFakeService fakeService, string one, string two)
        {
            FakeService = fakeService;
            One = one;
            Two = two;
        }

        public IFakeService FakeService { get; }

        public string One { get; }

        public string Two { get; }
    }
}
