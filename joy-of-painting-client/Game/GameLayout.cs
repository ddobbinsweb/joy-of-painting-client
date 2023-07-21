using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Spectre.Console;

namespace joy_of_painting_client.Game
{
    public static class GameLayout
    {
        public static async Task Setup()
        {
            // add Header
            await InsertHeader();
        }
        public static async Task InsertHeader()
        {
            AnsiConsole.Write(await HeaderAsync());
        }
        private static async Task<Panel> HeaderAsync()
        {
            string? userkey = GameSettings.GetUserKey();
            if (!string.IsNullOrWhiteSpace(userkey))
            {
                // TODO: move this to app startup and cache values
                // get Player
                var painterClient = new BaseClient("painter", userkey);

                var userResponse = await painterClient.GetAllAsync<SingleResponse<Painter>>("/me");
                if (userResponse.Item != null)
                {
                    // insert player 
                    var grid = new Grid();

                    // Add columns 
                    grid.AddColumn();
                    grid.AddColumn();
                    grid.AddColumn();

                    // Add header row 
                    grid.AddRow("Player", "Score", "Profile");
                    grid.AddRow(
                        $"{userResponse.Item.Name}",
                        $"{userResponse.Item.Pixelations.Sum(x => x.Score)}",
                        $"[green] https://jop.revunit.com/profile/{userResponse.Item.Id} [/]"
                    );

                    // Write to Console
                    Panel panel = new(grid);
                    return new Panel(grid)
                          .Expand()
                          .SquareBorder()
                          .Header("[red]Welcome to Joy of Painting[/]");
                }


            }
            // Header

            return new Panel(new Text("Joy of Painting!").Centered())
                .Expand()
                .SquareBorder()
                .Header("[red]Welcome to[/]");
        }
        public static async Task Reset()
        {
            AnsiConsole.Clear();
            await Setup();
        }

    }
}
