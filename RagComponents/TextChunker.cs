using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyRagApp.RagComponents
{
    public static class TextChunker
    {
        public static List<string> ChunkText(string text, int maxTokens = 200)
        {
            var chunks = new List<string>();
            var sentences = text.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
            var currentChunk = "";

            foreach (var sentence in sentences)
            {
                if ((currentChunk + sentence).Length > maxTokens)
                {
                    chunks.Add(currentChunk.Trim());
                    currentChunk = "";
                }
                currentChunk += sentence + ". ";
            }

            if (!string.IsNullOrWhiteSpace(currentChunk))
                chunks.Add(currentChunk.Trim());

            return chunks;
        }
    }
}