namespace joy_of_painting_client.Responses
{
    public class PixelationResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<string> ValidationErrors { get; set; }
        public int Id { get; set; }
        public string Url {  get; set; }
        public int Score { get; set; }
    }
}
