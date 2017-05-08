namespace Logger.Common.IO.Endianess
{
    public static class EndianessExtensions
    {
        #region Static Methods

        public static Endianess Resolve (this Endianess endianess)
        {
            switch (endianess)
            {
                case Endianess.LocalMachine:
                {
                    return EndianConverter.LocalMachine;
                }
            }

            return endianess;
        }

        #endregion
    }
}
