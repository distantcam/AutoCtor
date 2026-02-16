using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AttributeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c =>
            c.AddSource(Emitter.HintName, Emitter.GenerateSource()));
    }
}
