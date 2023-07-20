namespace joy_of_painting_client.Models;

public class Painter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Pixelation> Pixelations { get; set; } = new();
    public List<Orginal> Orginals { get; set; } = new();
} 
