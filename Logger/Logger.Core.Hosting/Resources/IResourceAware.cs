namespace Logger.Core.Resources
{
    public interface IResourceAware
    {
        void OnResourceChanged (string key, object value);
    }
}
