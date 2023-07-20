using joy_of_painting_client.Game;
using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Spectre.Console;
using System.Drawing;
using System.Drawing.Imaging;
using Color = System.Drawing.Color;

namespace joy_of_painting_client;

public class Pixelator
{
    public async Task PixelateImageAsync()
    {

        string findPaintingOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] How would you like to find a painting by [/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices("Artist", "Category")
                );

        string? userKey = GameSettings.GetUserKey();
        Painting painting = null;

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

                    IEnumerable<string> artistPaintings = artists.FirstOrDefault(x => x.Name == selectedArtist).Paintings.Select(x => x.Name);

                    var selectedPainting = AnsiConsole.Prompt(new SelectionPrompt<string>()
                      .Title("[green]Which Painting would you like to select [/]?")
                      .MoreChoicesText("[grey](Move up and down to reveal more paintings)[/]")
                      .AddChoices(artistPaintings)
                      );
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
                var categories = categoryResponse.Items.Cast<PaintingCategory>().ToList();
                var categoryNames = categories.Select(x => x.Name);
                // output the category options
                var selectedCategory = AnsiConsole.Prompt(
                      new SelectionPrompt<string>()
                          .Title("[green]Which Category would you like to select [/]?")
                          .MoreChoicesText("[grey](Move up and down to reveal more artist)[/]")
                          .AddChoices(categoryNames)
                          );

                if (selectedCategory != null)
                {
                    AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", selectedCategory);
                    // get paintings by category selected
                    int? selectedCategoryId = categories.FirstOrDefault(x => x.Name == selectedCategory)?.Id;
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
                                int? selectedPaintingId = paintingResponse.Items.FirstOrDefault(x => x.Name == selectedPainting)?.Id;
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

        if (painting != null)
        {


            bool approved = false;
            while (!approved)
            {
                int pixelSize = AnsiConsole.Prompt(new TextPrompt<int>("[grey][[Optional]][/] [green]what pixel size[/]?")
                          .DefaultValue<int>(20));
                List<Brushstroke> strokes = new();
                await AnsiConsole.Status().StartAsync("Pixelating image...", async ctx =>
                {

                    strokes = await PixelateImageAsync(painting, pixelSize);
                });
                if (AnsiConsole.Confirm("Does this look good?"))
                {
                    approved = true;
                    await UploadPixalation(userKey, painting, pixelSize, strokes);
                }
            }


            if (!AnsiConsole.Confirm("Pixelate another image?"))
            {
                AnsiConsole.Clear();
                return;
            }
            AnsiConsole.Clear();
        }
    }

    public async Task<List<Brushstroke>> PixelateImageAsync(Painting painting, int size = 20, int failureCount = 0)
    {
        List<Brushstroke> brushstrokes = new();

        MemoryStream imageDownload = await Helper.DownloadImageAsync(new Uri(painting.Url));

        Bitmap image = new Bitmap(imageDownload);

        int width = image.Width;
        int height = image.Height;

        int pixelSize = (int)Math.Ceiling((double)width / size); // Adjust the pixel size as needed

        int order = 0;

        for (int y = 0; y < height; y += pixelSize)
        {
            for (int x = 0; x < width; x += pixelSize)
            {
                Brushstroke brushstroke = new Brushstroke();
                brushstroke.Order = order;

                int toX = Math.Min(x + pixelSize, width);
                int toY = Math.Min(y + pixelSize, height);

                brushstroke.FromX = x;
                brushstroke.ToX = toX;
                brushstroke.FromY = y;
                brushstroke.ToY = toY;

                string color = CalculateAverageColor(image, x, y, toX, toY).ToArgb().ToString("X6");
                brushstroke.Color = "#" + color;

                brushstroke.Width = toX - x;
                brushstroke.Height = toY - y;

                brushstrokes.Add(brushstroke);

                order++;
            }
        }
        if (brushstrokes.Count > 1000)
        {
            await PixelateImageAsync(painting, size - 2, failureCount + 1);
        }

        RotateBrushstrokes(brushstrokes, width, height);

        MemoryStream previewImageStream = CreateImageStream(width, height, brushstrokes);
        var previewImage = new CanvasImage(previewImageStream);
        previewImage.MaxWidth = pixelSize;

        AnsiConsole.Write(previewImage);

        return brushstrokes;
    }

