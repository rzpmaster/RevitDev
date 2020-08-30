using System;
using System.IO;
using System.Text;

namespace Log4NetDemo.Appender.LockingModels
{
    public class ExclusiveLock : LockingModelBase
    {
        private Stream m_stream = null;

        /// <summary>
        /// Open the file specified and prepare for logging.
        /// </summary>
        /// <param name="filename">The filename to use</param>
        /// <param name="append">Whether to append to the file, or overwrite</param>
        /// <param name="encoding">The encoding to use</param>
        /// <remarks>
        /// <para>
        /// Open the file specified and prepare for logging. 
        /// No writes will be made until <see cref="AcquireLock"/> is called.
        /// Must be called before any calls to <see cref="AcquireLock"/>,
        /// <see cref="ReleaseLock"/> and <see cref="CloseFile"/>.
        /// </para>
        /// </remarks>
        public override void OpenFile(string filename, bool append, Encoding encoding)
        {
            try
            {
                m_stream = CreateStream(filename, append, FileShare.Read);
            }
            catch (Exception e1)
            {
                CurrentAppender.ErrorHandler.Error("Unable to acquire lock on file " + filename + ". " + e1.Message);
            }
        }

        /// <summary>
        /// Close the file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Close the file. No further writes will be made.
        /// </para>
        /// </remarks>
        public override void CloseFile()
        {
            CloseStream(m_stream);
            m_stream = null;
        }

        /// <summary>
        /// Acquire the lock on the file
        /// </summary>
        /// <returns>A stream that is ready to be written to.</returns>
        /// <remarks>
        /// <para>
        /// Does nothing. The lock is already taken
        /// </para>
        /// </remarks>
        public override Stream AcquireLock()
        {
            return m_stream;
        }

        /// <summary>
        /// Release the lock on the file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Does nothing. The lock will be released when the file is closed.
        /// </para>
        /// </remarks>
        public override void ReleaseLock()
        {
            //NOP
        }

        /// <summary>
        /// Initializes all resources used by this locking model.
        /// </summary>
        public override void ActivateOptions()
        {
            //NOP
        }

        /// <summary>
        /// Disposes all resources that were initialized by this locking model.
        /// </summary>
        public override void OnClose()
        {
            //NOP
        }
    }
}
