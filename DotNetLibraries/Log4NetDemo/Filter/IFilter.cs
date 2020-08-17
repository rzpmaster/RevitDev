using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;

namespace Log4NetDemo.Filter
{
	/// <summary>
	/// 日志过滤器接口
	/// </summary>
	/// <remarks>
	/// <para>
	/// Users should implement this interface to implement customized logging
	/// event filtering. Note that <see cref="Log4NetDemo.Repository.Hierarchy.Logger"/> and 
	/// <see cref="Log4NetDemo.Appender.AppenderSkeleton"/>, the parent class of all standard
	/// appenders, have built-in filtering rules. It is suggested that you
	/// first use and understand the built-in rules before rushing to write
	/// your own custom filters.
	/// </para>
	/// <para>
	/// This abstract class assumes and also imposes that filters be
	/// organized in a linear chain. The <see cref="Decide"/>
	/// method of each filter is called sequentially, in the order of their 
	/// addition to the chain.
	/// </para>
	/// <para>
	/// The <see cref="Decide"/> method must return one
	/// of the integer constants <see cref="FilterDecision.Deny"/>, 
	/// <see cref="FilterDecision.Neutral"/> or <see cref="FilterDecision.Accept"/>.
	/// </para>
	/// <para>
	/// If the value <see cref="FilterDecision.Deny"/> is returned, then the log event is dropped 
	/// immediately without consulting with the remaining filters.
	/// </para>
	/// <para>
	/// If the value <see cref="FilterDecision.Neutral"/> is returned, then the next filter
	/// in the chain is consulted. If there are no more filters in the
	/// chain, then the log event is logged. Thus, in the presence of no
	/// filters, the default behavior is to log all logging events.
	/// </para>
	/// <para>
	/// If the value <see cref="FilterDecision.Accept"/> is returned, then the log
	/// event is logged without consulting the remaining filters.
	/// </para>
	/// <para>
	/// The philosophy of log4net filters is largely inspired from the
	/// Linux ipchains.
	/// </para>
	/// </remarks>
	public interface IFilter : IOptionHandler
    {
		/// <summary>
		/// 返回当前过滤器对给定日志事件的策略
		/// 是记录？还是不记录？还是询问过滤器链表中的下一个？
		/// </summary>
		/// <param name="loggingEvent"></param>
		/// <returns></returns>
        FilterDecision Decide(LoggingEvent loggingEvent);

        IFilter Next { get; set; }
    }
}
