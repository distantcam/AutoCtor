using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AttributeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(static c =>
            c.AddSource(Emitter.HintName, Emitter.GenerateSource()));
    }
    public void Execute(GeneratorExecutionContext context) { }
}
