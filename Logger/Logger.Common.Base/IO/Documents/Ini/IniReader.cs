using System;
using System.IO;
using System.Text;




namespace Logger.Common.IO.Documents.Ini
{
    public sealed class IniReader : IDisposable
    {
        #region Instance Constructor/Destructor

        public IniReader (TextReader reader, bool doNotOwnStream)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.IsClosing = false;

            this.Reader = reader;

            this.CurrentSection = null;
            this.CurrentElement = null;

            this.Settings = new IniSettings();

            this.DoNotOwnStream = doNotOwnStream;
        }

        public IniReader (Stream stream, bool doNotOwnStream)
                : this(new StreamReader(stream), doNotOwnStream)
        {
        }

        public IniReader (Stream stream, bool doNotOwnStream, Encoding encoding)
                : this(new StreamReader(stream, encoding), doNotOwnStream)
        {
        }

        public IniReader (Stream stream, bool doNotOwnStream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks)
                : this(new StreamReader(stream, encoding, detectByteOrderFromTextEncodingMarks), doNotOwnStream)
        {
        }

        public IniReader (Stream stream, bool doNotOwnStream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks, int bufferSize)
                : this(new StreamReader(stream, encoding, detectByteOrderFromTextEncodingMarks, bufferSize), doNotOwnStream)
        {
        }

        public IniReader (TextReader reader)
                : this(reader, false)
        {
        }

        public IniReader (Stream stream)
                : this(new StreamReader(stream), false)
        {
        }

        public IniReader (Stream stream, Encoding encoding)
                : this(new StreamReader(stream, encoding), false)
        {
        }

        public IniReader (Stream stream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks)
                : this(new StreamReader(stream, encoding, detectByteOrderFromTextEncodingMarks), false)
        {
        }

        public IniReader (Stream stream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks, int bufferSize)
                : this(new StreamReader(stream, encoding, detectByteOrderFromTextEncodingMarks, bufferSize), false)
        {
        }

        ~IniReader ()
        {
            this.Close();
        }

        #endregion




        #region Instance Fields

        private IniSettings _settings;

        #endregion




        #region Instance Properties/Indexer

        public IniSectionElement CurrentElement { get; private set; }

        public IniSection CurrentSection { get; private set; }

        public bool IsClosed
        {
            get
            {
                return this.Reader == null;
            }
        }

        public bool IsClosing { get; private set; }

        public IniSettings Settings
        {
            get
            {
                return this._settings;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this._settings = value;
            }
        }

        private bool DoNotOwnStream { get; }

        private TextReader Reader { get; set; }

        #endregion




        #region Instance Methods

        public void Close ()
        {
            this.IsClosing = true;

            if (this.Reader != null)
            {
                if (!this.DoNotOwnStream)
                {
                    this.Reader.Close();
                }
                this.Reader = null;
            }
        }

        public bool Read ()
        {
            this.VerifyNotClosed();

            while (true)
            {
                this.CurrentSection = null;
                this.CurrentElement = null;

                string line = this.Reader.ReadLine();

                if (line == null)
                {
                    return false;
                }

                IniValueElement value = null;
                IniTextElement text = null;
                IniSection section = null;

                if (IniSection.TryParse(line, this.Settings.SectionHeaderStartChar, this.Settings.SectionHeaderEndChar, this.Settings.EscapeChar, out section))
                {
                    this.CurrentSection = section;
                }
                else if (IniValueElement.TryParse(line, this.Settings.KeyValueDelimiterChar, this.Settings.CommentStartChar, this.Settings.EscapeChar, out value))
                {
                    this.CurrentElement = value;
                }
                else if (IniTextElement.TryParse(line, this.Settings.CommentStartChar, out text))
                {
                    this.CurrentElement = text;
                }
                else
                {
                    continue;
                }

                return true;
            }
        }

        private void VerifyNotClosed ()
        {
            if (this.IsClosed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion




        #region Interface: IDisposable

        void IDisposable.Dispose ()
        {
            this.Close();
        }

        #endregion
    }
}
