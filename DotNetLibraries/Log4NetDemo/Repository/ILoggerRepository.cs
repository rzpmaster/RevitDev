using Log4NetDemo.Appender;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Data.Map;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.ObjectRenderer;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
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
	/// This interface is implemented by logger repositories. e.g. <see cref="Hierarchy.Hierarchy"/>.
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

        /// <summary>
        /// 查找已经存在的 ILogger，没有找到返回空
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILogger Exists(string name);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILogger[] GetCurrentLoggers();
        /// <summary>
        /// 根据名称返回 ILogger，没有回创建一个新的
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILogger GetLogger(string name);

        /// <summary>
        /// 关闭 ILoggerRepository
        /// </summary>
        /// <remarks>
        /// <para>释放非托管资源</para>
        /// </remarks>
        void Shutdown();

        /// <summary>
        /// Reset the repositories configuration to a default state
        /// </summary>
        /// <remarks>
        /// <para>
        /// 将此 ILoggerRepository 中的所有实例初始化为默认状态。
        /// </para>
        /// <para>
        /// 谨慎使用，此方法会阻止所有日志记录，知道重置操作完成
        /// </para>
        /// </remarks>
        void ResetConfiguration();

        /// <summary>
        /// 通过这个 ILoggerRepository 记录日志
        /// </summary>
        /// <param name="logEvent"></param>
        /// <remarks>
        /// <para>此方法不应该作为常用方法记录日志，应该使用<see cref="ILog"/>接口的方法，可以使用<see cref="LogManager.GetLogger(string)"/>方法得到 ILog 对象</para>
        /// </remarks>
        void Log(LoggingEvent logEvent);

        /// <summary>
        /// 指示是否配置完成
        /// </summary>
        bool Configured { get; set; }
        /// <summary>
        /// 配置期间的日志消息列表
        /// </summary>
        ICollection ConfigurationMessages { get; set; }

        event LoggerRepositoryShutdownEventHandler ShutdownEvent;
        event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset;
        event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged;

        PropertiesDictionary Properties { get; }

        IAppender[] GetAppenders();
    }
}
