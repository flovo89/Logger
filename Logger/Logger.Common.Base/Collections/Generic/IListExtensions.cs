using System;
using System.Collections.Generic;

using Logger.Common.Base.DataTypes;
using Logger.Common.Base.Randomizing;




namespace Logger.Common.Base.Collections.Generic
{
    // ReSharper disable once InconsistentNaming
    public static class IListExtensions
    {
        #region Static Methods

        public static int IndexOf <T> (this IList<T> collection, T item, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.IndexOf(item, index, collection.Count - index, null);
            }
        }

        public static int IndexOf <T> (this IList<T> collection, T item, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.IndexOf(item, index, count, null);
            }
        }

        public static int IndexOf <T> (this IList<T> collection, T item, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.IndexOf(item, 0, collection.Count, equalityComparer);
            }
        }

        public static int IndexOf <T> (this IList<T> collection, T item, int index, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.IndexOf(item, index, collection.Count - index, equalityComparer);
            }
        }

        public static int IndexOf <T> (this IList<T> collection, T item, int index, int count, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            lock (collection.GetSyncRoot(true))
            {
                for (int i1 = index; ( i1 < collection.Count ) && ( i1 < index + count ); i1++)
                {
                    if (equalityComparer.Equals(collection[i1], item))
                    {
                        return i1;
                    }
                }

                return -1;
            }
        }

        public static IList<T> InsertRange <T> (this IList<T> collection, int index, IEnumerable<T> items)
        {
            return collection.InsertRangeUnique(index, items, false);
        }

        public static IList<T> InsertRangeUnique <T> (this IList<T> collection, int index, IEnumerable<T> items, bool avoidDuplicate)
        {
            return collection.InsertRangeUnique(index, items, avoidDuplicate, null);
        }

        public static IList<T> InsertRangeUnique <T> (this IList<T> collection, int index, IEnumerable<T> items, bool avoidDuplicate, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            items = items ?? new T[0];

            lock (collection.GetSyncRoot(true))
            {
                List<T> added = new List<T>();
                int count = 0;

                foreach (T item in items)
                {
                    if (avoidDuplicate && collection.Contains(item, equalityComparer))
                    {
                        continue;
                    }

                    collection.Insert(index + count, item);

                    count++;

                    added.Add(item);
                }

                return added;
            }
        }

        public static bool InsertUnique <T> (this IList<T> collection, int index, T value, bool avoidDuplicate)
        {
            return collection.InsertUnique(index, value, avoidDuplicate, null);
        }

        public static bool InsertUnique <T> (this IList<T> collection, int index, T value, bool avoidDuplicate, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                if (avoidDuplicate && collection.Contains(value, equalityComparer))
                {
                    return false;
                }

                collection.Insert(index, value);
                return true;
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.LastIndexOf(item, 0, collection.Count, null);
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.LastIndexOf(item, index, collection.Count - index, null);
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.LastIndexOf(item, index, count, null);
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.LastIndexOf(item, 0, collection.Count, equalityComparer);
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item, int index, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.LastIndexOf(item, index, collection.Count - index, equalityComparer);
            }
        }

        public static int LastIndexOf <T> (this IList<T> collection, T item, int index, int count, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                List<int> indices = new List<int>();

                for (int i1 = index; ( i1 < collection.Count ) && ( i1 < index + count ); i1++)
                {
                    if (equalityComparer.Equals(collection[i1], item))
                    {
                        indices.Add(i1);
                    }
                }

                if (indices.Count == 0)
                {
                    return -1;
                }

                return indices[indices.Count - 1];
            }
        }

        public static void Mix <T> (this IList<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Mix(null, 0, collection.Count);
            }
        }

        public static void Mix <T> (this IList<T> collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Mix(null, index, collection.Count - index);
            }
        }

        public static void Mix <T> (this IList<T> collection, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Mix(null, index, count);
        }

        public static void Mix <T> (this IList<T> collection, Random randomizer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Mix(randomizer, 0, collection.Count);
            }
        }

        public static void Mix <T> (this IList<T> collection, Random randomizer, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Mix(randomizer, index, collection.Count - index);
            }
        }

        public static void Mix <T> (this IList<T> collection, Random randomizer, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                randomizer = randomizer ?? new Random();

                IList<T> mixed = collection.ToSubList(index, count);

                randomizer.Mix(mixed);

                for (int i1 = index; i1 < index + count; i1++)
                {
                    collection[i1] = mixed[i1 - index];
                }
            }
        }

        public static void Move <T> (this IList<T> collection, int oldIndex, int newIndex)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                List<T> list = collection.ToSubList();
                T temp = collection[oldIndex];
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, temp);

                for (int i1 = 0; i1 < list.Count; i1++)
                {
                    collection[i1] = list[i1];
                }
            }
        }

        public static IList<T> RemoveAtRange <T> (this IList<T> collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                return collection.RemoveAtRange(index, collection.Count - index);
            }
        }

        public static IList<T> RemoveAtRange <T> (this IList<T> collection, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                List<T> removed = new List<T>();

                for (int i1 = 0; i1 < count; i1++)
                {
                    removed.Add(collection[index]);
                    collection.RemoveAt(index);
                }

                return removed;
            }
        }

        public static void Reverse <T> (this IList<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Reverse(0, collection.Count);
            }
        }

        public static void Reverse <T> (this IList<T> collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Reverse(index, collection.Count - index);
            }
        }

        public static void Reverse <T> (this IList<T> collection, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                T[] reversed = collection.ToSubArray(index, count);

                Array.Reverse(reversed);

                for (int i1 = index; i1 < index + count; i1++)
                {
                    collection[i1] = reversed[i1 - index];
                }
            }
        }

        public static void Sort <T> (this IList<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Sort(0, collection.Count, null);
            }
        }

        public static void Sort <T> (this IList<T> collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Sort(index, collection.Count - index, null);
            }
        }

        public static void Sort <T> (this IList<T> collection, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Sort(index, count, null);
            }
        }

        public static void Sort <T> (this IList<T> collection, IComparer<T> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Sort(0, collection.Count, comparer);
            }
        }

        public static void Sort <T> (this IList<T> collection, int index, IComparer<T> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                collection.Sort(index, collection.Count - index, comparer);
            }
        }

        public static void Sort <T> (this IList<T> collection, int index, int count, IComparer<T> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            comparer = comparer ?? Comparer<T>.Default;

            lock (collection.GetSyncRoot(true))
            {
                T[] sorted = collection.ToSubArray(index, count);

                Array.Sort(sorted, comparer);

                collection.RemoveAtRange(index, count);

                for (int i1 = index; i1 < index + count; i1++)
                {
                    collection.Insert(i1, sorted[i1 - index]);
                    //collection[i1] = sorted[i1 - index];
                }
            }
        }

        public static void Swap <T> (this IList<T> collection, int firstIndex, int secondIndex)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            T temp = collection[firstIndex];
            collection[firstIndex] = collection[secondIndex];
            collection[secondIndex] = temp;
        }

        #endregion
    }
}
