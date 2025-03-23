﻿using System.Reflection;

internal partial class CodeBuilder
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

    public CodeBuilder AddCompilerGeneratedAttribute()
        => AppendLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]");

    public CodeBuilder AddDebuggerNonUserCodeAttribute()
        => AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute]");

    public CodeBuilder AddGeneratedCodeAttribute()
        => AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{s_assemblyName}\", \"{s_version}\")]");

    public CodeBuilder AppendHeader() =>
        AppendLine($"//------------------------------------------------------------------------------")
        .AppendLine($"// <auto-generated>")
        .AppendLine(!string.IsNullOrEmpty(s_packageProjectUrl), $"//     This code was generated by {s_packageProjectUrl!}")
        .AppendLine($"//     Version: {s_version}")
        .AppendLine(!string.IsNullOrEmpty(s_gitSha), $"//     SHA: {s_gitSha!}")
        .AppendLine($"// </auto-generated>")
        .AppendLine($"//------------------------------------------------------------------------------");
}
