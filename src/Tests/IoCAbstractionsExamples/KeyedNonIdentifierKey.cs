using AutoCtor;

// A key that cannot be spelled as an identifier gets no strongly typed getter; it stays
// reachable through GetKeyedService.
[ServiceProvider]
[Singleton<IEndpoint, PublicEndpoint>(Key = "api/v1")]
[Singleton<IEndpoint, AdminEndpoint>(Key = 7)]
public partial class EndpointProvider;

public interface IEndpoint;
public class PublicEndpoint : IEndpoint;
public class AdminEndpoint : IEndpoint;
