using System;
using System.Collections.Generic;
using System.Diagnostics;

using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    [DebuggerDisplay ("Count = {Count}")]
    public sealed class IniSectionCollection : List<IniSection>,
            ICloneable<IniSectionCollection>
    {
        #region Instance Constructor/Destructor

        internal IniSectionCollection ()
        {
        }

        #endregion




        #region Instance Properties/Indexer

        public IniSection this [string name]
        {
            get
            {
                IniSection[] sections = this.GetSections(name);

                if (sections.Length == 0)
                {
                    throw new KeyNotFoundException();
                }

                return sections[0];
            }
        }

        #endregion




        #region Instance Methods

        public bool ContainsSection (string name)
        {
            return this.GetSections(name).Length > 0;
        }

        public IniSection[] GetSections (string name)
        {
            List<IniSection> sections = new List<IniSection>();

            foreach (IniSection section in this)
            {
                if (string.Equals(name, section.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    sections.Add(section);
                }
            }

            return sections.ToArray();
        }

        #endregion




        #region Interface: ICloneable<IniSectionCollection>

        public IniSectionCollection Clone ()
        {
            IniSectionCollection clone = new IniSectionCollection();
            foreach (IniSection section in this)
            {
                clone.Add(section.Clone());
            }
            return clone;
        }

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        #endregion
    }
}
