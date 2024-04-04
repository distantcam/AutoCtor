using System.Collections.Immutable;
using FluentAssertions;
using Xunit.Abstractions;

namespace AutoCtor.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile(theoryData.Codes);
        var generator = new AutoConstructSourceGenerator();
        var driver = Helpers.CreateDriver(theoryData.Options, generator)
            .RunGenerators(compilation);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var compilation = await Helpers.Compile(theoryData.Codes);
        var generator = new AutoConstructSourceGenerator();
        Helpers.CreateDriver(theoryData.Options, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        //diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

    private static DirectoryInfo BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    public static TheoryData<CodeFileTheoryData> GetExamples()
    {
        if (BaseDir == null)
            throw new Exception("BaseDir is null");

        var data = new TheoryData<CodeFileTheoryData>();

        var examples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "Examples"), "*.cs");
        var exampleCode = File.ReadAllText(Path.Combine(BaseDir.FullName, "Examples", "IExampleInterfaces.cs"));
        foreach (var example in examples)
        {
            if (example.Contains(".g.") || example.Contains("IExampleInterfaces"))
                continue;

            data.Add(
                new CodeFileTheoryData
                {
                    Name = Path.GetFileNameWithoutExtension(example),
                    Codes = [exampleCode, File.ReadAllText(example)],
                    Options = [],
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(example), "Verified")
                }
            );
        }

        var guardExamples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "GuardExamples"), "*.cs");
        foreach (var guardExample in guardExamples)
        {
            if (guardExample.Contains(".g."))
                continue;

            data.Add(
                new CodeFileTheoryData
                {
                    Name = Path.GetFileNameWithoutExtension(guardExample),
                    Codes = [File.ReadAllText(guardExample)],
                    Options = [new("build_property.AutoCtorGuards", "true")],
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(guardExample), "Verified"),
                }
            );
        }

        var langExamples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "LangExamples"), "*.cs");
        foreach (var langExample in langExamples)
        {
            if (langExample.Contains(".g."))
                continue;

            data.Add(
                new CodeFileTheoryData
                {
                    Name = Path.GetFileNameWithoutExtension(langExample),
                    Codes = [exampleCode, File.ReadAllText(langExample)],
                    Options = [],
#if ROSLYN_3_11
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample), "Verified_3_11")
#elif ROSLYN_4_0
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample), "Verified_4_0")
#elif ROSLYN_4_4
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample), "Verified_4_4")
#endif
                }
            );
        }

        return data;
    }

    public class CodeFileTheoryData : IXunitSerializable
    {
        public string Name { get; set; }
        public string[] Codes { get; set; }
        public (string, string)[] Options { get; set; }
        public string VerifiedDirectory { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Codes = info.GetValue<string[]>(nameof(Codes));
            Options = info.GetValue<string[]>(nameof(Options))
                .Select(o => o.Split('|'))
                .Select(o => (o[0], o[1]))
                .ToArray();
            VerifiedDirectory = info.GetValue<string>(nameof(VerifiedDirectory));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Codes), Codes);
            info.AddValue(nameof(Options), Options.Select(o => $"{o.Item1}|{o.Item2}").ToArray());
            info.AddValue(nameof(VerifiedDirectory), VerifiedDirectory);
        }

        public override string ToString() => Name + ".cs";
    }
}
