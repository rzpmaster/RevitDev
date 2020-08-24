using System;
using System.Collections;

namespace Log4NetDemo.Util.Collections
{
    public sealed class NullEnumerator : IEnumerator
    {
        private NullEnumerator()
        {
        }

        public static NullEnumerator Instance
        {
            get { return s_instance; }
        }

        #region Implementation of IEnumerator

        public object Current
        {
            get { throw new InvalidOperationException(); }
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        #endregion Implementation of IEnumerator

        private readonly static NullEnumerator s_instance = new NullEnumerator();
    }
}
