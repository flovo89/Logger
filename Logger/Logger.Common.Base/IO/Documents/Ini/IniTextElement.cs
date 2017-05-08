using System;
using System.Text;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
#pragma warning disable 660
#pragma warning disable 661

    public sealed class IniTextElement : IniSectionElement,
            IComparable<IniTextElement>,
            ICloneable<IniTextElement>
    {
        #region Static Methods

        public static int Compare (IniTextElement x, IniTextElement y)
        {
            return ObjectComparer.Compare<IniTextElement>(x, y);
        }

        public static bool operator > (IniTextElement x, IniTextElement y)
        {
            return IniTextElement.Compare(x, y) > 0;
        }

        public static bool operator >= (IniTextElement x, IniTextElement y)
        {
            return IniTextElement.Compare(x, y) >= 0;
        }

        public static bool operator < (IniTextElement x, IniTextElement y)
        {
            return IniTextElement.Compare(x, y) < 0;
        }

        public static bool operator <= (IniTextElement x, IniTextElement y)
        {
            return IniTextElement.Compare(x, y) <= 0;
        }

        internal static bool TryParse (string str, char commentStartChar, out IniTextElement text)
        {
            if (str == null)
            {
                text = null;
                return false;
            }

            string textValue = IniTextElement.DecodePiece(str, commentStartChar);
            text = new IniTextElement(textValue);
            return true;
        }

        private static string DecodePiece (string piece, char commentStartChar)
        {
            string commentStartString = new string(commentStartChar, 1);

            int commentStartIndex = piece.IndexOf(commentStartString, StringComparison.InvariantCultureIgnoreCase);

            if (piece.Trim().StartsWith(commentStartString, StringComparison.InvariantCultureIgnoreCase) && ( commentStartIndex != -1 ))
            {
                piece = commentStartIndex == piece.Length - 1 ? string.Empty : piece.Substring(commentStartIndex, piece.Length - commentStartIndex);
            }

            if (piece.IsEmpty())
            {
                piece = string.Empty;
            }

            return piece;
        }

        #endregion




        #region Instance Constructor/Destructor

        public IniTextElement ()
                : this(null)
        {
        }

        public IniTextElement (string text)
        {
            this.Text = text;
        }

        #endregion




        #region Instance Fields

        private string _text;

        #endregion




        #region Instance Properties/Indexer

        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                this._text = value;
            }
        }

        #endregion




        #region Instance Methods

        private string EncodePiece (string piece, char commentStartChar)
        {
            if (piece.IsEmpty(true))
            {
                return string.Empty;
            }

            if (piece.IsEmpty())
            {
                piece = string.Empty;
            }

            return commentStartChar + piece;
        }

        #endregion




        #region Overrides

        protected override IniSectionElement CloneInternal ()
        {
            return new IniTextElement(this.Text);
        }

        protected override IniSectionElement CloneShallowInternal ()
        {
            return new IniTextElement(this.Text);
        }

        protected override int CompareInternal (IniSectionElement other)
        {
            if (other == null)
            {
                return 1;
            }

            return 0;
        }

        protected override string EncodeInternal (char keyValueDelimiterChar, char commentStartChar, char escapeChar, string newLine)
        {
            if (newLine == null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            StringBuilder sb = new StringBuilder();

            string[] lines = this.Text.SplitLineBreaks(StringSplitOptions.None);

            for (int i1 = 0; i1 < lines.Length; i1++)
            {
                sb.Append(this.EncodePiece(lines[i1], commentStartChar));

                if (i1 < lines.Length - 1)
                {
                    sb.Append(newLine);
                }
            }

            return sb.ToString();
        }

        protected override string ToStringInternal ()
        {
            return this.GetType().Name + " (" + this.Text + ")";
        }

        #endregion




        #region Interface: ICloneable<IniTextElement>

        public new IniTextElement Clone ()
        {
            return (IniTextElement)base.Clone();
        }

        #endregion




        #region Interface: IComparable<IniTextElement>

        public int CompareTo (IniTextElement other)
        {
            return this.CompareTo((IniSectionElement)other);
        }

        #endregion
    }

#pragma warning restore 660
#pragma warning restore 661
}
