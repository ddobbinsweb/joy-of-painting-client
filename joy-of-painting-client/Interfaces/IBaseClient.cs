namespace joy_of_painting_client.Interfaces
{
    public interface IBaseClient<T>: IDisposable
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task Post(T item);
        Task<List<T>> Post(object item);
    }
}
