using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RaceDayMVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public RaceDayApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Retrieve data from the API
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            using var response =
                await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<T>(_jsonOptions);
        }

        // Send data to the API
        public async Task<HttpResponseMessage> PostAsync<T>(
            string endpoint, T data)
        {
            return await _httpClient.PostAsJsonAsync(
                endpoint, data, _jsonOptions);
        }

        // Update data through the API
        public async Task<HttpResponseMessage> PutAsync<T>(
            string endpoint, T data)
        {
            return await _httpClient.PutAsJsonAsync(
                endpoint, data, _jsonOptions);
        }

        // Delete data through the API
        public async Task<HttpResponseMessage> DeleteAsync(
            string endpoint)
        {
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}