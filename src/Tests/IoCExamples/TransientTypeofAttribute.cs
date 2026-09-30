using AutoCtor;

[ServiceProvider]
[Transient(typeof(ITransientTypeofService), typeof(TransientTypeofService))]
public sealed partial class TransientTypeofAttributeProvider;

public interface ITransientTypeofService;
public class TransientTypeofService : ITransientTypeofService;
