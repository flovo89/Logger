using System;
using System.Collections.Generic;
using System.Linq;

using Logger.Common.Base.DataTypes;




namespace Logger.Common.Base.Collections.Generic
{
    // ReSharper disable once InconsistentNaming
    public static class ICollectionExtensions
    {
        #region Static Methods

        public static ICollection<T> AddRange <T> (this ICollection<T> collection, IEnumerable<T> items)
        {
            return collection.AddRangeUnique(items, false);
        }

        public static ICollection<T> AddRangeUnique <T> (this ICollection<T> collection, IEnumerable<T> items, bool avoidDuplicate)
        {
            return collection.AddRangeUnique(items, avoidDuplicate, null);
        }

        public static ICollection<T> AddRangeUnique <T> (this ICollection<T> collection, IEnumerable<T> items, bool avoidDuplicate, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            items = items ?? new T[0];

            lock (collection.GetSyncRoot(true))
            {
                List<T> added = new List<T>();

                foreach (T item in items)
                {
                    if (avoidDuplicate && collection.Contains(item, equalityComparer))
                    {
                        continue;
                    }

                    collection.Add(item);

                    added.Add(item);
                }

                return added;
            }
        }

        public static bool AddUnique <T> (this ICollection<T> collection, T value, bool avoidDuplicate)
        {
            return collection.AddUnique(value, avoidDuplicate, null);
        }

        public static bool AddUnique <T> (this ICollection<T> collection, T value, bool avoidDuplicate, IEqualityComparer<T> equalityComparer)
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

                collection.Add(value);
                return true;
            }
        }

        public static bool Contains <T> (this ICollection<T> collection, T item, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            lock (collection.GetSyncRoot(true))
            {
                foreach (T currentItem in collection)
                {
                    if (equalityComparer.Equals(currentItem, item))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool RemoveAll <T> (this ICollection<T> collection, T value)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                bool removed = false;

                while (collection.Remove(value))
                {
                    removed = true;
                }

                return removed;
            }
        }

        public static ICollection<T> RemoveDuplicates <T> (this ICollection<T> collection)
        {
            return collection.RemoveDuplicates(null);
        }

        public static ICollection<T> RemoveDuplicates <T> (this ICollection<T> collection, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            lock (collection.GetSyncRoot(true))
            {
                List<T> removed = new List<T>();

                List<T> items = collection.ToList();

                for (int i1 = 0; i1 < items.Count; i1++)
                {
                    for (int i2 = i1 + 1; i2 < items.Count; i2++)
                    {
                        if (equalityComparer.Equals(items[i1], items[i2]))
                        {
                            collection.Remove(items[i2]);
                            removed.Add(items[i2]);
                        }
                    }
                }

                return removed;
            }
        }

        public static ICollection<T> RemoveRange <T> (this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            items = items ?? new T[0];

            lock (collection.GetSyncRoot(true))
            {
                List<T> removed = new List<T>();

                foreach (T item in items)
                {
                    if (collection.Remove(item))
                    {
                        removed.Add(item);
                    }
                }

                return removed;
            }
        }

        public static ICollection<T> RemoveRangeAll <T> (this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            items = items ?? new T[0];

            lock (collection.GetSyncRoot(true))
            {
                List<T> removed = new List<T>();

                foreach (T item in items)
                {
                    if (collection.RemoveAll(item))
                    {
                        removed.Add(item);
                    }
                }

                return removed;
            }
        }

        public static ICollection<T> RemoveWhere <T> (this ICollection<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            lock (collection.GetSyncRoot(true))
            {
                List<T> removed = new List<T>();

                bool cont = true;
                while (cont)
                {
                    cont = false;
                    foreach (T item in collection)
                    {
                        if (predicate(item))
                        {
                            collection.Remove(item);
                            removed.Add(item);
                            cont = true;
                            break;
                        }
                    }
                }

                return removed;
            }
        }

        #endregion
    }
}