    public MemoryStream CreateImageStream(int width, int height, List<Brushstroke> brushstrokes)
    {
        Bitmap bitmap = new Bitmap(width, height);

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            ColorConverter colorConverter = new ColorConverter();
            g.Clear(Color.White);

            foreach (Brushstroke stroke in brushstrokes.OrderBy(x => x.Order).ToList())
            {
                Pen pen = new Pen((Color)colorConverter.ConvertFromString(stroke.Color));
                pen.Width = stroke.Width;
                g.DrawLine(pen, stroke.FromX, stroke.FromY, stroke.ToX, stroke.ToY);
            }
        }
        MemoryStream memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0;
        return memoryStream;

    }
    private void RotateBrushstrokes(List<Brushstroke> brushstrokes, int width, int height)
    {
        foreach (var brushstroke in brushstrokes)
        {
            int centerX = brushstroke.FromX + (brushstroke.Width / 2);
            int centerY = brushstroke.FromY + (brushstroke.Height / 2);

            double angle = Math.PI / 4; // 45 degrees clockwise rotation

            int rotatedFromX = (int)Math.Round((brushstroke.FromX - centerX) * Math.Cos(angle) - (brushstroke.FromY - centerY) * Math.Sin(angle) + centerX);
            int rotatedFromY = (int)Math.Round((brushstroke.FromX - centerX) * Math.Sin(angle) + (brushstroke.FromY - centerY) * Math.Cos(angle) + centerY);
            int rotatedToX = (int)Math.Round((brushstroke.ToX - centerX) * Math.Cos(angle) - (brushstroke.ToY - centerY) * Math.Sin(angle) + centerX);
            int rotatedToY = (int)Math.Round((brushstroke.ToX - centerX) * Math.Sin(angle) + (brushstroke.ToY - centerY) * Math.Cos(angle) + centerY);

            brushstroke.FromX = rotatedFromX;
            brushstroke.FromY = rotatedFromY;
            brushstroke.ToX = rotatedToX;
            brushstroke.ToY = rotatedToY;

            // Ensure the coordinates are within the image bounds
            brushstroke.FromX = Math.Max(0, Math.Min(brushstroke.FromX, width - 1));
            brushstroke.FromY = Math.Max(0, Math.Min(brushstroke.FromY, height - 1));
            brushstroke.ToX = Math.Max(0, Math.Min(brushstroke.ToX, width - 1));
            brushstroke.ToY = Math.Max(0, Math.Min(brushstroke.ToY, height - 1));
        }
    }

    private Color CalculateAverageColor(Bitmap image, int fromX, int fromY, int toX, int toY)
    {
        int rTotal = 0, gTotal = 0, bTotal = 0;
        int pixelCount = 0;

        for (int y = fromY; y < toY; y++)
        {
            for (int x = fromX; x < toX; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                rTotal += pixelColor.R;
                gTotal += pixelColor.G;
                bTotal += pixelColor.B;
                pixelCount++;
            }
        }

        int avgR = rTotal / pixelCount;
        int avgG = gTotal / pixelCount;
        int avgB = bTotal / pixelCount;

        return Color.FromArgb(avgR, avgG, avgB);
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
                AnsiConsole.Write($"[red] error: {pixelationResponse.ValidationErrors.ToList()}");
            }
            MemoryStream pixalateImage = await Helper.DownloadImageAsync(new Uri(pixelationResponse.Url));

            CanvasImage image = new(pixalateImage);
            image.MaxWidth(pixelSize);

            AnsiConsole.Write(image);
            AnsiConsole.WriteLine(pixelationResponse.Message);
        }
    }
}
