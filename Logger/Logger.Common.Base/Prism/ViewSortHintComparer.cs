using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using Logger.Common.Base.DataTypes;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Common.Base.Prism
{
    public sealed class ViewSortHintComparer <T> : IComparer,
            IComparer<T>
            where T : class
    {
        #region Static Fields

        private static ViewSortHintComparer<T> _currentCulture;

        private static ViewSortHintComparer<T> _currentCultureIgnoreCase;

        private static ViewSortHintComparer<T> _invariantCulture;

        private static ViewSortHintComparer<T> _invariantCultureIgnoreCase;

        #endregion




        #region Static Properties/Indexer

        public static ViewSortHintComparer<T> CurrentCulture
        {
            get
            {
                if (ViewSortHintComparer<T>._currentCulture == null)
                {
                    ViewSortHintComparer<T>._currentCulture = new ViewSortHintComparer<T>(StringComparison.CurrentCulture);
                }

                return ViewSortHintComparer<T>._currentCulture;
            }
        }

        public static ViewSortHintComparer<T> CurrentCultureIgnoreCase
        {
            get
            {
                if (ViewSortHintComparer<T>._currentCultureIgnoreCase == null)
                {
                    ViewSortHintComparer<T>._currentCultureIgnoreCase = new ViewSortHintComparer<T>(StringComparison.CurrentCultureIgnoreCase);
                }

                return ViewSortHintComparer<T>._currentCultureIgnoreCase;
            }
        }

        public static ViewSortHintComparer<T> InvariantCulture
        {
            get
            {
                if (ViewSortHintComparer<T>._invariantCulture == null)
                {
                    ViewSortHintComparer<T>._invariantCulture = new ViewSortHintComparer<T>(StringComparison.InvariantCulture);
                }

                return ViewSortHintComparer<T>._invariantCulture;
            }
        }

        public static ViewSortHintComparer<T> InvariantCultureIgnoreCase
        {
            get
            {
                if (ViewSortHintComparer<T>._invariantCultureIgnoreCase == null)
                {
                    ViewSortHintComparer<T>._invariantCultureIgnoreCase = new ViewSortHintComparer<T>(StringComparison.InvariantCultureIgnoreCase);
                }

                return ViewSortHintComparer<T>._invariantCultureIgnoreCase;
            }
        }

        #endregion




        #region Instance Constructor/Destructor

        public ViewSortHintComparer (StringComparison comparisonKind)
        {
            this.ComparisonKind = comparisonKind;

            this.CompareCulture = null;
        }

        public ViewSortHintComparer (CultureInfo compareCulture, CompareOptions compareOptions)
        {
            if (compareCulture == null)
            {
                throw new ArgumentNullException(nameof(compareCulture));
            }

            this.CompareCulture = compareCulture;
            this.CompareOptions = compareOptions;
        }

        #endregion




        #region Instance Properties/Indexer

        public CultureInfo CompareCulture { get; }

        public CompareOptions CompareOptions { get; }

        public StringComparison ComparisonKind { get; }

        #endregion




        #region Instance Methods

        private ViewSortHintAttribute GetAttribute (object obj)
        {
            return obj.GetAttribute<ViewSortHintAttribute>(true);
        }

        #endregion




        #region Interface: IComparer

        int IComparer.Compare (object x, object y)
        {
            return this.Compare(x as T, y as T);
        }

        #endregion




        #region Interface: IComparer<T>

        public int Compare (T x, T y)
        {
            if (( x == null ) && ( y == null ))
            {
                return 0;
            }

            if (y == null)
            {
                return 1;
            }

            if (x == null)
            {
                return -1;
            }

            ViewSortHintAttribute xAttribute = this.GetAttribute(x);
            ViewSortHintAttribute yAttribute = this.GetAttribute(y);

            if (( xAttribute == null ) && ( yAttribute == null ))
            {
                return 0;
            }

            if (yAttribute == null)
            {
                return 1;
            }

            if (xAttribute == null)
            {
                return -1;
            }

            if (this.CompareCulture != null)
            {
                return string.Compare(xAttribute.Hint, yAttribute.Hint, this.CompareCulture, this.CompareOptions);
            }

            return string.Compare(xAttribute.Hint, yAttribute.Hint, this.ComparisonKind);
        }

        #endregion
    }
}
