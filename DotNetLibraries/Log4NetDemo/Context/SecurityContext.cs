using System;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 与受保护的资源(例如:操作系统服务)交互时,使用 SecurityContext
    /// </summary>
    public abstract class SecurityContext
    {
        /// <summary>
        /// 模拟这个 SecurityContext
        /// </summary>
        /// <param name="state">调用者提供的状态</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>模拟一个线程安全的上下文,释放后还原为之前的状态</para>
        /// </remarks>
        public abstract IDisposable Impersonate(object state);
    }
}
