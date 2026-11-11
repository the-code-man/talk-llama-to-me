using Newtonsoft.Json.Linq;

namespace TalkLlama.Orchestrator.Services
{
    public class SttClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _sttServiceUrl;

        public SttClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _sttServiceUrl = configuration["SttServiceUrl"] ?? "http://localhost:5002/api/audio/transcribe";
        }

        public async Task<string> TranscribeAudioAsync(IFormFile audioFile)
        {
            using var form = new MultipartFormDataContent();
            using var stream = audioFile.OpenReadStream();
            using var streamContent = new StreamContent(stream);

            form.Add(streamContent, "file", audioFile.FileName);

            var response = await _httpClient.PostAsync(_sttServiceUrl, form);

            if (!response.IsSuccessStatusCode)
            {
                throw new System.Exception($"STT service failed with status code {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            // Parse text response from JSON (e.g., {"text": "..."}
            var jsonObject = JObject.Parse(jsonString);

            return jsonObject["text"]?.ToString() ?? string.Empty;
        }
    }
}