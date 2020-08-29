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
    /// LoggerRepository 配置改变事件参数
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        private readonly ICollection configurationMessages;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationMessages"></param>
        public ConfigurationChangedEventArgs(ICollection configurationMessages)
        {
            this.configurationMessages = configurationMessages;
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection ConfigurationMessages
        {
            get { return configurationMessages; }
        }
    }
}
