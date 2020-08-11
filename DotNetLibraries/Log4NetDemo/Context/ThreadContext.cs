namespace Log4NetDemo.Context
{
    public sealed class ThreadContext
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
		private ThreadContext()
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
		/// The <c>ThreadContext</c> properties override any <see cref="GlobalContext"/>
		/// properties with the same name.
		/// </para>
		/// </remarks>
		public static ThreadContextProperties Properties
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
		/// The thread local stacks.
		/// </para>
		/// </remarks>
		public static ThreadContextStacks Stacks
		{
			get { return s_stacks; }
		}

		#endregion Public Static Properties

		#region Private Static Fields

		/// <summary>
		/// The thread context properties instance
		/// </summary>
		private readonly static ThreadContextProperties s_properties = new ThreadContextProperties();

		/// <summary>
		/// The thread context stacks instance
		/// </summary>
		private readonly static ThreadContextStacks s_stacks = new ThreadContextStacks(s_properties);

		#endregion Private Static Fields
	}
}
