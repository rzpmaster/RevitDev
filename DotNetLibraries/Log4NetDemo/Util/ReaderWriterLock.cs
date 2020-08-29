namespace Log4NetDemo.Util
{
    /// <summary>
    /// 定义一个支持单个写入，多个读取的锁
    /// </summary>
    public sealed class ReaderWriterLock
    {
        public ReaderWriterLock()
        {
            m_lock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.SupportsRecursion);
        }

        /// <summary>
        /// 请求读取
        /// </summary>
        /// <remarks>
        /// <para>如果另一个线程具有写入锁，或者至少有一个线程正在等待写入锁，则阻塞</para>
        /// </remarks>
        public void AcquireReaderLock()
        {
            try { }
            finally
            {
                m_lock.EnterReadLock();
            }
        }

        /// <summary>
        /// 释放读取
        /// </summary>
        public void ReleaseReaderLock()
        {
            m_lock.ExitReadLock();
        }

        /// <summary>
        /// 请求写入锁
        /// </summary>
        /// <remarks>
        /// <para>如果另一个线程具有读取或写入锁，则阻塞</para>
        /// </remarks>
        public void AcquireWriterLock()
        {
            try { }
            finally
            {
                m_lock.EnterWriteLock();
            }
        }

        /// <summary>
        /// 释放写入锁
        /// </summary>
        public void ReleaseWriterLock()
        {
            m_lock.ExitWriteLock();
        }

        private System.Threading.ReaderWriterLockSlim m_lock;
    }
}
