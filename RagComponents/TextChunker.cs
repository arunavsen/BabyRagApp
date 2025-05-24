namespace BabyRagApp.RagComponents
{
    /// <summary>
    /// Provides functionality for breaking down large texts into smaller, overlapping chunks
    /// to improve retrieval performance in RAG (Retrieval Augmented Generation) applications.
    /// </summary>
    public static class TextChunker
    {
        /// <summary>
        /// Splits a text into smaller, potentially overlapping chunks based on word boundaries.
        /// 
        /// This implementation:
        /// - Divides text by words rather than characters or sentences
        /// - Creates chunks of consistent size (measured in words)
        /// - Supports configurable overlap between chunks to maintain context continuity
        /// - Uses settings from RagSettings for default values with option to override
        /// 
        /// The overlap helps ensure that semantic concepts that might be split between
        /// chunks remain discoverable, improving retrieval quality in vector searches.
        /// </summary>
        /// <param name="text">The input text to chunk</param>
        /// <param name="chunkSize">Optional: Number of words per chunk (defaults to RagSettings value)</param>
        /// <param name="overlap">Optional: Number of words to overlap between chunks (defaults to RagSettings value)</param>
        /// <returns>A list of text chunks</returns>
        public static List<string> ChunkText(string text, int? chunkSize = null, int? overlap = null)
        {
            // Use provided values or fall back to settings
            int actualChunkSize = chunkSize ?? RagSettings.TextChunking.ChunkSize;
            int actualOverlap = overlap ?? RagSettings.TextChunking.Overlap;
            
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();

            for (int i = 0; i < words.Length; i += actualChunkSize - actualOverlap)
            {
                var chunk = string.Join(" ", words.Skip(i).Take(actualChunkSize));
                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);
            }

            return chunks;
        }
    }
}