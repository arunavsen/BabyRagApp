using System.Text;
using System.Text.Json;

namespace BabyRagApp
{
    public class OllamaService
    {

        public static async Task<string> AskOllamaAsync(string model, string prompt)
        {
            using var client = new HttpClient();
            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:11434/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("response").GetString();
        }
    }
}
