using Log4NetDemo.Core.Data;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Log4NetDemo.Appender
{
    public class ColoredConsoleAppender : AppenderSkeleton
    {
        public ColoredConsoleAppender()
        {
        }

        virtual public string Target
        {
            get { return m_writeToErrorStream ? ConsoleError : ConsoleOut; }
            set
            {
                string v = value.Trim();

                if (string.Compare(ConsoleError, v, true, CultureInfo.InvariantCulture) == 0)
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

        [System.Security.SecuritySafeCritical]
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        override protected void Append(LoggingEvent loggingEvent)
        {
            if (m_consoleOutputWriter != null)
            {
                IntPtr consoleHandle = IntPtr.Zero;
                if (m_writeToErrorStream)
                {
                    // Write to the error stream
                    consoleHandle = GetStdHandle(STD_ERROR_HANDLE);
                }
                else
                {
                    // Write to the output stream
                    consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                }

                // Default to white on black
                ushort colorInfo = (ushort)Colors.White;

                // see if there is a specified lookup
                LevelColors levelColors = m_levelMapping.Lookup(loggingEvent.Level) as LevelColors;
                if (levelColors != null)
                {
                    colorInfo = levelColors.CombinedColor;
                }

                // Render the event to a string
                string strLoggingMessage = RenderLoggingEvent(loggingEvent);

                // get the current console color - to restore later
                CONSOLE_SCREEN_BUFFER_INFO bufferInfo;
                GetConsoleScreenBufferInfo(consoleHandle, out bufferInfo);

                // set the console colors
                SetConsoleTextAttribute(consoleHandle, colorInfo);

                char[] messageCharArray = strLoggingMessage.ToCharArray();
                int arrayLength = messageCharArray.Length;
                bool appendNewline = false;

                // Trim off last newline, if it exists
                if (arrayLength > 1 && messageCharArray[arrayLength - 2] == '\r' && messageCharArray[arrayLength - 1] == '\n')
                {
                    arrayLength -= 2;
                    appendNewline = true;
                }

                // Write to the output stream
                m_consoleOutputWriter.Write(messageCharArray, 0, arrayLength);

                // Restore the console back to its previous color scheme
                SetConsoleTextAttribute(consoleHandle, bufferInfo.wAttributes);

                if (appendNewline)
                {
                    // Write the newline, after changing the color scheme
                    m_consoleOutputWriter.Write(s_windowsNewline, 0, 2);
                }
            }
        }

        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion

        #region implementation of IOptionHandler

        [System.Security.SecuritySafeCritical]
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        public override void ActivateOptions()
        {
            base.ActivateOptions();
            m_levelMapping.ActivateOptions();

            System.IO.Stream consoleOutputStream = null;

            // Use the Console methods to open a Stream over the console std handle
            if (m_writeToErrorStream)
            {
                // Write to the error stream
                consoleOutputStream = Console.OpenStandardError();
            }
            else
            {
                // Write to the output stream
                consoleOutputStream = Console.OpenStandardOutput();
            }

            // Lookup the codepage encoding for the console
            System.Text.Encoding consoleEncoding = System.Text.Encoding.GetEncoding(GetConsoleOutputCP());

            // Create a writer around the console stream
            m_consoleOutputWriter = new System.IO.StreamWriter(consoleOutputStream, consoleEncoding, 0x100);

            m_consoleOutputWriter.AutoFlush = true;

            // SuppressFinalize on m_consoleOutputWriter because all it will do is flush
            // and close the file handle. Because we have set AutoFlush the additional flush
            // is not required. The console file handle should not be closed, so we don't call
            // Dispose, Close or the finalizer.
            GC.SuppressFinalize(m_consoleOutputWriter);
        }

        #endregion

        #region Win32 Methods

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetConsoleOutputCP();

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetConsoleTextAttribute(
            IntPtr consoleHandle,
            ushort attributes);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetConsoleScreenBufferInfo(
            IntPtr consoleHandle,
            out CONSOLE_SCREEN_BUFFER_INFO bufferInfo);

        private const UInt32 STD_OUTPUT_HANDLE = unchecked((UInt32)(-11));
        private const UInt32 STD_ERROR_HANDLE = unchecked((UInt32)(-12));

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetStdHandle(
            UInt32 type);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public UInt16 x;
            public UInt16 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public UInt16 Left;
            public UInt16 Top;
            public UInt16 Right;
            public UInt16 Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        #endregion

        #region Colors Enum

        /// <summary>
        /// The enum of possible color values for use with the color mapping method
        /// </summary>
        /// <remarks>
        /// <para>
        /// The following flags can be combined together to
        /// form the colors.
        /// </para>
        /// </remarks>
        /// <seealso cref="ColoredConsoleAppender" />
        [Flags]
        public enum Colors : int
        {
            /// <summary>
            /// color is blue
            /// </summary>
            Blue = 0x0001,

            /// <summary>
            /// color is green
            /// </summary>
            Green = 0x0002,

            /// <summary>
            /// color is red
            /// </summary>
            Red = 0x0004,

            /// <summary>
            /// color is white
            /// </summary>
            White = Blue | Green | Red,

            /// <summary>
            /// color is yellow
            /// </summary>
            Yellow = Red | Green,

            /// <summary>
            /// color is purple
            /// </summary>
            Purple = Red | Blue,

            /// <summary>
            /// color is cyan
            /// </summary>
            Cyan = Green | Blue,

            /// <summary>
            /// color is intensified
            /// </summary>
            HighIntensity = 0x0008,
        }

        #endregion // Colors Enum

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
            private Colors m_foreColor;
            private Colors m_backColor;
            private ushort m_combinedColor = 0;

            /// <summary>
            /// The mapped foreground color for the specified level
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped foreground color for the specified level.
            /// </para>
            /// </remarks>
            public Colors ForeColor
            {
                get { return m_foreColor; }
                set { m_foreColor = value; }
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
            public Colors BackColor
            {
                get { return m_backColor; }
                set { m_backColor = value; }
            }

            /// <summary>
            /// Initialize the options for the object
            /// </summary>
            /// <remarks>
            /// <para>
            /// Combine the <see cref="ForeColor"/> and <see cref="BackColor"/> together.
            /// </para>
            /// </remarks>
            public override void ActivateOptions()
            {
                base.ActivateOptions();
                m_combinedColor = (ushort)((int)m_foreColor + (((int)m_backColor) << 4));
            }

            /// <summary>
            /// The combined <see cref="ForeColor"/> and <see cref="BackColor"/> suitable for 
            /// setting the console color.
            /// </summary>
            internal ushort CombinedColor
            {
                get { return m_combinedColor; }
            }
        }

        #endregion // LevelColors LevelMapping Entry


        public const string ConsoleOut = "Console.Out";
        public const string ConsoleError = "Console.Error";

        private bool m_writeToErrorStream = false;
        private LevelMapping m_levelMapping = new LevelMapping();
        private System.IO.StreamWriter m_consoleOutputWriter = null;

        private static readonly char[] s_windowsNewline = { '\r', '\n' };
    }

}
