using joy_of_painting_client.Game;
using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Policy;
using Color = System.Drawing.Color;

namespace joy_of_painting_client;

public static class Pixelator
{
    public static async Task PixelateImageAsync()
    {
        string? userKey = GameSettings.GetUserKey();
        if (string.IsNullOrEmpty(userKey))
        {
            AnsiConsole.WriteLine("User Key not configured");
            return;
        }
        Painting? painting = await GetPaintingAsync(userKey);

        if (painting != null)
        {
            bool approved = false;
            while (!approved)
            {
                approved = await SubmitPixelation(userKey, painting, approved);
            }

            if (!AnsiConsole.Confirm("Pixelate another image?"))
            {
                AnsiConsole.Clear();
                return;
            }
            else
            {
                await PixelateImageAsync();
            }
            AnsiConsole.Clear();
        }
    }

    public static async Task<Painting?> GetPaintingAsync(string userKey)
    {
        string findPaintingOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] How would you like to find a painting by [/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices("Artist", "Category","Back")
                );

        Painting? painting = null;
        if (findPaintingOption == "Back") return painting;

        if (findPaintingOption == "Artist")
        {
            var artistClient = new BaseClient("artist", userKey);
            // get artists
            var values = new Dictionary<string, string?>
            {
                { "name", null },
            };
            ListResponse<Artist> response = null;

            await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                response = await artistClient.Post<ListResponse<Artist>>(values, "/search");
            });
            if (response != null)
            {
                var artists = response.Items.OrderBy(x => x.Id);
                string[] artistNames = artists.Select(x => x.Name).Append("Back").ToArray();

                var selectedArtist = AnsiConsole.Prompt(
                      new SelectionPrompt<string>()
                          .Title("[green]Which Artist would you like to select [/]?")
                          .MoreChoicesText("[grey](Move up and down to reveal more artist)[/]")
                          .AddChoices(artistNames)
                          );
                if (selectedArtist == "Back") await GetPaintingAsync(userKey);
                // TODO: make this reusable incase there is an error
                if (selectedArtist != null)
                {
                    AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedArtist);

                    IEnumerable<string> artistPaintings = artists.FirstOrDefault(x => x.Name == selectedArtist).Paintings.Select(x => x.Name).Append("Back");

                    var selectedPainting = AnsiConsole.Prompt(new SelectionPrompt<string>()
                      .Title("[green]Which Painting would you like to select [/]?")
                      .MoreChoicesText("[grey](Move up and down to reveal more paintings)[/]")
                      .AddChoices(artistPaintings)
                      );
                    if (selectedPainting == "Back") return painting;
                    AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedPainting);

                    painting = artists.FirstOrDefault(x => x.Name == selectedArtist)?.Paintings.Find(x => x.Name == selectedPainting);
                }
            }
        }
        else
        {
            // get all categories
            var paintingClient = new BaseClient("painting", userKey);

            var categoryResponse = await paintingClient.GetAllAsync<ListResponse<PaintingCategory>>("/categories");
            if (categoryResponse != null)
            {
                var categories = categoryResponse.Items.ToList();
                var categoryNames = categories.Select(x => x.Name).Append("Back");
                // output the category options
                var selectedCategory = AnsiConsole.Prompt(
                      new SelectionPrompt<string>()
                          .Title("[green]Which Category would you like to select [/]?")
                          .MoreChoicesText("[grey](Move up and down to reveal more artist)[/]")
                          .AddChoices(categoryNames)
                          );

                if (selectedCategory == "Back") return painting;

                if (selectedCategory != null)
                {
                    AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedCategory);
                    // get paintings by category selected
                    int? selectedCategoryId = categories.Find(x => x.Name == selectedCategory)?.Id;
                    if (selectedCategoryId != null)
                    {
                        var values = new Dictionary<string, string?>
                        {
                             { "artistId", "0" },
                             {"paintingCategoryId" , selectedCategoryId.ToString() }
                        };
                        ListResponse<Painting> paintingResponse = null;

                        await AnsiConsole.Status()
                        .StartAsync("Loading...", async ctx =>
                        {
                            paintingResponse = await paintingClient.Post<ListResponse<Painting>>(values, "/search");
                        });

                        if (paintingResponse != null)
                        {
                            var paintingNames = paintingResponse.Items.Select(x => x.Name);

                            var selectedPainting = AnsiConsole.Prompt(
                                 new SelectionPrompt<string>()
                                .Title("[green]Which Category would you like to select [/]?")
                                .MoreChoicesText("[grey](Move up and down to reveal more artist)[/]")
                                 .AddChoices(paintingNames));

                            if (selectedPainting != null)
                            {
                                AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedPainting);
                                int? selectedPaintingId = paintingResponse.Items.Find(x => x.Name == selectedPainting)?.Id;
                                if (selectedPaintingId != null)
                                {
                                    SingleResponse<Painting> selectedPaintingResponse = await paintingClient.GetAsync<SingleResponse<Painting>>(selectedPaintingId.Value, null);
                                    painting = selectedPaintingResponse.Item;
                                }

                            }
                        }
                    }
                }
            }
        }
        return painting;
    }

    public static (MemoryStream, List<Brushstroke>) PixelateImage(MemoryStream imageStream, int pixelSize)
    {
        List<Brushstroke> brushstrokes = new List<Brushstroke>();
        using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageStream))
        {
            int width = image.Width;
            int height = image.Height;

            for (int y = 0; y < height; y += pixelSize)
            {
                for (int x = 0; x < width; x += pixelSize)
                {
                    int toX = Math.Min(x + pixelSize, width);
                    int toY = Math.Min(y + pixelSize, height);

                    Rgba32 averageColor = CalculateAverageColorRgb(image, x, y, toX, toY);
                    var brushstroke = new Brushstroke
                    {
                        FromX = x,
                        ToX = toX,
                        FromY = y,
                        ToY = toY,
                        Color = $"#{averageColor.R:X2}{averageColor.G:X2}{averageColor.B:X2}",
                        Height = toY - y,
                        Width = toX - x
                    };
                    brushstrokes.Add(brushstroke);

                    FillBlockWithColor(image, x, y, toX, toY, averageColor);
                }
            }

            MemoryStream pixelatedImageStream = new MemoryStream();

            image.SaveAsPng(pixelatedImageStream);
            pixelatedImageStream.Position = 0;

            return (pixelatedImageStream, brushstrokes);
        }
    }

    private static Rgba32 CalculateAverageColorRgb(Image<Rgba32> image, int fromX, int fromY, int toX, int toY)
    {
        int totalR = 0, totalG = 0, totalB = 0, totalA = 0;
        int pixelCount = 0;

        for (int y = fromY; y < toY; y++)
        {
            for (int x = fromX; x < toX; x++)
            {
                var pixel = image[x, y];
                totalR += pixel.R;
                totalG += pixel.G;
                totalB += pixel.B;
                totalA += pixel.A;
                pixelCount++;
            }
        }

        if (pixelCount > 0)
        {
            return new Rgba32(
                (byte)(totalR / pixelCount),
                (byte)(totalG / pixelCount),
                (byte)(totalB / pixelCount),
                (byte)(totalA / pixelCount)
            );
        }

        return new Rgba32(0, 0, 0);
    }

    private static void FillBlockWithColor(Image<Rgba32> image, int fromX, int fromY, int toX, int toY, Rgba32 color)
    {
        for (int y = fromY; y < toY; y++)
        {
            for (int x = fromX; x < toX; x++)
            {
                image[x, y] = color;
            }
        }
    }
    private static async Task<bool> SubmitPixelation(string? userKey, Painting painting, bool approved)
    {
        int pixelSize = AnsiConsole.Prompt(new TextPrompt<int>("[grey][[Optional]][/] [green]what pixel size[/]?")
            .ValidationErrorMessage("[red] That's not a valid value[/]")
            .Validate(size =>
            {
                return size switch
                {
                    <= 0 => ValidationResult.Error("[red] Pixel Size must be greater than 0 [/]"),
                    >= 100 => ValidationResult.Error("[red] Pixel Size can not be greater than 100 [/]"),
                    _ => ValidationResult.Success(),
                };
            }));

        List<Brushstroke> strokes = new();

        await AnsiConsole.Status().StartAsync("Pixelating image...", async ctx =>
        {
            MemoryStream imageDownload = await Helper.DownloadImageAsync(new Uri(painting.Url));
            (MemoryStream, List<Brushstroke>) pixelatedImage = PixelateImage(imageDownload, pixelSize);

            var previewImage = new CanvasImage(pixelatedImage.Item1);
            previewImage.MaxWidth = pixelSize;

            AnsiConsole.Write(previewImage);
            strokes = pixelatedImage.Item2;
            AnsiConsole.WriteLine($"Pixel Count:{strokes.Count}");
        });

        if (AnsiConsole.Confirm("Does this look good?"))
        {
            approved = true;
            await UploadPixalation(userKey, painting, pixelSize, strokes);
        }

        return approved;
    }

    private static async Task UploadPixalation(string? userKey, Painting painting, int pixelSize, List<Brushstroke> strokes)
    {
        PixelationSubmission pixelation = new()
        {
            Brushstrokes = strokes,
            PaintingId = painting.Id
        };

        PixelationResponse pixelationResponse = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Uploading pixelation...", async ctx =>
            {
                // upload pixalation
                var pixelationClient = new BaseClient("pixelation", userKey);
                pixelationResponse = await pixelationClient.Post<PixelationResponse>(pixelation, null);
            });

        if (pixelationResponse != null)
        {
            if (pixelationResponse.ValidationErrors.Count > 0)
            {
                // log error
                AnsiConsole.Write($"[red] error[/] : {String.Join("", pixelationResponse.ValidationErrors)}");
                if (AnsiConsole.Confirm("[green] Try Again?[/]"))
                {
                    if (await SubmitPixelation(userKey, painting, false))
                    {
                        // do something
                    }
                }
            }
            else
            {
                
                MemoryStream pixalateImage = await Helper.DownloadImageAsync(new Uri(pixelationResponse.Url));

                CanvasImage image = new(pixalateImage);
                image.MaxWidth(pixelSize);

                AnsiConsole.Write(image);
                AnsiConsole.WriteLine(pixelationResponse.Message);
            }
        }
    }
}
