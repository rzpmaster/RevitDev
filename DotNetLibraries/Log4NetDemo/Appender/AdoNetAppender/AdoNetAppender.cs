using Log4NetDemo.Context;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;

namespace Log4NetDemo.Appender
{
    public class AdoNetAppender : BufferingAppenderSkeleton
    {
        public AdoNetAppender()
        {
            ConnectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            UseTransactions = true;
            CommandType = System.Data.CommandType.Text;
            m_parameters = new ArrayList();
            ReconnectOnError = false;
        }

        #region Public Instance Properties

        public string ConnectionString
        {
            get { return m_connectionString; }
            set { m_connectionString = value; }
        }

        public string AppSettingsKey
        {
            get { return m_appSettingsKey; }
            set { m_appSettingsKey = value; }
        }

        public string ConnectionStringName
        {
            get { return m_connectionStringName; }
            set { m_connectionStringName = value; }
        }

        public string ConnectionType
        {
            get { return m_connectionType; }
            set { m_connectionType = value; }
        }

        public string CommandText
        {
            get { return m_commandText; }
            set { m_commandText = value; }
        }

        public CommandType CommandType
        {
            get { return m_commandType; }
            set { m_commandType = value; }
        }

        public bool UseTransactions
        {
            get { return m_useTransactions; }
            set { m_useTransactions = value; }
        }

        public SecurityContext SecurityContext
        {
            get { return m_securityContext; }
            set { m_securityContext = value; }
        }

        public bool ReconnectOnError
        {
            get { return m_reconnectOnError; }
            set { m_reconnectOnError = value; }
        }

        protected IDbConnection Connection
        {
            get { return m_dbConnection; }
            set { m_dbConnection = value; }
        }

        #endregion

        #region Implementation of IOptionHandler

        override public void ActivateOptions()
        {
            base.ActivateOptions();

            if (SecurityContext == null)
            {
                SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }

            InitializeDatabaseConnection();
        }

        #endregion

        #region Override implementation of AppenderSkeleton

        override protected void OnClose()
        {
            base.OnClose();
            DiposeConnection();
        }

        #endregion

        #region Override implementation of BufferingAppenderSkeleton

        override protected void SendBuffer(LoggingEvent[] events)
        {
            if (ReconnectOnError && (Connection == null || Connection.State != ConnectionState.Open))
            {
                LogLog.Debug(declaringType, "Attempting to reconnect to database. Current Connection State: " + ((Connection == null) ? SystemInfo.NullText : Connection.State.ToString()));

                InitializeDatabaseConnection();
            }

            // Check that the connection exists and is open
            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                if (UseTransactions)
                {
                    // Create transaction
                    // NJC - Do this on 2 lines because it can confuse the debugger
                    using (IDbTransaction dbTran = Connection.BeginTransaction())
                    {
                        try
                        {
                            SendBuffer(dbTran, events);

                            // commit transaction
                            dbTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            // rollback the transaction
                            try
                            {
                                dbTran.Rollback();
                            }
                            catch (Exception)
                            {
                                // Ignore exception
                            }

                            // Can't insert into the database. That's a bad thing
                            ErrorHandler.Error("Exception while writing to database", ex);
                        }
                    }
                }
                else
                {
                    // Send without transaction
                    SendBuffer(null, events);
                }
            }
        }

        #endregion

        #region Public Instance Methods

        protected ArrayList m_parameters;

        public void AddParameter(AdoNetAppenderParameter parameter)
        {
            m_parameters.Add(parameter);
        }

        #endregion

        #region Protected Instance Methods

