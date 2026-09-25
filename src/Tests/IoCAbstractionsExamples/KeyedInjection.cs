using AutoCtor;

// A keyed parameter resolves against registrations carrying the same key. Both AutoCtor's
// own attribute and Microsoft's FromKeyedServices are honoured.
[ServiceProvider]
[Singleton<IQueue, PrimaryQueue>(Key = "primary")]
[Singleton<IQueue, BackupQueue>(Key = "backup")]
[Singleton<IWorker, Worker>]
public partial class QueueProvider;

public interface IQueue;
public class PrimaryQueue : IQueue;
public class BackupQueue : IQueue;

public interface IWorker;

public class Worker : IWorker
{
    public Worker(
        [AutoKeyedService("primary")] IQueue primary,
        [AutoKeyedService("backup")] IQueue backup)
    { }
}
