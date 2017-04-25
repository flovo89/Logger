using System;
using System.IO;




namespace Logger.Common.IO.Streams
{
    public sealed class ReadOnlyStream : Stream
    {
        #region Instance Constructor/Destructor

        public ReadOnlyStream (Stream stream)
                : this(stream, false)
        {
        }

        public ReadOnlyStream (Stream stream, bool doNotOwnStream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.BaseStream = stream;
            this.DoNotOwnStream = doNotOwnStream;
        }

        ~ReadOnlyStream ()
        {
            this.Close();
        }

        #endregion




        #region Instance Properties/Indexer

        public Stream BaseStream { get; private set; }

        private bool DoNotOwnStream { get; }

        #endregion




        #region Instance Methods

        private bool CheckNotClosed ()
        {
            return this.BaseStream != null;
        }

        private void CloseInternal ()
        {
            if (this.BaseStream != null)
            {
                if (!this.DoNotOwnStream)
                {
                    this.BaseStream.Close();
                }
                this.BaseStream = null;
            }
        }

        private void VerifyNotClosed ()
        {
            if (!this.CheckNotClosed())
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion




        #region Overrides

        public override bool CanRead
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                this.VerifyNotClosed();

                return false;
            }
        }

        public override long Length
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.Position;
            }
            set
            {
                this.VerifyNotClosed();

                this.BaseStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                this.VerifyNotClosed();

                return this.BaseStream.ReadTimeout;
            }
            set
            {
                this.VerifyNotClosed();

                this.BaseStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                this.VerifyNotClosed();

                throw new StreamNotWriteableException();
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                this.VerifyNotClosed();

                throw new StreamNotWriteableException();
            }
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.VerifyNotClosed();

            return this.BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.VerifyNotClosed();

            throw new StreamNotWriteableException();
        }

        public override void Close ()
        {
            this.CloseInternal();

            base.Close();
        }

        public override int EndRead (IAsyncResult asyncResult)
        {
            this.VerifyNotClosed();

            return this.BaseStream.EndRead(asyncResult);
        }

        public override void EndWrite (IAsyncResult asyncResult)
        {
            this.VerifyNotClosed();

            throw new StreamNotWriteableException();
        }

        public override void Flush ()
        {
            this.VerifyNotClosed();

            this.BaseStream.Flush();
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            this.VerifyNotClosed();

            return this.BaseStream.Read(buffer, offset, count);
        }

        public override int ReadByte ()
        {
            this.VerifyNotClosed();

            return this.BaseStream.ReadByte();
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            this.VerifyNotClosed();

            return this.BaseStream.Seek(offset, origin);
        }

        public override void SetLength (long value)
        {
            this.VerifyNotClosed();

            this.BaseStream.SetLength(value);
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            this.VerifyNotClosed();

            throw new StreamNotWriteableException();
        }

        public override void WriteByte (byte value)
        {
            this.VerifyNotClosed();

            throw new StreamNotWriteableException();
        }

        protected override void Dispose (bool disposing)
        {
            this.CloseInternal();

            base.Dispose(disposing);
        }

        #endregion
    }
}
