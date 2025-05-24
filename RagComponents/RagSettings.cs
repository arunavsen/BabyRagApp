using System;

namespace BabyRagApp.RagComponents
{
    /// <summary>
    /// Class containing settings for RAG components
    /// </summary>
    public static class RagSettings
    {
        /// <summary>
        /// Text chunking settings
        /// </summary>
        public static class TextChunking
        {
            /// <summary>
            /// Size of text chunks in words
            /// </summary>
            public const int ChunkSize = 100;
            
            /// <summary>
            /// Number of words to overlap between chunks
            /// </summary>
            public const int Overlap = 25;
        }

        /// <summary>
        /// Vector search settings for retrieving relevant documents
        /// </summary>
        public static class VectorSearch
        {
            /// <summary>
            /// Maximum number of documents to retrieve in a search
            /// </summary>
            public const int TopN = 5;
            
            /// <summary>
            /// Similarity threshold (0-1) for filtering search results
            /// Higher values require more similarity between the query and results
            /// </summary>
            public const float SimilarityThreshold = 0.65f;
            
            /// <summary>
            /// Multiplier for initial retrieval when using re-ranking
            /// (e.g., 2.0 means retrieve twice as many documents initially, then re-rank and take top N)
            /// </summary>
            public const double ReRankingMultiplier = 2.0;
        }
    }
}