using System;

using Logger.Common.Comparison;
using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    public abstract class IniSectionElement : IComparable,
            IComparable<IniSectionElement>,
            ICloneable<IniSectionElement>
    {
        #region Static Methods

        public static int Compare (IniSectionElement x, IniSectionElement y)
        {
            return ObjectComparer.Compare<IniSectionElement>(x, y);
        }

        public static bool operator > (IniSectionElement x, IniSectionElement y)
        {
            return IniSectionElement.Compare(x, y) > 0;
        }

        public static bool operator >= (IniSectionElement x, IniSectionElement y)
        {
            return IniSectionElement.Compare(x, y) >= 0;
        }

        public static bool operator < (IniSectionElement x, IniSectionElement y)
        {
            return IniSectionElement.Compare(x, y) < 0;
        }

        public static bool operator <= (IniSectionElement x, IniSectionElement y)
        {
            return IniSectionElement.Compare(x, y) <= 0;
        }

        #endregion




        #region Instance Constructor/Destructor

        internal IniSectionElement ()
        {
        }

        #endregion




        #region Instance Methods

        public string Encode (char keyValueDelimiterChar, char commentStartChar, char escapeChar, string newLine)
        {
            return this.EncodeInternal(keyValueDelimiterChar, commentStartChar, escapeChar, newLine);
        }

        #endregion




        #region Abstracts

        protected abstract IniSectionElement CloneInternal ();

        protected abstract IniSectionElement CloneShallowInternal ();

        protected abstract int CompareInternal (IniSectionElement other);

        protected abstract string EncodeInternal (char keyValueDelimiterChar, char commentStartChar, char escapeChar, string newLine);

        protected abstract string ToStringInternal ();

        #endregion




        #region Overrides

        public sealed override string ToString ()
        {
            return this.ToStringInternal();
        }

        #endregion




        #region Interface: ICloneable<IniSectionElement>

        public IniSectionElement Clone ()
        {
            return this.CloneInternal();
        }

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        #endregion




        #region Interface: IComparable

        int IComparable.CompareTo (object obj)
        {
            return this.CompareTo(obj as IniSectionElement);
        }

        #endregion




        #region Interface: IComparable<IniSectionElement>

        public int CompareTo (IniSectionElement other)
        {
            return this.CompareInternal(other);
        }

        #endregion
    }
}
