using System;
using System.Reflection;
using Log4NetDemo.Context;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;

namespace Log4NetDemo.Configration.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    [Serializable]
    public sealed class SecurityContextProviderAttribute : ConfiguratorAttribute
    {
        public SecurityContextProviderAttribute(Type providerType) : base(100) /* configurator priority 100 to execute before the XmlConfigurator */
        {
            m_providerType = providerType;
        }

        public Type ProviderType
        {
            get { return m_providerType; }
            set { m_providerType = value; }
        }

        override public void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository)
        {
            if (m_providerType == null)
            {
                LogLog.Error(declaringType, "Attribute specified on assembly [" + sourceAssembly.FullName + "] with null ProviderType.");
            }
            else
            {
                LogLog.Debug(declaringType, "Creating provider of type [" + m_providerType.FullName + "]");

                SecurityContextProvider provider = Activator.CreateInstance(m_providerType) as SecurityContextProvider;

                if (provider == null)
                {
                    LogLog.Error(declaringType, "Failed to create SecurityContextProvider instance of type [" + m_providerType.Name + "].");
                }
                else
                {
                    SecurityContextProvider.DefaultProvider = provider;
                }
            }
        }

        private Type m_providerType = null;

        private readonly static Type declaringType = typeof(SecurityContextProviderAttribute);

    }
}
