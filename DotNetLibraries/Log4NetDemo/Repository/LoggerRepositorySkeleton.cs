using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Data.Map;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.ObjectRenderer;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Repository
{
    public abstract class LoggerRepositorySkeleton// : ILoggerRepository, IFlushable
    {
		protected LoggerRepositorySkeleton() : this(new PropertiesDictionary())
		{
		}

		protected LoggerRepositorySkeleton(PropertiesDictionary properties)
		{
			m_properties = properties;
			m_rendererMap = new RendererMap();
			//m_pluginMap = new PluginMap(this);
			m_levelMap = new LevelMap();
			//m_configurationMessages = EmptyCollection.Instance;
			m_configured = false;

			//AddBuiltinLevels();

			// Don't disable any levels by default.
			m_threshold = Level.All;
		}

		private string m_name;
		private RendererMap m_rendererMap;
		private LevelMap m_levelMap;
		private Level m_threshold;
		private bool m_configured;
		private ICollection m_configurationMessages;
		private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;
		private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;
		private event LoggerRepositoryConfigurationChangedEventHandler m_configurationChangedEvent;
		private PropertiesDictionary m_properties;

		private readonly static Type declaringType = typeof(LoggerRepositorySkeleton);
    }
}
