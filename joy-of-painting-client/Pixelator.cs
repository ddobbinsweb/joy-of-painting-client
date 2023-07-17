using System.Drawing;

namespace joy_of_painting_client;

public class Pixelator
{
    public List<Brushstroke> PixelateImage(string imagePath)
    {
        List<Brushstroke> brushstrokes = new List<Brushstroke>();

        Bitmap image = new Bitmap(imagePath);

        int width = image.Width;
        int height = image.Height;

        int pixelSize = (int)Math.Ceiling((double)width / 16); // Adjust the pixel size as needed

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
                //if (brushstrokes.Count >= 500)
                //    return brushstrokes;
            }
        }

        return brushstrokes;
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
