using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

using Point = System.Drawing.Point;




namespace Logger.Common.Imaging
{
    public static class BitmapExtensions
    {
        #region Static Methods

        public static Bitmap ToBitmap (this byte[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            using (MemoryStream ms = new MemoryStream(array, false))
            {
                using (Bitmap bmp = new Bitmap(ms))
                {
                    return bmp.Clone(new Rectangle(new Point(0, 0), bmp.Size), bmp.PixelFormat);
                }
            }
        }

        public static BitmapFrame ToBitmapFrame (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            BitmapSource bmp = image.ToBitmapSource();

            return BitmapFrame.Create(bmp);
        }

        public static BitmapSource ToBitmapSource (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                hBitmap = image.GetHbitmap();

                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                return bitmapSource.Clone();
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    BitmapExtensions.DeleteObject(hBitmap);
                    hBitmap = IntPtr.Zero;
                }
            }
        }

        public static byte[] ToBmpByteArray (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.ToByteArray(ImageFormat.Bmp);
        }

        public static byte[] ToByteArray (this Bitmap image, ImageFormat format)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);

                ms.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public static Icon ToIcon (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            using (Icon icon = Icon.FromHandle(image.GetHicon()))
            {
                return (Icon)icon.Clone();
            }
        }

        public static byte[] ToJpegByteArray (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.ToByteArray(ImageFormat.Jpeg);
        }

        public static byte[] ToPngByteArray (this Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.ToByteArray(ImageFormat.Png);
        }

        [DllImport ("gdi32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool DeleteObject (IntPtr hObject);

        #endregion
    }
}
