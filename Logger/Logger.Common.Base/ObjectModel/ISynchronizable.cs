namespace Logger.Common.Base.ObjectModel
{
    public interface ISynchronizable
    {
        bool IsSynchronized { get; }

        object SyncRoot { get; }
    }
}
