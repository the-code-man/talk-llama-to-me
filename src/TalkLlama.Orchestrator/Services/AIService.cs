using OllamaSharp;

namespace TalkLlama.Orchestrator.Services
{
    public class AIService
    {
        private readonly IOllamaApiClient _client;
        private readonly Chat _chatSession;

        public AIService(IConfiguration configuration)
        {
            var ollamaUri = configuration["AiSettings:ServiceHost"] ?? "http://localhost:11434";

            _client = new OllamaApiClient(uri: new Uri(ollamaUri))
            {
                // Default to Llama 3 or whatever model pulled locally
                SelectedModel = configuration["AiSettings:ModelName"] ?? "llama3.1:8b"
            };

            // Initialize chat session once with system prompt and history memory
            _chatSession = new Chat(_client, "You are a helpful, consise voice assistant. Keep answers short and conversational.");
        }


        /// <summary>
        /// Retuns a response from the AI model based on the provided prompt.
        /// </summary>
        /// <param name="prompt">The user's prompt</param>
        /// <param name="isStateful">Indicates whether to use a stateful chat session</param>
        /// <returns>The AI-generated response</returns>
        public Task<string> GenerateResponseAsync(string prompt, bool isStateful) => isStateful
            ? GenerateResponseStateful(prompt)
            : GenerateResponseStateless(prompt);

        /// <summary>
        /// Stateful Chat Session: It instantiates a new Chat object, 
        /// which wraps the client with a system prompt ("You are a 
        /// helpful, concise voice assistant...") and maintains conversation 
        /// history/context in memory for the lifetime of that Chat instance.
        /// 
        /// Behavior: Every message sent via chat.SendAsync(prompt) appends 
        /// both the user prompt and the model's response to an internal 
        /// history list, meaning subsequent calls on that same chat object 
        /// would remember past turns. (Note: Because a new Chat object is 
        /// created locally inside the method scope every time it's called, 
        /// that history is currently discarded right after the method returns).
        /// </summary>
        /// <param name="prompt">The user's prompt</param>
        /// <returns>The AI-generated response</returns>
        private async Task<string> GenerateResponseStateful(string prompt)
        {
            string responseText = string.Empty; 

            await foreach (var token in _chatSession.SendAsync(prompt))
            {
                responseText += token;
            }

            return responseText;
        }

        /// <summary>
        /// Stateless Generation: It uses _client.GenerateAsync(prompt) 
        /// directly, which is a one-off prompt completion.
        /// 
        /// Behavior: It sends a standalone text prompt to the selected 
        /// model without any persistent conversation history or system 
        /// prompt context attached.
        /// </summary>
        /// <param name="prompt">The user's prompt</param>
        /// <returns>The AI-generated response</returns>
        private async Task<string> GenerateResponseStateless(string prompt)
        {
            string responseText = string.Empty;

            // Stream or aggregate tokens from Ollama
            await foreach (var streamToken in _client.GenerateAsync(prompt))
            {
                responseText += streamToken?.Response;
            }

            return responseText;
        }
    }
}
