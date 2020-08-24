using System;
using System.Collections;

namespace Log4NetDemo.Util.Collections
{
    [Serializable]
    public sealed class EmptyCollection : ICollection, IEnumerable
    {
        private EmptyCollection()
        {
        }

        public static EmptyCollection Instance
        {
            get { return s_instance; }
        }

        #region Implementation of ICollection

        public void CopyTo(System.Array array, int index)
        {
            // copy nothing
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public int Count
        {
            get { return 0; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion Implementation of ICollection

        #region Implementation of IEnumerable

        public IEnumerator GetEnumerator()
        {
            return NullEnumerator.Instance;
        }

        #endregion Implementation of IEnumerable

        private readonly static EmptyCollection s_instance = new EmptyCollection();
    }
}
