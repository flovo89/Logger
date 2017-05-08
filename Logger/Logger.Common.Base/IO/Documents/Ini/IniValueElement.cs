using System;
using System.Text;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Documents.Ini
{
#pragma warning disable 660
#pragma warning disable 661

    public sealed class IniValueElement : IniSectionElement,
            IComparable<IniValueElement>,
            ICloneable<IniValueElement>
    {
        #region Static Methods

        public static int Compare (IniValueElement x, IniValueElement y)
        {
            return ObjectComparer.Compare<IniValueElement>(x, y);
        }

        public static bool operator > (IniValueElement x, IniValueElement y)
        {
            return IniValueElement.Compare(x, y) > 0;
        }

        public static bool operator >= (IniValueElement x, IniValueElement y)
        {
            return IniValueElement.Compare(x, y) >= 0;
        }

        public static bool operator < (IniValueElement x, IniValueElement y)
        {
            return IniValueElement.Compare(x, y) < 0;
        }

        public static bool operator <= (IniValueElement x, IniValueElement y)
        {
            return IniValueElement.Compare(x, y) <= 0;
        }

        internal static bool TryParse (string str, char keyValueDelimiterChar, char commentStartChar, char escapeChar, out IniValueElement value)
        {
            if (str == null)
            {
                value = null;
                return false;
            }

            string keyValueDelimiterString = new string(keyValueDelimiterChar, 1);
            string commentStartString = new string(commentStartChar, 1);

            int equalSignIndex = str.IndexOf(keyValueDelimiterString, StringComparison.InvariantCultureIgnoreCase);

            if (equalSignIndex > 0)
            {
                string keyString = IniValueElement.DecodePiece(str.Substring(0, equalSignIndex), escapeChar).Trim();
                string valueString = IniValueElement.DecodePiece(equalSignIndex == str.Length - 1 ? string.Empty : str.Substring(equalSignIndex + 1), escapeChar);

                if (keyString.StartsWith(commentStartString, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = null;
                    return false;
                }

                value = new IniValueElement(keyString, valueString);
                return true;
            }

            value = null;
            return false;
        }

        private static string DecodePiece (string piece, char escapeChar)
        {
            piece = piece.ReplaceSingleStart(escapeChar + "n", "\n", StringComparison.InvariantCultureIgnoreCase);

            piece = piece.Replace("\n", Environment.NewLine);

            piece = piece.HalveOccurrence(escapeChar);

            return piece;
        }

        #endregion




        #region Instance Constructor/Destructor

        public IniValueElement (string key)
                : this(key, null)
        {
        }

        public IniValueElement (string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.Key = key;
            this.Value = value;
        }

        #endregion




        #region Instance Fields

        private string _key;

        private string _value;

        #endregion




        #region Instance Properties/Indexer

        public string Key
        {
            get
            {
                return this._key;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.IsEmpty())
                {
                    throw new EmptyStringArgumentException(nameof(value));
                }

                this._key = value;
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                this._value = value;
            }
        }

        #endregion




        #region Instance Methods

        private string EncodePiece (string piece, char escapeChar)
        {
            piece = piece.DoubleOccurrence(escapeChar);
            piece = piece.Replace("\r", string.Empty);
            piece = piece.Replace("\n", escapeChar + "n");

            return piece;
        }

        #endregion




        #region Overrides

        protected override IniSectionElement CloneInternal ()
        {
            return new IniValueElement(this.Key, this.Value);
        }

        protected override IniSectionElement CloneShallowInternal ()
        {
            return new IniValueElement(this.Key, this.Value);
        }

        protected override int CompareInternal (IniSectionElement other)
        {
            if (other == null)
            {
                return 1;
            }

            if (!( other is IniValueElement ))
            {
                return 0;
            }

            IniValueElement other2 = (IniValueElement)other;

            int keyComparison = string.Compare(this.Key, other2.Key, StringComparison.InvariantCultureIgnoreCase);

            if (keyComparison != 0)
            {
                return keyComparison;
            }

            return string.Compare(other2.Value, other2.Value, StringComparison.InvariantCulture);
        }

        protected override string EncodeInternal (char keyValueDelimiterChar, char commentStartChar, char escapeChar, string newLine)
        {
            if (newLine == null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(this.EncodePiece(this.Key.Trim(), escapeChar));
            sb.Append(keyValueDelimiterChar);
            sb.Append(this.EncodePiece(this.Value, escapeChar));

            return sb.ToString();
        }

        protected override string ToStringInternal ()
        {
            return this.GetType().Name + " (" + this.Key + " = " + this.Value + ")";
        }

        #endregion




        #region Interface: ICloneable<IniValueElement>

        public new IniValueElement Clone ()
        {
            return (IniValueElement)base.Clone();
        }

        #endregion




        #region Interface: IComparable<IniValueElement>

        public int CompareTo (IniValueElement other)
        {
            return this.CompareTo((IniSectionElement)other);
        }

        #endregion
    }

#pragma warning restore 660
#pragma warning restore 661
}
