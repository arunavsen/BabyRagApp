using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BabyRagApp.RagComponents;

public class RagChatRunner
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly EmbeddingGenerator _embedder;
    private readonly SimpleMemoryStore _memory = new();
    private readonly ChatHistory _chat;

    public RagChatRunner()
    {
        _kernel = OllamaChatKernelBuilder.BuildKernel();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        _embedder = new EmbeddingGenerator(_kernel);
        _chat = OllamaChatCompletion.CreateNewChat("You are a helpful assistant. Use the provided knowledge if it's relevant.");
    }

    public async Task RunAsync()
    {
        await LoadKnowledgeAsync("knowledge.txt");

        Console.WriteLine("Chat ready. Type your message or 'exit' to quit.");

        while (true)
        {
            Console.Write("User > ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

            var queryEmbedding = await _embedder.GenerateEmbeddingAsync(input);
            var topChunks = _memory.Search(queryEmbedding);

            var context = string.Join("\n", topChunks);
            var augmentedQuery = $"Use this context:\n{context}\n\nQuestion: {input}";

            _chat.AddUserMessage(augmentedQuery);
            var response = await _chatService.GetChatMessageContentsAsync(_chat);
            var answer = response.LastOrDefault()?.Content ?? "(No reply)";
            Console.WriteLine($"Assistant > {answer}");

            _chat.AddAssistantMessage(answer);
        }
    }

    private async Task LoadKnowledgeAsync(string filePath)
    {
        //if (!File.Exists(filePath))
        //{
        //    Console.WriteLine($"⚠️ Knowledge file not found: {filePath}");
        //    return;
        //}
        
        var text = await File.ReadAllTextAsync(@"D:\Own Projects\BabyRagApp\knowledge.txt");
        var chunks = await _embedder.ChunkAndEmbedAsync(text);
        foreach (var (chunk, embedding) in chunks)
            _memory.Add(chunk, embedding);

        Console.WriteLine($"✅ Loaded {chunks.Count} knowledge chunks.");
    }
}
