using Whisper.net;
using Whisper.net.Ggml;

namespace TalkLlama.Stt.Services
{
    public class TranscriptionService
    {
        private readonly string _modelFileName;
        private bool _isModelLoaded = false;

        public TranscriptionService(IConfiguration configuration)
        {
            _modelFileName = configuration["SttSettings:ModelFileName"] ?? "ggml-base.bin";
        }

        public async Task EnsureModelInitializedAsync()
        {
            if (_isModelLoaded && File.Exists(_modelFileName)) return;

            if (!File.Exists(_modelFileName))
            {
                using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
                using var fileWriter = File.OpenWrite(_modelFileName);
                await modelStream.CopyToAsync(fileWriter);
            }

            _isModelLoaded = true;
        }

        public async Task<string> TranscribeAsync(Stream audioStream)
        {
            await EnsureModelInitializedAsync();

            using var whisperFactory = WhisperFactory.FromPath(_modelFileName);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            string fullText = string.Empty;

            await foreach (var result in processor.ProcessAsync(audioStream))
            {
                fullText += result.Text + " ";
            }

            return fullText.Trim();
        }
    }
}
