using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;




namespace Logger.Common.Imaging
{
    public static class IconExtensions
    {
        #region Static Methods

        public static BitmapFrame ToBitmapFrame (this Icon icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException(nameof(icon));
            }

            BitmapSource bitmapSource = icon.ToBitmapSource();

            return BitmapFrame.Create(bitmapSource);
        }

        public static BitmapSource ToBitmapSource (this Icon icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException(nameof(icon));
            }

            /*using (MemoryStream ms = new MemoryStream())
			{
				icon.Save(ms);

				ms.Flush();
				ms.Position = 0;

				BitmapImage image = new BitmapImage();

				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.CreateOptions = BitmapCreateOptions.None;
				image.StreamSource = ms;
				image.EndInit();

				return ( image.Clone() );
			}*/

            return icon.ToBitmap().ToBitmapSource();
        }

        public static byte[] ToByteArray (this Icon icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException(nameof(icon));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);

                ms.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public static Icon ToIcon (this byte[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            using (MemoryStream ms = new MemoryStream(array))
            {
                return (Icon)new Icon(ms).Clone();
            }
        }

        #endregion
    }
}
