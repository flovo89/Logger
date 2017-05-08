using System;
using System.Runtime.Serialization;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    [Serializable]
    public sealed class IniSettings : ISerializable,
            ICloneable<IniSettings>,
            ICloneable
    {
        #region Constants

        public static readonly char DefaultCommentStartChar = ';';

        public static readonly char DefaultEscapeChar = '|';

        public static readonly char DefaultKeyValueDelimiterChar = '=';

        public static readonly char DefaultSectionHeaderEndChar = ']';

        public static readonly char DefaultSectionHeaderStartChar = '[';

        #endregion




        #region Static Methods

        public static bool Equals (IniSettings x, IniSettings y)
        {
            return ObjectComparer.Equals<IniSettings>(x, y);
        }

        #endregion




        #region Instance Constructor/Destructor

        public IniSettings ()
        {
            this.CommentStartChar = IniSettings.DefaultCommentStartChar;
            this.EscapeChar = IniSettings.DefaultEscapeChar;
            this.KeyValueDelimiterChar = IniSettings.DefaultKeyValueDelimiterChar;
            this.SectionHeaderEndChar = IniSettings.DefaultSectionHeaderEndChar;
            this.SectionHeaderStartChar = IniSettings.DefaultSectionHeaderStartChar;
        }

        private IniSettings (SerializationInfo info, StreamingContext context)
                : this()
        {
            this.CommentStartChar = info.GetChar(nameof(this.CommentStartChar));
            this.EscapeChar = info.GetChar(nameof(this.EscapeChar));
            this.KeyValueDelimiterChar = info.GetChar(nameof(this.KeyValueDelimiterChar));
            this.SectionHeaderEndChar = info.GetChar(nameof(this.SectionHeaderEndChar));
            this.SectionHeaderStartChar = info.GetChar(nameof(this.SectionHeaderStartChar));
        }

        #endregion




        #region Instance Properties/Indexer

        public char CommentStartChar { get; set; }

        public char EscapeChar { get; set; }

        public char KeyValueDelimiterChar { get; set; }

        public char SectionHeaderEndChar { get; set; }

        public char SectionHeaderStartChar { get; set; }

        #endregion




        #region Interface: ICloneable<IniSettings>

        public IniSettings Clone ()
        {
            return this.CloneDeep();
        }

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        #endregion




        #region Interface: ISerializable

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.CommentStartChar), this.CommentStartChar);
            info.AddValue(nameof(this.EscapeChar), this.EscapeChar);
            info.AddValue(nameof(this.KeyValueDelimiterChar), this.KeyValueDelimiterChar);
            info.AddValue(nameof(this.SectionHeaderEndChar), this.SectionHeaderEndChar);
            info.AddValue(nameof(this.SectionHeaderStartChar), this.SectionHeaderStartChar);
        }

        #endregion
    }
}
