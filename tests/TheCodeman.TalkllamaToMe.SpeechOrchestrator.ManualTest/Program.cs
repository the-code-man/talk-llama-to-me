using System;
using System.Threading.Tasks;
using OllamaSharp;

class Program
{
    static async Task Main(string[] args)
    {
        // Point to the containerized Ollama instance
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);

        //Select the model we pulled into the container
        ollama.SelectedModel = "llama3.1:8b";

        Console.WriteLine("🚀 talk-llama-to-me C# Orchestrator Active!");
        Console.WriteLine("Type your prompt below (or type 'exit' to quit). {0}", Environment.NewLine);

        while (true)
        {
            Console.Write("👤 User: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.Write("🦙 Llama 3.1: ");

            //Create a chat session with a consise system prompt
            var chat = new Chat(ollama, "You are a helpful voice assistant. Keep answers short and conversational (1-2 sentences)");

            // Stream tokens asynchrounously as they generate from the GPU container\
            await foreach (var token in chat.SendAsync(input))
            {
                Console.Write(token);
            }

            Console.WriteLine(Environment.NewLine);
        }
    }
}