﻿using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Text;

internal interface IPartialTypeModel
{
    string? Namespace { get; }
    IReadOnlyList<string> TypeDeclarations { get; }
}

internal class CodeBuilder
{
    private static readonly string s_assemblyName;
    private static readonly string s_version;
    private static readonly string? s_packageProjectUrl;
    private static readonly string? s_gitSha;

    static CodeBuilder()
    {
        var assembly = Assembly.GetAssembly(typeof(CodeBuilder));
        s_assemblyName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Untitled";
        s_version = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0.0.0";
        var metadata = assembly?.GetCustomAttributes<AssemblyMetadataAttribute>()?.ToDictionary(m => m.Key, m => m.Value);
        if (metadata != null)
        {
            metadata.TryGetValue("PackageProjectUrl", out s_packageProjectUrl);
            metadata.TryGetValue("GitSha", out s_gitSha);
        }
    }

    private readonly StringBuilder _stringBuilder = new();
    private int _indent = 0;

    public CodeBuilder Append(string text)
    {
        _stringBuilder.Append(text);
        return this;
    }

    public CodeBuilder Append(bool enabled, string text)
    {
        if (enabled)
            _stringBuilder.Append(text);
        return this;
    }

    public CodeBuilder AppendIndent()
    {
        _stringBuilder.Append(Indent);
        return this;
    }

    public CodeBuilder AppendLine()
    {
        _stringBuilder.AppendLine();
        return this;
    }

    public CodeBuilder AppendLine(string line)
    {
        _stringBuilder.AppendLine(Indent + line);
        return this;
    }

    public void IncreaseIndent() { _indent++; }
    public void DecreaseIndent()
    {
        if (_indent > 0)
        {
            _indent--;
        }
    }

    public CodeBuilder OpenBlock()
    {
        AppendLine("{");
        IncreaseIndent();
        return this;
    }
    public CodeBuilder CloseBlock()
    {
        DecreaseIndent();
        AppendLine("}");
        return this;
    }

    public char IndentChar { get; set; } = '\t';
    public string Indent => new(IndentChar, _indent);

    public CodeBuilder AddCompilerGeneratedAttribute() => AppendLine(
        "[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]");

    public CodeBuilder AddDebuggerNonUserCodeAttribute() => AppendLine(
        "[global::System.Diagnostics.DebuggerNonUserCodeAttribute]");

    public CodeBuilder AddGeneratedCodeAttribute() => AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{s_assemblyName}\", \"{s_version}\")]");

    public CodeBuilder AppendHeader() =>
    AppendLine($"//------------------------------------------------------------------------------")
    .AppendLine($"// <auto-generated>")
    .AppendLine($"//     This code was generated by {s_packageProjectUrl}")
    .AppendLine($"//     Version: {s_version}")
    .AppendLine($"//     SHA: {s_gitSha}")
    .AppendLine($"// </auto-generated>")
    .AppendLine($"//------------------------------------------------------------------------------");

    public IDisposable StartPartialType(IPartialTypeModel typeModel)
    {
        if (!string.IsNullOrEmpty(typeModel.Namespace))
        {
            AppendLine($"namespace {typeModel.Namespace}");
            OpenBlock();
        }

        for (var i = 0; i < typeModel.TypeDeclarations.Count; i++)
        {
            AppendLine(typeModel.TypeDeclarations[i]);
            OpenBlock();
        }

        return new CloseBlockDisposable(this, typeModel.TypeDeclarations.Count + (typeModel.Namespace != null ? 1 : 0));
    }

    public IDisposable StartBlock(string? line = null)
    {
        if (!string.IsNullOrEmpty(line))
            AppendLine(line!);
        OpenBlock();
        return new CloseBlockDisposable(this, 1);
    }

    public static implicit operator SourceText(CodeBuilder codeBuilder)
        => SourceText.From(codeBuilder._stringBuilder.ToString(), Encoding.UTF8);

    private readonly struct CloseBlockDisposable(CodeBuilder codeBuilder, int count) : IDisposable
    {
        public void Dispose() { for (var i = 0; i < count; i++) codeBuilder.CloseBlock(); }
    }
}
