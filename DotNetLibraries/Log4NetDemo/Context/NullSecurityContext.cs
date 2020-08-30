using System;

namespace Log4NetDemo.Context
{
    public sealed class NullSecurityContext : SecurityContext
    {
        public static readonly NullSecurityContext Instance = new NullSecurityContext();

        private NullSecurityContext()
        {
        }

        public override IDisposable Impersonate(object state)
        {
            return null;
        }
    }
}
