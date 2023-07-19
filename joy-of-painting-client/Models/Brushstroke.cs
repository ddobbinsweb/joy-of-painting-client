namespace joy_of_painting_client.Models;

public class Brushstroke
{
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }
    public string Color { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Order { get; set; }
}
