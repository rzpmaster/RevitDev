using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.ServiceLookup.ILEmit
{
    internal readonly struct ILEmitCallSiteAnalysisResult
    {
        public ILEmitCallSiteAnalysisResult(int size) : this()
        {
            Size = size;
        }

        public ILEmitCallSiteAnalysisResult(int size, bool hasScope)
        {
            Size = size;
            HasScope = hasScope;
        }

        public readonly int Size;

        public readonly bool HasScope;

        public ILEmitCallSiteAnalysisResult Add(in ILEmitCallSiteAnalysisResult other) =>
            new ILEmitCallSiteAnalysisResult(Size + other.Size, HasScope | other.HasScope);
    }
}
