using System;
using System.Collections.Generic;
using System.Text;

using Claymount.Console.Common.IO.Documents.Ini;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    public sealed class IniSection : IComparable,
            IComparable<IniSection>,
            ICloneable<IniSection>
    {
        #region Static Methods

        public static int Compare (IniSection x, IniSection y)
        {
            return ObjectComparer.Compare<IniSection>(x, y);
        }

        public static bool operator > (IniSection x, IniSection y)
        {
            return IniSection.Compare(x, y) > 0;
        }

        public static bool operator >= (IniSection x, IniSection y)
        {
            return IniSection.Compare(x, y) >= 0;
        }

        public static bool operator < (IniSection x, IniSection y)
        {
            return IniSection.Compare(x, y) < 0;
        }

        public static bool operator <= (IniSection x, IniSection y)
        {
            return IniSection.Compare(x, y) <= 0;
        }

        internal static bool TryParse (string str, char sectionHeaderStartChar, char sectionHeaderEndChar, char escapeChar, out IniSection section)
        {
            if (str == null)
            {
                section = null;
                return false;
            }

            string sectionHeaderStartString = new string(sectionHeaderStartChar, 1);
            string sectionHeaderEndString = new string(sectionHeaderEndChar, 1);

            string trimmed = str.Trim();

            if (trimmed.StartsWith(sectionHeaderStartString, StringComparison.InvariantCultureIgnoreCase) && trimmed.EndsWith(sectionHeaderEndString, StringComparison.InvariantCultureIgnoreCase) && ( trimmed.Length >= 2 ))
            {
                string name = trimmed.Substring(1, trimmed.Length - 2).Trim();
                name = IniSection.Decode(name, escapeChar);

                section = new IniSection(name);
                return true;
            }

            section = null;
            return false;
        }

        private static string Decode (string name, char escapeChar)
        {
            name = name.ReplaceSingleStart(escapeChar + "n", "\n", StringComparison.InvariantCultureIgnoreCase);

            name = name.Replace("\n", Environment.NewLine);

            name = name.HalveOccurrence(escapeChar);

            return name;
        }

        #endregion




        #region Instance Constructor/Destructor

        public IniSection (string name)
        {
            this.Name = name;
            this.Elements = new IniSectionElementCollection();
        }

        public IniSection (string name, IEnumerable<IniSectionElement> elements)
                : this(name)
        {
            if (elements != null)
            {
                this.Elements.AddRange(elements);
            }
        }

        #endregion




        #region Instance Fields

        private string _name;

        #endregion




        #region Instance Properties/Indexer

        public IniSectionElementCollection Elements { get; }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (value.IsEmpty())
                {
                    value = string.Empty;
                }

                this._name = value;
            }
        }

        #endregion




        #region Instance Methods

        public bool IsEmpty ()
        {
            return this.IsEmpty(false);
        }

        public bool IsEmpty (bool countTextElements)
        {
            int count = 0;

            foreach (IniSectionElement element in this.Elements)
            {
                if (element is IniValueElement)
                {
                    count++;
                }

                if (countTextElements && element is IniTextElement)
                {
                    count++;
                }
            }

            return count == 0;
        }

        internal string Encode (char sectionHeaderStartChar, char sectionHeaderEndChar, char escapeChar)
        {
            string name = this.Name.Trim();
            name = name.DoubleOccurrence(escapeChar);
            name = name.Replace("\r", string.Empty);
            name = name.Replace("\n", escapeChar + "n");

            StringBuilder sb = new StringBuilder();

            sb.Append(sectionHeaderStartChar);
            sb.Append(name);
            sb.Append(sectionHeaderEndChar);

            return sb.ToString();
        }

        #endregion




        #region Overrides

        public override string ToString ()
        {
            return base.ToString() + " (" + this.Name + ")";
        }

        #endregion




        #region Interface: ICloneable<IniSection>

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        public IniSection Clone ()
        {
            IniSection clone = new IniSection(this.Name);
            foreach (IniSectionElement element in this.Elements)
            {
                clone.Elements.Add(element.Clone());
            }
            return clone;
        }

        #endregion




        #region Interface: IComparable

        int IComparable.CompareTo (object obj)
        {
            return this.CompareTo(obj as IniSection);
        }

        #endregion




        #region Interface: IComparable<IniSection>

        public int CompareTo (IniSection other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
