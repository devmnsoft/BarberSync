namespace BarberSync.PublicWeb.Services; public interface IApiClient{Task<T?> GetAsync<T>(string url); Task<T?> PostAsync<T>(string url,object body);} 
