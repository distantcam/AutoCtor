using AutoCtor;

[ServiceProvider]
[Singleton(typeof(ITypeofService), typeof(TypeofService))]
public partial class TypeofAttributeProvider;

public interface ITypeofService;
public class TypeofService : ITypeofService;
