using System;
using System.Collections.Generic;
using System.Linq;

using Logger.Common.Base.DataTypes;




namespace Logger.Common.Base.Collections.Generic
{
    // ReSharper disable once InconsistentNaming
    public static class IDictionaryExtensions
    {
        #region Static Methods

        public static IDictionary<TKey, TValue> AddRange <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            return dictionary.AddRangeUnique(keys, values, false);
        }

        public static IDictionary<TKey, TValue> AddRangeUnique <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, IEnumerable<TValue> values, bool avoidDuplicate)
        {
            return dictionary.AddRangeUnique(keys, values, avoidDuplicate, null);
        }

        public static IDictionary<TKey, TValue> AddRangeUnique <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, IEnumerable<TValue> values, bool avoidDuplicate, IEqualityComparer<TKey> equalityComparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            keys = keys ?? new TKey[0];
            values = values ?? new TValue[0];

            lock (dictionary.GetSyncRoot(true))
            {
                TKey[] keyArray = keys.ToArray();
                TValue[] valueArray = values.ToArray();

                IDictionary<TKey, TValue> added = new Dictionary<TKey, TValue>();

                for (int i1 = 0; ( i1 < keyArray.Length ) && ( i1 < valueArray.Length ); i1++)
                {
                    if (dictionary.AddUnique(keyArray[i1], valueArray[i1], avoidDuplicate, equalityComparer))
                    {
                        added.Add(keyArray[i1], valueArray[i1]);
                    }
                }

                return added;
            }
        }

        public static bool AddUnique <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, bool avoidDuplicate)
        {
            return dictionary.AddUnique(key, value, avoidDuplicate, null);
        }

        public static bool AddUnique <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, bool avoidDuplicate, IEqualityComparer<TKey> equalityComparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            lock (dictionary.GetSyncRoot(true))
            {
                if (avoidDuplicate && dictionary.ContainsKey(key, equalityComparer))
                {
                    return false;
                }

                dictionary.Add(key, value);
                return true;
            }
        }

        public static bool ContainsKey <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key, null);
        }

        public static bool ContainsKey <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, IEqualityComparer<TKey> equalityComparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            lock (dictionary.GetSyncRoot(true))
            {
                if (equalityComparer == null)
                {
                    return dictionary.ContainsKey(key);
                }

                foreach (TKey currentKey in dictionary.Keys)
                {
                    if (equalityComparer.Equals(currentKey, key))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool ContainsValue <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.ContainsValue(value, null);
        }

        public static bool ContainsValue <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TValue value, IEqualityComparer<TValue> equalityComparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;

            lock (dictionary.GetSyncRoot(true))
            {
                foreach (TValue item in dictionary.Values)
                {
                    if (equalityComparer.Equals(item, value))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static TKey[] GetKeys <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.GetKeys(value, null);
        }

        public static TKey[] GetKeys <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TValue value, IEqualityComparer<TValue> equalityComparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;

            lock (dictionary.GetSyncRoot(true))
            {
                List<TKey> keys = new List<TKey>();

                foreach (KeyValuePair<TKey, TValue> item in dictionary)
                {
                    if (equalityComparer.Equals(item.Value, value))
                    {
                        keys.Add(item.Key);
                    }
                }

                return keys.ToArray();
            }
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetValueOrDefault(key, null, default(TValue));
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.GetValueOrDefault(key, null, defaultValue);
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, IEqualityComparer<TKey> equalityComparer)
        {
            return dictionary.GetValueOrDefault(key, equalityComparer, default(TValue));
        }

        public static TValue GetValueOrDefault <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, IEqualityComparer<TKey> equalityComparer, TValue defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            lock (dictionary.GetSyncRoot(true))
            {
                if (dictionary.ContainsKey(key, equalityComparer))
                {
                    return dictionary[key];
                }

                return defaultValue;
            }
        }

        public static IDictionary<TKey, TValue> RemoveRange <TKey, TValue> (this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> items)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            items = items ?? new TKey[0];

            lock (dictionary.GetSyncRoot(true))
            {
                IDictionary<TKey, TValue> removed = new Dictionary<TKey, TValue>();

                foreach (TKey item in items)
                {
                    bool hasItem = dictionary.ContainsKey(item);

                    TValue value = default(TValue);

                    if (hasItem)
                    {
                        value = dictionary[item];
                    }

                    if (dictionary.Remove(item))
                    {
                        if (hasItem)
                        {
                            removed.Add(item, value);
                        }
                    }
                }

                return removed;
            }
        }

        #endregion
    }
}
