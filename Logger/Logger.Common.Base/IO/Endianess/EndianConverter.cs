using System;

using Logger.Common.Collections.Generic;




namespace Logger.Common.IO.Endianess
{
    public static class EndianConverter
    {
        #region Static Properties/Indexer

        public static Endianess LocalMachine
        {
            get
            {
                if (BitConverter.IsLittleEndian)
                {
                    return Endianess.Little;
                }

                return Endianess.Big;
            }
        }

        #endregion




        #region Static Methods

        public static byte[] GetBytes (double value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        public static byte[] GetBytes (float value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        public static byte[] GetBytes (int value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        public static byte[] GetBytes (short value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        public static byte[] GetBytes (long value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        [CLSCompliant (false)]
        public static byte[] GetBytes (uint value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        [CLSCompliant (false)]
        public static byte[] GetBytes (ushort value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        [CLSCompliant (false)]
        public static byte[] GetBytes (ulong value, Endianess target)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (EndianConverter.RequiresSwap(target))
            {
                buffer.Reverse();
            }

            return buffer;
        }

        public static bool RequiresSwap (Endianess target)
        {
            if (target == Endianess.LocalMachine)
            {
                return false;
            }

            return EndianConverter.LocalMachine != target;
        }

        public static double ToDouble (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 8);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToDouble(buffer, 0);
        }

        public static double ToDouble (byte[] value, Endianess source)
        {
            return EndianConverter.ToDouble(value, 0, source);
        }

        public static short ToInt16 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 2);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToInt16(buffer, 0);
        }

        public static short ToInt16 (byte[] value, Endianess source)
        {
            return EndianConverter.ToInt16(value, 0, source);
        }

        public static int ToInt32 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 4);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToInt32(buffer, 0);
        }

        public static int ToInt32 (byte[] value, Endianess source)
        {
            return EndianConverter.ToInt32(value, 0, source);
        }

        public static long ToInt64 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 8);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToInt64(buffer, 0);
        }

        public static long ToInt64 (byte[] value, Endianess source)
        {
            return EndianConverter.ToInt64(value, 0, source);
        }

        public static float ToSingle (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 4);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToSingle(buffer, 0);
        }

        public static float ToSingle (byte[] value, Endianess source)
        {
            return EndianConverter.ToSingle(value, 0, source);
        }

        [CLSCompliant (false)]
        public static ushort ToUInt16 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 2);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToUInt16(buffer, 0);
        }

        [CLSCompliant (false)]
        public static ushort ToUInt16 (byte[] value, Endianess source)
        {
            return EndianConverter.ToUInt16(value, 0, source);
        }

        [CLSCompliant (false)]
        public static uint ToUInt32 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 4);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToUInt32(buffer, 0);
        }

        [CLSCompliant (false)]
        public static uint ToUInt32 (byte[] value, Endianess source)
        {
            return EndianConverter.ToUInt32(value, 0, source);
        }

        [CLSCompliant (false)]
        public static ulong ToUInt64 (byte[] value, int offset, Endianess source)
        {
            byte[] buffer = value.ToSubArray(offset, 8);

            if (EndianConverter.RequiresSwap(source))
            {
                buffer.Reverse();
            }

            return BitConverter.ToUInt64(buffer, 0);
        }

        [CLSCompliant (false)]
        public static ulong ToUInt64 (byte[] value, Endianess source)
        {
            return EndianConverter.ToUInt64(value, 0, source);
        }

        #endregion
    }
}
