using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.ServiceLookup.Expressions
{
    internal class CallSiteExpressionBuilderContext
    {
        public ParameterExpression ScopeParameter { get; set; }
        public bool RequiresResolvedServices { get; set; }
    }
}
