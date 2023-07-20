namespace joy_of_painting_client.Models;

public class LeaderBoardEntry
{
    public int PainterId { get; set; }
    public int ArtistId { get; set; }
    public int PaintingCategoryId { get; set; }
    public int? OriginalId { get; set; }
    public string PainterName { get; set; }
    public string ItemName { get; set; }
    public int Score { get; set; }
    public int Order { get; set; }
}
