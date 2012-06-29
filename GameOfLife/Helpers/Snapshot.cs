using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace TAlex.GameOfLife.Helpers
{
    /// <summary>
    /// Capture and render content as an image.
    /// </summary>
    public static class Snapshot
    {
        public static void ToClipboard(FrameworkElement element)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)element.ActualWidth,
                (int)element.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);

            Clipboard.SetImage(rtb);
        }

        public static void ToClipboard(BitmapSource source)
        {
            Clipboard.SetImage(source);
        }

        public static void ToFile(FrameworkElement element, Window owner)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.DefaultExt = "png";
            sfd.FileName = "Snapshot";
            sfd.Filter = "PNG (*.png)|*.png";

            if (sfd.ShowDialog(owner) == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                        (int)element.ActualWidth,
                        (int)element.ActualHeight,
                        96, 96, PixelFormats.Pbgra32);
                rtb.Render(element);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));

                System.IO.Stream file = sfd.OpenFile();
                png.Save(file);
                file.Close();
            }
        }

        public static void ToFile(BitmapSource source, Window owner)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.DefaultExt = "png";
            sfd.FileName = "Snapshot";
            sfd.Filter = "PNG (*.png)|*.png";

            if (sfd.ShowDialog(owner) == true)
            {
                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(source));

                System.IO.Stream file = sfd.OpenFile();
                png.Save(file);
                file.Close();
            }
        }
    }
}
