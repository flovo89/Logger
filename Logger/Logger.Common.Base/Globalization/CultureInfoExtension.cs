using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Logger.Common.Collections.Generic;




namespace Logger.Common.Globalization
{
    public static class CultureInfoExtensions
    {
        #region Static Constructor/Destructor

        static CultureInfoExtensions ()
        {
            CultureInfoExtensions.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            CultureInfoExtensions.SpecificCultures = new Dictionary<CultureInfo, CultureInfo[]>();

            foreach (CultureInfo culture1 in CultureInfoExtensions.Cultures)
            {
                List<CultureInfo> specificCultures = new List<CultureInfo>();

                foreach (CultureInfo culture2 in CultureInfoExtensions.Cultures)
                {
                    if (culture2.IsNeutralCulture)
                    {
                        continue;
                    }

                    string[] i1Pieces = culture1.ToString().Split('-');
                    string[] i2Pieces = culture2.ToString().Split('-');

                    if (string.Equals(i1Pieces[0], i2Pieces[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        specificCultures.Add(culture2);
                    }
                }

                CultureInfoExtensions.SpecificCultures.Add(culture1, specificCultures.ToArray());
            }
        }

        #endregion




        #region Static Properties/Indexer

        private static CultureInfo[] Cultures { get; set; }

        private static Dictionary<CultureInfo, CultureInfo[]> SpecificCultures { get; set; }

        #endregion




        #region Static Methods

        public static CultureInfo[] GetAllCultures (this CultureInfo culture, bool useUserOverride)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            HashSet<CultureInfo> cultures = new HashSet<CultureInfo>();

            cultures.Add(culture.GetUserOverriden(useUserOverride));
            cultures.Add(culture.GetNeutralCulture(useUserOverride));
            cultures.AddRange(culture.GetSpecificCultures(useUserOverride));

            return cultures.ToArray();
        }

        public static CultureInfo GetDefaultSpecificCulture (this CultureInfo culture, bool useUserOverride)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            if (!culture.IsNeutralCulture)
            {
                return culture.GetUserOverriden(useUserOverride);
            }

            CultureInfo[] cultures = culture.GetSpecificCultures(useUserOverride);

            if (cultures.Length == 0)
            {
                culture = culture.GetConsoleFallbackUICulture();
                return culture.GetUserOverriden(useUserOverride);
            }

            return cultures[0];
        }

        public static CultureInfo GetNeutralCulture (this CultureInfo culture, bool useUserOverride)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            if (culture.IsNeutralCulture)
            {
                return culture.GetUserOverriden(useUserOverride);
            }

            string[] pieces = culture.ToString().Split('-');

            return new CultureInfo(pieces[0], useUserOverride);
        }

        public static CultureInfo[] GetSpecificCultures (this CultureInfo culture, bool useUserOverride)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            if (CultureInfoExtensions.SpecificCultures.ContainsKey(culture))
            {
                CultureInfo[] cultures = CultureInfoExtensions.SpecificCultures[culture];

                for (int i1 = 0; i1 < cultures.Length; i1++)
                {
                    cultures[i1] = cultures[i1].GetUserOverriden(useUserOverride);
                }

                return cultures;
            }

            return new CultureInfo[0];
        }

        public static CultureInfo GetUserOverriden (this CultureInfo culture, bool useUserOverride)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            return useUserOverride ? new CultureInfo(culture.ToString(), true) : culture;
        }

        #endregion
    }
}
