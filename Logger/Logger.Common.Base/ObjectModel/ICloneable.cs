using System;




namespace Logger.Common.Base.ObjectModel
{
    public interface ICloneable <out T> : ICloneable
    {
        new T Clone ();
    }
}
