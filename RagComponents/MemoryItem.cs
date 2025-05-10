namespace BabyRagApp.RagComponents
{
    public class MemoryItem
    {
        public string Text { get; set; } = "";
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
