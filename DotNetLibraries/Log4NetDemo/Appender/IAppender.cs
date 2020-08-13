using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// 实现这个接口，以特定的方式记录日志
    /// </summary>
    /// <remarks>
	/// <para>
	/// Implementors should consider extending the <see cref="AppenderSkeleton"/>
	/// class which provides a default implementation of this interface.
	/// </para>
	/// <para>
	/// Appenders can also implement the <see cref="Core.Interface.IOptionHandler"/> interface. Therefore
	/// they would require that the <see cref="Core.Interface.IOptionHandler.ActivateOptions()"/> method
	/// be called after the appenders properties have been configured.
	/// </para>
	/// </remarks>
    public interface IAppender
    {
        string Name { get; set; }

        /// <summary>
        /// 关闭 Appender 并释放资源
        /// </summary>
        void Close();

        /// <summary>
        /// 记录日志，当消息进入 Appender 时被调用
        /// </summary>
        /// <param name="loggingEvent"></param>
        void DoAppend(LoggingEvent loggingEvent);
    }
}
