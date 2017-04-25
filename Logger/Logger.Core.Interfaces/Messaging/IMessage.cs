using System;
using System.Collections.Generic;

using Logger.Common.ObjectModel;




namespace Logger.Core.Interfaces.Messaging
{
    public interface IMessage : IEquatable<IMessage>,
            ICloneable<IMessage>,
            ISynchronizable
    {
        IDictionary<string, object> Data { get; }

        dynamic DynamicData { get; }

        Guid Id { get; }

        bool IsSerializable { get; }

        string Name { get; }

        DateTime Timestamp { get; }
    }
}
