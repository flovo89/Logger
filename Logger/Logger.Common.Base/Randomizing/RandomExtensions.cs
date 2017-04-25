using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Logger.Common.DataTypes;
using Logger.Common.Resources;




namespace Logger.Common.Randomizing
{
    public static class RandomExtensions
    {
        #region Constants

        private const int CommaThreshold = 20;

        private const string LoremIpsumResourceName = "Claymount.Console.Common.Randomizing.LoremIpsum.txt";

        private static readonly Encoding LoremIpsumEncoding = Encoding.UTF8;

        #endregion




        #region Static Constructor/Destructor

        static RandomExtensions ()
        {
            RandomExtensions.LoremIpsum = null;
            RandomExtensions.LoremIpsumPieces = null;
        }

        #endregion




        #region Static Properties/Indexer

        private static string LoremIpsum { get; set; }

        private static string[] LoremIpsumPieces { get; set; }

        #endregion




        #region Static Methods

        public static T[] Mix <T> (this Random randomizer, IEnumerable<T> values)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            List<T> valuesList = values.ToList();

            if (valuesList.Count < 2)
            {
                return valuesList.ToArray();
            }

            T[] randomizedArray = new T[valuesList.Count];
            bool[] checkArray = new bool[valuesList.Count];

            while (valuesList.Count > 0)
            {
                T value = valuesList[0];

                while (true)
                {
                    int position = randomizer.Next(0, checkArray.Length);

                    if (!checkArray[position])
                    {
                        randomizedArray[position] = value;
                        checkArray[position] = true;
                        break;
                    }
                }

                valuesList.RemoveAt(0);
            }

            return randomizedArray;
        }

        public static void Mix <T> (this Random randomizer, IList<T> values)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            lock (values.GetSyncRoot(true))
            {
                T[] mixedValues = randomizer.Mix((IEnumerable<T>)values);

                for (int i1 = 0; i1 < mixedValues.Length; i1++)
                {
                    values[i1] = mixedValues[i1];
                }
            }
        }

        public static byte NextByte (this Random randomizer)
        {
            return randomizer.NextByte(byte.MinValue, byte.MaxValue);
        }

        public static byte NextByte (this Random randomizer, byte max)
        {
            return randomizer.NextByte(byte.MinValue, max);
        }

        public static byte NextByte (this Random randomizer, byte min, byte max)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            return (byte)randomizer.Next(min, max);
        }

        public static void NextBytes (this Random randomizer, byte[] buffer, int offset, int count)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            byte[] randomBuffer = new byte[count];
            randomizer.NextBytes(randomBuffer);

            randomBuffer.CopyTo(buffer, offset);
        }

        public static DateTime NextDate (this Random randomizer)
        {
            return randomizer.NextDate(DateTime.MinValue, DateTime.MaxValue);
        }

        public static DateTime NextDate (this Random randomizer, DateTime minDate, DateTime maxDate)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            double minTicks = minDate.Ticks;
            double maxTicks = maxDate.Ticks;

            double newTicks = randomizer.NextDouble() * ( maxTicks - minTicks ) + minTicks;

            DateTime newDate = new DateTime((long)newTicks);

            return newDate;
        }

        public static double NextDeviation (this Random randomizer, double value, double maxDeviation)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            return ( 1.0 + randomizer.NextDouble(-1 * maxDeviation, maxDeviation) ) * value;
        }

        public static double NextDouble (this Random randomizer, double max)
        {
            return randomizer.NextDouble(double.MinValue, max);
        }

        public static double NextDouble (this Random randomizer, double min, double max)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            return min + randomizer.NextDouble() * ( max - min );
        }

        public static bool NextPredicate (this Random randomizer, double chance)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            double value = randomizer.NextDouble();

            return value <= chance;
        }

        public static string NextText (this Random randomizer)
        {
            return randomizer.NextText(1, 1, 1, 1);
        }

        public static string NextText (this Random randomizer, int minLines, int maxLines, int minWordsPerLine, int maxWordsPerLine)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            if (minLines < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minLines));
            }

            if (( maxLines < 0 ) || ( maxLines < minLines ))
            {
                throw new ArgumentOutOfRangeException(nameof(maxLines));
            }

            if (minWordsPerLine < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minWordsPerLine));
            }

            if (( maxWordsPerLine < 0 ) || ( maxWordsPerLine < minWordsPerLine ))
            {
                throw new ArgumentOutOfRangeException(nameof(maxWordsPerLine));
            }

            if (( minLines == 0 ) || ( maxLines == 0 ))
            {
                return string.Empty;
            }

            if (RandomExtensions.LoremIpsum == null)
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(RandomExtensions.LoremIpsumResourceName))
                {
                    if (s == null)
                    {
                        throw new ResourceNotFoundException(RandomExtensions.LoremIpsumResourceName);
                    }

                    using (StreamReader r = new StreamReader(s, RandomExtensions.LoremIpsumEncoding))
                    {
                        RandomExtensions.LoremIpsum = r.ReadToEnd();
                    }
                }

                RandomExtensions.LoremIpsum = RandomExtensions.LoremIpsum.Replace("\r", string.Empty).Replace("\n", " ").Replace(".", string.Empty).Replace(",", string.Empty).ToLowerInvariant();
            }

            if (RandomExtensions.LoremIpsumPieces == null)
            {
                RandomExtensions.LoremIpsumPieces = RandomExtensions.LoremIpsum.Split(' ');
            }

            StringBuilder str = new StringBuilder();

            int lines = randomizer.Next(minLines, maxLines + 1);

            for (int i1 = 0; i1 < lines; i1++)
            {
                int words = randomizer.Next(minWordsPerLine, maxWordsPerLine + 1);
                int comma = -1;

                if (words >= RandomExtensions.CommaThreshold)
                {
                    comma = randomizer.Next(words / 3 * 1, words / 3 * 2);
                }

                if (i1 > 0)
                {
                    str.AppendLine();
                }

                for (int i2 = 0; i2 < words; i2++)
                {
                    string word = RandomExtensions.LoremIpsumPieces[randomizer.Next(0, RandomExtensions.LoremIpsumPieces.Length)];

                    if (i2 > 0)
                    {
                        str.Append(" ");
                        str.Append(word);
                    }
                    else
                    {
                        str.Append(word.Substring(0, 1).ToUpperInvariant());
                        str.Append(word.Substring(1));
                    }

                    if (i2 == comma)
                    {
                        str.Append(",");
                    }
                }

                str.Append(".");
            }

            return str.ToString();
        }

        public static TimeSpan NextTime (this Random randomizer)
        {
            return randomizer.NextTime(TimeSpan.MinValue, TimeSpan.MaxValue);
        }

        public static TimeSpan NextTime (this Random randomizer, TimeSpan minTime, TimeSpan maxTime)
        {
            if (randomizer == null)
            {
                throw new ArgumentNullException(nameof(randomizer));
            }

            double minTicks = minTime.Ticks;
            double maxTicks = maxTime.Ticks;

            double newTicks = randomizer.NextDouble() * ( maxTicks - minTicks ) + minTicks;

            TimeSpan newTime = new TimeSpan((long)newTicks);

            return newTime;
        }

        #endregion
    }
}
