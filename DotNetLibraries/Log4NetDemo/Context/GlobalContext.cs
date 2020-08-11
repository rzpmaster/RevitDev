using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;

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

        public static GlobalContextProperties Properties
        {
            get { return s_properties; }
        }

        #endregion Public Static Properties

        #region Private Static Fields

        private readonly static GlobalContextProperties s_properties = new GlobalContextProperties();

        #endregion Private Static Fields
    }
}
