using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Context
{
	public sealed class LogicalThreadContext
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private Constructor. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </para>
		/// </remarks>
		private LogicalThreadContext()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Properties

		/// <summary>
		/// The thread properties map
		/// </summary>
		/// <value>
		/// The thread properties map
		/// </value>
		/// <remarks>
		/// <para>
		/// The <c>LogicalThreadContext</c> properties override any <see cref="ThreadContext"/> 
		/// or <see cref="GlobalContext"/> properties with the same name.
		/// </para>
		/// </remarks>
		public static LogicalThreadContextProperties Properties
		{
			get { return s_properties; }
		}

		/// <summary>
		/// The thread stacks
		/// </summary>
		/// <value>
		/// stack map
		/// </value>
		/// <remarks>
		/// <para>
		/// The logical thread stacks.
		/// </para>
		/// </remarks>
		public static LogicalThreadContextStacks Stacks
		{
			get { return s_stacks; }
		}

		#endregion Public Static Properties

		#region Private Static Fields

		/// <summary>
		/// The thread context properties instance
		/// </summary>
		private readonly static LogicalThreadContextProperties s_properties = new LogicalThreadContextProperties();

		/// <summary>
		/// The thread context stacks instance
		/// </summary>
		private readonly static LogicalThreadContextStacks s_stacks = new LogicalThreadContextStacks(s_properties);

		#endregion Private Static Fields
	}
}
