using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Appender
{
    public class ManagedColoredConsoleAppender : AppenderSkeleton
    {
        public ManagedColoredConsoleAppender()
        {
        }

        virtual public string Target
        {
            get { return m_writeToErrorStream ? ConsoleError : ConsoleOut; }
            set
            {
                string v = value.Trim();

                if (SystemInfo.EqualsIgnoringCase(ConsoleError, v))
                {
                    m_writeToErrorStream = true;
                }
                else
                {
                    m_writeToErrorStream = false;
                }
            }
        }

        public void AddMapping(LevelColors mapping)
        {
            m_levelMapping.Add(mapping);
        }

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            System.IO.TextWriter writer;

            if (m_writeToErrorStream)
                writer = Console.Error;
            else
                writer = Console.Out;

            // Reset color
            Console.ResetColor();

            // see if there is a specified lookup
            LevelColors levelColors = m_levelMapping.Lookup(loggingEvent.Level) as LevelColors;
            if (levelColors != null)
            {
                // if the backColor has been explicitly set
                if (levelColors.HasBackColor)
                    Console.BackgroundColor = levelColors.BackColor;
                // if the foreColor has been explicitly set
                if (levelColors.HasForeColor)
                    Console.ForegroundColor = levelColors.ForeColor;
            }

            // Render the event to a string
            string strLoggingMessage = RenderLoggingEvent(loggingEvent);
            // and write it
            writer.Write(strLoggingMessage);

            // Reset color again
            Console.ResetColor();
        }

        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion

        #region implementation of IOptionHandler

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            m_levelMapping.ActivateOptions();
        }

        #endregion

        #region LevelColors LevelMapping Entry

        /// <summary>
		/// A class to act as a mapping between the level that a logging call is made at and
		/// the color it should be displayed as.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Defines the mapping between a level and the color it should be displayed in.
		/// </para>
		/// </remarks>
		public class LevelColors : LevelMappingEntry
        {
            /// <summary>
            /// The mapped foreground color for the specified level
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped foreground color for the specified level.
            /// </para>
            /// </remarks>
            public ConsoleColor ForeColor
            {
                get { return (this.foreColor); }
                // Keep a flag that the color has been set
                // and is no longer the default.
                set { this.foreColor = value; this.hasForeColor = true; }
            }
            private ConsoleColor foreColor;
            private bool hasForeColor;
            internal bool HasForeColor
            {
                get
                {
                    return hasForeColor;
                }
            }

            /// <summary>
            /// The mapped background color for the specified level
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped background color for the specified level.
            /// </para>
            /// </remarks>
            public ConsoleColor BackColor
            {
                get { return (this.backColor); }
                // Keep a flag that the color has been set
                // and is no longer the default.
                set { this.backColor = value; this.hasBackColor = true; }
            }
            private ConsoleColor backColor;
            private bool hasBackColor;
            internal bool HasBackColor
            {
                get
                {
                    return hasBackColor;
                }
            }
        }

        #endregion

        public const string ConsoleOut = "Console.Out";
        public const string ConsoleError = "Console.Error";

        private bool m_writeToErrorStream = false;
        private LevelMapping m_levelMapping = new LevelMapping();
    }
}
