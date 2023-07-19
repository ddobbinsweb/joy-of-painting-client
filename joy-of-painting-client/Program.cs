using joy_of_painting_client;
using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Spectre.Console;
using System.Data;


var panel = new Panel(
          Align.Center(
              new Markup("Welcome to Joy of Painting!"),
              VerticalAlignment.Middle));
AnsiConsole.Write(panel);
// get config settings
GameSettings.Check();
string? userKey = GameSettings.GetUserKey();


do
{
  

    /*
    *TODO: change app commands
    * first options-
    * pixelate image
    *  - sub options
    *      find paintings by Artist or Category
    *      
    * view leaderboard
    * create orignal
    *
    */

    /*
     *  TODO: ADD game navigation
     *  
     */

    string gameMenu = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold white]--Game Menu-- [/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices("Pixelate Image", "View LeaderBoard", "Settings", "Exit")
            );
    switch (gameMenu)
    {
        case "Pixelate Image":
            //Make code go to pixalor
            AnsiConsole.WriteLine("TODO:");
            break;
        case "View LeaderBoard":
            AnsiConsole.WriteLine("TODO:");
            break;
        case "Settings":
            AnsiConsole.WriteLine("TODO:");
            break;
            break;
        case "Exit":
            Environment.Exit(0);
            break;
        default:
            break;
    }

    // TODO: make this an input
    Pixelator pixelator = new();


    string findPaintingOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green] How would you like to find a painting by [/]?")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices("Artist", "Category")
            );
    CommandProcesor.ProcessCommand(findPaintingOption);

    if (findPaintingOption == "Artist")
    {
        var artistClient = new BaseClient<ListResponse<Artist>>("artist/search", userKey);
        // get artists
        var values = new Dictionary<string, string?>
        {
            { "name", null },
        };
        ListResponse<Artist> response = null;

        await AnsiConsole.Status()
        .StartAsync("Loading...", async ctx =>
        {
            response = await artistClient.Post(values);
        });
        if (response != null)
        {
            var artists = response.Items.OrderBy(x => x.Id);
            string[] artistNames = artists.Select(x => x.Name).ToArray();

            var selectedArtist = AnsiConsole.Prompt(
                  new SelectionPrompt<string>()
                      .Title("[green]Which Artist would you like to select [/]?")
                      .MoreChoicesText("[grey](Move up and down to reveal more artist)[/]")
                      .AddChoices(artistNames)
                      );
            // TODO: make this reusable incase there is an error
            if (selectedArtist != null)
            {
                AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedArtist);

                var artistPaintings = artists.FirstOrDefault(x => x.Name == selectedArtist).Paintings.Select(x => x.Name);

                var selectedPainting = AnsiConsole.Prompt(new SelectionPrompt<string>()
                  .Title("[green]Which Painting would you like to select [/]?")
                  .MoreChoicesText("[grey](Move up and down to reveal more paintings)[/]")
                  .AddChoices(artistPaintings)
                  );
                AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedPainting);

                var painting = artists.FirstOrDefault(x => x.Name == selectedArtist)?.Paintings.Find(x => x.Name == selectedPainting);

                if (painting != null)
                {
                    int pixelSize = AnsiConsole.Prompt(new TextPrompt<int>("[grey][[Optional]][/] [green]what pixel size[/]?")
                                         .DefaultValue<int>(20));

                    AnsiConsole.MarkupLine("Pixelating image");
                    string BaseImagePath = $@"C:\Users\David Dobbins\Pictures\joy-of-painting\"; // TODO:get from configuration

                    await Helper.DownloadImageAsync(BaseImagePath, painting.Id.ToString(), new Uri(painting.Url));

                    string originalImagePath = $@"{BaseImagePath}{painting.Id}.jpg";
                    CanvasImage originalImage = new(originalImagePath);
                    originalImage.MaxWidth(pixelSize);


                    List<Brushstroke> strokes = await pixelator.PixelateImageAsync(painting, pixelSize);

                    Pixelation pixelation = new()
                    {
                        Brushstrokes = strokes,
                        PaintingId = painting.Id
                    };

                    PixelationResponse pixelationResponse = null;

                    await AnsiConsole.Status().StartAsync("Loading...", async ctx =>
                    {
                        // upload pixalation
                        var pixelationClient = new BaseClient<PixelationResponse>("pixelation", userKey);
                        ctx.Status("Uploading pixelation");
                        ctx.Spinner(Spinner.Known.Star);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        pixelationResponse = await pixelationClient.Post(pixelation);
                    });

                    if (pixelationResponse != null)
                    {
                        if (pixelationResponse.ValidationErrors.Count > 0)
                        {
                            // log error
                            AnsiConsole.Write($"[red] error: {pixelationResponse.ValidationErrors.ToList()}");
                        }
                        await Helper.DownloadImageAsync($@"C:\Users\David Dobbins\Pictures\joy-of-painting\submissions", pixelationResponse.Id.ToString(), new Uri(pixelationResponse.Url));

                        string imagePath = $@"C:\Users\David Dobbins\Pictures\joy-of-painting\submissions\{pixelationResponse.Id}.jpg";
                        CanvasImage image = new (imagePath);
                        image.MaxWidth(pixelSize);

                        AnsiConsole.WriteLine(pixelationResponse.Message);
                    }
                    if (!AnsiConsole.Confirm("Pixelate another image?"))
                    {
                        AnsiConsole.MarkupLine("Ok... :(");
                        Environment.Exit(0);
                    }
                    AnsiConsole.Clear();

                }
            }
        }
    }
    else
    {
        // get all categories

        // output the category options

        // ask user for input of which category

        // get paintings by category selected
    }
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);


