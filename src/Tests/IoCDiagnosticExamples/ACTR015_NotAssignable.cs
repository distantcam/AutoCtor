using AutoCtor;

[ServiceProvider]
[Singleton(typeof(IUnrelatedService), typeof(UnrelatedImplementation))]
public partial class NotAssignableProvider;

public interface IUnrelatedService;
public class UnrelatedImplementation;
