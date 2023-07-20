
using joy_of_painting_client.Models;

namespace joy_of_painting_client.Responses;

public class LeaderBoardResponse: BaseResponse
{
    public int FilterBy { get; set; }
    public string Type { get; set; }
    public List<LeaderBoardEntry> Results { get; set; } = new();
}

