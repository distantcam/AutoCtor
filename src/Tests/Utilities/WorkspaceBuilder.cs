using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Tests.Utilities;

internal sealed class WorkspaceBuilder
{
    private ImmutableArray<MetadataReference> _references;
    private ImmutableArray<(DocumentId DocumentId, string Name, string Text)> _documents;

    private string _name;
    private ProjectId _projectId;

    public WorkspaceBuilder()
    {
        _references = [];
        _documents = [];
        _name = string.Empty;
        _projectId = ProjectId.CreateNewId();
    }

    public WorkspaceBuilder(WorkspaceBuilder other)
    {
        _references = other._references;
        _documents = other._documents;
        _name = other._name;
        _projectId = other._projectId;
    }

    public Workspace Build()
    {
        var workspace = new AdhocWorkspace();

        var solution = workspace.CurrentSolution
            .AddProject(_projectId, _name, _name, LanguageNames.CSharp);
        solution = solution.AddMetadataReferences(_projectId, _references);
        foreach (var doc in _documents)
            solution = solution.AddDocument(doc.DocumentId, doc.Name, doc.Text);
        workspace.TryApplyChanges(solution);

        return workspace;
    }

    public WorkspaceBuilder WithName(string name)
    {
        return new(this)
        {
            _name = name
        };
    }

    public WorkspaceBuilder WithReferences(IEnumerable<MetadataReference> references)
    {
        return new(this)
        {
            _references = references.ToImmutableArray()
        };
    }

    public DocumentId AddDocument(string name, string text)
    {
        var documentId = DocumentId.CreateNewId(_projectId);
        _documents = _documents.Add(new(documentId, name, text));
        return documentId;
    }
}
