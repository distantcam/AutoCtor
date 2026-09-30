using AutoCtor;

[ServiceProvider]
[Singleton(typeof(ITypeofService), typeof(TypeofService))]
public sealed partial class TypeofAttributeProvider;

public interface ITypeofService;
public class TypeofService : ITypeofService;
