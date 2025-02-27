public interface IServiceA { }
public interface IServiceB { }

[AutoCtor.AutoConstruct(guard: AutoCtor.GuardSetting.Enabled)]
public partial class NullableAnnotationTests
{
    private readonly IServiceA? _nullableService;
    private readonly IServiceB _guardedService;
}
