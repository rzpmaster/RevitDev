using System;

namespace SimpleIocDemo.ServiceLocator
{
    public static class ServiceLocator
    {
        private static Func<IServiceLocator> _currentProvider;

        public static bool IsLocationProviderSet => _currentProvider != null;

        public static IServiceLocator Current
        {
            get
            {
                if (!IsLocationProviderSet) throw new InvalidOperationException(" ServiceLocationProvider must be set.");

                return _currentProvider();
            }
        }

        public static void SetLocatorProvider(Func<IServiceLocator> newProvider)
        {
            _currentProvider = newProvider;
        }
    }
}
