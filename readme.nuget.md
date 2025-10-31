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
