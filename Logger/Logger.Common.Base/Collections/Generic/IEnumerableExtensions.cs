using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Logger.Common.DataTypes;




namespace Logger.Common.Collections.Generic
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        #region Static Methods

        public static IEnumerable<T> AsEnumerable <T> (this IEnumerable collection)
        {
            return collection.AsEnumerable<T>(false);
        }

        public static IEnumerable<T> AsEnumerable <T> (this IEnumerable collection, bool omitIncompatibleTypes)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return new NonGenericEnumerableWrapper<T>(collection, omitIncompatibleTypes);
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, Array array)
        {
            collection.CopyTo(array, 0, array.Length);
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, Array array, int index)
        {
            collection.CopyTo(array, index, array.Length - index);
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, Array array, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            lock (collection.GetSyncRoot(true))
            {
                int currentIndex = index;

                foreach (T item in collection)
                {
                    if (currentIndex >= index + count)
                    {
                        break;
                    }

                    array.SetValue(item, currentIndex);

                    currentIndex++;
                }
            }
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, T[] array)
        {
            collection.CopyTo((Array)array);
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, T[] array, int index)
        {
            collection.CopyTo((Array)array, index);
        }

        public static void CopyTo <T> (this IEnumerable<T> collection, T[] array, int index, int count)
        {
            collection.CopyTo((Array)array, index, count);
        }

        public static int Count (this IEnumerable collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int counter = 0;

            lock (collection.GetSyncRoot(true))
            {
                counter = Enumerable.Count(collection.Cast<object>());
            }

            return counter;
        }

        public static int ForEach <T> (this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int counter = 0;

            lock (collection.GetSyncRoot(true))
            {
                foreach (T item in collection)
                {
                    action(item);
                    counter++;
                }
            }

            return counter;
        }

        public static HashSet<T> ToHashSet <T> (this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return new HashSet<T>(collection);
        }

        public static HashSet<T> ToHashSet <T> (this IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (equalityComparer == null)
            {
                throw new ArgumentNullException(nameof(equalityComparer));
            }

            return new HashSet<T>(collection, equalityComparer);
        }

        public static T[] ToSubArray <T> (this IEnumerable<T> collection)
        {
            return collection.ToSubArray(0, -1);
        }

        public static T[] ToSubArray <T> (this IEnumerable<T> collection, int index)
        {
            return collection.ToSubArray(index, -1);
        }

        public static T[] ToSubArray <T> (this IEnumerable<T> collection, int index, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            List<T> list = new List<T>();

            int counter = 0;
            int indexer = 0;

            lock (collection.GetSyncRoot(true))
            {
                foreach (T item in collection)
                {
                    if (( counter >= count ) && ( count != -1 ))
                    {
                        break;
                    }

                    if (indexer >= index)
                    {
                        list.Add(item);

                        counter++;
                    }

                    indexer++;
                }
            }

            return list.ToArray();
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection)
        {
            return collection.ToSubHashSet(0, -1);
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection, int index)
        {
            return collection.ToSubHashSet(index, -1);
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection, int index, int count)
        {
            return new HashSet<T>(collection.ToSubArray(index, count));
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
        {
            return collection.ToSubHashSet(0, -1, equalityComparer);
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection, int index, IEqualityComparer<T> equalityComparer)
        {
            return collection.ToSubHashSet(index, -1, equalityComparer);
        }

        public static HashSet<T> ToSubHashSet <T> (this IEnumerable<T> collection, int index, int count, IEqualityComparer<T> equalityComparer)
        {
            return new HashSet<T>(collection.ToSubArray(index, count), equalityComparer);
        }

        public static List<T> ToSubList <T> (this IEnumerable<T> collection)
        {
            return collection.ToSubList(0, -1);
        }

        public static List<T> ToSubList <T> (this IEnumerable<T> collection, int index)
        {
            return collection.ToSubList(index, -1);
        }

        public static List<T> ToSubList <T> (this IEnumerable<T> collection, int index, int count)
        {
            return new List<T>(collection.ToSubArray(index, count));
        }

        #endregion




        #region Type: NonGenericEnumerableWrapper

        private sealed class NonGenericEnumerableWrapper <T> : IEnumerable<T>,
                IEnumerable
        {
            #region Instance Constructor/Destructor

            public NonGenericEnumerableWrapper (IEnumerable collection, bool omitIncompatibleTypes)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                this.Collection = collection;
                this.OmitIncompatibleTypes = omitIncompatibleTypes;
            }

            #endregion




            #region Instance Properties/Indexer

            private IEnumerable Collection { get; }

            private bool OmitIncompatibleTypes { get; }

            #endregion




            #region Interface: IEnumerable<T>

            public IEnumerator<T> GetEnumerator ()
            {
                return new NonGenericEnumeratorWrapper<T>(this.Collection.GetEnumerator(), this.OmitIncompatibleTypes);
            }

            IEnumerator IEnumerable.GetEnumerator ()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        #endregion




        #region Type: NonGenericEnumeratorWrapper

        private sealed class NonGenericEnumeratorWrapper <T> : IEnumerator<T>,
                IEnumerator,
                IDisposable
        {
            #region Instance Constructor/Destructor

            public NonGenericEnumeratorWrapper (IEnumerator enumerator, bool omitIncompatibleTypes)
            {
                if (enumerator == null)
                {
                    throw new ArgumentNullException(nameof(enumerator));
                }

                this.Enumerator = enumerator;
                this.OmitIncompatibleTypes = omitIncompatibleTypes;
            }

            #endregion




            #region Instance Properties/Indexer

            private IEnumerator Enumerator { get; }

            private bool OmitIncompatibleTypes { get; }

            #endregion




            #region Instance Methods

            private bool CheckTypeCompatibility (object value)
            {
                if (value == null)
                {
                    return default(T) == null;
                }

                Type valueType = value.GetType();

                return valueType.CanConvertTo(valueType);
            }

            private bool IsExceptionalAllowedNullValue (object value)
            {
                return !this.CheckTypeCompatibility(value) && ( value == null );
            }

            #endregion




            #region Interface: IEnumerator<T>

            public T Current
            {
                get
                {
                    return (T)this.Enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            void IDisposable.Dispose ()
            {
            }

            public bool MoveNext ()
            {
                if (this.OmitIncompatibleTypes)
                {
                    while (this.Enumerator.MoveNext())
                    {
                        if (this.CheckTypeCompatibility(this.Enumerator.Current))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return this.Enumerator.MoveNext();
            }

            public void Reset ()
            {
                this.Enumerator.Reset();
            }

            #endregion
        }

        #endregion
    }
}
