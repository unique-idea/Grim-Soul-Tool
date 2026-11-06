// Thuộc Project UI
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI.Helpers
{
    public static class ImageHelper
    {
        public static ImageSource? LoadIcon(string? iconKey)
        {
            if (string.IsNullOrEmpty(iconKey)) return null;

            try
            {
                // Load ảnh từ Pack URI của WPF (Assets/Icons/{iconKey}.png)
                Uri uri = new Uri($"pack://application:,,,/Assets/Icons/{iconKey}.png", UriKind.Absolute);
                var streamInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamInfo == null) return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = streamInfo.Stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null; // Nếu không tìm thấy file ảnh thì trả về null để dùng icon mặc định
            }
        }
    }
}
