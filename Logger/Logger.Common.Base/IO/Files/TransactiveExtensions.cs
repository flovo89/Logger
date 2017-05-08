using System;
using System.IO;
using System.Text;

using Logger.Common.IO.Streams;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Files
{
    public static class TransactiveExtensions
    {
        #region Static Methods

        public static bool EnsureValidTransactiveState (this FilePath file)
        {
            return file.EnsureValidTransactiveState(null);
        }

        public static bool EnsureValidTransactiveState (this FilePath file, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            transactiveFileAccessParameters = transactiveFileAccessParameters ?? new TransactiveFileAccessParameters();

            bool restored = false;

            if (transactiveFileAccessParameters.GetTempFile(file).Exists)
            {
                transactiveFileAccessParameters.GetTempFile(file).Delete();
                restored = true;
            }

            if (transactiveFileAccessParameters.GetBackupFile(file).Exists)
            {
                transactiveFileAccessParameters.GetBackupFile(file).Copy(transactiveFileAccessParameters.GetTempFile(file));
                file.Delete();
                transactiveFileAccessParameters.GetTempFile(file).Move(file);
                transactiveFileAccessParameters.GetTempFile(file).Delete();
                transactiveFileAccessParameters.GetBackupFile(file).Delete();
                restored = true;
            }

            transactiveFileAccessParameters.GetTempFile(file).Delete();
            transactiveFileAccessParameters.GetBackupFile(file).Delete();

            return restored;
        }

        public static byte[] ReadBinaryTransactive (this FilePath file)
        {
            return file.ReadBinaryTransactive(null);
        }

        public static byte[] ReadBinaryTransactive (this FilePath file, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Exists)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                file.ReadTransactive(ms, transactiveFileAccessParameters);

                ms.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public static string ReadTextTransactive (this FilePath file)
        {
            return file.ReadTextTransactive(null, null);
        }

        public static string ReadTextTransactive (this FilePath file, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            return file.ReadTextTransactive(null, transactiveFileAccessParameters);
        }

        public static string ReadTextTransactive (this FilePath file, Encoding encoding)
        {
            return file.ReadTextTransactive(encoding, null);
        }

        public static string ReadTextTransactive (this FilePath file, Encoding encoding, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Exists)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                file.ReadTransactive(ms, transactiveFileAccessParameters);

                ms.Flush();
                ms.Position = 0;

                using (StreamReader sr = new StreamReader(ms, encoding ?? Encoding.UTF8, true))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static int? ReadTransactive (this FilePath file, Stream stream)
        {
            return file.ReadTransactive(stream, null);
        }

        public static int? ReadTransactive (this FilePath file, Stream stream, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            return file.ReadTransactiveInternal(stream, transactiveFileAccessParameters, false);
        }

        public static void WriteBinaryTransactive (this FilePath file, byte[] data)
        {
            file.WriteBinaryTransactive(data, null);
        }

        public static void WriteBinaryTransactive (this FilePath file, byte[] data, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                file.WriteTransactive(ms, transactiveFileAccessParameters);
            }
        }

        public static void WriteTextTransactive (this FilePath file, string data)
        {
            file.WriteTextTransactive(data, null, null);
        }

        public static void WriteTextTransactive (this FilePath file, string data, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            file.WriteTextTransactive(data, null, transactiveFileAccessParameters);
        }

        public static void WriteTextTransactive (this FilePath file, string data, Encoding encoding)
        {
            file.WriteTextTransactive(data, encoding, null);
        }

        public static void WriteTextTransactive (this FilePath file, string data, Encoding encoding, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, encoding ?? Encoding.UTF8))
                {
                    sw.Write(data);

                    sw.Flush();
                    ms.Flush();
                    ms.Position = 0;

                    file.WriteTransactive(ms, transactiveFileAccessParameters);
                }
            }
        }

        public static void WriteTransactive (this FilePath file, Stream stream)
        {
            file.WriteTransactive(stream, null);
        }

        public static void WriteTransactive (this FilePath file, Stream stream, TransactiveFileAccessParameters transactiveFileAccessParameters)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new StreamNotReadableArgumentException(nameof(stream));
            }

            transactiveFileAccessParameters = transactiveFileAccessParameters ?? new TransactiveFileAccessParameters();

            file.Directory.Create();

            file.EnsureValidTransactiveState(transactiveFileAccessParameters);

            using (FileStream fs = transactiveFileAccessParameters.GetTempFile(file).OpenStream(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                fs.Write(stream);
            }

            if (file.Exists)
            {
                file.Move(transactiveFileAccessParameters.GetBackupFile(file));
            }

            file.Delete();
            transactiveFileAccessParameters.GetTempFile(file).Move(file);
            transactiveFileAccessParameters.GetTempFile(file).Delete();
            transactiveFileAccessParameters.GetBackupFile(file).Delete();
        }

        private static int? ReadTransactiveInternal (this FilePath file, Stream stream, TransactiveFileAccessParameters transactiveFileAccessParameters, bool concurrentOptimized)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new StreamNotWriteableArgumentException(nameof(stream));
            }

            transactiveFileAccessParameters = transactiveFileAccessParameters ?? new TransactiveFileAccessParameters();

            file.EnsureValidTransactiveState(transactiveFileAccessParameters);

            if (!file.Exists)
            {
                return null;
            }

            using (FileStream fs = concurrentOptimized ? file.OpenStream(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete) : file.OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Write(fs);
                return (int)fs.Length;
            }
        }

        #endregion
    }
}
