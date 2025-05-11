using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyRagApp.RagComponents.QdrantMemo
{
    public class QdrantMemoryStore
    {
        private readonly QdrantClient _client;
        private readonly string _collectionName = "knowledge_collection";

        public QdrantMemoryStore()
        {
            var channel = QdrantChannel.ForAddress("http://localhost:6334");
            var grpcClient = new QdrantGrpcClient(channel);
            _client = new QdrantClient(grpcClient);
        }

        public async Task InitializeAsync(int vectorSize)
        {
            var collections = await _client.ListCollectionsAsync();
            if (!collections.Any(c => c == _collectionName))
            {
                await _client.CreateCollectionAsync(_collectionName, new VectorParams
                {
                    Size = (uint)vectorSize,
                    Distance = Distance.Cosine
                });
            }
        }

        public async Task AddAsync(string text, float[] embedding)
        {
            // Create a unique ID for the point
            var pointId = new PointId { Uuid = Guid.NewGuid().ToString() };
            
            // Create payload with the text
            var payload = new Dictionary<string, Value>
            {
                { "text", new Value { StringValue = text } }
            };
            
            // Create the point with vector data
            var vectors = new Vectors();
            var vector = new Vector();
            foreach (var value in embedding)
            {
                vector.Data.Add(value);
            }
            vectors.Vector = vector;
            
            // Create the point structure
            var point = new PointStruct
            {
                Id = pointId,
                Vectors = vectors,
            };
            
            // Add the payload items to the point's payload
            foreach (var kvp in payload)
            {
                point.Payload.Add(kvp.Key, kvp.Value);
            }
            
            // Upsert the point into the collection
            await _client.UpsertAsync(_collectionName, new[] { point });
        }

        public async Task<List<string>> SearchAsync(float[] queryEmbedding, int topN = 3)
        {
            // Convert the embedding array to ReadOnlyMemory<float> for the search
            var vector = new ReadOnlyMemory<float>(queryEmbedding);
            
            // Search for similar vectors
            var searchResults = await _client.SearchAsync(
                _collectionName,
                vector,
                limit: (uint)topN
            );
            
            // Extract the text from each result
            return searchResults
                .Select(result => result.Payload["text"].StringValue)
                .ToList();
        }
    }
}
