using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BabyRagApp.Testing
{
    public partial class RetrieverEvaluator
    {
        public class TestCase
        {
            [JsonPropertyName("question")]
            public string Question { get; set; } = string.Empty;
            
            [JsonPropertyName("expectedAnswer")]
            public string ExpectedAnswer { get; set; } = string.Empty;
            
            [JsonPropertyName("requiredKeywords")]
            public List<string> RequiredKeywords { get; set; } = new();
            
            public List<string> RetrievedDocs { get; set; } = new();
        }
    }
}
