using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Logger.Common.ObjectModel;




namespace Logger.Common.DataTypes
{
    public static class ObjectExtensions
    {
        #region Static Fields

        [ThreadStatic]
        private static object _threadIndependendSyncRoot;

        #endregion




        #region Static Methods

        public static bool CanClone (this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj is ICloneable || obj.CanCloneDeep();
        }

        public static bool CanCloneDeep (this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.CanSerialize();
        }

        public static bool CanSerialize (this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.GetType().IsSerializable;
        }

        public static T Clone <T> (this T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj is ICloneable)
            {
                lock (obj.GetSyncRoot(true))
                {
                    return (T)( (ICloneable)obj ).Clone();
                }
            }

            return obj.CloneDeep();
        }

        public static T CloneDeep <T> (this T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Context = new StreamingContext(StreamingContextStates.Clone);

                    lock (obj.GetSyncRoot(true))
                    {
                        formatter.Serialize(ms, obj);
                    }

                    ms.Flush();
                    ms.Position = 0;

                    return (T)formatter.Deserialize(ms);
                }
            }
            catch (Exception exception)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ObjectExtensions_DeepCloning_ObjectNotCloneable, obj.GetType().FullName), exception);
            }
        }

        public static T GetAttribute <T> (this object obj, bool searchInherited)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            T[] attributes = obj.GetAttributes<T>(searchInherited);
            return attributes.Length == 0 ? default(T) : attributes[0];
        }

        public static T[] GetAttributes <T> (this object obj, bool searchInherited)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Type[] types = searchInherited ? new[]
            {
                obj.GetType()
            } : obj.GetType().GetTypeInheritance(true);

            List<T> attributes = new List<T>();

            foreach (Type type in types)
            {
                object[] typeAttributes = type.GetCustomAttributes(typeof(T), searchInherited);
                attributes.AddRange(typeAttributes.OfType<T>());
            }

            return attributes.ToArray();
        }

        public static object GetSyncRoot (this object obj)
        {
            return obj.GetSyncRoot(false);
        }

        public static object GetSyncRoot (this object obj, bool enforceSyncRoot)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            object syncRoot = null;

            if (obj is ISynchronizable)
            {
                syncRoot = ( (ISynchronizable)obj ).SyncRoot;
            }
            else if (obj is ICollection)
            {
                syncRoot = ( (ICollection)obj ).SyncRoot;
            }

            if (enforceSyncRoot)
            {
                if (syncRoot == null)
                {
                    if (ObjectExtensions._threadIndependendSyncRoot == null)
                    {
                        ObjectExtensions._threadIndependendSyncRoot = new object();
                    }

                    syncRoot = ObjectExtensions._threadIndependendSyncRoot;
                }
            }

            return syncRoot;
        }

        public static bool IsSynchronized (this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj is ISynchronizable)
            {
                return ( (ISynchronizable)obj ).IsSynchronized;
            }

            if (obj is ICollection)
            {
                return ( (ICollection)obj ).IsSynchronized;
            }

            return false;
        }

        #endregion
    }
}
