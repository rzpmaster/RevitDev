using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.ServiceLookup.CallSite;

namespace DependencyInjection.ServiceLookup.ILEmit
{
    internal sealed class ILEmitCallSiteAnalyzer : CallSiteVisitor<object, ILEmitCallSiteAnalysisResult>
    {
        private const int ConstructorILSize = 6;

        private const int ScopedILSize = 64;

        private const int ConstantILSize = 4;

        private const int ServiceProviderSize = 1;

        private const int FactoryILSize = 16;

        internal static ILEmitCallSiteAnalyzer Instance { get; } = new ILEmitCallSiteAnalyzer();

        protected override ILEmitCallSiteAnalysisResult VisitDisposeCache(ServiceCallSite transientCallSite, object argument) => VisitCallSiteMain(transientCallSite, argument);

        protected override ILEmitCallSiteAnalysisResult VisitConstructor(ConstructorCallSite constructorCallSite, object argument)
        {
            var result = new ILEmitCallSiteAnalysisResult(ConstructorILSize);
            foreach (var callSite in constructorCallSite.ParameterCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override ILEmitCallSiteAnalysisResult VisitRootCache(ServiceCallSite singletonCallSite, object argument) => VisitCallSiteMain(singletonCallSite, argument);

        protected override ILEmitCallSiteAnalysisResult VisitScopeCache(ServiceCallSite scopedCallSite, object argument)
        {
            return new ILEmitCallSiteAnalysisResult(ScopedILSize, hasScope: true).Add(VisitCallSiteMain(scopedCallSite, argument));
        }

        protected override ILEmitCallSiteAnalysisResult VisitConstant(ConstantCallSite constantCallSite, object argument) => new ILEmitCallSiteAnalysisResult(ConstantILSize);

        protected override ILEmitCallSiteAnalysisResult VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object argument) => new ILEmitCallSiteAnalysisResult(ServiceProviderSize);

        protected override ILEmitCallSiteAnalysisResult VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, object argument) => new ILEmitCallSiteAnalysisResult(ConstantILSize);

        protected override ILEmitCallSiteAnalysisResult VisitIEnumerable(IEnumerableCallSite enumerableCallSite, object argument)
        {
            var result = new ILEmitCallSiteAnalysisResult(ConstructorILSize);
            foreach (var callSite in enumerableCallSite.ServiceCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override ILEmitCallSiteAnalysisResult VisitFactory(FactoryCallSite factoryCallSite, object argument) => new ILEmitCallSiteAnalysisResult(FactoryILSize);

        public ILEmitCallSiteAnalysisResult CollectGenerationInfo(ServiceCallSite callSite) => VisitCallSite(callSite, null);
    }
}
