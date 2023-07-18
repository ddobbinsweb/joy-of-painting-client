using joy_of_painting_client.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace joy_of_painting_client
{
    public class BaseClient<T> : HttpClient, IBaseClient<T> where T : class
    {
        private readonly string basePath;
        private const string MEDIA_TYPE = "application/json";
        private const string BASE_ADDRESS = "https://api.jop.revunit.com/";
        private readonly string _key;

        public BaseClient(string basePath, string key)
        {
            BaseAddress = new Uri(BASE_ADDRESS);
            this.basePath = BaseAddress + basePath;
            _key = key;
        }
        public async Task Post(T item)
        {
            try
            {
                SetupHeaders();
                var serializedJson = GetSerializedObject(item);
                var bodyContent = GetBodyContent(serializedJson);

                var response = await PostAsync(basePath, bodyContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create the resource returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public async Task<T> Post(object item)
        {
            try
            {
                SetupHeaders();
                var serializedJson = GetSerializedObject(item);
                var bodyContent = GetBodyContent(serializedJson);

                var response = await PostAsync(basePath, bodyContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create the resource returned {response.StatusCode}");
                }
                var result = await response.Content.ReadAsStringAsync();
                var returnModel = JsonConvert.DeserializeObject<T>(result);

                return returnModel;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                SetupHeaders();

                var response = await GetAsync(basePath);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var returnModel = JsonConvert.DeserializeObject<List<T>>(result);

                    return returnModel;
                }
                else
                {
                    throw new Exception ($"Failed to retrieve items returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<T> GetAsync(int id)
        {
            try
            {
                SetupHeaders();

                var response = await GetAsync(basePath + $"/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var returnModel = JsonConvert.DeserializeObject<T>(result);

                    return returnModel;
                }
                else
                {
                    throw new Exception("Failed to retrieve item id: " + id + $" returned " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        #region Client Helper Methods
        protected virtual void SetupHeaders()
        {
            DefaultRequestHeaders.Clear();
            //Define request data format  
            DefaultRequestHeaders.Accept.Add
                (new MediaTypeWithQualityHeaderValue
                (MEDIA_TYPE));
            DefaultRequestHeaders.Add("X-API-KEY", _key);
        }

        protected virtual string GetSerializedObject(object obj)
        {
            var serializedJson = JsonConvert.SerializeObject(obj);

            return serializedJson;
        }

        protected virtual StringContent GetBodyContent(string serializedJson)
        {
            var bodyContent = new StringContent
                (serializedJson, Encoding.UTF8, "application/json");

            return bodyContent;
        }
        #endregion
    }
}
