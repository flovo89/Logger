using System;




namespace Logger.Common.IO.Documents.Ini
{
    [Flags]
    [Serializable]
    public enum IniDocumentNormalizeOptions
    {
        SortSections = 0x01,

        SortElements = 0x02,

        RemoveTextElements = 0x04,

        RemoveEmptySections = 0x08,

        KeepTextInEmptySections = 0x10,

        MergeSections = 0x20
    }
}
