using Spectre.Console;
using System.Configuration;

namespace joy_of_painting_client.Game;

public static class GameSettings
{
    public static async Task CheckAsync()
    {
        string? userKey = GetUserKey();
        if (string.IsNullOrEmpty(userKey))
        {
            SetUserInfo();
            await GameLayout.Reset();
        }
    }

    public static string? GetUserKey()
    {
        return ConfigurationManager.AppSettings["userKey"];
    }
    public static string? GetCanvasUrl(){
         return ConfigurationManager.AppSettings["canvasURL"];
    }
    public static void Menu()
    {
        string menu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
          .Title("[bold white]Settings Menu [/] [green] Select a option  [/]")
          .PageSize(10)
          .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
          .AddChoices("User", "Back")
          );

        if (menu == "Back") return;


        string UserMenu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
           .Title("[bold white]User Menu[/][green] Select a option [/]")
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
           .AddChoices("Api Key", "Back"));

        if (UserMenu == "Back") Menu();

        SetUserInfo();
    }

    private static void SetUserInfo()
    {
        if (!AnsiConsole.Confirm("[green]Do you have a Api Key to Play?[/]"))
        {
            AnsiConsole.MarkupLine("No Problem! goto: [green] http://jop.revunit.com[/]");
            AnsiConsole.MarkupLine("Sign in with your Revunit email.");
            AnsiConsole.MarkupLine("click on you avatar in the top right");
            AnsiConsole.MarkupLine("copy the value from the box");
            Helper.OpenBrowser("https://jop.revunit.com/");
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
