using Log4NetDemo.Appender;
using Log4NetDemo.Core;
using Log4NetDemo.Layout;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Reflection;

namespace Log4NetDemo.Configration
{
    public sealed class BasicConfigurator
    {
        private BasicConfigurator()
        {
        }

        static public ICollection Configure()
        {
            return BasicConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
        }

        static public ICollection Configure(params IAppender[] appenders)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, appenders);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(IAppender appender)
        {
            return Configure(new IAppender[] { appender });
        }

        static public ICollection Configure(ILoggerRepository repository)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                // Create the layout
                PatternLayout layout = new PatternLayout();
                layout.ConversionPattern = PatternLayout.DetailConversionPattern;
                layout.ActivateOptions();

                // Create the appender
                ConsoleAppender appender = new ConsoleAppender();
                appender.Layout = layout;
                appender.ActivateOptions();

                InternalConfigure(repository, appender);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(ILoggerRepository repository, IAppender appender)
        {
            return Configure(repository, new IAppender[] { appender });
        }

        static public ICollection Configure(ILoggerRepository repository, params IAppender[] appenders)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, appenders);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static private void InternalConfigure(ILoggerRepository repository, params IAppender[] appenders)
        {
            IBasicRepositoryConfigurator configurableRepository = repository as IBasicRepositoryConfigurator;
            if (configurableRepository != null)
            {
                configurableRepository.Configure(appenders);
            }
            else
            {
                LogLog.Warn(declaringType, "BasicConfigurator: Repository [" + repository + "] does not support the BasicConfigurator");
            }
        }

        private readonly static Type declaringType = typeof(BasicConfigurator);
    }
}
