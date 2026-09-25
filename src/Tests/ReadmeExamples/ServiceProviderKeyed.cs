using AutoCtor;

public interface IClient;

public class LiveClient : IClient;
public class SandboxClient : IClient;

#region ServiceProviderKeyed

[ServiceProvider]
[Singleton<IClient, LiveClient>(Key = "live")]
[Singleton<IClient, SandboxClient>(Key = "sandbox")]
[Singleton<IPaymentGateway, PaymentGateway>]
public partial class PaymentContainer;

public interface IPaymentGateway;

public class PaymentGateway : IPaymentGateway
{
    public PaymentGateway([AutoKeyedService("live")] IClient client) { }
}

#endregion
