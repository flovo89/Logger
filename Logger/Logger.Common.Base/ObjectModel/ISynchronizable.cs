namespace Logger.Common.ObjectModel
{
    public interface ISynchronizable
    {
        bool IsSynchronized { get; }

        object SyncRoot { get; }
    }
}
