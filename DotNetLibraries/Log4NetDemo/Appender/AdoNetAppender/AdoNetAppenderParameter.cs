using Log4NetDemo.Core.Data;
using Log4NetDemo.Layout.RawLayout;
using System;
using System.Data;

namespace Log4NetDemo.Appender
{
    public class AdoNetAppenderParameter
    {
        public AdoNetAppenderParameter()
        {
            Precision = 0;
            Scale = 0;
            Size = 0;
        }

        public string ParameterName
        {
            get { return m_parameterName; }
            set { m_parameterName = value; }
        }

        public DbType DbType
        {
            get { return m_dbType; }
            set
            {
                m_dbType = value;
                m_inferType = false;
            }
        }

        public byte Precision
        {
            get { return m_precision; }
            set { m_precision = value; }
        }

        public byte Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        public int Size
        {
            get { return m_size; }
            set { m_size = value; }
        }

        public IRawLayout Layout
        {
            get { return m_layout; }
            set { m_layout = value; }
        }

        #region Public Instance Methods

        virtual public void Prepare(IDbCommand command)
        {
            // Create a new parameter
            IDbDataParameter param = command.CreateParameter();

            // Set the parameter properties
            param.ParameterName = ParameterName;

            if (!m_inferType)
            {
                param.DbType = DbType;
            }
            if (Precision != 0)
            {
                param.Precision = Precision;
            }
            if (Scale != 0)
            {
                param.Scale = Scale;
            }
            if (Size != 0)
            {
                param.Size = Size;
            }

            // Add the parameter to the collection of params
            command.Parameters.Add(param);
        }

        virtual public void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
        {
            // Lookup the parameter
            IDbDataParameter param = (IDbDataParameter)command.Parameters[ParameterName];

            // Format the value
            object formattedValue = Layout.Format(loggingEvent);

            // If the value is null then convert to a DBNull
            if (formattedValue == null)
            {
                formattedValue = DBNull.Value;
            }

            param.Value = formattedValue;
        }

        #endregion

        /// <summary>
		/// The name of this parameter.
		/// </summary>
		private string m_parameterName;

        /// <summary>
        /// The database type for this parameter.
        /// </summary>
        private DbType m_dbType;

        /// <summary>
        /// Flag to infer type rather than use the DbType
        /// </summary>
        private bool m_inferType = true;

        /// <summary>
        /// The precision for this parameter.
        /// </summary>
        private byte m_precision;

        /// <summary>
        /// The scale for this parameter.
        /// </summary>
        private byte m_scale;

        /// <summary>
        /// The size for this parameter.
        /// </summary>
        private int m_size;

        /// <summary>
        /// The <see cref="IRawLayout"/> to use to render the
        /// logging event into an object for this parameter.
        /// </summary>
        private IRawLayout m_layout;
    }
}
