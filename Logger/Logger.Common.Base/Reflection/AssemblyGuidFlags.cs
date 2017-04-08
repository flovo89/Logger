using System;




namespace Logger.Common.Base.Reflection
{
    [Flags]
    [Serializable]
    public enum AssemblyGuidFlags
    {
        None = 0x00,

        TryGuidAttribute = 0x01,

        UseAssemblyName = 0x02,

        IgnoreVersion = 0x10
    }
}
