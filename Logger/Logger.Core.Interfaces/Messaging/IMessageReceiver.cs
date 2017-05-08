using Logger.Core.Interfaces.Messaging;




namespace Logger.Core.Messaging
{
    public interface IMessageReceiver
    {
        void ReceiveMessage (IMessage message);
    }
}
