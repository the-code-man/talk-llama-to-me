using Microsoft.AspNetCore.Mvc;
using TalkLlama.Orchestrator.Models;
using TalkLlama.Orchestrator.Services;

namespace TalkLlama.Orchestrator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromptController(AIService _ai, SttClientService _sttClient) : ControllerBase
    {
        [HttpPost]
        public Task<IActionResult> PostPrompt([FromBody] PromptRequest request)
        {
            return PostPrompt(request, true);
        }

        [HttpPost("audio")]
        public Task<IActionResult> ProcessAudioPrompt(IFormFile file)
        {
            return ProcessAudioPrompt(file, true);
        }

        [HttpPost("stateless")]
        public Task<IActionResult> PostPromptStateless([FromBody] PromptRequest request)
        {
            return PostPrompt(request, false);
        }

        [HttpPost("stateless/audio")]
        public Task<IActionResult> ProcessAudioPromptStateless(IFormFile file)
        {
            return ProcessAudioPrompt(file, false);
        }

        private async Task<IActionResult> PostPrompt(PromptRequest request, bool isStateful)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest(new
                {
                    error = "Prompt cannot be empty."
                });
            }

            var responseText = await _ai.GenerateResponseAsync(request.Prompt, isStateful);

            return Ok(new
            {
                prompt = request.Prompt,
                response = responseText
            });
        }

        private async Task<IActionResult> ProcessAudioPrompt(IFormFile file, bool isStateful)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No audio file provided." });
            }

            try
            {
                // 1. Send audio to talk-llama-stt microservice
                string transcript = await _sttClient.TranscribeAudioAsync(file);

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    return BadRequest(new { error = "Could not transcribe any speech from the provided audio." });
                }

                var responseText = await _ai.GenerateResponseAsync(transcript, isStateful);

                return Ok(new
                {
                    transcript,
                    response = responseText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}