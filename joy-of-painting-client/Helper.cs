using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace joy_of_painting_client;

public static class Helper
{
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

    public static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
