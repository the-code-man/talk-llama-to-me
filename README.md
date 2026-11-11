# 🗣️ talk-llama-to-me

A lightweight, fully containerized, real-time local speech orchestrator running entirely on your machine. No cloud APIs, zero data leakage, and low-latency voice interactions.

---

## 💻 Hardware & Host Specifications

| Parameter | Configuration |
| --- | --- |
| **Processor** | AMD Ryzen 7 7445HS @ 3.20 GHz |
| **Memory** | 16.0 GB RAM (15.3 GB usable) |
| **Primary GPU** | NVIDIA GeForce RTX 4050 Laptop GPU (6 GB VRAM) |
| **Secondary iGPU** | AMD Radeon 740M |
| **OS Environment** | Windows 11 with WSL 2 (Ubuntu backend) |

---

## 🏗️ Architecture & Component Pipeline

The orchestrator cascades three local components over an asynchronous containerized network:

[ Microphone ] ➔ 🎙️ STT (faster-whisper) ➔ 🧠 LLM (Ollama / Llama 3.1 8B Q4) ➔ 🔊 TTS (Kokoro-ONNX / Piper) ➔ [ Speaker ]

1. **Ear (STT):** `faster-whisper` (or `whisper.cpp`) converts raw microphone audio to text.
2. **Brain (LLM):** Ollama container hosting `Llama-3.1-8B-Instruct` (4-bit quantized GGUF Q4_K_M, taking ~4.5–5 GB VRAM).
3. **Mouth (TTS):** `Kokoro-ONNX` (or `Piper TTS`) for low-latency token-to-speech audio streaming.

---

## 🎯 Hardware & VRAM Allocation Strategy

Running a multi-model pipeline within a **6 GB VRAM** limit requires strict memory budgeting:

+----------------------------------------------------------------------+
|                  Total GPU VRAM Available: 6.0 GB                    |
+----------------------------------------------------------------------+
|  [LLM Core]       Llama 3.1 8B Instruct (Q4_K_M GGUF)  ~4.2 - 4.5 GB |
|  [STT Engine]     faster-whisper (small.en / float16)  ~0.8 - 1.0 GB |
|  [TTS Engine]     Kokoro-82M (ONNX Runtime / CUDA)     ~0.3 - 0.5 GB |
+----------------------------------------------------------------------+
|  VRAM Buffer      Headroom for Context KV Cache        ~0.2 - 0.5 GB |
+----------------------------------------------------------------------+

## 🛠️ Prerequisites & Setup

### 1. Host Drivers & Container Toolkit
* **NVIDIA Host Drivers:** Install the latest NVIDIA GeForce Game Ready/Studio drivers on Windows.
* **Docker Desktop:** Configured to use the **WSL 2 engine** backend.
* **NVIDIA Container Toolkit (WSL 2 Ubuntu):**

```bash
curl -fsSL [https://nvidia.github.io/libnvidia-container/gpgkey](https://nvidia.github.io/libnvidia-container/gpgkey) | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg
curl -fsSL [https://nvidia.github.io/libnvidia-container/stable-deb/nvidia-container-toolkit.list](https://nvidia.github.io/libnvidia-container/stable-deb/nvidia-container-toolkit.list) | \
  sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | \
  sudo tee /etc/apt/sources.list.d/nvidia-container-toolkit.list

sudo apt-get update && sudo apt-get install -y nvidia-container-toolkit
sudo nvidia-ctk runtime configure --runtime=docker
sudo systemctl restart docker
```

Test if the GPU passthrough works:

```bash
docker run --rm --gpus all nvidia/cuda:12.0.0-base-ubuntu22.04 nvidia-smi 
```

If it prints out your RTX 4050 details and VRAM usage table: Great! Docker Desktop or your environment already has the bridge configured, and you are good to go.

If it errors out, throws a nvidia driver error, or says --gpus is an unknown flag: You need to install/configure the NVIDIA Container Toolkit so Docker can pass instructions to your GPU.

## Add Speech-to-Text (STT) Engine

### Pre-requisites

1. Install Whisper.net and GPU Acceleration Packages

```bash
dotnet add package Whisper.net
dotnet add package Whisper.net.Runtime.Cuda.Windows
```

The CUDA runtime package ensures Whisper executes efficiently on your laptop's GPU instead of lagging on the CPU

