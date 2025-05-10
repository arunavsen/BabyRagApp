using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;

namespace BabyRagApp.RagComponents
{
    public class EmbeddingGenerator
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public EmbeddingGenerator(Kernel kernel)
        {
            _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
            return embedding.ToArray();
        }

        public async Task<List<(string, float[])>> ChunkAndEmbedAsync(string text)
        {
            var chunks = TextChunker.ChunkText(text);
            var result = new List<(string, float[])>();

            foreach (var chunk in chunks)
            {
                var embedding = await GenerateEmbeddingAsync(chunk);
                result.Add((chunk, embedding));
            }

            return result;
        }
    }
}
