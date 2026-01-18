using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace YotoCreator.Converters
{
    /// <summary>
    /// Converter to convert byte array to BitmapImage for display
    /// </summary>
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                var image = new BitmapImage();
                using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                {
                    // This is async, but we can't await in Convert
                    // So we use a workaround
                    var task = stream.WriteAsync(bytes.AsBuffer()).AsTask();
                    task.Wait();
                    stream.Seek(0);

                    var loadTask = image.SetSourceAsync(stream).AsTask();
                    loadTask.Wait();

                    return image;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
