using System.Configuration;
using System.Drawing;

namespace joy_of_painting_client;

public static class Helper
{

    public static async Task<Image> DownloadImageAsync(string directoryPath, string fileName, Uri uri)
    {
        Byte[] imageBytes;
        // Get the file extension
        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
        var fileExtension = Path.GetExtension(uriWithoutQuery);

        // Create file path and ensure directory exists
        var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
        Directory.CreateDirectory(directoryPath);
        using (var httpClient = new HttpClient())
        {
            using (var fileStream = await httpClient.GetStreamAsync(uri))
            {
                return Image.FromStream(fileStream);
            }
        }
    }

    public async static Task<MemoryStream> DownloadImageAsync(Uri imageUrl)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);
                return new MemoryStream(imageData);
            }
            catch (HttpRequestException)
            {
                // Handle exception if image cannot be downloaded
                return null;
            }
        }
    }
    public static void AddConfigValue(string key, string value)
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings[key].Value = value;
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }
}
