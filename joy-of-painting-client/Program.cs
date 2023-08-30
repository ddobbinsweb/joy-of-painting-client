using joy_of_painting_client;
using joy_of_painting_client.Game;
using Spectre.Console;


await GameLayout.Setup();
// get config settings
await GameSettings.CheckAsync();
string gameMenu;
do
{
    gameMenu = AnsiConsole.Prompt(
       new SelectionPrompt<string>()
           .Title("[bold white]Game Menu[/] [green] What would you like to do? [/]")
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
           .AddChoices(
               "Pixelate Image",
               "View LeaderBoard",
               "Create Original",
               "Settings",
               "Exit")
           );

    switch (gameMenu)
    {
        case "Pixelate Image":
            //Make code go to pixalor
            await Pixelator.PixelateImageAsync();
            break;
        case "View LeaderBoard":
            await LeaderBoard.ShowAsync();
            break;
        case "Create Original":
            Original.Create();
            break;
        case "Settings":
            GameSettings.Menu();
            break;
        case "Exit":
            AnsiConsole.WriteLine("GOOD BYE");
            break;
        default:
            AnsiConsole.WriteLine("[red] Invalid option! please try again [/]");
            break;
    }
 await GameLayout.Reset();

} while (gameMenu != "Exit");


