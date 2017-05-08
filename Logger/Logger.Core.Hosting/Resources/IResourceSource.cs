using Logger.Common.ObjectModel;




namespace Logger.Core.Resources
{
    public interface IResourceSource : ISynchronizable
    {
        IResourceSet[] GetAvailableResourceSets ();
    }
}
