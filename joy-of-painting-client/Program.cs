// See https://aka.ms/new-console-template for more information
using joy_of_painting_client;
using Newtonsoft.Json;
using System.Drawing;


Console.WriteLine("Hello, World!");
// Specifying a file path
string path = @"C:\Users\David Dobbins\Pictures\joy-of-painting\8_Dobbins.jpg";


Bitmap bitmap;
using (Stream bmpStream = System.IO.File.Open(path, System.IO.FileMode.Open))
{
    Image image = Image.FromStream(bmpStream);

    bitmap = new Bitmap(image);

}
if (bitmap != null)
{
    List<BrushStroke> strokes = new List<BrushStroke>();
    for (int i = 0; i < bitmap.Width; i++)
    {
        for (int j = 0; j < bitmap.Height; j++)
        {
            Color pixel = bitmap.GetPixel(i, j);
            BrushStroke stroke = new BrushStroke() { 
                FromX= ,
                FromY= ,
                ToX =,
                ToY = ,
                Color = ColorTranslator.ToHtml(pixel),
                Height= j,
                Width= i
            };

            strokes.Add(stroke);
        }
    }
    if(strokes.Count > 0)
    {
        var temp = JsonConvert.SerializeObject(strokes);
        var temp2  = 0;
    }
}

