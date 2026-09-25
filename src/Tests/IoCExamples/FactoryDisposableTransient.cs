using AutoCtor;

// Disposal is decided on the implementation type, which with a factory is the caller's word
// for what comes back. The provider still owns what it hands out.
[ServiceProvider]
[Transient<IWorkItem, WorkItem>(Factory = nameof(CreateWorkItem))]
public partial class WorkItemProvider
{
    private WorkItem CreateWorkItem() => new WorkItem();
}

public interface IWorkItem;
public sealed class WorkItem : IWorkItem, System.IDisposable
{
    public void Dispose() { }
}
