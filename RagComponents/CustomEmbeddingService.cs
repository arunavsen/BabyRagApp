using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System.Net.Http.Json;
using System.Text.Json;

namespace BabyRagApp.RagComponents
{
    public class CustomEmbeddingService : ITextEmbeddingGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public CustomEmbeddingService(string baseUrl = "http://localhost:11434", string modelName = "nomic-embed-text")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _modelName = modelName;
        }

        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var results = new List<ReadOnlyMemory<float>>();

            foreach (var item in data)
            {
                var request = new
                {
                    model = _modelName,
                    prompt = item,
                    stream = false
                };

                var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var embedding = doc.RootElement.GetProperty("embedding").EnumerateArray().Select(e => e.GetSingle()).ToArray();

                results.Add(new ReadOnlyMemory<float>(embedding));
            }

            return results;
        }

        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var list = await GenerateEmbeddingsAsync(new List<string> { text }, kernel, cancellationToken);
            return list[0];
        }
    }
}
