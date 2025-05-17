using System;
using System.Collections.Generic;
using System.Linq;

namespace BabyRagApp.Testing
{
    /// <summary>
    /// Utility class for calculating information retrieval metrics
    /// </summary>
    public class RetrievalMetrics
    {
        /// <summary>
        /// Total number of questions or queries evaluated
        /// </summary>
        public int TotalQueries { get; private set; }

        /// <summary>
        /// Total number of queries for which at least one relevant document was retrieved
        /// </summary>
        public int QueriesWithRelevantDocs { get; private set; }

        /// <summary>
        /// Total number of relevant documents retrieved across all queries
        /// </summary>
        public int RelevantDocsRetrieved { get; private set; }

        /// <summary>
        /// Total number of documents retrieved across all queries
        /// </summary>
        public int TotalDocsRetrieved { get; private set; }

        /// <summary>
        /// Total number of relevant documents available across all queries
        /// </summary>
        public int TotalRelevantDocs { get; private set; }

        /// <summary>
        /// Precision: ratio of retrieved documents that are relevant
        /// </summary>
        public double Precision => TotalDocsRetrieved > 0 
            ? (double)RelevantDocsRetrieved / TotalDocsRetrieved 
            : 0;

        /// <summary>
        /// Recall: ratio of relevant documents that are retrieved
        /// </summary>
        public double Recall => TotalRelevantDocs > 0 
            ? (double)RelevantDocsRetrieved / TotalRelevantDocs 
            : 0;

        /// <summary>
        /// F1 Score: harmonic mean of precision and recall
        /// </summary>
        public double F1Score => (Precision + Recall) > 0 
            ? 2 * (Precision * Recall) / (Precision + Recall) 
            : 0;

        /// <summary>
        /// Precision@k: ratio of queries for which at least one relevant document was retrieved
        /// </summary>
        public double PrecisionAtK => TotalQueries > 0 
            ? (double)QueriesWithRelevantDocs / TotalQueries 
            : 0;

        public RetrievalMetrics()
        {
            ResetMetrics();
        }

        /// <summary>
        /// Reset all metrics to zero
        /// </summary>
        public void ResetMetrics()
        {
            TotalQueries = 0;
            QueriesWithRelevantDocs = 0;
            RelevantDocsRetrieved = 0;
            TotalDocsRetrieved = 0;
            TotalRelevantDocs = 0;
        }

        /// <summary>
        /// Update metrics with a single query evaluation
        /// </summary>
        /// <param name="retrievedDocs">List of retrieved documents</param>
        /// <param name="relevantKeywords">List of keywords indicating relevance</param>
        /// <returns>Number of relevant documents found for this query</returns>
        public int UpdateWithQuery(IEnumerable<string> retrievedDocs, IEnumerable<string> relevantKeywords)
        {
            if (retrievedDocs == null || relevantKeywords == null)
                throw new ArgumentNullException(nameof(retrievedDocs) + " or " + nameof(relevantKeywords));

            var retrievedDocsList = retrievedDocs.ToList();
            var keywordsList = relevantKeywords.ToList();
            
            // Count relevant documents in the retrieved set
            int relevantDocsInQuery = 0;
            foreach (var doc in retrievedDocsList)
            {
                bool isRelevant = keywordsList.Any(keyword => 
                    doc.ToLower().Contains(keyword.ToLower()));
                
                if (isRelevant)
                    relevantDocsInQuery++;
            }

            // Update metrics
            TotalQueries++;
            TotalDocsRetrieved += retrievedDocsList.Count;
            RelevantDocsRetrieved += relevantDocsInQuery;
            TotalRelevantDocs += keywordsList.Count;
            
            if (relevantDocsInQuery > 0)
                QueriesWithRelevantDocs++;

            return relevantDocsInQuery;
        }

        /// <summary>
        /// Get a formatted string with all metrics
        /// </summary>
        public string GetFormattedMetrics()
        {
            return $"?? Evaluation Metrics:\n" +
                   $"Precision@k: {PrecisionAtK:P2} ({QueriesWithRelevantDocs}/{TotalQueries})\n" +
                   $"Precision: {Precision:P2} ({RelevantDocsRetrieved}/{TotalDocsRetrieved})\n" +
                   $"Recall: {Recall:P2} ({RelevantDocsRetrieved}/{TotalRelevantDocs})\n" +
                   $"F1 Score: {F1Score:P2}";
        }
    }
}