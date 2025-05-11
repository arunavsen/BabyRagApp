// Import for chat completion functionality from Semantic Kernel
using Microsoft.SemanticKernel.ChatCompletion;
// Import core Semantic Kernel functionality
using Microsoft.SemanticKernel;

namespace BabyRagApp.RagComponents.QdrantMemo
{
    // Class that implements a RAG (Retrieval Augmented Generation) chat system using Qdrant for vector storage
    public class RagChatRunnerWithQdrantMemoryStore
    {
        // The Semantic Kernel instance for orchestrating AI services
        private readonly Kernel _kernel;
        // The service responsible for generating chat completions
        private readonly IChatCompletionService _chatService;
        // Utility for generating embeddings from text
        private readonly EmbeddingGenerator _embedder;
        // The Qdrant memory store for vector storage and retrieval
        private readonly QdrantMemoryStore _memory;
        // The history of the ongoing chat conversation
        private readonly ChatHistory _chat;

        // Constructor that initializes all the required components
        public RagChatRunnerWithQdrantMemoryStore()
        {
            // Build a Semantic Kernel configured for Ollama (an LLM runner)
            _kernel = OllamaChatKernelBuilder.BuildKernel();
            // Get the chat completion service from the kernel
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
            // Create an embedding generator using the kernel
            _embedder = new EmbeddingGenerator(_kernel);
            // Initialize the Qdrant memory store
            _memory = new QdrantMemoryStore();
            // Create a new chat session with a system prompt
            _chat = OllamaChatCompletion.CreateNewChat("You are a helpful assistant. Use the provided knowledge if it's relevant.");
        }

        // Method to start the chat application
        public async Task RunAsync()
        {
            // Initialize the memory with the vector dimension size (768 for many embedding models)
            await _memory.InitializeAsync(768); // Assuming embedding size is 768
            // Load knowledge from a text file into the memory store
            await LoadKnowledgeAsync("knowledge.txt");

            // Display the ready message to the user
            Console.WriteLine("Chat ready. Type your message or 'exit' to quit.");

            // Start an infinite loop for the chat session
            while (true)
            {
                // Display the user prompt and get input
                Console.Write("User > ");
                var input = Console.ReadLine();

                // Skip empty inputs
                if (string.IsNullOrWhiteSpace(input)) continue;
                // Exit the loop if the user types "exit"
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // Generate an embedding for the user's query
                var queryEmbedding = await _embedder.GenerateEmbeddingAsync(input);
                // Search for relevant knowledge chunks using the query embedding
                var topChunks = await _memory.SearchAsync(queryEmbedding);

                // Join the retrieved chunks into a single context string
                var context = string.Join("\n", topChunks);
                // Create an augmented query that includes both the context and the original question
                var augmentedQuery = $"Use this context:\n{context}\n\nQuestion: {input}";

                // Add the augmented query to the chat history as a user message
                _chat.AddUserMessage(augmentedQuery);
                // Get a response from the chat completion service
                var response = await _chatService.GetChatMessageContentsAsync(_chat);
                // Extract the content from the response, or use a default if empty
                var answer = response.LastOrDefault()?.Content ?? "(No reply)";
                // Display the assistant's response
                Console.WriteLine($"Assistant > {answer}");

                // Add the assistant's response to the chat history
                _chat.AddAssistantMessage(answer);
            }
        }

        // Method to load knowledge from a file, chunk it, and store it with embeddings
        private async Task LoadKnowledgeAsync(string filePath)
        {
            // The commented line would use the provided filePath parameter
            //var text = await File.ReadAllTextAsync(filePath);
            // Instead, using a hardcoded path to the knowledge file
            var text = await File.ReadAllTextAsync(@"D:\Own Projects\BabyRagApp\knowledge.txt");
            // Chunk the text and generate embeddings for each chunk
            var chunks = await _embedder.ChunkAndEmbedAsync(text);
            // Add each chunk and its embedding to the memory store
            foreach (var (chunk, embedding) in chunks)
                await _memory.AddAsync(chunk, embedding);

            // Display confirmation of how many chunks were loaded
            Console.WriteLine($"✅ Loaded {chunks.Count} knowledge chunks.");
        }
    }
}
