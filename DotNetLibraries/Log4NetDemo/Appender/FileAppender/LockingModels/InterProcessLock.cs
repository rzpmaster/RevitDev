using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Log4NetDemo.Appender.LockingModels
{
    public class InterProcessLock : LockingModelBase
    {
        private Mutex m_mutex = null;
        private Stream m_stream = null;
        private int m_recursiveWatch = 0;

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
        /// -<see cref="ReleaseLock"/> and <see cref="CloseFile"/>.
        /// </para>
        /// </remarks>
        [System.Security.SecuritySafeCritical]
        public override void OpenFile(string filename, bool append, Encoding encoding)
        {
            try
            {
                m_stream = CreateStream(filename, append, FileShare.ReadWrite);
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
            try
            {
                CloseStream(m_stream);
                m_stream = null;
            }
            finally
            {
                ReleaseLock();
            }
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
            if (m_mutex != null)
            {
                // TODO: add timeout?
                m_mutex.WaitOne();

                // increment recursive watch
                m_recursiveWatch++;

                // should always be true (and fast) for FileStream
                if (m_stream != null)
                {
                    if (m_stream.CanSeek)
                    {
                        m_stream.Seek(0, SeekOrigin.End);
                    }
                }
                else
                {
                    // this can happen when the file appender cannot open a file for writing
                }
            }
            else
            {
                CurrentAppender.ErrorHandler.Error("Programming error, no mutex available to acquire lock! From here on things will be dangerous!");
            }
            return m_stream;
        }

        /// <summary>
        /// Releases the lock and allows others to acquire a lock.
        /// </summary>
        public override void ReleaseLock()
        {
            if (m_mutex != null)
            {
                if (m_recursiveWatch > 0)
                {
                    m_recursiveWatch--;
                    m_mutex.ReleaseMutex();
                }
            }
            else
            {
                CurrentAppender.ErrorHandler.Error("Programming error, no mutex available to release the lock!");
            }
        }

        /// <summary>
        /// Initializes all resources used by this locking model.
        /// </summary>
        public override void ActivateOptions()
        {
            if (m_mutex == null)
            {
                string mutexFriendlyFilename = CurrentAppender.File
                        .Replace("\\", "_")
                        .Replace(":", "_")
                        .Replace("/", "_");

                m_mutex = new Mutex(false, mutexFriendlyFilename);
            }
            else
            {
                CurrentAppender.ErrorHandler.Error("Programming error, mutex already initialized!");
            }
        }

        /// <summary>
        /// Disposes all resources that were initialized by this locking model.
        /// </summary>
        public override void OnClose()
        {
            if (m_mutex != null)
            {
                m_mutex.Dispose();
                m_mutex = null;
            }
            else
            {
                CurrentAppender.ErrorHandler.Error("Programming error, mutex not initialized!");
            }
        }
    }
}
