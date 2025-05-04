using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;

public class OllamaChatCompletion : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaChatCompletion(string baseUrl = "http://localhost:11434", string model = "mistral")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _model = model;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
    {
        { "model", _model },
        { "provider", "ollama" }
    };

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Join the contents of the chat history into a single string, separated by new lines.
        var prompt = string.Join("\n", chatHistory.Select(m => m.Content));

        // Serialize the model and prompt into a JSON string for the API request.
        var json = JsonSerializer.Serialize(new
        {
            model = _model, // The model to be used for chat completion.
            prompt = prompt, // The prompt generated from the chat history.
            stream = false // Indicates whether to stream the response or not.
        });

        // Create a StringContent object to send as the body of the HTTP POST request.
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Send a POST request to the API endpoint and await the response.
        var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);

        // Ensure the response indicates success; throw an exception if not.
        response.EnsureSuccessStatusCode();

        // Read the response content as a string asynchronously.
        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        // Deserialize the JSON response to access the data.
        var data = JsonSerializer.Deserialize<JsonElement>(result);

        // Extract the "response" property from the JSON data.
        var output = data.GetProperty("response").GetString();

        // Return a list containing a single ChatMessageContent object with the assistant's output.
        return new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, output) // Create a new ChatMessageContent with the assistant's role and output.
        };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
    ChatHistory chatHistory,
    PromptExecutionSettings? executionSettings = null,
    Kernel? kernel = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Join the contents of the chat history into a single string, separated by new lines.
        var prompt = string.Join("\n", chatHistory.Select(m => m.Content));

        // Serialize the model and prompt into a JSON string for the API request.
        var json = JsonSerializer.Serialize(new
        {
            model = _model, // The model to be used for chat completion.
            prompt = prompt, // The prompt generated from the chat history.
            stream = true // Indicates whether to stream the response or not.
        });

        // Create a StringContent object to send as the body of the HTTP POST request.
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create a new HttpRequestMessage for the POST request to the API endpoint.
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate") { Content = content };

        // Send the request and await the response, allowing for reading headers first.
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Ensure the response indicates success; throw an exception if not.
        response.EnsureSuccessStatusCode();

        // Read the response content as a stream asynchronously.
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Create a StreamReader to read the response stream line by line.
        using var reader = new StreamReader(stream);

        // Loop until the end of the stream is reached.
        while (!reader.EndOfStream)
        {
            // Read a line from the stream asynchronously.
            var line = await reader.ReadLineAsync();

            // Skip any empty or whitespace lines.
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Deserialize the JSON line to access the data.
            var data = JsonSerializer.Deserialize<JsonElement>(line);

            // Extract the "response" property from the JSON data.
            var output = data.GetProperty("response").GetString();

            // Yield a new StreamingChatMessageContent with the assistant's output.
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, output);
        }
    }


    public static ChatHistory CreateNewChat(string? instructions = null)
    {
        var chat = new ChatHistory();
        if (!string.IsNullOrEmpty(instructions))
        {
            chat.AddSystemMessage(instructions);
        }
        return chat;
    }
}
