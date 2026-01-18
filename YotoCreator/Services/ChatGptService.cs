using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YotoCreator.Services
{
    /// <summary>
    /// Service for interacting with ChatGPT/OpenAI API
    /// </summary>
    public class ChatGptService
    {
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private const string OPENAI_API_BASE = "https://api.openai.com/v1";

        public bool IsAuthenticated { get; private set; }

        public ChatGptService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Authenticate with the OpenAI API
        /// </summary>
        public async Task<bool> AuthenticateAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            try
            {
                _apiKey = apiKey;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                // Test the API key by making a simple request
                var response = await _httpClient.GetAsync($"{OPENAI_API_BASE}/models");
                IsAuthenticated = response.IsSuccessStatusCode;

                return IsAuthenticated;
            }
            catch
            {
                IsAuthenticated = false;
                return false;
            }
        }

        /// <summary>
        /// Generate an image using DALL-E
        /// </summary>
        /// <param name="prompt">Description of the image to generate</param>
        /// <param name="width">Width of the image (default 1024)</param>
        /// <param name="height">Height of the image (default 1024)</param>
        /// <returns>Image data as byte array</returns>
        public async Task<byte[]> GenerateImageAsync(string prompt, int width = 1024, int height = 1024)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                // Prepare the request
                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = $"{width}x{height}",
                    quality = "standard",
                    response_format = "url"
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Make the API request
                var response = await _httpClient.PostAsync($"{OPENAI_API_BASE}/images/generations", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseContent);

                // Get the image URL
                var imageUrl = jsonResponse["data"]?[0]?["url"]?.ToString();
                if (string.IsNullOrEmpty(imageUrl))
                    throw new Exception("No image URL returned from API");

                // Download the image
                var imageData = await _httpClient.GetByteArrayAsync(imageUrl);

                // If we need a specific size (like 16x16), we'd resize here
                // For now, returning the generated image as-is
                return imageData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generate a chat completion
        /// </summary>
        public async Task<string> GenerateTextAsync(string prompt, string systemMessage = null)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                var messages = new System.Collections.Generic.List<object>();

                if (!string.IsNullOrEmpty(systemMessage))
                {
                    messages.Add(new { role = "system", content = systemMessage });
                }

                messages.Add(new { role = "user", content = prompt });

                var requestBody = new
                {
                    model = "gpt-4",
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 1000
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{OPENAI_API_BASE}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseContent);

                return jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate text: {ex.Message}", ex);
            }
        }
    }
}
