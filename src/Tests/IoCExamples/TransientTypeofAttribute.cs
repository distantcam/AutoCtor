using AutoCtor;

[ServiceProvider]
[Transient(typeof(ITransientTypeofService), typeof(TransientTypeofService))]
public partial class TransientTypeofAttributeProvider;

public interface ITransientTypeofService;
public class TransientTypeofService : ITransientTypeofService;
