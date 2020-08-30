using System.IO;
using System.Text;

namespace Log4NetDemo.Appender.LockingModels
{
    /// <summary>
    /// Locking model base class
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base class for the locking models available to the <see cref="FileAppender"/> derived loggers.
    /// </para>
    /// </remarks>
    public abstract class LockingModelBase
    {
        private FileAppender m_appender = null;

        /// <summary>
        /// Open the output file
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
        public abstract void OpenFile(string filename, bool append, Encoding encoding);

        /// <summary>
        /// Close the file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Close the file. No further writes will be made.
        /// </para>
        /// </remarks>
        public abstract void CloseFile();

        /// <summary>
        /// Initializes all resources used by this locking model.
        /// </summary>
        public abstract void ActivateOptions();

        /// <summary>
        /// Disposes all resources that were initialized by this locking model.
        /// </summary>
        public abstract void OnClose();

        /// <summary>
        /// Acquire the lock on the file
        /// </summary>
        /// <returns>A stream that is ready to be written to.</returns>
        /// <remarks>
        /// <para>
        /// Acquire the lock on the file in preparation for writing to it. 
        /// Return a stream pointing to the file. <see cref="ReleaseLock"/>
        /// must be called to release the lock on the output file.
        /// </para>
        /// </remarks>
        public abstract Stream AcquireLock();

        /// <summary>
        /// Release the lock on the file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Release the lock on the file. No further writes will be made to the 
        /// stream until <see cref="AcquireLock"/> is called again.
        /// </para>
        /// </remarks>
        public abstract void ReleaseLock();

        /// <summary>
        /// Gets or sets the <see cref="FileAppender"/> for this LockingModel
        /// </summary>
        /// <value>
        /// The <see cref="FileAppender"/> for this LockingModel
        /// </value>
        /// <remarks>
        /// <para>
        /// The file appender this locking model is attached to and working on
        /// behalf of.
        /// </para>
        /// <para>
        /// The file appender is used to locate the security context and the error handler to use.
        /// </para>
        /// <para>
        /// The value of this property will be set before <see cref="OpenFile"/> is
        /// called.
        /// </para>
        /// </remarks>
        public FileAppender CurrentAppender
        {
            get { return m_appender; }
            set { m_appender = value; }
        }

        /// <summary>
        /// Helper method that creates a FileStream under CurrentAppender's SecurityContext.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Typically called during OpenFile or AcquireLock. 
        /// </para>
        /// <para>
        /// If the directory portion of the <paramref name="filename"/> does not exist, it is created
        /// via Directory.CreateDirecctory.
        /// </para>
        /// </remarks>
        /// <param name="filename"></param>
        /// <param name="append"></param>
        /// <param name="fileShare"></param>
        /// <returns></returns>
        protected Stream CreateStream(string filename, bool append, FileShare fileShare)
        {
            using (CurrentAppender.SecurityContext.Impersonate(this))
            {
                // Ensure that the directory structure exists
                string directoryFullName = Path.GetDirectoryName(filename);

                // Only create the directory if it does not exist
                // doing this check here resolves some permissions failures
                if (!Directory.Exists(directoryFullName))
                {
                    Directory.CreateDirectory(directoryFullName);
                }

                FileMode fileOpenMode = append ? FileMode.Append : FileMode.Create;
                return new FileStream(filename, fileOpenMode, FileAccess.Write, fileShare);
            }
        }

        /// <summary>
        /// Helper method to close <paramref name="stream"/> under CurrentAppender's SecurityContext.
        /// </summary>
        /// <remarks>
        /// Does not set <paramref name="stream"/> to null.
        /// </remarks>
        /// <param name="stream"></param>
        protected void CloseStream(Stream stream)
        {
            using (CurrentAppender.SecurityContext.Impersonate(this))
            {
                stream.Close();
            }
        }
    }
}
