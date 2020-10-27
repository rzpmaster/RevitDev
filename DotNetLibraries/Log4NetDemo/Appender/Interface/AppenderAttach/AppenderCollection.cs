using System;
using System.Collections;

namespace Log4NetDemo.Appender.AppenderAttach
{
    public class AppenderCollection : IList/*, ICollection, IEnumerable*/, ICloneable
    {
        private const int DEFAULT_CAPACITY = 16;

        private IAppender[] m_array;
        private int m_count = 0;
        private int m_version = 0;

        #region Constructors

        public AppenderCollection()
        {
            m_array = new IAppender[DEFAULT_CAPACITY];
        }

        public AppenderCollection(int capacity)
        {
            m_array = new IAppender[capacity];
        }

        public AppenderCollection(AppenderCollection c)
        {
            m_array = new IAppender[c.Count];
            AddRange(c);
        }

        public AppenderCollection(IAppender[] a)
        {
            m_array = new IAppender[a.Length];
            AddRange(a);
        }

        public AppenderCollection(ICollection col)
        {
            m_array = new IAppender[col.Count];
            AddRange(col);
        }

        /// <summary>
        /// 只能由子类使用
        /// </summary>
        /// <param name="tag"></param>
        internal protected AppenderCollection(Tag tag)
        {
            m_array = null;
        }

        internal protected enum Tag
        {
            Default
        }

        #endregion

        public static readonly AppenderCollection EmptyCollection = ReadOnly(new AppenderCollection(0));

        public static AppenderCollection ReadOnly(AppenderCollection list)
        {
            if (list == null) throw new ArgumentNullException("list");

            return new ReadOnlyAppenderCollection(list);
        }

        /// 
        /// 该类内部实现的接口方法，都直接或者间接调用这些虚方法。
        /// 子类只需要对这些虚方法重写，就能实现对接口的实现。
        /// 

        #region <virtual Methods>

        #region Operations (type-safe ICollection)

        public virtual int Count
        {
            get { return m_count; }
        }

        public virtual void CopyTo(IAppender[] array)
        {
            this.CopyTo(array, 0);
        }

        public virtual void CopyTo(IAppender[] array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
            {
                throw new System.ArgumentException("Destination array was not long enough.");
            }

            Array.Copy(m_array, 0, array, start, m_count);
        }

        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        public virtual object SyncRoot
        {
            get { return m_array; }
        }

        #endregion

        #region Operations (type-safe IList)

        public virtual IAppender this[int index]
        {
            get
            {
                ValidateIndex(index); // throws
                return m_array[index];
            }
            set
            {
                ValidateIndex(index); // throws
                ++m_version;
                m_array[index] = value;
            }
        }

        public virtual int Add(IAppender item)
        {
            if (m_count == m_array.Length)
            {
                EnsureCapacity(m_count + 1);
            }

            m_array[m_count] = item;
            m_version++;

            return m_count++;
        }

        public virtual void Clear()
        {
            ++m_version;
            m_array = new IAppender[DEFAULT_CAPACITY];
            m_count = 0;
        }

        public virtual object Clone()
        {
            AppenderCollection newCol = new AppenderCollection(m_count);
            Array.Copy(m_array, 0, newCol.m_array, 0, m_count);
            newCol.m_count = m_count;
            newCol.m_version = m_version;

            return newCol;
        }

