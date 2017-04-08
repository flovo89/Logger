using System;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Text;




namespace Logger.Common.Base.IO.Text
{
    public sealed class IndentedTextWriter : TextWriter
    {
        #region Instance Constructor/Destructor

        public IndentedTextWriter (TextWriter writer, bool doNotOwnWriter)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.BaseWriter = writer;

            this.IndentEmptyLines = false;
            this.IndentLevel = 0;
            this.IndentString = string.Empty;

            this.IndentPending = false;

            this.DoNotOwnWriter = doNotOwnWriter;
        }

        public IndentedTextWriter (TextWriter writer)
                : this(writer, false)
        {
        }

        ~IndentedTextWriter ()
        {
            this.Close();
        }

        #endregion




        #region Instance Fields

        private int _indentLevel;

        private string _indentString;

        #endregion




        #region Instance Properties/Indexer

        public TextWriter BaseWriter { get; private set; }

        public bool IndentEmptyLines { get; set; }

        public int IndentLevel
        {
            get
            {
                return this._indentLevel;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                this._indentLevel = value;
            }
        }

        public string IndentString
        {
            get
            {
                return this._indentString;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                this._indentString = value;
            }
        }

        private bool DoNotOwnWriter { get; }

        private bool IndentPending { get; set; }

        #endregion




        #region Instance Methods

        private bool CheckNotClosed ()
        {
            return this.BaseWriter != null;
        }

        private void CloseInternal ()
        {
            if (this.BaseWriter != null)
            {
                if (!this.DoNotOwnWriter)
                {
                    this.BaseWriter.Close();
                    this.BaseWriter.Dispose();
                }
                this.BaseWriter = null;
            }
        }

        private string CreateObjectString (object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is IFormattable)
            {
                return ( (IFormattable)value ).ToString(null, this.FormatProvider);
            }

            return value.ToString();
        }

        private void VerifyNotClosed ()
        {
            if (!this.CheckNotClosed())
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void WriteIndent ()
        {
            if (this.IndentPending)
            {
                for (int i1 = 0; i1 < this.IndentLevel; i1++)
                {
                    this.BaseWriter.Write(this.IndentString);
                }
            }

            this.IndentPending = false;
        }

        #endregion




        #region Overrides

        public override Encoding Encoding
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseWriter.Encoding;
            }
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseWriter.FormatProvider;
            }
        }

        public override string NewLine
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseWriter.NewLine;
            }
            set
            {
                this.VerifyNotClosed();

                this.BaseWriter.NewLine = value;
            }
        }

        public override void Close ()
        {
            this.CloseInternal();

            base.Close();
        }

        public override ObjRef CreateObjRef (Type requestedType)
        {
            this.VerifyNotClosed();

            return this.BaseWriter.CreateObjRef(requestedType);
        }

        public override void Flush ()
        {
            this.VerifyNotClosed();

            this.BaseWriter.Flush();
        }

        public override object InitializeLifetimeService ()
        {
            this.VerifyNotClosed();

            return this.BaseWriter.InitializeLifetimeService();
        }

        public override void Write (bool value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (char value)
        {
            this.VerifyNotClosed();

            this.WriteIndent();
            this.BaseWriter.Write(value);

            if (value == '\n')
            {
                this.IndentPending = true;
            }
        }

        public override void Write (char[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            foreach (char chr in buffer)
            {
                this.Write(chr);
            }
        }

        public override void Write (char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (( index < 0 ) || ( ( index >= buffer.Length ) && ( count > 0 ) ))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (( count < 0 ) || ( index + count >= buffer.Length ))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (int i1 = 0; i1 < count; i1++)
            {
                this.Write(buffer[i1 + index]);
            }
        }

        public override void Write (decimal value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (double value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (float value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (int value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (long value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (object value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.Write(this.CreateObjectString(value));
        }

        public override void Write (string format, object arg0)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg0);

            this.Write(str);
        }

        public override void Write (string format, object arg0, object arg1)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);

            this.Write(str);
        }

        public override void Write (string format, object arg0, object arg1, object arg2)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);

            this.Write(str);
        }

        public override void Write (string format, params object[] arg)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg);

            this.Write(str);
        }

        public override void Write (string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.VerifyNotClosed();

            string[] lines = value.SplitLineBreaks(StringSplitOptions.None);

            for (int i1 = 0; i1 < lines.Length; i1++)
            {
                if (i1 >= lines.Length - 1)
                {
                    this.WriteIndent();
                    this.BaseWriter.Write(lines[i1]);
                }
                else
                {
                    this.WriteLine(lines[i1]);
                }
            }
        }

        public override void Write (uint value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void Write (ulong value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.Write(value);
        }

        public override void WriteLine ()
        {
            this.VerifyNotClosed();

            if (this.IndentEmptyLines)
            {
                this.WriteIndent();
            }

            this.BaseWriter.WriteLine();
            this.IndentPending = true;
        }

        public override void WriteLine (bool value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (char value)
        {
            this.VerifyNotClosed();

            this.WriteIndent();
            this.BaseWriter.WriteLine(value);

            if (value == '\n')
            {
                this.BaseWriter.Write(value);

                this.IndentPending = true;
                this.WriteIndent();

                this.BaseWriter.WriteLine();
            }
            else
            {
                this.BaseWriter.WriteLine(value);
            }

            this.IndentPending = true;
        }

        public override void WriteLine (char[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            foreach (char chr in buffer)
            {
                this.Write(chr);
            }

            this.WriteLine();
        }

        public override void WriteLine (char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (( index < 0 ) || ( ( index >= buffer.Length ) && ( count > 0 ) ))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (( count < 0 ) || ( index + count >= buffer.Length ))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (int i1 = 0; i1 < count; i1++)
            {
                this.Write(buffer[i1 + index]);
            }

            this.WriteLine();
        }

        public override void WriteLine (decimal value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (double value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (float value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (int value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (long value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (object value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.WriteLine(this.CreateObjectString(value));
            this.IndentPending = true;
        }

        public override void WriteLine (string format, object arg0)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg0);

            this.WriteLine(str);
        }

        public override void WriteLine (string format, object arg0, object arg1)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);

            this.WriteLine(str);
        }

        public override void WriteLine (string format, object arg0, object arg1, object arg2)
        {
            this.VerifyNotClosed();

            string str = string.Format(format, arg0, arg1, arg2);

            this.WriteLine(str);
        }

        public override void WriteLine (string format, params object[] arg)
        {
            this.VerifyNotClosed();

            string str = string.Format(CultureInfo.InvariantCulture, format, arg);

            this.WriteLine(str);
        }

        public override void WriteLine (string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.VerifyNotClosed();

            string[] lines = value.SplitLineBreaks(StringSplitOptions.None);

            for (int i1 = 0; i1 < lines.Length; i1++)
            {
                if (i1 >= lines.Length - 1)
                {
                    if (this.IndentEmptyLines || ( lines[i1].Length > 0 ))
                    {
                        this.WriteIndent();
                    }

                    this.BaseWriter.WriteLine(lines[i1]);

                    this.IndentPending = true;
                }
                else
                {
                    this.WriteLine(lines[i1]);
                }
            }
        }

        public override void WriteLine (uint value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        public override void WriteLine (ulong value)
        {
            this.VerifyNotClosed();
            this.WriteIndent();
            this.BaseWriter.WriteLine(value);
            this.IndentPending = true;
        }

        protected override void Dispose (bool disposing)
        {
            this.CloseInternal();

            base.Dispose(disposing);
        }

        #endregion
    }
}
