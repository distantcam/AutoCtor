#region
using AutoCtor;

public interface IDataContext;
public interface IDataService;
public interface IExternalService;
public interface ICacheService;
public interface ICacheProvider;
public interface IUserService;
#endregion

public class Original
{
    private readonly IDataContext _dataContext;
    private readonly IDataService _dataService;
    private readonly IExternalService _externalService;
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _cacheProvider;
    private readonly IUserService _userService;

    public Original(
        IDataContext dataContext,
        IDataService dataService,
        IExternalService externalService,
        ICacheService cacheService,
        ICacheProvider cacheProvider,
        IUserService userService
    )
    {
        _dataContext = dataContext;
        _dataService = dataService;
        _externalService = externalService;
        _cacheService = cacheService;
        _cacheProvider = cacheProvider;
        _userService = userService;
    }
}

//                      ____
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                     |    |
//                   __|    |__
//                   \        /
//                    \      /
//                     \    /
//                      \  /
//                       \/

[AutoConstruct]
public partial class Smaller
{
    private readonly IDataContext _dataContext;
    private readonly IDataService _dataService;
    private readonly IExternalService _externalService;
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _cacheProvider;
    private readonly IUserService _userService;
}
