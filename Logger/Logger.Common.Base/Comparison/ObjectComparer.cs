using System;
using System.Collections.Generic;

using Logger.Common.Base.DataTypes;




namespace Logger.Common.Base.Comparison
{
    public static class ObjectComparer
    {
        #region Static Methods

        public static int Compare <T1> (T1 x, T1 y)
        {
            return ObjectComparer.Compare(x, y, null, null);
        }

        public static int Compare <T1> (T1 x, T1 y, IComparer<T1> comparer)
        {
            return ObjectComparer.Compare(x, y, comparer, comparer);
        }

        public static int Compare <T1, T2> (T1 x, T2 y)
        {
            return ObjectComparer.Compare(x, y, null, null);
        }

        public static int Compare <T1, T2> (T1 x, T2 y, IComparer<T1> xComparer, IComparer<T2> yComparer)
        {
            if (( x == null ) && ( y == null ))
            {
                return 0;
            }

            if (( x == null ) || ( y == null ))
            {
                return x == null ? -1 : 1;
            }

            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }

            lock (x.GetSyncRoot(true))
            {
                lock (y.GetSyncRoot(true))
                {
                    if (xComparer != null)
                    {
                        if (y is T1)
                        {
                            object temp = y;
                            return xComparer.Compare(x, (T1)temp);
                        }
                    }

                    if (yComparer != null)
                    {
                        if (x is T2)
                        {
                            object temp = x;
                            return yComparer.Compare(y, (T2)temp);
                        }
                    }

                    if (x is IComparable<T2>)
                    {
                        return ( (IComparable<T2>)x ).CompareTo(y);
                    }

                    if (y is IComparable<T1>)
                    {
                        return ( (IComparable<T1>)y ).CompareTo(x);
                    }

                    if (x is IComparable)
                    {
                        return ( (IComparable)x ).CompareTo(y);
                    }

                    if (y is IComparable)
                    {
                        return ( (IComparable)y ).CompareTo(x);
                    }

                    if (y is T1)
                    {
                        object temp = y;
                        return Comparer<T1>.Default.Compare(x, (T1)temp);
                    }

                    if (x is T2)
                    {
                        object temp = x;
                        return Comparer<T2>.Default.Compare(y, (T2)temp);
                    }

                    return 0;
                }
            }
        }

        public static bool Equals <T1> (T1 x, T1 y)
        {
            return ObjectComparer.Equals(x, y, null, null);
        }

        public static bool Equals <T1> (T1 x, T1 y, IEqualityComparer<T1> comparer)
        {
            return ObjectComparer.Equals(x, y, comparer, comparer);
        }

        public static bool Equals <T1, T2> (T1 x, T2 y)
        {
            return ObjectComparer.Equals(x, y, null, null);
        }

        public static bool Equals <T1, T2> (T1 x, T2 y, IEqualityComparer<T1> xComparer, IEqualityComparer<T2> yComparer)
        {
            if (( x == null ) && ( y == null ))
            {
                return true;
            }

            if (( x == null ) || ( y == null ))
            {
                return false;
            }

            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            lock (x.GetSyncRoot(true))
            {
                lock (y.GetSyncRoot(true))
                {
                    if (xComparer != null)
                    {
                        if (y is T1)
                        {
                            object temp = y;
                            return xComparer.Equals(x, (T1)temp);
                        }
                    }

                    if (yComparer != null)
                    {
                        if (x is T2)
                        {
                            object temp = x;
                            return yComparer.Equals(y, (T2)temp);
                        }
                    }

                    if (x is IEquatable<T2>)
                    {
                        return ( (IEquatable<T2>)x ).Equals(y);
                    }

                    if (y is IEquatable<T1>)
                    {
                        return ( (IEquatable<T1>)y ).Equals(x);
                    }

                    if (y is T1)
                    {
                        object temp = y;
                        return EqualityComparer<T1>.Default.Equals(x, (T1)temp);
                    }

                    if (x is T2)
                    {
                        object temp = x;
                        return EqualityComparer<T2>.Default.Equals(y, (T2)temp);
                    }

                    return x.Equals(y);
                }
            }
        }

        public static EnhancedComparer<T> GetComparer <T> ()
        {
            return new EnhancedComparer<T>();
        }

        public static EnhancedComparer<T> GetComparer <T> (OrderCompare<T> comparer)
        {
            return new EnhancedComparer<T>(comparer);
        }

        public static EnhancedEqualityComparer<T> GetEqualityComparer <T> ()
        {
            return new EnhancedEqualityComparer<T>();
        }

        public static EnhancedEqualityComparer<T> GetEqualityComparer <T> (EqualityCompare<T> comparer)
        {
            return new EnhancedEqualityComparer<T>(comparer);
        }

        public static EnhancedEqualityComparer<T> GetEqualityComparer <T> (HashCode<T> hashCoder)
        {
            return new EnhancedEqualityComparer<T>(hashCoder);
        }

        public static EnhancedEqualityComparer<T> GetEqualityComparer <T> (EqualityCompare<T> comparer, HashCode<T> hashCoder)
        {
            return new EnhancedEqualityComparer<T>(comparer, hashCoder);
        }

        public static int GetHashCode <T> (T obj)
        {
            if (obj == null)
            {
                return 0;
            }

            lock (obj.GetSyncRoot(true))
            {
                return obj.GetHashCode();
            }
        }

        #endregion
    }
}
