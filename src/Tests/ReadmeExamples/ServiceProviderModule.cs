using AutoCtor;

public interface IPaymentGateway;
public interface ILedger;
public class StripeGateway : IPaymentGateway;
public class FakeGateway : IPaymentGateway;
public class Ledger(IPaymentGateway gateway) : ILedger
{
    public IPaymentGateway Gateway => gateway;
}

#region ServiceProviderModule

[Singleton<IPaymentGateway, StripeGateway>]
[Scoped<ILedger>(Factory = nameof(CreateLedger))]
public class PaymentsModule
{
    public static ILedger CreateLedger(IPaymentGateway gateway) => new Ledger(gateway);
}

[ServiceProvider]
[Import<PaymentsModule>]
[Singleton<IPaymentGateway, FakeGateway>]
public partial class TestContainer;

#endregion
