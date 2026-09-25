using AutoCtor;

// As chooses what each match is registered under: here each notifier as itself and as every
// interface it has. The registration after the scan wins IAuditable, as it would after one
// written out by hand, and the collection still has both.
[ServiceProvider]
[ScanSingleton(typeof(INotifier), As = ScanAs.Self | ScanAs.ImplementedInterfaces)]
[Singleton<IAuditable, AuditLog>]
public partial class NotifierProvider;

public interface INotifier;
public interface IAuditable;

public class EmailNotifier : INotifier, IAuditable;
public class SmsNotifier : INotifier;
public class AuditLog : IAuditable;
