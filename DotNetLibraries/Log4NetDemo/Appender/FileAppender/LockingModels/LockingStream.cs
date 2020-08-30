using Log4NetDemo.Core.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Log4NetDemo.Appender.LockingModels
{
    /// <summary>
    /// Write only <see cref="Stream"/> that uses the <see cref="LockingModelBase"/> 
    /// to manage access to an underlying resource.
    /// </summary>
    internal sealed class LockingStream : Stream, IDisposable
    {
        public sealed class LockStateException : LogException
        {
            public LockStateException(string message)
                : base(message)
            {
            }
        }

        private Stream m_realStream = null;
        private LockingModelBase m_lockingModel = null;
        private int m_lockLevel = 0;

        internal LockingStream(LockingModelBase locking)
            : base()
        {
            if (locking == null)
            {
                throw new ArgumentException("Locking model may not be null", "locking");
            }
            m_lockingModel = locking;
        }

        #region Override Implementation of Stream

        private int m_readTotal = -1;

        // Methods
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            AssertLocked();
            IAsyncResult ret = m_realStream.BeginRead(buffer, offset, count, callback, state);
            m_readTotal = EndRead(ret);
            return ret;
        }

        /// <summary>
        /// True asynchronous writes are not supported, the implementation forces a synchronous write.
        /// </summary>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            AssertLocked();
            IAsyncResult ret = m_realStream.BeginWrite(buffer, offset, count, callback, state);
            EndWrite(ret);
            return ret;
        }

        public override void Close()
        {
            m_lockingModel.CloseFile();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            AssertLocked();
            return m_readTotal;
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            //No-op, it has already been handled
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            AssertLocked();
            return m_realStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            AssertLocked();
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Flush()
        {
            AssertLocked();
            m_realStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_realStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return m_realStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            AssertLocked();
            return m_realStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            AssertLocked();
            m_realStream.SetLength(value);
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertLocked();
            m_realStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            AssertLocked();
            m_realStream.WriteByte(value);
        }

        // Properties
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get
            {
                AssertLocked();
                return m_realStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                AssertLocked();
                return m_realStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                AssertLocked();
                return m_realStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                AssertLocked();
                return m_realStream.Position;
            }
            set
            {
                AssertLocked();
                m_realStream.Position = value;
            }
        }

        #endregion Override Implementation of Stream

        #region Locking Methods

        private void AssertLocked()
        {
            if (m_realStream == null)
            {
                throw new LockStateException("The file is not currently locked");
            }
        }

        public bool AcquireLock()
        {
            bool ret = false;
            lock (this)
            {
                if (m_lockLevel == 0)
                {
                    // If lock is already acquired, nop
                    m_realStream = m_lockingModel.AcquireLock();
                }
                if (m_realStream != null)
                {
                    m_lockLevel++;
                    ret = true;
                }
            }
            return ret;
        }

        public void ReleaseLock()
        {
            lock (this)
            {
                m_lockLevel--;
                if (m_lockLevel == 0)
                {
                    // If already unlocked, nop
                    m_lockingModel.ReleaseLock();
                    m_realStream = null;
                }
            }
        }

        #endregion Locking Methods
    }
}
