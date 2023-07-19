using Spectre.Console;
using System.Configuration;

namespace joy_of_painting_client;

public static class GameSettings
{

    public static void Check()
    {
        string? userKey = GetUserKey();
        if (string.IsNullOrEmpty(userKey))
        {
            //TODO: add way to login to get guid
            if (!AnsiConsole.Confirm("Do you have a Api Key to Play?"))
            {
                AnsiConsole.MarkupLine("No Problem! goto: [link green] http://jop.revunit.com[/]");
                AnsiConsole.MarkupLine("Sign in with your Revunit email.");
                AnsiConsole.MarkupLine("click on you avatar in the top left");
                AnsiConsole.MarkupLine("copy the value from the box");
                AnsiConsole.Confirm("Ready");
            }
            AnsiConsole.MarkupLine("Great!");
            userKey = AnsiConsole.Prompt(new TextPrompt<string>("[green] What's your api key[/]?"));

            Helper.AddConfigValue("userKey", userKey);

            AnsiConsole.Clear();
        }
    }

    public static string? GetUserKey()
    {
        return ConfigurationManager.AppSettings["userKey"];
    }
}
