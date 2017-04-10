namespace Logger.Core.Interfaces.Session
{
    public interface ISessionShutdownAware
    {
        void OnShutdown (int exitCode);
    }
}
