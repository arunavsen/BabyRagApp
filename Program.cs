#region Simpler Version of the RAG Code using Ollama and Mistral 
//using BabyRagApp;

//Console.WriteLine("Ask your question:");
//string prompt = Console.ReadLine();

//string response = await OllamaService.AskOllamaAsync("mistral", prompt);
//Console.WriteLine("\nOllama says:\n" + response);
#endregion

#region Simpler Version of the RAG Code using Ollama and Mistral and Semantic Kernal (Streaming)

//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;

//// Create a builder for the Semantic Kernel
//var builder = Kernel.CreateBuilder();

//// Register the OllamaChatCompletion service with the specified URL and model name
//builder.Services.AddSingleton<IChatCompletionService>(new OllamaChatCompletion("http://localhost:11434", "mistral"));

//// Build the kernel from the configured services
//var kernel = builder.Build();

//// Retrieve the chat completion service from the kernel
//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//// Create a new chat history instance
//var chat = new ChatHistory();

//// Add a system message to the chat history to define the assistant's role
//chat.AddSystemMessage("You are an assistant AI helping the user understand AI concepts.");

//// Set the console text color to dark magenta for the prompt
//Console.ForegroundColor = ConsoleColor.DarkMagenta;
//// Display a message to the user indicating they can ask questions
//Console.WriteLine("Ask anything about AI (type 'exit' to quit):");
//// Reset the console text color to default
//Console.ResetColor();

//// Start an infinite loop to continuously accept user input
//while (true)
//{
//    // Set the console text color to dark yellow for user input prompt
//    Console.ForegroundColor = ConsoleColor.DarkYellow;
//    // Prompt the user for input
//    Console.Write("User: ");
//    // Read the user's input from the console
//    var userInput = Console.ReadLine();
//    // Check if the user wants to exit the loop
//    if (string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase))
//        break; // Exit the loop if the user types 'exit'

//    // Add the user's message to the chat history
//    chat.AddUserMessage(userInput);

//    // Get the chat message contents asynchronously from the chat completion service
//    var result = await chatCompletionService.GetChatMessageContentsAsync(chat);
//    // Retrieve the last response from the result
//    var response = result.Last().Content;

//    // Set the console text color to dark gray for the assistant's response
//    Console.ForegroundColor = ConsoleColor.DarkGray;
//    // Display the assistant's response to the user
//    Console.WriteLine($"Assistant: {response}");
//}



#endregion

#region Simpler Version of the RAG Code using Ollama and Mistral and Semantic Kernal (Non-Streaming)

//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using System.Net.Http.Headers;

//var builder = Kernel.CreateBuilder();

//// Configure your custom Ollama chat completion service
//// Create an instance of HttpClient with a base address for the API
//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("http://localhost:11434") // Set the base address for the HTTP requests
//};
//// Add a header to accept JSON responses
//httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//// Register the Ollama chat completion service with the specified URL and model name
//builder.Services.AddSingleton<IChatCompletionService>(new OllamaChatCompletion("http://localhost:11434", "mistral"));

//// Build the kernel from the configured services
//var kernel = builder.Build();
//// Retrieve the chat completion service from the kernel
//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//// Create a new chat history instance
//var chat = new ChatHistory();
//// Add a system message to the chat history to define the assistant's role
//chat.AddSystemMessage("You are an assistant AI helping the user understand AI concepts.");

//// Start an infinite loop to continuously accept user input
//while (true)
//{
//    // Prompt the user for input
//    Console.Write("You: ");
//    // Read the user's input from the console
//    var input = Console.ReadLine();

//    // Check if the input is empty or whitespace, and break the loop if it is
//    if (string.IsNullOrWhiteSpace(input)) break;

//    // Add the user's message to the chat history
//    chat.AddUserMessage(input);

//    // Call the streaming method to get the assistant's response
//    await foreach (var message in chatCompletionService.GetStreamingChatMessageContentsAsync(chat))
//    {
//        // Output the content of the message to the console
//        Console.Write(message.Content);
//    }

//    // Print a new line for better readability
//    Console.WriteLine();
//}


#endregion

#region Simpler Version of the RAG Code using Ollama and Mistral and Semantic Kernal with "Knowledge.txt" File (Non-Streaming)

//using Microsoft.SemanticKernel.ChatCompletion;
//using Microsoft.SemanticKernel;
//using Microsoft.Extensions.DependencyInjection;

//// Create a builder for the Semantic Kernel
//var builder = Kernel.CreateBuilder();

//// Register the OllamaChatCompletion service with the default constructor
//builder.Services.AddSingleton<IChatCompletionService, OllamaChatCompletion>();

//// Build the kernel from the configured services
//var kernel = builder.Build();

//// Retrieve the chat completion service from the kernel
//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//// Step 1: Load knowledge.txt
//string knowledgeText = File.ReadAllText(@"D:\Own Projects\BabyRagApp\knowledge.txt");

//// Start an infinite loop to continuously accept user input
//while (true)
//{
//    // Prompt the user for input
//    Console.Write("User > ");

//    // Read the user's input from the console
//    var userInput = Console.ReadLine();

//    // Step 2: Create chat prompt with knowledge
//    var chat = OllamaChatCompletion.CreateNewChat("You are an assistant AI. Use the knowledge provided to answer the user's questions.");

//    // Add the user's message along with the knowledge text to the chat
//    chat.AddUserMessage($"{knowledgeText}\n\nUser: {userInput}");

//    // Step 3: Get response from the chat completion service
//    var reply = await chatCompletionService.GetChatMessageContentsAsync(chat);

//    // Display the AI's response to the user
//    Console.WriteLine($"AI > {reply.Last().Content}");
//}



#endregion
#region Simpler Version of the RAG Code using Ollama, Mistral, nomic-embed-text and  Semantic Kernal with "Knowledge.txt" File (Streaming) embedding approach

using BabyRagApp.RagComponents;

var ragChatRunner = new RagChatRunner();
await ragChatRunner.RunAsync();

#endregion

