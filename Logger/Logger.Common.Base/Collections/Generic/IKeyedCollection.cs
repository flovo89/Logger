using System.Collections;
using System.Collections.Generic;




namespace Logger.Common.Collections.Generic
{
    public interface IKeyedCollection <TKey, TItem> : ICollection<TItem>,
            ICollection,
            IEnumerable<TItem>,
            IEnumerable
    {
        new int Count { get; }

        ICollection<TKey> Keys { get; }

        ICollection<TItem> Values { get; }

        TItem this [TKey key] { get; set; }

        bool ContainsKey (TKey key);

        bool RemoveKey (TKey key);

        bool TryGetValue (TKey key, out TItem item);
    }
}
