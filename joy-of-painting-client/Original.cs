using joy_of_painting_client.Game;
using Spectre.Console;

namespace joy_of_painting_client
{
    public static class Original
    {
        //TODO: get from app settings
       // private static string _originalSite = "https://joy-of-painting-client.vercel.app/";
        private static string _originalSite = "http://localhost:3000/";
        public static void Create()
        {
            string? userKey = GameSettings.GetUserKey();
            AnsiConsole.MarkupLine("To Create an Original Masterpiece");
            AnsiConsole.MarkupLine($"-  Navigate to {_originalSite}");
            Helper.OpenBrowser($"{_originalSite}?key={userKey}");

            AnsiConsole.Confirm("All Done?");
        }
    }
}
