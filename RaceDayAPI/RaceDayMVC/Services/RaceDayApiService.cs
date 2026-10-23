using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public RaceDayApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Attach the current user's API session cookie.
        private void AddSessionCookie(
            HttpRequestMessage request)
        {
            var sessionCookie = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("ApiSessionCookie");

            if (!string.IsNullOrEmpty(sessionCookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", sessionCookie);
            }
        }

        // Retrieve information from the API.
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, endpoint);

            AddSessionCookie(request);

            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<T>(_jsonOptions);
        }

        // Send information to the API.
        public async Task<HttpResponseMessage> PostAsync<T>(
            string endpoint, T data)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(data)
            };

            AddSessionCookie(request);

            return await _httpClient.SendAsync(request);
        }

        // Update information through the API.
        public async Task<HttpResponseMessage> PutAsync<T>(
            string endpoint, T data)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Put, endpoint)
            {
                Content = JsonContent.Create(data)
            };

            AddSessionCookie(request);

            return await _httpClient.SendAsync(request);
        }

        // Delete information through the API.
        public async Task<HttpResponseMessage> DeleteAsync(
            string endpoint)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete, endpoint);

            AddSessionCookie(request);

            return await _httpClient.SendAsync(request);
        }
    }
}