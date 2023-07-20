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
    private const string FILE_PATH = $@"C:\Users\David Dobbins\Pictures\joy-of-painting\";
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

                    IEnumerable<string> artistPaintings = artists.FirstOrDefault(x => x.Name == selectedArtist).Paintings.Select(x => x.Name);

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


                        List<Brushstroke> strokes = await PixelateImageAsync(painting, pixelSize);

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
                            CanvasImage image = new(imagePath);
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
    }
    public async Task<List<Brushstroke>> PixelateImageAsync(Painting painting, int size = 20, int failureCount = 0)
    {
        List<Brushstroke> brushstrokes = new();
        await Helper.DownloadImageAsync(FILE_PATH, painting.Id.ToString(), new Uri(painting.Url));
        string imagePath = $"{FILE_PATH}/{painting.Id}.jpg";
        Bitmap image = new Bitmap(imagePath);

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
           await PixelateImageAsync(painting, size, failureCount + 1);
        }


        RotateBrushstrokes(brushstrokes, width, height);
        string previewImagePath = $@"{FILE_PATH}\preview\{DateTime.Now.ToFileTimeUtc}";

        CreateImageStream(width, height, brushstrokes, previewImagePath);
        var previewImage = new CanvasImage(previewImagePath);
        previewImage.MaxWidth = pixelSize;

        AnsiConsole.Write(previewImage);

        if (!AnsiConsole.Confirm("Does this look good?"))
        {
           await  PixelateImageAsync(painting, size);
        }
        return brushstrokes;
    }

    public  void CreateImageStream(int width, int height, List<Brushstroke> brushstrokes, string savePath)
    {
        using (Bitmap bitmap = new Bitmap(width, height))
        {
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
            bitmap.Save(savePath, ImageFormat.Jpeg);
        }
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

    
}
