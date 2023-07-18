using joy_of_painting_client;
using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Spectre.Console;


do
{
    AnsiConsole.WriteLine("Joy of Painting!");

    // TODO: make this an input
    var pixelator = new Pixelator();
    string key = AnsiConsole.Prompt(new TextPrompt<string>("[grey][[Optional]][/] [green] What's your api key[/]?")
                                         .DefaultValue<string>("486c153f-e4f0-4657-8ac8-fe3850bb51ad"));

    var findPaintingOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green] How would you like to find a painting  by [/]?")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices("Artist", "Category")
            );


    if (findPaintingOption == "Artist")
    {
        var artistClient = new BaseClient<ListResponse<Artist>>("artist/search", key);
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
                    await Helper.DownloadImageAsync($@"C:\Users\David Dobbins\Pictures\joy-of-painting\", painting.Id.ToString(), new Uri(painting.Url));

                    List<Brushstroke> strokes = pixelator.PixelateImage($@"C:\Users\David Dobbins\Pictures\joy-of-painting\{painting.Id}.jpg", pixelSize);
                    if(strokes.Count > 1000)
                    {
                        strokes = pixelator.PixelateImage($@"C:\Users\David Dobbins\Pictures\joy-of-painting\{painting.Id}.jpg", pixelSize - 1);
                    }
                    Pixelation pixelation = new()
                    {
                        Brushstrokes = strokes,
                        PaintingId = painting.Id
                    };
                    
                    PixelationResponse pixelationResponse = null;

                    await AnsiConsole.Status().StartAsync("Loading...", async ctx =>
                    {
                        // upload pixalation
                        var pixelationClient = new BaseClient<PixelationResponse>("pixelation", key);
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


                        var image = new CanvasImage($@"C:\Users\David Dobbins\Pictures\joy-of-painting\submissions\{pixelationResponse.Id}.jpg");
                        image.MaxWidth(32);
                        AnsiConsole.Write(image);

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


