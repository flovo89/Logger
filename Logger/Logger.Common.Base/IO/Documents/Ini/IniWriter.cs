using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Logger.Common.DataTypes;
using Logger.Common.IO.Documents.Ini;




namespace Claymount.Console.Common.IO.Documents.Ini
{
    public sealed class IniWriter : IDisposable
    {
        #region Instance Constructor/Destructor

        public IniWriter (TextWriter writer, bool doNotOwnStream)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.IsClosing = false;

            this.Writer = writer;

            this.WritingStarted = false;

            this.Settings = new IniSettings();

            this.DoNotOwnStream = doNotOwnStream;
        }

        public IniWriter (Stream stream, bool doNotOwnStream)
                : this(new StreamWriter(stream), doNotOwnStream)
        {
        }

        public IniWriter (Stream stream, bool doNotOwnStream, Encoding encoding)
                : this(new StreamWriter(stream, encoding), doNotOwnStream)
        {
        }

        public IniWriter (Stream stream, bool doNotOwnStream, Encoding encoding, int bufferSize)
                : this(new StreamWriter(stream, encoding, bufferSize), doNotOwnStream)
        {
        }

        public IniWriter (TextWriter writer)
                : this(writer, false)
        {
        }

        public IniWriter (Stream stream)
                : this(new StreamWriter(stream), false)
        {
        }

        public IniWriter (Stream stream, Encoding encoding)
                : this(new StreamWriter(stream, encoding), false)
        {
        }

        public IniWriter (Stream stream, Encoding encoding, int bufferSize)
                : this(new StreamWriter(stream, encoding, bufferSize), false)
        {
        }

        ~IniWriter ()
        {
            this.Close();
        }

        #endregion




        #region Instance Fields

        private IniSettings _settings;

        #endregion




        #region Instance Properties/Indexer

        public bool IsClosed
        {
            get
            {
                return this.Writer == null;
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

        private TextWriter Writer { get; set; }

        private bool WritingStarted { get; set; }

        #endregion




        #region Instance Methods

        public void Close ()
        {
            this.IsClosing = true;

            if (this.Writer != null)
            {
                if (!this.DoNotOwnStream)
                {
                    this.Writer.Close();
                }
                this.Writer = null;
            }
        }

        public void Flush ()
        {
            this.VerifyNotClosed();

            this.Writer.Flush();
        }

        public void Write (IniSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            this.VerifyNotClosed();

            if (!section.Name.IsEmpty())
            {
                if (this.WritingStarted)
                {
                    this.Writer.WriteLine();
                    this.Writer.WriteLine();
                }

                string encoded = section.Encode(this.Settings.SectionHeaderStartChar, this.Settings.SectionHeaderEndChar, this.Settings.EscapeChar);
                this.Writer.Write(encoded);

                this.WritingStarted = true;
            }

            this.Write(section.Elements);
        }

        public void Write (IniSectionElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            this.VerifyNotClosed();

            if (this.WritingStarted)
            {
                this.Writer.WriteLine();
            }

            string encoded = element.Encode(this.Settings.KeyValueDelimiterChar, this.Settings.CommentStartChar, this.Settings.EscapeChar, this.Writer.NewLine);
            this.Writer.Write(encoded);

            this.WritingStarted = true;
        }

        public void Write (IEnumerable<IniSection> sections)
        {
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            this.VerifyNotClosed();

            foreach (IniSection section in sections)
            {
                this.Write(section);
            }
        }

        public void Write (IEnumerable<IniSectionElement> elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            this.VerifyNotClosed();

            foreach (IniSectionElement element in elements)
            {
                this.Write(element);
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
