using System;
using System.IO;
using System.Text;

namespace Log4NetDemo.Appender.LockingModels
{
    public class MinimalLock : LockingModelBase
	{
		private string m_filename;
		private bool m_append;
		private Stream m_stream = null;

		/// <summary>
		/// Prepares to open the file when the first message is logged.
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
			m_filename = filename;
			m_append = append;
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
			// NOP
		}

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
		public override Stream AcquireLock()
		{
			if (m_stream == null)
			{
				try
				{
					m_stream = CreateStream(m_filename, m_append, FileShare.Read);
					m_append = true;
				}
				catch (Exception e1)
				{
					CurrentAppender.ErrorHandler.Error("Unable to acquire lock on file " + m_filename + ". " + e1.Message);
				}
			}
			return m_stream;
		}

		/// <summary>
		/// Release the lock on the file
		/// </summary>
		/// <remarks>
		/// <para>
		/// Release the lock on the file. No further writes will be made to the 
		/// stream until <see cref="AcquireLock"/> is called again.
		/// </para>
		/// </remarks>
		public override void ReleaseLock()
		{
			CloseStream(m_stream);
			m_stream = null;
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
