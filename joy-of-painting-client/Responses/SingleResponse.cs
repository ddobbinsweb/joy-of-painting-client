
namespace joy_of_painting_client.Responses;

public class SingleResponse<T>: BaseResponse where T : class
{
    public T Item { get; set; }
}
