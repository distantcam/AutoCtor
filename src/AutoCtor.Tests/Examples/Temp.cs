using AutoCtor;

public class GenericBase<T>
{
}

[AutoConstruct]
public partial class Generic : GenericBase<IExampleA>
{
    protected readonly IExampleA _exampleA;
}
