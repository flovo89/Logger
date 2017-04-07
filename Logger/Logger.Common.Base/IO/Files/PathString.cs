using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Logger.Common.Base.Collections.Generic;
using Logger.Common.Base.Comparison;
using Logger.Common.Base.ObjectModel.Exceptions;




namespace Logger.Common.Base.IO.Files
{
    [Serializable]
    public abstract class PathString : IEquatable<PathString>,
            IEquatable<string>,
            IComparable,
            IComparable<PathString>,
            IComparable<string>,
            ISerializable
    {
        #region Constants

        public static readonly string CurrentDirectorySpecifier = ".";

        public static readonly string ParentDirectorySpecifier = "..";

        public static readonly char[] WildcardSpecifiers =
        {
            '*', '?'
        };

        #endregion




        #region Static Methods

        public static int Compare (PathString x, PathString y)
        {
            return ObjectComparer.Compare<PathString>(x, y);
        }

        public static int Compare (string x, PathString y)
        {
            return ObjectComparer.Compare(x, y);
        }

        public static int Compare (PathString x, string y)
        {
            return ObjectComparer.Compare(x, y);
        }

        public static bool Equals (PathString x, PathString y)
        {
            return ObjectComparer.Equals<PathString>(x, y);
        }

        public static bool Equals (string x, PathString y)
        {
            return ObjectComparer.Equals(x, y);
        }

        public static bool Equals (PathString x, string y)
        {
            return ObjectComparer.Equals(x, y);
        }

        public static bool operator == (PathString x, PathString y)
        {
            return PathString.Equals(x, y);
        }

        public static bool operator > (PathString x, PathString y)
        {
            return PathString.Compare(x, y) > 0;
        }

        public static bool operator >= (PathString x, PathString y)
        {
            return PathString.Compare(x, y) >= 0;
        }

        public static bool operator != (PathString x, PathString y)
        {
            return !PathString.Equals(x, y);
        }

        public static bool operator < (PathString x, PathString y)
        {
            return PathString.Compare(x, y) < 0;
        }

        public static bool operator <= (PathString x, PathString y)
        {
            return PathString.Compare(x, y) <= 0;
        }

        protected static bool IsPath (string path, bool allowWildcards)
        {
            path = PathString.NormalizePath(path);
            path = PathString.UnfoldPath(path);

            path = path.Replace(new string(System.IO.Path.AltDirectorySeparatorChar, 1), string.Empty);
            path = path.Replace(new string(System.IO.Path.DirectorySeparatorChar, 1), string.Empty);
            path = path.Replace(new string(System.IO.Path.VolumeSeparatorChar, 1), string.Empty);
            path = path.Replace(PathString.ParentDirectorySpecifier, string.Empty);
            path = path.Replace(PathString.CurrentDirectorySpecifier, string.Empty);

            HashSet<char> invalidPathChars = new HashSet<char>();
            invalidPathChars.AddRange(System.IO.Path.GetInvalidPathChars());
            invalidPathChars.AddRange(System.IO.Path.GetInvalidFileNameChars());

            if (allowWildcards)
            {
                invalidPathChars.RemoveRangeAll(PathString.WildcardSpecifiers);
            }

            if (path.IndexOfAny(invalidPathChars.ToArray()) != -1)
            {
                return false;
            }

            if (path.IndexOf(System.IO.Path.PathSeparator) != -1)
            {
                return false;
            }

            return true;
        }

        protected static bool IsUncPath (string path)
        {
            return ( path.StartsWithCount(System.IO.Path.DirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase) == 2 ) || ( path.StartsWithCount(System.IO.Path.AltDirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase) == 2 );
        }

        protected static string NormalizePath (string path)
        {
            if (PathString.IsUncPath(path))
            {
                path = path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            }
            else
            {
                path = path.Trim(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            }

            path = path.Replace(new string(System.IO.Path.AltDirectorySeparatorChar, 1), new string(System.IO.Path.DirectorySeparatorChar, 1));

            return path;
        }

        protected static string SearchAndRemoveEncodingAndCultureFromDirectories (DirectoryPath directory)
        {
            Encoding encoding = null;
            CultureInfo culture = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromDirectories(directory, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromDirectories (DirectoryPath directory, out Encoding encoding)
        {
            CultureInfo culture = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromDirectories(directory, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromDirectories (DirectoryPath directory, out CultureInfo culture)
        {
            Encoding encoding = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromDirectories(directory, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromDirectories (DirectoryPath directory, out Encoding encoding, out CultureInfo culture)
        {
            encoding = null;
            culture = null;

            string[] pieces = directory.Path.Split(new[]
            {
                System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar
            }, StringSplitOptions.None);

            List<string> newDirectoryPieces = new List<string>();

            foreach (string piece in pieces)
            {
                bool newPiece = true;

                if (piece.IsCultureInfo())
                {
                    culture = piece.ToCultureInfo();
                    newPiece = false;
                }

                if (piece.IsEncoding())
                {
                    encoding = piece.ToEncoding();
                    newPiece = false;
                }

                if (newPiece)
                {
                    newDirectoryPieces.Add(piece);
                }
            }

            string newDirectory = string.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), newDirectoryPieces.ToArray());

            return newDirectory;
        }

        protected static string SearchAndRemoveEncodingAndCultureFromFilename (FilePath file)
        {
            Encoding encoding = null;
            CultureInfo culture = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromFilename(file, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromFilename (FilePath file, out Encoding encoding)
        {
            CultureInfo culture = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromFilename(file, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromFilename (FilePath file, out CultureInfo culture)
        {
            Encoding encoding = null;

            return PathString.SearchAndRemoveEncodingAndCultureFromFilename(file, out encoding, out culture);
        }

        protected static string SearchAndRemoveEncodingAndCultureFromFilename (FilePath file, out Encoding encoding, out CultureInfo culture)
        {
            encoding = null;
            culture = null;

            string fileName = file.FileName;

            string[] pieces = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);

            List<string> newFileNamePieces = new List<string>();

            foreach (string piece in pieces)
            {
                bool newPiece = true;

                if (piece.IsCultureInfo())
                {
                    culture = piece.ToCultureInfo();
                    newPiece = false;
                }

                if (piece.IsEncoding())
                {
                    encoding = piece.ToEncoding();
                    newPiece = false;
                }

                if (newPiece)
                {
                    newFileNamePieces.Add(piece);
                }
            }

            string newFileName = string.Join(".", newFileNamePieces.ToArray());

            return newFileName;
        }

        protected static string ToAbsolutePath (PathString path, PathString root)
        {
            if (root.IsRelative)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            if (path.IsAbsolute)
            {
                return path.Path;
            }

            string rootString = PathString.IsUncPath(root.Path) ? root.Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) : root.Path.Trim(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            string pathString = path.Path.Trim(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            string absolutePath = rootString + System.IO.Path.DirectorySeparatorChar + pathString;

            return absolutePath;
        }

        protected static string ToRelativePath (PathString path, PathString root)
        {
            return PathString.ToRelativePath(path, root, false);
        }

        protected static string ToRelativePath (PathString path, PathString root, bool includeCurrentSpecifier)
        {
            if (root.IsRelative)
            {
                throw new PathNotAbsoluteArgumentException(nameof(root));
            }

            if (path.IsRelative)
            {
                return includeCurrentSpecifier ? System.IO.Path.Combine(PathString.CurrentDirectorySpecifier, path.Path) : path.Path;
            }

            string rootString = root.Path.Trim(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            string pathString = path.Path.Trim(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            string[] rootPieces = rootString.Split(new[]
            {
                System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar
            }, StringSplitOptions.None);

            string[] pathPieces = pathString.Split(new[]
            {
                System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar
            }, StringSplitOptions.None);

            List<string> leadingMatch = new List<string>();

            for (int i1 = 0; i1 < Math.Min(rootPieces.Length, pathPieces.Length); i1++)
            {
                if (string.Equals(rootPieces[i1], pathPieces[i1], StringComparison.InvariantCultureIgnoreCase))
                {
                    leadingMatch.Add(rootPieces[i1]);
                }
                else
                {
                    break;
                }
            }

            List<string> trailingMatch = new List<string>();

            for (int i1 = 0; i1 < Math.Min(rootPieces.Length, pathPieces.Length); i1++)
            {
                int rootIndex = rootPieces.Length - ( 1 + i1 );
                int pathIndex = pathPieces.Length - ( 1 + i1 );

                if (string.Equals(rootPieces[rootIndex], pathPieces[pathIndex], StringComparison.InvariantCultureIgnoreCase))
                {
                    trailingMatch.Add(rootPieces[rootIndex]);
                }
                else
                {
                    break;
                }
            }

            if (leadingMatch.Count == 0)
            {
                return path.Path;
            }

            /*if (leadingMatch.Count == pathPieces.Length)
			{
				return (includeCurrentSpecifier ? CurrentDirectorySpecifier : string.Empty);
			}*/

            List<string> upLinks = new List<string>();

            for (int i1 = 0; i1 < rootPieces.Length - leadingMatch.Count; i1++)
            {
                upLinks.Add(PathString.ParentDirectorySpecifier);
            }

            List<string> downLinks = new List<string>();

            for (int i1 = leadingMatch.Count; i1 < pathPieces.Length; i1++)
            {
                downLinks.Add(pathPieces[i1]);
            }

            string upLinkPath = upLinks.Join(System.IO.Path.DirectorySeparatorChar);
            string downLinkPath = downLinks.Join(System.IO.Path.DirectorySeparatorChar);

            if (rootPieces.Length == leadingMatch.Count)
            {
                if (includeCurrentSpecifier)
                {
                    downLinkPath = System.IO.Path.Combine(PathString.CurrentDirectorySpecifier, downLinkPath);
                }

                return downLinkPath;
            }

            if (pathPieces.Length == leadingMatch.Count)
            {
                if (includeCurrentSpecifier)
                {
                    upLinkPath = System.IO.Path.Combine(PathString.CurrentDirectorySpecifier, upLinkPath);
                }

                return upLinkPath;
            }

            string relativePath = System.IO.Path.Combine(upLinkPath, downLinkPath);

            if (includeCurrentSpecifier)
            {
                relativePath = System.IO.Path.Combine(PathString.CurrentDirectorySpecifier, relativePath);
            }

            return relativePath;
        }

        protected static string UnfoldPath (string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            return path;
        }

        #endregion




        #region Instance Constructor/Destructor

        internal PathString (string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = PathString.UnfoldPath(path);
            path = PathString.NormalizePath(path);

            this.Path = path;
        }

        protected PathString (SerializationInfo info, StreamingContext context)
        {
            this.Path = info.GetString(nameof(this.Path));
        }

        #endregion




        #region Instance Properties/Indexer

        public bool IsAbsolute
        {
            get
            {
                if (PathString.IsUncPath(this.Path))
                {
                    return true;
                }

                return System.IO.Path.IsPathRooted(this.Path);
            }
        }

        public bool IsRelative
        {
            get
            {
                return !this.IsAbsolute;
            }
        }

        public bool IsRoot
        {
            get
            {
                return this.Equals(this.Root);
            }
        }

        public bool IsUnc
        {
            get
            {
                return PathString.IsUncPath(this.Path);
            }
        }

        public bool IsWildcarded
        {
            get
            {
                return this.Path.IndexOfAny(PathString.WildcardSpecifiers) != -1;
            }
        }

        public string Path { get; }

        public DirectoryPath Root
        {
            get
            {
                if (!this.IsAbsolute)
                {
                    return null;
                }

                return new DirectoryPath(System.IO.Path.GetPathRoot(this.Path));
            }
        }

        #endregion




        #region Instance Methods

        public long GetAvailableFreeSpace ()
        {
            DriveInfo rootDrive = this.GetRootDrive();

            if (rootDrive == null)
            {
                return 0;
            }

            return rootDrive.AvailableFreeSpace;
        }

        public DriveInfo GetRootDrive ()
        {
            PathString root = this.Root;

            if (root == null)
            {
                return null;
            }

            return new DriveInfo(root.Path);
        }

        public long GetTotalFreeSpace ()
        {
            DriveInfo rootDrive = this.GetRootDrive();

            if (rootDrive == null)
            {
                return 0;
            }

            return rootDrive.TotalFreeSpace;
        }

        public long GetTotalSize ()
        {
            DriveInfo rootDrive = this.GetRootDrive();

            if (rootDrive == null)
            {
                return 0;
            }

            return rootDrive.TotalSize;
        }

        public string ToBackwardSlashes ()
        {
            string str = this.Path;
            str = str.Replace(System.IO.Path.AltDirectorySeparatorChar, '\\');
            str = str.Replace(System.IO.Path.DirectorySeparatorChar, '\\');
            return str;
        }

        public string ToForwardSlashes ()
        {
            string str = this.Path;
            str = str.Replace(System.IO.Path.AltDirectorySeparatorChar, '/');
            str = str.Replace(System.IO.Path.DirectorySeparatorChar, '/');
            return str;
        }

        #endregion




        #region Virtuals

        protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Path), this.Path);
        }

        #endregion




        #region Overrides

        public sealed override bool Equals (object obj)
        {
            if (obj is PathString)
            {
                return this.Equals(obj as PathString);
            }

            if (obj is string)
            {
                return this.Equals(obj as string);
            }

            return false;
        }

        public override int GetHashCode ()
        {
            return this.Path.ToUpperInvariant().GetHashCode();
        }

        public override string ToString ()
        {
            return this.Path;
        }

        #endregion




        #region Interface: IComparable

        int IComparable.CompareTo (object obj)
        {
            if (obj is PathString)
            {
                return this.CompareTo(obj as PathString);
            }

            if (obj is string)
            {
                return this.CompareTo(obj as string);
            }

            return 1;
        }

        #endregion




        #region Interface: IComparable<PathString>

        public int CompareTo (PathString other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.CompareTo(other.Path);
        }

        #endregion




        #region Interface: IComparable<string>

        public int CompareTo (string other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(this.Path, other, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion




        #region Interface: IEquatable<PathString>

        public bool Equals (PathString other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Equals(other.Path);
        }

        #endregion




        #region Interface: IEquatable<string>

        public bool Equals (string other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(this.Path, other, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion




        #region Interface: ISerializable

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.GetObjectData(info, context);
        }

        #endregion
    }
}
