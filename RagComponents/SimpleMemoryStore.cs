namespace BabyRagApp.RagComponents
{

    public class SimpleMemoryStore
    {
        private readonly List<MemoryItem> _memory = new();

        public void Add(string text, float[] embedding)
        {
            _memory.Add(new MemoryItem { Text = text, Embedding = embedding });
        }

        public List<string> Search(float[] queryEmbedding, int topN = 3)
        {
            return _memory
                .Select(m => new { m.Text, Score = CosineSimilarity(queryEmbedding, m.Embedding) })
                .OrderByDescending(m => m.Score)
                .Take(topN)
                .Select(m => m.Text)
                .ToList();
        }

        private static float CosineSimilarity(float[] vec1, float[] vec2)
        {
            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                dot += vec1[i] * vec2[i];
                normA += vec1[i] * vec1[i];
                normB += vec2[i] * vec2[i];
            }
            return dot / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB) + 1e-5f);
        }
    }
}
