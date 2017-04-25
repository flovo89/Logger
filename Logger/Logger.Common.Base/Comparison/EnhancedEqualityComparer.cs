using System;
using System.Collections;
using System.Collections.Generic;




namespace Logger.Common.Comparison
{
    public sealed class EnhancedEqualityComparer <T> : IEqualityComparer<T>,
            IEqualityComparer
    {
        #region Instance Constructor/Destructor

        public EnhancedEqualityComparer ()
                : this((x, y) => ObjectComparer.Equals<T>(x, y), x => ObjectComparer.GetHashCode(x))
        {
        }

        public EnhancedEqualityComparer (EqualityCompare<T> comparer)
                : this(comparer, x => ObjectComparer.GetHashCode(x))
        {
        }

        public EnhancedEqualityComparer (HashCode<T> hashCoder)
                : this((x, y) => ObjectComparer.Equals<T>(x, y), hashCoder)
        {
        }

        public EnhancedEqualityComparer (EqualityCompare<T> comparer, HashCode<T> hashCoder)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            if (hashCoder == null)
            {
                throw new ArgumentNullException(nameof(hashCoder));
            }

            this.Comparer = comparer;
            this.HashCoder = hashCoder;
        }

        #endregion




        #region Instance Properties/Indexer

        public EqualityCompare<T> Comparer { get; }

        public HashCode<T> HashCoder { get; }

        #endregion




        #region Interface: IEqualityComparer

        public new bool Equals (object x, object y)
        {
            return this.Comparer((T)x, (T)y);
        }

        public int GetHashCode (object obj)
        {
            return this.HashCoder((T)obj);
        }

        #endregion




        #region Interface: IEqualityComparer<T>

        public bool Equals (T x, T y)
        {
            return this.Comparer(x, y);
        }

        public int GetHashCode (T obj)
        {
            return this.HashCoder(obj);
        }

        #endregion
    }
}
