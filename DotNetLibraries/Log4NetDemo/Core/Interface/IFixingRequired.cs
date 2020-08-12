using Log4NetDemo.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// 提供一个在任何线程都能访问到相同实例的方法
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interface that indicates that the object requires fixing before it
    /// can be taken outside the context of the appender's 
    /// <see cref="Appender.IAppender.DoAppend"/> method.
    /// </para>
    /// <para>
    /// When objects that implement this interface are stored 
    /// in the context properties maps <see cref="Context.GlobalContext"/>
    /// <see cref="Context.GlobalContext.Properties"/> and <see cref="Context.ThreadContext"/>
    /// <see cref="Context.ThreadContext.Properties"/> are fixed 
    /// (see <see cref="LoggingEvent.Fix"/>) the <see cref="GetFixedObject"/>
    /// method will be called.
    /// </para>
    /// </remarks>
    public interface IFixingRequired
    {
        /// <summary>
        /// 获得此对象的可移植版本，使其在任何线程都具有相同的结果
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// 获得一个当前对象的可移植版本实例，使其在任何线程都具有相同的结果
        /// </para>
        /// <para>
		/// Get a portable instance object that represents the current
		/// state of this object. The portable object can be stored
		/// and logged from any thread with identical results.
		/// </para>
        /// </remarks>
        object GetFixedObject();
    }
}
