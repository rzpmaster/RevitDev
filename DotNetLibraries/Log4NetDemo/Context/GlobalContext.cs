using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Context
{
    public sealed class GlobalContext
    {
        #region Private Instance Constructors

        /// <summary>
        /// Private Constructor. 
        /// </summary>
        /// <remarks>
        /// Uses a private access modifier to prevent instantiation of this class.
        /// </remarks>
        private GlobalContext()
        {
        }

        #endregion Private Instance Constructors

        static GlobalContext()
        {
            Properties[LoggingEvent.HostNameProperty] = SystemInfo.HostName;
        }

        #region Public Static Properties

        /// <summary>
        /// The global properties map.
        /// </summary>
        /// <value>
        /// The global properties map.
        /// </value>
        /// <remarks>
        /// <para>
        /// The global properties map.
        /// </para>
        /// </remarks>
        public static GlobalContextProperties Properties
        {
            get { return s_properties; }
        }

        #endregion Public Static Properties

        #region Private Static Fields

        /// <summary>
        /// The global context properties instance
        /// </summary>
        private readonly static GlobalContextProperties s_properties = new GlobalContextProperties();

        #endregion Private Static Fields
    }
}
