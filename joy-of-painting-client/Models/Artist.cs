namespace joy_of_painting_client.Models;

public class Artist : BaseModel
{
    public List<Painting> Paintings { get; set; } = new List<Painting>();
}
