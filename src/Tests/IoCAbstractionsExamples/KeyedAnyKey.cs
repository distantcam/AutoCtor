using AutoCtor;

// KeyedService.AnyKey is a key a caller passes to a resolve, never one a service is
// registered with. It asks for every keyed registration of a service type whatever the key,
// in registration order -- the unkeyed registration is not one of them. Only the collection
// answers it, so GetKeyedService(typeof(IHandler), AnyKey) finds nothing, as it does in
// Microsoft's container.
[ServiceProvider]
[Singleton<IHandler, DefaultHandler>]
[Singleton<IHandler, EmailHandler>(Key = "email")]
[Singleton<IHandler, SmsHandler>(Key = "sms")]
[Singleton<IHandler, PushHandler>(Key = "email")]
public sealed partial class HandlerProvider;

public interface IHandler;
public class DefaultHandler : IHandler;
public class EmailHandler : IHandler;
public class SmsHandler : IHandler;
public class PushHandler : IHandler;
