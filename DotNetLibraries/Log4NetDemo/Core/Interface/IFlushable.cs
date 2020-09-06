namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// Interface that can be implemented by Appenders that buffer logging data and expose a <see cref="Flush"/> method.
    /// </summary>
    public interface IFlushable
    {
        /// <summary>
        /// Flushes any buffered log data.
        /// </summary>
        /// <param name="millisecondsTimeout">等待 Flush 的最长时间</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Appenders that implement the <see cref="Flush"/> method must do so in a thread-safe manner: it can be called concurrently with
        /// the <see cref="Appender.IAppender.DoAppend(Data.LoggingEvent)"/> method.
        /// </para>
        /// <para>
        /// The <paramref name="millisecondsTimeout"/> parameter is only relevant for appenders that process logging events asynchronously,（仅用于异步处理） such as <see cref="RemotingAppender"/>.
        /// </para>
        /// </remarks>
        bool Flush(int millisecondsTimeout);
    }
}
