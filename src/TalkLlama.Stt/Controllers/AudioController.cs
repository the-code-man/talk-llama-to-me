using Microsoft.AspNetCore.Mvc;
using TalkLlama.Stt.Services;

namespace TalkLlama.Stt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController(TranscriptionService transcriptionService) : ControllerBase
    {
        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No audio file uploaded." });
            }

            var stream = file.OpenReadStream();

            if (stream.Length < 44) // WAV header is 44 bytes minimum
            {
                return BadRequest(new { error = "Invalid audio file length." });
            }

            try
            {
                string transcript = await transcriptionService.TranscribeAsync(stream);

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    return Ok(new { text = "", warning = "No speech detected in audio" });
                }
                else
                {
                    return Ok(new { text = transcript });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
