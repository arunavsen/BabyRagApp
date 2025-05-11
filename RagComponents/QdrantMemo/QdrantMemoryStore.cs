// Import the Qdrant client library, which is a vector database client
using Qdrant.Client;
// Import the gRPC client for Qdrant, used for communication with the Qdrant server
using Qdrant.Client.Grpc;


namespace BabyRagApp.RagComponents.QdrantMemo
{
    // Define a class that will handle vector storage and retrieval using Qdrant
    public class QdrantMemoryStore
    {
        // Field to store the Qdrant client connection
        private readonly QdrantClient _client;
        // Name of the collection in Qdrant where vectors will be stored
        private readonly string _collectionName = "knowledge_collection";

        // Constructor that initializes the Qdrant client
        public QdrantMemoryStore()
        {
            // Create a gRPC channel to connect to the local Qdrant server
            var channel = QdrantChannel.ForAddress("http://localhost:6334");
            // Create a gRPC client using the channel
            var grpcClient = new QdrantGrpcClient(channel);
            // Create the Qdrant client using the gRPC client
            _client = new QdrantClient(grpcClient);
        }

        // Method to initialize the vector collection with the specified dimensions
        public async Task InitializeAsync(int vectorSize)
        {
            // Get a list of all collections in the Qdrant database
            var collections = await _client.ListCollectionsAsync();
            // Check if our collection already exists
            if (!collections.Any(c => c == _collectionName))
            {
                // If the collection doesn't exist, create it with the specified vector size
                await _client.CreateCollectionAsync(_collectionName, new VectorParams
                {
                    // Set the dimension size of the vectors
                    Size = (uint)vectorSize,
                    // Use cosine similarity as the distance metric
                    Distance = Distance.Cosine
                });
            }
        }

        // Method to add a text chunk and its embedding to the vector database
        public async Task AddAsync(string text, float[] embedding)
        {
            // Create a unique ID for the point
            var pointId = new PointId { Uuid = Guid.NewGuid().ToString() };
            
            // Create payload with the text - this will store the original text alongside the vector
            var payload = new Dictionary<string, Value>
            {
                { "text", new Value { StringValue = text } }
            };
            
            // Create the point with vector data
            var vectors = new Vectors();
            var vector = new Vector();
            // Add each float value from the embedding array to the vector
            foreach (var value in embedding)
            {
                vector.Data.Add(value);
            }
            // Assign the vector to the vectors container
            vectors.Vector = vector;
            
            // Create the point structure with ID and vector data
            var point = new PointStruct
            {
                Id = pointId,
                Vectors = vectors,
            };
            
            // Add the payload items (text) to the point's payload dictionary
            foreach (var kvp in payload)
            {
                point.Payload.Add(kvp.Key, kvp.Value);
            }
            
            // Upsert (update or insert) the point into the collection
            await _client.UpsertAsync(_collectionName, new[] { point });
        }

        // Method to search for similar text chunks using a query embedding
        public async Task<List<string>> SearchAsync(float[] queryEmbedding, int topN = 3)
        {
            // Convert the embedding array to ReadOnlyMemory<float> for the search
            var vector = new ReadOnlyMemory<float>(queryEmbedding);
            
            // Search for similar vectors in the collection, limiting to topN results
            var searchResults = await _client.SearchAsync(
                _collectionName,
                vector,
                limit: (uint)topN
            );
            
            // Extract the text from each search result and return as a list
            return searchResults
                .Select(result => result.Payload["text"].StringValue)
                .ToList();
        }
    }
}
