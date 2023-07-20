using Spectre.Console;
using System.Configuration;

namespace joy_of_painting_client.Game;

public static class GameSettings
{

    public static void Check()
    {
        string? userKey = GetUserKey();
        if (string.IsNullOrEmpty(userKey))
        {
            //TODO: add way to login to get guid
            SetUserInfo();
            GameLayout.Reset();
        }
    }


    public static string? GetUserKey()
    {
        return ConfigurationManager.AppSettings["userKey"];
    }

    public static void Menu()
    {
        string menu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
          .Title("[bold white]--Settings Menu-- [/]")
          .PageSize(10)
          .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
          .AddChoices("User", "Back")
          );

        if (menu == "Back")
        {
            return;
        }
        else
        {
            string UserMenu = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
               .Title("[bold white]--User Menu-- [/]")
               .PageSize(10)
               .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
               .AddChoices("Api Key", "Back"));

            if (UserMenu == "Back")
            {
                Menu();
            }
            else
            {
                SetUserInfo();
            }
        }
    }

    private static void SetUserInfo()
    {
        if (!AnsiConsole.Confirm("Do you have a Api Key to Play?"))
        {
            AnsiConsole.MarkupLine("No Problem! goto: [link green] http://jop.revunit.com[/]");
            AnsiConsole.MarkupLine("Sign in with your Revunit email.");
            AnsiConsole.MarkupLine("click on you avatar in the top right");
            AnsiConsole.MarkupLine("copy the value from the box");
            if (!AnsiConsole.Confirm("Ready"))
            {
                AnsiConsole.Clear();
                return;
            }
        

        }
        AnsiConsole.MarkupLine("Great!");
        string userKey = AnsiConsole.Prompt(new TextPrompt<string>("[green] What's your api key[/]?"));

        Helper.AddConfigValue("userKey", userKey);

        AnsiConsole.Clear();
    }


}