        public virtual bool Contains(IAppender item)
        {
            for (int i = 0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual int IndexOf(IAppender item)
        {
            for (int i = 0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual void Insert(int index, IAppender item)
        {
            ValidateIndex(index, true); // throws

            if (m_count == m_array.Length)
            {
                EnsureCapacity(m_count + 1);
            }

            if (index < m_count)
            {
                Array.Copy(m_array, index, m_array, index + 1, m_count - index);
            }

            m_array[index] = item;
            m_count++;
            m_version++;
        }

        public virtual void Remove(IAppender item)
        {
            int i = IndexOf(item);
            if (i < 0)
            {
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
            }

            ++m_version;
            RemoveAt(i);
        }

        public virtual void RemoveAt(int index)
        {
            ValidateIndex(index); // throws

            m_count--;

            if (index < m_count)
            {
                Array.Copy(m_array, index + 1, m_array, index, m_count - index);
            }

            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it 
            // over the entry we want to erase.
            IAppender[] temp = new IAppender[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Operations (type-safe IEnumerable)

        public virtual IAppenderCollectionEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion Operations (type-safe IEnumerable)

        #endregion <virtual Methods\> 

        #region Implementation (ICollection)

        void ICollection.CopyTo(Array array, int start)
        {
            if (m_count > 0)
            {
                Array.Copy(m_array, 0, array, start, m_count);
            }
        }

        #endregion

        #region Implementation (IList)

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (IAppender)value; }
        }

        int IList.Add(object x)
        {
            return this.Add((IAppender)x);
        }

        bool IList.Contains(object x)
        {
            return this.Contains((IAppender)x);
        }

        int IList.IndexOf(object x)
        {
            return this.IndexOf((IAppender)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (IAppender)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((IAppender)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        #endregion

        #region Implementation (IEnumerable)

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(this.GetEnumerator());
        }

        #endregion

        #region Public helpers (just to mimic some nice features of ArrayList)

        public virtual int Capacity
        {
            get
            {
                return m_array.Length;
            }
            set
            {
                if (value < m_count)
                {
                    value = m_count;
                }

                if (value != m_array.Length)
                {
                    if (value > 0)
                    {
                        IAppender[] temp = new IAppender[value];
                        Array.Copy(m_array, 0, temp, 0, m_count);
                        m_array = temp;
                    }
                    else
                    {
                        m_array = new IAppender[DEFAULT_CAPACITY];
                    }
                }
            }
        }

        public virtual int AddRange(AppenderCollection x)
        {
            if (m_count + x.Count >= m_array.Length)
            {
                EnsureCapacity(m_count + x.Count);
            }

            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
            m_count += x.Count;
            m_version++;

            return m_count;
        }

        public virtual int AddRange(IAppender[] x)
        {
            if (m_count + x.Length >= m_array.Length)
            {
                EnsureCapacity(m_count + x.Length);
            }

            Array.Copy(x, 0, m_array, m_count, x.Length);
            m_count += x.Length;
            m_version++;

            return m_count;
        }

        public virtual int AddRange(ICollection col)
        {
            if (m_count + col.Count >= m_array.Length)
            {
                EnsureCapacity(m_count + col.Count);
            }

            foreach (object item in col)
            {
                Add((IAppender)item);
            }

            return m_count;
        }

        public virtual void TrimToSize()
        {
            this.Capacity = m_count;
        }

        public virtual IAppender[] ToArray()
        {
            IAppender[] resultArray = new IAppender[m_count];
            if (m_count > 0)
            {
                Array.Copy(m_array, 0, resultArray, 0, m_count);
            }
            return resultArray;
        }

        #endregion

        #region private helpers

        private void ValidateIndex(int i)
        {
            ValidateIndex(i, false);
        }

        private void ValidateIndex(int i, bool allowEqualEnd)
        {
            int max = (allowEqualEnd) ? (m_count) : (m_count - 1);
            if (i < 0 || i > max)
            {
                throw Util.SystemInfo.CreateArgumentOutOfRangeException("i", (object)i, "Index was out of range. Must be non-negative and less than the size of the collection. [" + (object)i + "] Specified argument was out of the range of valid values.");
            }
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
            if (newCapacity < min)
            {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
        }

        #endregion


        private sealed class ReadOnlyAppenderCollection : AppenderCollection, ICollection
        {
            private readonly AppenderCollection m_collection;
            internal ReadOnlyAppenderCollection(AppenderCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }

            #region ICollection

            public override void CopyTo(IAppender[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(IAppender[] array, int start)
            {
                m_collection.CopyTo(array, start);
            }

            void ICollection.CopyTo(Array array, int start)
            {
                ((ICollection)m_collection).CopyTo(array, start);
            }

            public override int Count
            {
                get { return m_collection.Count; }
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }

            #endregion

            #region IList

            public override IAppender this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(IAppender x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(IAppender x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(IAppender x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, IAppender x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(IAppender x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool IsFixedSize
            {
                get { return true; }
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            #endregion

            #region IEnumerable

            public override IAppenderCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }

            #endregion

            #region Helper

            public override int Capacity
            {
                get { return m_collection.Capacity; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(AppenderCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(IAppender[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override IAppender[] ToArray()
            {
                return m_collection.ToArray();
            }

            public override void TrimToSize()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            #endregion
        }

        public interface IAppenderCollectionEnumerator
        {
            IAppender Current { get; }

            bool MoveNext();
            void Reset();
        }

        private sealed class Enumerator : IEnumerator, IAppenderCollectionEnumerator
        {
            private readonly AppenderCollection m_collection;
            private int m_index;
            private int m_version;

            internal Enumerator(AppenderCollection tc)
            {
                m_collection = tc;
                m_index = -1;
                m_version = tc.m_version;
            }

            #region Operations (type-safe IEnumerator)

            public IAppender Current
            {
                get { return m_collection[m_index]; }
            }

            public bool MoveNext()
            {
                if (m_version != m_collection.m_version)
                {
                    throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                ++m_index;
                return (m_index < m_collection.Count);
            }

            public void Reset()
            {
                m_index = -1;
            }

            #endregion

            #region Implementation (IEnumerator)

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            #endregion
        }
    }
}
