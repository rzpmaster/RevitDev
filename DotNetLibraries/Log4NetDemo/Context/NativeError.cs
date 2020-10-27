using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 表示win32本地错误的类
    /// </summary>
    public sealed class NativeError
    {
        private NativeError(int number, string message)
        {
            m_number = number;
            m_message = message;
        }

        public int Number
        {
            get { return m_number; }
        }

        public string Message
        {
            get { return m_message; }
        }

        [System.Security.SecuritySafeCritical]
        public static NativeError GetLastError()
        {
            int number = Marshal.GetLastWin32Error();
            return new NativeError(number, NativeError.GetErrorMessage(number));
        }

        public static NativeError GetError(int number)
        {
            return new NativeError(number, NativeError.GetErrorMessage(number));
        }

        [System.Security.SecuritySafeCritical]
        public static string GetErrorMessage(int messageId)
        {
            // Win32 constants
            int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;    // The function should allocates a buffer large enough to hold the formatted message
            int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;     // Insert sequences in the message definition are to be ignored
            int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;        // The function should search the system message-table resource(s) for the requested message

            string msgBuf = "";             // buffer that will receive the message
            IntPtr sourcePtr = new IntPtr();    // Location of the message definition, will be ignored
            IntPtr argumentsPtr = new IntPtr(); // Pointer to array of values to insert, not supported as it requires unsafe code

            if (messageId != 0)
            {
                // If the function succeeds, the return value is the number of TCHARs stored in the output buffer, excluding the terminating null character
                int messageSize = FormatMessage(
                    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                    ref sourcePtr,
                    messageId,
                    0,
                    ref msgBuf,
                    255,
                    argumentsPtr);

                if (messageSize > 0)
                {
                    // Remove trailing null-terminating characters (\r\n) from the message
                    msgBuf = msgBuf.TrimEnd(new char[] { '\r', '\n' });
                }
                else
                {
                    // A message could not be located.
                    msgBuf = null;
                }
            }
            else
            {
                msgBuf = null;
            }

            return msgBuf;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int FormatMessage(
            int dwFlags,
            ref IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            ref String lpBuffer,
            int nSize,
            IntPtr Arguments);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "0x{0:x8}", this.Number) + (this.Message != null ? ": " + this.Message : "");
        }

        private int m_number;
        private string m_message;
    }
}
