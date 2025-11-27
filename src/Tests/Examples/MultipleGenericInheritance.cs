public interface IServiceA { }
public interface IServiceB { }
public interface IServiceC { }
public interface IServiceD { }
public interface IServiceE { }
public interface IServiceF { }

[AutoCtor.AutoConstruct]
public partial class Generic<TA, TB>
{
    protected readonly TA a;
    protected readonly TB b;
}

[AutoCtor.AutoConstruct]
public partial class Example1 : Generic<IServiceA, IServiceB>
{
    private readonly IServiceC c;
}

[AutoCtor.AutoConstruct]
public partial class Example2 : Generic<IServiceD, IServiceE>
{
    private readonly IServiceF f;
}
