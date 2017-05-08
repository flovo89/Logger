using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.Collections.Generic
{
    [DebuggerDisplay ("Count = {Count}")]
    public sealed class KeyedCollectionWrapper <TKey, TItem> : IKeyedCollection<TKey, TItem>,
            ICollection<TItem>,
            ICollection,
            IEnumerable<TItem>,
            IEnumerable,
            ISynchronizable
    {
        #region Instance Constructor/Destructor

        public KeyedCollectionWrapper (ICollection<TItem> collection, IKeyResolver<TKey, TItem> resolver)
                : this(collection, resolver, null)
        {
        }

        public KeyedCollectionWrapper (ICollection<TItem> collection, KeyResolveCallback<TKey, TItem> callback)
                : this(collection, callback, null)
        {
        }

        public KeyedCollectionWrapper (ICollection<TItem> collection, IKeyResolver<TKey, TItem> resolver, IEqualityComparer<TKey> comparer)
                : this(collection, resolver, null, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }
        }

        public KeyedCollectionWrapper (ICollection<TItem> collection, KeyResolveCallback<TKey, TItem> callback, IEqualityComparer<TKey> comparer)
                : this(collection, null, callback, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
        }

        private KeyedCollectionWrapper (ICollection<TItem> collection, IKeyResolver<TKey, TItem> resolver, KeyResolveCallback<TKey, TItem> callback, IEqualityComparer<TKey> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            this.Collection = collection;
            this.Resolver = resolver;
            this.Callback = callback;

            this.Comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        #endregion




        #region Instance Properties/Indexer

        public IEqualityComparer<TKey> Comparer { get; }

        private KeyResolveCallback<TKey, TItem> Callback { get; }

        private ICollection<TItem> Collection { get; }

        private IKeyResolver<TKey, TItem> Resolver { get; }

        #endregion




        #region Instance Methods

        private TKey GetKeyForItem (TItem item)
        {
            if (this.Resolver == null)
            {
                return this.Callback(item);
            }

            return this.Resolver.GetKeyForItem(item);
        }

        #endregion




        #region Interface: IKeyedCollection<TKey,TItem>

        public int Count
        {
            get
            {
                return this.Collection.Count;
            }
        }

        bool ICollection<TItem>.IsReadOnly
        {
            get
            {
                return this.Collection.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.Collection.IsSynchronized();
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>();

                foreach (TItem item in this)
                {
                    TKey currentKey = this.GetKeyForItem(item);

                    keys.Add(currentKey);
                }

                return keys;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.Collection.GetSyncRoot(false);
            }
        }

        public ICollection<TItem> Values
        {
            get
            {
                return new List<TItem>(this);
            }
        }

        public TItem this [TKey key]
        {
            get
            {
                foreach (TItem item in this.Collection)
                {
                    TKey currentKey = this.GetKeyForItem(item);
                    if (this.Comparer.Equals(key, currentKey))
                    {
                        return item;
                    }
                }

                throw new KeyNotFoundException();
            }
            set
            {
                TItem itemToReplace = default(TItem);
                bool found = false;

                foreach (TItem item in this.Collection)
                {
                    TKey currentKey = this.GetKeyForItem(item);
                    if (this.Comparer.Equals(key, currentKey))
                    {
                        itemToReplace = item;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new KeyNotFoundException();
                }

                this.Remove(itemToReplace);
                this.Add(value);
            }
        }

        public void Add (TItem item)
        {
            TKey key = this.GetKeyForItem(item);

            if (this.ContainsKey(key))
            {
                throw new ArgumentException(Properties.Resources.KeyedCollectionWrapper_CannotAddDuplicate);
            }

            this.Collection.Add(item);
        }

        public void Clear ()
        {
            this.Collection.Clear();
        }

        public bool Contains (TItem item)
        {
            return this.Collection.Contains(item);
        }

        public bool ContainsKey (TKey key)
        {
            foreach (TItem item in this.Collection)
            {
                TKey currentKey = this.GetKeyForItem(item);

                if (this.Comparer.Equals(key, currentKey))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo (TItem[] array, int arrayIndex)
        {
            this.Collection.CopyTo(array, arrayIndex);
        }

        public void CopyTo (Array array, int index)
        {
            this.Collection.CopyTo(array, index);
        }

        public IEnumerator<TItem> GetEnumerator ()
        {
            return this.Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return this.GetEnumerator();
        }

        public bool Remove (TItem item)
        {
            return this.Collection.Remove(item);
        }

        public bool RemoveKey (TKey key)
        {
            if (!this.ContainsKey(key))
            {
                return false;
            }

            return this.Remove(this[key]);
        }

        public bool TryGetValue (TKey key, out TItem item)
        {
            if (this.ContainsKey(key))
            {
                item = default(TItem);
                return false;
            }

            item = this[key];
            return true;
        }

        #endregion
    }
}
