using System;
using System.IO;

using Logger.Common.Base.DataTypes;
using Logger.Common.Base.ObjectModel.Exceptions;




namespace Logger.Common.Base.IO.Streams
{
    public static class StreamExtensions
    {
        #region Constants

        public static readonly int DefaultBufferSize = 4096;

        #endregion




        #region Static Methods

        public static Stream GetReadOnlyWrapper (this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new ReadOnlyStream(stream);
        }

        public static Stream GetSynchronizedWrapper (this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new SynchronizedStream(stream);
        }

        public static Stream GetUnclosableWrapper (this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new UnclosableStream(stream);
        }

        public static Stream GetWriteOnlyWrapper (this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new WriteOnlyStream(stream);
        }

        public static int Read (this Stream source, Stream target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return StreamExtensions.Copy(target, source, -1, -1);
        }

        public static int Read (this Stream source, Stream target, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return StreamExtensions.Copy(target, source, length, -1);
        }

        public static int Read (this Stream source, Stream target, int length, int bufferSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return StreamExtensions.Copy(target, source, length, bufferSize);
        }

        public static int Read (this Stream source, byte[] data)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return source.Read(data, 0, data.Length);
        }

        public static byte[] Read (this Stream source)
        {
            return source.Read(-1, -1);
        }

        public static byte[] Read (this Stream source, int length)
        {
            return source.Read(length, -1);
        }

        public static byte[] Read (this Stream source, int length, int bufferSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                StreamExtensions.Copy(ms, source, length, bufferSize);

                return ms.ToArray();
            }
        }

        public static void TruncateAtEnd (this Stream stream, int length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            lock (stream.GetSyncRoot(true))
            {
                if (!stream.CanRead)
                {
                    throw new StreamNotReadableArgumentException(nameof(stream));
                }

                if (!stream.CanWrite)
                {
                    throw new StreamNotWriteableArgumentException(nameof(stream));
                }

                if (!stream.CanSeek)
                {
                    throw new StreamNotSeekableArgumentException(nameof(stream));
                }

                int startPosition = (int)stream.Position;

                stream.SetLength(stream.Length - length);

                if (startPosition >= stream.Length)
                {
                    startPosition = (int)stream.Length;
                }

                stream.Position = startPosition;
            }
        }

        public static void TruncateAtStart (this Stream stream, int length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            lock (stream.GetSyncRoot(true))
            {
                if (!stream.CanRead)
                {
                    throw new StreamNotReadableArgumentException(nameof(stream));
                }

                if (!stream.CanWrite)
                {
                    throw new StreamNotWriteableArgumentException(nameof(stream));
                }

                if (!stream.CanSeek)
                {
                    throw new StreamNotSeekableArgumentException(nameof(stream));
                }

                int startPosition = (int)stream.Position;

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.Position = length;

                    StreamExtensions.Copy(ms, stream, (int)stream.Length - length, -1);

                    stream.SetLength(stream.Length - length);

                    stream.Position = 0;
                    ms.Position = 0;

                    StreamExtensions.Copy(stream, ms, -1, -1);
                }

                if (startPosition >= stream.Length)
                {
                    startPosition = (int)stream.Length;
                }

                stream.Position = startPosition;
            }
        }

        public static void Write (this Stream target, byte[] data)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            target.Write(data, 0, data.Length);
        }

        public static int Write (this Stream target, Stream source)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return StreamExtensions.Copy(target, source, -1, -1);
        }

        public static int Write (this Stream target, Stream source, int length)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return StreamExtensions.Copy(target, source, length, -1);
        }

        public static int Write (this Stream target, Stream source, int length, int bufferSize)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return StreamExtensions.Copy(target, source, length, bufferSize);
        }

        private static int Copy (Stream target, Stream source, int length, int bufferSize)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!target.CanWrite)
            {
                throw new StreamNotWriteableArgumentException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.CanRead)
            {
                throw new StreamNotReadableArgumentException(nameof(source));
            }

            if (( length < 0 ) && ( length != -1 ))
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (( bufferSize <= 0 ) && ( bufferSize != -1 ))
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            if (length == 0)
            {
                return 0;
            }

            if (bufferSize == -1)
            {
                bufferSize = StreamExtensions.DefaultBufferSize;
            }

            byte[] buffer = new byte[bufferSize];

            int total = 0;

            lock (target.GetSyncRoot(true))
            {
                lock (source.GetSyncRoot(true))
                {
                    if (length == -1)
                    {
                        if (source.CanSeek)
                        {
                            length = (int)( source.Length - source.Position );
                        }
                        else
                        {
                            while (true)
                            {
                                int read = source.Read(buffer, 0, bufferSize);

                                total += read;

                                if (read == 0)
                                {
                                    return total;
                                }

                                target.Write(buffer, 0, read);
                            }
                        }
                    }

                    if (length <= 0)
                    {
                        return 0;
                    }

                    while (true)
                    {
                        int remaining = length - total;

                        int read = source.Read(buffer, 0, remaining >= bufferSize ? bufferSize : remaining);

                        total += read;

                        target.Write(buffer, 0, read);

                        if (( read == 0 ) || ( total >= length ))
                        {
                            return total;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
