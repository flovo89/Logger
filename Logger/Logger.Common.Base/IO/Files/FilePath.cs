using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;

using Logger.Common.Base.DataTypes;
using Logger.Common.Base.IO.Streams;
using Logger.Common.Base.ObjectModel.Exceptions;
using Logger.Common.Base.Properties;




namespace Logger.Common.Base.IO.Files
{
    [Serializable]
    public sealed class FilePath : PathString,
            IEquatable<FilePath>,
            IComparable<FilePath>
    {
        #region Constants

        private const int MoveFileDelayUntilReboot = 4;

        private const string TemporaryExtension = ".tmp";

        #endregion




        #region Static Methods

        public static FilePath GetTempFile ()
        {
            return new FilePath(System.IO.Path.GetTempFileName());
        }

        public static FilePath GetTempFile (DirectoryPath tempDirectory)
        {
            if (tempDirectory == null)
            {
                throw new ArgumentNullException(nameof(tempDirectory));
            }

            FilePath file = tempDirectory.Append(new FilePath(Guid.NewGuid().ToString("N") + FilePath.TemporaryExtension));
            file.Create();
            return file;
        }

        public static bool IsFileExtension (string str, bool allowWildcards)
        {
            if (str == null)
            {
                return false;
            }

            if (str.IsEmpty())
            {
                return false;
            }

            return PathString.IsPath(str, allowWildcards);
        }

        public static bool IsFileName (string str, bool allowWildcards)
        {
            if (str == null)
            {
                return false;
            }

            if (str.IsEmpty())
            {
                return false;
            }

            return PathString.IsPath(str, allowWildcards);
        }

        public static bool IsFilePath (string str, bool allowWildcards)
        {
            if (str == null)
            {
                return false;
            }

            if (str.IsEmpty())
            {
                return false;
            }

            return PathString.IsPath(str, allowWildcards);
        }

        public static FilePath operator / (FilePath x, DirectoryPath y)
        {
            return x.ToRelativePath(y);
        }

        public static FilePath operator * (FilePath x, DirectoryPath y)
        {
            return x.ToAbsolutePath(y);
        }

        [DllImport ("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool MoveFileEx (string lpExistingFileName, string lpNewFileName, int dwFlags);

        #endregion




        #region Instance Constructor/Destructor

        public FilePath (string path)
                : base(path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!FilePath.IsFilePath(path, true))
            {
                throw new InvalidPathArgumentException(Properties.Resources.PathNotFileArgumentException, nameof(path));
            }
        }

        private FilePath (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion




        #region Instance Properties/Indexer

        public CultureInfo AnyCulture
        {
            get
            {
                return this.Culture ?? this.Directory.Culture;
            }
        }

        public Encoding AnyEncoding
        {
            get
            {
                return this.Encoding ?? this.Directory.Encoding;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return File.GetCreationTime(this.Path);
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return this.CreationTime.ToUniversalTime();
            }
        }

        public CultureInfo Culture
        {
            get
            {
                CultureInfo culture = null;
                PathString.SearchAndRemoveEncodingAndCultureFromFilename(this, out culture);
                return culture;
            }
        }

        public DirectoryPath Directory
        {
            get
            {
                return new DirectoryPath(System.IO.Path.GetDirectoryName(this.Path));
            }
        }

        public Encoding Encoding
        {
            get
            {
                Encoding encoding = null;
                PathString.SearchAndRemoveEncodingAndCultureFromFilename(this, out encoding);
                return encoding;
            }
        }

        public bool Exists
        {
            get
            {
                return File.Exists(this.Path);
            }
        }

        public string Extension
        {
            get
            {
                if (!System.IO.Path.HasExtension(this.Path))
                {
                    return null;
                }

                return System.IO.Path.GetExtension(this.Path);
            }
        }

        public TimeSpan FileAge
        {
            get
            {
                TimeSpan writeAge = TimeSpan.Zero;
                TimeSpan createAge = TimeSpan.Zero;

                do
                {
                    DateTime now = DateTime.UtcNow;

                    writeAge = now.Subtract(this.LastWriteTimeUtc);
                    createAge = now.Subtract(this.CreationTimeUtc);
                }
                while (createAge.IsNegative() || writeAge.IsNegative());

                if (writeAge > createAge)
                {
                    return createAge;
                }

                return writeAge;
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(this.Path);
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(this.Path);
            }
        }

        public FilePath FileWithoutEncodingOrCulture
        {
            get
            {
                return new FilePath(PathString.SearchAndRemoveEncodingAndCultureFromFilename(this));
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return File.GetLastAccessTime(this.Path);
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return this.LastAccessTime.ToUniversalTime();
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return File.GetLastWriteTime(this.Path);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return this.LastWriteTime.ToUniversalTime();
            }
        }

        public long Size
        {
            get
            {
                FileInfo fileInfo = new FileInfo(this.Path);
                return fileInfo.Length;
            }
        }

        #endregion




        #region Instance Methods

        public FilePath ChangeDirectory (DirectoryPath directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            return new FilePath(System.IO.Path.Combine(directory.Path, this.FileName));
        }

        public FilePath ChangeExtension (string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (!FilePath.IsFileExtension(extension, false))
            {
                throw new InvalidPathArgumentException(nameof(extension));
            }

            return new FilePath(System.IO.Path.ChangeExtension(this.Path, extension));
        }

        public FilePath ChangeFileName (string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!FilePath.IsFilePath(fileName, false))
            {
                throw new InvalidPathArgumentException(nameof(fileName));
            }

            return new FilePath(System.IO.Path.Combine(this.Directory.Path, fileName));
        }

        public FilePath ChangeFileNameWithoutExtension (string fileNameWithoutExtension)
        {
            if (fileNameWithoutExtension == null)
            {
                throw new ArgumentNullException(nameof(fileNameWithoutExtension));
            }

            if (!FilePath.IsFilePath(fileNameWithoutExtension, false))
            {
                throw new InvalidPathArgumentException(nameof(fileNameWithoutExtension));
            }

            return new FilePath(System.IO.Path.Combine(this.Directory.Path, fileNameWithoutExtension + ( this.Extension ?? string.Empty )));
        }

        public void Copy (DirectoryPath target)
        {
            this.Copy(target, false);
        }

        public void Copy (DirectoryPath target, bool overwrite)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Create();

            File.Copy(this.Path, System.IO.Path.Combine(target.Path, this.FileName), overwrite);
        }

        public void Copy (FilePath target)
        {
            this.Copy(target, false);
        }

        public void Copy (FilePath target, bool overwrite)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Directory.Create();

            File.Copy(this.Path, target.Path, overwrite);
        }

