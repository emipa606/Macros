using System.Collections;
using System.Collections.Generic;
using Verse;

namespace RimWorld.CactusPie.Macros.Collections
{
    // This collection encapsulates List as an IExposable object
    // It's necessary in order to properly persist a dictionary with List
    // as a value. For instance:
    // Dictionary<string, ExposableList<CustomClass>> _dictionary
    // Scribe_Collections.Look(ref _dictionary, "example", LookMode.Value, LookMode.Deep);
    // The above would not work for a regular list
    /// <summary>
    /// An exposable list supporting LookMode.Deep
    /// </summary>
    /// <typeparam name="TExposable">A type implementing IExposable</typeparam>
    public class ExposableList<TExposable> : IExposable, IList<TExposable>, IReadOnlyList<TExposable> where TExposable : IExposable
    {
        private List<TExposable> _list;

        public ExposableList()
        {
            _list = new List<TExposable>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _list, "ExposableList_list", LookMode.Deep);
        }

        public IEnumerator<TExposable> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TExposable item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(TExposable item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(TExposable[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(TExposable item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public int IndexOf(TExposable item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TExposable item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public TExposable this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }
}