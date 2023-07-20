namespace joy_of_painting_client.Responses;

public class ListResponse<T>: BaseResponse where T : class
{
    public List<T> Items { get; set; } = new List<T>();
}
