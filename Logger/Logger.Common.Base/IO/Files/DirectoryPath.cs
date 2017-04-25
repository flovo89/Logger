using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;

using Logger.Common.Collections.Generic;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Files
{
    [Serializable]
    public sealed class DirectoryPath : PathString,
            IEquatable<DirectoryPath>,
            IComparable<DirectoryPath>
    {
        #region Constants

        private const int MoveFileDelayUntilReboot = 4;

        #endregion




        #region Static Methods

        public static DirectoryPath GetCurrentDirectory ()
        {
            return new DirectoryPath(Environment.CurrentDirectory);
        }

        public static DirectoryPath GetTempDirectory ()
        {
            return new DirectoryPath(System.IO.Path.GetTempPath());
        }

        public static bool IsDirectoryPath (string str, bool allowWildcards)
        {
            if (str == null)
            {
                return false;
            }

            return PathString.IsPath(str, allowWildcards);
        }

        public static DirectoryPath operator + (DirectoryPath x, DirectoryPath y)
        {
            return x.Append(y);
        }

        public static FilePath operator + (DirectoryPath x, FilePath y)
        {
            return x.Append(y);
        }

        public static DirectoryPath operator / (DirectoryPath x, DirectoryPath y)
        {
            return x.ToRelativePath(y);
        }

        public static DirectoryPath operator * (DirectoryPath x, DirectoryPath y)
        {
            return x.ToAbsolutePath(y);
        }

        [DllImport ("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool MoveFileEx (string lpExistingFileName, string lpNewFileName, int dwFlags);

        #endregion




        #region Instance Constructor/Destructor

        public DirectoryPath ()
                : this(string.Empty)
        {
        }

        public DirectoryPath (string path)
                : base(path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!DirectoryPath.IsDirectoryPath(path, true))
            {
                throw new PathNotDirectoryArgumentException(nameof(path));
            }
        }

        private DirectoryPath (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion




        #region Instance Properties/Indexer

        public DateTime CreationTime
        {
            get
            {
                return Directory.GetCreationTime(this.Path);
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
                PathString.SearchAndRemoveEncodingAndCultureFromDirectories(this, out culture);
                return culture;
            }
        }

        public string DirectoryName
        {
            get
            {
                if (this.IsRoot)
                {
                    return this.Path;
                }

                string[] pieces = this.Path.Split(new[]
                {
                    System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar
                }, StringSplitOptions.None);

                return pieces[pieces.Length - 1];
            }
        }

        public DirectoryPath DirectoryWithoutEncodingOrCulture
        {
            get
            {
                return new DirectoryPath(PathString.SearchAndRemoveEncodingAndCultureFromDirectories(this));
            }
        }

        public Encoding Encoding
        {
            get
            {
                Encoding encoding = null;
                PathString.SearchAndRemoveEncodingAndCultureFromDirectories(this, out encoding);
                return encoding;
            }
        }

        public bool Exists
        {
            get
            {
                return Directory.Exists(this.Path);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.Path.IsEmpty();
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return Directory.GetLastAccessTime(this.Path);
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
                return Directory.GetLastWriteTime(this.Path);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return this.LastWriteTime.ToUniversalTime();
            }
        }

        public DirectoryPath Parent
        {
            get
            {
                if (this.IsRoot)
                {
                    return null;
                }

                string[] pieces = this.Path.Split(new[]
                {
                    System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar
                }, StringSplitOptions.None);

                string parent = string.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), pieces, 0, pieces.Length - 1);

                return new DirectoryPath(parent);
            }
        }

        public long Size
        {
            get
            {
                FilePath[] files = this.FindAllFiles(true);
                long size = ( from x in files select x.Size ).Sum();
                return size;
            }
        }

        #endregion




        #region Instance Methods

        public DirectoryPath Append (params DirectoryPath[] directories)
        {
            directories = directories ?? new DirectoryPath[0];

            for (int i1 = 0; i1 < directories.Length; i1++)
            {
                if (directories[i1].IsAbsolute)
                {
                    throw new PathNotRelativeArgumentException(nameof(directories));
                }
            }

            string path = this.Path;

            for (int i1 = 0; i1 < directories.Length; i1++)
            {
                path = System.IO.Path.Combine(path, directories[i1].Path);
            }

            return new DirectoryPath(path);
        }

        public FilePath Append (FilePath file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.IsAbsolute)
            {
                throw new PathNotRelativeArgumentException(nameof(file));
            }

            return new FilePath(System.IO.Path.Combine(this.Path, file.Path));
        }

        public DirectoryPath ChangeDirectoryName (string directoryName)
        {
            if (directoryName == null)
            {
                throw new ArgumentNullException(nameof(directoryName));
            }

            if (!DirectoryPath.IsDirectoryPath(directoryName, false))
            {
                throw new InvalidPathArgumentException(nameof(directoryName));
            }

            if (this.IsRoot)
            {
                return new DirectoryPath(directoryName + ":");
            }

            return new DirectoryPath(System.IO.Path.Combine(this.Parent.Path, directoryName));
        }

        public DirectoryPath ChangeParent (DirectoryPath newParent)
        {
            if (newParent == null)
            {
                throw new ArgumentNullException(nameof(newParent));
            }

            return newParent.Append(new DirectoryPath(this.DirectoryName));
        }

        public bool ContainsFile (FilePath file, bool recursive)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.IsAbsolute)
            {
                throw new PathNotRelativeArgumentException(nameof(file));
            }

            FilePath fileToCheck = file.ToAbsolutePath(this);
            FilePath[] filesFound = fileToCheck.FindWildcardedFiles(recursive);
            return filesFound.Length > 0;
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

            FilePath[] files = this.FindAllFiles(false);
            DirectoryPath[] directories = this.FindAllDirectories(false);

            foreach (FilePath file in files)
            {
                file.Copy(target, overwrite);
            }

            foreach (DirectoryPath directory in directories)
            {
                directory.Copy(target.Append(new DirectoryPath(directory.DirectoryName)), overwrite);
            }
        }

        public bool Create ()
        {
            if (this.Exists)
            {
                return false;
            }

            Directory.CreateDirectory(this.Path);

            return true;
        }

        public void CreateNew ()
        {
            this.Delete();
            this.Create();
        }

        public void Delete ()
        {
            if (!this.Exists)
            {
                return;
            }

            Directory.Delete(this.Path, true);
        }

        [Obsolete ("Does only work when executed as administrator.")]
        public bool DeleteAsap ()
        {
            return this.DeleteAsap(false);
        }

        [Obsolete ("Does only work when executed as administrator.")]
        public bool DeleteAsap (bool deleteFilesSeparately)
        {
            if (!this.Exists)
            {
                return true;
            }

            if (deleteFilesSeparately)
            {
                FilePath[] files = this.FindAllFiles(true);
                foreach (FilePath file in files)
                {
                    file.Delete();
                }
            }

            if (this.TryDelete())
            {
                return true;
            }

            this.DeleteReboot(deleteFilesSeparately);
            return false;
        }

        [Obsolete ("Does only work when executed as administrator.")]
        public bool DeleteReboot ()
        {
            return this.DeleteReboot(false);
        }

        [Obsolete ("Does only work when executed as administrator.")]
        public bool DeleteReboot (bool markFilesSeparately)
        {
            if (!this.Exists)
            {
                return true;
            }

            if (markFilesSeparately)
            {
                FilePath[] files = this.FindAllFiles(true);
                foreach (FilePath file in files)
                {
                    file.Delete();
                }
            }

            DirectoryPath.MoveFileEx(this.Path, null, DirectoryPath.MoveFileDelayUntilReboot);

            return false;
        }

        public DirectoryPath[] FindAllDirectories (bool recursive)
        {
            return this.FindDirectories(null, recursive);
        }

        public FilePath[] FindAllFiles (bool recursive)
        {
            return this.FindFiles(null, recursive);
        }

        public DirectoryPath[] FindDirectories (string searchPattern, bool recursive)
        {
            searchPattern = searchPattern == null ? null : ( searchPattern.IsEmpty() ? null : searchPattern );
            searchPattern = searchPattern ?? "*";

            List<DirectoryPath> subdirectories = new List<DirectoryPath>();

            if (this.Exists)
            {
                string[] foundDirectories = recursive ? this.FindDirectoriesRecursive(this, searchPattern) : Directory.GetDirectories(this.Path, searchPattern);
                foreach (string foundDirectory in foundDirectories)
                {
                    subdirectories.Add(new DirectoryPath(foundDirectory));
                }
            }

            return subdirectories.ToArray();
        }

        public FilePath[] FindFiles (string searchPattern, bool recursive)
        {
            searchPattern = searchPattern == null ? null : ( searchPattern.IsEmpty() ? null : searchPattern );
            searchPattern = searchPattern ?? "*";

            List<FilePath> files = new List<FilePath>();

            if (this.Exists)
            {
                string[] foundFiles = recursive ? this.FindFilesRecursive(this, searchPattern) : Directory.GetFiles(this.Path, searchPattern);
                foreach (string foundFile in foundFiles)
                {
                    files.Add(new FilePath(foundFile));
                }
            }

            return files.ToArray();
        }

        public DirectoryPath[] FindWildcardedDirectories (bool recursive)
        {
            if (this.IsRoot)
            {
                return new DirectoryPath[0];
            }

            return this.Parent.FindDirectories(this.DirectoryName, recursive);
        }

        public DirectorySecurity GetAccessControl (AccessControlSections sections)
        {
            return Directory.GetAccessControl(this.Path, sections);
        }

        public void Move (DirectoryPath target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Directory.Move(this.Path, target.Path);
        }

        public void SetAccessControl (DirectorySecurity accessControl)
        {
            Directory.SetAccessControl(this.Path, accessControl);
        }

        public DirectoryPath ToAbsolutePath (DirectoryPath root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new DirectoryPath(PathString.ToAbsolutePath(this, root));
        }

        public DirectoryPath ToRelativePath (DirectoryPath root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new DirectoryPath(PathString.ToRelativePath(this, root));
        }

        public DirectoryPath ToRelativePath (DirectoryPath root, bool includeCurrentSpecifier)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!root.IsAbsolute)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            return new DirectoryPath(PathString.ToRelativePath(this, root, includeCurrentSpecifier));
        }

        public bool TryDelete ()
        {
            try
            {
                this.Delete();
            }
            catch
            {
            }

            return !this.Exists;
        }

        public bool TryDeleteAsMuchAsPossible ()
        {
            this.FindAllDirectories(true).ForEach(x => x.TryDelete());
            this.FindAllFiles(true).ForEach(x => x.TryDelete());
            return this.TryDelete();
        }

        private string[] FindDirectoriesRecursive (DirectoryPath directory, string searchPattern)
        {
            List<string> directories = new List<string>();

            directories.AddRange(Directory.GetDirectories(directory.Path, searchPattern));

            foreach (string dir in Directory.GetDirectories(directory.Path, "*"))
            {
                directories.AddRange(this.FindDirectoriesRecursive(dir.ToDirectoryPath(), searchPattern));
            }

            return directories.ToArray();
        }

        private string[] FindFilesRecursive (DirectoryPath directory, string searchPattern)
        {
            List<string> files = new List<string>();

            files.AddRange(Directory.GetFiles(directory.Path, searchPattern));

            foreach (string dir in Directory.GetDirectories(directory.Path, "*"))
            {
                files.AddRange(this.FindFilesRecursive(dir.ToDirectoryPath(), searchPattern));
            }

            return files.ToArray();
        }

        #endregion




        #region Interface: IComparable<DirectoryPath>

        public int CompareTo (DirectoryPath other)
        {
            return this.CompareTo(other as PathString);
        }

        #endregion




        #region Interface: IEquatable<DirectoryPath>

        public bool Equals (DirectoryPath other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
