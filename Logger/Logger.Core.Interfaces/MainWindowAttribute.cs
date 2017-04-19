using System;

using JetBrains.Annotations;




namespace Logger.Core.Interfaces
{
    [Serializable]
    [AttributeUsage (AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired (typeof(IShell))]
    public sealed class MainWindowAttribute : Attribute
    {
        #region Instance Constructor/Destructor

        public MainWindowAttribute ()
                : this(0)
        {
        }

        public MainWindowAttribute (int priority)
        {
            this.Priority = priority;
        }

        #endregion




        #region Instance Properties/Indexer

        public int Priority { get; private set; }

        #endregion
    }
}
