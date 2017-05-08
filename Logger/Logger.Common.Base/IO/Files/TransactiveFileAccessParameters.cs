using System;
using System.Runtime.Serialization;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Files
{
    [Serializable]
    public sealed class TransactiveFileAccessParameters : ISerializable,
            ICloneable<TransactiveFileAccessParameters>,
            ICloneable
    {
        #region Constants

        public static readonly string DefaultBackupFileSuffix = ".bak";

        public static readonly string DefaultTempFileSuffix = ".tmp";

        #endregion




        #region Static Methods

        public static bool Equals (TransactiveFileAccessParameters x, TransactiveFileAccessParameters y)
        {
            return ObjectComparer.Equals<TransactiveFileAccessParameters>(x, y);
        }

        #endregion




        #region Instance Constructor/Destructor

        public TransactiveFileAccessParameters ()
                : this(null, null)
        {
        }

        public TransactiveFileAccessParameters (string tempFileSuffix, string backupFileSuffix)
        {
            tempFileSuffix = tempFileSuffix ?? TransactiveFileAccessParameters.DefaultTempFileSuffix;
            tempFileSuffix = tempFileSuffix.IsEmpty() ? TransactiveFileAccessParameters.DefaultTempFileSuffix : tempFileSuffix;

            backupFileSuffix = backupFileSuffix ?? TransactiveFileAccessParameters.DefaultBackupFileSuffix;
            backupFileSuffix = backupFileSuffix.IsEmpty() ? TransactiveFileAccessParameters.DefaultBackupFileSuffix : backupFileSuffix;

            if (!FilePath.IsFileExtension(tempFileSuffix, false))
            {
                throw new InvalidPathArgumentException(nameof(tempFileSuffix));
            }

            if (!FilePath.IsFileExtension(backupFileSuffix, false))
            {
                throw new InvalidPathArgumentException(nameof(backupFileSuffix));
            }

            this.TempFileSuffix = tempFileSuffix;
            this.BackupFileSuffix = backupFileSuffix;
        }

        private TransactiveFileAccessParameters (SerializationInfo info, StreamingContext context)
                : this()
        {
            this.TempFileSuffix = info.GetString(nameof(this.TempFileSuffix));
            this.BackupFileSuffix = info.GetString(nameof(this.BackupFileSuffix));
        }

        #endregion




        #region Instance Properties/Indexer

        public string BackupFileSuffix { get; }

        public string TempFileSuffix { get; }

        #endregion




        #region Instance Methods

        public FilePath GetBackupFile (FilePath originalFile)
        {
            if (originalFile == null)
            {
                throw new ArgumentNullException(nameof(originalFile));
            }

            return new FilePath(originalFile.Path + this.BackupFileSuffix);
        }

        public FilePath GetTempFile (FilePath originalFile)
        {
            if (originalFile == null)
            {
                throw new ArgumentNullException(nameof(originalFile));
            }

            return new FilePath(originalFile.Path + this.TempFileSuffix);
        }

        #endregion




        #region Interface: ICloneable<TransactiveFileAccessParameters>

        public TransactiveFileAccessParameters Clone ()
        {
            return this.CloneDeep();
        }

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        #endregion




        #region Interface: ISerializable

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.TempFileSuffix), this.TempFileSuffix);
            info.AddValue(nameof(this.BackupFileSuffix), this.BackupFileSuffix);
        }

        #endregion
    }
}
