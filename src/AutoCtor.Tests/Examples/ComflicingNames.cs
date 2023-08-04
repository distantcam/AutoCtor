using AutoCtor;

[AutoConstruct]
public partial class AClass
{
    private readonly IExampleA _example;
}

[AutoConstruct]
public partial class BClass : AClass
{
    private readonly IExampleB _example;
}

[AutoConstruct]
public partial class CClass : BClass
{
    private readonly IExampleC _example;
}
