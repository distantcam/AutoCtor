# AutoCtor

AutoCtor is a Roslyn Source Generator that will automatically create a constructor for your class for use with constructor Dependency Injection.

# How to Use

- Make the class `partial`
- Add `[AutoConstruct]`
- Remove the constructor
 
```diff
+[AutoConstruct]
public partial class AService
{
    private readonly IDataContext _dataContext;
    private readonly IDataService _dataService;
    private readonly IExternalService _externalService;
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _cacheProvider;
    private readonly IUserService _userService;

-    public AService(
-        IDataContext dataContext,
-        IDataService dataService,
-        IExternalService externalService,
-        ICacheService cacheService,
-        ICacheProvider cacheProvider,
-        IUserService userService
-    )
-    {
-        _dataContext = dataContext;
-        _dataService = dataService;
-        _externalService = externalService;
-        _cacheService = cacheService;
-        _cacheProvider = cacheProvider;
-        _userService = userService;
-    }
}
```

# Service Providers

AutoCtor can also build the container. Register services with attributes on a partial class, and the whole object graph is resolved during compilation and emitted as plain C#.

```c#
[ServiceProvider]
[Singleton<IClock, SystemClock>]
[Scoped<IUnitOfWork, UnitOfWork>]
[Transient<IReportBuilder, ReportBuilder>]
public partial class Container;
```

You get a real `IServiceProvider` with scopes, and disposal in reverse construction order. There is no reflection, no container and no registration API at run time -- every construction site is a literal `new`, so there is nothing for trimming or AOT to preserve.

Because the graph is decided when the compiler runs, the problems Microsoft's container finds when someone resolves a service are build errors instead: a dependency nothing is registered for, a circular dependency, a scoped service captured by a singleton.

Singletons, scoped services, transients, keyed services, open generics, `IEnumerable<T>` injection, factory members and `IAsyncDisposable` are all supported. AutoCtor adds no package dependency of its own: where `Microsoft.Extensions.DependencyInjection.Abstractions` is referenced the generated types implement the interfaces from it that fit, and where it is not, those interfaces are left off and every member behind them stands on its own. A provider can also fall back to another `IServiceProvider` for anything it does not register, which is how it sits under a host.

See the [full readme](https://github.com/distantcam/AutoCtor#service-provider) for details.
