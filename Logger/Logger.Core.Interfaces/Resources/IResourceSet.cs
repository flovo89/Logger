using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Logger.Common.ObjectModel;




namespace Logger.Core.Resources
{
    public interface IResourceSet : IEquatable<IResourceSet>,
            ISynchronizable
    {
        string DisplayName { get; }

        CultureInfo FormattingCulture { get; }

        bool IsDefaultSet { get; }

        bool IsSelectable { get; }

        string Key { get; }

        ICollection<string> SessionModes { get; }

        CultureInfo UiCulture { get; }

        string[] Load (IDictionary<string, object> resourceDictionary, Func<Stream, Encoding, string, object> converter);
    }
}
