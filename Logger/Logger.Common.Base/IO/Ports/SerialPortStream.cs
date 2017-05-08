using System;
using System.IO;
using System.IO.Ports;
using System.Text;

using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Ports
{
    public sealed class SerialPortStream : Stream,
            IDisposable
    {
        #region Static Methods

        private static SerialPort CreateSerialPort (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, Encoding textEncoding, string newLine, bool? discardNull, byte? parityReplace, int? readBufferSize, int? writeBufferSize)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException(nameof(serialPort));
            }

            SerialPort port = new SerialPort(serialPort.PortName, baudRate, parity, dataBits, stopBits);

            port.Handshake = handshake;

            if (textEncoding != null)
            {
                port.Encoding = textEncoding;
            }

            if (newLine != null)
            {
                port.NewLine = newLine;
            }

            if (discardNull.HasValue)
            {
                port.DiscardNull = discardNull.Value;
            }

            if (parityReplace.HasValue)
            {
                port.ParityReplace = parityReplace.Value;
            }

            if (readBufferSize.HasValue)
            {
                port.ReadBufferSize = readBufferSize.Value;
            }
            if (writeBufferSize.HasValue)
            {
                port.WriteBufferSize = writeBufferSize.Value;
            }

            return port;
        }

        #endregion




        #region Instance Constructor/Destructor

        public SerialPortStream (SerialPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }

            this.IsDisposingInternal = false;

            if (!port.IsOpen)
            {
                port.Open();
            }

            port.DiscardInBuffer();
            port.DiscardOutBuffer();

            this.Port = port;
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, null, null, null, null, null, null))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, bool? discardNull, byte? parityReplace)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, null, null, discardNull, parityReplace, null, null))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, int? readBufferSize, int? writeBufferSize)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, null, null, null, null, readBufferSize, writeBufferSize))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, bool? discardNull, byte? parityReplace, int? readBufferSize, int? writeBufferSize)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, null, null, discardNull, parityReplace, readBufferSize, writeBufferSize))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, Encoding textEncoding, string newLine)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, textEncoding, newLine, null, null, null, null))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, Encoding textEncoding, string newLine, int? readBufferSize, int? writeBufferSize)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, textEncoding, newLine, null, null, readBufferSize, writeBufferSize))
        {
        }

        public SerialPortStream (SerialPortInstance serialPort, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, Encoding textEncoding, string newLine, bool? discardNull, byte? parityReplace, int? readBufferSize, int? writeBufferSize)
                : this(SerialPortStream.CreateSerialPort(serialPort, baudRate, parity, dataBits, stopBits, handshake, textEncoding, newLine, discardNull, parityReplace, readBufferSize, writeBufferSize))
        {
        }

        ~SerialPortStream ()
        {
            this.Close();
        }

        #endregion




        #region Instance Properties/Indexer

        public bool IsOpen
        {
            get
            {
                if (this.Port == null)
                {
                    return false;
                }

                return this.Port.IsOpen;
            }
        }

        public SerialPort Port { get; private set; }

        private bool IsDisposingInternal { get; set; }

        #endregion




        #region Instance Methods

        public SerialPortInstance GetSerialPortInstance ()
        {
            this.VerifyNotClosed();

            return this.Port.GetSerialPortInstance();
        }

        public int ReadAllAvailableBytes (Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new StreamNotWriteableArgumentException(nameof(stream));
            }

            this.VerifyNotClosed();

            int count = 0;
            int available = this.Port.BytesToRead;

            while (available > 0)
            {
                byte[] buffer = new byte[available];

                int read = this.Port.Read(buffer, 0, available);

                stream.Write(buffer, 0, read);

                count += read;

                available = this.Port.BytesToRead;
            }

            return count;
        }

        public byte[] ReadAllAvailableBytes ()
        {
            this.VerifyNotClosed();

            using (MemoryStream ms = new MemoryStream())
            {
                this.ReadAllAvailableBytes(ms);

                return ms.ToArray();
            }
        }

        private bool CheckNotClosed ()
        {
            if (this.Port == null)
            {
                return false;
            }

            if (!this.Port.IsOpen)
            {
                return false;
            }

            return true;
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

                return this.Port.BaseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.BaseStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.BaseStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.BaseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.BaseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.BaseStream.Position;
            }
            set
            {
                this.VerifyNotClosed();

                this.Port.BaseStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.ReadTimeout;
            }
            set
            {
                this.VerifyNotClosed();

                this.Port.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                this.VerifyNotClosed();

                return this.Port.WriteTimeout;
            }
            set
            {
                this.VerifyNotClosed();

                this.Port.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.VerifyNotClosed();

            return this.Port.BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.VerifyNotClosed();

            return this.Port.BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close ()
        {
            this.IsDisposingInternal = true;

            if (this.Port != null)
            {
                this.Port.Close();
                this.Port = null;
            }
        }

        public override int EndRead (IAsyncResult asyncResult)
        {
            this.VerifyNotClosed();

            return this.Port.BaseStream.EndRead(asyncResult);
        }

        public override void EndWrite (IAsyncResult asyncResult)
        {
            this.VerifyNotClosed();

            this.Port.BaseStream.EndWrite(asyncResult);
        }

        public override void Flush ()
        {
            this.VerifyNotClosed();

            this.Port.BaseStream.Flush();
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            this.VerifyNotClosed();

            return this.Port.Read(buffer, offset, count);
        }

        public override int ReadByte ()
        {
            this.VerifyNotClosed();

            return this.Port.ReadByte();
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            this.VerifyNotClosed();

            return this.Port.BaseStream.Seek(offset, origin);
        }

        public override void SetLength (long value)
        {
            this.VerifyNotClosed();

            this.Port.BaseStream.SetLength(value);
        }

        public override string ToString ()
        {
            return base.ToString() + " (" + this.GetSerialPortInstance() + ")";
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            this.VerifyNotClosed();

            this.Port.Write(buffer, offset, count);
        }

        public override void WriteByte (byte value)
        {
            this.VerifyNotClosed();

            this.Port.Write(new[]
            {
                value
            }, 0, 1);
        }

        protected override void Dispose (bool disposing)
        {
            this.Close();
        }

        #endregion
    }
}
