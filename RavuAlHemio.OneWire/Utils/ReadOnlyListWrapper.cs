using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Utils
{
    public class ReadOnlyListWrapper<T> : IList<T>
    {
        private IList<T> _innerList;

        sealed class Enumerator : IEnumerator<T>
        {
            private IEnumerator<T> _innerEnumerator;

            public Enumerator(IEnumerator<T> inner)
            {
                _innerEnumerator = inner;
            }

            public void Dispose()
            {
                _innerEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                _innerEnumerator.Reset();
            }

            public T Current => _innerEnumerator.Current;

            object IEnumerator.Current => Current;
        }

        public ReadOnlyListWrapper([NotNull, ItemCanBeNull] IList<T> innerList)
        {
            if (innerList == null)
            {
                throw new ArgumentNullException(nameof(innerList));
            }

            _innerList = innerList;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_innerList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowCannotModify()
        {
            throw new InvalidOperationException("Cannot modify this list.");
        }

        public void Add(T item)
        {
            ThrowCannotModify();
        }

        public void Clear()
        {
            ThrowCannotModify();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            ThrowCannotModify();
            return false;
        }

        public int Count => _innerList.Count;
        public bool IsReadOnly => true;
        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ThrowCannotModify();
        }

        public void RemoveAt(int index)
        {
            ThrowCannotModify();
        }

        public T this[int index]
        {
            get { return _innerList[index]; }

            // ReSharper disable once ValueParameterNotUsed
            set { ThrowCannotModify(); }
        }
    }
}
