namespace joy_of_painting_client.Responses
{
    public class SingleResponse<T> where T : class
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Item { get; set; }
    }
}