        public bool Create ()
        {
            if (this.Exists)
            {
                return false;
            }

            this.Directory.Create();

            using (FileStream fs = this.OpenStream(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Close();
            }

            return true;
        }

        public void CreateNew ()
        {
            this.Directory.Create();

            using (FileStream fs = this.OpenStream(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Close();
            }
        }

        public void Delete ()
        {
            if (!this.Exists)
            {
                return;
            }

            File.Delete(this.Path);
        }

        public FilePath[] FindWildcardedFiles (bool recursive)
        {
            return this.Directory.FindFiles(this.FileName, recursive);
        }

        public FileSecurity GetAccessControl (AccessControlSections sections)
        {
            return File.GetAccessControl(this.Path, sections);
        }

        public void Move (DirectoryPath target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Create();

            File.Move(this.Path, System.IO.Path.Combine(target.Path, this.FileName));
        }

        public void Move (FilePath target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Directory.Create();

            File.Move(this.Path, target.Path);
        }

        public StreamReader OpenReader (Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            encoding = encoding ?? Encoding.UTF8;
            return new StreamReader(this.Path, encoding, detectEncodingFromByteOrderMarks);
        }

        public FileStream OpenStream (FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(this.Path, mode, access, share);
        }

        public StreamWriter OpenWriter (bool append, Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;
            return new StreamWriter(this.Path, append, encoding);
        }

        public byte[] ReadBinary ()
        {
            if (!this.Exists)
            {
                return null;
            }

            using (FileStream fs = this.OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return fs.Read();
            }
        }

        public int? ReadBinary (byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!this.Exists)
            {
                return null;
            }

            using (FileStream fs = this.OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return fs.Read(data, offset, count);
            }
        }

        public int? ReadBinary (Stream data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!data.CanWrite)
            {
                throw new StreamNotWriteableArgumentException(nameof(data));
            }

            if (!this.Exists)
            {
                return null;
            }

            using (FileStream fs = this.OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return fs.Read(data);
            }
        }

        public string ReadText ()
        {
            return this.ReadText(null);
        }

        public string ReadText (Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;

            if (!this.Exists)
            {
                return null;
            }

            using (FileStream fs = this.OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs, encoding, true))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public void SetAccessControl (FileSecurity accessControl)
        {
            File.SetAccessControl(this.Path, accessControl);
        }

        public FilePath ToAbsolutePath (DirectoryPath root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new FilePath(PathString.ToAbsolutePath(this, root));
        }

        public FilePath ToRelativePath (DirectoryPath root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new FilePath(PathString.ToRelativePath(this, root));
        }

        public FilePath ToRelativePath (DirectoryPath root, bool includeCurrentSpecifier)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new FilePath(PathString.ToRelativePath(this, root, includeCurrentSpecifier));
        }

        public bool TryDelete ()
        {
            try
            {
                this.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void WriteBinary (byte[] data, bool append)
        {
            this.WriteBinary(data, 0, data.Length, append);
        }

        public void WriteBinary (byte[] data, int offset, int count, bool append)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.Directory.Create();

            using (FileStream fs = this.OpenStream(append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Seek(0, append ? SeekOrigin.End : SeekOrigin.Begin);
                fs.Write(data, offset, count);
            }
        }

        public int WriteBinary (Stream data, bool append)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!data.CanRead)
            {
                throw new StreamNotReadableArgumentException(nameof(data));
            }

            this.Directory.Create();

            using (FileStream fs = this.OpenStream(append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Seek(0, append ? SeekOrigin.End : SeekOrigin.Begin);
                return data.Read(fs);
            }
        }

        public void WriteText (string data, bool append)
        {
            this.WriteText(data, append, null);
        }

        public void WriteText (string data, bool append, Encoding encoding)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.Directory.Create();

            encoding = encoding ?? Encoding.UTF8;

            using (FileStream fs = this.OpenStream(append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Seek(0, append ? SeekOrigin.End : SeekOrigin.Begin);
                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    sw.Write(data);
                }
            }
        }

        #endregion




        #region Interface: IComparable<FilePath>

        public int CompareTo (FilePath other)
        {
            return this.CompareTo(other as PathString);
        }

        #endregion




        #region Interface: IEquatable<FilePath>

        public bool Equals (FilePath other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
