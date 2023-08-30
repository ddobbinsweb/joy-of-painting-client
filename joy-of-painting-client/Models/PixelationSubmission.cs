namespace joy_of_painting_client.Models;
public class PixelationSubmission
{
    public List<Brushstroke> Brushstrokes { get; set; } = new();
    public int PaintingId { get; set; }
}