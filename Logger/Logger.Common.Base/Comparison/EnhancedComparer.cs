using System;
using System.Collections;
using System.Collections.Generic;




namespace Logger.Common.Comparison
{
    public sealed class EnhancedComparer <T> : IComparer<T>,
            IComparer
    {
        #region Instance Constructor/Destructor

        public EnhancedComparer ()
                : this((x, y) => ObjectComparer.Compare<T>(x, y))
        {
        }

        public EnhancedComparer (OrderCompare<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            this.Comparer = comparer;
        }

        #endregion




        #region Instance Properties/Indexer

        public OrderCompare<T> Comparer { get; }

        #endregion




        #region Interface: IComparer

        public int Compare (object x, object y)
        {
            return this.Comparer((T)x, (T)y);
        }

        #endregion




        #region Interface: IComparer<T>

        public int Compare (T x, T y)
        {
            return this.Comparer(x, y);
        }

        #endregion
    }
}
