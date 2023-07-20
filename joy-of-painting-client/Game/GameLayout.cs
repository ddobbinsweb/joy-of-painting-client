using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Reflection.Metadata.Ecma335;

namespace joy_of_painting_client.Game
{
    public static class GameLayout
    {
        // private static Layout _layout;
        public static void Setup()
        {
            // create layout
            // Create the layout
            //  _layout = new Layout("Top");
            // .SplitRows(new Layout("Top"));


            // Update the Top column
            //  Update("Top", Header());
            // Update("Center", Center());

            // Render the layout
            InsertHeader();
            // add Header
            // AnsiConsole.Write(_layout);

        }
        public static async void InsertHeader()
        {
            AnsiConsole.Write(await HeaderAsync());
        }
        private static async Task<Panel> HeaderAsync()
        {
            Markup userContent;
            Panel header;
            var userkey = GameSettings.GetUserKey();
            if (userkey != null)
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
                        $"[link green] https://jop.revunit.com/profile/{userResponse.Item.Id} [/]"
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
        private static Panel Center()
        {
            return new Panel(Align.Center(new Markup("TODO Insert a Grid with the current users data"), VerticalAlignment.Middle));
        }

        internal static void Reset()
        {
            AnsiConsole.Clear();
            Setup();
        }

        //public static void Update(string location, IRenderable update)
        //{
        //    _layout[location].Update(update);
        //}
    }
}
