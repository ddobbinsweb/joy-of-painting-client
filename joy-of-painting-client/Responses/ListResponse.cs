namespace joy_of_painting_client.Responses
{
    public class ListResponse<T> where T : class
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<T> Items { get; set; }
    }
}
