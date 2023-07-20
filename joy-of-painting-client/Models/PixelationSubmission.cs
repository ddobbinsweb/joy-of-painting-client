namespace joy_of_painting_client.Models;
public class PixelationSubmission
{
    public PixelationSubmission()
    {
    }

    public List<Brushstroke> Brushstrokes { get; set; }
    public int PaintingId { get; set; }
}