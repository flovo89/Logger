using System;




namespace Logger.Common.ObjectModel
{
    public interface ICloneable <out T> : ICloneable
    {
        new T Clone ();
    }
}
