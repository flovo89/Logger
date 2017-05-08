using System;




namespace Logger.Common.IO.Documents.Ini
{
    [Serializable]
    public enum IniMultiValueHandling
    {
        None = 0,

        First = 1,

        Last = 2,

        All = 3
    }
}
