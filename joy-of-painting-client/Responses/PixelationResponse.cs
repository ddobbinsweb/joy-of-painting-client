namespace joy_of_painting_client.Responses;

public class PixelationResponse : BaseResponse
{
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public int Id { get; set; }
    public string Url {  get; set; } = string.Empty;
    public int Score { get; set; }
}
