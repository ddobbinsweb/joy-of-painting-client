using Spectre.Console;
using Spectre.Console.Rendering;

namespace joy_of_painting_client.Game
{
    public class GameLayout
    {
       // private static Layout _layout;
        public static void Setup()
        {
            // create layout
            // Create the layout
            // _layout = new Layout("Top");
            // .SplitRows(new Layout("Top"));


            // Update the Top column
            // Update("Top", Header());
            // Update("Center", Center());

            // Render the layout
            InsertHeader();
            // add Header

        }
        public static void InsertHeader()
        {
            AnsiConsole.Write(Header());
        }
        private static Panel Header()
        {
            // Header
            return new Panel(Align.Center(new Markup("Welcome to Joy of Painting!"), VerticalAlignment.Middle));

           // AnsiConsole.Write(panel);

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
