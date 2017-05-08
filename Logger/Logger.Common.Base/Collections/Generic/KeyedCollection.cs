using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;




namespace Logger.Common.Collections.Generic
{
    [DebuggerDisplay ("Count = {Count}")]
    public abstract class KeyedCollection <TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>,
            IKeyedCollection<TKey, TItem>,
            IList<TItem>,
            IList,
            ICollection<TItem>,
            ICollection,
            IEnumerable<TItem>,
            IEnumerable
    {
        #region Instance Constructor/Destructor

        protected KeyedCollection ()
        {
        }

        protected KeyedCollection (IEqualityComparer<TKey> comparer)
                : base(comparer)
        {
        }

        #endregion




        #region Overrides

        protected sealed override void ClearItems ()
        {
            base.ClearItems();
        }

        protected sealed override void InsertItem (int index, TItem item)
        {
            base.InsertItem(index, item);
        }

        protected sealed override void RemoveItem (int index)
        {
            base.RemoveItem(index);
        }

        protected sealed override void SetItem (int index, TItem item)
        {
            base.SetItem(index, item);
        }

        #endregion




        #region Interface: IKeyedCollection<TKey,TItem>

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

        public ICollection<TItem> Values
        {
            get
            {
                return new List<TItem>(this);
            }
        }

        public new TItem this [TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                foreach (TItem item in this)
                {
                    TKey currentKey = this.GetKeyForItem(item);

                    if (this.Comparer.Equals(key, currentKey))
                    {
                        int index = this.IndexOf(item);
                        this[index] = value;
                    }
                }
            }
        }

        public bool ContainsKey (TKey key)
        {
            return this.Contains(key);
        }

        public bool RemoveKey (TKey key)
        {
            return this.Remove(key);
        }

        public bool TryGetValue (TKey key, out TItem item)
        {
            if (this.ContainsKey(key))
            {
                item = this[key];
                return true;
            }

            item = default(TItem);
            return false;
        }

        #endregion
    }
}