        virtual protected void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
        {
            // string.IsNotNullOrWhiteSpace() does not exist in ancient .NET frameworks
            if (CommandText != null && CommandText.Trim() != "")
            {
                using (IDbCommand dbCmd = Connection.CreateCommand())
                {
                    // Set the command string
                    dbCmd.CommandText = CommandText;

                    // Set the command type
                    dbCmd.CommandType = CommandType;
                    // Send buffer using the prepared command object
                    if (dbTran != null)
                    {
                        dbCmd.Transaction = dbTran;
                    }
                    // prepare the command, which is significantly faster
                    dbCmd.Prepare();
                    // run for all events
                    foreach (LoggingEvent e in events)
                    {
                        // clear parameters that have been set
                        dbCmd.Parameters.Clear();

                        // Set the parameter values
                        foreach (AdoNetAppenderParameter param in m_parameters)
                        {
                            param.Prepare(dbCmd);
                            param.FormatValue(dbCmd, e);
                        }

                        // Execute the query
                        dbCmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                // create a new command
                using (IDbCommand dbCmd = Connection.CreateCommand())
                {
                    if (dbTran != null)
                    {
                        dbCmd.Transaction = dbTran;
                    }
                    // run for all events
                    foreach (LoggingEvent e in events)
                    {
                        // Get the command text from the Layout
                        string logStatement = GetLogStatement(e);

                        LogLog.Debug(declaringType, "LogStatement [" + logStatement + "]");

                        dbCmd.CommandText = logStatement;
                        dbCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        virtual protected string GetLogStatement(LoggingEvent logEvent)
        {
            if (Layout == null)
            {
                ErrorHandler.Error("AdoNetAppender: No Layout specified.");
                return "";
            }
            else
            {
                StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                Layout.Format(writer, logEvent);
                return writer.ToString();
            }
        }

        virtual protected IDbConnection CreateConnection(Type connectionType, string connectionString)
        {
            IDbConnection connection = (IDbConnection)Activator.CreateInstance(connectionType);
            connection.ConnectionString = connectionString;
            return connection;
        }

        virtual protected string ResolveConnectionString(out string connectionStringContext)
        {
            if (ConnectionString != null && ConnectionString.Length > 0)
            {
                connectionStringContext = "ConnectionString";
                return ConnectionString;
            }
            if (!String.IsNullOrEmpty(ConnectionStringName))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                if (settings != null)
                {
                    connectionStringContext = "ConnectionStringName";
                    return settings.ConnectionString;
                }
                else
                {
                    throw new LogException("Unable to find [" + ConnectionStringName + "] ConfigurationManager.ConnectionStrings item");
                }
            }
            if (AppSettingsKey != null && AppSettingsKey.Length > 0)
            {
                connectionStringContext = "AppSettingsKey";
                string appSettingsConnectionString = SystemInfo.GetAppSetting(AppSettingsKey);
                if (appSettingsConnectionString == null || appSettingsConnectionString.Length == 0)
                {
                    throw new LogException("Unable to find [" + AppSettingsKey + "] AppSettings key.");
                }
                return appSettingsConnectionString;
            }

            connectionStringContext = "Unable to resolve connection string from ConnectionString, ConnectionStrings, or AppSettings.";
            return string.Empty;
        }

        virtual protected Type ResolveConnectionType()
        {
            try
            {
                return SystemInfo.GetTypeFromString(ConnectionType, true, false);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Failed to load connection type [" + ConnectionType + "]", ex);
                throw;
            }
        }

        #endregion

        #region Private Instance Methods

        private void InitializeDatabaseConnection()
        {
            string connectionStringContext = "Unable to determine connection string context.";
            string resolvedConnectionString = string.Empty;

            try
            {
                DiposeConnection();

                // Set the connection string
                resolvedConnectionString = ResolveConnectionString(out connectionStringContext);

                Connection = CreateConnection(ResolveConnectionType(), resolvedConnectionString);

                using (SecurityContext.Impersonate(this))
                {
                    // Open the database connection
                    Connection.Open();
                }
            }
            catch (Exception e)
            {
                // Sadly, your connection string is bad.
                ErrorHandler.Error("Could not open database connection [" + resolvedConnectionString + "]. Connection string context [" + connectionStringContext + "].", e);

                Connection = null;
            }
        }

        private void DiposeConnection()
        {
            if (Connection != null)
            {
                try
                {
                    Connection.Close();
                }
                catch (Exception ex)
                {
                    LogLog.Warn(declaringType, "Exception while disposing cached connection object", ex);
                }
                Connection = null;
            }
        }

        #endregion

        private SecurityContext m_securityContext;
        private IDbConnection m_dbConnection;
        private string m_connectionString;
        private string m_appSettingsKey;
        private string m_connectionStringName;
        private string m_connectionType;
        private string m_commandText;
        private CommandType m_commandType;
        private bool m_useTransactions;
        private bool m_reconnectOnError;

        private readonly static Type declaringType = typeof(AdoNetAppender);
    }
}
