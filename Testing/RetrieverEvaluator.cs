using BabyRagApp.RagComponents;
using BabyRagApp.RagComponents.QdrantMemo;
using System.Text.Json;

namespace BabyRagApp.Testing
{
    public partial class RetrieverEvaluator
    {
        private readonly RagChatRunnerWithQdrantMemoryStore _rag;
        private readonly RetrievalMetrics _metrics;

        public RetrieverEvaluator(RagChatRunnerWithQdrantMemoryStore rag)
        {
            _rag = rag;
            _metrics = new RetrievalMetrics();
        }

        public async Task RunTestsAsync(string testDataPath)
        {
            var json = await File.ReadAllTextAsync(testDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var testCases = JsonSerializer.Deserialize<List<TestCase>>(json, options);

            if (testCases == null || testCases.Count == 0)
            {
                Console.WriteLine("No test cases found.");
                return;
            }

            // Reset metrics before running tests
            _metrics.ResetMetrics();

            foreach (var testCase in testCases)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n🔍 Question: {testCase.Question}");

                // Get the rewritten query using the RAG's query rewriter
                string rewrittenQuery = await _rag.GetRewrittenQueryAsync(testCase.Question);
                
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"🔄 Rewritten query: {rewrittenQuery}");

                // Use the rewritten query to retrieve documents
                var docs = await _rag.GetTopKRelevantDocsAsync(rewrittenQuery, RagSettings.VectorSearch.TopN);

                // Store the retrieved docs in the test case
                testCase.RetrievedDocs = docs;

                // Update metrics and get relevant docs count for this query
                int relevantDocsForQuestion = _metrics.UpdateWithQuery(docs, testCase.RequiredKeywords);

                var anyRelevantFound = relevantDocsForQuestion > 0;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Expected: {testCase.ExpectedAnswer}");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"📄 Retrieved docs: {string.Join(" | ", docs)}");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"🎯 Any relevant docs: {(anyRelevantFound ? "Yes" : "No")}");
                Console.WriteLine($"🎯 Relevant documents found: {relevantDocsForQuestion}/{docs.Count}");

                Console.ResetColor();
            }

            // Display metrics
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("\n" + _metrics.GetFormattedMetrics());
            Console.ResetColor();
        }
    }
}
