Launch the Stack & Pull Llama 3.1

Run the compose file

```bash
docker compose up -d
```

Next, pull the optimized 8-bit or 4-bit Llama 3.1 model into your running container:
```bash
docker exec -it talk-ollama-brain ollama pull llama3.1:8b "Hello!"
```

Once the download finishes, verify that Ollama successfully recognized your RTX 4050 GPU:
```bash
docker exec -it talk-ollama-brain ollama ps
```