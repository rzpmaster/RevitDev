using Log4NetDemo.Appender;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Data.Map;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.ObjectRenderer;
using Log4NetDemo.Util;
using System;
using System.Collections;

namespace Log4NetDemo.Repository
{
    /// <summary>
    /// LoggerRepository 关闭事件委托
    /// </summary>
    /// <param name="sender">将要关闭的 LoggerRepository</param>
    /// <param name="e">空的事件参数</param>
    public delegate void LoggerRepositoryShutdownEventHandler(object sender, EventArgs e);

    /// <summary>
    /// LoggerRepository 配置重新设置事件委托
    /// </summary>
    /// <param name="sender">配置将要重新设置的 LoggerRepository</param>
    /// <param name="e">空的事件参数</param>
    public delegate void LoggerRepositoryConfigurationResetEventHandler(object sender, EventArgs e);

    /// <summary>
    /// LoggerRepository 配置改变事件委托
    /// </summary>
    /// <param name="sender">配置将要改变的 LoggerRepository</param>
    /// <param name="e"></param>
    public delegate void LoggerRepositoryConfigurationChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// LoggerRepository 接口
    /// </summary>
    /// <remarks>
	/// <para>
	/// This interface is implemented by logger repositories. e.g. 
	/// <see cref="Hierarchy.Hierarchy"/>.
	/// </para>
	/// <para>
	/// This interface is used by the <see cref="LogManager"/>to obtain <see cref="ILog"/> interfaces.
	/// </para>
	/// </remarks>
    public interface ILoggerRepository
    {
        string Name { get; set; }

        RendererMap RendererMap { get; }
        LevelMap LevelMap { get; }
        Level Threshold { get; set; }

        ILogger Exists(string name);
        ILogger[] GetCurrentLoggers();
        ILogger GetLogger(string name);

        void Shutdown();
        void ResetConfiguration();

        void Log(LoggingEvent logEvent);

        bool Configured { get; set; }
        ICollection ConfigurationMessages { get; set; }

        event LoggerRepositoryShutdownEventHandler ShutdownEvent;
        event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset;
        event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged;

        PropertiesDictionary Properties { get; }

        IAppender[] GetAppenders();
    }
}
