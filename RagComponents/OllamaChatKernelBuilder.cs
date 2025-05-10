using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace BabyRagApp.RagComponents;

public static class OllamaChatKernelBuilder
{
    public static Kernel BuildKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<IChatCompletionService>(new OllamaChatCompletion());
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(
    new CustomEmbeddingService("http://localhost:11434", "nomic-embed-text")
);

        return builder.Build();
    }
}
