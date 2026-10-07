using AutoCtor;

[ServiceProvider]
[Singleton(typeof(IUnrelatedService), typeof(UnrelatedImplementation))]
public sealed partial class NotAssignableProvider;

public interface IUnrelatedService;
public class UnrelatedImplementation;
