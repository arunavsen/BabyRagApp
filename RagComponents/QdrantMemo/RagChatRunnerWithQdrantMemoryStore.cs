using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyRagApp.RagComponents.QdrantMemo
{
    public class RagChatRunnerWithQdrantMemoryStore
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly EmbeddingGenerator _embedder;
        private readonly QdrantMemoryStore _memory;
        private readonly ChatHistory _chat;

        public RagChatRunnerWithQdrantMemoryStore()
        {
            _kernel = OllamaChatKernelBuilder.BuildKernel();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
            _embedder = new EmbeddingGenerator(_kernel);
            _memory = new QdrantMemoryStore();
            _chat = OllamaChatCompletion.CreateNewChat("You are a helpful assistant. Use the provided knowledge if it's relevant.");
        }

        public async Task RunAsync()
        {
            await _memory.InitializeAsync(768); // Assuming embedding size is 768
            await LoadKnowledgeAsync("knowledge.txt");

            Console.WriteLine("Chat ready. Type your message or 'exit' to quit.");

            while (true)
            {
                Console.Write("User > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var queryEmbedding = await _embedder.GenerateEmbeddingAsync(input);
                var topChunks = await _memory.SearchAsync(queryEmbedding);

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
            //var text = await File.ReadAllTextAsync(filePath);
            var text = await File.ReadAllTextAsync(@"D:\Own Projects\BabyRagApp\knowledge.txt");
            var chunks = await _embedder.ChunkAndEmbedAsync(text);
            foreach (var (chunk, embedding) in chunks)
                await _memory.AddAsync(chunk, embedding);

            Console.WriteLine($"✅ Loaded {chunks.Count} knowledge chunks.");
        }
    }
}
