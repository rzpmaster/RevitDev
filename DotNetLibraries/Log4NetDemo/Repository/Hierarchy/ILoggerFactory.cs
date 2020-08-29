namespace Log4NetDemo.Repository.Hierarchy
{
    /// <summary>
    /// Logger 工厂
    /// </summary>
    /// <remarks>
    /// <para>
	/// This interface is used by the <see cref="Hierarchy"/> to 
	/// create new <see cref="Logger"/> objects.
	/// </para>
    /// </remarks>
    public interface ILoggerFactory
    {
        /// <summary>
        /// 创建一个 Logger 实例
        /// </summary>
        /// <param name="repository">The <see cref="ILoggerRepository" /> that will own the <see cref="Logger" />.</param>
        /// <param name="name">The name of the <see cref="Logger" />.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
		/// If the <paramref name="name"/> is <c>null</c> then the root logger
		/// must be returned.
		/// </para>
        /// </remarks>
        Logger CreateLogger(ILoggerRepository repository, string name);
    }
}
