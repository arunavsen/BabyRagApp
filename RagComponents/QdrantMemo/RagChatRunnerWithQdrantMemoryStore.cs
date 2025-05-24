using Microsoft.SemanticKernel.ChatCompletion;
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

                //Use the LLM to rewrite the query before embedding it.
                var rewrittenQuery = await GetRewrittenQueryAsync(input);

                // Generate an embedding for the user's query
                var queryEmbedding = await _embedder.GenerateEmbeddingAsync(rewrittenQuery);

                // Step 1: Initial retrieval using vector search
                // Retrieve more chunks than we need for re-ranking
                int initialK = (int)(RagSettings.VectorSearch.TopN * RagSettings.VectorSearch.ReRankingMultiplier);
                var initialChunks = await _memory.SearchAsync(queryEmbedding, initialK);

                // Step 2: Re-rank chunks using LLM if we have enough chunks
                List<string> topChunks;
                if (initialChunks.Count > 0)
                {
                    Console.WriteLine($"Re-ranking {initialChunks.Count} chunks...");
                    // Apply semantic re-ranking to improve relevance
                    topChunks = await ReRankChunksAsync(rewrittenQuery, initialChunks);
                    // Take only the top N chunks after re-ranking
                    topChunks = topChunks.Take(RagSettings.VectorSearch.TopN).ToList();
                }
                else
                {
                    topChunks = initialChunks;
                }

                // Join the retrieved chunks into a single context string
                var context = string.Join("\n\n", topChunks);
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


        public async Task<List<string>> GetTopKRelevantDocsAsync(string query, int k = RagSettings.VectorSearch.TopN)
        {
            // Generate embedding for the query
            var queryEmbedding = await _embedder.GenerateEmbeddingAsync(query);

            // Step 1: Initial retrieval with vector search (get more than we need)
            int initialK = (int)(k * RagSettings.VectorSearch.ReRankingMultiplier);
            var initialChunks = await _memory.SearchAsync(queryEmbedding, initialK);

            // Step 2: Re-rank if we have enough chunks
            if (initialChunks.Count > 0)
            {
                // Apply semantic re-ranking
                var rerankedChunks = await ReRankChunksAsync(query, initialChunks);
                // Return only the requested number after re-ranking
                return rerankedChunks.Take(k).ToList();
            }

            return initialChunks;
        }

        
        public async Task<string> GetRewrittenQueryAsync(string original)
        {
            var prompt = $"Rewrite this question clearly and fully so that it is self-contained: {original}";
            var reply = await _chatService.GetChatMessageContentsAsync(prompt);
            return reply.LastOrDefault()?.Content ?? original;
        }

        /// <summary>
        /// Performs semantic re-ranking of retrieved chunks using the LLM to score relevance.
        /// 
        /// This approach addresses the "embedding space limitation" where vector similarity
        /// doesn't always capture true semantic relevance. The LLM evaluates each chunk
        /// against the query and assigns a relevance score, which allows for a more
        /// accurate ranking beyond what embedding similarity can provide.
        /// </summary>
        /// <param name="query">The user's original query</param>
        /// <param name="chunks">List of text chunks to re-rank</param>
        /// <returns>Re-ranked list of chunks with most relevant first</returns>

        private async Task<List<string>> ReRankChunksAsync(string query, List<string> chunks)
        {
            var scored = new List<(string chunk, double score)>();

            foreach (var chunk in chunks)
            {
                // Clearer prompt that encourages just returning a numeric score
                var prompt = $"Rate the relevance of this context to the question on a scale from 0 to 1. Return ONLY the numeric score without any explanation or additional text.\n\nQuestion: {query}\n\nContext: {chunk}\n\nScore (just the number):";
                var reply = await _chatService.GetChatMessageContentsAsync(prompt);
                var scoreStr = reply.LastOrDefault()?.Content?.Trim();

                double score = 0;
                // Try to parse just the first number in the response if it contains text
                if (!string.IsNullOrEmpty(scoreStr))
                {
                    // Extract the first decimal number from the string
                    var match = System.Text.RegularExpressions.Regex.Match(scoreStr, @"0*\.?\d+");
                    if (match.Success && double.TryParse(match.Value, out score))
                    {
                        scored.Add((chunk, score));
                    }
                    else
                    {
                        // If parsing fails, add with a default score to avoid losing chunks
                        scored.Add((chunk, 0.5));
                        Console.WriteLine($"Warning: Could not parse score from: {scoreStr}");
                    }
                }
                else
                {
                    // Add with default score if no response
                    scored.Add((chunk, 0.5));
                }
            }

            // Even if scoring fails, ensure we return the chunks in their original order
            if (scored.Count == 0)
            {
                return chunks;
            }

            return scored.OrderByDescending(s => s.score).Select(s => s.chunk).ToList();
        }

    }
}
