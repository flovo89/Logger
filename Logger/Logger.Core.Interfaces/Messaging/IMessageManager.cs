using Logger.Common.Base.ObjectModel;




namespace Logger.Core.Interfaces.Messaging
{
    public interface IMessageManager : ISynchronizable
    {
        IMessage CreateMessage (string name);

        void PostMessage (IMessage message);
    }
}
