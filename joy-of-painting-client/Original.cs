using joy_of_painting_client.Game;
using Spectre.Console;

namespace joy_of_painting_client
{
    public static class Original
    {
        private static string? _originalSite = GameSettings.GetCanvasUrl();
   

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
