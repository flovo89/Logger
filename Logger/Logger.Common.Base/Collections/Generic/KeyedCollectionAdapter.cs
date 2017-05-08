using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.Collections.Generic
{
    [DebuggerDisplay ("Count = {Count}")]
    public sealed class KeyedCollectionAdapter <TKey, TItem> : IKeyedCollection<TKey, TItem>,
            ICollection<TItem>,
            ICollection,
            IEnumerable<TItem>,
            IEnumerable,
            ISynchronizable
    {
        #region Constants

        private const string GetKeyForItemMethodName = "GetKeyForItem";

        #endregion




        #region Instance Constructor/Destructor

        private KeyedCollectionAdapter (System.Collections.ObjectModel.KeyedCollection<TKey, TItem> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            this.Collection = collection;
        }

        #endregion




        #region Instance Properties/Indexer

        private System.Collections.ObjectModel.KeyedCollection<TKey, TItem> Collection { get; }

        #endregion




        #region Instance Methods

        private TKey GetKeyForItem (TItem item)
        {
            object result = null;

            try
            {
                result = this.Collection.GetType().CallMethod(this.Collection, KeyedCollectionAdapter<TKey, TItem>.GetKeyForItemMethodName, BindingFlags.NonPublic | BindingFlags.Instance, typeof(TKey), new[]
                {
                    typeof(TItem)
                }, new object[]
                {
                    item
                });
            }
            catch (Exception exception)
            {
                throw new NotSupportedException(exception.Message, exception);
            }

            if (result is TKey)
            {
                return (TKey)result;
            }

            throw new NotSupportedException();
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
                return ( (ICollection<TItem>)this.Collection ).IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ( (ICollection)this.Collection ).IsSynchronized;
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
                return ( (ICollection)this.Collection ).SyncRoot;
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
                return this.Collection[key];
            }
            set
            {
                TItem itemToReplace = default(TItem);
                bool found = false;

                foreach (TItem item in this.Collection)
                {
                    TKey currentKey = this.GetKeyForItem(item);
                    if (this.Collection.Comparer.Equals(key, currentKey))
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
            return this.Collection.Contains(key);
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
            return this.Collection.Remove(key);
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
