using System;
using System.IO;

using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Streams
{
    public sealed class SynchronizedStream : Stream,
            ISynchronizable
    {
        #region Instance Constructor/Destructor

        public SynchronizedStream (Stream stream)
                : this(stream, null, false, false)
        {
        }

        public SynchronizedStream (Stream stream, bool doNotOwnStream)
                : this(stream, null, false, doNotOwnStream)
        {
        }

        public SynchronizedStream (Stream stream, object syncRoot)
                : this(stream, syncRoot, false, false)
        {
        }

        public SynchronizedStream (Stream stream, object syncRoot, bool doNotOwnStream)
                : this(stream, syncRoot, false, doNotOwnStream)
        {
        }

        public SynchronizedStream (Stream stream, object syncRoot, bool noAutoFlush, bool doNotOwnStream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.SyncRoot = syncRoot ?? new object();

            this.BaseStream = stream;
            this.DoNotOwnStream = doNotOwnStream;

            this.NoAutoFlush = noAutoFlush;
        }

        ~SynchronizedStream ()
        {
            this.Close();
        }

        #endregion




        #region Instance Fields

        private bool _noAutoFlush;

        #endregion




        #region Instance Properties/Indexer

        public Stream BaseStream { get; private set; }

        public bool NoAutoFlush
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return this._noAutoFlush;
                }
            }
            set
            {
                lock (this.SyncRoot)
                {
                    this._noAutoFlush = value;
                }
            }
        }

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

        private void FlushIfNecessary ()
        {
            if (!this.NoAutoFlush)
            {
                this.Flush();
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
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.CanRead;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.CanSeek;
                }
            }
        }

        public override bool CanTimeout
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.CanTimeout;
                }
            }
        }

        public override bool CanWrite
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.CanWrite;
                }
            }
        }

        public override long Length
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.Length;
                }
            }
        }

        public override long Position
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.Position;
                }
            }
            set
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    this.BaseStream.Position = value;
                }
            }
        }

        public override int ReadTimeout
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.ReadTimeout;
                }
            }
            set
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    this.BaseStream.ReadTimeout = value;
                }
            }
        }

        public override int WriteTimeout
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    return this.BaseStream.WriteTimeout;
                }
            }
            set
            {
                lock (this.SyncRoot)
                {
                    this.VerifyNotClosed();

                    this.BaseStream.WriteTimeout = value;
                }
            }
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.BeginRead(buffer, offset, count, callback, state);
            }
        }

        public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.BeginWrite(buffer, offset, count, callback, state);
            }
        }

        public override void Close ()
        {
            lock (this.SyncRoot)
            {
                this.CloseInternal();

                base.Close();
            }
        }

        public override int EndRead (IAsyncResult asyncResult)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.EndRead(asyncResult);
            }
        }

        public override void EndWrite (IAsyncResult asyncResult)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                this.BaseStream.EndWrite(asyncResult);

                this.FlushIfNecessary();
            }
        }

        public override void Flush ()
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                this.BaseStream.Flush();
            }
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.Read(buffer, offset, count);
            }
        }

        public override int ReadByte ()
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.ReadByte();
            }
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                return this.BaseStream.Seek(offset, origin);
            }
        }

        public override void SetLength (long value)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                this.BaseStream.SetLength(value);

                this.FlushIfNecessary();
            }
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                this.BaseStream.Write(buffer, offset, count);

                this.FlushIfNecessary();
            }
        }

        public override void WriteByte (byte value)
        {
            lock (this.SyncRoot)
            {
                this.VerifyNotClosed();

                this.BaseStream.WriteByte(value);

                this.FlushIfNecessary();
            }
        }

        protected override void Dispose (bool disposing)
        {
            lock (this.SyncRoot)
            {
                this.CloseInternal();

                base.Dispose(disposing);
            }
        }

        #endregion




        #region Interface: ISynchronizable

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        #endregion
    }
}
