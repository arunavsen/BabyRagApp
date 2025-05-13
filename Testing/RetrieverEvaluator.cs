using BabyRagApp.RagComponents.QdrantMemo;
using System.Text.Json;

namespace BabyRagApp.Testing
{
    public partial class RetrieverEvaluator
    {
        private readonly RagChatRunnerWithQdrantMemoryStore _rag;

        public RetrieverEvaluator(RagChatRunnerWithQdrantMemoryStore rag)
        {
            _rag = rag;
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

            int totalTests = testCases.Count;
            int totalRelevantRetrieved = 0;

            foreach (var testCase in testCases)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n🔍 Question: {testCase.Question}");

                var docs = await _rag.GetTopKRelevantDocsAsync(testCase.Question, 5);

                var matched = docs.Any(doc =>
                    testCase.RequiredKeywords.Any(keyword =>
                        doc.ToLower().Contains(keyword.ToLower())));

                Console.ForegroundColor = ConsoleColor.Green; // Set color for expected answer
                Console.WriteLine($"✅ Expected: {testCase.ExpectedAnswer}");

                Console.ForegroundColor = ConsoleColor.Cyan; // Set color for retrieved docs
                Console.WriteLine($"📄 Retrieved docs: {string.Join(" | ", docs)}");

                Console.ForegroundColor = ConsoleColor.Yellow; // Set color for match found
                Console.WriteLine($"🎯 Match found: {(matched ? "Yes" : "No")}");

                Console.ResetColor(); // Reset to default color

                if (matched)
                    totalRelevantRetrieved++;
            }

            double precision = (double)totalRelevantRetrieved / totalTests;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"\n📊 Precision@5: {precision:P2} ({totalRelevantRetrieved}/{totalTests})");
        }
    }
}
