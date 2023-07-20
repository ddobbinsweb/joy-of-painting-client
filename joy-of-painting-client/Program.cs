using joy_of_painting_client;
using joy_of_painting_client.Game;
using Spectre.Console;


GameLayout.Setup();
// get config settings
GameSettings.Check();
string gameMenu;
do
{
    gameMenu = AnsiConsole.Prompt(
       new SelectionPrompt<string>()
           .Title("[bold white]--Game Menu-- [/]")
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
           .AddChoices(
               "Pixelate Image",
               "View LeaderBoard",
               "Settings",
               "Exit")
           );

    switch (gameMenu)
    {
        case "Pixelate Image":
            //Make code go to pixalor
            Pixelator pixelator = new();
            await pixelator.PixelateImageAsync();
            break;
        case "View LeaderBoard":
            await LeaderBoard.ShowAsync();
            break;
        case "Settings":
            GameSettings.Menu();
            break;
        case "Exit":
            AnsiConsole.WriteLine("GOOD BYE");
            break;
        default:
            break;
    }
    GameLayout.Reset();

} while (gameMenu != "Exit");


