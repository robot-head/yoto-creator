using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YotoCreator.Models;

namespace YotoCreator.Services
{
    /// <summary>
    /// Service for interacting with the Yoto API
    /// </summary>
    public class YotoApiService
    {
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private const string YOTO_API_BASE = "https://api.yotoplay.com/v1";

        public bool IsAuthenticated { get; private set; }

        public YotoApiService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Authenticate with the Yoto API
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
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // Test the API key by making a simple request (adjust endpoint as needed)
                var response = await _httpClient.GetAsync($"{YOTO_API_BASE}/user");
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
        /// Create new content on Yoto
        /// </summary>
        public async Task<string> CreateContentAsync(YotoContent content)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                // Step 1: Create the content metadata
                var contentMetadata = new
                {
                    title = content.Title,
                    description = content.Description,
                    language = "en"
                };

                var metadataJson = JsonConvert.SerializeObject(contentMetadata);
                var metadataContent = new StringContent(metadataJson, Encoding.UTF8, "application/json");

                var createResponse = await _httpClient.PostAsync($"{YOTO_API_BASE}/content", metadataContent);
                createResponse.EnsureSuccessStatusCode();

                var createResponseContent = await createResponse.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(createResponseContent);
                var contentId = jsonResponse["id"]?.ToString();

                if (string.IsNullOrEmpty(contentId))
                    throw new Exception("Failed to get content ID from response");

                // Step 2: Upload icon if available
                if (content.Icon != null && content.Icon.Length > 0)
                {
                    await UploadIconAsync(contentId, content.Icon);
                }

                // Step 3: Upload cover image if available
                if (content.CoverImage != null && content.CoverImage.Length > 0)
                {
                    await UploadCoverImageAsync(contentId, content.CoverImage);
                }

                // Step 4: Upload chapters and their audio files
                if (content.Chapters != null && content.Chapters.Count > 0)
                {
                    // Process chapters in order
                    foreach (var chapter in content.Chapters.OrderBy(c => c.Order))
                    {
                        // Upload chapter icon if available
                        if (chapter.Icon != null && chapter.Icon.Length > 0)
                        {
                            // Note: This assumes the Yoto API supports chapter icons
                            // If not, this could be used as a marker/thumbnail in the audio stream
                            await UploadChapterIconAsync(contentId, chapter.Order, chapter.Icon);
                        }

                        // Upload audio files for this chapter in order
                        foreach (var audioFile in chapter.AudioFiles.OrderBy(a => a.Order))
                        {
                            await UploadAudioFileAsync(contentId, audioFile);
                        }
                    }
                }
                else if (content.AudioFiles != null && content.AudioFiles.Count > 0)
                {
                    // Fallback to flat audio file structure for backward compatibility
                    foreach (var audioFile in content.AudioFiles)
                    {
                        await UploadAudioFileAsync(contentId, audioFile);
                    }
                }

                return contentId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create content: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update existing content
        /// </summary>
        public async Task<bool> UpdateContentAsync(string contentId, YotoContent content)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                var contentMetadata = new
                {
                    title = content.Title,
                    description = content.Description
                };

                var metadataJson = JsonConvert.SerializeObject(contentMetadata);
                var metadataContent = new StringContent(metadataJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{YOTO_API_BASE}/content/{contentId}", metadataContent);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Upload icon for content
        /// </summary>
        private async Task UploadIconAsync(string contentId, byte[] iconData)
        {
            using (var content = new MultipartFormDataContent())
            {
                var imageContent = new ByteArrayContent(iconData);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "icon", "icon.png");

                var response = await _httpClient.PostAsync($"{YOTO_API_BASE}/content/{contentId}/icon", content);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Upload cover image for content
        /// </summary>
        private async Task UploadCoverImageAsync(string contentId, byte[] coverData)
        {
            using (var content = new MultipartFormDataContent())
            {
                var imageContent = new ByteArrayContent(coverData);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "cover", "cover.png");

                var response = await _httpClient.PostAsync($"{YOTO_API_BASE}/content/{contentId}/cover", content);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Upload chapter icon for content
        /// </summary>
        private async Task UploadChapterIconAsync(string contentId, int chapterOrder, byte[] iconData)
        {
            using (var content = new MultipartFormDataContent())
            {
                var imageContent = new ByteArrayContent(iconData);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "icon", $"chapter_{chapterOrder}_icon.png");

                // Note: Adjust this endpoint based on actual Yoto API chapter support
                // This is a placeholder endpoint that may need to be updated
                var response = await _httpClient.PostAsync($"{YOTO_API_BASE}/content/{contentId}/chapters/{chapterOrder}/icon", content);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Upload audio file for content
        /// </summary>
        private async Task UploadAudioFileAsync(string contentId, AudioFile audioFile)
        {
            // Read the audio file
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(audioFile.FilePath);
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
            var audioData = new byte[buffer.Length];
            using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                reader.ReadBytes(audioData);
            }

            using (var content = new MultipartFormDataContent())
            {
                var audioContent = new ByteArrayContent(audioData);
                audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
                content.Add(audioContent, "audio", audioFile.FileName);

                var response = await _httpClient.PostAsync($"{YOTO_API_BASE}/content/{contentId}/audio", content);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Get content by ID
        /// </summary>
        public async Task<YotoContent> GetContentAsync(string contentId)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                var response = await _httpClient.GetAsync($"{YOTO_API_BASE}/content/{contentId}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseContent);

                return new YotoContent
                {
                    Id = jsonResponse["id"]?.ToString(),
                    Title = jsonResponse["title"]?.ToString(),
                    Description = jsonResponse["description"]?.ToString(),
                    CreatedAt = jsonResponse["created_at"]?.ToObject<DateTime>() ?? DateTime.MinValue,
                    UpdatedAt = jsonResponse["updated_at"]?.ToObject<DateTime>() ?? DateTime.MinValue
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get content: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete content by ID
        /// </summary>
        public async Task<bool> DeleteContentAsync(string contentId)
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");

            try
            {
                var response = await _httpClient.DeleteAsync($"{YOTO_API_BASE}/content/{contentId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
