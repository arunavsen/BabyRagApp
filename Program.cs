using BabyRagApp;

Console.WriteLine("Ask your question:");
string prompt = Console.ReadLine();

string response = await OllamaService.AskOllamaAsync("mistral", prompt);
Console.WriteLine("\nOllama says:\n" + response);
