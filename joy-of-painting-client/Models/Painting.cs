namespace joy_of_painting_client.Models;

public class Painting : BaseModel
{
    public PaintingCategory PaintingCategory { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
}
