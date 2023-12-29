public class BaseWith2Constructors
{
    [System.Obsolete]
    protected BaseWith2Constructors(string name) { }

    protected BaseWith2Constructors(string firstName, string lastName) { }
}

[AutoCtor.AutoConstruct]
public partial class AutoClass : BaseWith2Constructors
{
}
