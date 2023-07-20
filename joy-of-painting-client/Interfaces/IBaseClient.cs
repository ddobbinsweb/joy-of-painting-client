namespace joy_of_painting_client.Interfaces;

public interface IBaseClient: IDisposable
{
    Task<T> GetAllAsync<T>(string path);
    Task<T> GetAsync<T>(int id, string? path);
    Task Post<T>(T item, string path);
    Task<T> Post<T>(object item,string path);
}
