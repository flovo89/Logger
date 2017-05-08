using System;
using System.Collections.Generic;
using System.Diagnostics;

using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    [DebuggerDisplay ("Count = {Count}")]
    public sealed class IniSectionElementCollection : List<IniSectionElement>,
            ICloneable<IniSectionElementCollection>
    {
        #region Instance Constructor/Destructor

        internal IniSectionElementCollection ()
        {
        }

        #endregion




        #region Interface: ICloneable<IniSectionElementCollection>

        public IniSectionElementCollection Clone ()
        {
            IniSectionElementCollection clone = new IniSectionElementCollection();
            foreach (IniSectionElement element in this)
            {
                clone.Add(element.Clone());
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
