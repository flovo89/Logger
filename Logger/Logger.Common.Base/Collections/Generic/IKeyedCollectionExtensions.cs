using System;
using System.Collections.Generic;

using Logger.Common.DataTypes;




namespace Logger.Common.Collections.Generic
{
    // ReSharper disable once InconsistentNaming
    public static class IKeyedCollectionExtensions
    {
        #region Static Methods

        public static bool ContainsKey <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key)
        {
            return keyedCollection.ContainsKey(key, null);
        }

        public static bool ContainsKey <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key, IEqualityComparer<TKey> equalityComparer)
        {
            if (keyedCollection == null)
            {
                throw new ArgumentNullException(nameof(keyedCollection));
            }

            equalityComparer = equalityComparer ?? keyedCollection.GetKeyComparer();

            lock (keyedCollection.GetSyncRoot(true))
            {
                foreach (TKey currentKey in keyedCollection.Keys)
                {
                    if (equalityComparer.Equals(currentKey, key))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool ContainsValue <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TValue value)
        {
            return keyedCollection.ContainsValue(value, null);
        }

        public static bool ContainsValue <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TValue value, IEqualityComparer<TValue> equalityComparer)
        {
            if (keyedCollection == null)
            {
                throw new ArgumentNullException(nameof(keyedCollection));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;

            lock (keyedCollection.GetSyncRoot(true))
            {
                foreach (TValue item in keyedCollection)
                {
                    if (equalityComparer.Equals(item, value))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static IEqualityComparer<TKey> GetKeyComparer <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection)
        {
            if (keyedCollection == null)
            {
                throw new ArgumentNullException(nameof(keyedCollection));
            }

            IEqualityComparer<TKey> equalityComparer = null;

            lock (keyedCollection.GetSyncRoot(true))
            {
                if (keyedCollection is KeyedCollection<TKey, TValue>)
                {
                    equalityComparer = ( (KeyedCollection<TKey, TValue>)keyedCollection ).Comparer;
                }

                if (keyedCollection is KeyedCollectionWrapper<TKey, TValue>)
                {
                    equalityComparer = ( (KeyedCollectionWrapper<TKey, TValue>)keyedCollection ).Comparer;
                }
            }

            equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;

            return equalityComparer;
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key)
        {
            return keyedCollection.GetValueOrDefault(key, null, default(TValue));
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key, TValue defaultValue)
        {
            return keyedCollection.GetValueOrDefault(key, null, defaultValue);
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key, IEqualityComparer<TKey> equalityComparer)
        {
            return keyedCollection.GetValueOrDefault(key, equalityComparer, default(TValue));
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, TKey key, IEqualityComparer<TKey> equalityComparer, TValue defaultValue)
        {
            if (keyedCollection == null)
            {
                throw new ArgumentNullException(nameof(keyedCollection));
            }

            equalityComparer = equalityComparer ?? keyedCollection.GetKeyComparer();

            lock (keyedCollection.GetSyncRoot(true))
            {
                if (keyedCollection.ContainsKey(key, equalityComparer))
                {
                    return keyedCollection[key];
                }

                return defaultValue;
            }
        }

        public static ICollection<TValue> RemoveKeyRange <TKey, TValue> (this IKeyedCollection<TKey, TValue> keyedCollection, IEnumerable<TKey> keys)
        {
            if (keyedCollection == null)
            {
                throw new ArgumentNullException(nameof(keyedCollection));
            }

            keys = keys ?? new TKey[0];

            lock (keyedCollection.GetSyncRoot(true))
            {
                List<TValue> removed = new List<TValue>();

                foreach (TKey key in keys)
                {
                    bool hasItem = keyedCollection.ContainsKey(key);

                    TValue value = default(TValue);

                    if (hasItem)
                    {
                        value = keyedCollection[key];
                    }

                    if (keyedCollection.RemoveKey(key))
                    {
                        if (hasItem)
                        {
                            removed.Add(value);
                        }
                    }
                }

                return removed;
            }
        }

        #endregion
    }
}
