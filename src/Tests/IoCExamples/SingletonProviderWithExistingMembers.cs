using System;
using AutoCtor;

[ServiceProvider]
[Singleton<IExistingService, ExistingService>]
public sealed partial class ExistingMembersProvider : IServiceProvider
{
    public string Name => "provider";
}

public interface IExistingService;
public class ExistingService : IExistingService;
