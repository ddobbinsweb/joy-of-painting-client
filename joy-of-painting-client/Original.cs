using Spectre.Console;

namespace joy_of_painting_client
{
    public static class Original
    {
        //TODO: get from app settings
        private static string _originalSite = "https://joy-of-painting-client.vercel.app/";
        public static void Create()
        {
            AnsiConsole.MarkupLine("To Create an Original Masterpiece");
            AnsiConsole.MarkupLine($"-  Navigate to {_originalSite}");
            AnsiConsole.Confirm("All Done?");
        }
    }
}
