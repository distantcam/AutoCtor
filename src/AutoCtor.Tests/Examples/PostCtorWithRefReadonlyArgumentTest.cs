﻿public interface IServiceA { }
public interface IServiceB { }
public interface IServiceFactory { IServiceB CreateService(); }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithRefReadonlyArgumentTest
{
    private readonly IServiceA _serviceA;
    private readonly IServiceB _serviceB;

    [AutoCtor.AutoPostConstruct]
    private void Initialise(IServiceFactory serviceFactory, ref readonly IServiceB serviceB)
    {
    }
}
