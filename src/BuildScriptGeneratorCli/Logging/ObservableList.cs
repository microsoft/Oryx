using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class ObservableList<T> : IList<T>
    {
        private readonly int _messageThresholdLimit;
        private readonly List<T> _inner;

        public ObservableList(int messageThresholdLimit)
        {
            _inner = new List<T>();
            _messageThresholdLimit = messageThresholdLimit;
        }

        public event EventHandler MessageThresholdLimitReached;

        public T this[int index]
        {
            get
            {
                return _inner[index];
            }
            set
            {
                _inner[index] = value;
            }
        }

        public int Count => _inner.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _inner.Add(item);

            if (Count >= _messageThresholdLimit)
            {
                MessageThresholdLimitReached?.Invoke(this, new EventArgs());
            }
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(T item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _inner.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _inner.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _inner.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _inner.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
    }
}
