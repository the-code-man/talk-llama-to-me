# Talk Llama to Me

A local voice-assistant proof of concept. The WPF desktop client records microphone audio, the ASP.NET Core orchestrator sends it to the local speech-to-text service and Ollama, then displays the transcript and response.

The current implementation runs the .NET services on the host. The supplied Docker Compose file runs **Ollama only**; speech synthesis is not implemented yet.

## What is implemented

```text
Microphone
  -> WPF desktop client (16 kHz, 16-bit, mono WAV)
  -> Orchestrator (HTTP :5000)
       -> STT service / Whisper.net (HTTP :5002)
       -> Ollama / llama3.1:8b (HTTP :11434)
  -> transcript and response shown in the desktop client
```

The STT service downloads the Whisper `base` model on first use if it is not already present. The TTS service exposes a synthesis endpoint, but currently returns placeholder bytes rather than synthesized speech; the orchestrator and desktop client do not call it.

## Prerequisites

- **Windows 10/11, 64-bit** for the documented setup and WPF desktop client.
- **.NET 10 SDK** to restore, build, and run the projects.
- **Git** to clone the repository.
- **Ollama**, either using the included Docker Compose setup or a local Ollama installation.
- An internet connection for the initial NuGet restore and model downloads.
- For GPU acceleration:
  - An NVIDIA GPU and current NVIDIA Windows driver.
  - Docker Desktop using its WSL 2 backend when running the included GPU-enabled Ollama Compose service.
  - Whisper.net includes the Windows CUDA runtime package. GPU use depends on compatible NVIDIA drivers and runtime setup; CPU execution may be slower.

The repository's development machine has 16 GB RAM and an NVIDIA RTX 4050 Laptop GPU with 6 GB VRAM. These are reference specifications, not verified minimum requirements. Model performance and memory use depend on the selected Ollama model and hardware.

## Infrastructure setup

### Start Ollama with Docker Compose

The compose file is `tests/docker-compose.yml`. It publishes Ollama on port `11434`, persists downloaded models in a Docker volume, and requests one NVIDIA GPU.

From the repository root, start the container and download the configured model:

```powershell
docker compose -f .\tests\docker-compose.yml up -d
docker exec -it talk-ollama-brain ollama pull llama3.1:8b
docker exec -it talk-ollama-brain ollama list
```

Check the Windows NVIDIA driver with `nvidia-smi`, then confirm the container is running with `docker ps`. If Docker cannot satisfy the Compose GPU reservation, configure Docker Desktop/WSL 2 GPU support or run Ollama natively and adjust the Compose GPU reservation for your environment. The orchestrator expects Ollama at `http://localhost:11434` by default.

### Build the solution

From the repository root:

```powershell
dotnet restore .\TalkLlama.slnx
dotnet build .\TalkLlama.slnx
```

On the first audio transcription, Whisper.net downloads its `base` model into the STT service's working directory. The downloaded model is ignored by Git.

## Run the services

Start each service in a separate PowerShell terminal from the repository root. Start Ollama first.

```powershell
dotnet run --project .\src\TalkLlama.Stt\TalkLlama.Stt.csproj --launch-profile http
```

```powershell
dotnet run --project .\src\TalkLlama.Orchestrator\TalkLlama.Orchestrator.csproj --launch-profile http
```

The TTS API is optional and is currently only a placeholder:

```powershell
dotnet run --project .\src\TalkLlama.Tts\TalkLlama.Tts.csproj --launch-profile http
```

The development launch profiles use these addresses:

| Service | Address | Swagger | Health |
| --- | --- | --- | --- |
| Orchestrator | `http://localhost:5000` | `/swagger` | `/healthz` |
| STT | `http://localhost:5002` | `/swagger` | `/healthz` |
| TTS | `http://localhost:5004` | `/swagger` | `/healthz` |
| Ollama | `http://localhost:11434` | — | — |

The orchestrator settings are in `src/TalkLlama.Orchestrator/appsettings.json`. Defaults are `AiSettings:ServiceHost` = `http://localhost:11434`, `AiSettings:ModelName` = `llama3.1:8b`, and `SttServiceUrl` = `http://localhost:5002/api/audio/transcribe`. Override them with .NET environment variables if your services use different addresses; for example, `AiSettings__ServiceHost` maps to `AiSettings:ServiceHost`.

## Run the desktop client

With the orchestrator, STT service, and Ollama running:

```powershell
dotnet run --project .\src\Talkllama.DesktopClient\Talkllama.DesktopClient.csproj
```

Hold the **Hold to Speak** button while speaking, then release it to send the recording. The client posts a WAV file to `http://localhost:5000/api/prompt/audio` and displays the returned transcript and response.

## API and manual testing

- `POST /api/prompt` on the orchestrator accepts JSON such as `{"prompt":"Hello"}`.
- `POST /api/prompt/audio` accepts multipart form data with a `file` field and returns a transcript and LLM response.
- `POST /api/prompt/stateless` and `POST /api/prompt/stateless/audio` provide stateless prompt variants.
- `POST /api/audio/transcribe` on the STT service accepts multipart form data with a `file` field and returns `{"text":"..."}`.
- `POST /api/tts/synthesize` accepts `{"text":"..."}`, but is a placeholder and does not produce real speech.

The standard prompt routes use an in-memory chat session created per request; conversation history is not persisted across requests.

The sample PowerShell scripts use `scripts/test/test_audio.wav`. Run them from that directory so the relative audio-file path resolves:

```powershell
Push-Location .\scripts\test
.\test-stt.ps1
.\test-orchestrator.ps1
Pop-Location
```

`test-stt.ps1` requires the STT service; `test-orchestrator.ps1` requires the STT service, orchestrator, and Ollama with the configured model.

## Project layout

| Path | Purpose |
| --- | --- |
| `src/Talkllama.DesktopClient/` | WPF microphone recording client using NAudio. |
| `src/TalkLlama.Orchestrator/` | ASP.NET Core prompt/audio API; calls STT and Ollama. |
| `src/TalkLlama.Stt/` | ASP.NET Core transcription API using Whisper.net. |
| `src/TalkLlama.Tts/` | ASP.NET Core TTS API scaffold; synthesis is not implemented. |
| `tests/docker-compose.yml` | Ollama container and persistent model storage. |
| `scripts/test/` | PowerShell smoke-test scripts and sample audio. |
